import { ReactNode } from 'react'
import { motion } from 'framer-motion'
import Button from './Button'

interface EmptyStateProps {
  icon: ReactNode
  title: string
  description: string
  action?: {
    label: string
    onClick: () => void
  }
  className?: string
}

export const EmptyState = ({ icon, title, description, action, className = '' }: EmptyStateProps) => {
  return (
    <motion.div
      initial={{ opacity: 0, y: 20 }}
      animate={{ opacity: 1, y: 0 }}
      className={`text-center py-12 px-4 ${className}`}
    >
      <motion.div
        initial={{ scale: 0 }}
        animate={{ scale: 1 }}
        transition={{ delay: 0.1, type: 'spring', stiffness: 200 }}
        className="w-16 h-16 mx-auto mb-4 rounded-full bg-gray-100 flex items-center justify-center text-gray-400"
      >
        {icon}
      </motion.div>
      <h3 className="text-lg font-semibold text-gray-900 mb-2">{title}</h3>
      <p className="text-sm text-gray-600 mb-6 max-w-sm mx-auto">{description}</p>
      {action && (
        <Button onClick={action.onClick} size="sm">
          {action.label}
        </Button>
      )}
    </motion.div>
  )
}
