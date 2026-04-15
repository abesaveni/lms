import { useEffect, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { Clock, User } from 'lucide-react'
import { Card, CardHeader, CardTitle, CardContent } from '../../components/ui/Card'
import Button from '../../components/ui/Button'
import { bookSession, getSessionById, getSessionPricing, SessionDetailDto } from '../../services/sessionsApi'
import { verifySessionPayment } from '../../services/paymentsApi'

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

  useEffect(() => {
    const loadSession = async () => {
      if (!sessionId) return
      try {
        const data = await getSessionById(sessionId)
        setSession(data)

        // Auto-fill date and time from the dynamic session data
        if (data.scheduledAt) {
          const dateObj = new Date(data.scheduledAt)
          const dateStr = dateObj.toISOString().split('T')[0]
          const timeStr = dateObj.toTimeString().split(' ')[0].substring(0, 5)
          setSelectedDate(dateStr)
          setSelectedTime(timeStr)
        }
      } catch (err: any) {
        setBookingError(err.message || 'Failed to load session')
      }
    }
    loadSession()
  }, [sessionId])

  useEffect(() => {
    const loadPricing = async () => {
      if (!sessionId || !session) return
      try {
        const data = await getSessionPricing(
          sessionId,
          session.pricingType === 'Hourly' ? hours : undefined
        )
        setPricing(data)
      } catch (err: any) {
        setPricing(null)
      }
    }
    loadPricing()
  }, [sessionId, session, hours])

  const isSessionExpired = session
    ? new Date() > new Date(new Date(session.scheduledAt).getTime() + (session.duration || 60) * 60000)
    : false

  const handleBooking = async () => {
    setBookingError(null)
    if (!sessionId || !session || !pricing) {
      setBookingError('Session details missing. Please refresh the page.')
      return
    }
    if (isSessionExpired) {
      setBookingError('This session has already ended and is no longer available for booking.')
      return
    }
    setIsPaying(true)
    try {
      const booking = await bookSession({
        sessionId,
        hours: session.pricingType === 'Hourly' ? hours : undefined,
      })

      // Free session — backend already confirmed it, no payment needed
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
      amount: Math.round((booking.totalAmount || pricing?.totalAmount || 0) * 100),
      currency: 'INR',
      name: 'LiveExpert.ai',
      description: 'Session Booking',
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
      modal: {
        ondismiss: () => setIsPaying(false),
      },
    }

    const rzp = new (window as any).Razorpay(options)
    rzp.open()
  }

  const loadRazorpayScript = (): Promise<boolean> => {
    return new Promise((resolve) => {
      if ((window as any).Razorpay) {
        resolve(true)
        return
      }
      const script = document.createElement('script')
      script.src = 'https://checkout.razorpay.com/v1/checkout.js'
      script.onload = () => resolve(true)
      script.onerror = () => resolve(false)
      document.body.appendChild(script)
    })
  }

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
        <div className="mb-6 p-4 bg-green-50 border border-green-200 rounded-lg text-green-700 font-medium">
          Payment successful! Booking request submitted. Redirecting to My Sessions...
        </div>
      )}
      {bookingError && (
        <div className="mb-6 p-4 bg-red-50 border border-red-200 rounded-lg text-red-700 text-sm">
          {bookingError}
        </div>
      )}

      <div className="grid lg:grid-cols-3 gap-6">
        {/* Booking Form */}
        <div className="lg:col-span-2 space-y-6">
          <Card>
            <CardHeader>
              <CardTitle>Session Details</CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              <div>
                <h3 className="text-xl font-semibold text-gray-900 mb-2">{session?.title || 'Session'}</h3>
                <div className="flex items-center gap-4 text-sm text-gray-600">
                  <div className="flex items-center gap-1">
                    <User className="w-4 h-4" />
                    {session?.tutorName || 'Tutor'}
                  </div>
                  <div className="flex items-center gap-1">
                    <Clock className="w-4 h-4" />
                    {session?.duration ? `${session.duration} min` : '—'}
                  </div>
                </div>
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Select Date
                </label>
                <input
                  type="date"
                  value={selectedDate}
                  onChange={(e) => setSelectedDate(e.target.value)}
                  disabled={!!sessionId}
                  min={new Date().toISOString().split('T')[0]}
                  className="w-full px-4 py-2.5 rounded-lg border border-gray-300 focus:outline-none focus:ring-2 focus:ring-primary-500 bg-gray-50"
                />
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Select Time
                </label>
                <select
                  value={selectedTime}
                  onChange={(e) => setSelectedTime(e.target.value)}
                  disabled={!!sessionId}
                  className="w-full px-4 py-2.5 rounded-lg border border-gray-300 focus:outline-none focus:ring-2 focus:ring-primary-500 bg-gray-50"
                >
                  <option value="">Select time</option>
                  {!sessionId ? (
                    <>
                      <option value="10:00">10:00 AM</option>
                      <option value="14:00">2:00 PM</option>
                      <option value="16:00">4:00 PM</option>
                    </>
                  ) : (
                    <option value={selectedTime}>{selectedTime}</option>
                  )}
                </select>
              </div>

              {session?.pricingType === 'Hourly' && (
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">
                    Duration (hours)
                  </label>
                  <input
                    type="number"
                    min={1}
                    value={hours}
                    onChange={(e) => setHours(Number(e.target.value || 1))}
                    className="w-full px-4 py-2.5 rounded-lg border border-gray-300 focus:outline-none focus:ring-2 focus:ring-primary-500"
                  />
                </div>
              )}

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Special Notes (Optional)
                </label>
                <textarea
                  rows={4}
                  className="w-full px-4 py-2.5 rounded-lg border border-gray-300 focus:outline-none focus:ring-2 focus:ring-primary-500"
                  placeholder="Any special requirements or topics you'd like to focus on..."
                />
              </div>
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <CardTitle>Payment</CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="flex items-center justify-between">
                <span className="text-gray-600">Session Fee</span>
                <span className="text-lg font-bold text-gray-900">₹{pricing?.baseAmount ?? 0}</span>
              </div>
              <div className="flex items-center justify-between text-sm text-gray-600">
                <span>Platform Fee</span>
                <span>₹{pricing?.platformFee ?? 0}</span>
              </div>
              <div className="flex items-center justify-between">
                <span className="text-gray-900 font-semibold">Payable</span>
                <span className="text-lg font-bold text-gray-900">₹{pricing?.totalAmount ?? 0}</span>
              </div>
              <Button fullWidth onClick={handleBooking} disabled={isPaying || !pricing || isSessionExpired}>
                {isPaying ? 'Processing...' : 'Pay & Confirm Booking'}
              </Button>
            </CardContent>
          </Card>
        </div>

        {/* Summary */}
        <div>
          <Card className="sticky top-4">
            <CardHeader>
              <CardTitle>Booking Summary</CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="space-y-3">
                <div className="flex items-center justify-between text-sm">
                  <span className="text-gray-600">Session</span>
                  <span className="font-medium text-gray-900">{session?.title || 'Session'}</span>
                </div>
                <div className="flex items-center justify-between text-sm">
                  <span className="text-gray-600">Tutor</span>
                  <span className="font-medium text-gray-900">{session?.tutorName || 'Tutor'}</span>
                </div>
                <div className="flex items-center justify-between text-sm">
                  <span className="text-gray-600">Duration</span>
                  <span className="font-medium text-gray-900">
                    {session?.pricingType === 'Hourly' ? `${hours} hours` : `${session?.duration || 0} min`}
                  </span>
                </div>
                <div className="pt-3 border-t border-gray-200">
                  <div className="flex items-center justify-between">
                    <span className="text-gray-600">Session Fee</span>
                    <span className="text-lg font-bold text-gray-900">₹{pricing?.baseAmount ?? 0}</span>
                  </div>
                    <div className="flex flex-col gap-1 mt-2">
                      <div className="flex items-center justify-between text-sm text-gray-600">
                        <span>Platform Fee</span>
                        <span>₹{pricing?.platformFee ?? 0}</span>
                      </div>
                      {pricing?.platformFee && pricing.platformFee > 0 && (
                        <p className="text-[11px] text-red-600 font-medium">Note: This platform fee is charged only for your first session.</p>
                      )}
                    </div>
                  <div className="flex items-center justify-between mt-2">
                    <span className="text-gray-900 font-semibold">Payable</span>
                    <span className="text-lg font-bold text-gray-900">₹{pricing?.totalAmount ?? 0}</span>
                  </div>
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
