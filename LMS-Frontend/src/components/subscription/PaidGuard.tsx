import { ReactNode } from 'react'
import { motion } from 'framer-motion'
import { Lock, Zap } from 'lucide-react'
import { useSubscription } from '../../hooks/useSubscription'
import { createSubscriptionOrder, activateSubscription } from '../../services/aiApi'

declare global {
  interface Window { Razorpay: any }
}

async function loadRazorpay(): Promise<void> {
  if (typeof window.Razorpay !== 'undefined') return
  return new Promise((resolve, reject) => {
    const script = document.createElement('script')
    script.src = 'https://checkout.razorpay.com/v1/checkout.js'
    script.onload = () => resolve()
    script.onerror = () => reject(new Error('Failed to load Razorpay'))
    document.body.appendChild(script)
  })
}

interface PaidGuardProps {
  children: ReactNode
  featureName: string
}

export function PaidGuard({ children, featureName }: PaidGuardProps) {
  const { trialActive, subscriptionActive, isLoading, refetch } = useSubscription()

  if (isLoading) {
    return (
      <div className="flex items-center justify-center min-h-[300px]">
        <div className="w-8 h-8 border-4 border-indigo-500 border-t-transparent rounded-full animate-spin" />
      </div>
    )
  }

  if (trialActive || subscriptionActive) {
    return <>{children}</>
  }

  // Locked — show upgrade prompt
  const handlePayNow = async () => {
    try {
      const order = await createSubscriptionOrder()
      await loadRazorpay()
      const options = {
        key: order.keyId,
        amount: order.amount,
        currency: order.currency,
        name: 'LiveExpert.AI',
        description: order.description,
        order_id: order.orderId,
        handler: async (response: any) => {
          try {
            await activateSubscription({
              razorpayOrderId: response.razorpay_order_id,
              razorpayPaymentId: response.razorpay_payment_id,
              razorpaySignature: response.razorpay_signature,
            })
            window.dispatchEvent(new CustomEvent('subscription:activated'))
            refetch()
          } catch {
            alert('Payment verified but activation failed. Please contact support.')
          }
        },
        prefill: {},
        theme: { color: '#6366f1' },
      }
      new window.Razorpay(options).open()
    } catch (err: any) {
      alert(err?.message || 'Could not initiate payment. Please try again.')
    }
  }

  return (
    <div className="flex items-center justify-center min-h-[400px] p-6">
      <motion.div
        initial={{ opacity: 0, scale: 0.9 }}
        animate={{ opacity: 1, scale: 1 }}
        className="bg-white rounded-2xl shadow-xl border border-gray-100 p-10 max-w-md w-full text-center"
      >
        <div className="w-16 h-16 bg-indigo-100 rounded-2xl flex items-center justify-center mx-auto mb-5">
          <Lock className="w-8 h-8 text-indigo-600" />
        </div>
        <h2 className="text-xl font-bold text-gray-900 mb-2">Feature Locked</h2>
        <p className="text-gray-500 text-sm mb-1">{featureName} requires an active subscription.</p>
        <p className="text-gray-400 text-xs mb-6">Your 15-day free trial has ended.</p>
        <button
          onClick={handlePayNow}
          className="w-full flex items-center justify-center gap-2 bg-gradient-to-r from-indigo-600 to-purple-600 text-white font-bold py-3 rounded-xl hover:opacity-90 transition-opacity"
        >
          <Zap className="w-4 h-4" />
          Pay ₹100/month to Unlock
        </button>
        <p className="text-xs text-gray-400 mt-3">Secured by Razorpay · Cancel anytime</p>
      </motion.div>
    </div>
  )
}
