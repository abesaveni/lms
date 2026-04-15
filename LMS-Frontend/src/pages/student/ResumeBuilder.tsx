import React, { useState } from 'react'
import { motion, AnimatePresence } from 'framer-motion'
import {
  FileText, Plus, Trash2, Download, Loader2,
  Briefcase, GraduationCap, User, AlertCircle,
  Upload, CheckCircle, Sparkles
} from 'lucide-react'
import {
  generateFresherResume, generateExperiencedResume,
  enhanceUploadedResume, downloadResumePdfBlob,
  FresherResumeRequest, ExperiencedResumeRequest
} from '../../services/aiApi'

// ---------------------------------------------------------------------------
// Types
// ---------------------------------------------------------------------------
interface Project {
  id: string
  title: string
  techStack: string
  description: string
}

interface WorkExperience {
  id: string
  company: string
  role: string
  duration: string
  responsibilities: string
}

type Tab = 'fresher' | 'experienced' | 'enhance'

// ---------------------------------------------------------------------------
// PDF download — calls the backend QuestPDF endpoint for a real vector PDF
// (ATS-friendly, selectable text, ~50–200 KB vs the old 5–10 MB canvas approach)
// ---------------------------------------------------------------------------
async function downloadResumePdf(candidateName: string) {
  const blob = await downloadResumePdfBlob()
  const url = URL.createObjectURL(blob)
  const a = document.createElement('a')
  a.href = url
  a.download = `${candidateName.trim().toLowerCase().replace(/\s+/g, '_') || 'resume'}_resume.pdf`
  document.body.appendChild(a)
  a.click()
  document.body.removeChild(a)
  URL.revokeObjectURL(url)
}

// ---------------------------------------------------------------------------
// ATS Score Badge + Suggestions
// ---------------------------------------------------------------------------
const ATS_KEYWORDS = ['experience', 'project', 'skill', 'developed', 'led', 'managed', 'achieved', 'improved', 'built', 'designed']
const ATS_SECTIONS = ['summary', 'objective', 'education', 'skills', 'experience', 'projects', 'certifications', 'contact']
const ATS_SUGGESTIONS: Record<string, string> = {
  experience: 'Add action verbs like "Developed", "Led", "Managed", or "Achieved" to describe impact.',
  project: 'Include at least 2–3 project descriptions with tech stack and outcomes.',
  skill: 'Add a dedicated Skills section listing relevant tools, languages, and frameworks.',
  developed: 'Use past-tense action verbs (Built, Developed, Deployed) to quantify achievements.',
  led: 'Highlight leadership experience with team size and outcomes.',
  managed: 'Describe what you managed (team, budget, timeline) and the result.',
  achieved: 'Quantify achievements — e.g., "Improved load time by 40%".',
  improved: 'Back up improvements with metrics or percentages.',
  built: 'Mention the technologies and scale of what you built.',
  designed: 'Describe the design context — problem solved, tools used, impact.',
  summary: 'Add a 2–3 sentence professional summary at the top.',
  certifications: 'List relevant certifications (AWS, Google, etc.) to boost ATS scores.',
  contact: 'Include LinkedIn URL, GitHub, and a professional email address.',
}

function getAtsAnalysis(resumeText: string) {
  const lower = resumeText.toLowerCase()
  const hits = ATS_KEYWORDS.filter(k => lower.includes(k))
  const score = Math.min(98, 50 + hits.length * 5)
  const missing = [...ATS_KEYWORDS, ...ATS_SECTIONS].filter(k => !lower.includes(k))
  const suggestions = missing.slice(0, 4).map(k => ATS_SUGGESTIONS[k]).filter(Boolean)
  return { score, suggestions }
}

function AtsScoreBadge({ resumeText }: { resumeText: string }) {
  const { score, suggestions } = getAtsAnalysis(resumeText)
  const isAtsFriendly = score >= 80

  return (
    <div className="space-y-2">
      {isAtsFriendly ? (
        <div className="inline-flex items-center gap-1.5 px-3 py-1 rounded-full border text-sm font-bold text-green-700 bg-green-50 border-green-200">
          <CheckCircle className="w-4 h-4" />
          ATS Friendly — Score {score}/100
        </div>
      ) : (
        <div className="inline-flex items-center gap-1.5 px-3 py-1 rounded-full border text-sm font-bold text-yellow-700 bg-yellow-50 border-yellow-200">
          <AlertCircle className="w-4 h-4" />
          ATS Score: {score}/100 — Needs Improvement
        </div>
      )}
      {!isAtsFriendly && suggestions.length > 0 && (
        <div className="bg-amber-50 border border-amber-200 rounded-xl p-3 space-y-1.5">
          <p className="text-xs font-bold text-amber-800 flex items-center gap-1">
            <Sparkles className="w-3.5 h-3.5" />
            Suggestions to reach 100/100
          </p>
          {suggestions.map((s, i) => (
            <p key={i} className="text-xs text-amber-700 pl-4 before:content-['•'] before:-ml-3 before:mr-1">{s}</p>
          ))}
        </div>
      )}
    </div>
  )
}

