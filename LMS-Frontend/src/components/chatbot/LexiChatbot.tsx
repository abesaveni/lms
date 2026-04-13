import React, { useState, useRef, useEffect, useCallback } from 'react'
import { motion, AnimatePresence } from 'framer-motion'
import {
  Bot, Send, X, Minimize2, User, Loader2,
  Sparkles, BookOpen, Mic, GraduationCap, RefreshCw, Smile
} from 'lucide-react'
import { sendChatbotMessage, startMockInterview, ChatMessage } from '../../services/aiApi'

interface Message {
  id: string
  text: string
  sender: 'user' | 'lexi'
  timestamp: Date
  isStreaming?: boolean
}

// ---------------------------------------------------------------------------
// Lightweight markdown renderer
// ---------------------------------------------------------------------------
function renderInline(text: string, key: string | number): React.ReactNode {
  const parts = text.split(/(\*\*[^*]+\*\*|\*[^*]+\*)/g)
  return (
    <span key={key}>
      {parts.map((part, i) => {
        if (part.startsWith('**') && part.endsWith('**'))
          return <strong key={i}>{part.slice(2, -2)}</strong>
        if (part.startsWith('*') && part.endsWith('*'))
          return <em key={i}>{part.slice(1, -1)}</em>
        return part
      })}
    </span>
  )
}

function LexiMarkdown({ text }: { text: string }) {
  const lines = text.split('\n')
  const nodes: React.ReactNode[] = []
  let bulletBuffer: string[] = []
  let numberedBuffer: string[] = []

  const flushBullets = (key: string) => {
    if (bulletBuffer.length === 0) return
    nodes.push(
      <ul key={key} className="list-none space-y-1 my-1.5">
        {bulletBuffer.map((b, i) => (
          <li key={i} className="flex gap-2">
            <span className="text-violet-400 mt-0.5 flex-shrink-0">•</span>
            <span>{renderInline(b, i)}</span>
          </li>
        ))}
      </ul>
    )
    bulletBuffer = []
  }

  const flushNumbered = (key: string) => {
    if (numberedBuffer.length === 0) return
    nodes.push(
      <ol key={key} className="list-none space-y-1 my-1.5">
        {numberedBuffer.map((b, i) => (
          <li key={i} className="flex gap-2">
            <span className="text-violet-400 font-semibold flex-shrink-0">{i + 1}.</span>
            <span>{renderInline(b, i)}</span>
          </li>
        ))}
      </ol>
    )
    numberedBuffer = []
  }

  lines.forEach((raw, idx) => {
    const trimmed = raw.trim()
    if (!trimmed) {
      flushBullets(`bl-${idx}`)
      flushNumbered(`nl-${idx}`)
      return
    }
    const bulletMatch = trimmed.match(/^[*\-]\s+(.+)/)
    if (bulletMatch) {
      flushNumbered(`nl-${idx}`)
      bulletBuffer.push(bulletMatch[1])
      return
    }
    const numberedMatch = trimmed.match(/^\d+\.\s+(.+)/)
    if (numberedMatch) {
      flushBullets(`bl-${idx}`)
      numberedBuffer.push(numberedMatch[1])
      return
    }
    flushBullets(`bl-${idx}`)
    flushNumbered(`nl-${idx}`)
    nodes.push(
      <p key={idx} className="leading-relaxed">
        {renderInline(trimmed, idx)}
      </p>
    )
  })
  flushBullets('end-bl')
  flushNumbered('end-nl')

  return <div className="space-y-1 text-sm">{nodes}</div>
}

// ---------------------------------------------------------------------------
// Typing dots animation
// ---------------------------------------------------------------------------
function TypingDots() {
  return (
    <div className="flex items-center gap-1 px-1 py-0.5">
      {[0, 1, 2].map(i => (
        <motion.span
          key={i}
          className="w-1.5 h-1.5 bg-violet-400 rounded-full block"
          animate={{ y: [0, -4, 0], opacity: [0.5, 1, 0.5] }}
          transition={{ duration: 0.8, repeat: Infinity, delay: i * 0.15 }}
        />
      ))}
    </div>
  )
}

