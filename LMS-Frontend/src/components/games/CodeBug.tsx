import { useState, useRef } from 'react'
import { Bug, CheckCircle } from 'lucide-react'
import Button from '../ui/Button'

interface CodeBugContent {
  language: string
  code: string
  description: string
  options: string[]
  category: string
}

interface Props {
  contentJson: string
  onSubmit: (answerJson: string, timeTaken: number) => void
  disabled?: boolean
}

// Very lightweight syntax highlight — colours keywords for common languages
function highlight(code: string, lang: string): React.ReactNode {
  const jsKeywords = ['function', 'return', 'let', 'const', 'var', 'if', 'else', 'while', 'for', 'async', 'await', 'true', 'false', 'null', 'undefined', 'new', 'class', 'import', 'export', 'default', 'from']
  const pyKeywords = ['def', 'return', 'if', 'else', 'elif', 'while', 'for', 'in', 'True', 'False', 'None', 'import', 'from', 'class', 'print']
  const keywords = lang === 'python' ? pyKeywords : jsKeywords

  const lines = code.split('\n')
  return (
    <>
      {lines.map((line, li) => {
        const parts = line.split(/(\b\w+\b|[^a-zA-Z0-9_]+)/).filter(Boolean)
        return (
          <div key={li} className="leading-6">
            <span className="select-none text-gray-500 mr-4 text-xs">{(li + 1).toString().padStart(2, ' ')}</span>
            {parts.map((part, pi) => {
              if (keywords.includes(part))
                return <span key={pi} className="text-purple-400 font-semibold">{part}</span>
              if (/^".*"$|^'.*'$|^`.*`$/.test(part))
                return <span key={pi} className="text-green-400">{part}</span>
              if (/^\d+$/.test(part))
                return <span key={pi} className="text-orange-400">{part}</span>
              return <span key={pi}>{part}</span>
            })}
          </div>
        )
      })}
    </>
  )
}

export default function CodeBug({ contentJson, onSubmit, disabled }: Props) {
  const content: CodeBugContent = JSON.parse(contentJson)
  const [selected, setSelected] = useState<number | null>(null)
  const startRef = useRef(Date.now())

  const handleSubmit = () => {
    if (selected === null) return
    const timeTaken = Math.round((Date.now() - startRef.current) / 1000)
    onSubmit(JSON.stringify({ correct: selected }), timeTaken)
  }

  return (
    <div className="space-y-5">
      {/* Category */}
      <span className="text-xs font-semibold uppercase tracking-widest text-red-600 bg-red-50 px-3 py-1 rounded-full inline-flex items-center gap-1.5">
        <Bug className="w-3.5 h-3.5" />
        {content.category}
      </span>

      {/* Question */}
      <p className="text-sm font-medium text-gray-700">{content.description}</p>

      {/* Code block */}
      <div className="rounded-xl overflow-hidden border border-gray-800">
        <div className="flex items-center gap-1.5 px-4 py-2 bg-gray-900">
          <span className="w-3 h-3 rounded-full bg-red-500" />
          <span className="w-3 h-3 rounded-full bg-yellow-500" />
          <span className="w-3 h-3 rounded-full bg-green-500" />
          <span className="text-xs text-gray-400 ml-2">{content.language}</span>
        </div>
        <pre className="bg-gray-950 text-gray-100 text-sm p-4 overflow-x-auto font-mono">
          <code>{highlight(content.code, content.language)}</code>
        </pre>
      </div>

      {/* Options */}
      <div className="space-y-2.5">
        {content.options.map((opt, idx) => (
          <button
            key={idx}
            onClick={() => !disabled && setSelected(idx)}
            disabled={disabled}
            className={`w-full text-left px-4 py-3 rounded-xl border-2 text-sm font-medium transition-all
              ${selected === idx
                ? 'border-red-500 bg-red-50 text-red-900'
                : 'border-gray-200 bg-white text-gray-700 hover:border-red-300 hover:bg-red-50/30'
              }
              ${disabled ? 'cursor-default' : 'cursor-pointer'}`}
          >
            <span className="flex items-center gap-3">
              <span className={`w-6 h-6 rounded-full flex items-center justify-center text-xs font-bold flex-shrink-0
                ${selected === idx ? 'bg-red-500 text-white' : 'bg-gray-100 text-gray-500'}`}>
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
        Submit Answer
      </Button>
    </div>
  )
}
