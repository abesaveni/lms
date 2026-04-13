import { HTMLAttributes } from 'react'
import { clsx } from 'clsx'

interface BadgeProps extends HTMLAttributes<HTMLSpanElement> {
  variant?: 'default' | 'success' | 'warning' | 'error' | 'info'
  children?: React.ReactNode
}

export const Badge = ({ children, className, variant = 'default', ...props }: BadgeProps) => {
  return (
    <span
      className={clsx(
        'inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium',
        {
          'bg-primary-100 text-primary-800': variant === 'default',
          'bg-secondary-100 text-secondary-800': variant === 'success',
          'bg-yellow-100 text-yellow-800': variant === 'warning',
          'bg-red-100 text-red-800': variant === 'error',
          'bg-blue-100 text-blue-800': variant === 'info',
        },
        className
      )}
      {...props}
    >
      {children}
    </span>
  )
}

export default Badge
