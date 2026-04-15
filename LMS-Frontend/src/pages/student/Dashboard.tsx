import { useNavigate } from 'react-router-dom'
import { motion } from 'framer-motion'
import {
  Calendar, BookOpen, Clock, Search, ArrowRight, Gift, Flame, Trophy,
  Star, Zap, TrendingUp, Sparkles, Bot, FileText, Mic, Code2,
  Briefcase, MapPin, Layout, Brain, Gamepad2, Heart, Target,
} from 'lucide-react'
import { EnhancedStatsCard } from '../../components/domain/EnhancedStatsCard'
import { AnimatedCard, AnimatedCardContent } from '../../components/ui/AnimatedCard'
import { EmptyState } from '../../components/ui/EmptyState'
import { Avatar } from '../../components/ui/Avatar'
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

const ACHIEVEMENT_BADGES = [
  { id: 'first_session', icon: '🎯', label: 'First Session', desc: 'Attended your first session', threshold: 1, field: 'totalSessionsAttended' },
  { id: 'five_sessions', icon: '⭐', label: 'Rising Star', desc: '5 sessions completed', threshold: 5, field: 'totalSessionsAttended' },
  { id: 'ten_sessions', icon: '🔥', label: 'On Fire', desc: '10 sessions completed', threshold: 10, field: 'totalSessionsAttended' },
  { id: 'reviewer', icon: '✍️', label: 'Reviewer', desc: 'Left your first review', threshold: 1, field: 'totalReviewsLeft' },
  { id: 'referrer', icon: '🤝', label: 'Connector', desc: 'Referred a friend', threshold: 1, field: 'totalReferrals' },
]

