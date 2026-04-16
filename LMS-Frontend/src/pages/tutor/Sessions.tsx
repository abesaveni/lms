import { useState, useEffect } from 'react'
import { useNavigate } from 'react-router-dom'
import { Calendar, Clock, Video, CheckCircle, X, Plus, Loader2 } from 'lucide-react'
import Button from '../../components/ui/Button'
import { Card } from '../../components/ui/Card'
import { Tabs, TabsList, TabsTrigger, TabsContent } from '../../components/ui/Tabs'
import { Badge } from '../../components/ui/Badge'
import { Avatar } from '../../components/ui/Avatar'
import { getTutorSessions, SessionDto, startSession, markSessionComplete } from '../../services/sessionsApi'

const TutorSessions = () => {
  const navigate = useNavigate()
  const [sessions, setSessions] = useState<SessionDto[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [actionError, setActionError] = useState<string | null>(null)

  const fetchSessions = async () => {
    setIsLoading(true);
    setError(null);
    try {
      const response = await getTutorSessions();
      setSessions(response.items || []);
    } catch (err: any) {
      setError(err.message || "Failed to fetch sessions");
    } finally {
      setIsLoading(false);
    }
  }

  const handleStartSession = async (sessionId: string) => {
    setActionError(null)
    try {
      await startSession(sessionId);
      fetchSessions(); // Refresh to update Live status
      navigate(`/session/${sessionId}/join`);
    } catch (err: any) {
      setActionError(err.message || "Failed to start session");
    }
  }

  const handleJoinSession = async (sessionId: string) => {
    navigate(`/session/${sessionId}/join`);
  }

  const handleMarkComplete = async (sessionId: string) => {
    if (!confirm("Are you sure you want to mark this session as completed? This will finalize earnings and notify students.")) return;
    setActionError(null)
    try {
      await markSessionComplete(sessionId);
      fetchSessions();
    } catch (err: any) {
      setActionError(err.message || "Failed to complete session");
    }
  }

  useEffect(() => {
    fetchSessions();
  }, []);

  const upcomingSessions = sessions.filter(s => {
    const sLower = (s.status || '').toLowerCase();
    if (sLower === 'cancelled' || sLower === 'completed') return false;
    
    const endTime = new Date(new Date(s.scheduledAt).getTime() + (s.duration || 60) * 60000);
    return new Date() <= endTime;
  });

  const completedSessions = sessions.filter(s => {
    const sLower = (s.status || '').toLowerCase();
    if (sLower === 'completed') return true;
    if (sLower === 'cancelled') return false;
    
    const endTime = new Date(new Date(s.scheduledAt).getTime() + (s.duration || 60) * 60000);
    return new Date() > endTime;
  });

  const cancelledSessions = sessions.filter(s => s.status.toLowerCase() === 'cancelled');

  return (
    <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      {error && <div className="p-4 text-red-500 bg-red-50 rounded-md mb-4">{error}</div>}
      {actionError && (
        <div className="p-4 bg-red-50 border border-red-200 rounded-lg text-red-700 text-sm mb-4">{actionError}</div>
      )}
      <div className="mb-8 flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold text-gray-900 mb-2">My Sessions</h1>
          <p className="text-gray-600">Manage your teaching sessions</p>
        </div>
        <Button onClick={() => navigate('/tutor/sessions/create')}>
          <Plus className="mr-2 w-5 h-5" />
          Create Session
        </Button>
      </div>

      <Tabs defaultValue="upcoming">
        <TabsList>
          <TabsTrigger value="upcoming">Upcoming ({upcomingSessions.length})</TabsTrigger>
          <TabsTrigger value="completed">Completed ({completedSessions.length})</TabsTrigger>
          <TabsTrigger value="cancelled">Cancelled ({cancelledSessions.length})</TabsTrigger>
        </TabsList>
        <TabsContent value="upcoming">
          {isLoading ? (
             <div className="flex justify-center py-12"><Loader2 className="animate-spin text-primary-500" /></div>
          ) : error ? (
            <div className="p-4 text-red-500 bg-red-50 rounded-md mb-4">{error}</div>
          ) : upcomingSessions.length > 0 ? (
            <div className="space-y-4">
              {upcomingSessions.map((session) => (
                <Card key={session.id} hover>
                  <div className="flex flex-col md:flex-row items-start md:items-center gap-4">
                    <Avatar name={session.tutorName || "Student"} size="lg" />
                    <div className="flex-1">
                      <div className="flex items-start justify-between mb-2">
                        <div>
                          <h3 className="text-lg font-semibold text-gray-900">{session.title}</h3>
                          <p className="text-gray-600">{session.subjectName || session.subject || "Teaching"}</p>
                        </div>
                        <Badge variant={session.status.toLowerCase() === 'confirmed' ? 'success' : 'warning'}>
                          {session.status}
                        </Badge>
                      </div>
                      <div className="flex flex-wrap items-center gap-4 text-sm text-gray-600">
                        <div className="flex items-center gap-1">
                          <Calendar className="w-4 h-4" />
                          {new Date(session.scheduledAt).toLocaleDateString()}
                        </div>
                        <div className="flex items-center gap-1">
                          <Clock className="w-4 h-4" />
                          {new Date(session.scheduledAt).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })} • {session.duration}m
                        </div>
                        <div className="flex items-center gap-1">
                          <span className="font-semibold text-primary-600">₹{session.basePrice}</span>
                        </div>
                      </div>
                    </div>
                    <div className="flex flex-wrap gap-2">
                       {session.status.toLowerCase() === 'scheduled' ? (
                        <Button size="sm" onClick={() => handleStartSession(session.id)}>
                          <Video className="mr-2 w-4 h-4" />
                          Start Session
                        </Button>
                      ) : (session.status.toLowerCase() === 'live' || session.status.toLowerCase() === 'inprogress') ? (
                        <>
                          <Button size="sm" variant="primary" onClick={() => handleJoinSession(session.id)}>
                            <Video className="mr-2 w-4 h-4 text-green-300" />
                            Join Session
                          </Button>
                          <Button size="sm" variant="outline" onClick={() => handleMarkComplete(session.id)}>
                            <CheckCircle className="mr-2 w-4 h-4" />
                            Complete
                          </Button>
                        </>
                      ) : null}
                    </div>
                  </div>
                </Card>
              ))}
            </div>
          ) : (
            <Card>
              <div className="text-center py-12 text-gray-500">
                <Calendar className="w-12 h-12 mx-auto mb-2 opacity-50" />
                <p>No upcoming sessions</p>
              </div>
            </Card>
          )}
        </TabsContent>

        <TabsContent value="completed">
          <div className="space-y-4">
            {completedSessions.map((session) => (
              <Card key={session.id} hover>
                <div className="flex flex-col md:flex-row items-start md:items-center gap-4">
                  <Avatar name={session.tutorName || "Student"} size="lg" />
                  <div className="flex-1">
                    <div className="flex items-start justify-between mb-2">
                      <div>
                        <h3 className="text-lg font-semibold text-gray-900">{session.title}</h3>
                        <p className="text-gray-600">{session.subjectName || session.subject}</p>
                      </div>
                      <Badge variant="default">Completed</Badge>
                    </div>
                    <div className="flex flex-wrap items-center gap-4 text-sm text-gray-600">
                      <div className="flex items-center gap-1">
                        <Calendar className="w-4 h-4" />
                        {new Date(session.scheduledAt).toLocaleDateString()}
                      </div>
                      <div className="flex items-center gap-1">
                        <span className="font-semibold text-primary-600">₹{session.basePrice}</span>
                      </div>
                    </div>
                  </div>
                </div>
              </Card>
            ))}
            {completedSessions.length === 0 && (
              <Card>
                <div className="text-center py-12 text-gray-500">
                  <CheckCircle className="w-12 h-12 mx-auto mb-2 opacity-50" />
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
                  <Avatar name={session.tutorName || "Student"} size="lg" />
                  <div className="flex-1">
                    <div className="flex items-start justify-between mb-2">
                      <div>
                        <h3 className="text-lg font-semibold text-gray-900">{session.title}</h3>
                        <p className="text-gray-600">{session.subjectName || session.subject}</p>
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
    </div>
  )
}

export default TutorSessions
