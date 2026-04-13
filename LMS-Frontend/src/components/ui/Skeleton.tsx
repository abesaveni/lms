import { motion } from 'framer-motion'

interface SkeletonProps {
  className?: string
  variant?: 'text' | 'circular' | 'rectangular'
  width?: string | number
  height?: string | number
}

export const Skeleton = ({ 
  className = '', 
  variant = 'rectangular',
  width,
  height 
}: SkeletonProps) => {
  const baseClasses = 'bg-gray-200 rounded'
  
  const variantClasses = {
    text: 'h-4 rounded',
    circular: 'rounded-full',
    rectangular: 'rounded-lg',
  }

  const style: React.CSSProperties = {}
  if (width) style.width = typeof width === 'number' ? `${width}px` : width
  if (height) style.height = typeof height === 'number' ? `${height}px` : height

  return (
    <motion.div
      className={`${baseClasses} ${variantClasses[variant]} ${className}`}
      style={style}
      animate={{
        opacity: [0.5, 1, 0.5],
      }}
      transition={{
        duration: 1.5,
        repeat: Infinity,
        ease: 'easeInOut',
      }}
    />
  )
}

export const SkeletonCard = () => (
  <div className="p-6 bg-white rounded-xl border border-gray-200">
    <Skeleton variant="text" width="60%" className="mb-4" />
    <Skeleton variant="text" width="40%" />
  </div>
)

export const SkeletonStats = () => (
  <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
    {[1, 2, 3, 4].map((i) => (
      <SkeletonCard key={i} />
    ))}
  </div>
)
