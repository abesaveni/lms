import { useState, KeyboardEvent } from 'react'
import { Lightbulb, Loader2, ChevronDown, ChevronUp, X } from 'lucide-react'
import { getProjectIdeas } from '../../services/aiApi'
import { PaidGuard } from '../../components/subscription/PaidGuard'
import { AIMarkdown } from '../../components/ui/AIMarkdown'
import { stripCodeFences } from '../../utils/aiUtils'

const DOMAINS = ['Web','Mobile','Data Science','AI/ML','Cybersecurity','Game Dev','IoT','DevOps','Fintech','EdTech']
const EXP_LEVELS = ['Beginner','Intermediate','Advanced']

function TagInput({ tags, onChange, placeholder }: { tags: string[]; onChange: (t: string[]) => void; placeholder?: string }) {
  const [val, setVal] = useState('')
  const addVal = () => { if (val.trim()) { onChange([...tags, val.trim()]); setVal('') } }
  const onKey = (e: KeyboardEvent<HTMLInputElement>) => {
    if (e.key === 'Enter') { e.preventDefault(); addVal() }
  }
  return (
    <div className="border border-gray-200 rounded-xl px-3 py-2 flex flex-wrap gap-2 focus-within:ring-2 focus-within:ring-amber-500">
      {tags.map(t=>(
        <span key={t} className="flex items-center gap-1 bg-amber-100 text-amber-700 text-xs font-medium px-2.5 py-1 rounded-full">
          {t}<button type="button" onClick={()=>onChange(tags.filter(x=>x!==t))}><X className="w-3 h-3"/></button>
        </span>
      ))}
      <input value={val} onChange={e=>setVal(e.target.value)} onKeyDown={onKey} onBlur={addVal} placeholder={tags.length===0?placeholder:'Add more...'} className="flex-1 min-w-[120px] text-sm outline-none bg-transparent"/>
    </div>
  )
}

interface Project { title: string; description: string; techStack: string[]; features: string[]; buildSteps: string[]; difficultyLevel: string; estimatedDays: number }

