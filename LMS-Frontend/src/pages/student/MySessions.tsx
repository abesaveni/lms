import { useNavigate } from 'react-router-dom'
import { Calendar, Clock, Video, X, User, Loader2 } from 'lucide-react'
import Button from '../../components/ui/Button'
import { Card } from '../../components/ui/Card'
import { Tabs, TabsList, TabsTrigger, TabsContent } from '../../components/ui/Tabs'
import { Badge } from '../../components/ui/Badge'
import { Avatar } from '../../components/ui/Avatar'
import { useEffect, useState } from 'react'
import { getStudentSessions, SessionDto, cancelBooking, joinSession } from '../../services/sessionsApi'
import Modal from '../../components/ui/Modal'
import { submitReview } from '../../services/reviewsApi'
import { Star } from 'lucide-react'
import { clsx } from 'clsx'

const MySessions = () => {
  const navigate = useNavigate()
  const [sessions, setSessions] = useState<SessionDto[]>([])
  const [isLoading, setIsLoading] = useState(true)
  
  // Review Modal State
  const [isReviewModalOpen, setIsReviewModalOpen] = useState(false)
  const [selectedSession, setSelectedSession] = useState<SessionDto | null>(null)
  const [reviewRating, setReviewRating] = useState(5)
  const [reviewComment, setReviewComment] = useState('')
  const [isSubmitting, setIsSubmitting] = useState(false)

  const handleOpenReviewModal = (session: SessionDto) => {
    setSelectedSession(session)
    setReviewRating(5)
    setReviewComment('')
    setIsReviewModalOpen(true)
  }

  const loadSessions = async () => {
    try {
      setIsLoading(true)
      const data = await getStudentSessions()
      setSessions(data.items)
    } catch (err: any) {
      console.error(err)
    } finally {
      setIsLoading(false)
    }
  }

  useEffect(() => {
    loadSessions()
  }, [])

  const handleSubmitReview = async () => {
    if (!selectedSession || !reviewComment.trim()) {
      alert('Please provide a comment.')
      return
    }

    try {
      setIsSubmitting(true)
      await submitReview({
        sessionId: selectedSession.id,
        rating: reviewRating,
        comment: reviewComment
      })
      
      alert('Thank you for your review!')
      setIsReviewModalOpen(false)
      // Refresh sessions to show updated status if applicable (though backend mainly updates tutor profile)
      await loadSessions()
    } catch (err: any) {
      alert(err.message || 'Failed to submit review')
    } finally {
      setIsSubmitting(false)
    }
  }

  const handleCancelBooking = async (sessionId: string) => {
    if (!confirm("Are you sure you want to cancel this booking?")) return;
    
    try {
      await cancelBooking(sessionId);
      alert("Booking cancelled successfully.");
      await loadSessions();
    } catch (err: any) {
      alert(err.message || "Failed to cancel booking");
    }
  }

  const handleJoinSession = async (sessionId: string) => {
    const session = sessions.find(s => s.id === sessionId)
    if (session?.meetingLink?.startsWith('https://meet.google.com')) {
      window.open(session.meetingLink, '_blank')
      return
    }
    try {
      const { meetUrl } = await joinSession(sessionId)
      if (meetUrl) {
        window.open(meetUrl, '_blank')
      } else {
        navigate(`/session/${sessionId}/join`)
      }
    } catch (err: any) {
      navigate(`/session/${sessionId}/join`)
    }
  }

  const canCancelOrReschedule = (scheduledAt: string, status: string) => {
    if (status.toLowerCase() === 'pending') return true; // Always allow cancelling pending requests
    
    const sessionTime = new Date(scheduledAt).getTime();
    const now = new Date().getTime();
    const diffHours = (sessionTime - now) / (1000 * 60 * 60);
    return diffHours > 24; // Only allow confirmed sessions to be cancelled 24 hours before
  }

  const now = new Date()
  
  const upcomingSessions = sessions.filter(s => {
    const sLower = (s.status || '').toLowerCase();
    if (sLower === 'cancelled') return false;
    
    const startTime = new Date(s.scheduledAt)
    const endTime = new Date(startTime.getTime() + (s.duration || 60) * 60000)
    return now < endTime;
  })

  const completedSessions = sessions.filter(s => {
    const sLower = (s.status || '').toLowerCase();
    if (sLower === 'cancelled') return false;

    const startTime = new Date(s.scheduledAt)
    const endTime = new Date(startTime.getTime() + (s.duration || 60) * 60000)
    return now >= endTime;
  })

  const cancelledSessions = sessions.filter(s => (s.status || '').toLowerCase() === 'cancelled')

  if (isLoading) {
    return (
      <div className="flex items-center justify-center p-20 text-gray-500">
        <Loader2 className="w-8 h-8 animate-spin mr-3 text-primary-500" />
        <span className="text-xl">Loading your sessions...</span>
      </div>
    )
  }

  return (
    <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <div className="mb-8">
        <h1 className="text-3xl font-bold text-gray-900 mb-2">My Sessions</h1>
        <p className="text-gray-600">Manage your learning sessions</p>
      </div>

      <Tabs defaultValue="upcoming">
        <TabsList>
          <TabsTrigger value="upcoming">
            Upcoming ({upcomingSessions.length})
          </TabsTrigger>
          <TabsTrigger value="completed">
            Completed ({completedSessions.length})
          </TabsTrigger>
          <TabsTrigger value="cancelled">
            Cancelled ({cancelledSessions.length})
          </TabsTrigger>
        </TabsList>

        <TabsContent value="upcoming">
          <div className="space-y-4">
            {upcomingSessions.map((session) => (
              <Card key={session.id} hover>
                <div className="flex flex-col md:flex-row items-start md:items-center gap-4">
                  <Avatar name={session.tutorName} size="lg" />
                  <div className="flex-1">
                    <div className="flex items-start justify-between mb-2">
                      <div>
                        <h3 className="text-lg font-semibold text-gray-900">{session.tutorName}</h3>
                        <p className="text-gray-600">{session.subjectName}</p>
                      </div>
                      <Badge variant={session.status.toLowerCase() === 'confirmed' ? 'success' : 'info'}>{session.status}</Badge>
                    </div>
                    <div className="flex flex-wrap items-center gap-4 text-sm text-gray-600">
                      <div className="flex items-center gap-1">
                        <Calendar className="w-4 h-4" />
                        {new Date(session.scheduledAt).toLocaleDateString()}
                      </div>
                      <div className="flex items-center gap-1">
                        <Clock className="w-4 h-4" />
                        {new Date(session.scheduledAt).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })} • {session.duration} min
                      </div>
                    </div>
                  </div>
                  <div className="flex flex-wrap gap-2">
                    {['live', 'inprogress', 'confirmed'].includes(session.status.toLowerCase()) ? (
                      <Button 
                        size="sm"
                        onClick={() => handleJoinSession(session.id)}
                      >
                        <Video className="mr-2 w-4 h-4" />
                        Join Session
                      </Button>
                    ) : ['pending'].includes(session.status.toLowerCase()) ? (
                      <Button 
                        size="sm"
                        variant="outline"
                        onClick={() => navigate(`/student/book-session/${session.id}`)}
                      >
                        <Video className="mr-2 w-4 h-4" />
                        Complete Payment
                      </Button>
                    ) : (
                      <Badge variant={session.status.toLowerCase() === 'cancelled' ? 'error' : 'info'}>
                        {session.status}
                      </Badge>
                    )}

                    <Button 
                      size="sm" 
                      variant="outline"
                      onClick={() => navigate(`/student/tutors/${session.tutorId || 'list'}`)}
                    >
                      <User className="mr-2 w-4 h-4" />
                      View Tutor
                    </Button>
                    <Button 
                      size="sm" 
                      variant="ghost"
                      disabled={!canCancelOrReschedule(session.scheduledAt, session.status)}
                      onClick={() => handleCancelBooking(session.id)}
                      title={!canCancelOrReschedule(session.scheduledAt, session.status) ? "Cannot cancel within 24 hours" : ""}
                    >
                      <X className="mr-2 w-4 h-4" />
                      Cancel
                    </Button>
                  </div>
                </div>
              </Card>
            ))}
            {upcomingSessions.length === 0 && (
              <Card>
                <div className="text-center py-12 text-gray-500">
                  <Calendar className="w-12 h-12 mx-auto mb-2 opacity-50" />
                  <p>No upcoming sessions</p>
                </div>
              </Card>
            )}
          </div>
        </TabsContent>

        <TabsContent value="completed">
          <div className="space-y-4">
            {completedSessions.map((session) => (
              <Card key={session.id} hover>
                <div className="flex flex-col md:flex-row items-start md:items-center gap-4">
                  <Avatar name={session.tutorName} size="lg" />
                  <div className="flex-1">
                    <div className="flex items-start justify-between mb-2">
                      <div>
                        <h3 className="text-lg font-semibold text-gray-900">{session.tutorName}</h3>
                        <p className="text-gray-600">{session.subjectName || session.subject || 'N/A'}</p>
                      </div>
                      <Badge variant="default">Completed</Badge>
                    </div>
                    <div className="flex flex-wrap items-center gap-4 text-sm text-gray-600">
                      <div className="flex items-center gap-1">
                        <Calendar className="w-4 h-4" />
                        {new Date(session.scheduledAt).toLocaleDateString()}
                      </div>
                      <div className="flex items-center gap-1">
                        <Clock className="w-4 h-4" />
                        {new Date(session.scheduledAt).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })} • {session.duration} min
                      </div>
                    </div>
                  </div>
                  <div className="flex gap-2">
                    <Button 
                      size="sm" 
                      variant={session.isReviewed ? "outline" : "primary"}
                      onClick={() => handleOpenReviewModal(session)}
                      disabled={session.isReviewed || session.status.toLowerCase() === 'noshow'}
                    >
                      {session.isReviewed ? 'Already Rated' : session.status.toLowerCase() === 'noshow' ? 'Missed' : 'Rate & Review'}
                    </Button>
                  </div>
                </div>
              </Card>
            ))}
            {completedSessions.length === 0 && (
              <Card>
                <div className="text-center py-12 text-gray-500">
                  <Calendar className="w-12 h-12 mx-auto mb-2 opacity-50" />
                  <p>No completed sessions</p>
                </div>
              </Card>
            )}
          </div>
        </TabsContent>

        <TabsContent value="cancelled">
          <div className="space-y-4">
            {cancelledSessions.map((session) => (
              <Card key={session.id} hover>
                <div className="flex flex-col md:flex-row items-start md:items-center gap-4">
                  <Avatar name={session.tutorName} size="lg" />
                  <div className="flex-1 opacity-60">
                    <div className="flex items-start justify-between mb-2">
                      <div>
                        <h3 className="text-lg font-semibold text-gray-900">{session.tutorName}</h3>
                        <p className="text-gray-600">{session.subjectName || session.subject || 'N/A'}</p>
                      </div>
                      <Badge variant="error">Cancelled</Badge>
                    </div>
                    <div className="flex flex-wrap items-center gap-4 text-sm text-gray-600">
                      <div className="flex items-center gap-1">
                        <Calendar className="w-4 h-4" />
                        {new Date(session.scheduledAt).toLocaleDateString()}
                      </div>
                    </div>
                  </div>
                </div>
              </Card>
            ))}
            {cancelledSessions.length === 0 && (
              <Card>
                <div className="text-center py-12 text-gray-500">
                  <X className="w-12 h-12 mx-auto mb-2 opacity-50" />
                  <p>No cancelled sessions</p>
                </div>
              </Card>
            )}
          </div>
        </TabsContent>
      </Tabs>

      {/* Review Modal */}
      <Modal
        isOpen={isReviewModalOpen}
        onClose={() => !isSubmitting && setIsReviewModalOpen(false)}
        title="Rate your Session"
        size="md"
      >
        <div className="space-y-6">
          {selectedSession && (
            <div className="flex items-center gap-4 p-4 bg-gray-50 rounded-lg border border-gray-100">
               <Avatar name={selectedSession.tutorName} size="md" />
               <div>
                 <h4 className="font-semibold text-gray-900">{selectedSession.tutorName}</h4>
                 <p className="text-xs text-gray-500">{selectedSession.title}</p>
               </div>
            </div>
          )}

          <div>
            <label className="block text-sm font-semibold text-gray-700 mb-3">Rating</label>
            <div className="flex gap-2">
              {[1, 2, 3, 4, 5].map((star) => (
                <button
                  key={star}
                  type="button"
                  onClick={() => setReviewRating(star)}
                  className="focus:outline-none transition-transform hover:scale-110 active:scale-95"
                >
                  <Star 
                    className={clsx(
                      "w-8 h-8",
                      star <= reviewRating 
                        ? "fill-yellow-400 text-yellow-400" 
                        : "text-gray-300"
                    )} 
                  />
                </button>
              ))}
            </div>
          </div>

          <div>
            <label className="block text-sm font-semibold text-gray-700 mb-2">Share your experience</label>
            <textarea
              className="w-full px-4 py-3 bg-gray-50 border border-gray-200 rounded-xl focus:ring-2 focus:ring-primary-500/20 focus:border-primary-500 outline-none transition-all resize-none text-sm h-32"
              placeholder="How was the session? What did you learn?"
              value={reviewComment}
              onChange={(e) => setReviewComment(e.target.value)}
              disabled={isSubmitting}
            />
          </div>

          <div className="flex gap-3 pt-2">
            <Button
              variant="outline"
              fullWidth
              onClick={() => setIsReviewModalOpen(false)}
              disabled={isSubmitting}
            >
              Cancel
            </Button>
            <Button
              fullWidth
              onClick={handleSubmitReview}
              isLoading={isSubmitting}
              disabled={!reviewComment.trim()}
            >
              Submit Review
            </Button>
          </div>
        </div>
      </Modal>
    </div>
  )
}

export default MySessions
