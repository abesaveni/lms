import { useState, useEffect } from 'react'
import { CreditCard, TrendingUp, BookOpen, Calendar, AlertCircle } from 'lucide-react'
import { Card, CardHeader, CardTitle, CardContent } from '../../components/ui/Card'
import { getBillingHistory, getBillingSummary, BillingItem, BillingSummary } from '../../services/billingApi'

const statusColors: Record<string, string> = {
  Success: 'text-green-700 bg-green-100',
  Active: 'text-green-700 bg-green-100',
  Pending: 'text-yellow-700 bg-yellow-100',
  Failed: 'text-red-700 bg-red-100',
  Refunded: 'text-orange-700 bg-orange-100',
  Cancelled: 'text-gray-600 bg-gray-100',
}

const BillingHistory = () => {
  const [items, setItems] = useState<BillingItem[]>([])
  const [summary, setSummary] = useState<BillingSummary | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [page, setPage] = useState(1)
  const [total, setTotal] = useState(0)
  const pageSize = 20

  useEffect(() => {
    fetchData()
  }, [page])

  const fetchData = async () => {
    setLoading(true)
    try {
      const [histRes, summRes] = await Promise.all([
        getBillingHistory({ page, pageSize }),
        summary ? Promise.resolve(summary) : getBillingSummary(),
      ])
      setItems(histRes.data)
      setTotal(histRes.total)
      if (!summary) setSummary(summRes as BillingSummary)
    } catch (err: any) {
      setError(err.message || 'Failed to load billing data')
    } finally {
      setLoading(false)
    }
  }

  const totalPages = Math.ceil(total / pageSize)

  return (
    <div className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <div className="mb-8">
        <h1 className="text-3xl font-bold text-gray-900">Billing History</h1>
        <p className="text-gray-500 mt-1">Your payment history and spending overview</p>
      </div>

      {error && (
        <div className="mb-6 p-4 bg-red-50 border border-red-200 rounded-lg text-red-700 text-sm flex items-center gap-2">
          <AlertCircle className="w-4 h-4 flex-shrink-0" />
          {error}
        </div>
      )}

      {/* Summary cards */}
      {summary && (
        <div className="grid grid-cols-2 md:grid-cols-4 gap-4 mb-8">
          <Card>
            <CardContent className="p-4">
              <div className="flex items-center gap-2 mb-1">
                <TrendingUp className="w-4 h-4 text-primary-500" />
                <span className="text-xs text-gray-500">Total Spent</span>
              </div>
              <p className="text-xl font-bold text-gray-900">₹{summary.totalSpent.toLocaleString()}</p>
            </CardContent>
          </Card>
          <Card>
            <CardContent className="p-4">
              <div className="flex items-center gap-2 mb-1">
                <Calendar className="w-4 h-4 text-blue-500" />
                <span className="text-xs text-gray-500">Sessions</span>
              </div>
              <p className="text-xl font-bold text-gray-900">₹{summary.sessionPayments.toLocaleString()}</p>
            </CardContent>
          </Card>
          <Card>
            <CardContent className="p-4">
              <div className="flex items-center gap-2 mb-1">
                <BookOpen className="w-4 h-4 text-indigo-500" />
                <span className="text-xs text-gray-500">Courses</span>
              </div>
              <p className="text-xl font-bold text-gray-900">₹{summary.coursePayments.toLocaleString()}</p>
            </CardContent>
          </Card>
          <Card>
            <CardContent className="p-4">
              <div className="flex items-center gap-2 mb-1">
                <CreditCard className="w-4 h-4 text-green-500" />
                <span className="text-xs text-gray-500">Active Courses</span>
              </div>
              <p className="text-xl font-bold text-gray-900">{summary.activeEnrollments}</p>
            </CardContent>
          </Card>
        </div>
      )}

      {/* Transaction list */}
      <Card>
        <CardHeader>
          <CardTitle>Transactions</CardTitle>
        </CardHeader>
        <CardContent>
          {loading ? (
            <div className="flex items-center justify-center py-10">
              <div className="animate-spin rounded-full h-7 w-7 border-b-2 border-primary-600" />
            </div>
          ) : items.length === 0 ? (
            <div className="text-center py-10 text-gray-400">
              <CreditCard className="w-8 h-8 mx-auto mb-2 opacity-40" />
              <p className="text-sm">No transactions yet</p>
            </div>
          ) : (
            <div className="divide-y">
              {items.map(item => (
                <div key={item.id} className="py-4 flex items-start gap-4">
                  <div className={`w-9 h-9 rounded-lg flex items-center justify-center flex-shrink-0 ${
                    item.type === 'Course' ? 'bg-indigo-100' : 'bg-blue-100'
                  }`}>
                    {item.type === 'Course'
                      ? <BookOpen className="w-4 h-4 text-indigo-600" />
                      : <Calendar className="w-4 h-4 text-blue-600" />
                    }
                  </div>

                  <div className="flex-1 min-w-0">
                    <div className="flex items-start justify-between gap-3">
                      <div>
                        <p className="font-medium text-gray-900 text-sm">{item.title}</p>
                        <p className="text-xs text-gray-500 mt-0.5">
                          {item.type} • {item.paymentMethod}
                          {item.gatewayPaymentId && (
                            <span className="text-gray-400"> • {item.gatewayPaymentId.slice(-8)}</span>
                          )}
                        </p>
                        {item.extra?.sessionsPurchased && (
                          <p className="text-xs text-gray-400 mt-0.5">
                            {item.extra.sessionsPurchased} sessions purchased
                          </p>
                        )}
                      </div>
                      <div className="text-right flex-shrink-0">
                        <p className="font-bold text-gray-900">₹{item.amount.toLocaleString()}</p>
                        {item.platformFee > 0 && (
                          <p className="text-xs text-gray-400">Fee: ₹{item.platformFee.toLocaleString()}</p>
                        )}
                      </div>
                    </div>
                    <div className="flex items-center gap-2 mt-1.5">
                      <span className={`text-xs px-2 py-0.5 rounded-full font-medium ${statusColors[item.status] || 'text-gray-600 bg-gray-100'}`}>
                        {item.status}
                      </span>
                      <span className="text-xs text-gray-400">
                        {new Date(item.date).toLocaleDateString('en-IN', {
                          day: '2-digit', month: 'short', year: 'numeric'
                        })}
                      </span>
                    </div>
                  </div>
                </div>
              ))}
            </div>
          )}

          {totalPages > 1 && (
            <div className="flex items-center justify-center gap-2 mt-6 pt-4 border-t">
              <button
                disabled={page <= 1}
                onClick={() => setPage(p => p - 1)}
                className="px-3 py-1.5 text-sm border border-gray-300 rounded-lg hover:bg-gray-50 disabled:opacity-40 disabled:cursor-not-allowed"
              >
                Previous
              </button>
              <span className="text-sm text-gray-600">Page {page} of {totalPages}</span>
              <button
                disabled={page >= totalPages}
                onClick={() => setPage(p => p + 1)}
                className="px-3 py-1.5 text-sm border border-gray-300 rounded-lg hover:bg-gray-50 disabled:opacity-40 disabled:cursor-not-allowed"
              >
                Next
              </button>
            </div>
          )}
        </CardContent>
      </Card>
    </div>
  )
}

export default BillingHistory
