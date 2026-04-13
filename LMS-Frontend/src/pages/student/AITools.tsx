import { Link } from 'react-router-dom'
import { motion } from 'framer-motion'
import {
  Bot, FileText, Mic, MapPin, Linkedin, Lightbulb, Code2,
  Globe, BookOpen, CreditCard, ClipboardList, Calendar, Heart, Sparkles
} from 'lucide-react'
import { useSubscription } from '../../hooks/useSubscription'

const tools = [
  { icon: Bot, title: 'AI Chatbot Lexi', desc: 'Chat with Lexi for instant learning help on any topic.', path: '/student/ai-assistant', color: 'indigo' },
  { icon: FileText, title: 'Resume Builder', desc: 'Build an ATS-optimised resume in minutes.', path: '/student/resume-builder', color: 'blue' },
  { icon: Mic, title: 'Mock Interview', desc: 'Practice interviews with real-time AI feedback.', path: '/student/mock-interview', color: 'purple' },
  { icon: MapPin, title: 'Career Roadmap', desc: '6-month personalised roadmap for your career goal.', path: '/student/career-path', color: 'emerald' },
  { icon: Linkedin, title: 'LinkedIn Optimizer', desc: 'Rewrite your profile to attract more recruiters.', path: '/student/linkedin-optimizer', color: 'sky' },
  { icon: Lightbulb, title: 'Project Ideas', desc: 'Get 5 portfolio project ideas with build guides.', path: '/student/project-ideas', color: 'amber' },
  { icon: Code2, title: 'Code Reviewer', desc: 'AI code review with bugs, improvements & score.', path: '/student/code-review', color: 'rose' },
  { icon: Globe, title: 'Portfolio Generator', desc: 'Generate a complete HTML portfolio website.', path: '/student/portfolio-generator', color: 'violet' },
  { icon: BookOpen, title: 'Daily Quiz', desc: '10 fresh MCQs on any subject, any difficulty.', path: '/student/daily-quiz', color: 'orange' },
  { icon: CreditCard, title: 'Smart Flashcards', desc: 'Interactive flip cards with difficulty tracking.', path: '/student/flashcards', color: 'teal' },
  { icon: ClipboardList, title: 'Assignment Helper', desc: 'Guided approach to solve your assignments.', path: '/student/assignment-helper', color: 'pink' },
  { icon: Calendar, title: 'Study Scheduler', desc: 'Day-by-day timetable from today to your exam.', path: '/student/study-schedule', color: 'cyan' },
  { icon: Heart, title: 'Wellness Check-in', desc: 'Track your energy, stress & get personalised tips.', path: '/student/wellness', color: 'red' },
]

const colorMap: Record<string, string> = {
  indigo: 'bg-indigo-100 text-indigo-600',
  blue: 'bg-blue-100 text-blue-600',
  purple: 'bg-purple-100 text-purple-600',
  emerald: 'bg-emerald-100 text-emerald-600',
  sky: 'bg-sky-100 text-sky-600',
  amber: 'bg-amber-100 text-amber-600',
  rose: 'bg-rose-100 text-rose-600',
  violet: 'bg-violet-100 text-violet-600',
  orange: 'bg-orange-100 text-orange-600',
  teal: 'bg-teal-100 text-teal-600',
  pink: 'bg-pink-100 text-pink-600',
  cyan: 'bg-cyan-100 text-cyan-600',
  red: 'bg-red-100 text-red-600',
}

export default function AITools() {
  const { trialActive, daysLeftInTrial, subscriptionActive, isLoading } = useSubscription()

  return (
    <div className="p-6 max-w-6xl mx-auto">
      {/* Header */}
      <div className="mb-8">
        <div className="flex items-center gap-3 mb-2">
          <div className="w-10 h-10 bg-indigo-100 rounded-xl flex items-center justify-center">
            <Sparkles className="w-5 h-5 text-indigo-600" />
          </div>
          <div>
            <h1 className="text-2xl font-bold text-gray-900">AI Tools</h1>
            <p className="text-sm text-gray-500">Powered by LiveExpert.AI</p>
          </div>
          {!isLoading && (
            <div className="ml-auto">
              {subscriptionActive ? (
                <span className="inline-flex items-center gap-1.5 bg-green-100 text-green-700 text-xs font-bold px-3 py-1.5 rounded-full border border-green-200">
                  ✓ Subscribed
                </span>
              ) : trialActive ? (
                <span className="inline-flex items-center gap-1.5 bg-amber-100 text-amber-700 text-xs font-bold px-3 py-1.5 rounded-full border border-amber-200">
                  ⏳ {daysLeftInTrial} day{daysLeftInTrial !== 1 ? 's' : ''} left in trial
                </span>
              ) : (
                <Link
                  to="/student/subscription"
                  className="inline-flex items-center gap-1.5 bg-red-100 text-red-700 text-xs font-bold px-3 py-1.5 rounded-full border border-red-200 hover:bg-red-200 transition-colors"
                >
                  Trial expired — Subscribe ₹100/mo
                </Link>
              )}
            </div>
          )}
        </div>
        <p className="text-gray-600 mt-3">13 AI-powered tools to accelerate your learning and career.</p>
      </div>

      {/* Grid */}
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-4">
        {tools.map((tool, i) => {
          const Icon = tool.icon
          const iconClass = colorMap[tool.color] || 'bg-indigo-100 text-indigo-600'
          return (
            <motion.div
              key={tool.path}
              initial={{ opacity: 0, y: 20 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ delay: i * 0.04 }}
            >
              <Link
                to={tool.path}
                className="group flex flex-col bg-white border border-gray-100 rounded-2xl p-5 hover:shadow-lg hover:border-indigo-200 transition-all h-full"
              >
                <div className={`w-11 h-11 rounded-xl flex items-center justify-center mb-4 ${iconClass} group-hover:scale-110 transition-transform`}>
                  <Icon className="w-5 h-5" />
                </div>
                <h3 className="font-bold text-gray-900 text-sm mb-1">{tool.title}</h3>
                <p className="text-xs text-gray-500 flex-1 leading-relaxed">{tool.desc}</p>
                <div className="mt-4 text-xs font-semibold text-indigo-600 group-hover:text-indigo-700">
                  Open →
                </div>
              </Link>
            </motion.div>
          )
        })}
      </div>
    </div>
  )
}
