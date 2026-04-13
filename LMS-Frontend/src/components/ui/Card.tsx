import { HTMLAttributes, ReactNode } from 'react'
import { clsx } from 'clsx'

interface CardProps extends HTMLAttributes<HTMLDivElement> {
  children: ReactNode
  hover?: boolean
}

export const Card = ({ children, className, hover, ...props }: CardProps) => {
  return (
    <div
      className={clsx(
        'bg-white rounded-xl shadow-sm border border-gray-200 p-6',
        hover && 'transition-all duration-200 hover:shadow-md hover:border-primary-200',
        className
      )}
      {...props}
    >
      {children}
    </div>
  )
}

export const CardHeader = ({ children, className, ...props }: HTMLAttributes<HTMLDivElement>) => {
  return (
    <div className={clsx('mb-4', className)} {...props}>
      {children}
    </div>
  )
}

export const CardTitle = ({ children, className, ...props }: HTMLAttributes<HTMLHeadingElement>) => {
  return (
    <h3 className={clsx('text-xl font-semibold text-gray-900', className)} {...props}>
      {children}
    </h3>
  )
}

export const CardContent = ({ children, className, ...props }: HTMLAttributes<HTMLDivElement>) => {
  return (
    <div className={clsx('', className)} {...props}>
      {children}
    </div>
  )
}

export default Card
