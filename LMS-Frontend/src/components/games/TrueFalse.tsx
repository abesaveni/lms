import { useState, useRef } from 'react'
import { CheckCircle, ThumbsUp, ThumbsDown } from 'lucide-react'
import Button from '../ui/Button'

interface Statement { id: string; text: string }
interface TrueFalseContent { statements: Statement[] }

interface Props {
  contentJson: string
  onSubmit: (answerJson: string, timeTaken: number) => void
  disabled?: boolean
}

export default function TrueFalse({ contentJson, onSubmit, disabled }: Props) {
  const content: TrueFalseContent = JSON.parse(contentJson)
  const [answers, setAnswers] = useState<Record<string, boolean>>({})
  const startRef = useRef(Date.now())

  const setAnswer = (id: string, val: boolean) => {
    if (disabled) return
    setAnswers(prev => ({ ...prev, [id]: val }))
  }

  const allAnswered = content.statements.every(s => s.id in answers)

  const handleSubmit = () => {
    if (!allAnswered) return
    const timeTaken = Math.round((Date.now() - startRef.current) / 1000)
    onSubmit(JSON.stringify({ answers }), timeTaken)
  }

  return (
    <div className="space-y-4">
      <p className="text-sm text-gray-500">
        Mark each statement as <strong>True</strong> or <strong>False</strong>.
      </p>

      {content.statements.map((stmt, idx) => {
        const val = answers[stmt.id]
        const answered = stmt.id in answers
        return (
          <div
            key={stmt.id}
            className={`p-4 rounded-xl border-2 transition-all
              ${answered
                ? val ? 'border-green-400 bg-green-50' : 'border-red-300 bg-red-50'
                : 'border-gray-200 bg-white'}`}
          >
            <div className="flex items-start justify-between gap-4">
              <div className="flex items-start gap-3 flex-1">
                <span className="w-6 h-6 rounded-full bg-gray-100 flex items-center justify-center text-xs font-bold text-gray-500 flex-shrink-0 mt-0.5">
                  {idx + 1}
                </span>
                <p className="text-sm font-medium text-gray-800 leading-relaxed">{stmt.text}</p>
              </div>
              <div className="flex gap-2 flex-shrink-0">
                <button
                  onClick={() => setAnswer(stmt.id, true)}
                  disabled={disabled}
                  className={`flex items-center gap-1.5 px-3 py-1.5 rounded-lg text-sm font-semibold transition-all
                    ${val === true
                      ? 'bg-green-500 text-white shadow-sm'
                      : 'bg-gray-100 text-gray-600 hover:bg-green-100 hover:text-green-700'
                    } ${disabled ? 'cursor-default' : 'cursor-pointer'}`}
                >
                  <ThumbsUp className="w-3.5 h-3.5" />
                  True
                </button>
                <button
                  onClick={() => setAnswer(stmt.id, false)}
                  disabled={disabled}
                  className={`flex items-center gap-1.5 px-3 py-1.5 rounded-lg text-sm font-semibold transition-all
                    ${val === false
                      ? 'bg-red-500 text-white shadow-sm'
                      : 'bg-gray-100 text-gray-600 hover:bg-red-100 hover:text-red-700'
                    } ${disabled ? 'cursor-default' : 'cursor-pointer'}`}
                >
                  <ThumbsDown className="w-3.5 h-3.5" />
                  False
                </button>
              </div>
            </div>
          </div>
        )
      })}

      {/* Progress */}
      <div className="flex items-center gap-2 text-xs text-gray-500">
        <div className="flex gap-1">
          {content.statements.map(s => (
            <div key={s.id}
              className={`w-3 h-3 rounded-full ${s.id in answers ? 'bg-indigo-500' : 'bg-gray-200'}`} />
          ))}
        </div>
        {Object.keys(answers).length}/{content.statements.length} answered
      </div>

      <Button onClick={handleSubmit} disabled={disabled || !allAnswered} className="w-full">
        <CheckCircle className="mr-2 w-4 h-4" />
        Submit All Answers
      </Button>
    </div>
  )
}
