import { useNavigate } from 'react-router-dom'
import { motion } from 'framer-motion'
import { Calendar, BookOpen, Clock, Search, ArrowRight, Gift } from 'lucide-react'
import { EnhancedStatsCard } from '../../components/domain/EnhancedStatsCard'
import { AnimatedCard, AnimatedCardContent } from '../../components/ui/AnimatedCard'
import { EmptyState } from '../../components/ui/EmptyState'
import { Avatar } from '../../components/ui/Avatar'
import { Badge } from '../../components/ui/Badge'
import Button from '../../components/ui/Button'
import CalendarBlockingScreen from '../../components/calendar/CalendarBlockingScreen'
import { useState, useEffect } from 'react'
import { isAdmin } from '../../utils/auth'
import { getCurrentUser } from '../../utils/auth'
import { getBonusPointsSummary, BonusPointsSummary } from '../../services/bonusPointsApi'
import { getStudentDashboard, getStudentStats, StudentDashboardDto, StudentStatsDto } from '../../services/studentApi'
import {
  AIChatAssistant
} from '../../components/ai'

const StudentDashboard = () => {
  const navigate = useNavigate()
  const [isCalendarConnected, setIsCalendarConnected] = useState<boolean | null>(null)
  const [bonusSummary, setBonusSummary] = useState<BonusPointsSummary | null>(null)
  const user = getCurrentUser()
  const userName = user?.username || 'there'
  const [dashboardData, setDashboardData] = useState<StudentDashboardDto | null>(null)
  const [statsData, setStatsData] = useState<StudentStatsDto | null>(null)

  useEffect(() => {
    const fetchStudentData = async () => {
      if (isAdmin()) return;
      try {
        const dData = await getStudentDashboard()
        setDashboardData(dData)
        const sData = await getStudentStats()
        setStatsData(sData)
      } catch (err) {
        console.error(err)
      }
    }
    fetchStudentData()
  }, [])

  // Check calendar connection (skip for admins)
  useEffect(() => {
    if (isAdmin()) {
      setIsCalendarConnected(true)
      return
    }

    const checkCalendarConnection = async () => {
      try {
        const { checkCalendarConnection: checkConnection } = await import('../../services/calendarApi')
        const isConnected = await checkConnection()
        setIsCalendarConnected(isConnected)
      } catch (error) {
        setIsCalendarConnected(false)
      }
    }
    checkCalendarConnection()
  }, [])

  useEffect(() => {
    const loadBonuses = async () => {
      try {
        const data = await getBonusPointsSummary()
        setBonusSummary(data)
      } catch (error) {
        setBonusSummary({
          totalPoints: 0,
          items: [],
        })
      }
    }

    if (!isAdmin()) {
      loadBonuses()
    }
  }, [])

  // MANDATORY: Block access if calendar not connected (skip for admins)
  if (!isAdmin() && isCalendarConnected === false) {
    return <CalendarBlockingScreen userRole="student" />
  }

  // Upcoming sessions are now loaded from the API

  return (
    <div className="min-h-screen bg-gradient-to-br from-gray-50 via-white to-gray-50">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        {/* Welcome Header */}
        <motion.div
          initial={{ opacity: 0, y: -20 }}
          animate={{ opacity: 1, y: 0 }}
          className="mb-8"
        >
          <div className="flex items-center justify-between mb-2">
            <div>
              <h1 className="text-4xl font-bold text-gray-900 mb-2 flex items-center gap-3">
                Welcome back, {userName}!
                <motion.span
                  animate={{ rotate: [0, 14, -8, 10, 0] }}
                  transition={{ duration: 0.5 }}
                  style={{ display: 'inline-block' }}
                >
                  👋
                </motion.span>
              </h1>
              <p className="text-gray-600 text-lg">Here's what's happening with your learning journey</p>
            </div>
          </div>
        </motion.div>

        {/* Stats Cards */}
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-8">
          <EnhancedStatsCard
            title="Total Sessions"
            value={statsData?.totalSessionsAttended?.toString() || '0'}
            icon={<Calendar className="w-6 h-6" />}
            delay={0.2}
            gradient="primary"
            description="All time"
          />
          <EnhancedStatsCard
            title="Completed"
            value={dashboardData?.completedSessions?.toString() || '0'}
            icon={<BookOpen className="w-6 h-6" />}
            delay={0.3}
            gradient="success"
            description="Past sessions"
          />
          <EnhancedStatsCard
            title="Learning Hours"
            value={statsData?.totalHoursLearned?.toString() || '0'}
            icon={<Clock className="w-6 h-6" />}
            delay={0.5}
            gradient="primary"
            description="Total learning time"
          />
          <EnhancedStatsCard
            title="Bonus Points"
            value={`${bonusSummary?.totalPoints?.toLocaleString() ?? dashboardData?.totalBonusPoints?.toLocaleString() ?? '0'}`}
            icon={<Gift className="w-6 h-6" />}
            delay={0.6}
            gradient="success"
            description="Referral + Registration"
          />
        </div>

        {/* Quick Actions & Upcoming Sessions */}
        <div className="grid md:grid-cols-2 gap-6 mb-8">
          <AnimatedCard delay={0.3}>
            <AnimatedCardContent
              title="Quick Actions"
              className="h-full"
            >
              <div className="space-y-3">
                <motion.div whileHover={{ scale: 1.02 }} whileTap={{ scale: 0.98 }}>
                  <Button
                    fullWidth
                    variant="outline"
                    onClick={() => navigate('/student/find-tutors')}
                    className="justify-start group hover:border-primary-300 hover:bg-primary-50"
                  >
                    <Search className="mr-2 w-5 h-5 group-hover:text-primary-600" />
                    Find Tutors
                  </Button>
                </motion.div>
                <motion.div whileHover={{ scale: 1.02 }} whileTap={{ scale: 0.98 }}>
                  <Button
                    fullWidth
                    variant="outline"
                    onClick={() => navigate('/student/my-sessions')}
                    className="justify-start group hover:border-primary-300 hover:bg-primary-50"
                  >
                    <Calendar className="mr-2 w-5 h-5 group-hover:text-primary-600" />
                    View My Sessions
                  </Button>
                </motion.div>
                <motion.div whileHover={{ scale: 1.02 }} whileTap={{ scale: 0.98 }}>
                  <Button
                    fullWidth
                    variant="outline"
                    onClick={() => navigate('/student/wallet')}
                    className="justify-start group hover:border-primary-300 hover:bg-primary-50"
                  >
                    <Gift className="mr-2 w-5 h-5 group-hover:text-primary-600" />
                    Bonus Points
                  </Button>
                </motion.div>
                <motion.div whileHover={{ scale: 1.02 }} whileTap={{ scale: 0.98 }}>
                  <Button
                    fullWidth
                    variant="outline"
                    onClick={() => navigate('/student/referrals')}
                    className="justify-start group hover:border-primary-300 hover:bg-primary-50"
                  >
                    <Gift className="mr-2 w-5 h-5 group-hover:text-primary-600" />
                    Referral Program
                  </Button>
                </motion.div>
              </div>
            </AnimatedCardContent>
          </AnimatedCard>

          <AnimatedCard delay={0.35}>
            <AnimatedCardContent
              title="Bonus History"
              className="h-full"
            >
              {bonusSummary?.items?.length ? (
                <div className="space-y-3">
                  {bonusSummary.items.slice(0, 3).map((item) => (
                    <div
                      key={item.id}
                      className="flex items-center justify-between rounded-lg border border-gray-200 p-3 text-sm"
                    >
                      <div>
                        <p className="font-medium text-gray-900">{item.reason}</p>
                        <p className="text-xs text-gray-500">{new Date(item.createdAt).toLocaleDateString()}</p>
                      </div>
                      <Badge variant="success">{item.points} pts</Badge>
                    </div>
                  ))}
                </div>
              ) : (
                <EmptyState
                  icon={<Gift className="w-8 h-8" />}
                  title="No bonus points yet"
                  description="Invite friends or keep learning to earn points."
                />
              )}
            </AnimatedCardContent>
          </AnimatedCard>

          {/* Upcoming Sessions */}
          <AnimatedCard delay={0.4}>
            <AnimatedCardContent
              title="Upcoming Sessions"
              headerAction={
                <Button
                  variant="ghost"
                  size="sm"
                  onClick={() => navigate('/student/my-sessions')}
                  className="text-primary-600 hover:text-primary-700"
                >
                  View All
                  <ArrowRight className="ml-1 w-4 h-4" />
                </Button>
              }
              className="h-full"
            >
              {(dashboardData?.upcomingSessions && dashboardData.upcomingSessions.length > 0) ? (
                <div className="space-y-4">
                  {dashboardData.upcomingSessions
                    .filter(session => {
                      const now = new Date();
                      const scheduledTime = new Date(session.scheduledAt);
                      const endTime = new Date(scheduledTime.getTime() + (session.duration || 60) * 60000);
                      
                      // Show if it hasn't ended and is confirmed/pending
                      return now < endTime && (session.bookingStatus === 'Confirmed' || session.bookingStatus === 'Pending');
                    })
                    .map((session, idx) => (
                    <motion.div
                      key={session.sessionId}
                      initial={{ opacity: 0, y: 10 }}
                      animate={{ opacity: 1, y: 0 }}
                      transition={{ delay: 0.5 + idx * 0.1 }}
                      whileHover={{ scale: 1.02 }}
                      className="flex items-center gap-4 p-4 rounded-xl border border-gray-200 hover:border-primary-200 hover:bg-primary-50/50 transition-all duration-200 cursor-pointer group"
                      onClick={() => navigate(`/session/${session.sessionId}/join`)}
                    >
                      <Avatar name={session.tutorName || '-'} size="md" />
                      <div className="flex-1">
                        <h4 className="font-semibold text-gray-900 group-hover:text-primary-700">{session.tutorName || '-'}</h4>
                        <p className="text-sm text-gray-600">{session.subject || '-'}</p>
                        <p className="text-xs text-gray-500 mt-1">
                          {new Date(session.scheduledAt).toLocaleDateString()} at {new Date(session.scheduledAt).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })} • {session.duration || '-'} min
                        </p>
                      </div>
                      <Button size="sm" className="opacity-0 group-hover:opacity-100 transition-opacity" onClick={(e) => { e.stopPropagation(); navigate(`/session/${session.sessionId}/join`); }}>
                        Join
                      </Button>
                    </motion.div>
                  ))}
                </div>
              ) : (
                <EmptyState
                  icon={<Calendar className="w-8 h-8" />}
                  title="No upcoming sessions"
                  description="Book a session with a tutor to get started on your learning journey"
                  action={{
                    label: 'Find Tutors',
                    onClick: () => navigate('/student/find-tutors')
                  }}
                />
              )}
            </AnimatedCardContent>
          </AnimatedCard>
        </div>

        {/* Recent Activity */}
        <AnimatedCard delay={0.5}>
          <AnimatedCardContent
            title="Recent Activity"
            className="h-full"
          >
            <div className="space-y-4">
              {(dashboardData?.recentActivity && dashboardData.recentActivity.length > 0) ? (
                dashboardData.recentActivity.map((activity, idx) => (
                  <motion.div
                    key={idx}
                    initial={{ opacity: 0, x: -20 }}
                    animate={{ opacity: 1, x: 0 }}
                    transition={{ delay: 0.6 + idx * 0.1 }}
                    className="flex items-center gap-4 p-4 rounded-xl border border-gray-200 hover:border-primary-200 hover:bg-primary-50/50 transition-all duration-200"
                  >
                    <div className="w-12 h-12 rounded-xl bg-primary-100 flex items-center justify-center text-primary-600">
                      <BookOpen className="w-6 h-6" />
                    </div>
                    <div className="flex-1">
                      <p className="font-medium text-gray-900">{activity.description || '-'}</p>
                      <p className="text-sm text-gray-600">{new Date(activity.timestamp).toLocaleDateString()} at {new Date(activity.timestamp).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}</p>
                    </div>
                    <Badge variant="info">{activity.type || '-'}</Badge>
                  </motion.div>
                ))
              ) : (
                <EmptyState
                  icon={<BookOpen className="w-8 h-8" />}
                  title="No recent activity"
                  description="Your recent learning activities will appear here."
                />
              )}
            </div>
          </AnimatedCardContent>
        </AnimatedCard>

        <AIChatAssistant userRole="student" />
      </div>
    </div>
  )
}

export default StudentDashboard
