import { useNavigate } from 'react-router-dom'
import { motion } from 'framer-motion'
import { Calendar, BookOpen, Clock, Search, ArrowRight, Gift, Flame, Trophy, Star, Zap, TrendingUp } from 'lucide-react'
import { EnhancedStatsCard } from '../../components/domain/EnhancedStatsCard'
import { AnimatedCard, AnimatedCardContent } from '../../components/ui/AnimatedCard'
import { EmptyState } from '../../components/ui/EmptyState'
import { Avatar } from '../../components/ui/Avatar'
import { Badge } from '../../components/ui/Badge'
import Button from '../../components/ui/Button'
import { useState, useEffect } from 'react'
import { isAdmin } from '../../utils/auth'
import { getCurrentUser } from '../../utils/auth'
import { getBonusPointsSummary, BonusPointsSummary } from '../../services/bonusPointsApi'
import { getStudentDashboard, getStudentStats, StudentDashboardDto, StudentStatsDto } from '../../services/studentApi'

// XP level config
const LEVELS = [
  { name: 'Bronze', min: 0, max: 500, color: 'text-amber-600', bg: 'from-amber-500 to-orange-600', icon: '🥉' },
  { name: 'Silver', min: 500, max: 1500, color: 'text-gray-500', bg: 'from-gray-400 to-gray-600', icon: '🥈' },
  { name: 'Gold', min: 1500, max: 3500, color: 'text-yellow-500', bg: 'from-yellow-400 to-amber-500', icon: '🥇' },
  { name: 'Platinum', min: 3500, max: 99999, color: 'text-indigo-600', bg: 'from-indigo-500 to-purple-600', icon: '💎' },
]

const getLevelInfo = (xp: number) => {
  const level = LEVELS.find(l => xp >= l.min && xp < l.max) || LEVELS[LEVELS.length - 1]
  const nextLevel = LEVELS[LEVELS.indexOf(level) + 1]
  const progress = nextLevel ? ((xp - level.min) / (nextLevel.min - level.min)) * 100 : 100
  return { level, nextLevel, progress }
}

