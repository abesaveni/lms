import { useState, useEffect } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { Star, Users, Clock, CheckCircle, BookOpen, User, ChevronDown, ChevronUp } from 'lucide-react'
import { Card, CardContent, CardHeader, CardTitle } from '../../components/ui/Card'
import Button from '../../components/ui/Button'
import { getCourseById, CourseDetail, SyllabusItem } from '../../services/courseApi'
import {
  checkEnrollment, createEnrollmentOrder, verifyEnrollmentPayment, bookTrial, EnrollmentCheck
} from '../../services/enrollmentApi'

const loadRazorpay = (): Promise<boolean> =>
  new Promise(resolve => {
    if ((window as any).Razorpay) { resolve(true); return }
    const s = document.createElement('script')
    s.src = 'https://checkout.razorpay.com/v1/checkout.js'
    s.onload = () => resolve(true)
    s.onerror = () => resolve(false)
    document.body.appendChild(s)
  })

const CourseDetailPage = () => {
  const navigate = useNavigate()
  const { id } = useParams()
  const [course, setCourse] = useState<CourseDetail | null>(null)
  const [enrollment, setEnrollment] = useState<EnrollmentCheck | null>(null)
  const [loading, setLoading] = useState(true)
  const [enrolling, setEnrolling] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [enrollType, setEnrollType] = useState<'Full' | 'Partial'>('Full')
  const [sessionsToPurchase, setSessionsToPurchase] = useState(5)
  const [syllabusOpen, setSyllabusOpen] = useState(false)
  const [syllabus, setSyllabus] = useState<SyllabusItem[]>([])

  useEffect(() => {
    if (!id) return
    Promise.all([
      getCourseById(id),
      checkEnrollment(id).catch(() => ({ isEnrolled: false }))
    ]).then(([c, e]) => {
      setCourse(c)
      setEnrollment(e as EnrollmentCheck)
      if (c.syllabusJson) {
        try { setSyllabus(JSON.parse(c.syllabusJson)) } catch {}
      }
    }).catch(err => setError(err.message || 'Failed to load course'))
      .finally(() => setLoading(false))
  }, [id])

  const handleEnroll = async () => {
    if (!course || !id) return
    setEnrolling(true)
    setError(null)
    try {
      const order = await createEnrollmentOrder({
        courseId: id,
        enrollmentType: enrollType,
        sessionsToPurchase: enrollType === 'Partial' ? sessionsToPurchase : 0,
      })

      const loaded = await loadRazorpay()
      if (!loaded) throw new Error('Razorpay failed to load')

      await new Promise<void>((resolve, reject) => {
        const rzp = new (window as any).Razorpay({
          key: order.keyId,
          amount: order.amount,
          currency: order.currency,
          name: 'LiveExpert.ai',
          description: order.description,
          order_id: order.orderId,
          handler: async (res: any) => {
            try {
              await verifyEnrollmentPayment({
                courseId: id,
                razorpayOrderId: res.razorpay_order_id,
                razorpayPaymentId: res.razorpay_payment_id,
                razorpaySignature: res.razorpay_signature,
                enrollmentType: enrollType,
                sessionsPurchased: order.sessionsPurchased,
                amountPaid: order.amount / 100,
              })
              resolve()
            } catch (e: any) {
              reject(e)
            }
          },
          modal: { ondismiss: () => reject(new Error('Payment cancelled')) },
        })
        rzp.open()
      })

      navigate('/student/my-enrollments')
    } catch (err: any) {
      if (err.message !== 'Payment cancelled') {
        setError(err.message || 'Enrollment failed')
      }
    } finally {
      setEnrolling(false)
    }
  }

  const handleBookTrial = async () => {
    if (!course || !id) return
    setEnrolling(true)
    try {
      const res = await bookTrial({ tutorId: course.tutor.tutorId, courseId: id })
      alert(res.message)
    } catch (err: any) {
      setError(err.message || 'Failed to book trial')
    } finally {
      setEnrolling(false)
    }
  }

  if (loading) return (
    <div className="flex items-center justify-center h-64">
      <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary-600" />
    </div>
  )

  if (!course) return (
    <div className="text-center py-16 text-gray-500">Course not found</div>
  )

  const bundleSavings = course.bundlePrice
    ? (course.pricePerSession * course.totalSessions) - course.bundlePrice
    : 0

  const partialTotal = course.pricePerSession * sessionsToPurchase

  return (
    <div className="max-w-6xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      {error && (
        <div className="mb-6 p-4 bg-red-50 border border-red-200 rounded-lg text-red-700 text-sm">{error}</div>
      )}

      <div className="grid lg:grid-cols-3 gap-8">
        {/* Main content */}
        <div className="lg:col-span-2 space-y-6">
          {/* Header */}
          <div>
            {course.thumbnailUrl ? (
              <img src={course.thumbnailUrl} alt={course.title} className="w-full h-52 object-cover rounded-xl mb-4" />
            ) : (
              <div className="w-full h-52 bg-gradient-to-br from-primary-100 to-indigo-200 rounded-xl mb-4 flex items-center justify-center">
                <BookOpen className="w-14 h-14 text-primary-400" />
              </div>
            )}
            <div className="flex flex-wrap gap-2 mb-2">
              <span className="text-xs bg-primary-100 text-primary-700 px-2 py-0.5 rounded-full">{course.level}</span>
              <span className="text-xs bg-gray-100 text-gray-600 px-2 py-0.5 rounded-full">{course.language}</span>
              <span className="text-xs bg-gray-100 text-gray-600 px-2 py-0.5 rounded-full">{course.deliveryType}</span>
              {course.subjectName && (
                <span className="text-xs bg-indigo-100 text-indigo-700 px-2 py-0.5 rounded-full">{course.subjectName}</span>
              )}
            </div>
            <h1 className="text-2xl font-bold text-gray-900 mb-2">{course.title}</h1>
            {course.shortDescription && (
              <p className="text-gray-600 mb-3">{course.shortDescription}</p>
            )}
            <div className="flex flex-wrap items-center gap-4 text-sm text-gray-500">
              {course.averageRating > 0 && (
                <span className="flex items-center gap-1">
                  <Star className="w-4 h-4 text-yellow-400 fill-yellow-400" />
                  {course.averageRating.toFixed(1)} ({course.totalReviews} reviews)
                </span>
              )}
              <span className="flex items-center gap-1">
                <Users className="w-4 h-4" /> {course.totalEnrollments} students
              </span>
              <span className="flex items-center gap-1">
                <Clock className="w-4 h-4" /> {course.totalSessions} sessions × {course.sessionDurationMinutes} min
              </span>
            </div>
          </div>

          {/* What you'll learn */}
          {course.whatYouWillLearn && (
            <Card>
              <CardHeader><CardTitle>What You'll Learn</CardTitle></CardHeader>
              <CardContent>
                <p className="text-sm text-gray-700 whitespace-pre-line">{course.whatYouWillLearn}</p>
              </CardContent>
            </Card>
          )}

          {/* Full description */}
          {course.fullDescription && (
            <Card>
              <CardHeader><CardTitle>About This Course</CardTitle></CardHeader>
              <CardContent>
                <p className="text-sm text-gray-700 whitespace-pre-line">{course.fullDescription}</p>
              </CardContent>
            </Card>
          )}

          {/* Syllabus */}
          {syllabus.length > 0 && (
            <Card>
              <CardHeader>
                <div className="flex items-center justify-between">
                  <CardTitle>Syllabus ({syllabus.length} sessions)</CardTitle>
                  <button
                    onClick={() => setSyllabusOpen(!syllabusOpen)}
                    className="flex items-center gap-1 text-sm text-primary-600"
                  >
                    {syllabusOpen ? <><ChevronUp className="w-4 h-4" /> Hide</> : <><ChevronDown className="w-4 h-4" /> Show all</>}
                  </button>
                </div>
              </CardHeader>
              <CardContent>
                <div className="space-y-2">
                  {(syllabusOpen ? syllabus : syllabus.slice(0, 3)).map(s => (
                    <div key={s.sessionNumber} className="flex gap-3 py-2 border-b last:border-0">
                      <span className="w-7 h-7 rounded-full bg-primary-100 text-primary-700 text-xs font-bold flex items-center justify-center flex-shrink-0">
                        {s.sessionNumber}
                      </span>
                      <div>
                        <p className="text-sm font-medium text-gray-800">{s.title}</p>
                        {s.topics && <p className="text-xs text-gray-500 mt-0.5">{s.topics}</p>}
                      </div>
                    </div>
                  ))}
                  {!syllabusOpen && syllabus.length > 3 && (
                    <button
                      onClick={() => setSyllabusOpen(true)}
                      className="text-sm text-primary-600 hover:underline mt-1"
                    >
                      + {syllabus.length - 3} more sessions
                    </button>
                  )}
                </div>
              </CardContent>
            </Card>
          )}

          {/* Prerequisites */}
          {course.prerequisites && (
            <Card>
              <CardHeader><CardTitle>Prerequisites</CardTitle></CardHeader>
              <CardContent>
                <p className="text-sm text-gray-700 whitespace-pre-line">{course.prerequisites}</p>
              </CardContent>
            </Card>
          )}

          {/* Tutor */}
          <Card>
            <CardHeader><CardTitle>Your Tutor</CardTitle></CardHeader>
            <CardContent>
              <div className="flex items-start gap-4">
                <div className="w-12 h-12 rounded-full bg-gray-200 flex items-center justify-center flex-shrink-0">
                  <User className="w-6 h-6 text-gray-500" />
                </div>
                <div>
                  <h4 className="font-semibold text-gray-900">{course.tutor.name}</h4>
                  {course.tutor.headline && <p className="text-sm text-gray-500">{course.tutor.headline}</p>}
                  {course.tutor.bio && <p className="text-sm text-gray-600 mt-2 line-clamp-3">{course.tutor.bio}</p>}
                  <div className="flex gap-4 mt-2 text-xs text-gray-500">
                    {course.tutor.averageRating && (
                      <span className="flex items-center gap-1">
                        <Star className="w-3.5 h-3.5 text-yellow-400 fill-yellow-400" />
                        {course.tutor.averageRating.toFixed(1)} rating
                      </span>
                    )}
                    {course.tutor.yearsOfExperience && (
                      <span>{course.tutor.yearsOfExperience} years experience</span>
                    )}
                  </div>
                </div>
              </div>
            </CardContent>
          </Card>
        </div>

        {/* Sidebar: Enrollment CTA */}
        <div className="space-y-4">
          <Card className="sticky top-6">
            <CardContent className="p-5 space-y-4">
              {enrollment?.isEnrolled ? (
                <div>
                  <div className="flex items-center gap-2 text-green-700 mb-3">
                    <CheckCircle className="w-5 h-5" />
                    <span className="font-semibold">Enrolled</span>
                  </div>
                  {enrollment.enrollment && (
                    <div className="text-sm text-gray-600 space-y-1">
                      <div className="flex justify-between">
                        <span>Sessions remaining</span>
                        <span className="font-medium">{enrollment.enrollment.sessionsRemaining}</span>
                      </div>
                      <div className="flex justify-between">
                        <span>Completed</span>
                        <span className="font-medium">{enrollment.enrollment.sessionsCompleted}</span>
                      </div>
                      {enrollment.enrollment.expiresAt && (
                        <div className="flex justify-between text-xs text-gray-400 mt-1">
                          <span>Expires</span>
                          <span>{new Date(enrollment.enrollment.expiresAt).toLocaleDateString()}</span>
                        </div>
                      )}
                    </div>
                  )}
                  <Button
                    className="w-full mt-4"
                    variant="outline"
                    onClick={() => navigate('/student/my-enrollments')}
                  >
                    Go to My Courses
                  </Button>
                </div>
              ) : (
                <>
                  {/* Enrollment type selector */}
                  <div>
                    <p className="text-sm font-medium text-gray-700 mb-2">Choose Enrollment Type</p>
                    <div className="space-y-2">
                      <label className={`flex items-start gap-3 p-3 border rounded-lg cursor-pointer ${enrollType === 'Full' ? 'border-primary-500 bg-primary-50' : 'border-gray-200'}`}>
                        <input
                          type="radio" className="mt-0.5" checked={enrollType === 'Full'}
                          onChange={() => setEnrollType('Full')}
                        />
                        <div>
                          <p className="text-sm font-medium">Full Course</p>
                          {course.bundlePrice ? (
                            <p className="text-primary-700 font-bold">₹{course.bundlePrice.toLocaleString()}</p>
                          ) : (
                            <p className="text-primary-700 font-bold">
                              ₹{(course.pricePerSession * course.totalSessions).toLocaleString()}
                            </p>
                          )}
                          {bundleSavings > 0 && (
                            <p className="text-xs text-green-600">Save ₹{bundleSavings.toLocaleString()}</p>
                          )}
                          <p className="text-xs text-gray-400">{course.totalSessions} sessions</p>
                        </div>
                      </label>

                      {course.allowPartialBooking && (
                        <label className={`flex items-start gap-3 p-3 border rounded-lg cursor-pointer ${enrollType === 'Partial' ? 'border-primary-500 bg-primary-50' : 'border-gray-200'}`}>
                          <input
                            type="radio" className="mt-0.5" checked={enrollType === 'Partial'}
                            onChange={() => setEnrollType('Partial')}
                          />
                          <div className="flex-1">
                            <p className="text-sm font-medium">Partial Booking</p>
                            {enrollType === 'Partial' && (
                              <div className="mt-2">
                                <div className="flex items-center gap-2">
                                  <input
                                    type="range"
                                    min={course.minSessionsForPartial}
                                    max={course.totalSessions - 1}
                                    value={sessionsToPurchase}
                                    onChange={e => setSessionsToPurchase(parseInt(e.target.value))}
                                    className="flex-1"
                                  />
                                  <span className="text-sm font-medium w-8">{sessionsToPurchase}</span>
                                </div>
                                <p className="text-xs text-gray-400">sessions (min {course.minSessionsForPartial})</p>
                              </div>
                            )}
                            <p className="text-primary-700 font-bold mt-1">
                              ₹{enrollType === 'Partial' ? partialTotal.toLocaleString() : (course.pricePerSession * course.minSessionsForPartial).toLocaleString()}
                            </p>
                            <p className="text-xs text-gray-400">₹{course.pricePerSession}/session</p>
                          </div>
                        </label>
                      )}
                    </div>
                  </div>

                  <Button className="w-full" onClick={handleEnroll} disabled={enrolling}>
                    {enrolling ? 'Processing...' : 'Enroll Now'}
                  </Button>

                  {course.trialAvailable && (
                    <Button variant="outline" className="w-full" onClick={handleBookTrial} disabled={enrolling}>
                      {course.trialPrice === 0
                        ? `Book Free Trial (${course.trialDurationMinutes} min)`
                        : `Book Trial — ₹${course.trialPrice} (${course.trialDurationMinutes} min)`
                      }
                    </Button>
                  )}

                  {course.refundPolicy && (
                    <p className="text-xs text-gray-400 text-center">{course.refundPolicy}</p>
                  )}
                </>
              )}
            </CardContent>
          </Card>
        </div>
      </div>
    </div>
  )
}

export default CourseDetailPage