// ---------------------------------------------------------------------------
// Suggested quick actions
// ---------------------------------------------------------------------------
const SUGGESTED_ACTIONS = [
  { icon: <GraduationCap className="w-3.5 h-3.5" />, label: 'Find me a tutor', prompt: 'Can you help me find the right tutor for me?' },
  { icon: <BookOpen className="w-3.5 h-3.5" />, label: 'Make a study plan', prompt: 'Help me create a study plan for my upcoming exams.' },
  { icon: <Mic className="w-3.5 h-3.5" />, label: 'Mock interview', prompt: 'Start a mock interview for a software engineering role.' },
  { icon: <Sparkles className="w-3.5 h-3.5" />, label: 'Career advice', prompt: 'I need career guidance — what skills should I focus on?' },
]

// ---------------------------------------------------------------------------
// Simulate streaming by revealing text word-by-word
// ---------------------------------------------------------------------------
function useStreamText(fullText: string, isStreaming: boolean, onDone: () => void) {
  const [displayed, setDisplayed] = useState('')
  const indexRef = useRef(0)
  const timerRef = useRef<ReturnType<typeof setTimeout> | null>(null)

  useEffect(() => {
    if (!isStreaming) {
      setDisplayed(fullText)
      return
    }
    setDisplayed('')
    indexRef.current = 0

    const words = fullText.split(' ')
    const reveal = () => {
      if (indexRef.current >= words.length) {
        onDone()
        return
      }
      setDisplayed(words.slice(0, indexRef.current + 1).join(' '))
      indexRef.current++
      // Variable speed: shorter words faster, long pauses after punctuation
      const word = words[indexRef.current - 1] ?? ''
      const delay = word.endsWith('.') || word.endsWith('!') || word.endsWith('?')
        ? 60 + Math.random() * 40
        : 18 + Math.random() * 20
      timerRef.current = setTimeout(reveal, delay)
    }
    timerRef.current = setTimeout(reveal, 80)

    return () => { if (timerRef.current) clearTimeout(timerRef.current) }
  }, [fullText, isStreaming]) // eslint-disable-line react-hooks/exhaustive-deps

  return displayed
}

// ---------------------------------------------------------------------------
// Individual Lexi message with streaming
// ---------------------------------------------------------------------------
function LexiMessage({ msg, isLatest }: { msg: Message; isLatest: boolean }) {
  const [doneStreaming, setDoneStreaming] = useState(!msg.isStreaming)
  const displayed = useStreamText(msg.text, !!(msg.isStreaming && isLatest), () => setDoneStreaming(true))

  return (
    <div className="px-3 py-2.5 rounded-2xl rounded-tl-none text-sm leading-relaxed shadow-sm bg-white text-gray-800 border border-gray-100">
      <LexiMarkdown text={displayed} />
      {msg.isStreaming && isLatest && !doneStreaming && (
        <span className="inline-block w-0.5 h-3.5 bg-violet-400 ml-0.5 animate-pulse rounded-sm" />
      )}
    </div>
  )
}

