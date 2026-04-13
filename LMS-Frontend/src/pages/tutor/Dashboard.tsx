import { useState, useEffect } from 'react'
import { useNavigate } from 'react-router-dom'
import { motion } from 'framer-motion'
import { DollarSign, Calendar, Users, Star, Video, Plus, MessageCircle, ArrowRight, Loader2, Settings, User, Bell } from 'lucide-react'
import { EnhancedStatsCard } from '../../components/domain/EnhancedStatsCard'
import { AnimatedCard, AnimatedCardContent } from '../../components/ui/AnimatedCard'
import { EmptyState } from '../../components/ui/EmptyState'
import Button from '../../components/ui/Button'
import { Avatar } from '../../components/ui/Avatar'
import { isAdmin, getCurrentUser } from '../../utils/auth'
import CalendarBlockingScreen from '../../components/calendar/CalendarBlockingScreen'

import { getTutorProfile, TutorProfileDto, getEarningsOverview } from '../../services/tutorApi'
import { getTutorSessions, SessionDto } from '../../services/sessionsApi'

const TutorDashboard = () => {
  const navigate = useNavigate()
  const [profile, setProfile] = useState<TutorProfileDto | null>(null)
  const [upcomingSessions, setUpcomingSessions] = useState<SessionDto[]>([])
  const [earnings, setEarnings] = useState({ totalEarned: 0, pending: 0, available: 0 })
  const [isLoading, setIsLoading] = useState(true)
  const [isSessionsLoading, setIsSessionsLoading] = useState(true)
  const [isCalendarConnected, setIsCalendarConnected] = useState<boolean | null>(null)

  const user = getCurrentUser()
  const userName = profile?.firstName || user?.username || 'there'

  // Fetch Core Profile and Checks
  useEffect(() => {
    const fetchData = async () => {
      if (isAdmin()) {
        setIsLoading(false)
        return
      }

      try {
        const [profileData, earningsData, { checkCalendarConnection: checkConnection }] = await Promise.all([
          getTutorProfile(),
          getEarningsOverview(),
          import('../../services/calendarApi')
        ])

        setProfile(profileData)
        setEarnings(earningsData)

        // Check verification status
        if (profileData.verificationStatus !== 'Approved') {
          navigate('/tutor/verification-pending')
          return
        }

        const connected = await checkConnection()
        setIsCalendarConnected(connected)
      } catch (error) {
        console.error('Error fetching dashboard data:', error)
      } finally {
        setIsLoading(false)
      }
    }

    fetchData()
  }, [navigate])

  // Fetch Sessions once profile is ready
  useEffect(() => {
    const fetchDashboardSessions = async () => {
      try {
        // Fetch real sessions from backend
        const response = await getTutorSessions({ pageSize: 5, upcoming: true });
        setUpcomingSessions(response.items || []);
      } catch (err) {
        console.error("Failed to fetch dashboard sessions", err);
      } finally {
        setIsSessionsLoading(false);
      }
    }

    // Only fetch if calendar is connected or if admin (skip calendar check)
    if (!isLoading && (isCalendarConnected || isAdmin())) {
      fetchDashboardSessions();
    } else if (isCalendarConnected === false) {
      setIsSessionsLoading(false);
    }
  }, [isLoading, isCalendarConnected]);

  if (isLoading) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <Loader2 className="w-8 h-8 animate-spin text-primary-500" />
      </div>
    )
  }

  // MANDATORY: Block access if calendar not connected (skip for admins)
  if (!isAdmin() && isCalendarConnected === false) {
    return <CalendarBlockingScreen userRole="tutor" />
  }

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
              <p className="text-gray-600 text-lg">Here's your teaching overview</p>
            </div>
          </div>
        </motion.div>

        {/* Stats Grid */}
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-8">
          <EnhancedStatsCard
            title="Total Students"
            value="127"
            icon={<Users className="w-6 h-6" />}
            trend={{ value: 15, isPositive: true }}
            delay={0.3}
            gradient="primary"
            description="Active students"
          />
          <EnhancedStatsCard
            title="Upcoming Sessions"
            value={upcomingSessions.length.toString()}
            icon={<Calendar className="w-6 h-6" />}
            delay={0.4}
            gradient="success"
            description="Confirmed tasks"
          />
          <EnhancedStatsCard
            title="Total Earnings"
            value={`$${earnings.totalEarned}`}
            icon={<DollarSign className="w-6 h-6" />}
            delay={0.5}
            gradient="warning"
            description="All time"
          />
          <EnhancedStatsCard
            title="Average Rating"
            value={profile?.averageRating?.toString() || "0.0"}
            icon={<Star className="w-6 h-6" />}
            delay={0.6}
            gradient="primary"
            description={`From ${profile?.totalReviews || 0} reviews`}
          />
        </div>

        {/* Virtual Classroom CTA */}
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ delay: 0.3 }}
          className="mb-8"
        >
          <div className="relative overflow-hidden rounded-2xl bg-gradient-to-br from-primary-500 to-primary-600 p-8 text-white shadow-xl">
            <div className="absolute inset-0 opacity-20" style={{
              backgroundImage: `url("data:image/svg+xml,%3Csvg width='60' height='60' viewBox='0 0 60 60' xmlns='http://www.w3.org/2000/svg'%3E%3Cg fill='none' fill-rule='evenodd'%3E%3Cg fill='%23ffffff' fill-opacity='0.05'%3E%3Cpath d='M36 34v-4h-2v4h-4v2h4v4h2v-4h4v-2h-4zm0-30V0h-2v4h-4v2h4v4h2V6h4V4h-4zM6 34v-4H4v4H0v2h4v4h2v-4h4v-2H6zM6 4V0H4v4H0v2h4v4h2V6h4V4H6z'/%3E%3C/g%3E%3C/g%3E%3C/svg%3E")`
            }} />
            <div className="relative flex items-center justify-between">
              <div>
                <h3 className="text-2xl font-semibold mb-2">Virtual Classroom</h3>
                <p className="text-primary-100 mb-4">Launch your session with integrated tools.</p>
                <motion.div whileHover={{ scale: 1.05 }} whileTap={{ scale: 0.95 }}>
                  <Button
                    variant="secondary"
                    onClick={() => navigate('/tutor/sessions/create')}
                    className="bg-white text-primary-600 hover:bg-primary-50"
                  >
                    <Video className="mr-2 w-5 h-5" />
                    Start Virtual Classroom
                  </Button>
                </motion.div>
              </div>
              <motion.div
                animate={{ rotate: [0, 5, -5, 0] }}
                transition={{ duration: 3, repeat: Infinity, repeatDelay: 2 }}
                className="hidden md:block w-32 h-32 rounded-full bg-white/20 backdrop-blur-sm flex items-center justify-center"
              >
                <Video className="w-16 h-16" />
              </motion.div>
            </div>
          </div>
        </motion.div>

        {/* Quick Actions & Upcoming Sessions */}
        <div className="grid md:grid-cols-2 gap-6 mb-8">
          <AnimatedCard delay={0.4}>
            <AnimatedCardContent
              title="Quick Actions"
              className="h-full"
            >
              <div className="space-y-4">
                <motion.div whileHover={{ scale: 1.02 }} whileTap={{ scale: 0.98 }}>
                  <Button
                    fullWidth
                    variant="outline"
                    onClick={() => navigate('/tutor/sessions/create')}
                    className="justify-start group hover:border-primary-300 hover:bg-primary-50 min-h-[50px]"
                  >
                    <Plus className="mr-2 w-5 h-5 group-hover:text-primary-600" />
                    Create Session
                  </Button>
                </motion.div>
                <motion.div whileHover={{ scale: 1.02 }} whileTap={{ scale: 0.98 }}>
                  <Button
                    fullWidth
                    variant="outline"
                    onClick={() => navigate('/tutor/inbox')}
                    className="justify-start group hover:border-primary-300 hover:bg-primary-50 min-h-[50px]"
                  >
                    <MessageCircle className="mr-2 w-5 h-5 group-hover:text-primary-600" />
                    View Messages
                  </Button>
                </motion.div>
                <motion.div whileHover={{ scale: 1.02 }} whileTap={{ scale: 0.98 }}>
                  <Button
                    fullWidth
                    variant="outline"
                    onClick={() => navigate('/tutor/earnings')}
                    className="justify-start group hover:border-primary-300 hover:bg-primary-50 min-h-[50px]"
                  >
                    <DollarSign className="mr-2 w-5 h-5 group-hover:text-primary-600" />
                    View Earnings
                  </Button>
                </motion.div>
                <motion.div whileHover={{ scale: 1.02 }} whileTap={{ scale: 0.98 }}>
                  <Button
                    fullWidth
                    variant="outline"
                    onClick={() => navigate('/tutor/profile')}
                    className="justify-start group hover:border-primary-300 hover:bg-primary-50 min-h-[50px]"
                  >
                    <User className="mr-2 w-5 h-5 group-hover:text-primary-600" />
                    Public Profile
                  </Button>
                </motion.div>
                <motion.div whileHover={{ scale: 1.02 }} whileTap={{ scale: 0.98 }}>
                  <Button
                    fullWidth
                    variant="outline"
                    onClick={() => navigate('/tutor/profile-settings')}
                    className="justify-start group hover:border-primary-300 hover:bg-primary-50 min-h-[50px]"
                  >
                    <Settings className="mr-2 w-5 h-5 group-hover:text-primary-600" />
                    Account Settings
                  </Button>
                </motion.div>
                <motion.div whileHover={{ scale: 1.02 }} whileTap={{ scale: 0.98 }}>
                  <Button
                    fullWidth
                    variant="outline"
                    onClick={() => navigate('/tutor/notifications')}
                    className="justify-start group hover:border-primary-300 hover:bg-primary-50 min-h-[50px]"
                  >
                    <Bell className="mr-2 w-5 h-5 group-hover:text-primary-600" />
                    Notifications
                  </Button>
                </motion.div>
              </div>
            </AnimatedCardContent>
          </AnimatedCard>

          {/* Upcoming Sessions */}
          <AnimatedCard delay={0.5}>
            <AnimatedCardContent
              title="Upcoming Sessions"
              headerAction={
                <Button
                  variant="ghost"
                  size="sm"
                  onClick={() => navigate('/tutor/sessions')}
                  className="text-primary-600 hover:text-primary-700"
                >
                  View All
                  <ArrowRight className="ml-1 w-4 h-4" />
                </Button>
              }
              className="h-full"
            >
              {isSessionsLoading ? (
                <div className="flex items-center justify-center py-12">
                  <Loader2 className="w-8 h-8 animate-spin text-primary-300" />
                </div>
              ) : upcomingSessions.length > 0 ? (
                <div className="space-y-4">
                  {upcomingSessions.map((session, idx) => (
                    <motion.div
                      key={session.id}
                      initial={{ opacity: 0, y: 10 }}
                      animate={{ opacity: 1, y: 0 }}
                      transition={{ delay: 0.6 + idx * 0.1 }}
                      whileHover={{ scale: 1.02 }}
                      className="flex items-center gap-4 p-4 rounded-xl border border-gray-200 hover:border-primary-200 hover:bg-primary-50/50 transition-all duration-200 cursor-pointer group"
                      onClick={() => navigate(`/tutor/sessions`)}
                    >
                      <Avatar name={session.tutorName || "Student"} size="md" />
                      <div className="flex-1">
                        <h4 className="font-semibold text-gray-900 group-hover:text-primary-700">{session.title || "Session"}</h4>
                        <p className="text-sm text-gray-600">{session.subjectName || session.subject || "Teaching"}</p>
                        <p className="text-xs text-gray-500 mt-1">
                          {new Date(session.scheduledAt).toLocaleDateString()} at {new Date(session.scheduledAt).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })} • {session.duration}m
                        </p>
                      </div>
                      <Button
                        size="sm"
                        className="opacity-0 group-hover:opacity-100 transition-opacity"
                        onClick={(e) => { e.stopPropagation(); navigate(`/session/${session.id}/join`); }}
                      >
                        Join
                      </Button>
                    </motion.div>
                  ))}
                </div>
              ) : (
                <EmptyState
                  icon={<Calendar className="w-8 h-8" />}
                  title="No upcoming sessions"
                  description="Create a session to start teaching students"
                  action={{
                    label: 'Create Session',
                    onClick: () => navigate('/tutor/sessions/create')
                  }}
                />
              )}
            </AnimatedCardContent>
          </AnimatedCard>
        </div>

        {/* Earnings Overview */}
        <AnimatedCard delay={0.6}>
          <AnimatedCardContent
            title="Earnings Overview"
            headerAction={
              <Button
                variant="ghost"
                size="sm"
                onClick={() => navigate('/tutor/earnings')}
                className="text-primary-600 hover:text-primary-700"
              >
                View Details
                <ArrowRight className="ml-1 w-4 h-4" />
              </Button>
            }
          >
            <div className="grid md:grid-cols-3 gap-6">
              <motion.div
                initial={{ opacity: 0, scale: 0.9 }}
                animate={{ opacity: 1, scale: 1 }}
                transition={{ delay: 0.7 }}
                className="p-6 rounded-xl bg-gradient-to-br from-success-50 to-success-100 border border-success-200"
              >
                <p className="text-sm font-medium text-success-700 mb-1">Total Earned</p>
                <p className="text-3xl font-bold text-success-900">${earnings.totalEarned}</p>
              </motion.div>
              <motion.div
                initial={{ opacity: 0, scale: 0.9 }}
                animate={{ opacity: 1, scale: 1 }}
                transition={{ delay: 0.8 }}
                className="p-6 rounded-xl bg-gradient-to-br from-primary-50 to-primary-100 border border-primary-200"
              >
                <p className="text-sm font-medium text-primary-700 mb-1">Available to Withdraw</p>
                <p className="text-3xl font-bold text-primary-900">${earnings.available}</p>
              </motion.div>
              <motion.div
                initial={{ opacity: 0, scale: 0.9 }}
                animate={{ opacity: 1, scale: 1 }}
                transition={{ delay: 0.9 }}
                className="p-6 rounded-xl bg-gradient-to-br from-warning-50 to-warning-100 border border-warning-200"
              >
                <p className="text-sm font-medium text-warning-700 mb-1">Pending Clearance</p>
                <p className="text-3xl font-bold text-warning-900">${earnings.pending}</p>
              </motion.div>
            </div>
          </AnimatedCardContent>
        </AnimatedCard>

      </div>
    </div>
  )
}

export default TutorDashboard