// AI Tools catalogue shown in the right panel
const AI_TOOLS = [
  { label: 'AI Tools Hub', icon: Sparkles, path: '/student/ai-tools', color: 'bg-indigo-50 text-indigo-600 border-indigo-100', desc: 'All tools in one place' },
  { label: 'Lexi Assistant', icon: Bot, path: '/student/ai-assistant', color: 'bg-purple-50 text-purple-600 border-purple-100', desc: 'Chat with your AI tutor' },
  { label: 'Resume Builder', icon: FileText, path: '/student/resume-builder', color: 'bg-blue-50 text-blue-600 border-blue-100', desc: 'ATS-optimised CV in seconds' },
  { label: 'Mock Interview', icon: Mic, path: '/student/mock-interview', color: 'bg-rose-50 text-rose-600 border-rose-100', desc: 'Practise real interview Q&A' },
  { label: 'Career Path', icon: MapPin, path: '/student/career-path', color: 'bg-green-50 text-green-600 border-green-100', desc: 'Map your dream career' },
  { label: 'Code Review', icon: Code2, path: '/student/code-review', color: 'bg-slate-50 text-slate-600 border-slate-100', desc: 'AI feedback on your code' },
  { label: 'Project Ideas', icon: Brain, path: '/student/project-ideas', color: 'bg-orange-50 text-orange-600 border-orange-100', desc: 'Curated project inspiration' },
  { label: 'LinkedIn Optimizer', icon: Briefcase, path: '/student/linkedin-optimizer', color: 'bg-sky-50 text-sky-600 border-sky-100', desc: 'Boost your profile views' },
  { label: 'Portfolio Gen', icon: Layout, path: '/student/portfolio-generator', color: 'bg-teal-50 text-teal-600 border-teal-100', desc: 'Auto-generate your portfolio' },
  { label: 'Daily Quiz', icon: Target, path: '/student/daily-quiz', color: 'bg-amber-50 text-amber-600 border-amber-100', desc: 'Sharpen your knowledge daily' },
  { label: 'Flashcards', icon: Zap, path: '/student/flashcards', color: 'bg-yellow-50 text-yellow-600 border-yellow-100', desc: 'Smart spaced-repetition' },
  { label: 'Daily Games', icon: Gamepad2, path: '/student/daily-games', color: 'bg-pink-50 text-pink-600 border-pink-100', desc: 'Learn while you play' },
  { label: 'Study Schedule', icon: Calendar, path: '/student/study-schedule', color: 'bg-cyan-50 text-cyan-600 border-cyan-100', desc: 'Plan your study week' },
  { label: 'Wellness Check', icon: Heart, path: '/student/wellness', color: 'bg-red-50 text-red-600 border-red-100', desc: 'Stay mentally healthy' },
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

  const xp = bonusSummary?.totalPoints ?? dashboardData?.totalBonusPoints ?? 0
  const { level, nextLevel, progress } = getLevelInfo(xp)

  const totalSessions = statsData?.totalSessionsAttended ?? dashboardData?.completedSessions ?? 0
  const studyStreak = Math.min(Math.floor(totalSessions / 2) + (totalSessions > 0 ? 1 : 0), 30)

  const unlockedBadges = ACHIEVEMENT_BADGES.filter(b => {
    if (b.field === 'totalSessionsAttended') return totalSessions >= b.threshold
    return false
  })

  return (
    <div className="min-h-screen bg-gray-50">
      <div className="max-w-screen-xl mx-auto px-4 py-6">

        {/* Calendar banner */}
        {showCalendarBanner && (
          <div className="mb-4 flex items-center justify-between gap-4 bg-amber-50 border border-amber-200 rounded-xl px-4 py-2.5">
            <div className="flex items-center gap-2">
              <Calendar className="w-4 h-4 text-amber-600 flex-shrink-0" />
              <p className="text-xs text-amber-800">Connect your Google Calendar to sync sessions and get reminders.</p>
            </div>
            <div className="flex items-center gap-2 flex-shrink-0">
              <button onClick={() => navigate('/calendar/connect')} className="text-xs font-semibold text-amber-700 underline">Connect</button>
              <button onClick={() => setIsCalendarConnected(true)} className="text-xs text-amber-500">Dismiss</button>
            </div>
          </div>
        )}

        {/* Two-column layout: main (left) + AI panel (right) */}
        <div className="flex gap-5">

          {/* ── LEFT: Main Content ───────────────────────────────────────────── */}
          <div className="flex-1 min-w-0 space-y-4">

            {/* Welcome + Level Badge */}
            <motion.div initial={{ opacity: 0, y: -16 }} animate={{ opacity: 1, y: 0 }}
              className="flex items-center justify-between">
              <div>
                <h1 className="text-xl font-black text-gray-900">Hey {userName}! 👋</h1>
                <p className="text-xs text-gray-500 mt-0.5">Track your progress and keep learning strong.</p>
              </div>
              <div className="hidden sm:flex items-center gap-2">
                <div className="flex items-center gap-1.5 bg-orange-50 border border-orange-100 rounded-lg px-2.5 py-1.5">
                  <Flame className="w-3.5 h-3.5 text-orange-500" />
                  <span className="text-xs font-bold text-orange-600">{studyStreak}d streak</span>
                </div>
                <div className={`flex items-center gap-1.5 bg-gradient-to-r ${level.bg} rounded-lg px-2.5 py-1.5`}>
                  <span className="text-xs">{level.icon}</span>
                  <span className="text-xs font-bold text-white">{level.name}</span>
                </div>
              </div>
            </motion.div>

            {/* XP Progress */}
            <motion.div initial={{ opacity: 0, y: 8 }} animate={{ opacity: 1, y: 0 }} transition={{ delay: 0.1 }}
              className="bg-white rounded-xl border border-gray-100 shadow-sm p-3">
              <div className="flex items-center justify-between mb-1.5">
                <div className="flex items-center gap-1.5">
                  <Zap className="w-3.5 h-3.5 text-indigo-500" />
                  <span className="text-xs font-bold text-gray-800">{xp.toLocaleString()} XP</span>
                  <span className={`text-[10px] font-bold px-1.5 py-0.5 rounded-full bg-indigo-50 ${level.color}`}>{level.icon} {level.name}</span>
                </div>
                {nextLevel && <span className="text-[10px] text-gray-400">{nextLevel.min - xp} XP to {nextLevel.name}</span>}
              </div>
              <div className="w-full h-2 bg-gray-100 rounded-full overflow-hidden">
                <motion.div initial={{ width: 0 }} animate={{ width: `${progress}%` }}
                  transition={{ duration: 1, ease: 'easeOut', delay: 0.3 }}
                  className={`h-full bg-gradient-to-r ${level.bg} rounded-full`} />
              </div>
            </motion.div>

            {/* Stats */}
            <div className="grid grid-cols-2 lg:grid-cols-4 gap-3">
              <EnhancedStatsCard title="Sessions" value={statsData?.totalSessionsAttended?.toString() || '0'} icon={<Calendar className="w-5 h-5" />} delay={0.15} gradient="primary" description="All time" />
              <EnhancedStatsCard title="Completed" value={dashboardData?.completedSessions?.toString() || '0'} icon={<BookOpen className="w-5 h-5" />} delay={0.2} gradient="success" description="Past sessions" />
              <EnhancedStatsCard title="Hours" value={statsData?.totalHoursLearned?.toString() || '0'} icon={<Clock className="w-5 h-5" />} delay={0.25} gradient="primary" description="Learning time" />
              <EnhancedStatsCard title="Points" value={`${bonusSummary?.totalPoints?.toLocaleString() ?? '0'}`} icon={<Gift className="w-5 h-5" />} delay={0.3} gradient="success" description="Redeemable" />
            </div>

            {/* Achievements */}
            <motion.div initial={{ opacity: 0, y: 12 }} animate={{ opacity: 1, y: 0 }} transition={{ delay: 0.2 }}
              className="bg-white rounded-xl border border-gray-100 shadow-sm p-3">
              <div className="flex items-center justify-between mb-2">
                <div className="flex items-center gap-1.5">
                  <Trophy className="w-3.5 h-3.5 text-amber-500" />
                  <span className="text-xs font-bold text-gray-800">Achievements</span>
                </div>
                <span className="text-[10px] text-gray-400">{unlockedBadges.length}/{ACHIEVEMENT_BADGES.length} unlocked</span>
              </div>
              <div className="flex gap-2 flex-wrap">
                {ACHIEVEMENT_BADGES.map((badge) => {
                  const unlocked = unlockedBadges.some(b => b.id === badge.id)
                  return (
                    <div key={badge.id} title={badge.desc}
                      className={`flex flex-col items-center gap-0.5 p-2 rounded-lg border w-16 transition-all ${
                        unlocked ? 'bg-indigo-50 border-indigo-200' : 'bg-gray-50 border-gray-100 opacity-40 grayscale'
                      }`}>
                      <span className="text-base">{badge.icon}</span>
                      <span className="text-[9px] font-bold text-center text-gray-600 leading-tight">{badge.label}</span>
                    </div>
                  )
                })}
              </div>
            </motion.div>

            {/* Quick Actions + Recent Rewards */}
            <div className="grid md:grid-cols-2 gap-4">
              <AnimatedCard delay={0.25}>
                <AnimatedCardContent title="Quick Actions" className="h-full">
                  <div className="space-y-1.5">
                    {[
                      { label: 'Find Tutors', icon: Search, route: '/student/find-tutors', color: 'hover:border-indigo-200 hover:bg-indigo-50' },
                      { label: 'My Sessions', icon: Calendar, route: '/student/my-sessions', color: 'hover:border-indigo-200 hover:bg-indigo-50' },
                      { label: 'Bonus Points', icon: Gift, route: '/student/wallet', color: 'hover:border-amber-200 hover:bg-amber-50' },
                      { label: 'Referral Program', icon: TrendingUp, route: '/student/referrals', color: 'hover:border-green-200 hover:bg-green-50' },
                    ].map(({ label, icon: Icon, route, color }) => (
                      <Button key={route} fullWidth variant="outline" onClick={() => navigate(route)}
                        className={`justify-start ${color} transition-all text-xs`}>
                        <Icon className="mr-2 w-3.5 h-3.5" />{label}
                      </Button>
                    ))}
                  </div>
                </AnimatedCardContent>
              </AnimatedCard>

              <AnimatedCard delay={0.3}>
                <AnimatedCardContent title="Recent Rewards" className="h-full">
                  {bonusSummary?.items?.length ? (
                    <div className="space-y-1.5">
                      {bonusSummary.items.slice(0, 4).map((item) => (
                        <div key={item.id} className="flex items-center justify-between rounded-lg border border-gray-100 bg-gray-50/50 px-2.5 py-2 text-xs">
                          <div>
                            <p className="font-semibold text-gray-900">{item.reason}</p>
                            <p className="text-[10px] text-gray-400 mt-0.5">{new Date(item.createdAt).toLocaleDateString()}</p>
                          </div>
                          <span className="text-[10px] font-black text-indigo-600 bg-indigo-50 border border-indigo-100 px-1.5 py-0.5 rounded-full">+{item.points}</span>
                        </div>
                      ))}
                    </div>
                  ) : (
                    <EmptyState icon={<Star className="w-6 h-6" />} title="No rewards yet" description="Book sessions and invite friends to earn points." />
                  )}
                </AnimatedCardContent>
              </AnimatedCard>
            </div>

            {/* Upcoming Sessions */}
            <AnimatedCard delay={0.35}>
              <AnimatedCardContent title="Upcoming Sessions"
                headerAction={
                  <Button variant="ghost" size="sm" onClick={() => navigate('/student/my-sessions')} className="text-indigo-600 text-xs">
                    View All <ArrowRight className="ml-1 w-3 h-3" />
                  </Button>
                }>
                {(dashboardData?.upcomingSessions && dashboardData.upcomingSessions.length > 0) ? (
                  <div className="grid sm:grid-cols-2 gap-2">
                    {dashboardData.upcomingSessions
                      .filter(s => {
                        const end = new Date(new Date(s.scheduledAt).getTime() + (s.duration || 60) * 60000)
                        return new Date() < end && (s.bookingStatus === 'Confirmed' || s.bookingStatus === 'Pending')
                      })
                      .map((session, idx) => (
                        <motion.div key={session.sessionId}
                          initial={{ opacity: 0, y: 8 }} animate={{ opacity: 1, y: 0 }} transition={{ delay: 0.4 + idx * 0.06 }}
                          className="flex items-center gap-2.5 p-2.5 rounded-lg border border-gray-100 bg-gradient-to-r from-white to-indigo-50/20 hover:border-indigo-200 hover:shadow-sm transition-all cursor-pointer group"
                          onClick={() => navigate(`/session/${session.sessionId}/join`)}>
                          <Avatar name={session.tutorName || '-'} size="sm" />
                          <div className="flex-1 min-w-0">
                            <h4 className="font-bold text-gray-900 text-xs truncate">{session.tutorName || '-'}</h4>
                            <p className="text-[10px] text-gray-400">{new Date(session.scheduledAt).toLocaleDateString()} · {new Date(session.scheduledAt).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}</p>
                          </div>
                          <Button size="sm" className="opacity-0 group-hover:opacity-100 transition-opacity text-[10px] px-2 py-1"
                            onClick={e => { e.stopPropagation(); navigate(`/session/${session.sessionId}/join`) }}>Join</Button>
                        </motion.div>
                      ))}
                  </div>
                ) : (
                  <EmptyState icon={<Calendar className="w-7 h-7" />} title="No upcoming sessions"
                    description="Book a session to start learning"
                    action={{ label: 'Find Tutors', onClick: () => navigate('/student/find-tutors') }} />
                )}
              </AnimatedCardContent>
            </AnimatedCard>

          </div>

          {/* ── RIGHT: AI Tools Panel ─────────────────────────────────────────── */}
          <aside className="hidden lg:flex flex-col w-64 xl:w-72 flex-shrink-0 space-y-4">

            {/* AI Tools Header */}
            <motion.div initial={{ opacity: 0, x: 20 }} animate={{ opacity: 1, x: 0 }} transition={{ delay: 0.15 }}
              className="bg-gradient-to-br from-indigo-600 to-purple-700 rounded-xl p-4 text-white">
              <div className="flex items-center gap-2 mb-1">
                <Sparkles className="w-4 h-4" />
                <span className="text-sm font-black">AI Power Tools</span>
              </div>
              <p className="text-[10px] text-indigo-200">Supercharge your learning with AI.</p>
            </motion.div>

            {/* Tool Grid */}
            <div className="bg-white rounded-xl border border-gray-100 shadow-sm p-3 flex-1 overflow-y-auto">
              <div className="grid grid-cols-2 gap-2">
                {AI_TOOLS.map((tool, i) => {
                  const Icon = tool.icon
                  return (
                    <motion.button
                      key={tool.path}
                      initial={{ opacity: 0, scale: 0.95 }}
                      animate={{ opacity: 1, scale: 1 }}
                      transition={{ delay: 0.1 + i * 0.03 }}
                      whileHover={{ scale: 1.03 }}
                      whileTap={{ scale: 0.97 }}
                      onClick={() => navigate(tool.path)}
                      className={`flex flex-col items-center gap-1.5 p-2.5 rounded-lg border text-center transition-all hover:shadow-sm ${tool.color}`}
                    >
                      <div className="w-7 h-7 rounded-lg flex items-center justify-center bg-white/70">
                        <Icon className="w-3.5 h-3.5" />
                      </div>
                      <span className="text-[10px] font-bold leading-tight">{tool.label}</span>
                    </motion.button>
                  )
                })}
              </div>
            </div>

            {/* Subscription CTA */}
            <motion.div initial={{ opacity: 0, x: 20 }} animate={{ opacity: 1, x: 0 }} transition={{ delay: 0.3 }}
              className="bg-gradient-to-br from-amber-50 to-orange-50 border border-amber-100 rounded-xl p-3">
              <p className="text-xs font-bold text-amber-800 mb-0.5">Unlock All AI Tools</p>
              <p className="text-[10px] text-amber-600 mb-2">Get unlimited AI assistance with a subscription plan.</p>
              <Button size="sm" onClick={() => navigate('/student/subscription')}
                className="w-full text-[10px] bg-amber-500 hover:bg-amber-600 text-white border-0">
                View Plans
              </Button>
            </motion.div>

          </aside>

        </div>
      </div>
    </div>
  )
}

export default StudentDashboard