// ---------------------------------------------------------------------------
// Main component
// ---------------------------------------------------------------------------
const LexiChatbot: React.FC = () => {
  const [isOpen, setIsOpen] = useState(false)
  const [messages, setMessages] = useState<Message[]>([])
  const [history, setHistory] = useState<ChatMessage[]>([])
  const [input, setInput] = useState('')
  const [isLoading, setIsLoading] = useState(false)
  const [isMockInterview, setIsMockInterview] = useState(false)
  const [mockRole, setMockRole] = useState('')
  const [hasUnread, setHasUnread] = useState(false)
  const scrollRef = useRef<HTMLDivElement>(null)
  const inputRef = useRef<HTMLInputElement>(null)

  const scrollToBottom = useCallback(() => {
    if (scrollRef.current) {
      scrollRef.current.scrollTo({ top: scrollRef.current.scrollHeight, behavior: 'smooth' })
    }
  }, [])

  useEffect(() => { scrollToBottom() }, [messages, isOpen, scrollToBottom])

  useEffect(() => {
    if (isOpen) {
      setHasUnread(false)
      setTimeout(() => inputRef.current?.focus(), 350)
    }
  }, [isOpen])

  const addMessage = useCallback((text: string, sender: 'user' | 'lexi', streaming = false): Message => {
    const msg: Message = {
      id: `${Date.now()}-${Math.random()}`,
      text,
      sender,
      timestamp: new Date(),
      isStreaming: streaming,
    }
    setMessages(prev => [...prev, msg])
    if (sender === 'lexi' && !isOpen) setHasUnread(true)
    return msg
  }, [isOpen])

  const sendMessage = async (text: string) => {
    if (!text.trim() || isLoading) return
    setInput('')
    addMessage(text, 'user')
    setIsLoading(true)

    const newHistory: ChatMessage[] = [...history, { role: 'user', content: text }]
    setHistory(newHistory)

    try {
      if (isMockInterview) {
        const res = await startMockInterview({
          role: mockRole || 'Software Engineer',
          level: 'Entry Level',
          previousAnswer: text,
        })
        addMessage(res.response, 'lexi', true)
        setHistory(prev => [...prev, { role: 'assistant', content: res.response }])
      } else {
        const res = await sendChatbotMessage(
          history.map(m => ({ role: m.role, content: m.content })),
          text
        )
        addMessage(res.reply, 'lexi', true)
        setHistory(prev => [...prev, { role: 'assistant', content: res.reply }])
      }
    } catch (err: any) {
      if (err?.code === 'SUBSCRIPTION_REQUIRED') return

      // Friendly error messages depending on what went wrong
      const status = err?.message?.match(/HTTP (\d+)/)?.[1]
      let errText = "Hmm, something went wrong on my end — try again in a sec! 🙏"
      if (status === '401') {
        errText = "Hey! You'll need to log in to chat with me. Head over and sign in, then come back — I'll be here! 👋"
      } else if (status === '429') {
        errText = "Whoa, too many messages too fast! Give me a moment to catch my breath 😅"
      } else if (status === '503' || status === '502') {
        errText = "I'm taking a quick nap 😴 Our servers are restarting — try again in about 30 seconds!"
      }
      addMessage(errText, 'lexi')
    } finally {
      setIsLoading(false)
    }
  }

  const handleSuggestedAction = async (prompt: string) => {
    if (prompt.toLowerCase().includes('mock interview')) {
      setIsMockInterview(true)
      setMockRole('Software Engineer')
      addMessage(prompt, 'user')
      setIsLoading(true)
      try {
        const res = await startMockInterview({ role: 'Software Engineer', level: 'Entry Level', previousAnswer: '' })
        addMessage(res.response, 'lexi', true)
        setHistory([
          { role: 'user', content: prompt },
          { role: 'assistant', content: res.response },
        ])
      } catch (err: any) {
        if (err?.code !== 'SUBSCRIPTION_REQUIRED') {
          addMessage("Couldn't start the interview right now — try again in a moment!", 'lexi')
        }
      } finally {
        setIsLoading(false)
      }
    } else {
      await sendMessage(prompt)
    }
  }

  const handleKeyDown = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter' && !e.shiftKey) {
      e.preventDefault()
      sendMessage(input)
    }
  }

  const handleReset = () => {
    setMessages([])
    setHistory([])
    setIsMockInterview(false)
    setMockRole('')
  }

  return (
    <>
      {/* Floating Button */}
      <motion.button
        whileHover={{ scale: 1.08 }}
        whileTap={{ scale: 0.92 }}
        onClick={() => setIsOpen(o => !o)}
        className="fixed bottom-6 right-6 w-14 h-14 bg-gradient-to-tr from-violet-600 to-fuchsia-600 text-white rounded-full shadow-2xl flex items-center justify-center z-50 border-2 border-white/80 group"
        aria-label="Open Lexi chatbot"
      >
        <AnimatePresence mode="wait">
          {isOpen ? (
            <motion.div key="close" initial={{ rotate: -90, opacity: 0 }} animate={{ rotate: 0, opacity: 1 }} exit={{ rotate: 90, opacity: 0 }}>
              <X className="w-6 h-6" />
            </motion.div>
          ) : (
            <motion.div key="bot" initial={{ rotate: 90, opacity: 0 }} animate={{ rotate: 0, opacity: 1 }} exit={{ rotate: -90, opacity: 0 }} className="relative">
              <Bot className="w-6 h-6" />
              <motion.span
                className="absolute -top-0.5 -right-0.5 w-2.5 h-2.5 bg-green-400 border-2 border-white rounded-full"
                animate={{ scale: [1, 1.3, 1] }}
                transition={{ duration: 2, repeat: Infinity, repeatDelay: 3 }}
              />
            </motion.div>
          )}
        </AnimatePresence>

        {hasUnread && !isOpen && (
          <motion.span
            initial={{ scale: 0 }}
            animate={{ scale: 1 }}
            className="absolute -top-1 -right-1 w-4 h-4 bg-red-500 rounded-full border-2 border-white text-[8px] flex items-center justify-center text-white font-bold"
          >
            1
          </motion.span>
        )}

        {!isOpen && (
          <div className="absolute right-16 bottom-1/2 translate-y-1/2 bg-gray-900 text-white text-xs font-medium px-3 py-1.5 rounded-lg whitespace-nowrap opacity-0 group-hover:opacity-100 transition-opacity pointer-events-none shadow-xl">
            Chat with Lexi ✨
          </div>
        )}
      </motion.button>

      {/* Chat Panel */}
      <AnimatePresence>
        {isOpen && (
          <motion.div
            initial={{ opacity: 0, y: 80, scale: 0.88, x: 20 }}
            animate={{ opacity: 1, y: 0, scale: 1, x: 0 }}
            exit={{ opacity: 0, y: 80, scale: 0.88, x: 20 }}
            transition={{ type: 'spring', damping: 22, stiffness: 280 }}
            className="fixed bottom-[88px] right-6 w-[370px] h-[540px] z-50 flex flex-col rounded-2xl shadow-2xl overflow-hidden border border-gray-200 bg-white"
          >
            {/* Header */}
            <div className="bg-gradient-to-r from-violet-600 to-fuchsia-600 text-white px-4 py-3 flex items-center justify-between flex-shrink-0">
              <div className="flex items-center gap-3">
                <div className="relative">
                  <div className="w-9 h-9 rounded-full bg-white/20 flex items-center justify-center">
                    <Bot className="w-5 h-5" />
                  </div>
                  <motion.span
                    className="absolute bottom-0 right-0 w-2.5 h-2.5 bg-green-400 border-2 border-fuchsia-600 rounded-full"
                    animate={{ scale: [1, 1.2, 1] }}
                    transition={{ duration: 2, repeat: Infinity }}
                  />
                </div>
                <div>
                  <p className="font-bold text-sm leading-tight">Lexi</p>
                  <p className="text-[10px] text-violet-200">LiveExpert.AI · Always here for you</p>
                </div>
              </div>
              <div className="flex items-center gap-1">
                {isMockInterview && (
                  <span className="text-[10px] bg-green-400/30 text-green-100 px-2 py-0.5 rounded-full font-medium">
                    Interview Mode
                  </span>
                )}
                {messages.length > 0 && (
                  <button
                    onClick={handleReset}
                    title="Start over"
                    className="p-1.5 hover:bg-white/20 rounded-full transition-colors text-white/80 hover:text-white"
                  >
                    <RefreshCw className="w-3.5 h-3.5" />
                  </button>
                )}
                <button onClick={() => setIsOpen(false)} className="p-1.5 hover:bg-white/20 rounded-full transition-colors ml-0.5">
                  <Minimize2 className="w-4 h-4" />
                </button>
              </div>
            </div>

            {/* Messages */}
            <div ref={scrollRef} className="flex-1 overflow-y-auto p-4 space-y-4 bg-gray-50/60">
              {messages.length === 0 && (
                <div className="h-full flex flex-col items-center justify-center text-center space-y-5 pb-4">
                  <motion.div
                    initial={{ scale: 0.8, opacity: 0 }}
                    animate={{ scale: 1, opacity: 1 }}
                    transition={{ type: 'spring', stiffness: 200 }}
                    className="w-16 h-16 rounded-2xl bg-gradient-to-br from-violet-100 to-fuchsia-100 flex items-center justify-center"
                  >
                    <Smile className="w-8 h-8 text-violet-600" />
                  </motion.div>
                  <div>
                    <p className="font-bold text-gray-900 text-base">Hey, I'm Lexi! 👋</p>
                    <p className="text-sm text-gray-500 mt-1 leading-relaxed px-2">
                      Your learning companion — ask me anything, from homework help to career advice.
                    </p>
                  </div>
                  <div className="grid grid-cols-2 gap-2 w-full">
                    {SUGGESTED_ACTIONS.map((action, i) => (
                      <motion.button
                        key={i}
                        initial={{ opacity: 0, y: 10 }}
                        animate={{ opacity: 1, y: 0 }}
                        transition={{ delay: 0.1 + i * 0.07 }}
                        onClick={() => handleSuggestedAction(action.prompt)}
                        className="flex items-center gap-2 px-3 py-2.5 bg-white border border-gray-100 rounded-xl text-left text-xs font-semibold text-gray-700 hover:border-violet-300 hover:bg-violet-50 transition-all shadow-sm"
                      >
                        <div className="w-6 h-6 rounded-lg bg-violet-100 flex items-center justify-center text-violet-600 flex-shrink-0">
                          {action.icon}
                        </div>
                        <span className="leading-tight">{action.label}</span>
                      </motion.button>
                    ))}
                  </div>
                </div>
              )}

              {messages.map((msg, idx) => (
                <motion.div
                  key={msg.id}
                  initial={{ opacity: 0, y: 8, scale: 0.97 }}
                  animate={{ opacity: 1, y: 0, scale: 1 }}
                  transition={{ type: 'spring', stiffness: 300, damping: 24 }}
                  className={`flex ${msg.sender === 'user' ? 'justify-end' : 'justify-start'}`}
                >
                  <div className={`max-w-[88%] flex gap-2 ${msg.sender === 'user' ? 'flex-row-reverse' : 'flex-row'}`}>
                    <div className={`w-7 h-7 rounded-full flex-shrink-0 flex items-center justify-center mt-0.5 ${
                      msg.sender === 'user'
                        ? 'bg-gradient-to-br from-violet-600 to-fuchsia-600 text-white'
                        : 'bg-gradient-to-br from-violet-100 to-fuchsia-100 text-violet-600'
                    }`}>
                      {msg.sender === 'user' ? <User className="w-3.5 h-3.5" /> : <Bot className="w-3.5 h-3.5" />}
                    </div>
                    {msg.sender === 'lexi' ? (
                      <LexiMessage msg={msg} isLatest={idx === messages.length - 1} />
                    ) : (
                      <div className="px-3 py-2.5 rounded-2xl rounded-tr-none text-sm leading-relaxed shadow-sm bg-gradient-to-br from-violet-600 to-fuchsia-600 text-white">
                        <p>{msg.text}</p>
                      </div>
                    )}
                  </div>
                </motion.div>
              ))}

              {isLoading && (
                <motion.div
                  initial={{ opacity: 0, y: 6 }}
                  animate={{ opacity: 1, y: 0 }}
                  className="flex justify-start"
                >
                  <div className="flex gap-2">
                    <div className="w-7 h-7 rounded-full bg-gradient-to-br from-violet-100 to-fuchsia-100 text-violet-600 flex items-center justify-center mt-0.5">
                      <Bot className="w-3.5 h-3.5" />
                    </div>
                    <div className="bg-white border border-gray-100 px-3 py-2.5 rounded-2xl rounded-tl-none shadow-sm">
                      <TypingDots />
                    </div>
                  </div>
                </motion.div>
              )}
            </div>

            {/* Input */}
            <div className="p-3 bg-white border-t border-gray-100 flex-shrink-0">
              <div className="flex items-center gap-2 bg-gray-50 border border-gray-200 rounded-xl px-3 py-1.5 focus-within:border-violet-400 focus-within:bg-white transition-all">
                <input
                  ref={inputRef}
                  type="text"
                  value={input}
                  onChange={e => setInput(e.target.value)}
                  onKeyDown={handleKeyDown}
                  disabled={isLoading}
                  placeholder={isMockInterview ? 'Type your answer…' : 'Ask Lexi anything…'}
                  className="flex-1 text-sm bg-transparent focus:outline-none text-gray-800 placeholder-gray-400"
                />
                <motion.button
                  whileTap={{ scale: 0.85 }}
                  onClick={() => sendMessage(input)}
                  disabled={isLoading || !input.trim()}
                  className="p-1.5 bg-gradient-to-br from-violet-600 to-fuchsia-600 text-white rounded-lg hover:opacity-90 disabled:opacity-30 transition-all flex-shrink-0"
                >
                  {isLoading
                    ? <Loader2 className="w-3.5 h-3.5 animate-spin" />
                    : <Send className="w-3.5 h-3.5" />
                  }
                </motion.button>
              </div>
              <p className="text-center text-[10px] text-gray-400 mt-1.5">Lexi can make mistakes. Use your judgment.</p>
            </div>
          </motion.div>
        )}
      </AnimatePresence>
    </>
  )
}

export default LexiChatbot
