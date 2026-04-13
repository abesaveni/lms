import { useState } from 'react'
import { ClipboardList, Loader2 } from 'lucide-react'
import { getAssignmentHelp } from '../../services/aiApi'
import { PaidGuard } from '../../components/subscription/PaidGuard'
import { AIMarkdown } from '../../components/ui/AIMarkdown'
import { stripCodeFences } from '../../utils/aiUtils'

const SUBJECT_TYPES = ['Programming', 'Mathematics', 'Science', 'English / Writing', 'History', 'Economics', 'Data Science', 'Design', 'Other']
const TABS = ['Approach', 'Research', 'Topics', 'Checklist'] as const
type Tab = typeof TABS[number]

interface AssignmentResult {
  approach?: string
  researchPointers?: string[]
  topicsToCover?: string[]
  checklist?: string[]
  rawResponse?: string
}

export default function AssignmentHelper() {
  const [title, setTitle] = useState('')
  const [description, setDescription] = useState('')
  const [subjectType, setSubjectType] = useState(SUBJECT_TYPES[0])
  const [deadline, setDeadline] = useState('')
  const [loading, setLoading] = useState(false)
  const [result, setResult] = useState<AssignmentResult | null>(null)
  const [activeTab, setActiveTab] = useState<Tab>('Approach')
  const [error, setError] = useState('')

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setLoading(true); setError(''); setResult(null)
    try {
      const res = await getAssignmentHelp({ title, description, subjectType, deadline })
      const dataSource = res.data || res.rawResponse
      if (dataSource) {
        try {
          const cleaned = typeof dataSource === 'string' ? stripCodeFences(dataSource) : dataSource
          const parsed = typeof cleaned === 'string' ? JSON.parse(cleaned) : cleaned
          setResult(parsed?.root ? parsed.root : parsed)
        } catch { setResult({ rawResponse: res.rawResponse as string }) }
      } else {
        setResult({ rawResponse: res.rawResponse as string })
      }
    } catch (e: any) { setError(e.message) }
    finally { setLoading(false) }
  }

  const tabContent = () => {
    if (!result) return null
    if (result.rawResponse && !result.approach && !result.researchPointers) {
      return <AIMarkdown text={result.rawResponse} />
    }
    switch (activeTab) {
      case 'Approach':
        return result.approach ? <AIMarkdown text={result.approach} /> : <p className="text-gray-400 text-sm">No approach data available.</p>
      case 'Research':
        return result.researchPointers?.length ? (
          <ul className="space-y-2">{result.researchPointers.map((r, i) => <li key={i} className="flex items-start gap-2 text-sm text-gray-700"><span className="text-pink-500 font-bold flex-shrink-0">→</span>{r}</li>)}</ul>
        ) : <p className="text-gray-400 text-sm">No research pointers available.</p>
      case 'Topics':
        return result.topicsToCover?.length ? (
          <div className="flex flex-wrap gap-2">{result.topicsToCover.map((t, i) => <span key={i} className="bg-pink-100 text-pink-700 text-sm font-medium px-3 py-1.5 rounded-full">{t}</span>)}</div>
        ) : <p className="text-gray-400 text-sm">No topics available.</p>
      case 'Checklist':
        return result.checklist?.length ? (
          <ul className="space-y-2">{result.checklist.map((item, i) => (
            <li key={i} className="flex items-start gap-3 text-sm text-gray-700">
              <div className="w-5 h-5 border-2 border-pink-400 rounded flex-shrink-0 mt-0.5" />
              {item}
            </li>
          ))}</ul>
        ) : <p className="text-gray-400 text-sm">No checklist available.</p>
    }
  }

  return (
    <PaidGuard featureName="Assignment Helper">
      <div className="p-6 max-w-3xl mx-auto">
        <div className="flex items-center gap-3 mb-8">
          <div className="w-10 h-10 bg-pink-100 rounded-xl flex items-center justify-center">
            <ClipboardList className="w-5 h-5 text-pink-600" />
          </div>
          <div>
            <h1 className="text-2xl font-bold text-gray-900">Assignment Helper</h1>
            <p className="text-sm text-gray-500">Guided approach to solve your assignments</p>
          </div>
        </div>

        <form onSubmit={handleSubmit} className="bg-white rounded-2xl border border-gray-100 p-6 shadow-sm space-y-5 mb-6">
          <div>
            <label className="block text-sm font-semibold text-gray-700 mb-2">Assignment Title *</label>
            <input value={title} onChange={e => setTitle(e.target.value)} required placeholder="e.g. Build a REST API with authentication" className="w-full border border-gray-200 rounded-xl px-4 py-2.5 text-sm focus:outline-none focus:ring-2 focus:ring-pink-500" />
          </div>
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-semibold text-gray-700 mb-2">Subject Type</label>
              <select value={subjectType} onChange={e => setSubjectType(e.target.value)} className="w-full border border-gray-200 rounded-xl px-4 py-2.5 text-sm focus:outline-none focus:ring-2 focus:ring-pink-500">
                {SUBJECT_TYPES.map(s => <option key={s}>{s}</option>)}
              </select>
            </div>
            <div>
              <label className="block text-sm font-semibold text-gray-700 mb-2">Deadline <span className="text-gray-400 font-normal">(optional)</span></label>
              <input value={deadline} onChange={e => setDeadline(e.target.value)} type="date" className="w-full border border-gray-200 rounded-xl px-4 py-2.5 text-sm focus:outline-none focus:ring-2 focus:ring-pink-500" />
            </div>
          </div>
          <div>
            <label className="block text-sm font-semibold text-gray-700 mb-2">Assignment Description</label>
            <textarea value={description} onChange={e => setDescription(e.target.value)} placeholder="Describe the assignment in detail — requirements, constraints, what needs to be submitted..." className="w-full border border-gray-200 rounded-xl px-4 py-3 text-sm min-h-[120px] resize-none focus:outline-none focus:ring-2 focus:ring-pink-500" />
          </div>
          <button type="submit" disabled={loading} className="w-full flex items-center justify-center gap-2 bg-pink-500 hover:bg-pink-600 text-white font-bold py-3 rounded-xl transition-colors disabled:opacity-60">
            {loading ? <><Loader2 className="w-4 h-4 animate-spin" />Analysing Assignment...</> : 'Get Guided Help'}
          </button>
        </form>

        {error && <div className="bg-red-50 text-red-700 rounded-xl p-4 text-sm mb-6">{error}</div>}

        {result && (
          <div className="bg-white rounded-2xl border border-gray-100 shadow-sm overflow-hidden">
            {/* Tabs */}
            <div className="flex border-b border-gray-100">
              {TABS.map(tab => (
                <button key={tab} onClick={() => setActiveTab(tab)} className={`flex-1 py-3 text-sm font-semibold transition-colors ${activeTab === tab ? 'text-pink-600 border-b-2 border-pink-500 bg-pink-50' : 'text-gray-500 hover:text-gray-700'}`}>
                  {tab}
                </button>
              ))}
            </div>
            <div className="p-5">
              {tabContent()}
            </div>
          </div>
        )}
      </div>
    </PaidGuard>
  )
}