// Achievement badge config
const ACHIEVEMENT_BADGES = [
  { id: 'first_session', icon: '🎯', label: 'First Session', desc: 'Attended your first session', threshold: 1, field: 'totalSessionsAttended' },
  { id: 'five_sessions', icon: '⭐', label: 'Rising Star', desc: '5 sessions completed', threshold: 5, field: 'totalSessionsAttended' },
  { id: 'ten_sessions', icon: '🔥', label: 'On Fire', desc: '10 sessions completed', threshold: 10, field: 'totalSessionsAttended' },
  { id: 'reviewer', icon: '✍️', label: 'Reviewer', desc: 'Left your first review', threshold: 1, field: 'totalReviewsLeft' },
  { id: 'referrer', icon: '🤝', label: 'Connector', desc: 'Referred a friend', threshold: 1, field: 'totalReferrals' },
]

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
      if (isAdmin()) return
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

  useEffect(() => {
    if (isAdmin()) { setIsCalendarConnected(true); return }
    const checkCalendarConnection = async () => {
      try {
        const { checkCalendarConnection: checkConnection } = await import('../../services/calendarApi')
        const isConnected = await checkConnection()
        setIsCalendarConnected(isConnected)
      } catch {
        setIsCalendarConnected(false)
      }
    }
    checkCalendarConnection()
  }, [])

  useEffect(() => {
    if (isAdmin()) return
    const loadBonuses = async () => {
      try {
        const data = await getBonusPointsSummary()
        setBonusSummary(data)
      } catch {
        setBonusSummary({ totalPoints: 0, items: [] })
      }
    }
    loadBonuses()
  }, [])

  const showCalendarBanner = !isAdmin() && isCalendarConnected === false

  // XP = bonus points as proxy for now
  const xp = bonusSummary?.totalPoints ?? dashboardData?.totalBonusPoints ?? 0
  const { level, nextLevel, progress } = getLevelInfo(xp)

  // Study streak — derived from sessions attended (mock: sessions / 3)
  const totalSessions = statsData?.totalSessionsAttended ?? dashboardData?.completedSessions ?? 0
  const studyStreak = Math.min(Math.floor(totalSessions / 2) + (totalSessions > 0 ? 1 : 0), 30)

  // Unlocked badges
  const unlockedBadges = ACHIEVEMENT_BADGES.filter(b => {
    if (b.field === 'totalSessionsAttended') return totalSessions >= b.threshold
    return false // other fields need API support
  })

  return (
    <div className="min-h-screen bg-gradient-to-br from-gray-50 via-white to-indigo-50/30">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">

        {/* Calendar banner */}
        {showCalendarBanner && (
          <div className="mb-6 flex items-center justify-between gap-4 bg-amber-50 border border-amber-200 rounded-xl px-4 py-3">
            <div className="flex items-center gap-3">
              <Calendar className="w-5 h-5 text-amber-600 flex-shrink-0" />
              <p className="text-sm text-amber-800">Connect your Google Calendar to sync sessions and get reminders.</p>
            </div>
            <div className="flex items-center gap-2 flex-shrink-0">
              <button onClick={() => navigate('/calendar/connect')} className="text-xs font-semibold text-amber-700 hover:text-amber-900 underline">Connect</button>
              <button onClick={() => setIsCalendarConnected(true)} className="text-xs text-amber-500 hover:text-amber-700">Dismiss</button>
            </div>
          </div>
        )}

        {/* Welcome Header */}
        <motion.div initial={{ opacity: 0, y: -20 }} animate={{ opacity: 1, y: 0 }} className="mb-6">
          <div className="flex items-center justify-between">
            <div>
              <h1 className="text-2xl font-black text-gray-900">
                Hey {userName}! 👋
              </h1>
              <p className="text-sm text-gray-500 mt-1">Track your progress and keep learning strong.</p>
            </div>
            <div className="hidden sm:flex items-center gap-3">
              <div className="flex items-center gap-1.5 bg-orange-50 border border-orange-100 rounded-xl px-3 py-2">
                <Flame className="w-4 h-4 text-orange-500" />
                <span className="text-sm font-bold text-orange-600">{studyStreak} day streak</span>
              </div>
              <div className={`flex items-center gap-1.5 bg-gradient-to-r ${level.bg} rounded-xl px-3 py-2`}>
                <span className="text-sm">{level.icon}</span>
                <span className="text-sm font-bold text-white">{level.name}</span>
              </div>
            </div>
          </div>
        </motion.div>

        {/* XP Progress Bar */}
        <motion.div
          initial={{ opacity: 0, y: 10 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ delay: 0.1 }}
          className="mb-6 bg-white rounded-2xl border border-gray-100 shadow-sm p-4"
        >
          <div className="flex items-center justify-between mb-2">
            <div className="flex items-center gap-2">
              <Zap className="w-4 h-4 text-indigo-500" />
              <span className="text-sm font-bold text-gray-800">{xp.toLocaleString()} XP</span>
              <span className={`text-xs font-bold px-2 py-0.5 rounded-full bg-indigo-50 ${level.color}`}>{level.icon} {level.name}</span>
            </div>
            {nextLevel && (
              <span className="text-xs text-gray-400 font-medium">{nextLevel.min - xp} XP to {nextLevel.name}</span>
            )}
          </div>
          <div className="w-full h-2.5 bg-gray-100 rounded-full overflow-hidden">
            <motion.div
              initial={{ width: 0 }}
              animate={{ width: `${progress}%` }}
              transition={{ duration: 1, ease: 'easeOut', delay: 0.3 }}
              className={`h-full bg-gradient-to-r ${level.bg} rounded-full`}
            />
          </div>
        </motion.div>

        {/* Stats Cards */}
        <div className="grid grid-cols-2 lg:grid-cols-4 gap-4 mb-6">
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
            delay={0.4}
            gradient="primary"
            description="Total learning time"
          />
          <EnhancedStatsCard
            title="Bonus Points"
            value={`${bonusSummary?.totalPoints?.toLocaleString() ?? dashboardData?.totalBonusPoints?.toLocaleString() ?? '0'}`}
            icon={<Gift className="w-6 h-6" />}
            delay={0.5}
            gradient="success"
            description="Redeemable rewards"
          />
        </div>

        {/* Achievement Badges */}
        <motion.div
          initial={{ opacity: 0, y: 16 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ delay: 0.25 }}
          className="mb-6 bg-white rounded-2xl border border-gray-100 shadow-sm p-4"
        >
          <div className="flex items-center justify-between mb-3">
            <div className="flex items-center gap-2">
              <Trophy className="w-4 h-4 text-amber-500" />
              <h3 className="text-sm font-bold text-gray-800">Achievements</h3>
            </div>
            <span className="text-xs text-gray-400 font-medium">{unlockedBadges.length}/{ACHIEVEMENT_BADGES.length} unlocked</span>
          </div>
          <div className="flex flex-wrap gap-3">
            {ACHIEVEMENT_BADGES.map((badge) => {
              const unlocked = unlockedBadges.some(b => b.id === badge.id)
              return (
                <div
                  key={badge.id}
                  title={badge.desc}
                  className={`flex flex-col items-center gap-1 p-3 rounded-xl border transition-all w-[72px] ${
                    unlocked
                      ? 'bg-gradient-to-b from-indigo-50 to-white border-indigo-200 shadow-sm'
                      : 'bg-gray-50 border-gray-100 opacity-40 grayscale'
                  }`}
                >
                  <span className="text-xl">{badge.icon}</span>
                  <span className="text-[9px] font-bold text-center text-gray-600 leading-tight">{badge.label}</span>
                </div>
              )
            })}
          </div>
        </motion.div>

        {/* Main Grid: Quick Actions + Upcoming + Bonus */}
        <div className="grid md:grid-cols-2 gap-5 mb-5">

          {/* Quick Actions */}
          <AnimatedCard delay={0.3}>
            <AnimatedCardContent title="Quick Actions" className="h-full">
              <div className="space-y-2.5">
                {[
                  { label: 'Find Tutors', icon: Search, route: '/student/find-tutors', color: 'hover:border-indigo-300 hover:bg-indigo-50' },
                  { label: 'My Sessions', icon: Calendar, route: '/student/my-sessions', color: 'hover:border-indigo-300 hover:bg-indigo-50' },
                  { label: 'Bonus Points', icon: Gift, route: '/student/wallet', color: 'hover:border-amber-300 hover:bg-amber-50' },
                  { label: 'Referral Program', icon: TrendingUp, route: '/student/referrals', color: 'hover:border-green-300 hover:bg-green-50' },
                ].map(({ label, icon: Icon, route, color }) => (
                  <motion.div key={route} whileHover={{ scale: 1.01 }} whileTap={{ scale: 0.99 }}>
                    <Button
                      fullWidth
                      variant="outline"
                      onClick={() => navigate(route)}
                      className={`justify-start group ${color} transition-all`}
                    >
                      <Icon className="mr-2 w-4 h-4" />
                      {label}
                    </Button>
                  </motion.div>
                ))}
              </div>
            </AnimatedCardContent>
          </AnimatedCard>

          {/* Bonus History */}
          <AnimatedCard delay={0.35}>
            <AnimatedCardContent title="Recent Rewards" className="h-full">
              {bonusSummary?.items?.length ? (
                <div className="space-y-2.5">
                  {bonusSummary.items.slice(0, 4).map((item) => (
                    <div key={item.id} className="flex items-center justify-between rounded-xl border border-gray-100 bg-gray-50/50 p-3 text-sm">
                      <div>
                        <p className="font-semibold text-gray-900 text-xs">{item.reason}</p>
                        <p className="text-[10px] text-gray-400 mt-0.5">{new Date(item.createdAt).toLocaleDateString()}</p>
                      </div>
                      <span className="text-xs font-black text-indigo-600 bg-indigo-50 border border-indigo-100 px-2 py-0.5 rounded-full">+{item.points} pts</span>
                    </div>
                  ))}
                </div>
              ) : (
                <EmptyState
                  icon={<Star className="w-7 h-7" />}
                  title="No rewards yet"
                  description="Book sessions and invite friends to earn points."
                />
              )}
            </AnimatedCardContent>
          </AnimatedCard>

          {/* Upcoming Sessions */}
          <AnimatedCard delay={0.4} className="md:col-span-2">
            <AnimatedCardContent
              title="Upcoming Sessions"
              headerAction={
                <Button variant="ghost" size="sm" onClick={() => navigate('/student/my-sessions')} className="text-indigo-600 hover:text-indigo-700">
                  View All <ArrowRight className="ml-1 w-3.5 h-3.5" />
                </Button>
              }
              className="h-full"
            >
              {(dashboardData?.upcomingSessions && dashboardData.upcomingSessions.length > 0) ? (
                <div className="grid sm:grid-cols-2 gap-3">
                  {dashboardData.upcomingSessions
                    .filter(session => {
                      const now = new Date()
                      const scheduledTime = new Date(session.scheduledAt)
                      const endTime = new Date(scheduledTime.getTime() + (session.duration || 60) * 60000)
                      return now < endTime && (session.bookingStatus === 'Confirmed' || session.bookingStatus === 'Pending')
                    })
                    .map((session, idx) => (
                      <motion.div
                        key={session.sessionId}
                        initial={{ opacity: 0, y: 10 }}
                        animate={{ opacity: 1, y: 0 }}
                        transition={{ delay: 0.5 + idx * 0.08 }}
                        whileHover={{ scale: 1.01 }}
                        className="flex items-center gap-3 p-3.5 rounded-xl border border-gray-100 bg-gradient-to-r from-white to-indigo-50/30 hover:border-indigo-200 hover:shadow-sm transition-all cursor-pointer group"
                        onClick={() => navigate(`/session/${session.sessionId}/join`)}
                      >
                        <Avatar name={session.tutorName || '-'} size="md" />
                        <div className="flex-1 min-w-0">
                          <h4 className="font-bold text-gray-900 text-sm truncate group-hover:text-indigo-700">{session.tutorName || '-'}</h4>
                          <p className="text-xs text-gray-500 truncate">{session.subject || '-'}</p>
                          <p className="text-[10px] text-gray-400 mt-0.5">
                            {new Date(session.scheduledAt).toLocaleDateString()} · {new Date(session.scheduledAt).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}
                          </p>
                        </div>
                        <Button size="sm" className="opacity-0 group-hover:opacity-100 transition-opacity text-xs px-3"
                          onClick={(e) => { e.stopPropagation(); navigate(`/session/${session.sessionId}/join`) }}>
                          Join
                        </Button>
                      </motion.div>
                    ))}
                </div>
              ) : (
                <EmptyState
                  icon={<Calendar className="w-8 h-8" />}
                  title="No upcoming sessions"
                  description="Book a session to start your learning journey"
                  action={{ label: 'Find Tutors', onClick: () => navigate('/student/find-tutors') }}
                />
              )}
            </AnimatedCardContent>
          </AnimatedCard>
        </div>

        {/* Recent Activity */}
        <AnimatedCard delay={0.5}>
          <AnimatedCardContent title="Recent Activity" className="h-full">
            <div className="space-y-3">
              {(dashboardData?.recentActivity && dashboardData.recentActivity.length > 0) ? (
                dashboardData.recentActivity.map((activity, idx) => (
                  <motion.div
                    key={idx}
                    initial={{ opacity: 0, x: -20 }}
                    animate={{ opacity: 1, x: 0 }}
                    transition={{ delay: 0.6 + idx * 0.08 }}
                    className="flex items-center gap-3 p-3.5 rounded-xl border border-gray-100 hover:border-indigo-100 hover:bg-indigo-50/30 transition-all"
                  >
                    <div className="w-10 h-10 rounded-xl bg-indigo-100 flex items-center justify-center text-indigo-600 flex-shrink-0">
                      <BookOpen className="w-5 h-5" />
                    </div>
                    <div className="flex-1 min-w-0">
                      <p className="font-medium text-gray-900 text-sm truncate">{activity.description || '-'}</p>
                      <p className="text-xs text-gray-400">{new Date(activity.timestamp).toLocaleDateString()} · {new Date(activity.timestamp).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}</p>
                    </div>
                    <Badge variant="info" className="text-[10px] flex-shrink-0">{activity.type || '-'}</Badge>
                  </motion.div>
                ))
              ) : (
                <EmptyState
                  icon={<BookOpen className="w-8 h-8" />}
                  title="No recent activity"
                  description="Your learning activities will appear here."
                />
              )}
            </div>
          </AnimatedCardContent>
        </AnimatedCard>

      </div>
    </div>
  )
}

export default StudentDashboard
