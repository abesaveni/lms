import React, { useState, useRef, useEffect } from 'react'
import { motion, AnimatePresence } from 'framer-motion'
import {
  Bot, Send, X, Minimize2, User, Loader2,
  Sparkles, BookOpen, Mic, GraduationCap
} from 'lucide-react'
import { sendChatbotMessage, startMockInterview, ChatMessage } from '../../services/aiApi'

interface Message {
  id: string
  text: string
  sender: 'user' | 'lexi'
  timestamp: Date
}

// ---------------------------------------------------------------------------
// Lightweight markdown renderer for Lexi responses
// Handles: **bold**, *italic*, bullet lines (* / -), numbered lists, blank lines
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
      <ul key={key} className="list-none space-y-0.5 my-1">
        {bulletBuffer.map((b, i) => (
          <li key={i} className="flex gap-1.5">
            <span className="text-indigo-500 mt-px">•</span>
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
      <ol key={key} className="list-none space-y-0.5 my-1">
        {numberedBuffer.map((b, i) => (
          <li key={i} className="flex gap-1.5">
            <span className="text-indigo-500 font-semibold flex-shrink-0">{i + 1}.</span>
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

  return <div className="space-y-1 text-xs">{nodes}</div>
}

const SUGGESTED_ACTIONS = [
  { icon: <GraduationCap className="w-4 h-4" />, label: 'Find me a tutor', prompt: 'Can you help me find the right tutor for my subject?' },
  { icon: <BookOpen className="w-4 h-4" />, label: 'Create study plan', prompt: 'Help me create a personalised study plan.' },
  { icon: <Mic className="w-4 h-4" />, label: 'Mock interview', prompt: 'Start a mock interview for a software engineering role.' },
  { icon: <Sparkles className="w-4 h-4" />, label: 'Course suggestions', prompt: 'Suggest courses based on my learning goals.' },
]

const LexiChatbot: React.FC = () => {
  const [isOpen, setIsOpen] = useState(false)
  const [messages, setMessages] = useState<Message[]>([])
  const [history, setHistory] = useState<ChatMessage[]>([])
  const [input, setInput] = useState('')
  const [isLoading, setIsLoading] = useState(false)
  const [isMockInterview, setIsMockInterview] = useState(false)
  const [mockRole, setMockRole] = useState('')
  const scrollRef = useRef<HTMLDivElement>(null)
  const inputRef = useRef<HTMLInputElement>(null)

  useEffect(() => {
    if (scrollRef.current) {
      scrollRef.current.scrollTop = scrollRef.current.scrollHeight
    }
  }, [messages, isOpen])

  useEffect(() => {
    if (isOpen && inputRef.current) {
      setTimeout(() => inputRef.current?.focus(), 300)
    }
  }, [isOpen])

  const addMessage = (text: string, sender: 'user' | 'lexi') => {
    const msg: Message = { id: Date.now().toString(), text, sender, timestamp: new Date() }
    setMessages(prev => [...prev, msg])
    return msg
  }

  const sendMessage = async (text: string) => {
    if (!text.trim() || isLoading) return
    setInput('')
    addMessage(text, 'user')
    setIsLoading(true)

    const newHistory: ChatMessage[] = [...history, { role: 'user', content: text }]
    setHistory(newHistory)

    try {
      if (isMockInterview) {
        // In mock interview mode — send latest user turn as previousAnswer
        const res = await startMockInterview({
          role: mockRole || 'Software Engineer',
          level: 'Entry Level',
          previousAnswer: text,
        })
        addMessage(res.response, 'lexi')
        setHistory(prev => [...prev, { role: 'assistant', content: res.response }])
      } else {
        const res = await sendChatbotMessage(
          history.map(m => ({ role: m.role, content: m.content })),
          text
        )
        addMessage(res.reply, 'lexi')
        setHistory(prev => [...prev, { role: 'assistant', content: res.reply }])
      }
    } catch (err: any) {
      if (err?.code !== 'SUBSCRIPTION_REQUIRED') {
        addMessage("Sorry, I'm having a moment! Please try again.", 'lexi')
      }
    } finally {
      setIsLoading(false)
    }
  }

  const handleSuggestedAction = async (prompt: string) => {
    // Detect mock interview start
    if (prompt.toLowerCase().includes('mock interview')) {
      setIsMockInterview(true)
      setMockRole('Software Engineer')
      addMessage(prompt, 'user')
      setIsLoading(true)
      try {
        const res = await startMockInterview({ role: 'Software Engineer', level: 'Entry Level', previousAnswer: '' })
        addMessage(res.response, 'lexi')
        setHistory([
          { role: 'user', content: prompt },
          { role: 'assistant', content: res.response },
        ])
      } catch (err: any) {
        if (err?.code !== 'SUBSCRIPTION_REQUIRED') {
          addMessage("Sorry, I couldn't start the interview. Please try again.", 'lexi')
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

  const handleClose = () => {
    setIsOpen(false)
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
        className="fixed bottom-6 right-6 w-14 h-14 bg-gradient-to-tr from-indigo-600 to-purple-600 text-white rounded-full shadow-2xl flex items-center justify-center z-50 border-2 border-white group"
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
              <span className="absolute -top-0.5 -right-0.5 w-2.5 h-2.5 bg-green-400 border-2 border-white rounded-full" />
            </motion.div>
          )}
        </AnimatePresence>

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
            initial={{ opacity: 0, y: 80, scale: 0.85, x: 20 }}
            animate={{ opacity: 1, y: 0, scale: 1, x: 0 }}
            exit={{ opacity: 0, y: 80, scale: 0.85, x: 20 }}
            transition={{ type: 'spring', damping: 25, stiffness: 300 }}
            className="fixed bottom-[88px] right-6 w-[360px] h-[500px] z-50 flex flex-col rounded-2xl shadow-2xl overflow-hidden border border-gray-200 bg-white"
          >
            {/* Header */}
            <div className="bg-gradient-to-r from-indigo-600 to-purple-600 text-white px-4 py-3 flex items-center justify-between flex-shrink-0">
              <div className="flex items-center gap-3">
                <div className="w-9 h-9 rounded-full bg-white/20 flex items-center justify-center">
                  <Bot className="w-5 h-5" />
                </div>
                <div>
                  <p className="font-bold text-sm leading-tight">Lexi</p>
                  <p className="text-[10px] text-indigo-200">LiveExpert.AI Assistant · Online</p>
                </div>
              </div>
              <div className="flex items-center gap-1">
                {messages.length > 0 && (
                  <button onClick={handleReset} className="text-[10px] text-white/70 hover:text-white px-2 py-1 rounded hover:bg-white/10 transition-colors">
                    Reset
                  </button>
                )}
                {isMockInterview && (
                  <span className="text-[10px] bg-green-400/30 text-green-100 px-2 py-0.5 rounded-full">Interview Mode</span>
                )}
                <button onClick={handleClose} className="p-1 hover:bg-white/20 rounded-full transition-colors ml-1">
                  <Minimize2 className="w-4 h-4" />
                </button>
              </div>
            </div>

            {/* Messages */}
            <div ref={scrollRef} className="flex-1 overflow-y-auto p-4 space-y-3 bg-gray-50/40">
              {messages.length === 0 && (
                <div className="h-full flex flex-col items-center justify-center text-center space-y-4 pb-4">
                  <div className="w-14 h-14 rounded-2xl bg-indigo-50 flex items-center justify-center text-indigo-600">
                    <Sparkles className="w-7 h-7" />
                  </div>
                  <div>
                    <p className="font-bold text-gray-900 text-sm">Hi, I'm Lexi! 👋</p>
                    <p className="text-xs text-gray-500 mt-1">Your LiveExpert.AI learning companion.</p>
                  </div>
                  <div className="space-y-2 w-full">
                    {SUGGESTED_ACTIONS.map((action, i) => (
                      <button
                        key={i}
                        onClick={() => handleSuggestedAction(action.prompt)}
                        className="w-full flex items-center gap-3 px-3 py-2.5 bg-white border border-gray-100 rounded-xl text-left text-xs font-semibold text-gray-700 hover:border-indigo-300 hover:bg-indigo-50 transition-all shadow-sm"
                      >
                        <div className="w-7 h-7 rounded-lg bg-indigo-100 flex items-center justify-center text-indigo-600 flex-shrink-0">
                          {action.icon}
                        </div>
                        {action.label}
                      </button>
                    ))}
                  </div>
                </div>
              )}

              {messages.map(msg => (
                <motion.div
                  key={msg.id}
                  initial={{ opacity: 0, y: 8 }}
                  animate={{ opacity: 1, y: 0 }}
                  className={`flex ${msg.sender === 'user' ? 'justify-end' : 'justify-start'}`}
                >
                  <div className={`max-w-[85%] flex gap-2 ${msg.sender === 'user' ? 'flex-row-reverse' : 'flex-row'}`}>
                    <div className={`w-6 h-6 rounded-full flex-shrink-0 flex items-center justify-center ${
                      msg.sender === 'user' ? 'bg-indigo-600 text-white' : 'bg-purple-100 text-purple-600'
                    }`}>
                      {msg.sender === 'user' ? <User className="w-3 h-3" /> : <Bot className="w-3 h-3" />}
                    </div>
                    <div className={`px-3 py-2 rounded-2xl text-xs leading-relaxed shadow-sm ${
                      msg.sender === 'user'
                        ? 'bg-indigo-600 text-white rounded-tr-none'
                        : 'bg-white text-gray-800 border border-gray-100 rounded-tl-none'
                    }`}>
                      {msg.sender === 'lexi'
                        ? <LexiMarkdown text={msg.text} />
                        : <p>{msg.text}</p>
                      }
                    </div>
                  </div>
                </motion.div>
              ))}

              {isLoading && (
                <div className="flex justify-start">
                  <div className="flex gap-2">
                    <div className="w-6 h-6 rounded-full bg-purple-100 text-purple-600 flex items-center justify-center">
                      <Bot className="w-3 h-3" />
                    </div>
                    <div className="bg-white border border-gray-100 px-3 py-2 rounded-2xl rounded-tl-none shadow-sm flex items-center gap-1.5">
                      <Loader2 className="w-3 h-3 animate-spin text-indigo-500" />
                      <span className="text-xs text-gray-400">Lexi is typing…</span>
                    </div>
                  </div>
                </div>
              )}
            </div>

            {/* Input */}
            <div className="p-3 bg-white border-t border-gray-100 flex-shrink-0">
              <div className="flex items-center gap-2">
                <input
                  ref={inputRef}
                  type="text"
                  value={input}
                  onChange={e => setInput(e.target.value)}
                  onKeyDown={handleKeyDown}
                  disabled={isLoading}
                  placeholder={isMockInterview ? 'Type your answer…' : 'Ask Lexi anything…'}
                  className="flex-1 px-3 py-2 text-xs bg-gray-50 border border-gray-200 rounded-xl focus:outline-none focus:ring-1 focus:ring-indigo-400 focus:bg-white transition-all"
                />
                <button
                  onClick={() => sendMessage(input)}
                  disabled={isLoading || !input.trim()}
                  className="p-2 bg-indigo-600 text-white rounded-xl hover:bg-indigo-700 disabled:opacity-30 transition-all flex-shrink-0"
                >
                  <Send className="w-4 h-4" />
                </button>
              </div>
            </div>
          </motion.div>
        )}
      </AnimatePresence>
    </>
  )
}

export default LexiChatbot
