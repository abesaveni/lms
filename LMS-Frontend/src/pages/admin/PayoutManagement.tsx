import { useState, useEffect } from 'react'
import {
  CheckCircle, XCircle, Building2, Loader2, DollarSign,
  TrendingUp, Users, Clock, CreditCard, RefreshCw
} from 'lucide-react'
import { Card, CardHeader, CardTitle, CardContent } from '../../components/ui/Card'
import Button from '../../components/ui/Button'
import { Badge } from '../../components/ui/Badge'
import { Avatar } from '../../components/ui/Avatar'
import Input from '../../components/ui/Input'
import { Tabs, TabsList, TabsTrigger, TabsContent } from '../../components/ui/Tabs'
import {
  getPendingPayouts,
  getAllPayouts,
  getRevenueSummary,
  approvePayout,
  rejectPayout,
  markPayoutAsPaid,
} from '../../services/adminApi'

type RevenuePeriod = 'today' | 'week' | 'month' | 'year' | 'all'

const PayoutManagement = () => {
  // Revenue summary state
  const [revenuePeriod, setRevenuePeriod] = useState<RevenuePeriod>('month')
  const [revenue, setRevenue] = useState<any>(null)
  const [revenueLoading, setRevenueLoading] = useState(false)

  // Payout requests state
  const [pendingPayouts, setPendingPayouts] = useState<any[]>([])
  const [allPayouts, setAllPayouts] = useState<any[]>([])
  const [statusFilter, setStatusFilter] = useState<string>('')
  const [listLoading, setListLoading] = useState(false)

  // Action state
  const [selectedPayout, setSelectedPayout] = useState<any | null>(null)
  const [action, setAction] = useState<'approve' | 'reject' | 'mark-paid' | null>(null)
  const [notes, setNotes] = useState('')
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    fetchRevenue()
  }, [revenuePeriod])

  useEffect(() => {
    fetchPendingPayouts()
    fetchAllPayouts()
  }, [])

  useEffect(() => {
    fetchAllPayouts()
  }, [statusFilter])

  const fetchRevenue = async () => {
    setRevenueLoading(true)
    try {
      const data = await getRevenueSummary(revenuePeriod)
      setRevenue(data)
    } catch (e: any) {
      console.error('Failed to fetch revenue summary:', e)
    } finally {
      setRevenueLoading(false)
    }
  }

  const fetchPendingPayouts = async () => {
    try {
      const data = await getPendingPayouts()
      setPendingPayouts(data || [])
    } catch (e: any) {
      console.error('Failed to fetch pending payouts:', e)
    }
  }

  const fetchAllPayouts = async () => {
    setListLoading(true)
    try {
      const data = await getAllPayouts(statusFilter || undefined)
      setAllPayouts(data || [])
    } catch (e: any) {
      console.error('Failed to fetch all payouts:', e)
    } finally {
      setListLoading(false)
    }
  }

  const handleApprove = async (payoutId: string) => {
    setIsSubmitting(true)
    setError(null)
    try {
      await approvePayout(payoutId, notes)
      closeModal()
      fetchPendingPayouts()
      fetchAllPayouts()
    } catch (e: any) {
      setError(e.message || 'Failed to approve payout')
    } finally {
      setIsSubmitting(false)
    }
  }

  const handleReject = async (payoutId: string) => {
    if (!notes.trim()) {
      setError('Please provide a rejection reason')
      return
    }
    setIsSubmitting(true)
    setError(null)
    try {
      await rejectPayout(payoutId, notes)
      closeModal()
      fetchPendingPayouts()
      fetchAllPayouts()
    } catch (e: any) {
      setError(e.message || 'Failed to reject payout')
    } finally {
      setIsSubmitting(false)
    }
  }

  const handleMarkPaid = async (payoutId: string) => {
    setIsSubmitting(true)
    setError(null)
    try {
      await markPayoutAsPaid(payoutId, notes || undefined)
      closeModal()
      fetchPendingPayouts()
      fetchAllPayouts()
    } catch (e: any) {
      setError(e.message || 'Failed to mark payout as paid')
    } finally {
      setIsSubmitting(false)
    }
  }

  const closeModal = () => {
    setAction(null)
    setSelectedPayout(null)
    setNotes('')
    setError(null)
  }

  const statusVariant = (status: string) => {
    switch (status) {
      case 'Pending': return 'warning'
      case 'Approved': return 'info'
      case 'Paid': return 'success'
      case 'Rejected': return 'error'
      default: return 'default'
    }
  }

  return (
    <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <div className="mb-8">
        <h1 className="text-3xl font-bold text-gray-900 mb-2">Payout Management</h1>
        <p className="text-gray-600">Platform revenue overview and tutor payout processing</p>
      </div>

      {/* Revenue Summary */}
      <Card className="mb-8">
        <CardHeader>
          <div className="flex items-center justify-between flex-wrap gap-3">
            <CardTitle className="flex items-center gap-2">
              <TrendingUp className="w-5 h-5 text-green-600" />
              Platform Revenue
            </CardTitle>
            <div className="flex items-center gap-2">
              {(['today', 'week', 'month', 'year', 'all'] as RevenuePeriod[]).map((p) => (
                <button
                  key={p}
                  onClick={() => setRevenuePeriod(p)}
                  className={`px-3 py-1 text-sm rounded-full font-medium transition-colors ${
                    revenuePeriod === p
                      ? 'bg-primary-600 text-white'
                      : 'bg-gray-100 text-gray-600 hover:bg-gray-200'
                  }`}
                >
                  {p.charAt(0).toUpperCase() + p.slice(1)}
                </button>
              ))}
              <button onClick={fetchRevenue} className="p-1 text-gray-500 hover:text-gray-700">
                <RefreshCw className={`w-4 h-4 ${revenueLoading ? 'animate-spin' : ''}`} />
              </button>
            </div>
          </div>
        </CardHeader>
        <CardContent>
          {revenueLoading ? (
            <div className="flex items-center justify-center py-8">
              <Loader2 className="w-6 h-6 animate-spin text-primary-600" />
            </div>
          ) : revenue ? (
            <>
              {/* Top stats */}
              <div className="grid grid-cols-2 md:grid-cols-4 gap-4 mb-6">
                <div className="p-4 bg-green-50 rounded-xl">
                  <p className="text-xs text-green-600 font-medium uppercase tracking-wide mb-1">Total Revenue</p>
                  <p className="text-2xl font-bold text-green-700">₹{(revenue.totalPlatformRevenue ?? 0).toLocaleString()}</p>
                </div>
                <div className="p-4 bg-blue-50 rounded-xl">
                  <p className="text-xs text-blue-600 font-medium uppercase tracking-wide mb-1">Session Fees (2%)</p>
                  <p className="text-2xl font-bold text-blue-700">₹{(revenue.breakdown?.sessionCommissions ?? 0).toLocaleString()}</p>
                </div>
                <div className="p-4 bg-purple-50 rounded-xl">
                  <p className="text-xs text-purple-600 font-medium uppercase tracking-wide mb-1">Course Fees (2%)</p>
                  <p className="text-2xl font-bold text-purple-700">₹{(revenue.breakdown?.courseCommissions ?? 0).toLocaleString()}</p>
                </div>
                <div className="p-4 bg-indigo-50 rounded-xl">
                  <p className="text-xs text-indigo-600 font-medium uppercase tracking-wide mb-1">Subscriptions (₹99)</p>
                  <p className="text-2xl font-bold text-indigo-700">₹{(revenue.breakdown?.subscriptions ?? 0).toLocaleString()}</p>
                  <p className="text-xs text-indigo-500 mt-1">{revenue.breakdown?.subscriptionCount ?? 0} students</p>
                </div>
              </div>

              {/* Payout & Net stats */}
              <div className="grid grid-cols-2 md:grid-cols-3 gap-4 mb-6">
                <div className="p-4 bg-orange-50 rounded-xl flex items-center gap-3">
                  <CreditCard className="w-8 h-8 text-orange-500" />
                  <div>
                    <p className="text-xs text-orange-600 font-medium">Total Paid Out</p>
                    <p className="text-xl font-bold text-orange-700">₹{(revenue.payouts?.totalPaidOut ?? 0).toLocaleString()}</p>
                  </div>
                </div>
                <div className="p-4 bg-yellow-50 rounded-xl flex items-center gap-3">
                  <Clock className="w-8 h-8 text-yellow-500" />
                  <div>
                    <p className="text-xs text-yellow-600 font-medium">Pending Requests</p>
                    <p className="text-xl font-bold text-yellow-700">{revenue.payouts?.pendingCount ?? 0}</p>
                  </div>
                </div>
                <div className="p-4 bg-teal-50 rounded-xl flex items-center gap-3">
                  <DollarSign className="w-8 h-8 text-teal-500" />
                  <div>
                    <p className="text-xs text-teal-600 font-medium">Net Retained</p>
                    <p className="text-xl font-bold text-teal-700">₹{(revenue.netRetained ?? 0).toLocaleString()}</p>
                  </div>
                </div>
              </div>

              {/* Top tutors breakdown */}
              {revenue.tutorBreakdown?.length > 0 && (
                <div>
                  <h4 className="text-sm font-semibold text-gray-700 mb-3 flex items-center gap-2">
                    <Users className="w-4 h-4" />
                    Top Tutors by Platform Fees
                  </h4>
                  <div className="overflow-x-auto">
                    <table className="w-full text-sm">
                      <thead>
                        <tr className="border-b border-gray-200">
                          <th className="text-left py-2 px-3 text-gray-500 font-medium">Tutor ID</th>
                          <th className="text-right py-2 px-3 text-gray-500 font-medium">Gross</th>
                          <th className="text-right py-2 px-3 text-gray-500 font-medium">Platform Fee</th>
                          <th className="text-right py-2 px-3 text-gray-500 font-medium">Net to Tutor</th>
                          <th className="text-right py-2 px-3 text-gray-500 font-medium">Sessions</th>
                        </tr>
                      </thead>
                      <tbody>
                        {revenue.tutorBreakdown.map((t: any) => (
                          <tr key={t.tutorId} className="border-b border-gray-100 hover:bg-gray-50">
                            <td className="py-2 px-3 font-mono text-xs text-gray-500">{t.tutorId.slice(0, 8)}…</td>
                            <td className="py-2 px-3 text-right">₹{t.grossEarnings.toLocaleString()}</td>
                            <td className="py-2 px-3 text-right text-green-600 font-medium">₹{t.platformFees.toLocaleString()}</td>
                            <td className="py-2 px-3 text-right">₹{t.netEarnings.toLocaleString()}</td>
                            <td className="py-2 px-3 text-right text-gray-500">{t.count}</td>
                          </tr>
                        ))}
                      </tbody>
                    </table>
                  </div>
                </div>
              )}
            </>
          ) : (
            <p className="text-gray-500 text-center py-6">No revenue data available</p>
          )}
        </CardContent>
      </Card>

      {/* Payout Requests Tabs */}
      <Tabs defaultValue="pending">
        <TabsList className="mb-6">
          <TabsTrigger value="pending">
            Pending ({pendingPayouts.length})
          </TabsTrigger>
          <TabsTrigger value="all">All Payouts</TabsTrigger>
        </TabsList>

        {/* Pending tab */}
        <TabsContent value="pending">
          <div className="space-y-6">
            {pendingPayouts.length === 0 ? (
              <Card>
                <CardContent className="pt-6 text-center py-12">
                  <CheckCircle className="w-12 h-12 mx-auto mb-4 text-green-600" />
                  <p className="text-gray-600 font-medium">No pending payout requests</p>
                  <p className="text-gray-400 text-sm mt-1">All caught up!</p>
                </CardContent>
              </Card>
            ) : (
              pendingPayouts.map((payout) => (
                <PayoutCard
                  key={payout.id}
                  payout={payout}
                  onApprove={() => { setSelectedPayout(payout); setAction('approve') }}
                  onReject={() => { setSelectedPayout(payout); setAction('reject') }}
                  showMarkPaid={false}
                />
              ))
            )}
          </div>
        </TabsContent>

        {/* All payouts tab */}
        <TabsContent value="all">
          <div className="mb-4 flex items-center gap-3 flex-wrap">
            <span className="text-sm font-medium text-gray-700">Filter by status:</span>
            {['', 'Pending', 'Approved', 'Paid', 'Rejected'].map((s) => (
              <button
                key={s}
                onClick={() => setStatusFilter(s)}
                className={`px-3 py-1 text-sm rounded-full font-medium transition-colors ${
                  statusFilter === s
                    ? 'bg-primary-600 text-white'
                    : 'bg-gray-100 text-gray-600 hover:bg-gray-200'
                }`}
              >
                {s || 'All'}
              </button>
            ))}
            <button onClick={fetchAllPayouts} className="p-1 text-gray-500 hover:text-gray-700 ml-auto">
              <RefreshCw className={`w-4 h-4 ${listLoading ? 'animate-spin' : ''}`} />
            </button>
          </div>

          {listLoading ? (
            <div className="flex items-center justify-center py-12">
              <Loader2 className="w-6 h-6 animate-spin text-primary-600" />
            </div>
          ) : allPayouts.length === 0 ? (
            <Card>
              <CardContent className="text-center py-12">
                <p className="text-gray-500">No payout requests found</p>
              </CardContent>
            </Card>
          ) : (
            <div className="space-y-4">
              {allPayouts.map((payout) => (
                <PayoutCard
                  key={payout.id}
                  payout={payout}
                  statusVariant={statusVariant}
                  onApprove={payout.status === 'Pending' ? () => { setSelectedPayout(payout); setAction('approve') } : undefined}
                  onReject={payout.status === 'Pending' ? () => { setSelectedPayout(payout); setAction('reject') } : undefined}
                  onMarkPaid={payout.status === 'Approved' ? () => { setSelectedPayout(payout); setAction('mark-paid') } : undefined}
                  showMarkPaid={payout.status === 'Approved'}
                />
              ))}
            </div>
          )}
        </TabsContent>
      </Tabs>

      {/* Action Modal */}
      {action && selectedPayout && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4">
          <Card className="max-w-md w-full">
            <CardHeader>
              <CardTitle>
                {action === 'approve' && 'Approve Payout'}
                {action === 'reject' && 'Reject Payout'}
                {action === 'mark-paid' && 'Mark as Paid'}
              </CardTitle>
            </CardHeader>
            <CardContent>
              <div className="space-y-4">
                <div className="p-3 bg-gray-50 rounded-lg space-y-1 text-sm">
                  <p><span className="text-gray-500">Tutor:</span> <span className="font-medium">{selectedPayout.tutorName}</span></p>
                  <p><span className="text-gray-500">Email:</span> {selectedPayout.tutorEmail}</p>
                  <p><span className="text-gray-500">Amount:</span> <span className="font-bold text-primary-600">₹{selectedPayout.amount?.toLocaleString()}</span></p>
                  {selectedPayout.bankAccount && (
                    <>
                      <p><span className="text-gray-500">Bank:</span> {selectedPayout.bankAccount.bankName}</p>
                      <p><span className="text-gray-500">Account:</span> <span className="font-mono">{selectedPayout.bankAccount.accountNumber}</span></p>
                      <p><span className="text-gray-500">IFSC:</span> <span className="font-mono">{selectedPayout.bankAccount.ifscCode ?? selectedPayout.bankAccount.ifsCode}</span></p>
                    </>
                  )}
                </div>

                {error && (
                  <div className="p-3 bg-red-50 border border-red-200 rounded text-sm text-red-600">
                    {error}
                  </div>
                )}

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    {action === 'reject' ? 'Rejection Reason *' : action === 'mark-paid' ? 'Transaction Reference' : 'Notes (Optional)'}
                  </label>
                  <Input
                    type="text"
                    value={notes}
                    onChange={(e) => setNotes(e.target.value)}
                    placeholder={
                      action === 'reject'
                        ? 'Enter rejection reason...'
                        : action === 'mark-paid'
                        ? 'Enter UTR / transaction ID...'
                        : 'Optional notes...'
                    }
                  />
                </div>

                <div className="flex gap-3">
                  <Button
                    fullWidth
                    onClick={() => {
                      if (action === 'approve') handleApprove(selectedPayout.id)
                      else if (action === 'reject') handleReject(selectedPayout.id)
                      else if (action === 'mark-paid') handleMarkPaid(selectedPayout.id)
                    }}
                    disabled={isSubmitting}
                  >
                    {isSubmitting ? (
                      <><Loader2 className="mr-2 w-4 h-4 animate-spin" />Processing...</>
                    ) : (
                      <>
                        {action === 'approve' && <><CheckCircle className="mr-2 w-4 h-4" />Approve</>}
                        {action === 'reject' && <><XCircle className="mr-2 w-4 h-4" />Reject</>}
                        {action === 'mark-paid' && <><CheckCircle className="mr-2 w-4 h-4" />Mark as Paid</>}
                      </>
                    )}
                  </Button>
                  <Button fullWidth variant="outline" onClick={closeModal} disabled={isSubmitting}>
                    Cancel
                  </Button>
                </div>
              </div>
            </CardContent>
          </Card>
        </div>
      )}
    </div>
  )
}

