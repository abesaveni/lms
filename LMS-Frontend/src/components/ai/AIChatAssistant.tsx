import React, { useState, useRef, useEffect } from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import { 
  Send, 
  Bot, 
  User, 
  BookOpen, 
  CheckCircle2, 
  Users, 
  GraduationCap, 
  Layout, 
  MessageSquare,
  Loader2,
  X,
  Minimize2
} from 'lucide-react';
import { Card } from '../ui/Card';
import { sendAiChatMessage } from '../../services/aiApi';
import { getCurrentUser } from '../../utils/auth';

interface Message {
  id: string;
  text: string;
  sender: 'user' | 'ai';
  timestamp: Date;
}

// ---------------------------------------------------------------------------
// Markdown renderer — handles **bold**, *italic*, bullets, numbered lists
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

function AIMarkdown({ text }: { text: string }) {
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
            <span className="text-indigo-500 mt-px flex-shrink-0">•</span>
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
    if (!trimmed) { flushBullets(`bl-${idx}`); flushNumbered(`nl-${idx}`); return }

    const bulletMatch = trimmed.match(/^[*\-]\s+(.+)/)
    if (bulletMatch) { flushNumbered(`nl-${idx}`); bulletBuffer.push(bulletMatch[1]); return }

    const numberedMatch = trimmed.match(/^\d+\.\s+(.+)/)
    if (numberedMatch) { flushBullets(`bl-${idx}`); numberedBuffer.push(numberedMatch[1]); return }

    flushBullets(`bl-${idx}`)
    flushNumbered(`nl-${idx}`)
    nodes.push(<p key={idx} className="leading-relaxed">{renderInline(trimmed, idx)}</p>)
  })

  flushBullets('end-bl')
  flushNumbered('end-nl')
  return <div className="space-y-1 text-xs">{nodes}</div>
}

interface AIChatAssistantProps {
  userRole: 'student' | 'tutor' | 'admin';
}

