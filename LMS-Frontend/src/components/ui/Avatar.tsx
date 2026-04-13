import { HTMLAttributes } from 'react'
import { clsx } from 'clsx'
import { getMediaUrl } from '../../services/api'

interface AvatarProps extends HTMLAttributes<HTMLDivElement> {
  src?: string
  alt?: string
  size?: 'sm' | 'md' | 'lg' | 'xl'
  name?: string
}

export const Avatar = ({ src, alt, size = 'md', name, className, ...props }: AvatarProps) => {
  const mediaUrl = getMediaUrl(src)

  const getInitials = (name: string) => {
    return name
      .split(' ')
      .map((n) => n[0])
      .join('')
      .toUpperCase()
      .slice(0, 2)
  }

  const sizeClasses = {
    sm: 'w-8 h-8 text-xs',
    md: 'w-10 h-10 text-sm',
    lg: 'w-12 h-12 text-base',
    xl: 'w-16 h-16 text-lg',
  }

  return (
    <div
      className={clsx(
        'rounded-full bg-gradient-primary flex items-center justify-center text-white font-semibold overflow-hidden',
        sizeClasses[size],
        className
      )}
      {...props}
    >
      {mediaUrl ? (
        <img src={mediaUrl} alt={alt || name} className="w-full h-full object-cover" />
      ) : (
        <span>{name ? getInitials(name) : '?'}</span>
      )}
    </div>
  )
}

export default Avatar