// Reusable payout card component
interface PayoutCardProps {
  payout: any
  statusVariant?: (s: string) => string
  onApprove?: () => void
  onReject?: () => void
  onMarkPaid?: () => void
  showMarkPaid?: boolean
}

const PayoutCard = ({ payout, statusVariant, onApprove, onReject, onMarkPaid, showMarkPaid }: PayoutCardProps) => {
  const variantFn = statusVariant ?? (() => 'warning')

  return (
    <Card hover>
      <CardHeader>
        <div className="flex items-start justify-between gap-4">
          <div className="flex items-center gap-4">
            <Avatar name={payout.tutorName} size="lg" />
            <div>
              <h3 className="text-lg font-semibold text-gray-900">{payout.tutorName}</h3>
              <p className="text-gray-500 text-sm">{payout.tutorEmail}</p>
              <p className="text-xs text-gray-400 mt-1">
                Requested: {new Date(payout.requestedAt).toLocaleDateString()}
                {payout.processedAt && ` · Processed: ${new Date(payout.processedAt).toLocaleDateString()}`}
              </p>
            </div>
          </div>
          <div className="text-right shrink-0">
            <p className="text-xl font-bold text-primary-600">₹{payout.amount?.toLocaleString()}</p>
            <Badge variant={variantFn(payout.status) as any}>{payout.status}</Badge>
          </div>
        </div>
      </CardHeader>
      <CardContent>
        <div className="grid md:grid-cols-2 gap-6 mb-4">
          {/* Bank details */}
          <div>
            <h4 className="text-sm font-semibold text-gray-700 mb-2">Bank Account</h4>
            <div className="space-y-1 text-sm text-gray-600">
              <div className="flex items-center gap-2">
                <Building2 className="w-3.5 h-3.5 text-gray-400" />
                {payout.bankAccount?.bankName}
              </div>
              <p>Holder: <span className="font-medium text-gray-800">{payout.bankAccount?.accountHolderName}</span></p>
              <p>Account: <span className="font-mono text-gray-800">{payout.bankAccount?.accountNumber}</span></p>
              <p>IFSC: <span className="font-mono text-gray-800">{payout.bankAccount?.ifscCode ?? payout.bankAccount?.ifsCode}</span></p>
            </div>
          </div>

          {/* Earnings history */}
          {payout.earningsHistory?.length > 0 && (
            <div>
              <h4 className="text-sm font-semibold text-gray-700 mb-2">Recent Earnings</h4>
              <div className="space-y-1">
                {payout.earningsHistory.slice(0, 5).map((e: any, i: number) => (
                  <div key={i} className="flex items-center justify-between text-sm p-1.5 bg-gray-50 rounded">
                    <span className="text-gray-600">₹{e.netAmount?.toLocaleString()}</span>
                    <Badge variant={e.status === 'Available' ? 'success' : e.status === 'Paid' ? 'info' : 'warning'}>
                      {e.status}
                    </Badge>
                  </div>
                ))}
              </div>
            </div>
          )}

          {/* Transaction ref if paid */}
          {payout.transactionReference && (
            <div>
              <h4 className="text-sm font-semibold text-gray-700 mb-1">Transaction Reference</h4>
              <p className="font-mono text-sm text-gray-800">{payout.transactionReference}</p>
            </div>
          )}

          {/* Admin notes */}
          {payout.adminNotes && (
            <div>
              <h4 className="text-sm font-semibold text-gray-700 mb-1">Admin Notes</h4>
              <p className="text-sm text-gray-600">{payout.adminNotes}</p>
            </div>
          )}
        </div>

        {(onApprove || onReject || onMarkPaid) && (
          <div className="flex gap-3 pt-4 border-t border-gray-100">
            {onApprove && (
              <Button onClick={onApprove} className="flex-1">
                <CheckCircle className="mr-2 w-4 h-4" />Approve
              </Button>
            )}
            {onReject && (
              <Button variant="outline" onClick={onReject} className="flex-1">
                <XCircle className="mr-2 w-4 h-4" />Reject
              </Button>
            )}
            {showMarkPaid && onMarkPaid && (
              <Button variant="outline" onClick={onMarkPaid} className="flex-1">
                <CheckCircle className="mr-2 w-4 h-4 text-green-600" />Mark as Paid
              </Button>
            )}
          </div>
        )}
      </CardContent>
    </Card>
  )
}

export default PayoutManagement