// ---------------------------------------------------------------------------
// Markdown → Resume renderer
// ---------------------------------------------------------------------------

/** Converts inline markdown (bold, italic) to React nodes */
function renderInline(line: string, key: string | number): React.ReactNode {
  // Split on **…** and *…*
  const parts = line.split(/(\*\*[^*]+\*\*|\*[^*]+\*)/g)
  return (
    <span key={key}>
      {parts.map((part, i) => {
        if (part.startsWith('**') && part.endsWith('**'))
          return <strong key={i}>{part.slice(2, -2)}</strong>
        if (part.startsWith('*') && part.endsWith('*'))
          return <em key={i}>{part.slice(1, -1)}</em>
        return part
      })}
    </span>
  )
}

/** Detects whether a line (after stripping **) is an ALL-CAPS section header.
 *  Must be: all uppercase letters/spaces only, no digits, no colons, min 4 chars. */
function isSectionHeader(raw: string): boolean {
  const text = raw.replace(/\*\*/g, '').trim()
  // Must be at least 4 chars, ALL CAPS letters and spaces only (no digits, no : | . etc.)
  return (
    text.length >= 4 &&
    /^[A-Z][A-Z\s&/]+$/.test(text) &&
    text === text.toUpperCase()
  )
}

/** Renders the full markdown resume text as beautiful HTML */
function MarkdownResume({ text }: { text: string }) {
  const lines = text.split('\n')
  const nodes: React.ReactNode[] = []
  let i = 0
  let bulletBuffer: string[] = []
  // Track whether we have passed the first section header.
  // Contact-line centering and name detection only apply in the header block.
  let headerDone = false

  const flushBullets = (key: string) => {
    if (bulletBuffer.length === 0) return
    nodes.push(
      <ul key={key} className="list-disc list-outside ml-5 space-y-0.5 mb-2">
        {bulletBuffer.map((b, idx) => (
          <li key={idx} className="text-gray-700 text-sm leading-snug">
            {renderInline(b, idx)}
          </li>
        ))}
      </ul>
    )
    bulletBuffer = []
  }

  while (i < lines.length) {
    const raw = lines[i]
    const trimmed = raw.trim()

    // blank line or horizontal rule (--- or ***)
    if (!trimmed || /^[-*_]{2,}$/.test(trimmed)) {
      flushBullets(`bl-${i}`)
      if (trimmed) nodes.push(<hr key={`hr-${i}`} className="border-gray-200 my-2" />)
      else nodes.push(<div key={`sp-${i}`} className="h-1" />)
      i++
      continue
    }

    // bullet point: lines starting with * or -
    const bulletMatch = trimmed.match(/^[*\-]\s+(.+)/)
    if (bulletMatch) {
      bulletBuffer.push(bulletMatch[1])
      i++
      continue
    } else {
      flushBullets(`bf-${i}`)
    }

    // Markdown h3 / h4 → sub-section headers
    if (trimmed.startsWith('### ') || trimmed.startsWith('#### ')) {
      headerDone = true
      const label = trimmed.replace(/^#{3,}\s*/, '')
      nodes.push(
        <h3 key={i} className="text-sm font-bold text-gray-700 mt-3 mb-0.5">
          {label}
        </h3>
      )
      i++; continue
    }

    // Markdown h2 — section headers (## EXPERIENCE, ## SKILLS, etc.)
    if (trimmed.startsWith('##') && !trimmed.startsWith('###')) {
      headerDone = true
      const label = trimmed.replace(/^#{2}\s*/, '')
      nodes.push(
        <h2 key={i} className="text-base font-bold text-indigo-700 uppercase tracking-wide border-b border-indigo-200 pb-0.5 mt-4 mb-1">
          {label}
        </h2>
      )
      i++; continue
    }

    // Markdown h1 — candidate name (# Name or #Name — with or without space)
    if (trimmed.startsWith('#') && !trimmed.startsWith('##')) {
      const name = trimmed.replace(/^#+\s*/, '').trim()
      // strip surrounding **bold** if present
      const cleanName = name.match(/^\*\*([^*]+)\*\*$/) ? name.slice(2, -2) : name
      nodes.push(
        <h1 key={i} className="text-2xl font-extrabold text-gray-900 text-center tracking-wide mb-1">
          {cleanName}
        </h1>
      )
      i++; continue
    }

    // ── Name detection (fallback for resumes that don't use # prefix) ───────
    // First non-empty content line = candidate name, whether bold-wrapped or ALL-CAPS plain
    if (!headerDone && nodes.filter(n => n !== null).length === 0) {
      const boldOnly = trimmed.match(/^\*\*([^*]+)\*\*$/)
      const plainName = boldOnly ? boldOnly[1] : trimmed
      nodes.push(
        <h1 key={i} className="text-2xl font-extrabold text-gray-900 text-center tracking-wide mb-1">
          {plainName}
        </h1>
      )
      i++; continue
    }

    // ALL-CAPS section header (wrapped in ** or not)
    if (isSectionHeader(trimmed)) {
      headerDone = true
      const label = trimmed.replace(/\*\*/g, '')
      nodes.push(
        <h2 key={i} className="text-sm font-extrabold text-indigo-700 uppercase tracking-widest border-b-2 border-indigo-200 pb-0.5 mt-5 mb-2">
          {label}
        </h2>
      )
      i++; continue
    }

    // Contact / sub-header line — only in the header block (before first section)
    if (!headerDone) {
      const isContactLine = trimmed.includes('|') || trimmed.match(/^(Email|Phone|Location|LinkedIn|GitHub|Website):/i)
      if (isContactLine) {
        nodes.push(
          <p key={i} className="text-center text-xs text-gray-500 mb-0.5">
            {renderInline(trimmed, i)}
          </p>
        )
        i++; continue
      }
    }

    // Job-title / role line inside experience: bold + pipe-separated, render left-aligned
    const isRoleLine = trimmed.includes('|') && (trimmed.includes('**') || /[A-Z]/.test(trimmed[0]))
    if (headerDone && isRoleLine) {
      nodes.push(
        <p key={i} className="text-sm font-semibold text-gray-800 mt-3 mb-0.5">
          {renderInline(trimmed, i)}
        </p>
      )
      i++; continue
    }

    // Default: normal paragraph line
    // Strip any stray leading # characters that weren't caught above
    const safeText = trimmed.replace(/^#+\s*/, '')
    nodes.push(
      <p key={i} className="text-sm text-gray-800 leading-snug mb-0.5">
        {renderInline(safeText, i)}
      </p>
    )
    i++
  }
  flushBullets('end')

  return <>{nodes}</>
}

function ResumePreview({ text }: { text: string }) {
  return (
    <div
      id="resume-print-area"
      className="bg-white border border-gray-200 rounded-xl p-8 shadow-inner print:shadow-none print:border-none print:rounded-none print:p-0"
    >
      <MarkdownResume text={text} />
    </div>
  )
}

// ---------------------------------------------------------------------------
// Download button with spinner while generating PDF
// ---------------------------------------------------------------------------
function DownloadButton({ name }: { name: string }) {
  const [busy, setBusy] = useState(false)

  const handleClick = async () => {
    setBusy(true)
    try {
      await downloadResumePdf(name)
    } finally {
      setBusy(false)
    }
  }

  return (
    <button
      onClick={handleClick}
      disabled={busy}
      className="flex items-center gap-2 px-4 py-2 bg-gray-800 text-white text-sm font-medium rounded-lg hover:bg-gray-900 transition-colors disabled:opacity-60"
    >
      {busy ? <Loader2 className="w-4 h-4 animate-spin" /> : <Download className="w-4 h-4" />}
      {busy ? 'Preparing…' : 'Download PDF'}
    </button>
  )
}

// ---------------------------------------------------------------------------
// Fresher Tab
// ---------------------------------------------------------------------------
function FresherTab() {
  const [form, setForm] = useState<Omit<FresherResumeRequest, 'skills' | 'projects'> & {
    technicalSkills: string
    softSkills: string
  }>({
    fullName: '', email: '', phone: '', degree: '', college: '',
    graduationYear: '', cgpa: '', internships: '', certifications: '',
    careerObjective: '', targetRole: '', technicalSkills: '', softSkills: '',
  })
  const [projects, setProjects] = useState<Project[]>([
    { id: '1', title: '', techStack: '', description: '' }
  ])
  const [resume, setResume] = useState('')
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')

  const set = (key: string) => (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) =>
    setForm(prev => ({ ...prev, [key]: e.target.value }))

  const addProject = () =>
    setProjects(prev => [...prev, { id: Date.now().toString(), title: '', techStack: '', description: '' }])

  const removeProject = (id: string) =>
    setProjects(prev => prev.filter(p => p.id !== id))

  const updateProject = (id: string, field: keyof Project, value: string) =>
    setProjects(prev => prev.map(p => p.id === id ? { ...p, [field]: value } : p))

  const handleGenerate = async () => {
    if (!form.fullName || !form.targetRole) {
      setError('Please fill in at least your full name and target role.')
      return
    }
    setError('')
    setLoading(true)
    try {
      const projectsText = projects
        .filter(p => p.title)
        .map(p => `${p.title} (${p.techStack}): ${p.description}`)
        .join('\n')

      const res = await generateFresherResume({
        fullName: form.fullName, email: form.email, phone: form.phone,
        degree: form.degree, college: form.college, graduationYear: form.graduationYear,
        cgpa: form.cgpa, targetRole: form.targetRole,
        skills: `Technical: ${form.technicalSkills}\nSoft Skills: ${form.softSkills}`,
        projects: projectsText,
        internships: form.internships, certifications: form.certifications,
        careerObjective: form.careerObjective,
      })
      setResume(res.resume)
    } catch (err: any) {
      if (err?.code !== 'SUBSCRIPTION_REQUIRED') {
        setError(err?.message || 'Failed to generate resume. Please try again.')
      }
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="space-y-6">
      {/* Form */}
      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        {[
          ['fullName', 'Full Name *', 'text'],
          ['email', 'Email', 'email'],
          ['phone', 'Phone', 'tel'],
          ['targetRole', 'Target Role *', 'text'],
          ['degree', 'Degree', 'text'],
          ['college', 'College / University', 'text'],
          ['graduationYear', 'Graduation Year', 'text'],
          ['cgpa', 'CGPA / Percentage', 'text'],
        ].map(([key, label, type]) => (
          <div key={key}>
            <label className="block text-xs font-semibold text-gray-700 mb-1">{label}</label>
            <input type={type} value={(form as any)[key]} onChange={set(key)}
              className="w-full px-3 py-2 border border-gray-200 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-indigo-400" />
          </div>
        ))}
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        <div>
          <label className="block text-xs font-semibold text-gray-700 mb-1">Technical Skills</label>
          <textarea value={form.technicalSkills} onChange={set('technicalSkills')} rows={2}
            placeholder="Python, React, SQL, AWS…"
            className="w-full px-3 py-2 border border-gray-200 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-indigo-400 resize-none" />
        </div>
        <div>
          <label className="block text-xs font-semibold text-gray-700 mb-1">Soft Skills</label>
          <textarea value={form.softSkills} onChange={set('softSkills')} rows={2}
            placeholder="Communication, Leadership…"
            className="w-full px-3 py-2 border border-gray-200 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-indigo-400 resize-none" />
        </div>
      </div>

      <div>
        <label className="block text-xs font-semibold text-gray-700 mb-1">Career Objective</label>
        <textarea value={form.careerObjective} onChange={set('careerObjective')} rows={2}
          placeholder="Brief objective statement…"
          className="w-full px-3 py-2 border border-gray-200 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-indigo-400 resize-none" />
      </div>

      {/* Projects */}
      <div>
        <div className="flex items-center justify-between mb-2">
          <label className="text-xs font-semibold text-gray-700">Projects</label>
          <button onClick={addProject} className="flex items-center gap-1 text-xs text-indigo-600 hover:text-indigo-700 font-medium">
            <Plus className="w-3.5 h-3.5" /> Add Project
          </button>
        </div>
        <div className="space-y-3">
          {projects.map((proj, i) => (
            <div key={proj.id} className="border border-gray-200 rounded-lg p-3 space-y-2 bg-gray-50/50">
              <div className="flex items-center justify-between">
                <span className="text-xs font-medium text-gray-500">Project {i + 1}</span>
                {projects.length > 1 && (
                  <button onClick={() => removeProject(proj.id)} className="text-red-400 hover:text-red-600">
                    <Trash2 className="w-3.5 h-3.5" />
                  </button>
                )}
              </div>
              <div className="grid grid-cols-2 gap-2">
                <input placeholder="Project title" value={proj.title}
                  onChange={e => updateProject(proj.id, 'title', e.target.value)}
                  className="px-2 py-1.5 border border-gray-200 rounded text-xs focus:outline-none focus:ring-1 focus:ring-indigo-400" />
                <input placeholder="Tech stack" value={proj.techStack}
                  onChange={e => updateProject(proj.id, 'techStack', e.target.value)}
                  className="px-2 py-1.5 border border-gray-200 rounded text-xs focus:outline-none focus:ring-1 focus:ring-indigo-400" />
              </div>
              <textarea placeholder="Description & impact…" value={proj.description} rows={2}
                onChange={e => updateProject(proj.id, 'description', e.target.value)}
                className="w-full px-2 py-1.5 border border-gray-200 rounded text-xs focus:outline-none focus:ring-1 focus:ring-indigo-400 resize-none" />
            </div>
          ))}
        </div>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        <div>
          <label className="block text-xs font-semibold text-gray-700 mb-1">Internships</label>
          <textarea value={form.internships} onChange={set('internships')} rows={2}
            placeholder="Company, role, duration…"
            className="w-full px-3 py-2 border border-gray-200 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-indigo-400 resize-none" />
        </div>
        <div>
          <label className="block text-xs font-semibold text-gray-700 mb-1">Certifications</label>
          <textarea value={form.certifications} onChange={set('certifications')} rows={2}
            placeholder="AWS, Google, Coursera…"
            className="w-full px-3 py-2 border border-gray-200 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-indigo-400 resize-none" />
        </div>
      </div>

      {error && (
        <div className="flex items-center gap-2 text-sm text-red-600 bg-red-50 border border-red-200 rounded-lg p-3">
          <AlertCircle className="w-4 h-4 flex-shrink-0" /> {error}
        </div>
      )}

      <button onClick={handleGenerate} disabled={loading}
        className="w-full flex items-center justify-center gap-2 bg-gradient-to-r from-indigo-600 to-purple-600 text-white font-bold py-3 rounded-xl hover:opacity-90 transition-opacity disabled:opacity-60">
        {loading ? <><Loader2 className="w-4 h-4 animate-spin" /> Generating…</> : <><FileText className="w-4 h-4" /> Generate Resume</>}
      </button>

      {/* Preview */}
      {resume && (
        <motion.div initial={{ opacity: 0, y: 20 }} animate={{ opacity: 1, y: 0 }} className="space-y-4">
          <div className="flex items-center justify-between">
            <AtsScoreBadge resumeText={resume} />
            <DownloadButton name={form.fullName} />
          </div>
          <ResumePreview text={resume} />
        </motion.div>
      )}
    </div>
  )
}

// ---------------------------------------------------------------------------
// Experienced Tab
// ---------------------------------------------------------------------------
function ExperiencedTab() {
  const [form, setForm] = useState<Omit<ExperiencedResumeRequest, 'workHistory' | 'keyAchievements'> & {
    keyAchievements: string
  }>({
    fullName: '', email: '', phone: '', totalExperience: 0,
    currentRole: '', currentCompany: '', currentCtc: '', expectedCtc: '',
    noticePeriod: '30 days', skills: '', education: '', certifications: '',
    targetRole: '', professionalSummary: '', keyAchievements: '',
  })
  const [experiences, setExperiences] = useState<WorkExperience[]>([
    { id: '1', company: '', role: '', duration: '', responsibilities: '' }
  ])
  const [resume, setResume] = useState('')
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')

  const set = (key: string) => (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) =>
    setForm(prev => ({ ...prev, [key]: e.target.value }))

  const addExperience = () =>
    setExperiences(prev => [...prev, { id: Date.now().toString(), company: '', role: '', duration: '', responsibilities: '' }])

  const removeExperience = (id: string) =>
    setExperiences(prev => prev.filter(e => e.id !== id))

  const updateExperience = (id: string, field: keyof WorkExperience, value: string) =>
    setExperiences(prev => prev.map(e => e.id === id ? { ...e, [field]: value } : e))

  const handleGenerate = async () => {
    if (!form.fullName || !form.targetRole) {
      setError('Please fill in at least your full name and target role.')
      return
    }
    setError('')
    setLoading(true)
    try {
      const workHistory = experiences
        .filter(e => e.company)
        .map(e => `${e.role} at ${e.company} (${e.duration}):\n${e.responsibilities}`)
        .join('\n\n')

      const res = await generateExperiencedResume({
        ...form,
        workHistory,
      })
      setResume(res.resume)
    } catch (err: any) {
      if (err?.code !== 'SUBSCRIPTION_REQUIRED') {
        setError(err?.message || 'Failed to generate resume. Please try again.')
      }
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="space-y-6">
      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        {[
          ['fullName', 'Full Name *'], ['email', 'Email'], ['phone', 'Phone'],
          ['targetRole', 'Target Role *'], ['currentRole', 'Current Role'],
          ['currentCompany', 'Current Company'], ['currentCtc', 'Current CTC'],
          ['expectedCtc', 'Expected CTC'], ['noticePeriod', 'Notice Period'],
        ].map(([key, label]) => (
          <div key={key}>
            <label className="block text-xs font-semibold text-gray-700 mb-1">{label}</label>
            <input value={(form as any)[key]} onChange={set(key)}
              className="w-full px-3 py-2 border border-gray-200 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-indigo-400" />
          </div>
        ))}
        <div>
          <label className="block text-xs font-semibold text-gray-700 mb-1">Total Experience (years)</label>
          <input type="number" value={form.totalExperience}
            onChange={e => setForm(prev => ({ ...prev, totalExperience: +e.target.value }))}
            className="w-full px-3 py-2 border border-gray-200 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-indigo-400" />
        </div>
      </div>

      <div>
        <label className="block text-xs font-semibold text-gray-700 mb-1">Skills</label>
        <textarea value={form.skills} onChange={set('skills')} rows={2}
          placeholder="React, Node.js, AWS, Docker…"
          className="w-full px-3 py-2 border border-gray-200 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-indigo-400 resize-none" />
      </div>

      {/* Work Experience */}
      <div>
        <div className="flex items-center justify-between mb-2">
          <label className="text-xs font-semibold text-gray-700">Work Experience</label>
          <button onClick={addExperience} className="flex items-center gap-1 text-xs text-indigo-600 hover:text-indigo-700 font-medium">
            <Plus className="w-3.5 h-3.5" /> Add Role
          </button>
        </div>
        <div className="space-y-3">
          {experiences.map((exp, i) => (
            <div key={exp.id} className="border border-gray-200 rounded-lg p-3 space-y-2 bg-gray-50/50">
              <div className="flex items-center justify-between">
                <span className="text-xs font-medium text-gray-500">Role {i + 1}</span>
                {experiences.length > 1 && (
                  <button onClick={() => removeExperience(exp.id)} className="text-red-400 hover:text-red-600">
                    <Trash2 className="w-3.5 h-3.5" />
                  </button>
                )}
              </div>
              <div className="grid grid-cols-3 gap-2">
                <input placeholder="Company" value={exp.company}
                  onChange={e => updateExperience(exp.id, 'company', e.target.value)}
                  className="px-2 py-1.5 border border-gray-200 rounded text-xs focus:outline-none focus:ring-1 focus:ring-indigo-400" />
                <input placeholder="Role / Title" value={exp.role}
                  onChange={e => updateExperience(exp.id, 'role', e.target.value)}
                  className="px-2 py-1.5 border border-gray-200 rounded text-xs focus:outline-none focus:ring-1 focus:ring-indigo-400" />
                <input placeholder="Duration (e.g. 2022–2024)" value={exp.duration}
                  onChange={e => updateExperience(exp.id, 'duration', e.target.value)}
                  className="px-2 py-1.5 border border-gray-200 rounded text-xs focus:outline-none focus:ring-1 focus:ring-indigo-400" />
              </div>
              <textarea placeholder="Key responsibilities and achievements…" value={exp.responsibilities} rows={3}
                onChange={e => updateExperience(exp.id, 'responsibilities', e.target.value)}
                className="w-full px-2 py-1.5 border border-gray-200 rounded text-xs focus:outline-none focus:ring-1 focus:ring-indigo-400 resize-none" />
            </div>
          ))}
        </div>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        <div>
          <label className="block text-xs font-semibold text-gray-700 mb-1">Education</label>
          <textarea value={form.education} onChange={set('education')} rows={2}
            placeholder="B.Tech Computer Science, IIT Delhi, 2018"
            className="w-full px-3 py-2 border border-gray-200 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-indigo-400 resize-none" />
        </div>
        <div>
          <label className="block text-xs font-semibold text-gray-700 mb-1">Certifications</label>
          <textarea value={form.certifications} onChange={set('certifications')} rows={2}
            placeholder="AWS Solutions Architect, PMP…"
            className="w-full px-3 py-2 border border-gray-200 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-indigo-400 resize-none" />
        </div>
      </div>

      <div>
        <label className="block text-xs font-semibold text-gray-700 mb-1">Key Achievements</label>
        <textarea value={form.keyAchievements} onChange={set('keyAchievements')} rows={3}
          placeholder="Led team of 8 to deliver ₹2Cr project on time. Reduced API latency by 40%…"
          className="w-full px-3 py-2 border border-gray-200 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-indigo-400 resize-none" />
      </div>

      <div>
        <label className="block text-xs font-semibold text-gray-700 mb-1">Professional Summary (optional)</label>
        <textarea value={form.professionalSummary} onChange={set('professionalSummary')} rows={2}
          placeholder="Seasoned engineer with 5+ years…"
          className="w-full px-3 py-2 border border-gray-200 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-indigo-400 resize-none" />
      </div>

      {error && (
        <div className="flex items-center gap-2 text-sm text-red-600 bg-red-50 border border-red-200 rounded-lg p-3">
          <AlertCircle className="w-4 h-4 flex-shrink-0" /> {error}
        </div>
      )}

      <button onClick={handleGenerate} disabled={loading}
        className="w-full flex items-center justify-center gap-2 bg-gradient-to-r from-indigo-600 to-purple-600 text-white font-bold py-3 rounded-xl hover:opacity-90 transition-opacity disabled:opacity-60">
        {loading ? <><Loader2 className="w-4 h-4 animate-spin" /> Generating…</> : <><FileText className="w-4 h-4" /> Generate Resume</>}
      </button>

      {resume && (
        <motion.div initial={{ opacity: 0, y: 20 }} animate={{ opacity: 1, y: 0 }} className="space-y-4">
          <div className="flex items-center justify-between">
            <AtsScoreBadge resumeText={resume} />
            <DownloadButton name={form.fullName} />
          </div>
          <ResumePreview text={resume} />
        </motion.div>
      )}
    </div>
  )
}

// ---------------------------------------------------------------------------
// Enhance Existing Resume Tab
// ---------------------------------------------------------------------------
function EnhanceTab() {
  const [file, setFile] = useState<File | null>(null)
  const [targetRole, setTargetRole] = useState('')
  const [resume, setResume] = useState('')
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')
  const [dragging, setDragging] = useState(false)
  const inputRef = React.useRef<HTMLInputElement>(null)

  const ACCEPTED = '.pdf,.doc,.docx,.txt'

  const handleFile = (f: File) => {
    const ext = f.name.split('.').pop()?.toLowerCase()
    if (!['pdf', 'doc', 'docx', 'txt'].includes(ext ?? '')) {
      setError('Only PDF, DOCX, DOC, and TXT files are supported.')
      return
    }
    if (f.size > 10 * 1024 * 1024) {
      setError('File size must be under 10 MB.')
      return
    }
    setError('')
    setFile(f)
  }

  const onDrop = (e: React.DragEvent) => {
    e.preventDefault()
    setDragging(false)
    const f = e.dataTransfer.files[0]
    if (f) handleFile(f)
  }

  const handleEnhance = async () => {
    if (!file) { setError('Please upload your resume file first.'); return }
    setError('')
    setLoading(true)
    try {
      const res = await enhanceUploadedResume(file, targetRole)
      setResume(res.resume)
    } catch (err: any) {
      if (err?.code !== 'SUBSCRIPTION_REQUIRED')
        setError(err?.message || 'Failed to enhance resume. Please try again.')
    } finally {
      setLoading(false)
    }
  }

  const candidateName = file?.name.replace(/\.[^.]+$/, '') ?? 'resume'

  return (
    <div className="space-y-6">
      {/* Upload area */}
      <div>
        <label className="block text-xs font-semibold text-gray-700 mb-3">Upload Your Existing Resume</label>
        <div
          onDragOver={e => { e.preventDefault(); setDragging(true) }}
          onDragLeave={() => setDragging(false)}
          onDrop={onDrop}
          onClick={() => inputRef.current?.click()}
          className={`relative cursor-pointer border-2 border-dashed rounded-2xl p-10 flex flex-col items-center justify-center gap-3 transition-all ${
            dragging ? 'border-indigo-500 bg-indigo-50' : file ? 'border-green-400 bg-green-50' : 'border-gray-300 bg-gray-50 hover:border-indigo-400 hover:bg-indigo-50/40'
          }`}
        >
          <input ref={inputRef} type="file" accept={ACCEPTED} className="hidden"
            onChange={e => e.target.files?.[0] && handleFile(e.target.files[0])} />

          {file ? (
            <>
              <CheckCircle className="w-10 h-10 text-green-500" />
              <div className="text-center">
                <p className="font-semibold text-green-700 text-sm">{file.name}</p>
                <p className="text-xs text-green-600">{(file.size / 1024).toFixed(0)} KB — click to change</p>
              </div>
            </>
          ) : (
            <>
              <Upload className="w-10 h-10 text-indigo-400" />
              <div className="text-center">
                <p className="font-semibold text-gray-700 text-sm">Drag & drop your resume here</p>
                <p className="text-xs text-gray-500 mt-1">or click to browse — PDF, DOCX, DOC, TXT · Max 10 MB</p>
              </div>
            </>
          )}
        </div>
      </div>

      {/* Target role */}
      <div>
        <label className="block text-xs font-semibold text-gray-700 mb-1">Target Role <span className="text-gray-400 font-normal">(optional — helps AI tailor ATS keywords)</span></label>
        <input
          type="text"
          value={targetRole}
          onChange={e => setTargetRole(e.target.value)}
          placeholder="e.g. Software Engineer, Data Analyst, Product Manager…"
          className="w-full px-3 py-2 border border-gray-200 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-indigo-400"
        />
      </div>

      {/* How it works */}
      <div className="bg-gradient-to-r from-indigo-50 to-purple-50 border border-indigo-100 rounded-xl p-4">
        <div className="flex items-start gap-3">
          <Sparkles className="w-5 h-5 text-indigo-600 flex-shrink-0 mt-0.5" />
          <div className="text-xs text-indigo-800 space-y-1">
            <p className="font-bold text-sm">What the AI does to your resume:</p>
            <p>✦ Rewrites every bullet with strong action verbs + quantified metrics</p>
            <p>✦ Injects 10-15 ATS keywords recruiters search for</p>
            <p>✦ Adds missing sections (Key Achievements, Key Highlights)</p>
            <p>✦ Restructures skills into clean categories</p>
            <p>✦ Elevates generic descriptions into recruiter-ready language</p>
          </div>
        </div>
      </div>

      {error && (
        <div className="flex items-center gap-2 text-sm text-red-600 bg-red-50 border border-red-200 rounded-lg p-3">
          <AlertCircle className="w-4 h-4 flex-shrink-0" /> {error}
        </div>
      )}

      <button onClick={handleEnhance} disabled={loading || !file}
        className="w-full flex items-center justify-center gap-2 bg-gradient-to-r from-indigo-600 to-purple-600 text-white font-bold py-3 rounded-xl hover:opacity-90 transition-opacity disabled:opacity-50">
        {loading
          ? <><Loader2 className="w-4 h-4 animate-spin" /> Enhancing your resume…</>
          : <><Sparkles className="w-4 h-4" /> Enhance to ATS 10/10</>}
      </button>

      {resume && (
        <motion.div initial={{ opacity: 0, y: 20 }} animate={{ opacity: 1, y: 0 }} className="space-y-4">
          <div className="flex items-center justify-between">
            <AtsScoreBadge resumeText={resume} />
            <DownloadButton name={candidateName} />
          </div>
          <ResumePreview text={resume} />
        </motion.div>
      )}
    </div>
  )
}

// ---------------------------------------------------------------------------
// Main Page
// ---------------------------------------------------------------------------
const ResumeBuilder: React.FC = () => {
  const [activeTab, setActiveTab] = useState<Tab>('fresher')

  return (
    <div className="min-h-screen bg-gray-50 p-6">
      <div className="max-w-4xl mx-auto space-y-6">
        {/* Header */}
        <div className="">
          <div className="flex items-center gap-3">
            <div className="w-10 h-10 bg-indigo-100 rounded-xl flex items-center justify-center">
              <FileText className="w-5 h-5 text-indigo-600" />
            </div>
            <div>
              <h1 className="text-2xl font-bold text-gray-900">AI Resume Builder</h1>
              <p className="text-sm text-gray-500">Powered by Claude AI — ATS-optimised resumes in seconds</p>
            </div>
          </div>
        </div>

        {/* Tabs */}
        <div className=" bg-white rounded-2xl shadow-sm border border-gray-100 overflow-hidden">
          <div className="flex border-b border-gray-100">
            {([
              ['fresher', 'Fresher / Entry Level', GraduationCap],
              ['experienced', 'Experienced Professional', Briefcase],
              ['enhance', 'Enhance My Resume', Upload],
            ] as const).map(
              ([tab, label, Icon]) => (
                <button
                  key={tab}
                  onClick={() => setActiveTab(tab)}
                  className={`flex-1 flex items-center justify-center gap-2 py-4 text-sm font-semibold transition-colors ${
                    activeTab === tab
                      ? 'border-b-2 border-indigo-600 text-indigo-700 bg-indigo-50/50'
                      : 'text-gray-500 hover:text-gray-700 hover:bg-gray-50'
                  }`}
                >
                  <Icon className="w-4 h-4" />
                  {label}
                </button>
              )
            )}
          </div>

          <div className="p-6">
            <AnimatePresence mode="wait">
              <motion.div
                key={activeTab}
                initial={{ opacity: 0, x: 20 }}
                animate={{ opacity: 1, x: 0 }}
                exit={{ opacity: 0, x: -20 }}
                transition={{ duration: 0.2 }}
              >
                {activeTab === 'fresher' ? <FresherTab />
                  : activeTab === 'experienced' ? <ExperiencedTab />
                  : <EnhanceTab />}
              </motion.div>
            </AnimatePresence>
          </div>
        </div>

        {/* Tips */}
        <div className=" bg-indigo-50 border border-indigo-100 rounded-xl p-4">
          <div className="flex items-start gap-3">
            <User className="w-5 h-5 text-indigo-600 flex-shrink-0 mt-0.5" />
            <div className="text-sm text-indigo-800">
              <p className="font-semibold mb-1">Pro Tips for a Great Resume</p>
              <ul className="space-y-1 text-xs list-disc list-inside text-indigo-700">
                <li>Add specific numbers — "Reduced load time by 40%" beats "improved performance"</li>
                <li>Tailor the target role to match the exact job description keywords</li>
                <li>Use the Download PDF button to print — use Chrome for best results</li>
                <li>Get a second opinion with the AI Review feature in the AI Assistant tab</li>
              </ul>
            </div>
          </div>
        </div>
      </div>
    </div>
  )
}

export default ResumeBuilder