export default function ProjectIdeas() {
  const [techStack, setTechStack] = useState<string[]>([])
  const [expLevel, setExpLevel] = useState(EXP_LEVELS[0])
  const [domain, setDomain] = useState(DOMAINS[0])
  const [loading, setLoading] = useState(false)
  const [projects, setProjects] = useState<Project[]>([])
  const [expanded, setExpanded] = useState<number | null>(null)
  const [error, setError] = useState('')
  const [rawFallback, setRawFallback] = useState('')

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    if (techStack.length === 0) { setError('Please add at least one technology.'); return }
    setLoading(true); setError(''); setProjects([]); setRawFallback('')
    try {
      const res = await getProjectIdeas({ techStack: techStack.join(', '), experienceLevel: expLevel, interestedDomain: domain })
      let arr: Project[] = []
      const dataSource = res.projects || res.rawResponse
      if (dataSource) {
        try {
          const cleaned = typeof dataSource === 'string' ? stripCodeFences(dataSource) : dataSource
          const raw = typeof cleaned === 'string' ? JSON.parse(cleaned) : cleaned
          const list = (raw as any)?.root ? (raw as any).root : raw
          arr = Array.isArray(list) ? list : []
        } catch { arr = [] }
      }
      setProjects(arr)
      if (arr.length === 0 && res.rawResponse) setRawFallback(res.rawResponse as string)
    } catch (e: any) { setError(e.message) }
    finally { setLoading(false) }
  }

  const diffColor = (d: string) => d?.toLowerCase().includes('adv') ? 'text-red-600 bg-red-50' : d?.toLowerCase().includes('int') ? 'text-amber-600 bg-amber-50' : 'text-green-600 bg-green-50'

  return (
    <PaidGuard featureName="Project Ideas">
      <div className="p-6 max-w-3xl mx-auto">
        <div className="flex items-center gap-3 mb-8">
          <div className="w-10 h-10 bg-amber-100 rounded-xl flex items-center justify-center">
            <Lightbulb className="w-5 h-5 text-amber-600" />
          </div>
          <div>
            <h1 className="text-2xl font-bold text-gray-900">Project Ideas</h1>
            <p className="text-sm text-gray-500">5 portfolio project ideas with step-by-step build guides</p>
          </div>
        </div>

        <form onSubmit={handleSubmit} className="bg-white rounded-2xl border border-gray-100 p-6 shadow-sm space-y-5 mb-6">
          <div>
            <label className="block text-sm font-semibold text-gray-700 mb-2">Tech Stack <span className="text-gray-400 font-normal">(press Enter to add)</span></label>
            <TagInput tags={techStack} onChange={setTechStack} placeholder="e.g. React, Node.js, MongoDB..."/>
          </div>
          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-semibold text-gray-700 mb-2">Experience Level</label>
              <select value={expLevel} onChange={e=>setExpLevel(e.target.value)} className="w-full border border-gray-200 rounded-xl px-4 py-2.5 text-sm focus:outline-none focus:ring-2 focus:ring-amber-500">
                {EXP_LEVELS.map(l=><option key={l}>{l}</option>)}
              </select>
            </div>
            <div>
              <label className="block text-sm font-semibold text-gray-700 mb-2">Domain</label>
              <select value={domain} onChange={e=>setDomain(e.target.value)} className="w-full border border-gray-200 rounded-xl px-4 py-2.5 text-sm focus:outline-none focus:ring-2 focus:ring-amber-500">
                {DOMAINS.map(d=><option key={d}>{d}</option>)}
              </select>
            </div>
          </div>
          <button type="submit" disabled={loading} className="w-full flex items-center justify-center gap-2 bg-amber-500 hover:bg-amber-600 text-white font-bold py-3 rounded-xl transition-colors disabled:opacity-60">
            {loading ? <><Loader2 className="w-4 h-4 animate-spin"/>Generating Ideas...</> : 'Generate 5 Project Ideas'}
          </button>
        </form>

        {error && <div className="bg-red-50 text-red-700 rounded-xl p-4 text-sm mb-6">{error}</div>}

        {rawFallback && projects.length === 0 && (
          <div className="bg-white rounded-2xl border border-gray-100 p-6 shadow-sm mb-6">
            <p className="text-xs font-bold text-amber-600 uppercase mb-3">Project Ideas</p>
            <AIMarkdown text={rawFallback} />
          </div>
        )}

        {projects.length > 0 && (
          <div className="space-y-4">
            {projects.map((p, i) => (
              <div key={i} className="bg-white rounded-2xl border border-gray-100 shadow-sm overflow-hidden">
                <button onClick={()=>setExpanded(expanded===i?null:i)} className="w-full flex items-start gap-4 p-5 text-left">
                  <div className="w-8 h-8 bg-amber-100 text-amber-700 rounded-xl flex items-center justify-center font-bold text-sm flex-shrink-0">{i+1}</div>
                  <div className="flex-1 min-w-0">
                    <div className="flex items-center gap-2 flex-wrap mb-1">
                      <h3 className="font-bold text-gray-900">{p.title}</h3>
                      {p.difficultyLevel && <span className={`text-xs font-bold px-2 py-0.5 rounded-full ${diffColor(p.difficultyLevel)}`}>{p.difficultyLevel}</span>}
                      {p.estimatedDays && <span className="text-xs text-gray-400">{p.estimatedDays} days</span>}
                    </div>
                    <p className="text-sm text-gray-500 line-clamp-2">{p.description}</p>
                    <div className="flex flex-wrap gap-1.5 mt-2">
                      {p.techStack?.map(t=><span key={t} className="bg-gray-100 text-gray-600 text-xs px-2 py-0.5 rounded">{t}</span>)}
                    </div>
                  </div>
                  {expanded===i ? <ChevronUp className="w-5 h-5 text-gray-400 flex-shrink-0"/> : <ChevronDown className="w-5 h-5 text-gray-400 flex-shrink-0"/>}
                </button>
                {expanded===i && (
                  <div className="px-5 pb-5 pt-0 space-y-4 border-t border-gray-100">
                    {p.features?.length > 0 && (
                      <div>
                        <p className="text-xs font-bold text-gray-500 uppercase mb-2">Features</p>
                        <ul className="space-y-1">{p.features.map((f,j)=><li key={j} className="flex items-start gap-2 text-sm text-gray-700"><span className="text-amber-500 mt-0.5">•</span>{f}</li>)}</ul>
                      </div>
                    )}
                    {p.buildSteps?.length > 0 && (
                      <div>
                        <p className="text-xs font-bold text-gray-500 uppercase mb-2">Build Steps</p>
                        <ol className="space-y-1">{p.buildSteps.map((s,j)=><li key={j} className="flex items-start gap-2 text-sm text-gray-700"><span className="font-bold text-amber-500 flex-shrink-0">{j+1}.</span>{s}</li>)}</ol>
                      </div>
                    )}
                  </div>
                )}
              </div>
            ))}
          </div>
        )}
      </div>
    </PaidGuard>
  )
}
