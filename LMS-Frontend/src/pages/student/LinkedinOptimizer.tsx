import { useState, KeyboardEvent } from 'react'
import { Linkedin, Loader2, Copy, Check, X } from 'lucide-react'
import { optimiseLinkedIn } from '../../services/aiApi'
import { PaidGuard } from '../../components/subscription/PaidGuard'
import { AIMarkdown } from '../../components/ui/AIMarkdown'
import { stripCodeFences } from '../../utils/aiUtils'

function TagInput({ tags, onChange, placeholder }: { tags: string[]; onChange: (t: string[]) => void; placeholder?: string }) {
  const [val, setVal] = useState('')
  const addVal = () => { if (val.trim()) { onChange([...tags, val.trim()]); setVal('') } }
  const onKey = (e: KeyboardEvent<HTMLInputElement>) => {
    if (e.key === 'Enter') { e.preventDefault(); addVal() }
  }
  return (
    <div className="border border-gray-200 rounded-xl px-3 py-2 flex flex-wrap gap-2 focus-within:ring-2 focus-within:ring-sky-500">
      {tags.map(t=>(
        <span key={t} className="flex items-center gap-1 bg-sky-100 text-sky-700 text-xs font-medium px-2.5 py-1 rounded-full">
          {t}<button type="button" onClick={()=>onChange(tags.filter(x=>x!==t))}><X className="w-3 h-3"/></button>
        </span>
      ))}
      <input value={val} onChange={e=>setVal(e.target.value)} onKeyDown={onKey} onBlur={addVal} placeholder={tags.length===0?placeholder:'Add more...'} className="flex-1 min-w-[120px] text-sm outline-none bg-transparent"/>
    </div>
  )
}

function CopyCard({ label, content }: { label: string; content: string }) {
  const [copied, setCopied] = useState(false)
  const copy = () => { navigator.clipboard.writeText(content); setCopied(true); setTimeout(()=>setCopied(false),2000) }
  return (
    <div className="bg-gray-50 rounded-xl p-4 relative">
      <div className="flex items-center justify-between mb-2">
        <p className="text-xs font-bold text-gray-500 uppercase">{label}</p>
        <button onClick={copy} className="text-xs text-gray-400 hover:text-sky-600 flex items-center gap-1 transition-colors">
          {copied ? <><Check className="w-3 h-3"/>Copied!</> : <><Copy className="w-3 h-3"/>Copy</>}
        </button>
      </div>
      <p className="text-sm text-gray-700 leading-relaxed">{content}</p>
    </div>
  )
}

