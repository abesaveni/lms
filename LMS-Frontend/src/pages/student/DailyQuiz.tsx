import { useState } from 'react'
import { BookOpen, Loader2, RotateCcw, Trophy } from 'lucide-react'
import { getDailyQuiz } from '../../services/aiApi'
import { PaidGuard } from '../../components/subscription/PaidGuard'
import { AIMarkdown } from '../../components/ui/AIMarkdown'
import { stripCodeFences } from '../../utils/aiUtils'

const SUBJECTS = ['JavaScript', 'Python', 'React', 'Data Structures', 'Algorithms', 'SQL', 'System Design', 'Computer Networks', 'Operating Systems', 'Machine Learning', 'Web Development', 'Cloud Computing']
const DIFFICULTIES = ['Easy', 'Medium', 'Hard']

interface MCQ { question: string; options: string[]; correctAnswer: number; explanation: string }

export default function DailyQuiz() {
  const [subject, setSubject] = useState(SUBJECTS[0])
  const [difficulty, setDifficulty] = useState(DIFFICULTIES[0])
  const [loading, setLoading] = useState(false)
  const [questions, setQuestions] = useState<MCQ[]>([])
  const [current, setCurrent] = useState(0)
  const [selected, setSelected] = useState<number | null>(null)
  const [revealed, setRevealed] = useState(false)
  const [score, setScore] = useState(0)
  const [answered, setAnswered] = useState<boolean[]>([])
  const [done, setDone] = useState(false)
  const [error, setError] = useState('')
  const [rawFallback, setRawFallback] = useState('')

  const startQuiz = async () => {
    setLoading(true); setError(''); setRawFallback('')
    try {
      const res = await getDailyQuiz({ subject, difficulty })
      let arr: MCQ[] = []
      const dataSource = res.questions || res.rawResponse
      if (dataSource) {
        try {
          const cleaned = typeof dataSource === 'string' ? stripCodeFences(dataSource) : dataSource
          const raw = typeof cleaned === 'string' ? JSON.parse(cleaned) : cleaned
          arr = Array.isArray(raw) ? raw : (raw?.root ? raw.root : [])
        } catch { arr = [] }
      }
      if (arr.length === 0) { setRawFallback(res.rawResponse as string || ''); return }
      setQuestions(arr)
      setCurrent(0); setSelected(null); setRevealed(false); setScore(0); setAnswered([]); setDone(false)
    } catch (e: any) { setError(e.message) }
    finally { setLoading(false) }
  }

  const handleSelect = (idx: number) => {
    if (revealed) return
    setSelected(idx); setRevealed(true)
    const correct = questions[current].correctAnswer
    const newAnswered = [...answered]
    newAnswered[current] = idx === correct
    setAnswered(newAnswered)
    if (idx === correct) setScore(s => s + 1)
  }

  const next = () => {
    if (current + 1 >= questions.length) { setDone(true); return }
    setCurrent(c => c + 1); setSelected(null); setRevealed(false)
  }

  const reset = () => { setQuestions([]); setDone(false); setCurrent(0); setSelected(null); setRevealed(false); setScore(0); setAnswered([]) }

  const q = questions[current]

  const optionStyle = (i: number) => {
    if (!revealed) return selected === i ? 'border-orange-400 bg-orange-50' : 'border-gray-200 hover:border-orange-300 hover:bg-orange-50'
    if (i === q.correctAnswer) return 'border-green-500 bg-green-50 text-green-800'
    if (i === selected && i !== q.correctAnswer) return 'border-red-400 bg-red-50 text-red-700'
    return 'border-gray-200 opacity-60'
  }

  return (
    <PaidGuard featureName="Daily Quiz">
      <div className="p-6 max-w-2xl mx-auto">
        <div className="flex items-center gap-3 mb-8">
          <div className="w-10 h-10 bg-orange-100 rounded-xl flex items-center justify-center">
            <BookOpen className="w-5 h-5 text-orange-600" />
          </div>
          <div>
            <h1 className="text-2xl font-bold text-gray-900">Daily Quiz</h1>
            <p className="text-sm text-gray-500">10 fresh MCQs on any subject, any difficulty</p>
          </div>
        </div>

        {questions.length === 0 && !done && (
          <div className="bg-white rounded-2xl border border-gray-100 p-6 shadow-sm space-y-5">
            <div>
              <label className="block text-sm font-semibold text-gray-700 mb-2">Subject</label>
              <select value={subject} onChange={e => setSubject(e.target.value)} className="w-full border border-gray-200 rounded-xl px-4 py-2.5 text-sm focus:outline-none focus:ring-2 focus:ring-orange-500">
                {SUBJECTS.map(s => <option key={s}>{s}</option>)}
              </select>
            </div>
            <div>
              <label className="block text-sm font-semibold text-gray-700 mb-2">Difficulty</label>
              <div className="flex gap-3">
                {DIFFICULTIES.map(d => (
                  <button key={d} type="button" onClick={() => setDifficulty(d)} className={`flex-1 py-2.5 rounded-xl text-sm font-medium border transition-colors ${difficulty === d ? 'bg-orange-500 text-white border-orange-500' : 'border-gray-200 text-gray-600 hover:border-orange-300'}`}>{d}</button>
                ))}
              </div>
            </div>
            {error && <div className="bg-red-50 text-red-700 rounded-xl p-4 text-sm">{error}</div>}
            {rawFallback && (
              <div className="bg-orange-50 border border-orange-100 rounded-xl p-4">
                <p className="text-xs font-bold text-orange-600 uppercase mb-2">Quiz Questions</p>
                <AIMarkdown text={rawFallback} />
              </div>
            )}
            <button onClick={startQuiz} disabled={loading} className="w-full flex items-center justify-center gap-2 bg-orange-500 hover:bg-orange-600 text-white font-bold py-3 rounded-xl transition-colors disabled:opacity-60">
              {loading ? <><Loader2 className="w-4 h-4 animate-spin" />Generating Quiz...</> : 'Start 10-Question Quiz'}
            </button>
          </div>
        )}

        {questions.length > 0 && !done && q && (
          <div className="space-y-5">
            {/* Progress */}
            <div className="bg-white rounded-2xl border border-gray-100 p-4 shadow-sm">
              <div className="flex items-center justify-between mb-2">
                <span className="text-sm font-semibold text-gray-700">Question {current + 1} of {questions.length}</span>
                <span className="text-sm font-bold text-orange-600">{score} correct</span>
              </div>
              <div className="w-full bg-gray-100 rounded-full h-2">
                <div className="bg-orange-500 h-2 rounded-full transition-all" style={{ width: `${(current / questions.length) * 100}%` }} />
              </div>
            </div>

            {/* Question */}
            <div className="bg-white rounded-2xl border border-gray-100 p-5 shadow-sm">
              <p className="font-semibold text-gray-900 leading-relaxed mb-5">{q.question}</p>
              <div className="space-y-3">
                {q.options?.map((opt, i) => (
                  <button key={i} onClick={() => handleSelect(i)} className={`w-full text-left px-4 py-3 rounded-xl border text-sm transition-all ${optionStyle(i)}`}>
                    <span className="font-bold mr-2">{String.fromCharCode(65 + i)}.</span>{opt}
                  </button>
                ))}
              </div>
            </div>

            {/* Explanation */}
            {revealed && (
              <div className={`rounded-2xl p-4 text-sm ${selected === q.correctAnswer ? 'bg-green-50 border border-green-200 text-green-800' : 'bg-red-50 border border-red-200 text-red-800'}`}>
                <p className="font-bold mb-1">{selected === q.correctAnswer ? '✓ Correct!' : '✗ Incorrect'}</p>
                <p>{q.explanation}</p>
              </div>
            )}

            {revealed && (
              <button onClick={next} className="w-full bg-orange-500 hover:bg-orange-600 text-white font-bold py-3 rounded-xl transition-colors">
                {current + 1 >= questions.length ? 'See Results' : 'Next Question →'}
              </button>
            )}
          </div>
        )}

        {done && (
          <div className="bg-white rounded-2xl border border-gray-100 p-8 shadow-sm text-center space-y-5">
            <Trophy className="w-14 h-14 text-orange-500 mx-auto" />
            <div>
              <p className="text-4xl font-black text-gray-900">{score}/{questions.length}</p>
              <p className="text-gray-500 mt-1">
                {score >= 8 ? 'Excellent! 🎉' : score >= 6 ? 'Great job! 👍' : score >= 4 ? 'Keep practising! 💪' : 'Keep studying! 📚'}
              </p>
            </div>
            <div className="grid grid-cols-10 gap-1.5 max-w-xs mx-auto">
              {answered.map((correct, i) => (
                <div key={i} className={`h-4 rounded-sm ${correct ? 'bg-green-500' : 'bg-red-400'}`} title={`Q${i + 1}`} />
              ))}
            </div>
            <button onClick={reset} className="flex items-center justify-center gap-2 mx-auto bg-orange-500 hover:bg-orange-600 text-white font-bold px-6 py-3 rounded-xl transition-colors">
              <RotateCcw className="w-4 h-4" />Try Another Quiz
            </button>
          </div>
        )}
      </div>
    </PaidGuard>
  )
}
