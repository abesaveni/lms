import React from 'react'

interface Props {
  text: string
  /** 'light' = default white background, 'dark' = terminal/dark background */
  variant?: 'light' | 'dark'
}

function renderInline(text: string, key: string | number): React.ReactNode {
  // Handle bold+italic (***text***), bold (**text**), italic (*text*), inline code (`code`)
  const parts = text.split(/(`[^`]+`|\*\*\*[^*]+\*\*\*|\*\*[^*]+\*\*|\*[^*]+\*)/g)
  return (
    <span key={key}>
      {parts.map((part, i) => {
        if (part.startsWith('`') && part.endsWith('`') && part.length > 2)
          return <code key={i} className="bg-gray-100 text-pink-600 text-[0.8em] px-1.5 py-0.5 rounded font-mono">{part.slice(1, -1)}</code>
        if (part.startsWith('***') && part.endsWith('***'))
          return <strong key={i}><em>{part.slice(3, -3)}</em></strong>
        if (part.startsWith('**') && part.endsWith('**'))
          return <strong key={i}>{part.slice(2, -2)}</strong>
        if (part.startsWith('*') && part.endsWith('*'))
          return <em key={i}>{part.slice(1, -1)}</em>
        return part
      })}
    </span>
  )
}

/**
 * Renders AI-generated markdown text as formatted HTML.
 * Handles: **bold**, *italic*, `code`, bullet/numbered lists,
 * headings (#/##/###/####), horizontal rules (---), pipe tables.
 * Use variant="dark" for terminal-style dark backgrounds.
 */
export function AIMarkdown({ text, variant = 'light' }: Props) {
  const dark = variant === 'dark'
  const accentColor = dark ? 'text-emerald-400' : 'text-indigo-500'
  const headingColor = dark ? 'text-white' : 'text-gray-900'
  const h2Color = dark ? 'text-emerald-300' : 'text-indigo-700'
  const h3Color = dark ? 'text-emerald-400' : 'text-indigo-600'
  const h4Color = dark ? 'text-emerald-500' : 'text-gray-700'
  const mutedColor = dark ? 'text-slate-400' : 'text-gray-400'

  const lines = text.split('\n')
  const nodes: React.ReactNode[] = []
  let bulletBuffer: string[] = []
  let numberedBuffer: string[] = []
  let tableBuffer: string[][] = []
  let tableHasHeader = false

  const flushBullets = (key: string) => {
    if (bulletBuffer.length === 0) return
    nodes.push(
      <ul key={key} className="list-none space-y-1 my-1.5 ml-1">
        {bulletBuffer.map((b, i) => (
          <li key={i} className="flex gap-2">
            <span className={`${accentColor} flex-shrink-0 mt-0.5 font-bold`}>•</span>
            <span className="leading-relaxed">{renderInline(b, i)}</span>
          </li>
        ))}
      </ul>
    )
    bulletBuffer = []
  }

  const flushNumbered = (key: string) => {
    if (numberedBuffer.length === 0) return
    nodes.push(
      <ol key={key} className="list-none space-y-1 my-1.5 ml-1">
        {numberedBuffer.map((b, i) => (
          <li key={i} className="flex gap-2">
            <span className={`${accentColor} font-bold flex-shrink-0 min-w-[1.2rem]`}>{i + 1}.</span>
            <span className="leading-relaxed">{renderInline(b, i)}</span>
          </li>
        ))}
      </ol>
    )
    numberedBuffer = []
  }

  const flushTable = (key: string) => {
    if (tableBuffer.length === 0) return
    const headers = tableHasHeader ? tableBuffer[0] : null
    const rows = tableHasHeader ? tableBuffer.slice(1) : tableBuffer
    nodes.push(
      <div key={key} className="overflow-x-auto my-3">
        <table className={`w-full text-xs border-collapse rounded-lg overflow-hidden ${dark ? 'border-slate-700' : 'border-gray-200'} border`}>
          {headers && (
            <thead>
              <tr className={dark ? 'bg-slate-700' : 'bg-indigo-50'}>
                {headers.map((h, i) => (
                  <th key={i} className={`px-3 py-2 text-left font-bold ${dark ? 'text-emerald-300 border-slate-600' : 'text-indigo-700 border-gray-200'} border-b`}>
                    {renderInline(h.trim(), i)}
                  </th>
                ))}
              </tr>
            </thead>
          )}
          <tbody>
            {rows.map((row, ri) => (
              <tr key={ri} className={ri % 2 === 0
                ? (dark ? 'bg-slate-900' : 'bg-white')
                : (dark ? 'bg-slate-800' : 'bg-gray-50')
              }>
                {row.map((cell, ci) => (
                  <td key={ci} className={`px-3 py-2 ${dark ? 'text-slate-300 border-slate-700' : 'text-gray-700 border-gray-100'} border-b`}>
                    {renderInline(cell.trim(), ci)}
                  </td>
                ))}
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    )
    tableBuffer = []
    tableHasHeader = false
  }

  const flushAll = (key: string) => {
    flushBullets(key + '-b')
    flushNumbered(key + '-n')
    flushTable(key + '-t')
  }

  lines.forEach((raw, idx) => {
    const trimmed = raw.trim()

    // Blank line — flush buffers
    if (!trimmed) {
      flushAll(`e-${idx}`)
      return
    }

    // Horizontal rule --- or ===
    if (/^[-=]{3,}$/.test(trimmed)) {
      flushAll(`hr-${idx}`)
      nodes.push(<hr key={idx} className={`my-3 border-0 border-t ${dark ? 'border-slate-700' : 'border-gray-200'}`} />)
      return
    }

    // Table row (starts and ends with |)
    if (trimmed.startsWith('|') && trimmed.endsWith('|')) {
      const cells = trimmed.slice(1, -1).split('|')
      // Separator row (---|---|---) → marks previous row as header
      if (cells.every(c => /^[\s\-:]+$/.test(c))) {
        tableHasHeader = tableBuffer.length > 0
        return
      }
      flushBullets(`bl-${idx}`)
      flushNumbered(`nl-${idx}`)
      tableBuffer.push(cells)
      return
    }

    // Bullet line
    const bulletMatch = trimmed.match(/^[*\-]\s+(.+)/)
    if (bulletMatch) {
      flushNumbered(`nl-${idx}`)
      flushTable(`tbl-${idx}`)
      bulletBuffer.push(bulletMatch[1])
      return
    }

    // Numbered line
    const numberedMatch = trimmed.match(/^\d+\.\s+(.+)/)
    if (numberedMatch) {
      flushBullets(`bl-${idx}`)
      flushTable(`tbl-${idx}`)
      numberedBuffer.push(numberedMatch[1])
      return
    }

    flushAll(`f-${idx}`)

    // H4 ####
    if (trimmed.startsWith('#### ')) {
      nodes.push(
        <p key={idx} className={`font-semibold text-xs mt-2 mb-0.5 ${h4Color}`}>
          {renderInline(trimmed.slice(5), idx)}
        </p>
      )
      return
    }

    // H3 ###
    if (trimmed.startsWith('### ')) {
      nodes.push(
        <p key={idx} className={`font-bold text-sm mt-3 mb-1 ${h3Color}`}>
          {renderInline(trimmed.slice(4), idx)}
        </p>
      )
      return
    }

    // H2 ##
    if (trimmed.startsWith('## ')) {
      nodes.push(
        <p key={idx} className={`font-bold text-base mt-4 mb-1 ${h2Color}`}>
          {renderInline(trimmed.slice(3), idx)}
        </p>
      )
      return
    }

    // H1 #
    if (trimmed.startsWith('# ')) {
      nodes.push(
        <p key={idx} className={`font-extrabold text-lg mt-4 mb-1.5 ${headingColor}`}>
          {renderInline(trimmed.slice(2), idx)}
        </p>
      )
      return
    }

    // Blockquote >
    if (trimmed.startsWith('> ')) {
      nodes.push(
        <blockquote key={idx} className={`border-l-4 pl-3 py-0.5 my-1 italic ${dark ? 'border-emerald-600 text-slate-400' : 'border-indigo-300 text-gray-500'}`}>
          {renderInline(trimmed.slice(2), idx)}
        </blockquote>
      )
      return
    }

    // Normal paragraph — skip lines that are only punctuation/dividers
    if (trimmed === '---' || trimmed === '***') return

    nodes.push(
      <p key={idx} className={`leading-relaxed ${dark ? '' : 'text-gray-700'}`}>
        {renderInline(trimmed, idx)}
      </p>
    )
  })

  flushAll('end')

  // Suppress unused variable warning
  void mutedColor

  return <div className="space-y-1 text-sm">{nodes}</div>
}
