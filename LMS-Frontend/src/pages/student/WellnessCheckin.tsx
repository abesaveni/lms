import { useState } from 'react'
import { Heart, Loader2 } from 'lucide-react'
import { submitWellnessCheckin } from '../../services/aiApi'
import { PaidGuard } from '../../components/subscription/PaidGuard'
import { AIMarkdown } from '../../components/ui/AIMarkdown'
import { stripCodeFences } from '../../utils/aiUtils'

const ENERGY_OPTIONS = [
  { value: 1, emoji: '😴', label: 'Exhausted' },
  { value: 2, emoji: '😔', label: 'Low' },
  { value: 3, emoji: '😐', label: 'Okay' },
  { value: 4, emoji: '😊', label: 'Good' },
  { value: 5, emoji: '🚀', label: 'Energised' },
]

const STRESS_OPTIONS = [
  { value: 1, emoji: '😌', label: 'Very Low' },
  { value: 2, emoji: '🙂', label: 'Low' },
  { value: 3, emoji: '😤', label: 'Moderate' },
  { value: 4, emoji: '😰', label: 'High' },
  { value: 5, emoji: '🤯', label: 'Extreme' },
]

interface WellnessResult {
  wellnessScore?: number
  summary?: string
  tips?: string[]
  affirmation?: string
  // legacy fallback fields
  tip?: string
  motivationalMessage?: string
  rawResponse?: string
}

