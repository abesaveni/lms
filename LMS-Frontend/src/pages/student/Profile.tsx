import { Calendar, Mail, Phone, MapPin, Edit } from 'lucide-react'
import { Card, CardHeader, CardTitle, CardContent } from '../../components/ui/Card'
import { Avatar } from '../../components/ui/Avatar'
import { Badge } from '../../components/ui/Badge'
import Button from '../../components/ui/Button'
import { useNavigate } from 'react-router-dom'
import { useState, useEffect } from 'react'
import { getCurrentUserProfile, UserProfileDto } from '../../services/usersApi'
import { getStudentStats, StudentStatsDto } from '../../services/studentApi'



import { getMediaUrl } from '../../services/api'

const StudentProfile = () => {
  const navigate = useNavigate()

  const [profile, setProfile] = useState<UserProfileDto | null>(null)
  const [statsData, setStatsData] = useState<StudentStatsDto | null>(null)

  useEffect(() => {
    const fetchProfile = async () => {
      try {
        const data = await getCurrentUserProfile()
        setProfile(data)
        const sData = await getStudentStats()
        setStatsData(sData)
      } catch (err) {
        console.error(err)
      }
    }

    fetchProfile()

    window.addEventListener('profileUpdated', fetchProfile)
    window.addEventListener('tokenUpdated', fetchProfile)
    return () => {
      window.removeEventListener('profileUpdated', fetchProfile)
      window.removeEventListener('tokenUpdated', fetchProfile)
    }
  }, [])


  const stats = [
    { label: 'Sessions Completed', value: statsData?.totalSessionsAttended?.toString() || '0' },
    { label: 'Learning Hours', value: statsData?.totalHoursLearned?.toString() || '0' },
    { label: 'Certificates', value: '-' },
  ]

  const profileImageUrl = getMediaUrl(profile?.profileImageUrl);

  return (
    <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <div className="mb-8">
        <h1 className="text-3xl font-bold text-gray-900 mb-2">My Profile</h1>
        <p className="text-gray-600">View and manage your profile information</p>
      </div>

      <div className="grid lg:grid-cols-3 gap-6">
        {/* Profile Header */}
        <div className="lg:col-span-2">
          <Card>
            <CardContent className="pt-6">
              <div className="flex flex-col md:flex-row items-start gap-6">
                <Avatar name={profile ? `${profile.firstName || ''} ${profile.lastName || ''}`.trim() || profile.username : '-'} src={profileImageUrl} size="xl" />
                <div className="flex-1">
                  <div className="flex items-start justify-between mb-4">
                    <div>
                      <h2 className="text-2xl font-bold text-gray-900 mb-1">{profile ? `${profile.firstName || ''} ${profile.lastName || ''}`.trim() || profile.username : '-'}</h2>
                      <p className="text-gray-600 mb-2">{profile?.email || '-'}</p>
                      <Badge variant="success">Active</Badge>
                    </div>
                    <Button variant="outline" onClick={() => navigate('/student/profile-settings')}>
                      <Edit className="mr-2 w-4 h-4" />
                      Edit Profile
                    </Button>
                  </div>
                  <div className="grid md:grid-cols-2 gap-4">
                    <div className="flex items-center gap-3 text-gray-600">
                      <Calendar className="w-5 h-5" />
                      <span>{`Member since ${profile?.createdAt ? new Date(profile.createdAt).toLocaleDateString(undefined, { month: 'long', year: 'numeric' }) : '-'}`}</span>
                    </div>
                    <div className="flex items-center gap-3 text-gray-600">
                      <Phone className="w-5 h-5" />
                      <span>{profile?.phoneNumber || '-'}</span>
                    </div>
                    <div className="flex items-center gap-3 text-gray-600">
                      <MapPin className="w-5 h-5" />
                      <span>{profile?.location || 'Not specified'}</span>
                    </div>
                    <div className="flex items-center gap-3 text-gray-600">
                      <Mail className="w-6 h-6" />
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
              <p className="text-gray-700 leading-relaxed">
                {profile?.bio || '-'}
              </p>
            </CardContent>
          </Card>

          {/* Learning Goals */}
          <Card className="mt-6">
            <CardHeader>
              <CardTitle>Learning Goals</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="flex flex-wrap gap-2">
                {profile?.studentProfile?.preferredSubjects || profile?.bio ? (
                  (profile?.studentProfile?.preferredSubjects || profile?.bio || '').split(',').map((subject: string, idx: number) => (


                    <Badge key={idx}>{subject.trim()}</Badge>
                  ))
                ) : (
                  <span className="text-gray-500 italic">No learning goals defined</span>
                )}
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
              <Button variant="outline" fullWidth onClick={() => navigate('/student/profile-settings')}>
                Account Settings
              </Button>
              <Button variant="outline" fullWidth onClick={() => navigate('/student/my-sessions')}>
                My Sessions
              </Button>
            </CardContent>
          </Card>
        </div>
      </div>
    </div>
  )
}

export default StudentProfile
