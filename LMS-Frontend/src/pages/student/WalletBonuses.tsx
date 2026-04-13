import { useEffect, useState } from 'react'
import { Gift, Wallet } from 'lucide-react'
import { Card, CardHeader, CardTitle, CardContent } from '../../components/ui/Card'
import { StatsCard } from '../../components/domain/StatsCard'
import { Badge } from '../../components/ui/Badge'
import { EmptyState } from '../../components/ui/EmptyState'
import { getBonusPointsSummary, BonusPointsSummary } from '../../services/bonusPointsApi'

const WalletBonuses = () => {
  const [bonusSummary, setBonusSummary] = useState<BonusPointsSummary | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    const loadBonuses = async () => {
      setIsLoading(true)
      setError(null)
      try {
        const data = await getBonusPointsSummary()
        setBonusSummary(data)
      } catch (err: any) {
        setError(err.message || 'Failed to load bonuses')
      } finally {
        setIsLoading(false)
      }
    }

    loadBonuses()
  }, [])

  return (
    <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <div className="mb-8">
        <h1 className="text-3xl font-bold text-gray-900 mb-2">Bonus Points</h1>
        <p className="text-gray-600">Track your referral and registration bonus points</p>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-3 gap-6 mb-8">
        <StatsCard
          title="Total Bonus Points"
          value={`${bonusSummary?.totalPoints?.toLocaleString() ?? '0'}`}
          icon={<Wallet className="w-6 h-6" />}
        />
        <StatsCard
          title="Referral Points"
          value={`${bonusSummary?.items?.filter((item) => item.reason === 'Referral').reduce((sum, item) => sum + item.points, 0) ?? 0}`}
          icon={<Gift className="w-6 h-6" />}
        />
        <StatsCard
          title="Registration Points"
          value={`${bonusSummary?.items?.filter((item) => item.reason === 'Registration').reduce((sum, item) => sum + item.points, 0) ?? 0}`}
          icon={<Gift className="w-6 h-6" />}
        />
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Bonus History</CardTitle>
        </CardHeader>
        <CardContent>
          {error && <div className="mb-4 text-sm text-red-600">{error}</div>}
          <div className="overflow-x-auto">
            <table className="w-full">
              <thead>
                <tr className="border-b border-gray-200">
                  <th className="text-left py-3 px-4 text-sm font-semibold text-gray-700">Type</th>
                  <th className="text-left py-3 px-4 text-sm font-semibold text-gray-700">Points</th>
                  <th className="text-left py-3 px-4 text-sm font-semibold text-gray-700">Date</th>
                </tr>
              </thead>
              <tbody>
                {isLoading ? (
                  <tr>
                    <td colSpan={3} className="py-6 text-center text-gray-500">
                      Loading bonuses...
                    </td>
                  </tr>
                ) : bonusSummary?.items?.length ? (
                  bonusSummary.items.map((item) => (
                    <tr key={item.id} className="border-b border-gray-100 hover:bg-gray-50">
                      <td className="py-4 px-4 text-sm font-medium text-gray-900">{item.reason}</td>
                      <td className="py-4 px-4">
                        <Badge variant="success">{item.points} pts</Badge>
                      </td>
                      <td className="py-4 px-4 text-sm text-gray-600">
                        {new Date(item.createdAt).toLocaleDateString()}
                      </td>
                    </tr>
                  ))
                ) : (
                  <tr>
                    <td colSpan={3} className="py-6">
                      <EmptyState
                        icon={<Gift className="w-8 h-8" />}
                        title="No bonus points yet"
                        description="Invite friends or complete sessions to earn points."
                      />
                    </td>
                  </tr>
                )}
              </tbody>
            </table>
          </div>
        </CardContent>
      </Card>
    </div>
  )
}

export default WalletBonuses
