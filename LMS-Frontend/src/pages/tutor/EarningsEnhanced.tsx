import { useState, useEffect } from 'react'
import {
  DollarSign, Clock, Plus, Building2,
  CheckCircle, Loader2, Wallet, ArrowDownCircle, Info
} from 'lucide-react'
import { Card, CardHeader, CardTitle, CardContent } from '../../components/ui/Card'
import Button from '../../components/ui/Button'
import { Badge } from '../../components/ui/Badge'
import { Tabs, TabsList, TabsTrigger, TabsContent } from '../../components/ui/Tabs'
import Input from '../../components/ui/Input'
import {
  getBankAccounts,
  addBankAccount,
  requestPayout,
  getPayoutHistory,
} from '../../services/tutorApi'
import { apiGet } from '../../services/api'

// Wallet summary from new endpoint
interface WalletSummary {
  available: number
  pending: number
  totalPaid: number
  maxPayout: number
  reserve: number
  maxPayoutPercent: number
  nextReleaseAt?: string
  hasBankAccount: boolean
}

interface EarningHistoryItem {
  id: string
  sourceType: string
  sourceId: string
  amount: number
  netAmount: number
  commissionAmount: number
  status: string
  createdAt: string
  availableAt?: string
  paidAt?: string
}

const statusColors: Record<string, string> = {
  Pending:   'bg-yellow-100 text-yellow-700',
  Available: 'bg-green-100 text-green-700',
  Paid:      'bg-blue-100 text-blue-700',
}

const payoutStatusColors: Record<string, 'success' | 'info' | 'error' | 'warning'> = {
  Paid:     'success',
  Approved: 'info',
  Rejected: 'error',
  Pending:  'warning',
}