export default function WellnessCheckin() {
  const [energy, setEnergy] = useState(3)
  const [stress, setStress] = useState(3)
  const [mood, setMood] = useState('')
  const [challenges, setChallenges] = useState('')
  const [loading, setLoading] = useState(false)
  const [result, setResult] = useState<WellnessResult | null>(null)
  const [error, setError] = useState('')

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setLoading(true); setError(''); setResult(null)
    try {
      const res = await submitWellnessCheckin({ energyLevel: energy, stressLevel: stress, mood: mood || undefined, challenges: challenges || undefined })
      const dataSource = res.data || res.rawResponse
      if (dataSource) {
        try {
          const cleaned = typeof dataSource === 'string' ? stripCodeFences(dataSource) : dataSource
          const raw = typeof cleaned === 'string' ? JSON.parse(cleaned) : cleaned
          const parsed = raw?.root ? raw.root : raw
          // normalize tip/motivationalMessage → tips/affirmation
          if (parsed.tip && !parsed.tips) parsed.tips = [parsed.tip]
          if (parsed.motivationalMessage && !parsed.affirmation) parsed.affirmation = parsed.motivationalMessage
          setResult(parsed)
        } catch { setResult({ rawResponse: res.rawResponse as string }) }
      } else {
        setResult({ rawResponse: res.rawResponse as string })
      }
    } catch (e: any) { setError(e.message) }
    finally { setLoading(false) }
  }

  const scoreColor = (n: number) => n >= 70 ? 'text-green-600' : n >= 45 ? 'text-amber-500' : 'text-red-500'
  const scoreRing = (n: number) => n >= 70 ? 'stroke-green-500' : n >= 45 ? 'stroke-amber-400' : 'stroke-red-400'
  const circumference = 2 * Math.PI * 40
  const dashOffset = (score: number) => circumference - (score / 100) * circumference

  return (
    <PaidGuard featureName="Wellness Check-in">
      <div className="p-6 max-w-2xl mx-auto">
        <div className="flex items-center gap-3 mb-8">
          <div className="w-10 h-10 bg-red-100 rounded-xl flex items-center justify-center">
            <Heart className="w-5 h-5 text-red-500" />
          </div>
          <div>
            <h1 className="text-2xl font-bold text-gray-900">Wellness Check-in</h1>
            <p className="text-sm text-gray-500">Track your energy, stress & get personalised tips</p>
          </div>
        </div>

        <form onSubmit={handleSubmit} className="bg-white rounded-2xl border border-gray-100 p-6 shadow-sm space-y-6 mb-6">
          {/* Energy */}
          <div>
            <label className="block text-sm font-semibold text-gray-700 mb-3">How's your energy today?</label>
            <div className="flex gap-2">
              {ENERGY_OPTIONS.map(opt => (
                <button key={opt.value} type="button" onClick={() => setEnergy(opt.value)} className={`flex-1 flex flex-col items-center gap-1 py-3 rounded-xl border-2 transition-all ${energy === opt.value ? 'border-red-400 bg-red-50 scale-105' : 'border-gray-200 hover:border-red-200'}`}>
                  <span className="text-2xl">{opt.emoji}</span>
                  <span className="text-xs font-medium text-gray-600">{opt.label}</span>
                </button>
              ))}
            </div>
          </div>

          {/* Stress */}
          <div>
            <label className="block text-sm font-semibold text-gray-700 mb-3">What's your stress level?</label>
            <div className="flex gap-2">
              {STRESS_OPTIONS.map(opt => (
                <button key={opt.value} type="button" onClick={() => setStress(opt.value)} className={`flex-1 flex flex-col items-center gap-1 py-3 rounded-xl border-2 transition-all ${stress === opt.value ? 'border-red-400 bg-red-50 scale-105' : 'border-gray-200 hover:border-red-200'}`}>
                  <span className="text-2xl">{opt.emoji}</span>
                  <span className="text-xs font-medium text-gray-600">{opt.label}</span>
                </button>
              ))}
            </div>
          </div>

          {/* Mood */}
          <div>
            <label className="block text-sm font-semibold text-gray-700 mb-2">How are you feeling? <span className="text-gray-400 font-normal">(optional)</span></label>
            <input value={mood} onChange={e => setMood(e.target.value)} placeholder="e.g. Anxious about exams, motivated, overwhelmed..." className="w-full border border-gray-200 rounded-xl px-4 py-2.5 text-sm focus:outline-none focus:ring-2 focus:ring-red-400" />
          </div>

          {/* Challenges */}
          <div>
            <label className="block text-sm font-semibold text-gray-700 mb-2">Any challenges you're facing? <span className="text-gray-400 font-normal">(optional)</span></label>
            <textarea value={challenges} onChange={e => setChallenges(e.target.value)} placeholder="e.g. Struggling to focus, not sleeping well, behind on assignments..." className="w-full border border-gray-200 rounded-xl px-4 py-3 text-sm min-h-[80px] resize-none focus:outline-none focus:ring-2 focus:ring-red-400" />
          </div>

          <button type="submit" disabled={loading} className="w-full flex items-center justify-center gap-2 bg-red-500 hover:bg-red-600 text-white font-bold py-3 rounded-xl transition-colors disabled:opacity-60">
            {loading ? <><Loader2 className="w-4 h-4 animate-spin" />Analysing...</> : 'Get My Wellness Report'}
          </button>
        </form>

        {error && <div className="bg-red-50 text-red-700 rounded-xl p-4 text-sm mb-6">{error}</div>}

        {result && (
          <div className="space-y-4">
            {/* Score Circle */}
            {result.wellnessScore !== undefined && (
              <div className="bg-white rounded-2xl border border-gray-100 p-6 shadow-sm flex items-center gap-6">
                <div className="relative flex-shrink-0">
                  <svg width="96" height="96" viewBox="0 0 96 96">
                    <circle cx="48" cy="48" r="40" fill="none" stroke="#f3f4f6" strokeWidth="8" />
                    <circle cx="48" cy="48" r="40" fill="none" strokeWidth="8" strokeLinecap="round"
                      className={scoreRing(result.wellnessScore)}
                      strokeDasharray={circumference}
                      strokeDashoffset={dashOffset(result.wellnessScore)}
                      style={{ transform: 'rotate(-90deg)', transformOrigin: '50% 50%', transition: 'stroke-dashoffset 0.8s ease' }}
                    />
                  </svg>
                  <div className="absolute inset-0 flex flex-col items-center justify-center">
                    <span className={`text-2xl font-black ${scoreColor(result.wellnessScore)}`}>{result.wellnessScore}</span>
                    <span className="text-xs text-gray-400">/100</span>
                  </div>
                </div>
                <div>
                  <p className="font-bold text-gray-900">Wellness Score</p>
                  {result.summary && <p className="text-sm text-gray-500 mt-1">{result.summary}</p>}
                </div>
              </div>
            )}

            {/* Affirmation */}
            {result.affirmation && (
              <div className="bg-gradient-to-r from-red-50 to-pink-50 border border-red-100 rounded-2xl p-5 text-center">
                <p className="text-lg font-semibold text-red-700 italic">"{result.affirmation}"</p>
              </div>
            )}

            {/* Tips */}
            {result.tips?.length ? (
              <div className="bg-white rounded-2xl border border-gray-100 p-5 shadow-sm">
                <p className="text-xs font-bold text-gray-500 uppercase mb-3">Personalised Wellness Tips</p>
                <ul className="space-y-3">
                  {result.tips.map((tip, i) => (
                    <li key={i} className="flex items-start gap-3 text-sm text-gray-700">
                      <span className="w-6 h-6 bg-red-100 text-red-600 rounded-full flex items-center justify-center text-xs font-bold flex-shrink-0">{i + 1}</span>
                      {tip}
                    </li>
                  ))}
                </ul>
              </div>
            ) : null}

            {/* Raw fallback */}
            {result.rawResponse && !result.wellnessScore && (
              <div className="bg-white rounded-2xl border border-gray-100 p-5 shadow-sm">
                <AIMarkdown text={result.rawResponse} />
              </div>
            )}
          </div>
        )}
      </div>
    </PaidGuard>
  )
}
