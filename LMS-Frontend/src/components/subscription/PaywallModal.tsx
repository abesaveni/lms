import { useEffect, useState } from 'react'
import { motion, AnimatePresence } from 'framer-motion'
import { Lock, Zap, CheckCircle, ShieldCheck, BookOpen, Brain, Star } from 'lucide-react'
import { createSubscriptionOrder, activateSubscription } from '../../services/aiApi'

declare global {
  interface Window {
    Razorpay: any
  }
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

const FEATURES = [
  { icon: BookOpen, text: 'Browse & enroll in courses' },
  { icon: Brain, text: 'AI tools — Lexi, Resume Builder, Mock Interviews' },
  { icon: Star, text: 'Session booking with expert tutors' },
  { icon: ShieldCheck, text: 'All platform features, unlimited access' },
]

export function PaywallModal() {
  const [isOpen, setIsOpen] = useState(false)
  const [paying, setPaying] = useState(false)
  const [success, setSuccess] = useState(false)
  const [errorMsg, setErrorMsg] = useState('')

  useEffect(() => {
    const handler = () => {
      setIsOpen(true)
      setSuccess(false)
      setErrorMsg('')
    }
    window.addEventListener('subscription:required', handler)
    return () => window.removeEventListener('subscription:required', handler)
  }, [])

  const handlePayNow = async () => {
    setPaying(true)
    setErrorMsg('')
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
        handler: async (response: { razorpay_order_id: string; razorpay_payment_id: string; razorpay_signature: string }) => {
          try {
            await activateSubscription({
              razorpayOrderId: response.razorpay_order_id,
              razorpayPaymentId: response.razorpay_payment_id,
              razorpaySignature: response.razorpay_signature,
            })
            setSuccess(true)
            window.dispatchEvent(new CustomEvent('subscription:activated'))
            // Close after showing success
            setTimeout(() => setIsOpen(false), 2500)
          } catch {
            setErrorMsg('Payment received but activation failed. Please contact support.')
          }
        },
        prefill: {},
        theme: { color: '#6366f1' },
        modal: {
          // Don't allow closing Razorpay checkout without paying
          ondismiss: () => setPaying(false),
          escape: false,
          backdropclose: false,
        },
      }

      const rzp = new window.Razorpay(options)
      rzp.on('payment.failed', () => {
        setErrorMsg('Payment failed. Please try again.')
        setPaying(false)
      })
      rzp.open()
    } catch (err: any) {
      setErrorMsg(err?.message || 'Could not initiate payment. Please try again.')
      setPaying(false)
    }
  }

  return (
    <AnimatePresence>
      {isOpen && (
        <motion.div
          initial={{ opacity: 0 }}
          animate={{ opacity: 1 }}
          exit={{ opacity: 0 }}
          // No onClick to close — intentionally non-dismissible
          className="fixed inset-0 z-[200] flex items-center justify-center bg-black/70 backdrop-blur-sm px-4"
        >
          <motion.div
            initial={{ scale: 0.85, opacity: 0, y: 30 }}
            animate={{ scale: 1, opacity: 1, y: 0 }}
            exit={{ scale: 0.85, opacity: 0, y: 30 }}
            transition={{ type: 'spring', damping: 25, stiffness: 280 }}
            className="bg-white rounded-2xl shadow-2xl w-full max-w-md overflow-hidden"
          >
            {/* Header — no X button */}
            <div className="bg-gradient-to-r from-indigo-600 to-purple-600 p-6 text-white">
              <div className="flex items-center gap-3">
                <div className="w-12 h-12 bg-white/20 rounded-2xl flex items-center justify-center">
                  <Lock className="w-6 h-6" />
                </div>
                <div>
                  <h2 className="text-xl font-bold">Free Trial Ended</h2>
                  <p className="text-indigo-200 text-sm">Subscribe to continue learning</p>
                </div>
              </div>
            </div>

            {/* Body */}
            <div className="p-6 space-y-5">
              {success ? (
                <motion.div
                  initial={{ opacity: 0, scale: 0.9 }}
                  animate={{ opacity: 1, scale: 1 }}
                  className="flex flex-col items-center gap-3 py-6"
                >
                  <CheckCircle className="w-16 h-16 text-green-500" />
                  <h3 className="text-lg font-bold text-gray-900">You're subscribed!</h3>
                  <p className="text-sm text-gray-500 text-center">
                    Welcome to LiveExpert Pro. Full access restored.
                  </p>
                </motion.div>
              ) : (
                <>
                  <p className="text-gray-600 text-sm text-center">
                    Your 15-day free trial has ended. Pay just <strong>₹99/month</strong> to keep full access to everything.
                  </p>

                  {/* Feature list */}
                  <div className="space-y-2">
                    {FEATURES.map(({ icon: Icon, text }) => (
                      <div key={text} className="flex items-center gap-3 text-sm text-gray-700">
                        <div className="w-7 h-7 rounded-lg bg-indigo-100 flex items-center justify-center flex-shrink-0">
                          <Icon className="w-3.5 h-3.5 text-indigo-600" />
                        </div>
                        <span>{text}</span>
                      </div>
                    ))}
                  </div>

                  {/* Price */}
                  <div className="border-2 border-indigo-200 rounded-xl p-4 bg-indigo-50 text-center">
                    <div className="text-4xl font-extrabold text-indigo-700">₹99</div>
                    <div className="text-sm text-indigo-500 font-medium">per month · cancel anytime</div>
                  </div>

                  {errorMsg && (
                    <p className="text-sm text-red-600 text-center bg-red-50 rounded-lg p-2">{errorMsg}</p>
                  )}

                  <button
                    onClick={handlePayNow}
                    disabled={paying}
                    className="w-full flex items-center justify-center gap-2 bg-gradient-to-r from-indigo-600 to-purple-600 text-white font-bold py-3.5 rounded-xl hover:opacity-90 transition-opacity disabled:opacity-60 text-base"
                  >
                    <Zap className="w-4 h-4" />
                    {paying ? 'Opening Payment…' : 'Pay ₹99 & Continue'}
                  </button>

                  <p className="text-xs text-gray-400 text-center">
                    Secured by Razorpay · 256-bit SSL encryption
                  </p>
                </>
              )}
            </div>
          </motion.div>
        </motion.div>
      )}
    </AnimatePresence>
  )
}
