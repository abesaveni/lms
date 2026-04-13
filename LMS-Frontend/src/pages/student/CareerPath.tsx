import { useState } from 'react'
import { MapPin, Loader2 } from 'lucide-react'
import { getCareerPath } from '../../services/aiApi'
import { PaidGuard } from '../../components/subscription/PaidGuard'
import { AIMarkdown } from '../../components/ui/AIMarkdown'

const INTERESTS = ['Web Development','Mobile Development','Data Science','AI/ML','Cybersecurity','Cloud/DevOps','Game Development','UI/UX Design','Digital Marketing','Finance Tech']
const LEVELS = ['Beginner','Intermediate','Advanced']

export default function CareerPath() {
  const [interest, setInterest] = useState(INTERESTS[0])
  const [level, setLevel] = useState(LEVELS[0])
  const [goal, setGoal] = useState('')
  const [loading, setLoading] = useState(false)
  const [roadmap, setRoadmap] = useState('')
  const [error, setError] = useState('')

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setLoading(true); setError(''); setRoadmap('')
    try {
      const res = await getCareerPath({ interest, currentLevel: level, careerGoal: goal })
      setRoadmap(res.roadmap)
    } catch (e: any) { setError(e.message) }
    finally { setLoading(false) }
  }

  return (
    <PaidGuard featureName="Career Roadmap">
      <div className="p-6 max-w-3xl mx-auto">
        <div className="flex items-center gap-3 mb-8">
          <div className="w-10 h-10 bg-emerald-100 rounded-xl flex items-center justify-center">
            <MapPin className="w-5 h-5 text-emerald-600" />
          </div>
          <div>
            <h1 className="text-2xl font-bold text-gray-900">Career Roadmap</h1>
            <p className="text-sm text-gray-500">Your personalised 6-month tech career plan</p>
          </div>
        </div>

        <form onSubmit={handleSubmit} className="bg-white rounded-2xl border border-gray-100 p-6 shadow-sm space-y-5 mb-6">
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-semibold text-gray-700 mb-2">Interest Area</label>
              <select value={interest} onChange={e=>setInterest(e.target.value)} className="w-full border border-gray-200 rounded-xl px-4 py-2.5 text-sm focus:outline-none focus:ring-2 focus:ring-emerald-500">
                {INTERESTS.map(i=><option key={i}>{i}</option>)}
              </select>
            </div>
            <div>
              <label className="block text-sm font-semibold text-gray-700 mb-2">Current Level</label>
              <div className="flex gap-2">
                {LEVELS.map(l=>(
                  <button type="button" key={l} onClick={()=>setLevel(l)} className={`flex-1 py-2.5 rounded-xl text-sm font-medium border transition-colors ${level===l?'bg-emerald-600 text-white border-emerald-600':'border-gray-200 text-gray-600 hover:border-emerald-300'}`}>{l}</button>
                ))}
              </div>
            </div>
          </div>
          <div>
            <label className="block text-sm font-semibold text-gray-700 mb-2">Career Goal</label>
            <input value={goal} onChange={e=>setGoal(e.target.value)} placeholder="e.g. Get hired as a senior developer at a product startup" required className="w-full border border-gray-200 rounded-xl px-4 py-2.5 text-sm focus:outline-none focus:ring-2 focus:ring-emerald-500"/>
          </div>
          <button type="submit" disabled={loading} className="w-full flex items-center justify-center gap-2 bg-emerald-600 hover:bg-emerald-700 text-white font-bold py-3 rounded-xl transition-colors disabled:opacity-60">
            {loading ? <><Loader2 className="w-4 h-4 animate-spin"/>Generating Roadmap...</> : 'Generate My Roadmap'}
          </button>
        </form>

        {error && <div className="bg-red-50 text-red-700 rounded-xl p-4 text-sm mb-6">{error}</div>}

        {roadmap && (
          <div className="bg-white rounded-2xl border border-gray-100 p-6 shadow-sm">
            <h2 className="text-lg font-bold text-gray-900 mb-5">Your 6-Month Roadmap</h2>
            <AIMarkdown text={roadmap} />
          </div>
        )}
      </div>
    </PaidGuard>
  )
}
