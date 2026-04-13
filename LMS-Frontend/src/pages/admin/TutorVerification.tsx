import { useEffect, useState } from 'react'
import { CheckCircle, XCircle, Eye, Download, Play } from 'lucide-react'
import { Card, CardHeader, CardTitle, CardContent } from '../../components/ui/Card'
import Button from '../../components/ui/Button'
import { Badge } from '../../components/ui/Badge'
import { Avatar } from '../../components/ui/Avatar'
import { Tabs, TabsList, TabsTrigger, TabsContent } from '../../components/ui/Tabs'
import {
  getPendingTutorVerifications,
  getVerifiedTutors,
  approveTutorVerification,
  rejectTutorVerification,
  TutorVerificationDto,
  VerifiedTutorDto,
} from '../../services/adminApi'

const TutorVerification = () => {
  const [pendingTutors, setPendingTutors] = useState<TutorVerificationDto[]>([])
  const [verifiedTutors, setVerifiedTutors] = useState<VerifiedTutorDto[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    const loadData = async () => {
      setIsLoading(true)
      setError(null)
      try {
        const [pending, verified] = await Promise.all([
          getPendingTutorVerifications(),
          getVerifiedTutors(),
        ])
        setPendingTutors(pending)
        setVerifiedTutors(verified)
      } catch (err: any) {
        setError(err.message || 'Failed to load verifications')
      } finally {
        setIsLoading(false)
      }
    }
    loadData()
  }, [])

  const handleApprove = async (verificationId: string) => {
    if (!confirm('Approve this tutor? They will be able to create sessions.')) return
    try {
      await approveTutorVerification(verificationId)
      const pending = await getPendingTutorVerifications()
      const verified = await getVerifiedTutors()
      setPendingTutors(pending)
      setVerifiedTutors(verified)
    } catch (err: any) {
      setError(err.message || 'Failed to approve tutor')
    }
  }

  const handleReject = async (verificationId: string) => {
    const reason = prompt('Enter rejection reason:')
    if (!reason) return
    try {
      await rejectTutorVerification(verificationId, reason)
      const pending = await getPendingTutorVerifications()
      setPendingTutors(pending)
    } catch (err: any) {
      setError(err.message || 'Failed to reject tutor')
    }
  }

  return (
    <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <div className="mb-8">
        <h1 className="text-3xl font-bold text-gray-900 mb-2">Tutor Verification</h1>
        <p className="text-gray-600">Review and verify tutor applications</p>
      </div>

      <Tabs defaultValue="pending">
        <TabsList>
          <TabsTrigger value="pending">
            Pending ({pendingTutors.length})
          </TabsTrigger>
          <TabsTrigger value="verified">
            Verified ({verifiedTutors.length})
          </TabsTrigger>
        </TabsList>

        <TabsContent value="pending">
          <div className="space-y-6">
            {isLoading ? (
              <div className="text-gray-500">Loading...</div>
            ) : error ? (
              <div className="text-red-600">{error}</div>
            ) : pendingTutors.length === 0 ? (
              <div className="text-gray-500">No pending tutors</div>
            ) : pendingTutors.map((tutor) => (
              <Card key={tutor.id} hover>
                <CardHeader>
                  <div className="flex items-start justify-between">
                    <div className="flex items-center gap-4">
                      <Avatar name={tutor.tutorName} size="lg" />
                      <div>
                        <h3 className="text-xl font-semibold text-gray-900">{tutor.tutorName}</h3>
                        <p className="text-gray-600">{tutor.tutorEmail}</p>
                        <p className="text-sm text-gray-500 mt-1">
                          Submitted: {new Date(tutor.submittedAt).toLocaleDateString()}
                        </p>
                      </div>
                    </div>
                    <Badge variant="warning">Pending</Badge>
                  </div>
                </CardHeader>
                <CardContent>
                  <div className="grid md:grid-cols-2 gap-6 mb-6">
                    <div>
                      <h4 className="font-semibold text-gray-900 mb-3">Profile Information</h4>
                      <div className="space-y-2 text-sm">
                        <div>
                          <span className="font-medium text-gray-700">Skills:</span>
                          <p className="text-gray-600">{tutor.skills}</p>
                        </div>
                        <div>
                          <span className="font-medium text-gray-700">Experience:</span>
                          <p className="text-gray-600">{tutor.experience} years</p>
                        </div>
                        <div>
                          <span className="font-medium text-gray-700">Education:</span>
                          <p className="text-gray-600">{tutor.education}</p>
                        </div>
                        <div>
                          <span className="font-medium text-gray-700">Certifications:</span>
                          <p className="text-gray-600">{tutor.certifications}</p>
                        </div>
                      </div>
                    </div>
                    <div>
                      <h4 className="font-semibold text-gray-900 mb-3">Documents & Media</h4>
                      <div className="space-y-3">
                        {tutor.resumeUrl && (
                          <a
                            href={tutor.resumeUrl}
                            target="_blank"
                            rel="noopener noreferrer"
                            className="flex items-center gap-2 p-2 border border-gray-200 rounded-lg hover:bg-gray-50"
                          >
                            <Download className="w-4 h-4 text-gray-600" />
                            <span className="text-sm text-gray-700">View Resume</span>
                          </a>
                        )}
                        {tutor.introVideoUrl && (
                          <a
                            href={tutor.introVideoUrl}
                            target="_blank"
                            rel="noopener noreferrer"
                            className="flex items-center gap-2 p-2 border border-gray-200 rounded-lg hover:bg-gray-50"
                          >
                            <Play className="w-4 h-4 text-gray-600" />
                            <span className="text-sm text-gray-700">Watch Intro Video</span>
                          </a>
                        )}
                        {tutor.govtIdUrl && (
                          <a
                            href={tutor.govtIdUrl}
                            target="_blank"
                            rel="noopener noreferrer"
                            className="flex items-center gap-2 p-2 border border-gray-200 rounded-lg hover:bg-gray-50"
                          >
                            <Eye className="w-4 h-4 text-gray-600" />
                            <span className="text-sm text-gray-700">View Government ID</span>
                          </a>
                        )}
                      </div>
                    </div>
                  </div>

                  <div className="flex gap-3 pt-4 border-t border-gray-200">
                    <Button
                      onClick={() => handleApprove(tutor.id)}
                      className="flex-1"
                    >
                      <CheckCircle className="mr-2 w-4 h-4" />
                      Approve
                    </Button>
                    <Button
                      variant="outline"
                      onClick={() => handleReject(tutor.id)}
                      className="flex-1"
                    >
                      <XCircle className="mr-2 w-4 h-4" />
                      Reject
                    </Button>
                  </div>
                </CardContent>
              </Card>
            ))}
          </div>
        </TabsContent>

        <TabsContent value="verified">
          <Card>
            <CardHeader>
              <CardTitle>Verified Tutors</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="overflow-x-auto">
                <table className="w-full">
                  <thead>
                    <tr className="border-b border-gray-200">
                      <th className="text-left py-3 px-4 text-sm font-semibold text-gray-700">Tutor</th>
                      <th className="text-left py-3 px-4 text-sm font-semibold text-gray-700">Email</th>
                      <th className="text-left py-3 px-4 text-sm font-semibold text-gray-700">Verified At</th>
                      <th className="text-left py-3 px-4 text-sm font-semibold text-gray-700">Verified By</th>
                      <th className="text-left py-3 px-4 text-sm font-semibold text-gray-700">Status</th>
                    </tr>
                  </thead>
                  <tbody>
                    {isLoading ? (
                      <tr>
                        <td colSpan={5} className="py-6 text-center text-gray-500">
                          Loading...
                        </td>
                      </tr>
                    ) : verifiedTutors.length === 0 ? (
                      <tr>
                        <td colSpan={5} className="py-6 text-center text-gray-500">
                          No verified tutors
                        </td>
                      </tr>
                    ) : verifiedTutors.map((tutor) => (
                      <tr key={tutor.id} className="border-b border-gray-100 hover:bg-gray-50">
                        <td className="py-4 px-4">
                          <div className="flex items-center gap-3">
                            <Avatar name={tutor.name} size="sm" />
                            <span className="font-medium text-gray-900">{tutor.name}</span>
                          </div>
                        </td>
                        <td className="py-4 px-4 text-sm text-gray-600">{tutor.email}</td>
                        <td className="py-4 px-4 text-sm text-gray-600">
                          {new Date(tutor.verifiedAt).toLocaleDateString()}
                        </td>
                        <td className="py-4 px-4 text-sm text-gray-600">{tutor.verifiedBy}</td>
                        <td className="py-4 px-4">
                          <Badge variant="success">Verified</Badge>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>
    </div>
  )
}

export default TutorVerification
