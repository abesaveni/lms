import { motion } from 'framer-motion'
import { useEffect, useState } from 'react'
import { Users, Calendar, DollarSign, TrendingUp, UserPlus, ArrowRight, Sparkles } from 'lucide-react'
import { EnhancedStatsCard } from '../../components/domain/EnhancedStatsCard'
import { AnimatedCard, AnimatedCardContent } from '../../components/ui/AnimatedCard'
import { EmptyState } from '../../components/ui/EmptyState'
import { Avatar } from '../../components/ui/Avatar'
import { Badge } from '../../components/ui/Badge'
import Button from '../../components/ui/Button'
import { useNavigate } from 'react-router-dom'
import { getAdminDashboard, AdminDashboardDto } from '../../services/adminApi'
import { AIChatAssistant } from '../../components/ai'

const AdminDashboard = () => {
  const navigate = useNavigate()
  const [dashboard, setDashboard] = useState<AdminDashboardDto | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    const loadDashboard = async () => {
      setIsLoading(true)
      setError(null)
      try {
        const data = await getAdminDashboard()
        setDashboard(data)
      } catch (err: any) {
        setError(err.message || 'Failed to load dashboard')
      } finally {
        setIsLoading(false)
      }
    }
    loadDashboard()
  }, [])

  const recentUsers = dashboard?.recentUsers || []

  const platformStats = [
    {
      label: 'Total Students',
      value: `${dashboard?.totalStudents ?? 0}`,
      subtitle: `${dashboard?.totalStudents ?? 0} registered`,
      icon: Users,
      color: 'primary',
    },
    {
      label: 'Total Tutors',
      value: `${dashboard?.totalTutors ?? 0}`,
      subtitle: `${dashboard?.totalTutors ?? 0} verified`,
      icon: UserPlus,
      color: 'success',
    },
    {
      label: 'Sessions Completed',
      value: `${dashboard?.completedSessions ?? 0}`,
      subtitle: `${dashboard?.upcomingSessions ?? 0} upcoming`,
      icon: Calendar,
      color: 'primary',
    },
  ]

  return (
    <div className="min-h-screen bg-gradient-to-br from-gray-50 via-white to-gray-50">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        {/* Header */}
        <motion.div
          initial={{ opacity: 0, y: -20 }}
          animate={{ opacity: 1, y: 0 }}
          className="mb-8"
        >
          <div className="flex items-center justify-between mb-2">
            <div>
              <h1 className="text-4xl font-bold text-gray-900 mb-2 flex items-center gap-3">
                Dashboard
                <motion.span
                  animate={{ rotate: [0, 10, -10, 0] }}
                  transition={{ duration: 2, repeat: Infinity, repeatDelay: 3 }}
                >
                  <Sparkles className="w-6 h-6 text-primary-500" />
                </motion.span>
              </h1>
              <p className="text-gray-600 text-lg">Platform overview and real-time statistics</p>
            </div>
          </div>
        </motion.div>

        {/* Primary Stats - Hero Metric */}
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ delay: 0.1 }}
          className="mb-8"
        >
          <div className="relative overflow-hidden rounded-2xl bg-gradient-to-br from-primary-500 to-primary-600 p-8 text-white shadow-xl">
            <div className="absolute inset-0 opacity-20" style={{
              backgroundImage: `url("data:image/svg+xml,%3Csvg width='60' height='60' viewBox='0 0 60 60' xmlns='http://www.w3.org/2000/svg'%3E%3Cg fill='none' fill-rule='evenodd'%3E%3Cg fill='%23ffffff' fill-opacity='0.05'%3E%3Cpath d='M36 34v-4h-2v4h-4v2h4v4h2v-4h4v-2h-4zm0-30V0h-2v4h-4v2h4v4h2V6h4V4h-4zM6 34v-4H4v4H0v2h4v4h2v-4h4v-2H6zM6 4V0H4v4H0v2h4v4h2V6h4V4H6z'/%3E%3C/g%3E%3C/g%3E%3C/svg%3E")`
            }} />
            <div className="relative">
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-primary-100 text-sm font-medium mb-2">Total Platform Revenue</p>
                  <motion.div
                    initial={{ scale: 0.9, opacity: 0 }}
                    animate={{ scale: 1, opacity: 1 }}
                    transition={{ delay: 0.3 }}
                    className="text-5xl font-bold mb-2"
                  >
                    ${dashboard?.totalRevenue?.toLocaleString() ?? '0'}
                  </motion.div>
                  <div className="flex items-center gap-2 text-primary-100">
                    <TrendingUp className="w-4 h-4" />
                    <span className="text-sm">{dashboard?.activeUsers ?? 0} active users</span>
                  </div>
                </div>
                <motion.div
                  initial={{ scale: 0 }}
                  animate={{ scale: 1 }}
                  transition={{ delay: 0.2, type: 'spring', stiffness: 200 }}
                  className="w-24 h-24 rounded-full bg-white/20 backdrop-blur-sm flex items-center justify-center"
                >
                  <DollarSign className="w-12 h-12" />
                </motion.div>
              </div>
            </div>
          </div>
        </motion.div>

        {/* Secondary Stats Grid */}
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-8">
          <EnhancedStatsCard
            title="Total Users"
            value={`${dashboard?.totalUsers ?? 0}`}
            icon={<Users className="w-6 h-6" />}
            trend={{ value: 12, isPositive: true }}
            delay={0.2}
            gradient="primary"
          />
          <EnhancedStatsCard
            title="Active Sessions"
            value={`${dashboard?.upcomingSessions ?? 0}`}
            icon={<Calendar className="w-6 h-6" />}
            delay={0.3}
            gradient="success"
          />
          <EnhancedStatsCard
            title="Tutors"
            value={`${dashboard?.totalTutors ?? 0}`}
            icon={<UserPlus className="w-6 h-6" />}
            delay={0.4}
            gradient="warning"
          />
        </div>

        {/* Content Grid */}
        <div className="grid lg:grid-cols-2 gap-6">
          {/* Recent Users */}
          <AnimatedCard delay={0.3}>
            <AnimatedCardContent
              title="Recent Users"
              headerAction={
                <Button
                  variant="ghost"
                  size="sm"
                  onClick={() => navigate('/admin/users')}
                  className="text-primary-600 hover:text-primary-700"
                >
                  View All
                  <ArrowRight className="ml-1 w-4 h-4" />
                </Button>
              }
              className="h-full"
            >
              {isLoading ? (
                <div className="text-center py-8 text-gray-500">Loading users...</div>
              ) : error ? (
                <div className="text-center py-8 text-red-600">{error}</div>
              ) : recentUsers.length > 0 ? (
                <div className="space-y-3">
                  {recentUsers.map((user, idx) => (
                    <motion.div
                      key={idx}
                      initial={{ opacity: 0, x: -20 }}
                      animate={{ opacity: 1, x: 0 }}
                      transition={{ delay: 0.4 + idx * 0.1 }}
                      className="group flex items-center gap-4 p-4 rounded-xl border border-gray-200 hover:border-primary-200 hover:bg-primary-50/50 transition-all duration-200 cursor-pointer"
                    >
                      <Avatar name={user.username} size="md" />
                      <div className="flex-1 min-w-0">
                        <h4 className="font-semibold text-gray-900 truncate">{user.username}</h4>
                        <p className="text-sm text-gray-600 truncate">{user.email}</p>
                        <p className="text-xs text-gray-500 mt-1">
                          Joined {new Date(user.createdAt).toLocaleDateString()}
                        </p>
                      </div>
                      <Badge variant={user.role === 'Tutor' ? 'info' : 'default'}>
                        {user.role}
                      </Badge>
                    </motion.div>
                  ))}
                </div>
              ) : (
                <EmptyState
                  icon={<Users className="w-8 h-8" />}
                  title="No users yet"
                  description="When users register, they'll appear here"
                />
              )}
            </AnimatedCardContent>
          </AnimatedCard>

          {/* Platform Statistics */}
          <AnimatedCard delay={0.4}>
            <AnimatedCardContent
              title="Platform Statistics"
              className="h-full"
            >
              <div className="space-y-6">
                {platformStats.map((stat, idx) => {
                  const Icon = stat.icon
                  return (
                    <motion.div
                      key={idx}
                      initial={{ opacity: 0, x: 20 }}
                      animate={{ opacity: 1, x: 0 }}
                      transition={{ delay: 0.5 + idx * 0.1 }}
                      className="flex items-center justify-between p-4 rounded-xl bg-gradient-to-r from-gray-50 to-white border border-gray-200 hover:border-primary-200 transition-colors"
                    >
                      <div className="flex items-center gap-4">
                        <div className={`w-12 h-12 rounded-xl bg-${stat.color}-100 flex items-center justify-center text-${stat.color}-600`}>
                          <Icon className="w-6 h-6" />
                        </div>
                        <div>
                          <p className="font-semibold text-gray-900">{stat.label}</p>
                          <p className="text-sm text-gray-600">{stat.subtitle}</p>
                        </div>
                      </div>
                      <span className="text-2xl font-bold text-gray-900">{stat.value}</span>
                    </motion.div>
                  )
                })}
              </div>
            </AnimatedCardContent>
          </AnimatedCard>
        </div>

        <AIChatAssistant userRole="admin" />
      </div>
    </div>
  )
}

export default AdminDashboard
