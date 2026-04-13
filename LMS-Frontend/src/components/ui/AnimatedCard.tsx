import { motion } from 'framer-motion'
import { ReactNode } from 'react'
import { Card, CardHeader, CardTitle, CardContent } from './Card'

interface AnimatedCardProps {
  children: ReactNode
  className?: string
  delay?: number
  hover?: boolean
}

export const AnimatedCard = ({ children, className = '', delay = 0, hover = true }: AnimatedCardProps) => {
  return (
    <motion.div
      initial={{ opacity: 0, y: 20 }}
      animate={{ opacity: 1, y: 0 }}
      transition={{ duration: 0.4, delay }}
      whileHover={hover ? { y: -4, transition: { duration: 0.2 } } : undefined}
      className={className}
    >
      {children}
    </motion.div>
  )
}

interface AnimatedCardContentProps {
  title?: string
  headerAction?: ReactNode
  children: ReactNode
  className?: string
}

export const AnimatedCardContent = ({ title, headerAction, children, className = '' }: AnimatedCardContentProps) => {
  return (
    <Card className={`transition-all duration-300 hover:shadow-lg ${className}`}>
      {title && (
        <CardHeader>
          <div className="flex items-center justify-between">
            {title && <CardTitle>{title}</CardTitle>}
            {headerAction}
          </div>
        </CardHeader>
      )}
      <CardContent>{children}</CardContent>
    </Card>
  )
}