export default function LinkedInOptimizer() {
  const [about, setAbout] = useState('')
  const [headline, setHeadline] = useState('')
  const [skills, setSkills] = useState<string[]>([])
  const [targetRole, setTargetRole] = useState('')
  const [loading, setLoading] = useState(false)
  const [result, setResult] = useState<any>(null)
  const [error, setError] = useState('')

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setLoading(true); setError(''); setResult(null)
    try {
      const res = await optimiseLinkedIn({ currentAbout: about, currentHeadline: headline, skills: skills.join(', '), targetRole })
      let parsed = res.data || res
      if (typeof parsed === 'string') {
        try { parsed = JSON.parse(stripCodeFences(parsed)) } catch { /* keep as string */ }
      }
      setResult(parsed)
    } catch (e: any) { setError(e.message) }
    finally { setLoading(false) }
  }

  const data = result?.root ? (() => { try { return JSON.parse(JSON.stringify(result.root)) } catch { return null } })() : result

  return (
    <PaidGuard featureName="LinkedIn Optimizer">
      <div className="p-6 max-w-3xl mx-auto">
        <div className="flex items-center gap-3 mb-8">
          <div className="w-10 h-10 bg-sky-100 rounded-xl flex items-center justify-center">
            <Linkedin className="w-5 h-5 text-sky-600" />
          </div>
          <div>
            <h1 className="text-2xl font-bold text-gray-900">LinkedIn Optimizer</h1>
            <p className="text-sm text-gray-500">Rewrite your profile for maximum recruiter visibility</p>
          </div>
        </div>

        <form onSubmit={handleSubmit} className="bg-white rounded-2xl border border-gray-100 p-6 shadow-sm space-y-5 mb-6">
          <div>
            <label className="block text-sm font-semibold text-gray-700 mb-2">Target Role *</label>
            <input value={targetRole} onChange={e=>setTargetRole(e.target.value)} required placeholder="e.g. Senior React Developer" className="w-full border border-gray-200 rounded-xl px-4 py-2.5 text-sm focus:outline-none focus:ring-2 focus:ring-sky-500"/>
          </div>
          <div>
            <label className="block text-sm font-semibold text-gray-700 mb-2">Current Headline</label>
            <input value={headline} onChange={e=>setHeadline(e.target.value)} placeholder="Your current LinkedIn headline" className="w-full border border-gray-200 rounded-xl px-4 py-2.5 text-sm focus:outline-none focus:ring-2 focus:ring-sky-500"/>
          </div>
          <div>
            <label className="block text-sm font-semibold text-gray-700 mb-2">Current Skills <span className="text-gray-400 font-normal">(press Enter to add)</span></label>
            <TagInput tags={skills} onChange={setSkills} placeholder="Add a skill and press Enter..."/>
          </div>
          <div>
            <label className="block text-sm font-semibold text-gray-700 mb-2">Current About / Summary</label>
            <textarea value={about} onChange={e=>setAbout(e.target.value)} placeholder="Paste your current LinkedIn About section..." className="w-full border border-gray-200 rounded-xl px-4 py-3 text-sm min-h-[120px] resize-none focus:outline-none focus:ring-2 focus:ring-sky-500"/>
          </div>
          <button type="submit" disabled={loading||!targetRole} className="w-full flex items-center justify-center gap-2 bg-sky-600 hover:bg-sky-700 text-white font-bold py-3 rounded-xl transition-colors disabled:opacity-60">
            {loading ? <><Loader2 className="w-4 h-4 animate-spin"/>Optimising Profile...</> : 'Optimise My LinkedIn'}
          </button>
        </form>

        {error && <div className="bg-red-50 text-red-700 rounded-xl p-4 text-sm mb-6">{error}</div>}

        {result && (
          <div className="space-y-4">
            {data?.optimisedAbout && <CopyCard label="Optimised About Section" content={data.optimisedAbout}/>}
            {data?.optimisedHeadline && <CopyCard label="Optimised Headline" content={data.optimisedHeadline}/>}
            {Array.isArray(data?.skillsToAdd) && (
              <div className="bg-white rounded-2xl border border-gray-100 p-5 shadow-sm">
                <p className="text-xs font-bold text-gray-500 uppercase mb-3">10 Skills to Add</p>
                <div className="flex flex-wrap gap-2">
                  {data.skillsToAdd.map((s: string)=><span key={s} className="bg-sky-100 text-sky-700 text-xs font-medium px-3 py-1.5 rounded-full">{s}</span>)}
                </div>
              </div>
            )}
            {Array.isArray(data?.connectionMessages) && (
              <div className="bg-white rounded-2xl border border-gray-100 p-5 shadow-sm">
                <p className="text-xs font-bold text-gray-500 uppercase mb-3">Connection Message Templates</p>
                <div className="space-y-3">
                  {data.connectionMessages.map((m: string, i: number) => <CopyCard key={i} label={`Template ${i+1}`} content={m}/>)}
                </div>
              </div>
            )}
            {result.rawResponse && (
              <div className="bg-gray-50 rounded-2xl border border-gray-100 p-5">
                <AIMarkdown text={result.rawResponse} />
              </div>
            )}
          </div>
        )}
      </div>
    </PaidGuard>
  )
}
