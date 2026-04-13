import { useState } from 'react'
import { Code2, Loader2 } from 'lucide-react'
import { reviewCode } from '../../services/aiApi'
import { PaidGuard } from '../../components/subscription/PaidGuard'
import { AIMarkdown } from '../../components/ui/AIMarkdown'
import { stripCodeFences } from '../../utils/aiUtils'

const LANGUAGES = ['JavaScript', 'TypeScript', 'Python', 'Java', 'C++', 'C#', 'SQL', 'HTML/CSS']

interface Issue { severity: 'error' | 'warning' | 'suggestion'; message: string }
interface ReviewResult { overallScore: number; issues: Issue[]; improvedCode: string; explanations: string[] }

const severityStyle = (s: string) =>
  s === 'error' ? 'bg-red-50 text-red-700 border-red-200' : s === 'warning' ? 'bg-amber-50 text-amber-700 border-amber-200' : 'bg-blue-50 text-blue-700 border-blue-200'

const scoreColor = (n: number) => n >= 80 ? 'text-green-600 bg-green-50 border-green-200' : n >= 60 ? 'text-amber-600 bg-amber-50 border-amber-200' : 'text-red-600 bg-red-50 border-red-200'

export default function CodeReview() {
  const [code, setCode] = useState('')
  const [language, setLanguage] = useState(LANGUAGES[0])
  const [context, setContext] = useState('')
  const [loading, setLoading] = useState(false)
  const [result, setResult] = useState<ReviewResult | null>(null)
  const [rawResponse, setRawResponse] = useState('')
  const [error, setError] = useState('')

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!code.trim()) { setError('Please paste some code to review.'); return }
    setLoading(true); setError(''); setResult(null); setRawResponse('')
    try {
      const res = await reviewCode({ code, language, context })
      const dataSource = res.data || res.rawResponse
      if (dataSource) {
        try {
          const cleaned = typeof dataSource === 'string' ? stripCodeFences(dataSource) : dataSource
          const raw = typeof cleaned === 'string' ? JSON.parse(cleaned) : cleaned
          const parsed = raw?.root ? raw.root : raw
          setResult(parsed)
        } catch { setRawResponse(res.rawResponse as string || '') }
      } else if (res.rawResponse) {
        setRawResponse(res.rawResponse as string)
      }
    } catch (e: any) { setError(e.message) }
    finally { setLoading(false) }
  }

  return (
    <PaidGuard featureName="Code Reviewer">
      <div className="p-6 max-w-5xl mx-auto">
        <div className="flex items-center gap-3 mb-8">
          <div className="w-10 h-10 bg-rose-100 rounded-xl flex items-center justify-center">
            <Code2 className="w-5 h-5 text-rose-600" />
          </div>
          <div>
            <h1 className="text-2xl font-bold text-gray-900">Code Reviewer</h1>
            <p className="text-sm text-gray-500">AI code review with bugs, improvements & score</p>
          </div>
        </div>

        <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
          {/* Left Panel */}
          <form onSubmit={handleSubmit} className="space-y-4">
            <div className="bg-white rounded-2xl border border-gray-100 p-5 shadow-sm space-y-4">
              <div className="flex items-center gap-3">
                <div className="flex-1">
                  <label className="block text-sm font-semibold text-gray-700 mb-2">Language</label>
                  <select value={language} onChange={e => setLanguage(e.target.value)} className="w-full border border-gray-200 rounded-xl px-4 py-2.5 text-sm focus:outline-none focus:ring-2 focus:ring-rose-500">
                    {LANGUAGES.map(l => <option key={l}>{l}</option>)}
                  </select>
                </div>
              </div>
              <div>
                <label className="block text-sm font-semibold text-gray-700 mb-2">Your Code</label>
                <textarea
                  value={code}
                  onChange={e => setCode(e.target.value)}
                  placeholder="Paste your code here..."
                  className="w-full border border-gray-200 rounded-xl px-4 py-3 text-sm font-mono min-h-[280px] resize-y focus:outline-none focus:ring-2 focus:ring-rose-500 bg-gray-50"
                />
              </div>
              <div>
                <label className="block text-sm font-semibold text-gray-700 mb-2">Context <span className="text-gray-400 font-normal">(optional)</span></label>
                <input value={context} onChange={e => setContext(e.target.value)} placeholder="What does this code do?" className="w-full border border-gray-200 rounded-xl px-4 py-2.5 text-sm focus:outline-none focus:ring-2 focus:ring-rose-500" />
              </div>
              <button type="submit" disabled={loading} className="w-full flex items-center justify-center gap-2 bg-rose-500 hover:bg-rose-600 text-white font-bold py-3 rounded-xl transition-colors disabled:opacity-60">
                {loading ? <><Loader2 className="w-4 h-4 animate-spin" />Reviewing Code...</> : 'Review My Code'}
              </button>
            </div>
          </form>

          {/* Right Panel */}
          <div className="space-y-4">
            {error && <div className="bg-red-50 text-red-700 rounded-xl p-4 text-sm">{error}</div>}

            {!result && !rawResponse && !loading && (
              <div className="bg-gray-50 rounded-2xl border border-dashed border-gray-200 p-8 text-center">
                <Code2 className="w-10 h-10 text-gray-300 mx-auto mb-3" />
                <p className="text-sm text-gray-400">Paste your code and click Review to get AI feedback</p>
              </div>
            )}

            {loading && (
              <div className="bg-white rounded-2xl border border-gray-100 p-8 text-center shadow-sm">
                <Loader2 className="w-8 h-8 text-rose-500 animate-spin mx-auto mb-3" />
                <p className="text-sm text-gray-500">Analysing your code...</p>
              </div>
            )}

            {result && (
              <>
                {/* Score */}
                <div className={`rounded-2xl border p-5 text-center ${scoreColor(result.overallScore)}`}>
                  <p className="text-4xl font-black">{result.overallScore}<span className="text-xl font-bold">/100</span></p>
                  <p className="text-sm font-semibold mt-1">Overall Score</p>
                </div>

                {/* Issues */}
                {result.issues?.length > 0 && (
                  <div className="bg-white rounded-2xl border border-gray-100 p-5 shadow-sm">
                    <p className="text-xs font-bold text-gray-500 uppercase mb-3">Issues Found ({result.issues.length})</p>
                    <div className="space-y-2">
                      {result.issues.map((issue, i) => (
                        <div key={i} className={`rounded-xl border px-4 py-2.5 text-sm flex items-start gap-2 ${severityStyle(issue.severity)}`}>
                          <span className="font-bold uppercase text-xs flex-shrink-0 mt-0.5">{issue.severity}</span>
                          <span>{issue.message}</span>
                        </div>
                      ))}
                    </div>
                  </div>
                )}

                {/* Improved Code */}
                {result.improvedCode && (
                  <div className="bg-white rounded-2xl border border-gray-100 p-5 shadow-sm">
                    <p className="text-xs font-bold text-gray-500 uppercase mb-3">Improved Code</p>
                    <pre className="bg-gray-900 text-green-400 rounded-xl p-4 text-xs overflow-x-auto whitespace-pre-wrap font-mono leading-relaxed">{result.improvedCode}</pre>
                  </div>
                )}

                {/* Explanations */}
                {result.explanations?.length > 0 && (
                  <div className="bg-white rounded-2xl border border-gray-100 p-5 shadow-sm">
                    <p className="text-xs font-bold text-gray-500 uppercase mb-3">Explanations</p>
                    <ul className="space-y-2">
                      {result.explanations.map((exp, i) => (
                        <li key={i} className="flex items-start gap-2 text-sm text-gray-700">
                          <span className="text-rose-500 font-bold flex-shrink-0">{i + 1}.</span>{exp}
                        </li>
                      ))}
                    </ul>
                  </div>
                )}
              </>
            )}

            {rawResponse && !result && (
              <div className="bg-gray-50 rounded-2xl border border-gray-100 p-5">
                <p className="text-xs font-bold text-gray-500 uppercase mb-3">AI Review</p>
                <AIMarkdown text={rawResponse} />
              </div>
            )}
          </div>
        </div>
      </div>
    </PaidGuard>
  )
}
