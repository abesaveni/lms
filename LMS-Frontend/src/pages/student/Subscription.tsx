import { useState } from 'react'
import { CreditCard, CheckCircle2, Clock, Zap, Loader2, RefreshCw } from 'lucide-react'
import { useSubscription } from '../../hooks/useSubscription'
import { createSubscriptionOrder, activateSubscription } from '../../services/aiApi'

declare global {
  interface Window { Razorpay: any }
}

const FEATURES = [
  'AI Chatbot Lexi — instant learning help',
  'Resume Builder — ATS-optimised resumes',
  'Mock Interview — 10-question AI sessions',
  'Career Roadmap — 6-month personalised plan',
  'LinkedIn Optimizer — recruiter-ready profile',
  'Project Ideas — 5 portfolio projects with build guides',
  'Code Reviewer — bugs, improvements & score',
  'Portfolio Generator — full HTML portfolio',
  'Daily Quiz — 10 MCQs on any subject',
  'Smart Flashcards — interactive flip cards',
  'Assignment Helper — guided approach',
  'Study Scheduler — day-by-day timetable',
  'Wellness Check-in — energy, stress & tips',
]

function loadRazorpay(): Promise<boolean> {
  return new Promise(resolve => {
    if (window.Razorpay) { resolve(true); return }
    const script = document.createElement('script')
    script.src = 'https://checkout.razorpay.com/v1/checkout.js'
    script.onload = () => resolve(true)
    script.onerror = () => resolve(false)
    document.body.appendChild(script)
  })
}

