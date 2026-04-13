import { useState, useEffect, useCallback } from 'react'
import { getSubscriptionStatus, getTrialStatus, TrialStatus, SubscriptionStatus } from '../services/aiApi'

export interface UseSubscriptionReturn {
  trialActive: boolean
  daysLeftInTrial: number
  subscriptionActive: boolean
  subscribedUntil: string | null
  trialExpired: boolean
  requiresSubscription: boolean
  isLoading: boolean
  refetch: () => void
}

export function useSubscription(): UseSubscriptionReturn {
  const [trialStatus, setTrialStatus] = useState<TrialStatus | null>(null)
  const [subStatus, setSubStatus] = useState<SubscriptionStatus | null>(null)
  const [isLoading, setIsLoading] = useState(true)

  const fetchStatus = useCallback(async () => {
    setIsLoading(true)
    try {
      const [trial, sub] = await Promise.all([
        getTrialStatus().catch(() => null),
        getSubscriptionStatus().catch(() => null),
      ])
      setTrialStatus(trial)
      setSubStatus(sub)
    } catch {
      // silently fail — user may not be authenticated
    } finally {
      setIsLoading(false)
    }
  }, [])

  useEffect(() => {
    fetchStatus()
  }, [fetchStatus])

  // Listen for subscription activated event to auto-refresh
  useEffect(() => {
    const handler = () => fetchStatus()
    window.addEventListener('subscription:activated', handler)
    return () => window.removeEventListener('subscription:activated', handler)
  }, [fetchStatus])

  return {
    trialActive: trialStatus?.trialActive ?? false,
    daysLeftInTrial: trialStatus?.daysRemaining ?? 0,
    subscriptionActive: subStatus?.isSubscribed ?? false,
    subscribedUntil: subStatus?.subscribedUntil ?? null,
    trialExpired: trialStatus?.trialExpired ?? false,
    requiresSubscription: trialStatus?.requiresSubscription ?? false,
    isLoading,
    refetch: fetchStatus,
  }
}
