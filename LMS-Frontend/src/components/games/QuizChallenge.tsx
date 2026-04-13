import { useState, useEffect, useRef } from 'react'
import { Clock, CheckCircle } from 'lucide-react'
import Button from '../ui/Button'

interface QuizContent {
  question: string
  options: string[]
  timeLimit: number
  category: string
}

interface Props {
  contentJson: string
  onSubmit: (answerJson: string, timeTaken: number) => void
  disabled?: boolean
}

export default function QuizChallenge({ contentJson, onSubmit, disabled }: Props) {
  const content: QuizContent = JSON.parse(contentJson)
  const [selected, setSelected] = useState<number | null>(null)
  const [timeLeft, setTimeLeft] = useState(content.timeLimit)
  const [autoSubmitted, setAutoSubmitted] = useState(false)
  const startRef = useRef(Date.now())

  useEffect(() => {
    if (disabled || autoSubmitted) return
    const timer = setInterval(() => {
      setTimeLeft(prev => {
        if (prev <= 1) {
          clearInterval(timer)
          if (!autoSubmitted) {
            setAutoSubmitted(true)
            const timeTaken = Math.round((Date.now() - startRef.current) / 1000)
            onSubmit(JSON.stringify({ correct: selected ?? -1 }), timeTaken)
          }
          return 0
        }
        return prev - 1
      })
    }, 1000)
    return () => clearInterval(timer)
  }, [disabled, autoSubmitted, selected])

  const handleSubmit = () => {
    if (selected === null) return
    const timeTaken = Math.round((Date.now() - startRef.current) / 1000)
    onSubmit(JSON.stringify({ correct: selected }), timeTaken)
  }

  const timerPct = (timeLeft / content.timeLimit) * 100
  const timerColor = timerPct > 50 ? 'bg-green-500' : timerPct > 25 ? 'bg-amber-500' : 'bg-red-500'

  return (
    <div className="space-y-6">
      {/* Timer */}
      {!disabled && (
        <div className="space-y-1.5">
          <div className="flex items-center justify-between text-sm">
            <span className="flex items-center gap-1.5 text-gray-600 font-medium">
              <Clock className="w-4 h-4" />
              Time remaining
            </span>
            <span className={`font-bold ${timeLeft <= 10 ? 'text-red-600' : 'text-gray-700'}`}>
              {timeLeft}s
            </span>
          </div>
          <div className="w-full bg-gray-200 rounded-full h-2">
            <div
              className={`h-2 rounded-full transition-all duration-1000 ${timerColor}`}
              style={{ width: `${timerPct}%` }}
            />
          </div>
        </div>
      )}

      {/* Category */}
      <span className="text-xs font-semibold uppercase tracking-widest text-blue-600 bg-blue-50 px-3 py-1 rounded-full inline-block">
        {content.category}
      </span>

      {/* Question */}
      <p className="text-lg font-semibold text-gray-900 leading-relaxed">
        {content.question}
      </p>

      {/* Options */}
      <div className="space-y-3">
        {content.options.map((opt, idx) => (
          <button
            key={idx}
            onClick={() => !disabled && setSelected(idx)}
            disabled={disabled}
            className={`w-full text-left px-4 py-3.5 rounded-xl border-2 font-medium transition-all
              ${selected === idx
                ? 'border-blue-500 bg-blue-50 text-blue-900'
                : 'border-gray-200 bg-white text-gray-700 hover:border-blue-300 hover:bg-blue-50/40'
              }
              ${disabled ? 'cursor-default' : 'cursor-pointer'}`}
          >
            <span className="inline-flex items-center gap-3">
              <span className={`w-6 h-6 rounded-full flex items-center justify-center text-xs font-bold flex-shrink-0
                ${selected === idx ? 'bg-blue-500 text-white' : 'bg-gray-100 text-gray-500'}`}>
                {String.fromCharCode(65 + idx)}
              </span>
              {opt}
            </span>
          </button>
        ))}
      </div>

      <Button
        onClick={handleSubmit}
        disabled={disabled || selected === null}
        className="w-full"
      >
        <CheckCircle className="mr-2 w-4 h-4" />
        Lock In Answer
      </Button>
    </div>
  )
}
