import { useState, useEffect } from 'react'
import { useParams } from 'react-router-dom'
import { Video, Clock, Calendar, AlertCircle, CheckCircle, Loader2 } from 'lucide-react'
import { Card, CardHeader, CardTitle, CardContent } from '../../components/ui/Card'
import Button from '../../components/ui/Button'
import { Badge } from '../../components/ui/Badge'
import { Avatar } from '../../components/ui/Avatar'
import { getSessionById, getSessionMeetingLink, startSession, joinSession, SessionDto } from '../../services/sessionsApi'
import { getCurrentUserRole } from '../../utils/auth'

const JoinSession = () => {
  const { sessionId } = useParams()
  const [isTutor, setIsTutor] = useState(false)
  const [meetingStarted, setMeetingStarted] = useState(false) // This is local, but we'll prioritize session.status
  const [waitingForAdmission, setWaitingForAdmission] = useState(false)

  const [session, setSession] = useState<SessionDto | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [sessionError, setSessionError] = useState<string | null>(null)

  // Fetch session data
  const fetchSession = async () => {
    if (!sessionId) return;
    try {
      const data = await getSessionById(sessionId);
      setSession(data);
      
      // Sync local meetingStarted with backend status
      if (data.status === 'Live' || data.status === 'InProgress') {
        setMeetingStarted(true);
      }
    } catch (err: any) {
      if (isLoading) setError(err.message || 'Failed to fetch session details');
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    fetchSession();
  }, [sessionId]);

  // Dynamic Polling: Refresh session status every 5 seconds if not yet live
  useEffect(() => {
    if (!sessionId || isTutor || meetingStarted) return;
    
    const interval = setInterval(() => {
      fetchSession();
    }, 5000);
    
    return () => clearInterval(interval);
  }, [sessionId, isTutor, meetingStarted]);

  // Check if current user is the tutor
  useEffect(() => {
    const role = getCurrentUserRole();
    setIsTutor(role?.toLowerCase() === 'tutor');
  }, [])

  const getWorkingLink = (link?: string) => {
    if (!link) return '';
    // Simply return the link as provided by the backend to ensure both parties join the same meeting
    return link;
  };

  const handleGetLinkAndOpen = async () => {
    setSessionError(null)
    try {
      if (!sessionId) return;

      let meetUrl: string | undefined

      if (isTutor) {
        // Tutor: use existing meetingLink on session DTO, or GET /meeting-link
        if (session?.meetingLink) {
          meetUrl = getWorkingLink(session.meetingLink)
        } else {
          const data = await getSessionMeetingLink(sessionId)
          meetUrl = data.meetingLink || undefined
        }
      } else {
        // Student: use POST /join which returns the decrypted Jitsi URL
        const data = await joinSession(sessionId)
        meetUrl = data.meetUrl || undefined
      }

      if (meetUrl) {
        window.open(meetUrl, '_blank');
        setMeetingStarted(true);
      } else {
        setSessionError('Meeting link could not be retrieved. Please wait for the tutor to start the session.');
      }
    } catch (err: any) {
      setSessionError(err.message || 'Failed to get meeting link.');
    }
  }

  const handleStartMeeting = async () => {
    if (!sessionId) return;
    setSessionError(null)
    try {
      // 1. Call backend to start the session (updates status to Live)
      await startSession(sessionId);

      // 2. Update local state
      setMeetingStarted(true);
      setWaitingForAdmission(false);

      // 3. Open the link
      handleGetLinkAndOpen();
    } catch (err: any) {
      setSessionError(err.message || 'Failed to start meeting');
    }
  }

  const handleJoinMeeting = () => {
    if (!isTutor && !meetingStarted) {
      setWaitingForAdmission(true)
      // Show waiting message - tutor needs to start and admit
      return
    }
    handleGetLinkAndOpen()
  }

  const handleJoinEmbedded = () => {
    handleGetLinkAndOpen()
  }

  if (isLoading) {
    return (
      <div className="flex justify-center items-center min-h-[400px]">
        <Loader2 className="w-8 h-8 animate-spin text-primary-500" />
      </div>
    )
  }

  if (error || !session) {
    return (
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        <div className="p-4 bg-red-50 text-red-700 rounded-lg">
          {error || 'Session not found'}
        </div>
      </div>
    )
  }

  let derivedStatus = session.status || 'Scheduled';
  let badgeVariant: "default" | "error" | "success" | "warning" | "info" = 'info';

  if (session.scheduledAt) {
    const scheduledDate = new Date(session.scheduledAt);
    const endTime = new Date(scheduledDate.getTime() + (session.duration || 60) * 60000);
    if (new Date() > endTime && session.status.toLowerCase() !== 'cancelled' && session.status.toLowerCase() !== 'completed') {
      derivedStatus = 'Ended';
    }
  }

  const sLower = derivedStatus.toLowerCase();
  if (sLower === 'ended' || sLower === 'completed') badgeVariant = 'default';
  else if (sLower === 'cancelled') badgeVariant = 'error';
  else if (sLower === 'confirmed' || sLower === 'scheduled') badgeVariant = 'success';
  else badgeVariant = 'info';

  return (
    <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <div className="mb-8">
        <h1 className="text-3xl font-bold text-gray-900 mb-2">Session: {session.title}</h1>
        <p className="text-gray-600">Join your scheduled session</p>
      </div>

      {sessionError && (
        <div className="mb-6 p-4 bg-red-50 border border-red-200 rounded-lg text-red-700 text-sm">
          {sessionError}
        </div>
      )}

      <div className="grid lg:grid-cols-3 gap-6">
        {/* Session Info */}
        <div className="lg:col-span-1 space-y-6">
          <Card>
            <CardHeader>
              <CardTitle>Session Details</CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="flex items-center gap-3">
                <Avatar name={session.tutorName || "Tutor"} size="md" />
                <div>
                  <p className="font-semibold text-gray-900">{session.tutorName || "Tutor"}</p>
                  <p className="text-sm text-gray-600">Tutor</p>
                </div>
              </div>

              <div className="space-y-3 pt-4 border-t border-gray-200">
                <div className="flex items-center gap-3 text-sm">
                  <Calendar className="w-5 h-5 text-gray-400" />
                  <span className="text-gray-600">
                    {session.scheduledAt ? new Date(session.scheduledAt).toLocaleDateString('en-US', {
                      weekday: 'long',
                      year: 'numeric',
                      month: 'long',
                      day: 'numeric',
                    }) : 'TBD'}
                  </span>
                </div>
                <div className="flex items-center gap-3 text-sm">
                  <Clock className="w-5 h-5 text-gray-400" />
                  <span className="text-gray-600">
                    {session.scheduledAt ? new Date(session.scheduledAt).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }) : 'TBD'} ({session.duration || 60} min)
                  </span>
                </div>
              </div>

              <div className="pt-4 border-t border-gray-200">
                <Badge variant={badgeVariant}>{derivedStatus}</Badge>
              </div>
            </CardContent>
          </Card>

          {/* Tutor Controls */}
          {isTutor && (
            <Card>
              <CardHeader>
                <CardTitle>Tutor Controls</CardTitle>
              </CardHeader>
              <CardContent>
                {!meetingStarted ? (
                  <div className="space-y-4">
                    <p className="text-sm text-gray-600">
                      Start the meeting to allow students to join. You'll control who can enter.
                    </p>
                    <Button fullWidth onClick={handleStartMeeting}>
                      <Video className="mr-2 w-5 h-5" />
                      Start Meeting
                    </Button>
                  </div>
                ) : (
                  <div className="space-y-4">
                    <div className="p-3 bg-green-50 border border-green-200 rounded-lg">
                      <div className="flex items-center gap-2 text-green-700">
                        <CheckCircle className="w-5 h-5" />
                        <span className="font-medium">Meeting Started</span>
                      </div>
                      <p className="text-sm text-green-600 mt-1">
                        Students can now request to join. You can admit them from the meeting.
                      </p>
                    </div>
                    <Button fullWidth variant="outline" onClick={() => setMeetingStarted(false)}>
                      End Meeting
                    </Button>
                  </div>
                )}
              </CardContent>
            </Card>
          )}
        </div>

        {/* Meeting Area */}
        <div className="lg:col-span-2">
          <Card>
            <CardHeader>
              <CardTitle>Video Session</CardTitle>
            </CardHeader>
            <CardContent>
              {!isTutor && new Date() < new Date(session.scheduledAt) ? (
                <div className="p-12 text-center bg-gray-50 rounded-lg border-2 border-dashed border-gray-300">
                  <Video className="w-16 h-16 mx-auto mb-4 text-gray-400" />
                  <h3 className="text-xl font-semibold text-gray-900 mb-2">
                    Waiting for Tutor to Start
                  </h3>
                  <p className="text-gray-600 mb-6">
                    The tutor will start the meeting shortly. You'll be able to join once they begin.
                  </p>
                  <div className="flex items-center justify-center gap-2 text-sm text-gray-500">
                    <Clock className="w-4 h-4" />
                    <span>Session starts at {session.scheduledAt ? new Date(session.scheduledAt).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }) : 'TBD'}</span>
                  </div>
                </div>
              ) : waitingForAdmission ? (
                <div className="p-12 text-center bg-yellow-50 rounded-lg border border-yellow-200">
                  <AlertCircle className="w-16 h-16 mx-auto mb-4 text-yellow-600" />
                  <h3 className="text-xl font-semibold text-gray-900 mb-2">
                    Waiting for Admission
                  </h3>
                  <p className="text-gray-600 mb-6">
                    You've requested to join. The tutor will admit you to the meeting shortly.
                  </p>
                  <Button onClick={handleJoinMeeting} variant="outline">
                    Join Meeting
                  </Button>
                </div>
              ) : (
                <div className="space-y-4">
                  <div className="aspect-video bg-gray-900 rounded-lg flex items-center justify-center">
                    {/* Google Meet iframe would go here */}
                    {/* Note: Google Meet has embedding restrictions, so we'll use a button to open in new tab */}
                    <div className="text-center text-white">
                      <Video className="w-16 h-16 mx-auto mb-4 opacity-50" />
                      <p className="text-lg mb-4">Jitsi Video Session</p>
                      <Button
                        onClick={handleJoinEmbedded}
                        variant="secondary"
                        size="lg"
                      >
                        <Video className="mr-2 w-5 h-5" />
                        Join Video Meeting
                      </Button>
                      <p className="text-sm text-gray-400 mt-4">
                        Click to join the meeting in a new window
                      </p>
                    </div>
                  </div>
                  
                  <div className="p-4 bg-blue-50 border border-blue-200 rounded-lg">
                    <p className="text-sm text-blue-800">
                      <strong>Note:</strong> The video meeting will open in a new tab via Jitsi Meet.
                      No account or app installation required.
                    </p>
                  </div>
                </div>
              )}
            </CardContent>
          </Card>
        </div>
      </div>
    </div>
  )
}

export default JoinSession
