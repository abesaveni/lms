import { motion } from 'framer-motion'
import { ReactNode } from 'react'
import { CountUp } from '../ui/CountUp'

interface EnhancedStatsCardProps {
  title: string
  value: string | number
  icon?: ReactNode
  trend?: {
    value: number
    isPositive: boolean
  }
  delay?: number
  gradient?: 'primary' | 'success' | 'warning' | 'error'
  description?: string
}

export const EnhancedStatsCard = ({ 
  title, 
  value, 
  icon, 
  trend, 
  delay = 0,
  gradient = 'primary',
  description
}: EnhancedStatsCardProps) => {
  const gradientClasses = {
    primary: 'from-primary-500/10 to-primary-600/5',
    success: 'from-success-500/10 to-success-600/5',
    warning: 'from-warning-500/10 to-warning-600/5',
    error: 'from-error-500/10 to-error-600/5',
  }

  const iconBgClasses = {
    primary: 'bg-primary-500/10 text-primary-600',
    success: 'bg-success-500/10 text-success-600',
    warning: 'bg-warning-500/10 text-warning-600',
    error: 'bg-error-500/10 text-error-600',
  }

  return (
    <motion.div
      initial={{ opacity: 0, y: 20 }}
      animate={{ opacity: 1, y: 0 }}
      transition={{ duration: 0.4, delay }}
      whileHover={{ y: -4, transition: { duration: 0.2 } }}
      className="group relative overflow-hidden rounded-xl bg-white border border-gray-200 p-6 shadow-sm hover:shadow-lg transition-all duration-300"
    >
      {/* Gradient accent */}
      <div className={`absolute inset-0 bg-gradient-to-br ${gradientClasses[gradient]} opacity-0 group-hover:opacity-100 transition-opacity duration-300`} />
      
      <div className="relative flex items-start justify-between">
        <div className="flex-1">
          <p className="text-sm font-medium text-gray-600 mb-2">{title}</p>
          <div className="flex items-baseline gap-2 mb-1">
            <CountUp 
              value={value} 
              className="text-3xl font-bold text-gray-900"
            />
          </div>
          {description && (
            <p className="text-xs text-gray-500 mt-1">{description}</p>
          )}
          {trend && (
            <motion.div
              initial={{ opacity: 0, x: -10 }}
              animate={{ opacity: 1, x: 0 }}
              transition={{ delay: delay + 0.2 }}
              className={`inline-flex items-center gap-1 mt-3 text-sm font-medium ${
                trend.isPositive ? 'text-success-600' : 'text-error-600'
              }`}
            >
              <span>{trend.isPositive ? '↑' : '↓'}</span>
              <span>{Math.abs(trend.value)}%</span>
              <span className="text-gray-500 text-xs ml-1">vs last month</span>
            </motion.div>
          )}
        </div>
        {icon && (
          <motion.div
            initial={{ scale: 0 }}
            animate={{ scale: 1 }}
            transition={{ delay: delay + 0.1, type: 'spring', stiffness: 200 }}
            className={`p-3 rounded-xl ${iconBgClasses[gradient]} transition-colors duration-300`}
          >
            {icon}
          </motion.div>
        )}
      </div>
    </motion.div>
  )
}

export default EnhancedStatsCard
