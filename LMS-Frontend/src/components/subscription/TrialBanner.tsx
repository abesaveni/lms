import { motion, AnimatePresence } from 'framer-motion'
import { AlertTriangle, X, Zap } from 'lucide-react'
import { useState } from 'react'
import { useSubscription } from '../../hooks/useSubscription'
import { createSubscriptionOrder, activateSubscription } from '../../services/aiApi'

declare global {
  interface Window {
    Razorpay: any
  }
}

export function TrialBanner() {
  const { trialActive, daysLeftInTrial, subscriptionActive, isLoading, refetch } = useSubscription()
  const [dismissed, setDismissed] = useState(false)
  const [paying, setPaying] = useState(false)
  const [payError, setPayError] = useState<string | null>(null)

  // Show whenever trial is active and not yet subscribed
  // Urgency: amber when > 3 days, orange/red when ≤ 3 days
  const shouldShow = !isLoading && !subscriptionActive && trialActive && !dismissed
  const isUrgent = daysLeftInTrial <= 3

  const handlePayNow = async () => {
    setPaying(true)
    setPayError(null)
    try {
      const order = await createSubscriptionOrder()

      const options = {
        key: order.keyId,
        amount: order.amount,
        currency: order.currency,
        name: 'LiveExpert.AI',
        description: order.description,
        order_id: order.orderId,
        handler: async (response: { razorpay_order_id: string; razorpay_payment_id: string; razorpay_signature: string }) => {
          try {
            await activateSubscription({
              razorpayOrderId: response.razorpay_order_id,
              razorpayPaymentId: response.razorpay_payment_id,
              razorpaySignature: response.razorpay_signature,
            })
            window.dispatchEvent(new CustomEvent('subscription:activated'))
            refetch()
            setDismissed(true)
          } catch (err) {
            setPayError('Payment verified but activation failed. Please contact support.')
          }
        },
        prefill: {},
        theme: { color: '#6366f1' },
      }

      if (typeof window.Razorpay === 'undefined') {
        // Load Razorpay SDK dynamically
        await new Promise<void>((resolve, reject) => {
          const script = document.createElement('script')
          script.src = 'https://checkout.razorpay.com/v1/checkout.js'
          script.onload = () => resolve()
          script.onerror = () => reject(new Error('Failed to load Razorpay'))
          document.body.appendChild(script)
        })
      }

      const rzp = new window.Razorpay(options)
      rzp.open()
    } catch (err) {
      console.error('Payment error:', err)
      setPayError('Could not initiate payment. Please try again.')
    } finally {
      setPaying(false)
    }
  }

  return (
    <>
    {payError && (
      <div className="w-full bg-red-50 border-b border-red-200 px-4 py-2 text-sm text-red-700 text-center">
        {payError}
      </div>
    )}
    <AnimatePresence>
      {shouldShow && (
        <motion.div
          initial={{ y: -60, opacity: 0 }}
          animate={{ y: 0, opacity: 1 }}
          exit={{ y: -60, opacity: 0 }}
          transition={{ type: 'spring', stiffness: 300, damping: 30 }}
          className={`w-full text-white px-4 py-2.5 flex items-center justify-between shadow-md z-50 relative ${isUrgent ? 'bg-gradient-to-r from-red-500 to-orange-500' : 'bg-gradient-to-r from-indigo-500 to-purple-500'}`}
        >
          <div className="flex items-center gap-2 text-sm font-medium">
            <AlertTriangle className="w-4 h-4 flex-shrink-0" />
            {isUrgent ? (
              <span>
                ⚠️ Only <strong>{daysLeftInTrial} {daysLeftInTrial === 1 ? 'day' : 'days'}</strong> left in your free trial!
                Subscribe now to avoid losing access.
              </span>
            ) : (
              <span>
                🎉 Free trial active — <strong>{daysLeftInTrial} days</strong> remaining.
                Subscribe for ₹99/month to continue after your trial.
              </span>
            )}
          </div>

          <div className="flex items-center gap-2 flex-shrink-0">
            <button
              onClick={handlePayNow}
              disabled={paying}
              className="flex items-center gap-1.5 bg-white text-indigo-600 font-bold text-xs px-3 py-1.5 rounded-full hover:bg-indigo-50 transition-colors disabled:opacity-60"
            >
              <Zap className="w-3.5 h-3.5" />
              {paying ? 'Loading…' : 'Subscribe ₹99'}
            </button>
            <button
              onClick={() => setDismissed(true)}
              className="p-1 hover:bg-white/20 rounded-full transition-colors"
              aria-label="Dismiss"
            >
              <X className="w-4 h-4" />
            </button>
          </div>
        </motion.div>
      )}
    </AnimatePresence>
    </>
  )
}
