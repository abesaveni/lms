import { useState, useRef } from 'react'
import { CheckCircle } from 'lucide-react'
import Button from '../ui/Button'

interface FillBlankContent {
  sentence: string
  options: string[]
  category: string
}

interface Props {
  contentJson: string
  onSubmit: (answerJson: string, timeTaken: number) => void
  disabled?: boolean
}

export default function FillBlank({ contentJson, onSubmit, disabled }: Props) {
  const content: FillBlankContent = JSON.parse(contentJson)
  const [selected, setSelected] = useState<string | null>(null)
  const startRef = useRef(Date.now())

  // Split sentence at ___
  const parts = content.sentence.split('___')

  const handleSubmit = () => {
    if (!selected) return
    const timeTaken = Math.round((Date.now() - startRef.current) / 1000)
    onSubmit(JSON.stringify({ answer: selected }), timeTaken)
  }

  return (
    <div className="space-y-6">
      <span className="text-xs font-semibold uppercase tracking-widest text-purple-600 bg-purple-50 px-3 py-1 rounded-full inline-block">
        {content.category}
      </span>

      {/* Sentence with blank */}
      <div className="p-5 bg-gray-50 rounded-xl border border-gray-200">
        <p className="text-lg font-medium text-gray-800 leading-relaxed">
          {parts[0]}
          <span className={`inline-flex items-center justify-center min-w-[120px] mx-1 px-3 py-0.5 rounded-lg border-b-2 font-bold text-purple-700
            ${selected ? 'bg-purple-100 border-purple-500' : 'bg-white border-gray-400 text-gray-400'}`}>
            {selected ?? '  ?  '}
          </span>
          {parts[1]}
        </p>
      </div>

      {/* Option buttons */}
      <div className="grid grid-cols-2 gap-3">
        {content.options.map((opt, idx) => (
          <button
            key={idx}
            onClick={() => !disabled && setSelected(opt)}
            disabled={disabled}
            className={`px-4 py-3 rounded-xl border-2 text-sm font-semibold transition-all
              ${selected === opt
                ? 'border-purple-500 bg-purple-50 text-purple-900'
                : 'border-gray-200 bg-white text-gray-700 hover:border-purple-300 hover:bg-purple-50/30'
              }
              ${disabled ? 'cursor-default' : 'cursor-pointer'}`}
          >
            {opt}
          </button>
        ))}
      </div>

      <Button
        onClick={handleSubmit}
        disabled={disabled || !selected}
        className="w-full"
      >
        <CheckCircle className="mr-2 w-4 h-4" />
        Submit Answer
      </Button>
    </div>
  )
}
