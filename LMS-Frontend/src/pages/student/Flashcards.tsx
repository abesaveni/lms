import { useState } from 'react'
import { CreditCard, Loader2, RotateCcw, ChevronLeft, ChevronRight, Check, RefreshCw } from 'lucide-react'
import { generateFlashcardsNew } from '../../services/aiApi'
import { PaidGuard } from '../../components/subscription/PaidGuard'
import { AIMarkdown } from '../../components/ui/AIMarkdown'
import { stripCodeFences } from '../../utils/aiUtils'

const TOPICS = ['JavaScript', 'Python', 'React', 'Data Structures', 'SQL', 'System Design', 'Machine Learning', 'Computer Networks', 'Cloud Computing', 'TypeScript']
const COUNTS = [5, 10, 15, 20]

interface Flashcard { front: string; back: string }

export default function Flashcards() {
  const [topic, setTopic] = useState(TOPICS[0])
  const [count, setCount] = useState(10)
  const [loading, setLoading] = useState(false)
  const [cards, setCards] = useState<Flashcard[]>([])
  const [current, setCurrent] = useState(0)
  const [flipped, setFlipped] = useState(false)
  const [known, setKnown] = useState<Set<number>>(new Set())
  const [review, setReview] = useState<Set<number>>(new Set())
  const [error, setError] = useState('')
  const [done, setDone] = useState(false)
  const [rawFallback, setRawFallback] = useState('')

  const generate = async () => {
    setLoading(true); setError(''); setRawFallback('')
    try {
      const res = await generateFlashcardsNew({ topic, count })
      let arr: Flashcard[] = []
      const dataSource = res.flashcards || res.rawResponse
      if (dataSource) {
        try {
          const cleaned = typeof dataSource === 'string' ? stripCodeFences(dataSource) : dataSource
          const raw = typeof cleaned === 'string' ? JSON.parse(cleaned) : cleaned
          arr = Array.isArray(raw) ? raw : (raw?.root ? raw.root : [])
        } catch { arr = [] }
      }
      if (arr.length === 0) { setRawFallback(res.rawResponse as string || ''); return }
      setCards(arr); setCurrent(0); setFlipped(false); setKnown(new Set()); setReview(new Set()); setDone(false)
    } catch (e: any) { setError(e.message) }
    finally { setLoading(false) }
  }

  const markKnown = () => {
    setKnown(k => new Set([...k, current]))
    setReview(r => { const n = new Set(r); n.delete(current); return n })
    advance()
  }

  const markReview = () => {
    setReview(r => new Set([...r, current]))
    setKnown(k => { const n = new Set(k); n.delete(current); return n })
    advance()
  }

  const advance = () => {
    if (current + 1 >= cards.length) { setDone(true); return }
    setCurrent(c => c + 1); setFlipped(false)
  }

  const reset = () => { setCards([]); setCurrent(0); setFlipped(false); setKnown(new Set()); setReview(new Set()); setDone(false) }

  const restartReview = () => {
    const reviewCards = cards.filter((_, i) => review.has(i))
    setCards(reviewCards); setCurrent(0); setFlipped(false); setKnown(new Set()); setReview(new Set()); setDone(false)
  }

  const card = cards[current]

  return (
    <PaidGuard featureName="Smart Flashcards">
      <div className="p-6 max-w-2xl mx-auto">
        <div className="flex items-center gap-3 mb-8">
          <div className="w-10 h-10 bg-teal-100 rounded-xl flex items-center justify-center">
            <CreditCard className="w-5 h-5 text-teal-600" />
          </div>
          <div>
            <h1 className="text-2xl font-bold text-gray-900">Smart Flashcards</h1>
            <p className="text-sm text-gray-500">Interactive flip cards with difficulty tracking</p>
          </div>
        </div>

        {cards.length === 0 && !done && (
          <div className="bg-white rounded-2xl border border-gray-100 p-6 shadow-sm space-y-5">
            <div>
              <label className="block text-sm font-semibold text-gray-700 mb-2">Topic</label>
              <select value={topic} onChange={e => setTopic(e.target.value)} className="w-full border border-gray-200 rounded-xl px-4 py-2.5 text-sm focus:outline-none focus:ring-2 focus:ring-teal-500">
                {TOPICS.map(t => <option key={t}>{t}</option>)}
              </select>
            </div>
            <div>
              <label className="block text-sm font-semibold text-gray-700 mb-2">Number of Cards</label>
              <div className="flex gap-3">
                {COUNTS.map(c => (
                  <button key={c} type="button" onClick={() => setCount(c)} className={`flex-1 py-2.5 rounded-xl text-sm font-medium border transition-colors ${count === c ? 'bg-teal-600 text-white border-teal-600' : 'border-gray-200 text-gray-600 hover:border-teal-300'}`}>{c}</button>
                ))}
              </div>
            </div>
            {error && <div className="bg-red-50 text-red-700 rounded-xl p-4 text-sm">{error}</div>}
            {rawFallback && (
              <div className="bg-teal-50 border border-teal-100 rounded-xl p-4">
                <p className="text-xs font-bold text-teal-700 uppercase mb-2">Flashcard Content</p>
                <AIMarkdown text={rawFallback} />
              </div>
            )}
            <button onClick={generate} disabled={loading} className="w-full flex items-center justify-center gap-2 bg-teal-600 hover:bg-teal-700 text-white font-bold py-3 rounded-xl transition-colors disabled:opacity-60">
              {loading ? <><Loader2 className="w-4 h-4 animate-spin" />Generating Flashcards...</> : 'Generate Flashcards'}
            </button>
          </div>
        )}

        {cards.length > 0 && !done && card && (
          <div className="space-y-5">
            {/* Progress */}
            <div className="bg-white rounded-2xl border border-gray-100 p-4 shadow-sm">
              <div className="flex items-center justify-between mb-2">
                <span className="text-sm font-semibold text-gray-700">Card {current + 1} of {cards.length}</span>
                <div className="flex gap-3 text-xs">
                  <span className="text-green-600 font-bold">✓ {known.size} known</span>
                  <span className="text-amber-600 font-bold">↻ {review.size} review</span>
                </div>
              </div>
              <div className="w-full bg-gray-100 rounded-full h-2">
                <div className="bg-teal-500 h-2 rounded-full transition-all" style={{ width: `${((known.size + review.size) / cards.length) * 100}%` }} />
              </div>
            </div>

            {/* Flip Card */}
            <div className="perspective-1000" style={{ perspective: '1000px' }}>
              <div
                onClick={() => setFlipped(f => !f)}
                className="relative cursor-pointer select-none"
                style={{ transformStyle: 'preserve-3d', transition: 'transform 0.5s', transform: flipped ? 'rotateY(180deg)' : 'rotateY(0deg)', minHeight: '220px' }}
              >
                {/* Front */}
                <div className="absolute inset-0 bg-white rounded-2xl border border-gray-200 shadow-sm p-8 flex flex-col items-center justify-center text-center" style={{ backfaceVisibility: 'hidden' }}>
                  <p className="text-xs font-bold text-teal-600 uppercase mb-4">Question — click to reveal</p>
                  <p className="text-lg font-semibold text-gray-900 leading-relaxed">{card.front}</p>
                </div>
                {/* Back */}
                <div className="absolute inset-0 bg-teal-600 rounded-2xl shadow-sm p-8 flex flex-col items-center justify-center text-center" style={{ backfaceVisibility: 'hidden', transform: 'rotateY(180deg)' }}>
                  <p className="text-xs font-bold text-teal-100 uppercase mb-4">Answer</p>
                  <p className="text-base text-white leading-relaxed">{card.back}</p>
                </div>
              </div>
            </div>

            <p className="text-center text-xs text-gray-400">Click card to flip</p>

            {/* Action Buttons */}
            <div className="grid grid-cols-2 gap-3">
              <button onClick={markReview} className="flex items-center justify-center gap-2 border-2 border-amber-300 text-amber-600 font-bold py-3 rounded-xl hover:bg-amber-50 transition-colors">
                <RefreshCw className="w-4 h-4" />Review Again
              </button>
              <button onClick={markKnown} className="flex items-center justify-center gap-2 bg-teal-600 text-white font-bold py-3 rounded-xl hover:bg-teal-700 transition-colors">
                <Check className="w-4 h-4" />Got It!
              </button>
            </div>

            {/* Navigation */}
            <div className="flex items-center justify-between">
              <button onClick={() => { if (current > 0) { setCurrent(c => c - 1); setFlipped(false) } }} disabled={current === 0} className="flex items-center gap-1 text-sm text-gray-500 hover:text-gray-700 disabled:opacity-30 transition-colors">
                <ChevronLeft className="w-4 h-4" />Prev
              </button>
              <button onClick={advance} className="flex items-center gap-1 text-sm text-gray-500 hover:text-gray-700 transition-colors">
                Skip<ChevronRight className="w-4 h-4" />
              </button>
            </div>
          </div>
        )}

        {done && (
          <div className="bg-white rounded-2xl border border-gray-100 p-8 shadow-sm text-center space-y-5">
            <CreditCard className="w-12 h-12 text-teal-500 mx-auto" />
            <div>
              <p className="text-2xl font-black text-gray-900">Session Complete!</p>
              <p className="text-gray-500 mt-2">You went through all {cards.length} cards</p>
            </div>
            <div className="grid grid-cols-2 gap-4 max-w-xs mx-auto">
              <div className="bg-green-50 rounded-xl p-3 text-center">
                <p className="text-2xl font-black text-green-600">{known.size}</p>
                <p className="text-xs text-green-600 font-semibold">Known</p>
              </div>
              <div className="bg-amber-50 rounded-xl p-3 text-center">
                <p className="text-2xl font-black text-amber-600">{review.size}</p>
                <p className="text-xs text-amber-600 font-semibold">For Review</p>
              </div>
            </div>
            <div className="flex gap-3 justify-center">
              {review.size > 0 && (
                <button onClick={restartReview} className="flex items-center gap-2 bg-amber-500 hover:bg-amber-600 text-white font-bold px-5 py-3 rounded-xl transition-colors">
                  <RefreshCw className="w-4 h-4" />Review {review.size} cards
                </button>
              )}
              <button onClick={reset} className="flex items-center gap-2 border border-gray-200 text-gray-600 hover:bg-gray-50 font-bold px-5 py-3 rounded-xl transition-colors">
                <RotateCcw className="w-4 h-4" />New Set
              </button>
            </div>
          </div>
        )}
      </div>
    </PaidGuard>
  )
}
