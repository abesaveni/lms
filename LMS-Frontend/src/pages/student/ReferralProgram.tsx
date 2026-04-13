import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { Gift, Copy, Check } from 'lucide-react'
import { Card, CardHeader, CardTitle, CardContent } from '../../components/ui/Card'
import Button from '../../components/ui/Button'
import { Badge } from '../../components/ui/Badge'
import { getReferralCode, getReferralHistory, getReferralStats, ReferralHistoryDto } from '../../services/referralsApi'

const ReferralProgram = () => {
  const [referralCode, setReferralCode] = useState('')
  const [stats, setStats] = useState({
    totalReferrals: 0,
    successfulReferrals: 0,
    totalEarnings: 0,
    pendingEarnings: 0,
  })
  const [history, setHistory] = useState<ReferralHistoryDto[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [isCopied, setIsCopied] = useState(false)
  const [isLinkCopied, setIsLinkCopied] = useState(false)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    const loadReferralData = async () => {
      try {
        const [code, statsData, historyData] = await Promise.all([
          getReferralCode(),
          getReferralStats(),
          getReferralHistory(),
        ])
        setReferralCode(code.referralCode)
        setStats({
          totalReferrals: statsData.totalReferrals,
          successfulReferrals: statsData.successfulReferrals,
          totalEarnings: statsData.totalEarnings,
          pendingEarnings: statsData.pendingEarnings,
        })
        setHistory(historyData)
      } catch (err: any) {
        setError(err.message || 'Failed to load referral code')
      } finally {
        setIsLoading(false)
      }
    }

    loadReferralData()
  }, [])

  const referralLink = referralCode
    ? `${window.location.origin}/register?role=student&ref=${encodeURIComponent(referralCode)}`
    : ''

  const handleCopy = async () => {
    if (!referralCode) return
    try {
      await navigator.clipboard.writeText(referralCode)
      setIsCopied(true)
      setTimeout(() => setIsCopied(false), 2000)
    } catch {
      setIsCopied(false)
    }
  }

  const handleCopyLink = async () => {
    if (!referralLink) return
    try {
      await navigator.clipboard.writeText(referralLink)
      setIsLinkCopied(true)
      setTimeout(() => setIsLinkCopied(false), 2000)
    } catch {
      setIsLinkCopied(false)
    }
  }

  const handleWhatsAppShare = () => {
    if (!referralLink) return
    const message = `Join LiveExpert using my referral code ${referralCode}: ${referralLink}`
    const url = `https://wa.me/?text=${encodeURIComponent(message)}`
    window.open(url, '_blank', 'noopener,noreferrer')
  }

  return (
    <div className="max-w-5xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <div className="mb-8">
        <h1 className="text-3xl font-bold text-gray-900 mb-2">Referral Program</h1>
        <p className="text-gray-600">Invite students and earn bonus points after they book a session.</p>
      </div>

      <div className="grid gap-6">
        <Card>
          <CardHeader>
            <CardTitle>Your referral code</CardTitle>
          </CardHeader>
          <CardContent>
            {error && <div className="mb-4 text-sm text-red-600">{error}</div>}
            <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
              <div className="text-2xl font-semibold tracking-wider text-gray-900">
                {referralCode || '—'}
              </div>
              <Button
                onClick={handleCopy}
                disabled={!referralCode}
                variant="outline"
                className="w-full sm:w-auto"
              >
                {isCopied ? <Check className="w-4 h-4 mr-2" /> : <Copy className="w-4 h-4 mr-2" />}
                {isCopied ? 'Copied' : 'Copy code'}
              </Button>
            </div>
            <div className="mt-4 flex flex-col sm:flex-row gap-3">
              <Button
                onClick={handleCopyLink}
                disabled={!referralLink}
                variant="outline"
                className="w-full sm:w-auto"
              >
                {isLinkCopied ? <Check className="w-4 h-4 mr-2" /> : <Copy className="w-4 h-4 mr-2" />}
                {isLinkCopied ? 'Link copied' : 'Copy referral link'}
              </Button>
              <Button
                onClick={handleWhatsAppShare}
                disabled={!referralLink}
                className="w-full sm:w-auto"
              >
                Share on WhatsApp
              </Button>
            </div>
          </CardContent>
        </Card>

        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
          <Card>
            <CardHeader>
              <CardTitle>Total referrals</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="text-3xl font-semibold text-gray-900">
                {stats.totalReferrals.toLocaleString()}
              </div>
            </CardContent>
          </Card>
          <Card>
            <CardHeader>
              <CardTitle>Bonus Points</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="flex items-center justify-between text-sm text-gray-700">
                <span>Paid</span>
                <span className="font-semibold">₹{stats.totalEarnings.toLocaleString()}</span>
              </div>
              <div className="flex items-center justify-between text-sm text-gray-700 mt-2">
                <span>Pending</span>
                <span className="font-semibold">₹{stats.pendingEarnings.toLocaleString()}</span>
              </div>
            </CardContent>
          </Card>
        </div>

        <Card>
          <CardHeader>
            <CardTitle>Referral History</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="overflow-x-auto">
              <table className="w-full">
                <thead>
                  <tr className="border-b border-gray-200">
                    <th className="text-left py-3 px-4 text-sm font-semibold text-gray-700">Student</th>
                    <th className="text-left py-3 px-4 text-sm font-semibold text-gray-700">Referred At</th>
                    <th className="text-left py-3 px-4 text-sm font-semibold text-gray-700">Reward</th>
                    <th className="text-left py-3 px-4 text-sm font-semibold text-gray-700">Status</th>
                  </tr>
                </thead>
                <tbody>
                  {isLoading ? (
                    <tr>
                      <td colSpan={4} className="py-6 text-center text-gray-500">
                        Loading referrals...
                      </td>
                    </tr>
                  ) : history.length ? (
                    history.map((item) => (
                      <tr key={item.id} className="border-b border-gray-100 hover:bg-gray-50">
                        <td className="py-4 px-4 text-sm font-medium text-gray-900">{item.referredUserName}</td>
                        <td className="py-4 px-4 text-sm text-gray-600">
                          {new Date(item.referredAt).toLocaleDateString()}
                        </td>
                        <td className="py-4 px-4 text-sm text-gray-900">
                          ₹{item.reward.toLocaleString()}
                        </td>
                        <td className="py-4 px-4">
                          <Badge variant={item.isRewardClaimed ? 'success' : 'warning'}>
                            {item.status}
                          </Badge>
                          {!item.isRewardClaimed && (
                            <p className="text-xs text-gray-500 mt-1">Waiting for session booking</p>
                          )}
                        </td>
                      </tr>
                    ))
                  ) : (
                    <tr>
                      <td colSpan={4} className="py-6 text-center text-gray-500">
                        No referrals yet
                      </td>
                    </tr>
                  )}
                </tbody>
              </table>
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle>How it works</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="space-y-3 text-gray-700">
              <p className="flex items-start gap-2">
                <Gift className="w-5 h-5 text-primary-600 mt-0.5" />
                Share your code with new students during signup (optional).
              </p>
              <p className="flex items-start gap-2">
                <Gift className="w-5 h-5 text-primary-600 mt-0.5" />
                Bonus points are released only after the referred student books a session with a tutor.
              </p>
              <p className="flex items-start gap-2">
                <Gift className="w-5 h-5 text-primary-600 mt-0.5" />
                Referral program is available only for students (not tutors).
              </p>
              <p className="text-sm text-gray-500">
                See full terms in the{' '}
                <Link to="/terms-and-conditions" className="text-primary-600 hover:text-primary-700">
                  Terms & Conditions
                </Link>
                .
              </p>
            </div>
          </CardContent>
        </Card>
      </div>
    </div>
  )
}

export default ReferralProgram
