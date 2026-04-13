import { useEffect, useState } from 'react'
import { useSpring, useMotionValueEvent } from 'framer-motion'

interface CountUpProps {
  value: number | string
  duration?: number
  className?: string
  prefix?: string
  suffix?: string
  decimals?: number
}

export const CountUp = ({ 
  value, 
  duration = 1.5, 
  className = '',
  prefix = '',
  suffix = '',
  decimals = 0
}: CountUpProps) => {
  const numericValue = typeof value === 'string' ? parseFloat(value.replace(/[^0-9.]/g, '')) : value
  const spring = useSpring(0, { duration, bounce: 0 })
  const [display, setDisplay] = useState('0')

  useEffect(() => {
    spring.set(numericValue)
  }, [numericValue, spring])

  useMotionValueEvent(spring, 'change', (latest) => {
    if (decimals > 0) {
      setDisplay(latest.toFixed(decimals))
    } else {
      setDisplay(Math.floor(latest).toLocaleString())
    }
  })

  // If value contains non-numeric characters (like $, K, etc.), handle it differently
  if (typeof value === 'string' && /[^0-9.,]/.test(value)) {
    return <span className={className}>{value}</span>
  }

  return (
    <span className={className}>
      {prefix}
      {display}
      {suffix}
    </span>
  )
}
