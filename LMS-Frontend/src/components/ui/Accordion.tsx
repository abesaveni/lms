import { ReactNode, useState } from 'react'
import { ChevronDown } from 'lucide-react'
import { clsx } from 'clsx'

interface AccordionProps {
  items: Array<{
    title: string
    content: ReactNode
  }>
}

export const Accordion = ({ items }: AccordionProps) => {
  const [openIndex, setOpenIndex] = useState<number | null>(null)

  const toggle = (index: number) => {
    setOpenIndex(openIndex === index ? null : index)
  }

  return (
    <div className="space-y-2">
      {items.map((item, index) => (
        <div key={index} className="border border-gray-200 rounded-lg overflow-hidden">
          <button
            onClick={() => toggle(index)}
            className="w-full px-6 py-4 flex items-center justify-between text-left hover:bg-gray-50 transition-colors"
          >
            <span className="font-medium text-gray-900">{item.title}</span>
            <ChevronDown
              className={clsx(
                'w-5 h-5 text-gray-500 transition-transform',
                openIndex === index && 'transform rotate-180'
              )}
            />
          </button>
          {openIndex === index && (
            <div className="px-6 py-4 bg-gray-50 text-gray-700">
              {item.content}
            </div>
          )}
        </div>
      ))}
    </div>
  )
}

export default Accordion