export default function Subscription() {
  const { trialActive, daysLeftInTrial, subscriptionActive, subscribedUntil, isLoading, refetch } = useSubscription()
  const [paying, setPaying] = useState(false)
  const [error, setError] = useState('')

  const handleSubscribe = async () => {
    setPaying(true); setError('')
    try {
      const loaded = await loadRazorpay()
      if (!loaded) { setError('Payment gateway failed to load. Please try again.'); return }
      const order = await createSubscriptionOrder()
      await new Promise<void>((resolve, reject) => {
        const rzp = new window.Razorpay({
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
              resolve()
            } catch (e: any) { reject(e) }
          },
          modal: { ondismiss: () => reject(new Error('Payment cancelled')) },
          theme: { color: '#6366f1' },
        })
        rzp.open()
      })
    } catch (e: any) {
      if (e.message !== 'Payment cancelled') setError(e.message)
    } finally {
      setPaying(false)
    }
  }

  if (isLoading) {
    return (
      <div className="p-6 max-w-2xl mx-auto flex items-center justify-center min-h-[300px]">
        <Loader2 className="w-8 h-8 text-indigo-500 animate-spin" />
      </div>
    )
  }

  return (
    <div className="p-6 max-w-2xl mx-auto">
      <div className="flex items-center gap-3 mb-8">
        <div className="w-10 h-10 bg-indigo-100 rounded-xl flex items-center justify-center">
          <CreditCard className="w-5 h-5 text-indigo-600" />
        </div>
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Subscription</h1>
          <p className="text-sm text-gray-500">Manage your LiveExpert.AI access</p>
        </div>
        <button onClick={refetch} className="ml-auto text-gray-400 hover:text-gray-600 transition-colors">
          <RefreshCw className="w-4 h-4" />
        </button>
      </div>

      {/* Status Card */}
      <div className={`rounded-2xl border p-6 mb-6 ${subscriptionActive ? 'bg-green-50 border-green-200' : trialActive ? 'bg-amber-50 border-amber-200' : 'bg-red-50 border-red-200'}`}>
        <div className="flex items-start gap-4">
          <div className={`w-12 h-12 rounded-xl flex items-center justify-center flex-shrink-0 ${subscriptionActive ? 'bg-green-100' : trialActive ? 'bg-amber-100' : 'bg-red-100'}`}>
            {subscriptionActive ? <CheckCircle2 className="w-6 h-6 text-green-600" /> : trialActive ? <Clock className="w-6 h-6 text-amber-600" /> : <Zap className="w-6 h-6 text-red-500" />}
          </div>
          <div>
            {subscriptionActive ? (
              <>
                <p className="font-bold text-green-800 text-lg">Active Subscription</p>
                <p className="text-green-700 text-sm mt-1">You have full access to all 13 AI tools.</p>
                {subscribedUntil && <p className="text-green-600 text-xs mt-1.5 font-medium">Valid until {new Date(subscribedUntil).toLocaleDateString('en-IN', { day: 'numeric', month: 'long', year: 'numeric' })}</p>}
              </>
            ) : trialActive ? (
              <>
                <p className="font-bold text-amber-800 text-lg">Free Trial Active</p>
                <p className="text-amber-700 text-sm mt-1">You have <strong>{daysLeftInTrial} day{daysLeftInTrial !== 1 ? 's' : ''}</strong> left in your free trial.</p>
                <p className="text-amber-600 text-xs mt-1.5">Subscribe before it expires to keep your access.</p>
              </>
            ) : (
              <>
                <p className="font-bold text-red-700 text-lg">Trial Expired</p>
                <p className="text-red-600 text-sm mt-1">Your free trial has ended. Subscribe to regain full access.</p>
              </>
            )}
          </div>
        </div>
      </div>

      {/* Plan Card */}
      {!subscriptionActive && (
        <div className="bg-white rounded-2xl border border-gray-100 shadow-sm overflow-hidden mb-6">
          <div className="bg-gradient-to-r from-indigo-600 to-purple-600 p-6 text-white text-center">
            <p className="text-xs font-bold uppercase tracking-wider opacity-80 mb-1">LiveExpert.AI Pro</p>
            <p className="text-5xl font-black">₹100</p>
            <p className="opacity-80 text-sm mt-1">per month · cancel anytime</p>
          </div>
          <div className="p-6">
            <p className="text-sm font-semibold text-gray-700 mb-4">Everything included:</p>
            <ul className="space-y-2.5 mb-6">
              {FEATURES.map((f, i) => (
                <li key={i} className="flex items-start gap-2.5 text-sm text-gray-700">
                  <CheckCircle2 className="w-4 h-4 text-indigo-500 flex-shrink-0 mt-0.5" />
                  {f}
                </li>
              ))}
            </ul>
            {error && <div className="bg-red-50 text-red-700 rounded-xl p-3 text-sm mb-4">{error}</div>}
            <button onClick={handleSubscribe} disabled={paying} className="w-full flex items-center justify-center gap-2 bg-indigo-600 hover:bg-indigo-700 text-white font-bold py-4 rounded-xl transition-colors disabled:opacity-60 text-base">
              {paying ? <><Loader2 className="w-5 h-5 animate-spin" />Processing...</> : <><Zap className="w-5 h-5" />Subscribe for ₹100/month</>}
            </button>
            <p className="text-center text-xs text-gray-400 mt-3">Secure payment via Razorpay · UPI, Cards, Net Banking</p>
          </div>
        </div>
      )}

      {/* Active subscription renewal */}
      {subscriptionActive && (
        <div className="bg-white rounded-2xl border border-gray-100 p-5 shadow-sm">
          <p className="font-semibold text-gray-800 mb-1">Renew Subscription</p>
          <p className="text-sm text-gray-500 mb-4">Pay ₹100 to extend your access by another month.</p>
          {error && <div className="bg-red-50 text-red-700 rounded-xl p-3 text-sm mb-4">{error}</div>}
          <button onClick={handleSubscribe} disabled={paying} className="flex items-center gap-2 bg-indigo-600 hover:bg-indigo-700 text-white font-semibold px-5 py-2.5 rounded-xl transition-colors disabled:opacity-60">
            {paying ? <><Loader2 className="w-4 h-4 animate-spin" />Processing...</> : 'Renew — ₹100'}
          </button>
        </div>
      )}
    </div>
  )
}
