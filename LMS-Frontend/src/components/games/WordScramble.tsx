import { useState, useEffect, useRef } from 'react'
import { Shuffle, Lightbulb, CheckCircle } from 'lucide-react'
import Button from '../ui/Button'

interface WordScrambleContent {
  scrambled: string
  hint: string
  category: string
}

interface Props {
  contentJson: string
  onSubmit: (answerJson: string, timeTaken: number) => void
  disabled?: boolean
}

export default function WordScramble({ contentJson, onSubmit, disabled }: Props) {
  const content: WordScrambleContent = JSON.parse(contentJson)
  const letters = content.scrambled.toUpperCase().split('')

  const [tiles, setTiles] = useState<{ id: number; letter: string; placed: boolean }[]>(
    () => letters.map((l, i) => ({ id: i, letter: l, placed: false }))
  )
  const [answer, setAnswer] = useState<{ id: number; letter: string }[]>([])
  const [showHint, setShowHint] = useState(false)
  const startRef = useRef(Date.now())

  // Shuffle helper
  const shuffle = () => {
    setTiles(prev => {
      const available = prev.filter(t => !t.placed)
      const placed = prev.filter(t => t.placed)
      const shuffled = [...available].sort(() => Math.random() - 0.5)
      return [...shuffled, ...placed].map(t => ({ ...t }))
    })
    setAnswer([])
    setTiles(letters.map((l, i) => ({ id: i, letter: l, placed: false })))
  }

  const handleTileClick = (tile: { id: number; letter: string; placed: boolean }) => {
    if (disabled || tile.placed) return
    setAnswer(prev => [...prev, { id: tile.id, letter: tile.letter }])
    setTiles(prev => prev.map(t => t.id === tile.id ? { ...t, placed: true } : t))
  }

  const handleAnswerClick = (idx: number) => {
    if (disabled) return
    const removed = answer[idx]
    setAnswer(prev => prev.filter((_, i) => i !== idx))
    setTiles(prev => prev.map(t => t.id === removed.id ? { ...t, placed: false } : t))
  }

  const handleSubmit = () => {
    if (answer.length === 0) return
    const word = answer.map(a => a.letter).join('')
    const timeTaken = Math.round((Date.now() - startRef.current) / 1000)
    onSubmit(JSON.stringify({ word }), timeTaken)
  }

  useEffect(() => {
    // Auto-shuffle on mount
    setTiles(prev => [...prev].sort(() => Math.random() - 0.5))
  }, [])

  return (
    <div className="space-y-6">
      {/* Category badge */}
      <div className="flex items-center justify-between">
        <span className="text-xs font-semibold uppercase tracking-widest text-indigo-600 bg-indigo-50 px-3 py-1 rounded-full">
          {content.category}
        </span>
        <button
          onClick={() => setShowHint(!showHint)}
          className="flex items-center gap-1.5 text-sm text-amber-600 hover:text-amber-700 font-medium"
        >
          <Lightbulb className="w-4 h-4" />
          {showHint ? 'Hide hint' : 'Show hint'}
        </button>
      </div>

      {showHint && (
        <div className="p-3 bg-amber-50 border border-amber-200 rounded-lg text-sm text-amber-800 flex items-start gap-2">
          <Lightbulb className="w-4 h-4 shrink-0 mt-0.5" />
          {content.hint}
        </div>
      )}

      {/* Answer row */}
      <div>
        <p className="text-xs text-gray-500 font-medium uppercase tracking-wide mb-2">Your answer</p>
        <div className="min-h-[56px] flex flex-wrap gap-2 p-3 bg-gray-50 border-2 border-dashed border-gray-200 rounded-xl">
          {answer.length === 0 && (
            <span className="text-gray-400 text-sm self-center">Click letters below to build your word…</span>
          )}
          {answer.map((a, idx) => (
            <button
              key={idx}
              onClick={() => handleAnswerClick(idx)}
              disabled={disabled}
              className="w-10 h-10 flex items-center justify-center bg-indigo-600 text-white font-bold rounded-lg text-lg shadow hover:bg-indigo-700 transition-colors"
            >
              {a.letter}
            </button>
          ))}
        </div>
      </div>

      {/* Tile pool */}
      <div>
        <p className="text-xs text-gray-500 font-medium uppercase tracking-wide mb-2">Available letters</p>
        <div className="flex flex-wrap gap-2">
          {tiles.map(tile => (
            <button
              key={tile.id}
              onClick={() => handleTileClick(tile)}
              disabled={disabled || tile.placed}
              className={`w-10 h-10 flex items-center justify-center font-bold rounded-lg text-lg border-2 transition-all
                ${tile.placed
                  ? 'bg-gray-100 border-gray-200 text-gray-300 cursor-not-allowed'
                  : 'bg-white border-indigo-300 text-indigo-700 hover:bg-indigo-50 hover:border-indigo-500 shadow-sm cursor-pointer'
                }`}
            >
              {tile.letter}
            </button>
          ))}
        </div>
      </div>

      <div className="flex gap-3">
        <button
          onClick={shuffle}
          disabled={disabled}
          className="flex items-center gap-1.5 px-4 py-2 text-sm text-gray-600 border border-gray-300 rounded-lg hover:bg-gray-50 transition-colors"
        >
          <Shuffle className="w-4 h-4" />
          Shuffle
        </button>
        <Button
          onClick={handleSubmit}
          disabled={disabled || answer.length === 0}
          className="flex-1"
        >
          <CheckCircle className="mr-2 w-4 h-4" />
          Submit Answer
        </Button>
      </div>
    </div>
  )
}
