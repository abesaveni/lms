import { useState } from 'react'
import { motion, AnimatePresence } from 'framer-motion'
import { Mic, ChevronRight, RotateCcw, Download } from 'lucide-react'
import { startMockInterview } from '../../services/aiApi'
import { PaidGuard } from '../../components/subscription/PaidGuard'
import { AIMarkdown } from '../../components/ui/AIMarkdown'

const JOB_ROLES = ['Frontend Developer','Backend Developer','Full Stack Developer','Data Analyst','DevOps Engineer','UI/UX Designer','Android Developer','Product Manager','Data Scientist','Cloud Architect']
const LEVELS = ['Fresher','1-2 years','3-5 years','5+ years']

interface HistoryItem { question: string; answer: string }

export default function MockInterview() {
  const [step, setStep] = useState<'setup'|'interview'|'report'>('setup')
  const [role, setRole] = useState(JOB_ROLES[0])
  const [level, setLevel] = useState(LEVELS[0])
  const [history, setHistory] = useState<HistoryItem[]>([])
  const [currentQuestion, setCurrentQuestion] = useState('')
  const [feedback, setFeedback] = useState('')
  const [answer, setAnswer] = useState('')
  const [loading, setLoading] = useState(false)
  const [report, setReport] = useState('')

  const questionNumber = history.length + 1

  const startInterview = async () => {
    setLoading(true)
    try {
      const res = await startMockInterview({ role, level, previousAnswer: '' })
      setCurrentQuestion(res.response)
      setFeedback('')
      setStep('interview')
    } catch (e: any) { alert(e.message) }
    finally { setLoading(false) }
  }

  const submitAnswer = async () => {
    if (!answer.trim()) return
    setLoading(true)
    const newHistory = [...history, { question: currentQuestion, answer }]
    try {
      if (newHistory.length >= 10) {
        // Generate final report
        const prompt = `Generate a final mock interview report. Role: ${role}, Level: ${level}. Questions & Answers:\n${newHistory.map((h,i)=>`Q${i+1}: ${h.question}\nA: ${h.answer}`).join('\n\n')}\n\nProvide: Overall Score (x/100), Key Strengths (3 bullets), Areas to Improve (3 bullets), Next Steps (3 bullets), and a final encouragement.`
        const res = await startMockInterview({ role, level: 'Final Report', previousAnswer: prompt })
        setReport(res.response)
        setHistory(newHistory)
        setStep('report')
      } else {
        const res = await startMockInterview({ role, level, previousAnswer: answer })
        setFeedback(res.response.split('?')[0] + (res.response.includes('?') ? '' : ''))
        const lines = res.response.split('\n')
        const qLine = lines.find(l => l.includes('?')) || res.response
        setCurrentQuestion(qLine)
        setFeedback(res.response)
        setHistory(newHistory)
        setAnswer('')
      }
    } catch (e: any) { alert(e.message) }
    finally { setLoading(false) }
  }

  const reset = () => { setStep('setup'); setHistory([]); setAnswer(''); setFeedback(''); setCurrentQuestion(''); setReport('') }

  return (
    <PaidGuard featureName="Mock Interview">
      <div className="p-6 max-w-3xl mx-auto">
        <div className="flex items-center gap-3 mb-8">
          <div className="w-10 h-10 bg-purple-100 rounded-xl flex items-center justify-center">
            <Mic className="w-5 h-5 text-purple-600" />
          </div>
          <div>
            <h1 className="text-2xl font-bold text-gray-900">Mock Interview</h1>
            <p className="text-sm text-gray-500">Practice with AI — get real feedback</p>
          </div>
        </div>

        <AnimatePresence mode="wait">
          {step === 'setup' && (
            <motion.div key="setup" initial={{opacity:0,y:20}} animate={{opacity:1,y:0}} exit={{opacity:0,y:-20}} className="bg-white rounded-2xl border border-gray-100 p-6 shadow-sm space-y-5">
              <div>
                <label className="block text-sm font-semibold text-gray-700 mb-2">Target Role</label>
                <select value={role} onChange={e=>setRole(e.target.value)} className="w-full border border-gray-200 rounded-xl px-4 py-2.5 text-sm focus:outline-none focus:ring-2 focus:ring-purple-500">
                  {JOB_ROLES.map(r=><option key={r}>{r}</option>)}
                </select>
              </div>
              <div>
                <label className="block text-sm font-semibold text-gray-700 mb-2">Experience Level</label>
                <div className="flex flex-wrap gap-2">
                  {LEVELS.map(l=>(
                    <button key={l} onClick={()=>setLevel(l)} className={`px-4 py-2 rounded-full text-sm font-medium border transition-colors ${level===l ? 'bg-purple-600 text-white border-purple-600' : 'border-gray-200 text-gray-600 hover:border-purple-300'}`}>{l}</button>
                  ))}
                </div>
              </div>
              <button onClick={startInterview} disabled={loading} className="w-full flex items-center justify-center gap-2 bg-purple-600 hover:bg-purple-700 text-white font-bold py-3 rounded-xl transition-colors disabled:opacity-60">
                {loading ? <div className="w-5 h-5 border-2 border-white border-t-transparent rounded-full animate-spin"/> : <><ChevronRight className="w-4 h-4"/>Start Interview</>}
              </button>
            </motion.div>
          )}

          {step === 'interview' && (
            <motion.div key="interview" initial={{opacity:0,y:20}} animate={{opacity:1,y:0}} exit={{opacity:0,y:-20}} className="space-y-5">
              {/* Progress */}
              <div className="bg-white rounded-2xl border border-gray-100 p-4 shadow-sm">
                <div className="flex items-center justify-between mb-2">
                  <span className="text-sm font-semibold text-gray-700">Question {Math.min(questionNumber,10)} of 10</span>
                  <span className="text-xs text-gray-400">{role} · {level}</span>
                </div>
                <div className="w-full bg-gray-100 rounded-full h-2">
                  <div className="bg-purple-600 h-2 rounded-full transition-all" style={{width:`${Math.min(questionNumber-1,10)*10}%`}} />
                </div>
              </div>

              {/* Feedback from previous */}
              {feedback && history.length > 0 && (
                <div className="bg-purple-50 border border-purple-100 rounded-2xl p-5">
                  <p className="text-xs font-bold text-purple-600 uppercase mb-2">Interviewer Response</p>
                  <AIMarkdown text={feedback} />
                </div>
              )}

              {/* Current question */}
              <div className="bg-white rounded-2xl border border-gray-100 p-5 shadow-sm">
                <p className="text-xs font-bold text-gray-400 uppercase mb-3">Current Question</p>
                <div className="text-gray-900 font-medium">
                  <AIMarkdown text={currentQuestion} />
                </div>
              </div>

              {/* Answer */}
              <div className="bg-white rounded-2xl border border-gray-100 p-5 shadow-sm space-y-3">
                <textarea
                  value={answer}
                  onChange={e=>setAnswer(e.target.value)}
                  placeholder="Type your answer here..."
                  className="w-full border border-gray-200 rounded-xl px-4 py-3 text-sm min-h-[140px] resize-none focus:outline-none focus:ring-2 focus:ring-purple-500"
                />
                <button onClick={submitAnswer} disabled={loading||!answer.trim()} className="w-full flex items-center justify-center gap-2 bg-purple-600 hover:bg-purple-700 text-white font-bold py-3 rounded-xl transition-colors disabled:opacity-60">
                  {loading ? <div className="w-5 h-5 border-2 border-white border-t-transparent rounded-full animate-spin"/> : <>{questionNumber >= 10 ? 'Finish & Get Report' : 'Submit Answer'}</>}
                </button>
              </div>
            </motion.div>
          )}

          {step === 'report' && (
            <motion.div key="report" initial={{opacity:0,y:20}} animate={{opacity:1,y:0}} className="space-y-5">
              <div className="bg-white rounded-2xl border border-gray-100 p-6 shadow-sm">
                <div className="flex items-center justify-between mb-5">
                  <h2 className="text-lg font-bold text-gray-900">Interview Report</h2>
                  <div className="flex gap-2">
                    <button onClick={()=>window.print()} className="flex items-center gap-1.5 text-sm border border-gray-200 rounded-lg px-3 py-1.5 hover:bg-gray-50 transition-colors">
                      <Download className="w-4 h-4"/>Print
                    </button>
                    <button onClick={reset} className="flex items-center gap-1.5 text-sm bg-purple-600 text-white rounded-lg px-3 py-1.5 hover:bg-purple-700 transition-colors">
                      <RotateCcw className="w-4 h-4"/>Try Again
                    </button>
                  </div>
                </div>
                <AIMarkdown text={report} />
              </div>
            </motion.div>
          )}
        </AnimatePresence>
      </div>
    </PaidGuard>
  )
}
