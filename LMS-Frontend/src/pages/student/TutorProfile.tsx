import { useEffect, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { Star, MapPin, Calendar, MessageCircle, Clock, Loader2 } from 'lucide-react'
import Button from '../../components/ui/Button'
import { Card, CardHeader, CardTitle, CardContent } from '../../components/ui/Card'
import { Avatar } from '../../components/ui/Avatar'
import { Badge } from '../../components/ui/Badge'
import { getTutorReviews, ReviewDto } from '../../services/reviewsApi'
import { useRef } from 'react'
import { Tabs, TabsList, TabsTrigger, TabsContent } from '../../components/ui/Tabs'
import { createChatRequest } from '../../services/chatRequestsApi'
import { followTutor, unfollowTutor, getFollowStatus } from '../../services/followersApi'
import { getTutorById, TutorDto } from '../../services/tutorsApi'
import { getCurrentUser } from '../../utils/auth'
import { getSessions, SessionDto } from '../../services/sessionsApi'
import { getCoursesByTutor, SubjectRate } from '../../services/courseApi'

const TutorProfile = () => {
  const navigate = useNavigate()
  const { id } = useParams()
  const [tutor, setTutor] = useState<TutorDto | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [isFollowing, setIsFollowing] = useState(false)
  const [followerCount, setFollowerCount] = useState(0)
  const [sessions, setSessions] = useState<SessionDto[]>([])
  const [reviews, setReviews] = useState<ReviewDto[]>([])
  const [subjectRates, setSubjectRates] = useState<SubjectRate[]>([])
  const [activeTab, setActiveTab] = useState('about')
  const [profileNotice, setProfileNotice] = useState<{ type: 'info' | 'error'; text: string } | null>(null)
  const availabilityRef = useRef<HTMLDivElement>(null)
  const user = getCurrentUser()
  const isAuthenticated = !!user

  useEffect(() => {
    const loadTutor = async () => {
      if (!id) {
        setError('Tutor not found')
        setIsLoading(false)
        return
      }
      try {
        setIsLoading(true)
        setError(null)
        const data = await getTutorById(id)
        setTutor(data)
        setFollowerCount(data.followerCount || 0)
        
        // Fetch tutor's upcoming sessions using UserId (database link for sessions)
        const sessionData = await getSessions({ tutorId: data.userId, upcoming: true })
        setSessions(sessionData.items)

        // Fetch subject rates
        try {
          const ratesData = await getCoursesByTutor(data.userId)
          setSubjectRates(ratesData.subjectRates || [])
        } catch {
          // Non-critical
        }

        // Fetch tutor's reviews - Use UserId as backend expects it for followers/reviews
        const reviewData = await getTutorReviews(data.userId)
        setReviews(reviewData.items)
        
        // Load follow status using UserId (data.userId is the database link for followers)
        if (isAuthenticated) {
          try {
            const status = await getFollowStatus(data.userId)
            setIsFollowing(status)
          } catch (followErr) {
            console.error('Failed to load follow status:', followErr)
          }
        }
      } catch (err: any) {
        setError(err.message || 'Failed to load tutor')
      } finally {
        setIsLoading(false)
      }
    }

    loadTutor()
  }, [id, isAuthenticated])

  const handleBookSession = () => {
    if (!isAuthenticated) {
      if (confirm('Please login to book sessions. Would you like to login now?')) {
        navigate('/login')
      }
      return
    }
    // Scroll to availability tab if sessions exist
    if (sessions.length > 0) {
      setActiveTab('availability')
      setTimeout(() => {
        availabilityRef.current?.scrollIntoView({ behavior: 'smooth', block: 'start' })
      }, 100)
    } else {
      setProfileNotice({ type: 'info', text: 'This tutor has no upcoming sessions scheduled. Please message them to request a session.' })
    }
  }

  const handleMessage = () => {
    if (!isAuthenticated) {
      if (confirm('Please login to message tutors. Would you like to login now?')) {
        navigate('/login')
      }
      return
    }
    if (!tutor?.userId) {
      setProfileNotice({ type: 'error', text: 'Tutor details are unavailable. Please try again.' })
      return
    }
    ;(async () => {
      try {
        await createChatRequest(tutor.userId)
        navigate('/student/inbox')
      } catch (err: any) {
        setProfileNotice({ type: 'error', text: err.message || 'Failed to send chat request' })
      }
    })()
  }

  const handleFollowToggle = async () => {
    if (!isAuthenticated) {
      if (confirm('Please login to follow tutors. Would you like to login now?')) {
        navigate('/login')
      }
      return
    }
    if (!tutor?.userId) return

    try {
      if (isFollowing) {
        await unfollowTutor(tutor.userId)
        setIsFollowing(false)
        setFollowerCount((prev) => Math.max(0, prev - 1))
      } else {
        await followTutor(tutor.userId)
        setIsFollowing(true)
        setFollowerCount((prev) => prev + 1)
      }
    } catch (err: any) {
      setProfileNotice({ type: 'error', text: err.message || 'Failed to update follow status' })
    }
  }

  if (isLoading) {
    return (
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        <div className="flex items-center justify-center p-12 text-gray-600">
          <Loader2 className="w-6 h-6 animate-spin mr-2" />
          Loading tutor profile...
        </div>
      </div>
    )
  }

  if (error || !tutor) {
    return (
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        <div className="rounded-lg border border-red-200 bg-red-50 p-4 text-red-700">
          {error || 'Tutor not found'}
        </div>
      </div>
    )
  }

  // const staticReviews = [
  //   {
  //   name: 'Alex Thompson',
  //     rating: 5,
  //     review: 'Sarah is an amazing tutor! She explains complex concepts clearly and is very patient.',
  //     date: '2 weeks ago',
  //   },
  //   {
  //     name: 'Maria Garcia',
  //     rating: 5,
  //     review: 'Best tutor I\'ve had. Her teaching style is perfect for beginners.',
  //     date: '1 month ago',
  //   },
  // ]

  const rating = tutor.averageRating ?? 0
  const totalReviews = tutor.totalReviews ?? 0
  const subjects = tutor.subjects ?? []
  const isAvailable = tutor.available ?? true

  return (
    <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      {profileNotice && (
        <div className={`mb-4 p-4 rounded-lg text-sm font-medium ${profileNotice.type === 'info' ? 'bg-blue-50 text-blue-700 border border-blue-200' : 'bg-red-50 text-red-700 border border-red-200'}`}>
          {profileNotice.text}
        </div>
      )}
      {/* Tutor Header */}
      <Card className="mb-8">
        <div className="flex flex-col md:flex-row gap-6">
          <Avatar name={tutor.name} src={tutor.profileImage} size="xl" />
          <div className="flex-1">
            <div className="flex items-start justify-between mb-4">
              <div>
                <h1 className="text-3xl font-bold text-gray-900 mb-2">{tutor.name}</h1>
                <div className="flex items-center gap-4 mb-2">
                  <div className="flex items-center gap-1">
                    <Star className="w-5 h-5 fill-yellow-400 text-yellow-400" />
                    <span className="font-semibold">{rating.toFixed(1)}</span>
                    <span className="text-gray-600">({totalReviews} reviews)</span>
                  </div>
                  <div className="text-gray-600 text-sm">
                    Followers: <span className="font-semibold">{followerCount}</span>
                  </div>
                  <div className="flex items-center gap-1 text-gray-600">
                    <MapPin className="w-4 h-4" />
                    {tutor.location || '-'}
                  </div>
                </div>
                <div className="flex flex-wrap gap-2 mt-3">
                  {subjects.length > 0 ? (
                    subjects.map((subject, idx) => (
                      <Badge key={idx} variant="info">{subject}</Badge>
                    ))
                  ) : (
                    <Badge variant="default">General</Badge>
                  )}
                </div>
              </div>
              {isAvailable && <Badge variant="success">Available Now</Badge>}
            </div>
            <div className="flex gap-4">
              <Button onClick={handleBookSession}>
                <Calendar className="mr-2 w-5 h-5" />
                Book Session
              </Button>
              <Button variant="outline" onClick={handleMessage}>
                <MessageCircle className="mr-2 w-5 h-5" />
                Message
              </Button>
              <Button variant={isFollowing ? 'secondary' : 'outline'} onClick={handleFollowToggle}>
                {isFollowing ? 'Following' : 'Follow Tutor'}
              </Button>
            </div>
          </div>
        </div>
      </Card>

      <div className="grid lg:grid-cols-3 gap-8">
        {/* Main Content */}
        <div className="lg:col-span-2 space-y-6">
          <Tabs defaultValue="about" value={activeTab} onValueChange={setActiveTab}>
            <TabsList>
              <TabsTrigger value="about">About</TabsTrigger>
              <TabsTrigger value="reviews">Reviews ({totalReviews})</TabsTrigger>
              <TabsTrigger value="availability">Availability</TabsTrigger>
            </TabsList>

            <TabsContent value="about">
              <Card>
                <CardHeader>
                  <CardTitle>About</CardTitle>
                </CardHeader>
                <CardContent className="space-y-6">
                  <div>
                    <h3 className="font-semibold mb-2">Bio</h3>
                    <p className="text-gray-700">{tutor.bio || '-'}</p>
                  </div>
                  <div>
                    <h3 className="font-semibold mb-2">Education / Headline</h3>
                    <p className="text-gray-700">{tutor.headline || '-'}</p>
                  </div>
                  <div>
                    <h3 className="font-semibold mb-2">Experience</h3>
                    <p className="text-gray-700">
                      {tutor.yearsOfExperience ? `${tutor.yearsOfExperience}+ years` : '-'}
                    </p>
                  </div>
                </CardContent>
              </Card>
            </TabsContent>

            <TabsContent value="reviews">
              <Card>
                <CardHeader>
                  <CardTitle>Reviews</CardTitle>
                </CardHeader>
                <CardContent>
                  <div className="space-y-6">
                    {reviews.length > 0 ? (
                      reviews.map((review, idx) => (
                        <div key={idx} className="border-b border-gray-200 pb-6 last:border-0 last:pb-0">
                          <div className="flex items-start justify-between mb-2">
                            <div className="flex items-center gap-3">
                              <Avatar name={review.studentName} src={review.studentImage} size="sm" />
                              <div>
                                <h4 className="font-semibold">{review.studentName}</h4>
                                <div className="flex gap-1 mt-1">
                                  {[...Array(review.rating)].map((_, i) => (
                                    <Star key={i} className="w-4 h-4 fill-yellow-400 text-yellow-400" />
                                  ))}
                                </div>
                              </div>
                            </div>
                            <span className="text-sm text-gray-500">{new Date(review.createdAt).toLocaleDateString()}</span>
                          </div>
                          <p className="text-gray-700 mt-2">{review.comment}</p>
                          {review.response && (
                            <div className="mt-3 p-3 bg-gray-50 rounded-lg border-l-4 border-primary-500">
                              <p className="text-sm font-semibold text-primary-900 mb-1">Tutor Response:</p>
                              <p className="text-sm text-gray-700 italic">"{review.response}"</p>
                            </div>
                          )}
                        </div>
                      ))
                    ) : (
                      <div className="text-center py-8 text-gray-500">
                        No reviews yet.
                      </div>
                    )}
                  </div>
                </CardContent>
              </Card>
            </TabsContent>

            <TabsContent value="availability">
              <div ref={availabilityRef}>
                <Card>
                  <CardHeader>
                    <CardTitle>Available Sessions</CardTitle>
                  </CardHeader>
                  <CardContent>
                    <div className="space-y-4">
                      {sessions.length > 0 ? (
                        sessions.map((session) => (
                          <div key={session.id} className="p-4 border border-gray-100 rounded-xl hover:border-primary-200 transition-colors bg-gray-50/30">
                            <div className="flex flex-col md:flex-row items-start md:items-center justify-between gap-4">
                              <div className="flex-1">
                                <h4 className="font-bold text-gray-900 text-lg mb-1">{session.title}</h4>
                                <div className="flex flex-wrap gap-4 text-sm text-gray-600">
                                  <div className="flex items-center gap-1.5 bg-white px-2 py-1 rounded-md border border-gray-100">
                                    <Calendar className="w-4 h-4 text-primary-500" />
                                    {new Date(session.scheduledAt).toLocaleDateString()}
                                  </div>
                                  <div className="flex items-center gap-1.5 bg-white px-2 py-1 rounded-md border border-gray-100">
                                    <Clock className="w-4 h-4 text-primary-500" />
                                    {new Date(session.scheduledAt).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}
                                  </div>
                                  <div className="flex items-center gap-1.5 bg-white px-2 py-1 rounded-md border border-gray-100">
                                    <Badge variant="info" className="bg-primary-50 text-primary-700">
                                      {session.duration} min
                                    </Badge>
                                  </div>
                                </div>
                              </div>
                              <div className="text-right flex flex-col items-end gap-2">
                                <div className="text-xl font-bold text-primary-600">₹{session.basePrice}</div>
                                <Button size="sm" onClick={() => navigate(`/student/book-session/${session.id}`)}>
                                  Book Now
                                </Button>
                              </div>
                            </div>
                          </div>
                        ))
                      ) : (
                        <div className="text-center py-12 text-gray-500 bg-gray-50 rounded-xl border border-dashed border-gray-200">
                          <Calendar className="w-12 h-12 mx-auto mb-3 opacity-30 text-primary-400" />
                          <p className="text-lg font-medium">No sessions scheduled at the moment</p>
                          <p className="text-sm">Check back later or message the tutor to request a session.</p>
                        </div>
                      )}
                    </div>
                  </CardContent>
                </Card>
              </div>
            </TabsContent>
          </Tabs>
        </div>

        {/* Sidebar */}
        <div className="space-y-6">
          <Card>
            <CardHeader>
              <CardTitle>Pricing</CardTitle>
            </CardHeader>
            <CardContent className="space-y-3">
              {subjectRates.length > 0 ? (
                <>
                  <div className="space-y-2">
                    {subjectRates.map((rate, idx) => (
                      <div key={idx} className="flex items-center justify-between p-3 border border-gray-100 rounded-lg bg-gray-50/50">
                        <span className="font-medium text-gray-800 text-sm">{rate.subjectName}</span>
                        <div className="text-right">
                          <div className="font-bold text-primary-600">₹{rate.hourlyRate}/hr</div>
                          {rate.trialRate !== undefined && rate.trialRate !== null && rate.trialRate > 0 && (
                            <div className="text-xs text-green-600">Trial: ₹{rate.trialRate}</div>
                          )}
                          {rate.trialRate === 0 && (
                            <div className="text-xs text-green-600">Free trial</div>
                          )}
                        </div>
                      </div>
                    ))}
                  </div>
                  {(tutor.hourlyRateGroup || tutor.hourlyRate) && (
                    <div className="flex items-center justify-between p-3 border border-gray-100 rounded-lg bg-gray-50/50">
                      <span className="font-medium text-gray-800 text-sm">Group Session</span>
                      <div className="font-bold text-primary-600">
                        ₹{tutor.hourlyRateGroup || tutor.hourlyRate}/hr
                      </div>
                    </div>
                  )}
                </>
              ) : (
                <>
                  <div className="flex items-center justify-between p-3 border border-gray-100 rounded-lg bg-gray-50/50">
                    <span className="font-medium text-gray-800 text-sm">1-on-1 Session</span>
                    <span className="font-bold text-primary-600">₹{tutor.hourlyRate}/hr</span>
                  </div>
                  <div className="flex items-center justify-between p-3 border border-gray-100 rounded-lg bg-gray-50/50">
                    <span className="font-medium text-gray-800 text-sm">Group Session</span>
                    <span className="font-bold text-primary-600">
                      ₹{tutor.hourlyRateGroup || tutor.hourlyRate}/hr
                    </span>
                  </div>
                </>
              )}
              <Button fullWidth onClick={handleBookSession}>
                <Calendar className="mr-2 w-5 h-5" />
                Book Session
              </Button>
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <CardTitle>Session Types</CardTitle>
            </CardHeader>
            <CardContent className="space-y-3">
              <div className="flex items-center gap-3">
                <Clock className="w-5 h-5 text-gray-400" />
                <div>
                  <p className="font-medium">1-on-1 Sessions</p>
                  <p className="text-sm text-gray-600">Personalized learning</p>
                </div>
              </div>
              <div className="flex items-center gap-3">
                <Clock className="w-5 h-5 text-gray-400" />
                <div>
                  <p className="font-medium">Group Sessions</p>
                  <p className="text-sm text-gray-600">Learn with others</p>
                </div>
              </div>
            </CardContent>
          </Card>
        </div>
      </div>
    </div>
  )
}

export default TutorProfile
