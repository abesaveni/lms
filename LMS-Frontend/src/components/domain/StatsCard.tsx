import { Card } from '../ui/Card'
import { ReactNode } from 'react'

interface StatsCardProps {
  title: string
  value: string | number
  icon?: ReactNode
  trend?: {
    value: number
    isPositive: boolean
  }
}

export const StatsCard = ({ title, value, icon, trend }: StatsCardProps) => {
  return (
    <Card hover>
      <div className="flex items-start justify-between">
        <div>
          <p className="text-sm text-gray-600 mb-1">{title}</p>
          <p className="text-2xl font-bold text-gray-900">{value}</p>
          {trend && (
            <p className={`text-sm mt-2 ${trend.isPositive ? 'text-secondary-600' : 'text-red-600'}`}>
              {trend.isPositive ? '↑' : '↓'} {Math.abs(trend.value)}%
            </p>
          )}
        </div>
        {icon && (
          <div className="p-3 rounded-lg bg-gradient-primary/10 text-primary-600">
            {icon}
          </div>
        )}
      </div>
    </Card>
  )
}

export default StatsCard
