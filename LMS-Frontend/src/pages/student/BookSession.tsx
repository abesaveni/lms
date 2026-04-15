import { useEffect, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { Clock, User, Tag, Gift, Zap, CheckCircle2, AlertCircle } from 'lucide-react'
import { Card, CardHeader, CardTitle, CardContent } from '../../components/ui/Card'
import Button from '../../components/ui/Button'
import { bookSession, getSessionById, getSessionPricing, SessionDetailDto } from '../../services/sessionsApi'
import { validateCoupon, CouponValidationResult } from '../../services/couponsApi'
import { verifySessionPayment } from '../../services/paymentsApi'
import { getBonusPointsSummary } from '../../services/bonusPointsApi'

const BookSession = () => {
  const navigate = useNavigate()
  const { sessionId } = useParams()
  const [selectedDate, setSelectedDate] = useState('')
  const [selectedTime, setSelectedTime] = useState('')
  const [session, setSession] = useState<SessionDetailDto | null>(null)
  const [hours, setHours] = useState(1)
  const [pricing, setPricing] = useState<{ baseAmount: number; platformFee: number; totalAmount: number } | null>(null)
  const [isPaying, setIsPaying] = useState(false)
  const [bookingError, setBookingError] = useState<string | null>(null)
  const [bookingSuccess, setBookingSuccess] = useState(false)

  // Coupon state
  const [couponInput, setCouponInput] = useState('')
  const [couponValidating, setCouponValidating] = useState(false)
  const [couponResult, setCouponResult] = useState<CouponValidationResult | null>(null)
  const [couponError, setCouponError] = useState<string | null>(null)

  // Bonus points state
  const [availablePoints, setAvailablePoints] = useState(0)
  const [usePoints, setUsePoints] = useState(false)
  const [specialInstructions, setSpecialInstructions] = useState('')
  const [goals, setGoals] = useState('')

  useEffect(() => {
    const loadSession = async () => {
      if (!sessionId) return
      try {
        const data = await getSessionById(sessionId)
        setSession(data)
        if (data.scheduledAt) {
          const dateObj = new Date(data.scheduledAt)
          setSelectedDate(dateObj.toISOString().split('T')[0])
          setSelectedTime(dateObj.toTimeString().split(' ')[0].substring(0, 5))
        }
      } catch (err: any) {
        setBookingError(err.message || 'Failed to load session')
      }
    }
    const loadPoints = async () => {
      try {
        const summary = await getBonusPointsSummary()
        setAvailablePoints(summary.totalPoints || 0)
      } catch { /* no points = 0 */ }
    }
    loadSession()
    loadPoints()
  }, [sessionId])

  useEffect(() => {
    const loadPricing = async () => {
      if (!sessionId || !session) return
      try {
        const data = await getSessionPricing(sessionId, session.pricingType === 'Hourly' ? hours : undefined)
        setPricing(data)
      } catch { setPricing(null) }
    }
    loadPricing()
  }, [sessionId, session, hours])

  const isSessionExpired = session
    ? new Date() > new Date(new Date(session.scheduledAt).getTime() + (session.duration || 60) * 60000)
    : false

  // Flash sale helpers
  const isFlashSaleActive = session?.flashSalePrice != null && session.flashSaleEndsAt
    ? new Date(session.flashSaleEndsAt) > new Date()
    : false
  const effectiveBasePrice = isFlashSaleActive ? (session!.flashSalePrice ?? session!.basePrice) : (session?.basePrice ?? 0)

  // Final amount calculation (coupon + points)
  const baseForCalc = pricing?.baseAmount ?? effectiveBasePrice
  const platformFee = pricing?.platformFee ?? 0
  const couponDiscount = couponResult?.isValid ? couponResult.discountAmount : 0
  const pointsDiscount = usePoints ? Math.min(availablePoints, baseForCalc) : 0
  const finalAmount = Math.max(0, baseForCalc + platformFee - couponDiscount - pointsDiscount)

  const handleValidateCoupon = async () => {
    if (!couponInput.trim()) return
    setCouponValidating(true)
    setCouponError(null)
    setCouponResult(null)
    try {
      const result = await validateCoupon(couponInput, baseForCalc, session?.tutorId)
      setCouponResult(result)
      if (!result.isValid) setCouponError(result.message)
    } catch (err: any) {
      setCouponError(err.message || 'Failed to validate coupon')
    } finally {
      setCouponValidating(false)
    }
  }

  const handleBooking = async () => {
    setBookingError(null)
    if (!sessionId || !session) {
      setBookingError('Session details missing. Please refresh.')
      return
    }
    if (isSessionExpired) {
      setBookingError('This session has already ended and is no longer bookable.')
      return
    }
    setIsPaying(true)
    try {
      const booking = await bookSession({
        sessionId,
        hours: session.pricingType === 'Hourly' ? hours : undefined,
        usePoints,
        couponCode: couponResult?.isValid ? couponInput : undefined,
        specialInstructions: specialInstructions || undefined,
        goals: goals || undefined,
      })

      // Free / fully-covered booking
      if (!booking.razorpayOrderId) {
        setBookingSuccess(true)
        setTimeout(() => navigate('/student/my-sessions'), 2000)
        setIsPaying(false)
        return
      }

      await openRazorpayCheckout(booking)
    } catch (err: any) {
      setBookingError(err.message || 'Failed to book session')
      setIsPaying(false)
    }
  }

  const openRazorpayCheckout = async (booking: any) => {
    const loaded = await loadRazorpayScript()
    if (!loaded) {
      setBookingError('Failed to load payment gateway. Please try again.')
      setIsPaying(false)
      return
    }

    const options = {
      key: booking.razorpayKey,
      amount: Math.round(finalAmount * 100),
      currency: 'INR',
      name: 'LiveExpert.ai',
      description: session?.title || 'Session Booking',
      order_id: booking.razorpayOrderId,
      handler: async (response: any) => {
        try {
          await verifySessionPayment({
            razorpayOrderId: response.razorpay_order_id,
            razorpayPaymentId: response.razorpay_payment_id,
            razorpaySignature: response.razorpay_signature,
          })
          setBookingSuccess(true)
          setTimeout(() => navigate('/student/my-sessions'), 2000)
        } catch (err: any) {
          setBookingError(err.message || 'Payment verification failed')
        } finally {
          setIsPaying(false)
        }
      },
      modal: { ondismiss: () => setIsPaying(false) },
    }

    const rzp = new (window as any).Razorpay(options)
    rzp.open()
  }

  const loadRazorpayScript = (): Promise<boolean> =>
    new Promise((resolve) => {
      if ((window as any).Razorpay) { resolve(true); return }
      const script = document.createElement('script')
      script.src = 'https://checkout.razorpay.com/v1/checkout.js'
      script.onload = () => resolve(true)
      script.onerror = () => resolve(false)
      document.body.appendChild(script)
    })

  if (!sessionId) {
    return (
      <div className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 py-20 text-center">
        <p className="text-gray-600 mb-4">No session selected.</p>
        <Button onClick={() => navigate('/student/find-tutors')}>Browse Sessions</Button>
      </div>
    )
  }

  return (
    <div className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <div className="mb-8">
        <h1 className="text-3xl font-bold text-gray-900 mb-2">Book Session</h1>
        <p className="text-gray-600">Complete your booking details</p>
      </div>

      {bookingSuccess && (
        <div className="mb-6 p-4 bg-green-50 border border-green-200 rounded-xl text-green-700 font-semibold flex items-center gap-2">
          <CheckCircle2 className="w-5 h-5" />
          Booking confirmed! Redirecting to My Sessions…
        </div>
      )}
      {bookingError && (
        <div className="mb-6 p-4 bg-red-50 border border-red-200 rounded-xl text-red-700 text-sm flex items-center gap-2">
          <AlertCircle className="w-4 h-4 flex-shrink-0" />
          {bookingError}
        </div>
      )}

      {/* Special indicators */}
      {isFlashSaleActive && (
        <div className="mb-4 p-3 bg-orange-50 border border-orange-200 rounded-xl flex items-center gap-2 text-orange-700 text-sm font-semibold">
          <Zap className="w-4 h-4 fill-orange-500" />
          Flash Sale! Price ₹{session!.flashSalePrice} (was ₹{session!.basePrice}) — ends {new Date(session!.flashSaleEndsAt!).toLocaleTimeString()}
        </div>
      )}
      {session?.instantBooking && (
        <div className="mb-4 p-3 bg-indigo-50 border border-indigo-200 rounded-xl flex items-center gap-2 text-indigo-700 text-sm font-semibold">
          <Zap className="w-4 h-4" />
          Instant Booking — your spot is confirmed immediately after payment.
        </div>
      )}

      <div className="grid lg:grid-cols-3 gap-6">
        {/* Form */}
        <div className="lg:col-span-2 space-y-5">

          {/* Session Info */}
          <Card>
            <CardHeader><CardTitle>Session Details</CardTitle></CardHeader>
            <CardContent className="space-y-4">
              <div>
                <h3 className="text-xl font-semibold text-gray-900 mb-1">{session?.title || 'Session'}</h3>
                <div className="flex items-center gap-4 text-sm text-gray-500">
                  <span className="flex items-center gap-1"><User className="w-4 h-4" />{session?.tutorName || 'Tutor'}</span>
                  <span className="flex items-center gap-1"><Clock className="w-4 h-4" />{session?.duration ? `${session.duration} min` : '—'}</span>
                </div>
              </div>

              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1.5">Date</label>
                  <input type="date" value={selectedDate}
                    onChange={(e) => setSelectedDate(e.target.value)}
                    disabled={!!sessionId}
                    min={new Date().toISOString().split('T')[0]}
                    className="w-full px-3 py-2.5 rounded-lg border border-gray-300 focus:outline-none focus:ring-2 focus:ring-indigo-400 bg-gray-50 text-sm"
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1.5">Time</label>
                  <input type="text" value={selectedTime} readOnly
                    className="w-full px-3 py-2.5 rounded-lg border border-gray-200 bg-gray-50 text-sm text-gray-600"
                  />
                </div>
              </div>

              {session?.pricingType === 'Hourly' && (
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1.5">Duration (hours)</label>
                  <input type="number" min={1} value={hours}
                    onChange={(e) => setHours(Number(e.target.value || 1))}
                    className="w-full px-3 py-2.5 rounded-lg border border-gray-300 focus:outline-none focus:ring-2 focus:ring-indigo-400 text-sm"
                  />
                </div>
              )}

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1.5">Learning Goals (optional)</label>
                <input type="text" placeholder="What do you want to learn?"
                  value={goals} onChange={(e) => setGoals(e.target.value)}
                  className="w-full px-3 py-2.5 rounded-lg border border-gray-300 focus:outline-none focus:ring-2 focus:ring-indigo-400 text-sm"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1.5">Special Notes (optional)</label>
                <textarea rows={3}
                  value={specialInstructions} onChange={(e) => setSpecialInstructions(e.target.value)}
                  className="w-full px-3 py-2.5 rounded-lg border border-gray-300 focus:outline-none focus:ring-2 focus:ring-indigo-400 text-sm"
                  placeholder="Any special requirements or topics to focus on…"
                />
              </div>
            </CardContent>
          </Card>

          {/* Discounts */}
          <Card>
            <CardHeader><CardTitle>Discounts & Savings</CardTitle></CardHeader>
            <CardContent className="space-y-4">

              {/* Coupon Code */}
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1.5 flex items-center gap-1.5">
                  <Tag className="w-3.5 h-3.5 text-indigo-500" />
                  Coupon Code
                </label>
                <div className="flex gap-2">
                  <input
                    type="text"
                    placeholder="Enter promo code (e.g. WELCOME20)"
                    value={couponInput}
                    onChange={(e) => { setCouponInput(e.target.value.toUpperCase()); setCouponResult(null); setCouponError(null) }}
                    className="flex-1 px-3 py-2.5 rounded-lg border border-gray-300 focus:outline-none focus:ring-2 focus:ring-indigo-400 text-sm font-mono uppercase"
                  />
                  <Button size="sm" variant="outline" onClick={handleValidateCoupon} isLoading={couponValidating}
                    disabled={!couponInput.trim()}>
                    Apply
                  </Button>
                </div>
                {couponResult?.isValid && (
                  <p className="text-xs text-green-600 font-semibold mt-1.5 flex items-center gap-1">
                    <CheckCircle2 className="w-3.5 h-3.5" />{couponResult.message}
                  </p>
                )}
                {couponError && !couponResult?.isValid && (
                  <p className="text-xs text-red-500 mt-1.5">{couponError}</p>
                )}
              </div>

              {/* Bonus Points */}
              {availablePoints > 0 && (
                <label className="flex items-center gap-3 cursor-pointer p-3 rounded-xl border border-gray-100 bg-amber-50/50 hover:bg-amber-50 transition-colors">
                  <input type="checkbox" checked={usePoints} onChange={(e) => setUsePoints(e.target.checked)}
                    className="w-4 h-4 rounded border-gray-300 text-indigo-600 focus:ring-indigo-400"
                  />
                  <div className="flex items-center gap-2">
                    <Gift className="w-4 h-4 text-amber-500" />
                    <span className="text-sm font-semibold text-gray-800">
                      Use {availablePoints} bonus points (saves ₹{Math.min(availablePoints, baseForCalc)})
                    </span>
                  </div>
                </label>
              )}

            </CardContent>
          </Card>

          {/* Payment CTA */}
          <Card>
            <CardHeader><CardTitle>Payment</CardTitle></CardHeader>
            <CardContent className="space-y-3">
              <div className="flex justify-between text-sm text-gray-600">
                <span>Session Fee {isFlashSaleActive && <span className="text-orange-500 font-bold ml-1">🔥 Sale</span>}</span>
                <span className="font-semibold text-gray-900">
                  {isFlashSaleActive
                    ? <><s className="text-gray-400 mr-1">₹{session!.basePrice}</s> ₹{session!.flashSalePrice}</>
                    : `₹${pricing?.baseAmount ?? effectiveBasePrice}`}
                </span>
              </div>
              {platformFee > 0 && (
                <div className="flex justify-between text-xs text-gray-400">
                  <span>Platform Fee (first booking)</span>
                  <span>₹{platformFee}</span>
                </div>
              )}
              {couponDiscount > 0 && (
                <div className="flex justify-between text-xs text-green-600 font-semibold">
                  <span>Coupon Discount ({couponInput})</span>
                  <span>-₹{couponDiscount.toFixed(2)}</span>
                </div>
              )}
              {pointsDiscount > 0 && (
                <div className="flex justify-between text-xs text-amber-600 font-semibold">
                  <span>Bonus Points</span>
                  <span>-₹{pointsDiscount}</span>
                </div>
              )}
              <div className="flex justify-between text-base font-bold text-gray-900 border-t border-gray-100 pt-3">
                <span>Total Payable</span>
                <span>₹{finalAmount.toFixed(2)}</span>
              </div>
              <Button fullWidth onClick={handleBooking}
                disabled={isPaying || !session || isSessionExpired}
                className="mt-1">
                {isPaying ? 'Processing…' : finalAmount === 0 ? 'Confirm Free Booking' : `Pay ₹${finalAmount.toFixed(2)}`}
              </Button>
            </CardContent>
          </Card>
        </div>

        {/* Summary sidebar */}
        <div>
          <Card className="sticky top-4">
            <CardHeader><CardTitle>Summary</CardTitle></CardHeader>
            <CardContent className="space-y-3 text-sm">
              <div className="flex justify-between">
                <span className="text-gray-500">Session</span>
                <span className="font-semibold text-right max-w-[140px] truncate">{session?.title || '—'}</span>
              </div>
              <div className="flex justify-between">
                <span className="text-gray-500">Tutor</span>
                <span className="font-semibold">{session?.tutorName || '—'}</span>
              </div>
              <div className="flex justify-between">
                <span className="text-gray-500">Duration</span>
                <span className="font-semibold">
                  {session?.pricingType === 'Hourly' ? `${hours} hr` : `${session?.duration || 0} min`}
                </span>
              </div>
              <div className="flex justify-between">
                <span className="text-gray-500">Date</span>
                <span className="font-semibold">{selectedDate || '—'}</span>
              </div>
              {session?.requiresSubscription && (
                <div className="p-2 bg-indigo-50 rounded-lg text-indigo-700 text-xs font-medium">
                  Subscription required for this session.
                </div>
              )}
              <div className="border-t border-gray-100 pt-3">
                <div className="flex justify-between font-bold text-gray-900">
                  <span>Total</span>
                  <span>₹{finalAmount.toFixed(2)}</span>
                </div>
              </div>
            </CardContent>
          </Card>
        </div>
      </div>
    </div>
  )
}

export default BookSession