const TutorEarningsEnhanced = () => {
  const [wallet, setWallet]             = useState<WalletSummary | null>(null)
  const [history, setHistory]           = useState<EarningHistoryItem[]>([])
  const [bankAccounts, setBankAccounts] = useState<any[]>([])
  const [payouts, setPayouts]           = useState<any[]>([])
  const [showAddBank, setShowAddBank]   = useState(false)
  const [showPayout, setShowPayout]     = useState(false)
  const [selectedBank, setSelectedBank] = useState<string>('')
  const [payoutAmount, setPayoutAmount] = useState('')
  const [loading, setLoading]           = useState(true)
  const [submitting, setSubmitting]     = useState(false)
  const [error, setError]               = useState<string | null>(null)
  const [success, setSuccess]           = useState<string | null>(null)

  useEffect(() => { fetchAll() }, [])

  const fetchAll = async () => {
    setLoading(true)
    try {
      const [w, h, b, p] = await Promise.all([
        apiGet<WalletSummary>('/tutor/payouts/wallet'),
        apiGet<{ data: EarningHistoryItem[] }>('/tutor/earnings/history'),
        getBankAccounts(),
        getPayoutHistory(),
      ])
      setWallet(w)
      setHistory(h.data || [])
      setBankAccounts(b || [])
      setPayouts(p || [])
      if (b.length > 0) {
        const primary = b.find((a: any) => a.isPrimary) || b[0]
        setSelectedBank(primary.id)
      }
    } catch (e: any) {
      setError(e.message || 'Failed to load earnings')
    } finally {
      setLoading(false)
    }
  }

  const handleAddBank = async (e: React.FormEvent) => {
    e.preventDefault()
    setSubmitting(true); setError(null)
    const fd = new FormData(e.target as HTMLFormElement)
    try {
      await addBankAccount({
        accountHolderName: fd.get('accountHolderName') as string,
        accountNumber:     fd.get('accountNumber') as string,
        bankName:          fd.get('bankName') as string,
        ifscCode:          fd.get('ifscCode') as string,
        branchName:        fd.get('branchName') as string | undefined,
        accountType:       fd.get('accountType') as string,
        isPrimary:         fd.get('isPrimary') === 'on',
      })
      setShowAddBank(false)
      setSuccess('Bank account added successfully')
      fetchAll()
    } catch (e: any) {
      setError(e.message || 'Failed to add bank account')
    } finally {
      setSubmitting(false)
    }
  }

  const handleRequestPayout = async () => {
    const amount = parseFloat(payoutAmount)
    if (!selectedBank || !amount || amount <= 0) {
      setError('Select a bank account and enter a valid amount')
      return
    }
    if (wallet && amount > wallet.maxPayout) {
      setError(`Maximum payout is ₹${wallet.maxPayout.toLocaleString()} (${wallet.maxPayoutPercent}% of available balance)`)
      return
    }
    setSubmitting(true); setError(null)
    try {
      await requestPayout(selectedBank, amount)
      setShowPayout(false)
      setPayoutAmount('')
      setSuccess('Payout request submitted. Admin will process within 1-2 business days.')
      fetchAll()
    } catch (e: any) {
      setError(e.message || 'Failed to request payout')
    } finally {
      setSubmitting(false)
    }
  }

  if (loading) return (
    <div className="flex items-center justify-center h-64">
      <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary-600" />
    </div>
  )

  return (
    <div className="max-w-6xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <div className="mb-8">
        <h1 className="text-3xl font-bold text-gray-900">Earnings & Payouts</h1>
        <p className="text-gray-500 mt-1">Your earnings after 2% platform fee</p>
      </div>

      {error && (
        <div className="mb-4 p-4 bg-red-50 border border-red-200 rounded-lg text-sm text-red-700">{error}</div>
      )}
      {success && (
        <div className="mb-4 p-4 bg-green-50 border border-green-200 rounded-lg text-sm text-green-700">{success}</div>
      )}

      {/* Wallet cards */}
      {wallet && (
        <>
          <div className="grid grid-cols-2 md:grid-cols-4 gap-4 mb-6">
            <Card>
              <CardContent className="p-4">
                <div className="flex items-center gap-2 mb-1">
                  <Wallet className="w-4 h-4 text-green-500" />
                  <span className="text-xs text-gray-500">Available</span>
                </div>
                <p className="text-2xl font-bold text-gray-900">₹{wallet.available.toLocaleString()}</p>
              </CardContent>
            </Card>
            <Card>
              <CardContent className="p-4">
                <div className="flex items-center gap-2 mb-1">
                  <Clock className="w-4 h-4 text-yellow-500" />
                  <span className="text-xs text-gray-500">Pending (3-day hold)</span>
                </div>
                <p className="text-2xl font-bold text-gray-900">₹{wallet.pending.toLocaleString()}</p>
                {wallet.nextReleaseAt && (
                  <p className="text-xs text-gray-400 mt-1">
                    Next release: {new Date(wallet.nextReleaseAt).toLocaleDateString()}
                  </p>
                )}
              </CardContent>
            </Card>
            <Card>
              <CardContent className="p-4">
                <div className="flex items-center gap-2 mb-1">
                  <ArrowDownCircle className="w-4 h-4 text-blue-500" />
                  <span className="text-xs text-gray-500">Max Payout (90%)</span>
                </div>
                <p className="text-2xl font-bold text-primary-700">₹{wallet.maxPayout.toLocaleString()}</p>
                <p className="text-xs text-gray-400 mt-1">Reserve: ₹{wallet.reserve.toLocaleString()}</p>
              </CardContent>
            </Card>
            <Card>
              <CardContent className="p-4">
                <div className="flex items-center gap-2 mb-1">
                  <CheckCircle className="w-4 h-4 text-purple-500" />
                  <span className="text-xs text-gray-500">Total Paid Out</span>
                </div>
                <p className="text-2xl font-bold text-gray-900">₹{wallet.totalPaid.toLocaleString()}</p>
              </CardContent>
            </Card>
          </div>

          {/* Payout CTA */}
          {wallet.available > 0 && (
            <div className="mb-6 p-5 bg-gradient-to-r from-indigo-600 to-purple-600 rounded-xl text-white flex items-center justify-between">
              <div>
                <p className="text-indigo-200 text-sm mb-1">Ready to withdraw</p>
                <p className="text-3xl font-bold">₹{wallet.maxPayout.toLocaleString()}</p>
                <p className="text-indigo-200 text-xs mt-1">
                  Up to {wallet.maxPayoutPercent}% of ₹{wallet.available.toLocaleString()} available balance.
                  ₹{wallet.reserve.toLocaleString()} reserve stays in wallet.
                </p>
              </div>
              <Button
                variant="secondary"
                onClick={() => { setShowPayout(true); setError(null) }}
                disabled={!wallet.hasBankAccount}
              >
                {wallet.hasBankAccount ? 'Request Payout' : 'Add Bank Account First'}
              </Button>
            </div>
          )}

          {/* 2% fee info */}
          <div className="mb-6 flex items-start gap-2 p-3 bg-blue-50 border border-blue-200 rounded-lg text-sm text-blue-700">
            <Info className="w-4 h-4 mt-0.5 flex-shrink-0" />
            <span>
              A <strong>2% platform fee</strong> is deducted from each session/course payment before crediting to your wallet.
              Earnings are held for <strong>3 days</strong> after the session, then become Available for withdrawal.
            </span>
          </div>
        </>
      )}

      <Tabs defaultValue="history">
        <TabsList>
          <TabsTrigger value="history">Earning History</TabsTrigger>
          <TabsTrigger value="bank-accounts">Bank Accounts</TabsTrigger>
          <TabsTrigger value="payouts">Payout Requests</TabsTrigger>
        </TabsList>

        {/* Earning History */}
        <TabsContent value="history">
          <Card>
            <CardHeader><CardTitle>Earning History</CardTitle></CardHeader>
            <CardContent>
              {history.length === 0 ? (
                <div className="text-center py-10 text-gray-400">
                  <DollarSign className="w-8 h-8 mx-auto mb-2 opacity-40" />
                  <p className="text-sm">No earnings yet. Earnings appear here after a session payment is made.</p>
                </div>
              ) : (
                <div className="overflow-x-auto">
                  <table className="w-full text-sm">
                    <thead>
                      <tr className="border-b text-left text-xs text-gray-500 uppercase">
                        <th className="pb-3 pr-4">Source</th>
                        <th className="pb-3 pr-4">Gross</th>
                        <th className="pb-3 pr-4">2% Fee</th>
                        <th className="pb-3 pr-4">You Get</th>
                        <th className="pb-3 pr-4">Status</th>
                        <th className="pb-3 pr-4">Date</th>
                        <th className="pb-3">Available</th>
                      </tr>
                    </thead>
                    <tbody className="divide-y">
                      {history.map(e => (
                        <tr key={e.id} className="hover:bg-gray-50">
                          <td className="py-3 pr-4">
                            <span className={`text-xs px-2 py-0.5 rounded-full font-medium ${
                              e.sourceType === 'Session' ? 'bg-blue-100 text-blue-700' : 'bg-indigo-100 text-indigo-700'
                            }`}>{e.sourceType}</span>
                          </td>
                          <td className="py-3 pr-4 text-gray-700">₹{e.amount.toLocaleString()}</td>
                          <td className="py-3 pr-4 text-red-600">−₹{e.commissionAmount.toLocaleString()}</td>
                          <td className="py-3 pr-4 font-semibold text-green-700">₹{e.netAmount.toLocaleString()}</td>
                          <td className="py-3 pr-4">
                            <span className={`text-xs px-2 py-0.5 rounded-full font-medium ${statusColors[e.status] || 'bg-gray-100 text-gray-600'}`}>
                              {e.status}
                            </span>
                          </td>
                          <td className="py-3 pr-4 text-gray-500">
                            {new Date(e.createdAt).toLocaleDateString('en-IN', { day: '2-digit', month: 'short' })}
                          </td>
                          <td className="py-3 text-gray-500 text-xs">
                            {e.availableAt
                              ? e.status === 'Pending'
                                ? new Date(e.availableAt).toLocaleDateString('en-IN', { day: '2-digit', month: 'short' })
                                : '—'
                              : '—'
                            }
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              )}
            </CardContent>
          </Card>
        </TabsContent>

        {/* Bank Accounts */}
        <TabsContent value="bank-accounts">
          <Card>
            <CardHeader>
              <div className="flex items-center justify-between">
                <CardTitle>Bank Accounts</CardTitle>
                <Button size="sm" onClick={() => setShowAddBank(true)}>
                  <Plus className="w-3.5 h-3.5 mr-1.5" /> Add Account
                </Button>
              </div>
            </CardHeader>
            <CardContent>
              {showAddBank ? (
                <form onSubmit={handleAddBank} className="space-y-4 max-w-lg">
                  <Input label="Account Holder Name" name="accountHolderName" required />
                  <Input label="Account Number" name="accountNumber" required />
                  <Input label="Bank Name" name="bankName" required />
                  <Input label="IFSC Code" name="ifscCode" required />
                  <Input label="Branch Name (optional)" name="branchName" />
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">Account Type</label>
                    <select name="accountType" className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm" required>
                      <option value="Savings">Savings</option>
                      <option value="Current">Current</option>
                    </select>
                  </div>
                  <label className="flex items-center gap-2 text-sm text-gray-700 cursor-pointer">
                    <input type="checkbox" name="isPrimary" className="rounded" />
                    Set as primary account
                  </label>
                  <div className="flex gap-3">
                    <Button type="submit" disabled={submitting}>
                      {submitting ? <><Loader2 className="w-4 h-4 mr-2 animate-spin" />Adding…</> : 'Add Account'}
                    </Button>
                    <Button type="button" variant="outline" onClick={() => setShowAddBank(false)}>Cancel</Button>
                  </div>
                </form>
              ) : bankAccounts.length === 0 ? (
                <div className="text-center py-12 text-gray-400">
                  <Building2 className="w-10 h-10 mx-auto mb-2 opacity-40" />
                  <p className="text-sm mb-4">No bank accounts added</p>
                  <Button size="sm" variant="outline" onClick={() => setShowAddBank(true)}>Add Bank Account</Button>
                </div>
              ) : (
                <div className="space-y-3">
                  {bankAccounts.map(acc => (
                    <div key={acc.id} className="p-4 border border-gray-200 rounded-lg flex items-center justify-between">
                      <div>
                        <div className="flex items-center gap-2">
                          <span className="font-semibold text-sm text-gray-900">{acc.accountHolderName}</span>
                          {acc.isPrimary && <Badge variant="info">Primary</Badge>}
                        </div>
                        <p className="text-xs text-gray-500 mt-0.5">{acc.bankName} · {acc.accountNumber} · {acc.ifscCode}</p>
                      </div>
                    </div>
                  ))}
                </div>
              )}
            </CardContent>
          </Card>
        </TabsContent>

        {/* Payout History */}
        <TabsContent value="payouts">
          <Card>
            <CardHeader><CardTitle>Payout Requests</CardTitle></CardHeader>
            <CardContent>
              {payouts.length === 0 ? (
                <div className="text-center py-10 text-gray-400 text-sm">No payout requests yet.</div>
              ) : (
                <div className="divide-y">
                  {payouts.map((p: any) => (
                    <div key={p.id} className="py-4 flex items-center justify-between">
                      <div>
                        <p className="font-semibold text-gray-900">₹{p.amount.toLocaleString()}</p>
                        <p className="text-xs text-gray-500 mt-0.5">
                          {p.bankAccount?.bankName} · Requested {new Date(p.requestedAt).toLocaleDateString()}
                        </p>
                        {p.adminNotes && <p className="text-xs text-gray-400 mt-0.5">{p.adminNotes}</p>}
                        {p.transactionReference && (
                          <p className="text-xs text-green-600 mt-0.5">Ref: {p.transactionReference}</p>
                        )}
                      </div>
                      <Badge variant={payoutStatusColors[p.status] || 'warning'}>{p.status}</Badge>
                    </div>
                  ))}
                </div>
              )}
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>

      {/* Payout Request Modal */}
      {showPayout && wallet && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
          <div className="bg-white rounded-2xl shadow-2xl w-full max-w-sm p-6 space-y-4">
            <h3 className="text-lg font-bold text-gray-900">Request Payout</h3>

            <div className="bg-gray-50 rounded-lg p-3 text-sm space-y-1">
              <div className="flex justify-between">
                <span className="text-gray-500">Available balance</span>
                <span className="font-medium">₹{wallet.available.toLocaleString()}</span>
              </div>
              <div className="flex justify-between text-primary-700">
                <span>Max payout ({wallet.maxPayoutPercent}%)</span>
                <span className="font-bold">₹{wallet.maxPayout.toLocaleString()}</span>
              </div>
              <div className="flex justify-between text-gray-400 text-xs">
                <span>Reserve stays in wallet (10%)</span>
                <span>₹{wallet.reserve.toLocaleString()}</span>
              </div>
            </div>

            {error && <p className="text-sm text-red-600 bg-red-50 rounded-lg p-2">{error}</p>}

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Bank Account</label>
              <select
                value={selectedBank}
                onChange={e => setSelectedBank(e.target.value)}
                className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm"
              >
                {bankAccounts.map(acc => (
                  <option key={acc.id} value={acc.id}>
                    {acc.accountHolderName} – {acc.bankName}
                  </option>
                ))}
              </select>
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Amount (max ₹{wallet.maxPayout.toLocaleString()})
              </label>
              <Input
                type="number"
                min={1}
                max={wallet.maxPayout}
                value={payoutAmount}
                onChange={e => setPayoutAmount(e.target.value)}
                placeholder={`Enter amount up to ₹${wallet.maxPayout.toLocaleString()}`}
              />
            </div>

            <div className="flex gap-3 pt-2">
              <Button className="flex-1" onClick={handleRequestPayout} disabled={submitting}>
                {submitting ? <><Loader2 className="w-4 h-4 mr-2 animate-spin" />Submitting…</> : 'Submit Request'}
              </Button>
              <Button variant="outline" className="flex-1" onClick={() => setShowPayout(false)}>Cancel</Button>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}

export default TutorEarningsEnhanced
