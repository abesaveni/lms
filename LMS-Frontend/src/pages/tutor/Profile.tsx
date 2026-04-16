import { useEffect, useState } from 'react'
import { Calendar, Mail, Phone, MapPin, Edit, Star,  } from 'lucide-react'
import { Card, CardHeader, CardTitle, CardContent } from '../../components/ui/Card'
import { Avatar } from '../../components/ui/Avatar'
import { Badge } from '../../components/ui/Badge'
import Button from '../../components/ui/Button'
import { useNavigate } from 'react-router-dom'
import { getTutorProfile, getTutorDashboardStats, TutorProfileDto } from '../../services/tutorApi'
import { getMediaUrl } from '../../services/api'

const TutorProfile = () => {
  const navigate = useNavigate()
  const [profile, setProfile] = useState<TutorProfileDto | null>(null)
  const [loading, setLoading] = useState(true)
  const [totalEarnings, setTotalEarnings] = useState<number | null>(null)
  const [totalStudents, setTotalStudents] = useState(0)
  const [completedSessions, setCompletedSessions] = useState(0)
  const [averageRating, setAverageRating] = useState(0)

  useEffect(() => {
    const fetchAll = async () => {
      try {
        const [data, stats] = await Promise.all([
          getTutorProfile(),
          getTutorDashboardStats().catch(() => null),
        ])
        setProfile(data)
        if (stats) {
          setTotalStudents(stats.totalStudents)
          setCompletedSessions(stats.completedSessions)
          setAverageRating(stats.averageRating)
          setTotalEarnings(stats.totalEarnings)
        }
      } catch (error) {
        console.error('Failed to fetch profile:', error)
      } finally {
        setLoading(false)
      }
    }
    fetchAll()
  }, [])

  if (loading) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-primary-600"></div>
      </div>
    )
  }

  const stats = [
    { label: 'Total Students', value: totalStudents },
    { label: 'Sessions Completed', value: completedSessions },
    { label: 'Average Rating', value: averageRating > 0 ? averageRating.toFixed(1) : (profile?.averageRating || 0) },
    { label: 'Total Earnings', value: totalEarnings !== null ? `₹${totalEarnings.toLocaleString('en-IN')}` : '...' },
  ]

  const fullName = profile ? `${profile.firstName} ${profile.lastName}`.trim() : '-'
  const profileImageUrl = getMediaUrl(profile?.profilePictureUrl)

  return (
    <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <div className="mb-8">
        <h1 className="text-3xl font-bold text-gray-900 mb-2">My Profile</h1>
        <p className="text-gray-600">View and manage your tutor profile</p>
      </div>

      <div className="grid lg:grid-cols-3 gap-6">
        {/* Profile Header */}
        <div className="lg:col-span-2">
          <Card>
            <CardContent className="pt-6">
              <div className="flex flex-col md:flex-row items-start gap-6">
                <Avatar
                  name={fullName}
                  src={profileImageUrl}
                  size="xl"
                />
                <div className="flex-1">
                  <div className="flex items-start justify-between mb-4">
                    <div>
                      <h2 className="text-2xl font-bold text-gray-900 mb-1">{fullName}</h2>
                      <p className="text-gray-600 mb-2">{profile?.headline || '-'}</p>
                      <div className="flex items-center gap-2 mb-2">
                        <div className="flex items-center gap-1">
                          <Star className="w-4 h-4 fill-yellow-400 text-yellow-400" />
                          <span className="font-semibold">{profile?.averageRating || 0}</span>
                          <span className="text-gray-600">({profile?.totalReviews || 0} reviews)</span>
                        </div>
                      </div>
                      <Badge variant={profile?.verificationStatus === 'Approved' ? 'success' : 'warning'}>
                        {profile?.verificationStatus || 'Pending'}
                      </Badge>
                    </div>
                    <Button variant="outline" onClick={() => navigate('/tutor/profile-settings')}>
                      <Edit className="mr-2 w-4 h-4" />
                      Edit Profile
                    </Button>
                  </div>
                  <div className="grid md:grid-cols-2 gap-4">
                    <div className="flex items-center gap-3 text-gray-600">
                      <Calendar className="w-5 h-5" />
                      <span>{profile?.memberSince ? `Member since ${new Date(profile.memberSince).toLocaleDateString('en-IN', { year: 'numeric', month: 'long' })}` : '-'}</span>
                    </div>
                    <div className="flex items-center gap-3 text-gray-600">
                      <Phone className="w-5 h-5" />
                      <span>{profile?.phoneNumber || '-'}</span>
                    </div>
                    <div className="flex items-center gap-3 text-gray-600">
                      <MapPin className="w-5 h-5" />
                      <span>{profile?.location || 'Location not set'}</span>
                    </div>
                    <div className="flex items-center gap-3 text-gray-600">
                      <Mail className="w-5 h-5" />
                      <span>{profile?.email || '-'}</span>
                    </div>
                  </div>
                </div>
              </div>
            </CardContent>
          </Card>

          {/* Bio Section */}
          <Card className="mt-6">
            <CardHeader>
              <CardTitle>About Me</CardTitle>
            </CardHeader>
            <CardContent>
              <p className="text-gray-700 leading-relaxed mb-4">
                {profile?.bio || 'No bio provided.'}
              </p>
              <div>
                <h4 className="font-semibold text-gray-900 mb-2">Education</h4>
                <p className="text-gray-700">{profile?.education || '-'}</p>
              </div>
            </CardContent>
          </Card>

          {/* Specializations */}
          <Card className="mt-6">
            <CardHeader>
              <CardTitle>Specializations</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="flex flex-wrap gap-2">
                {profile?.skills ? profile.skills.split(',').map((skill, i) => (
                  <Badge key={i} variant="info">{skill.trim()}</Badge>
                )) : (
                  <span className="text-gray-500 text-sm">No specializations listed</span>
                )}
              </div>
            </CardContent>
          </Card>

          {/* Pricing */}
          <Card className="mt-6">
            <CardHeader>
              <CardTitle>Pricing</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="grid md:grid-cols-2 gap-4">
                <div className="p-4 border border-gray-200 rounded-lg">
                  <p className="text-sm text-gray-600 mb-1">1-on-1 Session</p>
                  <p className="text-2xl font-bold text-gray-900">₹{profile?.hourlyRate || 0}/hr</p>
                </div>
                <div className="p-4 border border-gray-200 rounded-lg">
                  <p className="text-sm text-gray-600 mb-1">Group Session</p>
                  <p className="text-2xl font-bold text-gray-900">{profile?.hourlyRateGroup ? `₹${profile.hourlyRateGroup}/hr` : '-'}</p>
                </div>
              </div>
            </CardContent>
          </Card>
        </div>

        {/* Stats Sidebar */}
        <div className="space-y-6">
          <Card>
            <CardHeader>
              <CardTitle>Statistics</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="space-y-4">
                {stats.map((stat, idx) => (
                  <div key={idx} className="flex items-center justify-between p-3 bg-gray-50 rounded-lg">
                    <span className="text-sm text-gray-600">{stat.label}</span>
                    <span className="text-lg font-bold text-gray-900">{stat.value}</span>
                  </div>
                ))}
              </div>
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <CardTitle>Quick Actions</CardTitle>
            </CardHeader>
            <CardContent className="space-y-2">
              <Button variant="outline" fullWidth onClick={() => navigate('/tutor/profile-settings')}>
                Account Settings
              </Button>
              <Button variant="outline" fullWidth onClick={() => navigate('/tutor/sessions')}>
                My Sessions
              </Button>
            </CardContent>
          </Card>
        </div>
      </div>
    </div>
  )
}

export default TutorProfile
