import { useEffect, useState } from 'react'
import { DollarSign, TrendingUp, Wallet } from 'lucide-react'
import { Card, CardHeader, CardTitle, CardContent } from '../../components/ui/Card'
import { StatsCard } from '../../components/domain/StatsCard'
import { Badge } from '../../components/ui/Badge'
import { getAdminFinancials, FinancialsResponse } from '../../services/adminApi'

const FinancialDashboard = () => {
  const [financials, setFinancials] = useState<FinancialsResponse | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    const loadFinancials = async () => {
      setIsLoading(true)
      setError(null)
      try {
        const data = await getAdminFinancials({ page: 1, pageSize: 50 })
        setFinancials(data)
      } catch (err: any) {
        setError(err.message || 'Failed to load financials')
      } finally {
        setIsLoading(false)
      }
    }
    loadFinancials()
  }, [])

  return (
    <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <div className="mb-8">
        <h1 className="text-3xl font-bold text-gray-900 mb-2">Financial Dashboard</h1>
        <p className="text-gray-600">Complete financial overview and transparency</p>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-6 gap-6 mb-8">
        <StatsCard
          title="Total Revenue"
          value={`₹${financials?.summary.totalRevenue?.toLocaleString() ?? '0'}`}
          icon={<DollarSign className="w-6 h-6" />}
        />
        <StatsCard
          title="Platform Fees"
          value={`₹${financials?.summary.totalPlatformFees?.toLocaleString() ?? '0'}`}
          icon={<DollarSign className="w-6 h-6" />}
        />
        <StatsCard
          title="Total Bookings"
          value={financials?.summary.totalSessionBookings?.toLocaleString() ?? '0'}
          icon={<TrendingUp className="w-6 h-6" />}
        />
        <StatsCard
          title="Tutor Earnings"
          value={`₹${financials?.summary.totalTutorEarnings?.toLocaleString() ?? '0'}`}
          icon={<Wallet className="w-6 h-6" />}
        />
        <StatsCard
          title="Total Withdrawals"
          value={`₹${financials?.summary.totalWithdrawals?.toLocaleString() ?? '0'}`}
          icon={<Wallet className="w-6 h-6" />}
        />
        <StatsCard
          title="Net Profit"
          value={`₹${financials?.summary.netProfit?.toLocaleString() ?? '0'}`}
          icon={<TrendingUp className="w-6 h-6" />}
        />
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Recent Transactions</CardTitle>
        </CardHeader>
        <CardContent>
          {error && <div className="mb-4 text-sm text-red-600">{error}</div>}
          <div className="overflow-x-auto">
            <table className="w-full">
              <thead>
                <tr className="border-b border-gray-200">
                  <th className="text-left py-3 px-4 text-sm font-semibold text-gray-700">ID</th>
                  <th className="text-left py-3 px-4 text-sm font-semibold text-gray-700">Type</th>
                  <th className="text-left py-3 px-4 text-sm font-semibold text-gray-700">User</th>
                  <th className="text-left py-3 px-4 text-sm font-semibold text-gray-700">Amount</th>
                  <th className="text-left py-3 px-4 text-sm font-semibold text-gray-700">Date</th>
                  <th className="text-left py-3 px-4 text-sm font-semibold text-gray-700">Status</th>
                </tr>
              </thead>
              <tbody>
                {isLoading ? (
                  <tr>
                    <td colSpan={6} className="py-6 text-center text-gray-500">
                      Loading transactions...
                    </td>
                  </tr>
                ) : financials?.transactions?.length ? (
                  financials.transactions.map((txn) => (
                    <tr key={txn.id} className="border-b border-gray-100 hover:bg-gray-50">
                      <td className="py-4 px-4 text-sm font-medium text-gray-900">{txn.id}</td>
                      <td className="py-4 px-4">
                        <Badge variant="info">{txn.type}</Badge>
                      </td>
                      <td className="py-4 px-4 text-sm text-gray-600">{txn.userId}</td>
                      <td className="py-4 px-4">
                        <span className="font-medium text-gray-900">
                          {txn.amount > 0 ? `₹${txn.amount.toLocaleString()}` : '-'}
                        </span>
                      </td>
                      <td className="py-4 px-4 text-sm text-gray-600">
                        {new Date(txn.createdAt).toLocaleDateString()}
                      </td>
                      <td className="py-4 px-4">
                        <Badge variant="success">{txn.status}</Badge>
                      </td>
                    </tr>
                  ))
                ) : (
                  <tr>
                    <td colSpan={6} className="py-6 text-center text-gray-500">
                      No transactions found
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

export default FinancialDashboard
