import { useState } from 'react'
import { Calendar, Loader2 } from 'lucide-react'
import { generateStudySchedule } from '../../services/aiApi'
import { PaidGuard } from '../../components/subscription/PaidGuard'
import { AIMarkdown } from '../../components/ui/AIMarkdown'
import { stripCodeFences } from '../../utils/aiUtils'

const HOURS_OPTIONS = [1, 2, 3, 4, 5, 6]

interface DaySchedule { day: string; date?: string; tasks: string[]; focusTopic: string; hoursPlanned: number }
interface ScheduleResult { schedule: DaySchedule[]; totalDays: number; studyTips: string[] }

export default function StudySchedule() {
  const [subject, setSubject] = useState('')
  const [examDate, setExamDate] = useState('')
  const [hoursPerDay, setHoursPerDay] = useState(3)
  const [currentLevel, setCurrentLevel] = useState('Beginner')
  const [topics, setTopics] = useState('')
  const [loading, setLoading] = useState(false)
  const [result, setResult] = useState<ScheduleResult | null>(null)
  const [rawResponse, setRawResponse] = useState('')
  const [error, setError] = useState('')

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setLoading(true); setError(''); setResult(null); setRawResponse('')
    try {
      const res = await generateStudySchedule({ subject, examDate, hoursPerDay, currentLevel, topics })
      const dataSource = res.schedule || res.rawResponse
      if (dataSource) {
        try {
          const cleaned = typeof dataSource === 'string' ? stripCodeFences(dataSource) : dataSource
          const raw = typeof cleaned === 'string' ? JSON.parse(cleaned) : cleaned
          const data = raw?.root ? raw.root : raw
          if (data?.schedule) setResult(data)
          else if (Array.isArray(data)) setResult({ schedule: data, totalDays: data.length, studyTips: [] })
          else setRawResponse(res.rawResponse || JSON.stringify(raw))
        } catch { setRawResponse(res.rawResponse || '') }
      } else {
        setRawResponse(res.rawResponse as string || '')
      }
    } catch (e: any) { setError(e.message) }
    finally { setLoading(false) }
  }

  const dayColors = ['bg-cyan-50 border-cyan-200', 'bg-blue-50 border-blue-200', 'bg-indigo-50 border-indigo-200', 'bg-violet-50 border-violet-200', 'bg-purple-50 border-purple-200', 'bg-pink-50 border-pink-200', 'bg-rose-50 border-rose-200']

  return (
    <PaidGuard featureName="Study Scheduler">
      <div className="p-6 max-w-4xl mx-auto">
        <div className="flex items-center gap-3 mb-8">
          <div className="w-10 h-10 bg-cyan-100 rounded-xl flex items-center justify-center">
            <Calendar className="w-5 h-5 text-cyan-600" />
          </div>
          <div>
            <h1 className="text-2xl font-bold text-gray-900">Study Scheduler</h1>
            <p className="text-sm text-gray-500">Day-by-day timetable from today to your exam</p>
          </div>
        </div>

        <form onSubmit={handleSubmit} className="bg-white rounded-2xl border border-gray-100 p-6 shadow-sm space-y-5 mb-6">
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-semibold text-gray-700 mb-2">Subject / Course *</label>
              <input value={subject} onChange={e => setSubject(e.target.value)} required placeholder="e.g. Data Structures, JavaScript" className="w-full border border-gray-200 rounded-xl px-4 py-2.5 text-sm focus:outline-none focus:ring-2 focus:ring-cyan-500" />
            </div>
            <div>
              <label className="block text-sm font-semibold text-gray-700 mb-2">Exam / Deadline Date *</label>
              <input value={examDate} onChange={e => setExamDate(e.target.value)} type="date" required className="w-full border border-gray-200 rounded-xl px-4 py-2.5 text-sm focus:outline-none focus:ring-2 focus:ring-cyan-500" />
            </div>
          </div>
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-semibold text-gray-700 mb-2">Hours Available Per Day</label>
              <div className="flex flex-wrap gap-2">
                {HOURS_OPTIONS.map(h => (
                  <button key={h} type="button" onClick={() => setHoursPerDay(h)} className={`px-4 py-2 rounded-xl text-sm font-medium border transition-colors ${hoursPerDay === h ? 'bg-cyan-600 text-white border-cyan-600' : 'border-gray-200 text-gray-600 hover:border-cyan-300'}`}>{h}h</button>
                ))}
              </div>
            </div>
            <div>
              <label className="block text-sm font-semibold text-gray-700 mb-2">Current Level</label>
              <div className="flex gap-2">
                {['Beginner', 'Intermediate', 'Advanced'].map(l => (
                  <button key={l} type="button" onClick={() => setCurrentLevel(l)} className={`flex-1 py-2 rounded-xl text-sm font-medium border transition-colors ${currentLevel === l ? 'bg-cyan-600 text-white border-cyan-600' : 'border-gray-200 text-gray-600 hover:border-cyan-300'}`}>{l}</button>
                ))}
              </div>
            </div>
          </div>
          <div>
            <label className="block text-sm font-semibold text-gray-700 mb-2">Topics to Cover <span className="text-gray-400 font-normal">(optional)</span></label>
            <textarea value={topics} onChange={e => setTopics(e.target.value)} placeholder="List the chapters or topics you need to cover..." className="w-full border border-gray-200 rounded-xl px-4 py-3 text-sm min-h-[80px] resize-none focus:outline-none focus:ring-2 focus:ring-cyan-500" />
          </div>
          <button type="submit" disabled={loading} className="w-full flex items-center justify-center gap-2 bg-cyan-600 hover:bg-cyan-700 text-white font-bold py-3 rounded-xl transition-colors disabled:opacity-60">
            {loading ? <><Loader2 className="w-4 h-4 animate-spin" />Building Schedule...</> : 'Generate Study Schedule'}
          </button>
        </form>

        {error && <div className="bg-red-50 text-red-700 rounded-xl p-4 text-sm mb-6">{error}</div>}

        {result && (
          <div className="space-y-4">
            {result.studyTips?.length > 0 && (
              <div className="bg-cyan-50 border border-cyan-200 rounded-2xl p-5">
                <p className="text-xs font-bold text-cyan-700 uppercase mb-3">Study Tips</p>
                <ul className="space-y-1">
                  {result.studyTips.map((tip, i) => (
                    <li key={i} className="flex items-start gap-2 text-sm text-cyan-800"><span className="text-cyan-500 font-bold flex-shrink-0">•</span>{tip}</li>
                  ))}
                </ul>
              </div>
            )}

            <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
              {result.schedule?.map((day, i) => (
                <div key={i} className={`rounded-2xl border p-4 ${dayColors[i % dayColors.length]}`}>
                  <div className="flex items-center justify-between mb-3">
                    <div>
                      <p className="font-bold text-gray-900 text-sm">{day.day}</p>
                      {day.date && <p className="text-xs text-gray-500">{day.date}</p>}
                    </div>
                    <span className="text-xs font-bold bg-white border border-current px-2 py-0.5 rounded-full text-cyan-700">{day.hoursPlanned}h</span>
                  </div>
                  {day.focusTopic && (
                    <p className="text-xs font-semibold text-cyan-700 mb-2 bg-white/60 rounded-lg px-2 py-1">Focus: {day.focusTopic}</p>
                  )}
                  <ul className="space-y-1">
                    {day.tasks?.map((task, j) => (
                      <li key={j} className="text-xs text-gray-700 flex items-start gap-1.5">
                        <span className="text-cyan-500 flex-shrink-0 mt-0.5">▸</span>{task}
                      </li>
                    ))}
                  </ul>
                </div>
              ))}
            </div>
          </div>
        )}

        {rawResponse && !result && (
          <div className="bg-white rounded-2xl border border-gray-100 p-5 shadow-sm">
            <p className="text-xs font-bold text-cyan-700 uppercase mb-3">Your Study Schedule</p>
            <AIMarkdown text={rawResponse} />
          </div>
        )}
      </div>
    </PaidGuard>
  )
}
