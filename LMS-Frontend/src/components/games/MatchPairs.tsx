import { useState, useRef } from 'react'
import { CheckCircle } from 'lucide-react'
import Button from '../ui/Button'

interface Pair { id: string; left: string }
interface Definition { id: string; right: string }
interface MatchContent {
  pairs: Pair[]
  definitions: Definition[]
}

interface Props {
  contentJson: string
  onSubmit: (answerJson: string, timeTaken: number) => void
  disabled?: boolean
}

export default function MatchPairs({ contentJson, onSubmit, disabled }: Props) {
  const content: MatchContent = JSON.parse(contentJson)

  // Shuffle definitions on mount
  const [defs] = useState(() => [...content.definitions].sort(() => Math.random() - 0.5))
  const [selectedLeft, setSelectedLeft] = useState<string | null>(null)
  const [matches, setMatches] = useState<Record<string, string>>({}) // leftId -> defId
  const startRef = useRef(Date.now())

  const handleLeftClick = (id: string) => {
    if (disabled) return
    setSelectedLeft(prev => (prev === id ? null : id))
  }

  const handleRightClick = (defId: string) => {
    if (disabled || !selectedLeft) return
    setMatches(prev => {
      const updated = { ...prev }
      // Unlink any existing connection to this def
      Object.keys(updated).forEach(k => { if (updated[k] === defId) delete updated[k] })
      updated[selectedLeft] = defId
      return updated
    })
    setSelectedLeft(null)
  }

  const removeMatch = (leftId: string) => {
    if (disabled) return
    setMatches(prev => { const n = { ...prev }; delete n[leftId]; return n })
  }

  const handleSubmit = () => {
    const timeTaken = Math.round((Date.now() - startRef.current) / 1000)
    onSubmit(JSON.stringify({ matches }), timeTaken)
  }

  const getDefText = (defId: string) =>
    defs.find(d => d.id === defId)?.right ?? ''

  const getDefColor = (defId: string) => {
    const paired = Object.values(matches).includes(defId)
    if (paired) return 'border-emerald-400 bg-emerald-50 text-emerald-800'
    if (!disabled && selectedLeft) return 'border-blue-300 bg-blue-50 text-blue-900 cursor-pointer hover:border-blue-500'
    return 'border-gray-200 bg-white text-gray-600 cursor-default'
  }

  const allMatched = content.pairs.every(p => matches[p.id])

  return (
    <div className="space-y-5">
      <p className="text-sm text-gray-500">
        Click a concept on the left, then click its matching definition on the right.
        Click a matched pair to remove the connection.
      </p>

      <div className="grid grid-cols-2 gap-4">
        {/* Left column — concepts */}
        <div className="space-y-2">
          <p className="text-xs font-semibold uppercase tracking-wide text-gray-400 mb-3">Concepts</p>
          {content.pairs.map(p => {
            const isMatched = !!matches[p.id]
            const isActive = selectedLeft === p.id
            return (
              <button
                key={p.id}
                onClick={() => isMatched ? removeMatch(p.id) : handleLeftClick(p.id)}
                disabled={disabled}
                className={`w-full text-left px-3 py-2.5 rounded-xl border-2 text-sm font-medium transition-all
                  ${isActive ? 'border-blue-500 bg-blue-50 text-blue-900 shadow-md' :
                    isMatched ? 'border-emerald-400 bg-emerald-50 text-emerald-800' :
                    'border-gray-200 bg-white text-gray-700 hover:border-blue-300'
                  } ${disabled ? 'cursor-default' : 'cursor-pointer'}`}
              >
                <span className="flex items-center gap-2">
                  <span className={`w-5 h-5 rounded-full flex items-center justify-center text-xs font-bold flex-shrink-0
                    ${isActive ? 'bg-blue-500 text-white' : isMatched ? 'bg-emerald-500 text-white' : 'bg-gray-100 text-gray-500'}`}>
                    {p.id}
                  </span>
                  {p.left}
                </span>
                {isMatched && (
                  <p className="text-xs text-emerald-600 mt-1 pl-7 truncate">→ {getDefText(matches[p.id])}</p>
                )}
              </button>
            )
          })}
        </div>

        {/* Right column — definitions */}
        <div className="space-y-2">
          <p className="text-xs font-semibold uppercase tracking-wide text-gray-400 mb-3">Definitions</p>
          {defs.map(def => (
            <button
              key={def.id}
              onClick={() => handleRightClick(def.id)}
              disabled={disabled || (!selectedLeft && !Object.values(matches).includes(def.id))}
              className={`w-full text-left px-3 py-2.5 rounded-xl border-2 text-sm transition-all
                ${getDefColor(def.id)}`}
            >
              {def.right}
            </button>
          ))}
        </div>
      </div>

      <Button
        onClick={handleSubmit}
        disabled={disabled || !allMatched}
        className="w-full"
      >
        <CheckCircle className="mr-2 w-4 h-4" />
        Submit Matches
      </Button>
    </div>
  )
}
