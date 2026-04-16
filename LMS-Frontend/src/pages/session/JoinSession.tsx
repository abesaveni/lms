import { useState, useEffect } from 'react'
import { useParams } from 'react-router-dom'
import { Video, Clock, Calendar, Loader2 } from 'lucide-react'
import { Card, CardHeader, CardTitle, CardContent } from '../../components/ui/Card'
import Button from '../../components/ui/Button'
import { Badge } from '../../components/ui/Badge'
import { Avatar } from '../../components/ui/Avatar'
import { getSessionById, startSession, joinSession, SessionDto } from '../../services/sessionsApi'
import { getCurrentUser, getCurrentUserRole } from '../../utils/auth'

const JoinSession = () => {
  const { sessionId } = useParams()
  const [isTutor, setIsTutor] = useState(false)
  const [session, setSession] = useState<SessionDto | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [actionError, setActionError] = useState<string | null>(null)
  const [jitsiUrl, setJitsiUrl] = useState<string | null>(null)
  const [isStarting, setIsStarting] = useState(false)

  const user = getCurrentUser()
  const displayName = user ? `${user.username || user.email || 'Participant'}` : 'Participant'

  useEffect(() => {
    const role = getCurrentUserRole()
    setIsTutor(role?.toLowerCase() === 'tutor')
  }, [])

  useEffect(() => {
    if (!sessionId) return
    const fetchSession = async () => {
      try {
        const data = await getSessionById(sessionId)
        setSession(data)
        // If already live and has a meeting link, pre-load it
        if ((data.status === 'Live' || data.status === 'InProgress') && data.meetingLink) {
          setJitsiUrl(buildJitsiUrl(data.meetingLink, displayName))
        }
      } catch (err: any) {
        setError(err.message || 'Failed to load session')
      } finally {
        setIsLoading(false)
      }
    }
    fetchSession()
  }, [sessionId])

  const buildJitsiUrl = (rawUrl: string, name: string) => {
    // Ensure we use the base URL without existing hash params
    const base = rawUrl.split('#')[0]
    const encodedName = encodeURIComponent(name)
    return `${base}#config.startWithAudioMuted=false&config.startWithVideoMuted=false&config.prejoinPageEnabled=false&userInfo.displayName=${encodedName}`
  }

  const handleStart = async () => {
    if (!sessionId) return
    setIsStarting(true)
    setActionError(null)
    try {
      const { meetUrl } = await startSession(sessionId)
      setJitsiUrl(buildJitsiUrl(meetUrl, displayName))
    } catch (err: any) {
      setActionError(err.message || 'Failed to start session')
    } finally {
      setIsStarting(false)
    }
  }

  const handleJoin = async () => {
    if (!sessionId) return
    setIsStarting(true)
    setActionError(null)
    try {
      const { meetUrl } = await joinSession(sessionId)
      setJitsiUrl(buildJitsiUrl(meetUrl, displayName))
    } catch (err: any) {
      setActionError(err.message || 'Failed to join session. The tutor may not have started it yet.')
    } finally {
      setIsStarting(false)
    }
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
      <div className="max-w-4xl mx-auto px-4 py-8">
        <div className="p-4 bg-red-50 text-red-700 rounded-lg">{error || 'Session not found'}</div>
      </div>
    )
  }

  const sLower = (session.status || '').toLowerCase()
  const isLive = sLower === 'live' || sLower === 'inprogress'
  const isScheduled = sLower === 'scheduled'

  return (
    <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <div className="mb-6">
        <h1 className="text-2xl font-bold text-gray-900 mb-1">{session.title}</h1>
        <div className="flex items-center gap-3 text-sm text-gray-600">
          <div className="flex items-center gap-1">
            <Calendar className="w-4 h-4" />
            {new Date(session.scheduledAt).toLocaleDateString()}
          </div>
          <div className="flex items-center gap-1">
            <Clock className="w-4 h-4" />
            {new Date(session.scheduledAt).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })} · {session.duration || 60}m
          </div>
          <Badge variant={isLive ? 'success' : isScheduled ? 'info' : 'default'}>
            {isLive ? 'Live' : session.status}
          </Badge>
        </div>
      </div>

      {actionError && (
        <div className="mb-4 p-3 bg-red-50 border border-red-200 rounded-lg text-red-700 text-sm">
          {actionError}
        </div>
      )}

      {/* Meeting area — iframe when active, launch button when not */}
      {jitsiUrl ? (
        <div className="rounded-xl overflow-hidden border border-gray-200 shadow-lg">
          <iframe
            src={jitsiUrl}
            allow="camera; microphone; fullscreen; display-capture; autoplay"
            style={{ width: '100%', height: '70vh', border: 'none', minHeight: '500px' }}
            title="Video Session"
          />
        </div>
      ) : (
        <div className="grid lg:grid-cols-3 gap-6">
          {/* Session info card */}
          <div className="lg:col-span-1">
            <Card>
              <CardHeader>
                <CardTitle>Session Details</CardTitle>
              </CardHeader>
              <CardContent className="space-y-4">
                <div className="flex items-center gap-3">
                  <Avatar name={session.tutorName || 'Tutor'} size="md" />
                  <div>
                    <p className="font-semibold text-gray-900">{session.tutorName || 'Tutor'}</p>
                    <p className="text-sm text-gray-500">Tutor</p>
                  </div>
                </div>
                <div className="text-sm text-gray-600 space-y-1 pt-2 border-t">
                  <p>{session.subjectName || session.subject || 'General'}</p>
                  {session.description && <p className="text-gray-500">{session.description}</p>}
                </div>
              </CardContent>
            </Card>
          </div>

          {/* Launch panel */}
          <div className="lg:col-span-2">
            <Card>
              <CardContent className="p-8">
                <div className="text-center">
                  <div className="w-20 h-20 mx-auto mb-4 rounded-full bg-primary-100 flex items-center justify-center">
                    <Video className="w-10 h-10 text-primary-600" />
                  </div>
                  <h2 className="text-xl font-semibold text-gray-900 mb-2">
                    {isTutor
                      ? isLive ? 'Rejoin your session' : 'Ready to start?'
                      : isLive ? 'Session is live — join now' : 'Waiting for tutor to start'}
                  </h2>
                  <p className="text-gray-500 mb-6 text-sm">
                    The meeting will open right here on this page.
                    Make sure your browser allows camera and microphone access.
                  </p>

                  {isTutor ? (
                    <Button
                      size="lg"
                      onClick={handleStart}
                      isLoading={isStarting}
                      className="px-8"
                    >
                      <Video className="mr-2 w-5 h-5" />
                      {isLive ? 'Rejoin Session' : 'Start Session'}
                    </Button>
                  ) : (
                    <Button
                      size="lg"
                      onClick={handleJoin}
                      isLoading={isStarting}
                      disabled={!isLive}
                      className="px-8"
                    >
                      <Video className="mr-2 w-5 h-5" />
                      {isLive ? 'Join Session' : 'Session Not Started Yet'}
                    </Button>
                  )}

                  <p className="mt-4 text-xs text-gray-400">
                    Powered by Jitsi Meet · No app installation needed
                  </p>
                </div>
              </CardContent>
            </Card>
          </div>
        </div>
      )}
    </div>
  )
}

export default JoinSession
