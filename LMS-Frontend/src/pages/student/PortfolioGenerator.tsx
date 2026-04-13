import { useState, useRef } from 'react'
import { Globe, Loader2, Download, Eye } from 'lucide-react'
import { generatePortfolio } from '../../services/aiApi'
import { PaidGuard } from '../../components/subscription/PaidGuard'
import { stripCodeFences } from '../../utils/aiUtils'

export default function PortfolioGenerator() {
  const [name, setName] = useState('')
  const [role, setRole] = useState('')
  const [bio, setBio] = useState('')
  const [skills, setSkills] = useState('')
  const [projects, setProjects] = useState('')
  const [email, setEmail] = useState('')
  const [github, setGithub] = useState('')
  const [loading, setLoading] = useState(false)
  const [html, setHtml] = useState('')
  const [error, setError] = useState('')
  const [tab, setTab] = useState<'preview' | 'code'>('preview')
  const iframeRef = useRef<HTMLIFrameElement>(null)

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setLoading(true); setError(''); setHtml('')
    try {
      const res = await generatePortfolio({ name, role, bio, skills, projects, email, github })
      const rawContent = (res.html || res.rawResponse || '') as string
      // Strip markdown code fences if AI wrapped HTML in ```html...```
      const content = rawContent.startsWith('```') ? stripCodeFences(rawContent) : rawContent
      setHtml(content)
    } catch (e: any) { setError(e.message) }
    finally { setLoading(false) }
  }

  const downloadHtml = () => {
    const blob = new Blob([html], { type: 'text/html' })
    const url = URL.createObjectURL(blob)
    const a = document.createElement('a')
    a.href = url; a.download = `${name.replace(/\s+/g, '-').toLowerCase() || 'portfolio'}.html`
    a.click(); URL.revokeObjectURL(url)
  }

  return (
    <PaidGuard featureName="Portfolio Generator">
      <div className="p-6 max-w-5xl mx-auto">
        <div className="flex items-center gap-3 mb-8">
          <div className="w-10 h-10 bg-violet-100 rounded-xl flex items-center justify-center">
            <Globe className="w-5 h-5 text-violet-600" />
          </div>
          <div>
            <h1 className="text-2xl font-bold text-gray-900">Portfolio Generator</h1>
            <p className="text-sm text-gray-500">Generate a complete HTML portfolio website instantly</p>
          </div>
        </div>

        <div className="grid grid-cols-1 lg:grid-cols-5 gap-6">
          {/* Form */}
          <form onSubmit={handleSubmit} className="lg:col-span-2 space-y-4">
            <div className="bg-white rounded-2xl border border-gray-100 p-5 shadow-sm space-y-4">
              <div>
                <label className="block text-sm font-semibold text-gray-700 mb-2">Full Name *</label>
                <input value={name} onChange={e => setName(e.target.value)} required placeholder="Your Name" className="w-full border border-gray-200 rounded-xl px-4 py-2.5 text-sm focus:outline-none focus:ring-2 focus:ring-violet-500" />
              </div>
              <div>
                <label className="block text-sm font-semibold text-gray-700 mb-2">Role / Title *</label>
                <input value={role} onChange={e => setRole(e.target.value)} required placeholder="e.g. Full Stack Developer" className="w-full border border-gray-200 rounded-xl px-4 py-2.5 text-sm focus:outline-none focus:ring-2 focus:ring-violet-500" />
              </div>
              <div>
                <label className="block text-sm font-semibold text-gray-700 mb-2">Bio</label>
                <textarea value={bio} onChange={e => setBio(e.target.value)} placeholder="2–3 sentences about yourself" className="w-full border border-gray-200 rounded-xl px-4 py-3 text-sm min-h-[80px] resize-none focus:outline-none focus:ring-2 focus:ring-violet-500" />
              </div>
              <div>
                <label className="block text-sm font-semibold text-gray-700 mb-2">Skills <span className="text-gray-400 font-normal">(comma-separated)</span></label>
                <input value={skills} onChange={e => setSkills(e.target.value)} placeholder="React, Node.js, Python..." className="w-full border border-gray-200 rounded-xl px-4 py-2.5 text-sm focus:outline-none focus:ring-2 focus:ring-violet-500" />
              </div>
              <div>
                <label className="block text-sm font-semibold text-gray-700 mb-2">Projects <span className="text-gray-400 font-normal">(names or brief list)</span></label>
                <textarea value={projects} onChange={e => setProjects(e.target.value)} placeholder="E-commerce app, Blog platform, Chat app..." className="w-full border border-gray-200 rounded-xl px-4 py-3 text-sm min-h-[70px] resize-none focus:outline-none focus:ring-2 focus:ring-violet-500" />
              </div>
              <div>
                <label className="block text-sm font-semibold text-gray-700 mb-2">Email</label>
                <input value={email} onChange={e => setEmail(e.target.value)} type="email" placeholder="your@email.com" className="w-full border border-gray-200 rounded-xl px-4 py-2.5 text-sm focus:outline-none focus:ring-2 focus:ring-violet-500" />
              </div>
              <div>
                <label className="block text-sm font-semibold text-gray-700 mb-2">GitHub URL</label>
                <input value={github} onChange={e => setGithub(e.target.value)} placeholder="https://github.com/username" className="w-full border border-gray-200 rounded-xl px-4 py-2.5 text-sm focus:outline-none focus:ring-2 focus:ring-violet-500" />
              </div>
              <button type="submit" disabled={loading} className="w-full flex items-center justify-center gap-2 bg-violet-600 hover:bg-violet-700 text-white font-bold py-3 rounded-xl transition-colors disabled:opacity-60">
                {loading ? <><Loader2 className="w-4 h-4 animate-spin" />Generating...</> : 'Generate Portfolio'}
              </button>
            </div>
          </form>

          {/* Preview */}
          <div className="lg:col-span-3">
            {error && <div className="bg-red-50 text-red-700 rounded-xl p-4 text-sm mb-4">{error}</div>}

            {!html && !loading && (
              <div className="bg-gray-50 rounded-2xl border border-dashed border-gray-200 h-full min-h-[400px] flex flex-col items-center justify-center">
                <Globe className="w-12 h-12 text-gray-300 mb-3" />
                <p className="text-sm text-gray-400">Your portfolio preview will appear here</p>
              </div>
            )}

            {loading && (
              <div className="bg-white rounded-2xl border border-gray-100 min-h-[400px] flex flex-col items-center justify-center shadow-sm">
                <Loader2 className="w-8 h-8 text-violet-500 animate-spin mb-3" />
                <p className="text-sm text-gray-500">Building your portfolio...</p>
              </div>
            )}

            {html && (
              <div className="bg-white rounded-2xl border border-gray-100 shadow-sm overflow-hidden">
                <div className="flex items-center justify-between px-4 py-3 border-b border-gray-100">
                  <div className="flex gap-2">
                    <button onClick={() => setTab('preview')} className={`px-3 py-1.5 rounded-lg text-xs font-semibold flex items-center gap-1.5 transition-colors ${tab === 'preview' ? 'bg-violet-600 text-white' : 'text-gray-600 hover:bg-gray-100'}`}>
                      <Eye className="w-3.5 h-3.5" />Preview
                    </button>
                    <button onClick={() => setTab('code')} className={`px-3 py-1.5 rounded-lg text-xs font-semibold transition-colors ${tab === 'code' ? 'bg-violet-600 text-white' : 'text-gray-600 hover:bg-gray-100'}`}>
                      HTML
                    </button>
                  </div>
                  <button onClick={downloadHtml} className="flex items-center gap-1.5 text-xs font-semibold text-violet-600 hover:text-violet-700 transition-colors">
                    <Download className="w-3.5 h-3.5" />Download HTML
                  </button>
                </div>
                {tab === 'preview' ? (
                  <iframe ref={iframeRef} srcDoc={html} className="w-full h-[550px] border-0" title="Portfolio Preview" sandbox="allow-scripts" />
                ) : (
                  <pre className="text-xs font-mono text-gray-700 p-4 overflow-auto h-[550px] bg-gray-50 whitespace-pre-wrap">{html}</pre>
                )}
              </div>
            )}
          </div>
        </div>
      </div>
    </PaidGuard>
  )
}