export const AIChatAssistant: React.FC<AIChatAssistantProps> = ({ userRole }) => {
  const [isOpen, setIsOpen] = useState(false);
  const [messages, setMessages] = useState<Message[]>([]);
  const [inputValue, setInputValue] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const scrollRef = useRef<HTMLDivElement>(null);
  const user = getCurrentUser();

  const quickActions = 
    userRole === 'student' ? [
      { icon: <GraduationCap className="w-4 h-4" />, label: "Create a Quiz", prompt: "I'd like to create a study quiz. Can you help me?" },
      { icon: <BookOpen className="w-4 h-4" />, label: "Study Plan", prompt: "Help me create a study plan for my upcoming week." },
      { icon: <Users className="w-4 h-4" />, label: "Tutor Match", prompt: "Recommend a tutor for me based on my needs." }
    ] : userRole === 'tutor' ? [
      { icon: <Layout className="w-4 h-4" />, label: "Lesson Plan", prompt: "Help me create a detailed lesson plan for my next session." },
      { icon: <MessageSquare className="w-4 h-4" />, label: "Session Notes", prompt: "Generate professional notes based on my last session transcript." },
      { icon: <CheckCircle2 className="w-4 h-4" />, label: "Progress Report", prompt: "Create a progress report for one of my students." }
    ] : [ // Admin Actions
      { icon: <Bot className="w-4 h-4" />, label: "Churn Prediction", prompt: "I need to run a churn prediction analysis for my students." },
      { icon: <Send className="w-4 h-4" />, label: "Fraud Scanner", prompt: "Check for any suspicious activity or potentially fraudulent transactions." },
      { icon: <MessageSquare className="w-4 h-4" />, label: "Dispute Mediator", prompt: "Can you help me mediate a session dispute between a tutor and student?" },
      { icon: <Users className="w-4 h-4" />, label: "Verification Report", prompt: "Show me a report of all pending tutor verifications." },
      { icon: <BookOpen className="w-4 h-4" />, label: "Revenue Summary", prompt: "Can you provide a summary of the platform's revenue this month?" }
    ];

  useEffect(() => {
    if (scrollRef.current) {
      scrollRef.current.scrollTop = scrollRef.current.scrollHeight;
    }
  }, [messages, isOpen]);

  const handleSendMessage = async (text: string) => {
    if (!text.trim()) return;

    const userMessage: Message = {
      id: Date.now().toString(),
      text,
      sender: 'user',
      timestamp: new Date()
    };

    setMessages(prev => [...prev, userMessage]);
    setInputValue('');
    setIsLoading(true);

    try {
      const response = await sendAiChatMessage({
        message: text,
        userContext: `User: ${user?.username}, Role: ${userRole}`
      });

      const aiMessage: Message = {
        id: (Date.now() + 1).toString(),
        text: response.response,
        sender: 'ai',
        timestamp: new Date()
      };

      setMessages(prev => [...prev, aiMessage]);
    } catch (error) {
      const errorMessage: Message = {
        id: (Date.now() + 1).toString(),
        text: "I'm sorry, I'm having trouble connecting right now.",
        sender: 'ai',
        timestamp: new Date()
      };
      setMessages(prev => [...prev, errorMessage]);
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <>
      {/* Floating Action Button */}
      <motion.button
        whileHover={{ scale: 1.1 }}
        whileTap={{ scale: 0.9 }}
        onClick={() => setIsOpen(!isOpen)}
        className="fixed bottom-6 right-6 w-12 h-12 bg-gradient-to-tr from-primary-600 to-indigo-600 text-white rounded-full shadow-2xl flex items-center justify-center z-50 group border-2 border-white"
      >
        <AnimatePresence mode="wait">
          {isOpen ? (
            <motion.div
              key="close"
              initial={{ rotate: -90, opacity: 0 }}
              animate={{ rotate: 0, opacity: 1 }}
              exit={{ rotate: 90, opacity: 0 }}
            >
              <X className="w-5 h-5" />
            </motion.div>
          ) : (
            <motion.div
              key="bot"
              initial={{ rotate: 90, opacity: 0 }}
              animate={{ rotate: 0, opacity: 1 }}
              exit={{ rotate: -90, opacity: 0 }}
              className="relative"
            >
              <Bot className="w-5 h-5" />
              <span className="absolute -top-1 -right-1 w-2.5 h-2.5 bg-green-500 border-2 border-white rounded-full"></span>
            </motion.div>
          )}
        </AnimatePresence>
        
        {/* Tooltip */}
        {!isOpen && (
          <div className="absolute right-20 bg-gray-900 text-white px-3 py-1.5 rounded-lg text-sm font-medium whitespace-nowrap opacity-0 group-hover:opacity-100 transition-opacity duration-200 pointer-events-none shadow-xl">
            Ask AI Assistant ✨
          </div>
        )}
      </motion.button>

      {/* Chat Window */}
      <AnimatePresence>
        {isOpen && (
          <motion.div
            initial={{ opacity: 0, y: 100, scale: 0.8, x: 20 }}
            animate={{ opacity: 1, y: 0, scale: 1, x: 0 }}
            exit={{ opacity: 0, y: 100, scale: 0.8, x: 20 }}
            transition={{ type: "spring", damping: 25, stiffness: 300 }}
            className="fixed bottom-[85px] right-6 w-[360px] h-[450px] z-50 flex flex-col"
          >
            <Card className="flex flex-col h-full overflow-hidden border border-gray-200 shadow-2xl bg-white/95 backdrop-blur-sm">
              {/* Header */}
              <div className="p-4 bg-gradient-to-r from-primary-600 to-indigo-600 text-white flex items-center justify-between">
                <div className="flex items-center gap-3">
                  <div className="w-8 h-8 rounded-full bg-white/20 flex items-center justify-center">
                    <Bot className="w-5 h-5" />
                  </div>
                  <div>
                    <h3 className="font-bold text-sm">AI Learning Assistant</h3>
                    <p className="text-[10px] text-primary-100 font-medium">Always Online</p>
                  </div>
                </div>
                <div className="flex items-center gap-2">
                  <button onClick={() => setIsOpen(false)} className="hover:bg-white/10 p-1 rounded-md transition-colors">
                    <Minimize2 className="w-4 h-4" />
                  </button>
                </div>
              </div>

              {/* Messages Area */}
              <div 
                ref={scrollRef}
                className="flex-1 overflow-y-auto p-4 space-y-4 bg-gray-50/30"
              >
                {messages.length === 0 && (
                  <div className="h-full flex flex-col items-center justify-center text-center p-4 space-y-4">
                    <div className="w-16 h-16 rounded-2xl bg-primary-50 flex items-center justify-center text-primary-600">
                      <Bot className="w-8 h-8" />
                    </div>
                    <div>
                      <h4 className="font-bold text-gray-900">Hi, I'm your AI Assistant!</h4>
                      <p className="text-xs text-gray-500 mt-1">
                        How can I help you today?
                      </p>
                    </div>
                    
                    <div className="space-y-2 w-full max-w-[280px]">
                      {quickActions.map((action, idx) => (
                        <button
                          key={idx}
                          onClick={() => handleSendMessage(action.prompt)}
                          className="w-full flex items-center gap-3 p-2.5 bg-white border border-gray-100 rounded-xl text-left text-xs font-semibold text-gray-700 hover:border-primary-300 hover:bg-primary-50 transition-all shadow-sm"
                        >
                          <div className="w-7 h-7 rounded-lg bg-primary-100 flex items-center justify-center text-primary-600 flex-shrink-0">
                            {action.icon}
                          </div>
                          {action.label}
                        </button>
                      ))}
                    </div>
                  </div>
                )}

                {messages.map((message) => (
                  <motion.div
                    key={message.id}
                    initial={{ opacity: 0, scale: 0.95 }}
                    animate={{ opacity: 1, scale: 1 }}
                    className={`flex ${message.sender === 'user' ? 'justify-end' : 'justify-start'}`}
                  >
                    <div className={`max-w-[85%] flex gap-2 ${message.sender === 'user' ? 'flex-row-reverse' : 'flex-row'}`}>
                      <div className={`w-6 h-6 rounded-full flex-shrink-0 flex items-center justify-center text-[10px] ${
                        message.sender === 'user' ? 'bg-indigo-600 text-white' : 'bg-primary-100 text-primary-600'
                      }`}>
                        {message.sender === 'user' ? <User className="w-3 h-3" /> : <Bot className="w-3 h-3" />}
                      </div>
                      <div className={`p-3 rounded-2xl text-xs shadow-sm ${
                        message.sender === 'user'
                          ? 'bg-indigo-600 text-white rounded-tr-none'
                          : 'bg-white text-gray-800 border border-gray-100 rounded-tl-none'
                      }`}>
                        {message.sender === 'ai'
                          ? <AIMarkdown text={message.text} />
                          : <p className="leading-relaxed">{message.text}</p>
                        }
                      </div>
                    </div>
                  </motion.div>
                ))}

                {isLoading && (
                  <div className="flex justify-start">
                    <div className="flex gap-2">
                      <div className="w-6 h-6 rounded-full bg-primary-100 text-primary-600 flex items-center justify-center">
                        <Bot className="w-3 h-3" />
                      </div>
                      <div className="bg-white border border-gray-100 p-2.5 rounded-2xl rounded-tl-none shadow-sm flex items-center gap-2">
                        <Loader2 className="w-3 h-3 animate-spin text-primary-600" />
                      </div>
                    </div>
                  </div>
                )}
              </div>

              {/* Input Area */}
              <div className="p-4 bg-white border-t border-gray-50">
                <form 
                  onSubmit={(e) => {
                    e.preventDefault();
                    handleSendMessage(inputValue);
                  }}
                  className="relative flex items-center gap-2"
                >
                  <input
                    type="text"
                    value={inputValue}
                    onChange={(e) => setInputValue(e.target.value)}
                    disabled={isLoading}
                    placeholder="Ask anything..."
                    className="flex-1 px-4 py-2.5 bg-gray-50 border border-gray-100 rounded-xl text-xs focus:outline-none focus:ring-1 focus:ring-primary-400 focus:bg-white transition-all"
                  />
                  <button
                    type="submit"
                    disabled={isLoading || !inputValue.trim()}
                    className="p-2.5 bg-primary-600 text-white rounded-xl hover:bg-primary-700 disabled:opacity-30 transition-all flex-shrink-0"
                  >
                    <Send className="w-4 h-4" />
                  </button>
                </form>
              </div>
            </Card>
          </motion.div>
        )}
      </AnimatePresence>
    </>
  );
};
