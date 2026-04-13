import { useState, useEffect } from 'react'
import { Plus, Edit2, Trash2, Check, X, AlertCircle } from 'lucide-react'
import { Card, CardHeader, CardTitle, CardContent } from '../ui/Card'
import Button from '../ui/Button'
import Input from '../ui/Input'
import { getBankAccounts, addBankAccount, updateBankAccount, deleteBankAccount, BankAccount } from '../../services/tutorApi'

const BankDetails = () => {
  const [accounts, setAccounts] = useState<BankAccount[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [isAdding, setIsAdding] = useState(false)
  const [editingId, setEditingId] = useState<string | null>(null)
  const [formData, setFormData] = useState({
    accountHolderName: '',
    accountNumber: '',
    bankName: '',
    ifscCode: '',
    branchName: '',
    accountType: 'Savings',
    isPrimary: false,
  })

  useEffect(() => {
    loadAccounts()
  }, [])

  const loadAccounts = async () => {
    try {
      setIsLoading(true)
      setError(null)
      const data = await getBankAccounts()
      setAccounts(data)
    } catch (err: any) {
      setError(err.message || 'Failed to load bank accounts')
    } finally {
      setIsLoading(false)
    }
  }

  const handleAdd = () => {
    setIsAdding(true)
    setEditingId(null)
    setFormData({
      accountHolderName: '',
      accountNumber: '',
      bankName: '',
      ifscCode: '',
      branchName: '',
      accountType: 'Savings',
      isPrimary: accounts.length === 0, // Auto-set as primary if first account
    })
  }

  const handleEdit = (account: BankAccount) => {
    setEditingId(account.id)
    setIsAdding(false)
    setFormData({
      accountHolderName: account.accountHolderName,
      accountNumber: '', // Don't show actual account number for security
      bankName: account.bankName,
      ifscCode: account.ifscCode,
      branchName: account.branchName || '',
      accountType: account.accountType,
      isPrimary: account.isPrimary,
    })
  }

  const handleCancel = () => {
    setIsAdding(false)
    setEditingId(null)
    setFormData({
      accountHolderName: '',
      accountNumber: '',
      bankName: '',
      ifscCode: '',
      branchName: '',
      accountType: 'Savings',
      isPrimary: false,
    })
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    try {
      setError(null)
      if (editingId) {
        await updateBankAccount(editingId, formData)
      } else {
        await addBankAccount(formData)
      }
      await loadAccounts()
      handleCancel()
    } catch (err: any) {
      setError(err.message || 'Failed to save bank account')
    }
  }

  const handleDelete = async (id: string) => {
    if (!confirm('Are you sure you want to delete this bank account?')) {
      return
    }
    try {
      setError(null)
      await deleteBankAccount(id)
      await loadAccounts()
    } catch (err: any) {
      setError(err.message || 'Failed to delete bank account')
    }
  }

  if (isLoading) {
    return (
      <Card>
        <CardContent className="py-8">
          <div className="text-center text-gray-500">Loading bank accounts...</div>
        </CardContent>
      </Card>
    )
  }

  return (
    <div className="space-y-6">
      {error && (
        <div className="bg-red-50 border border-red-200 rounded-lg p-4 flex items-start gap-3">
          <AlertCircle className="w-5 h-5 text-red-600 mt-0.5" />
          <div className="flex-1">
            <p className="text-sm font-medium text-red-800">Error</p>
            <p className="text-sm text-red-600 mt-1">{error}</p>
          </div>
        </div>
      )}

      <div className="flex items-center justify-between">
        <div>
          <h3 className="text-lg font-semibold text-gray-900">Bank Accounts</h3>
          <p className="text-sm text-gray-600 mt-1">
            Add your bank account details to receive payouts. At least one account is required to request payouts.
          </p>
        </div>
        <Button onClick={handleAdd} disabled={isAdding || editingId !== null}>
          <Plus className="mr-2 w-4 h-4" />
          Add Bank Account
        </Button>
      </div>

      {(isAdding || editingId) && (
        <Card>
          <CardHeader>
            <CardTitle>{editingId ? 'Edit Bank Account' : 'Add Bank Account'}</CardTitle>
          </CardHeader>
          <CardContent>
            <form onSubmit={handleSubmit} className="space-y-4">
              <div className="grid md:grid-cols-2 gap-4">
                <Input
                  label="Account Holder Name"
                  value={formData.accountHolderName}
                  onChange={(e) => setFormData({ ...formData, accountHolderName: e.target.value })}
                  required
                />
                <Input
                  label="Account Number"
                  type="text"
                  value={formData.accountNumber}
                  onChange={(e) => setFormData({ ...formData, accountNumber: e.target.value })}
                  required={!editingId}
                  placeholder={editingId ? 'Leave blank to keep current' : ''}
                />
                <Input
                  label="Bank Name"
                  value={formData.bankName}
                  onChange={(e) => setFormData({ ...formData, bankName: e.target.value })}
                  required
                />
                <Input
                  label="IFSC Code"
                  value={formData.ifscCode}
                  onChange={(e) => setFormData({ ...formData, ifscCode: e.target.value.toUpperCase() })}
                  required
                  maxLength={11}
                />
                <Input
                  label="Branch Name (Optional)"
                  value={formData.branchName}
                  onChange={(e) => setFormData({ ...formData, branchName: e.target.value })}
                  className="md:col-span-2"
                />
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1.5">
                    Account Type
                  </label>
                  <select
                    value={formData.accountType}
                    onChange={(e) => setFormData({ ...formData, accountType: e.target.value })}
                    className="w-full px-4 py-2.5 rounded-lg border border-gray-300 focus:outline-none focus:ring-2 focus:ring-primary-500"
                    required
                  >
                    <option value="Savings">Savings</option>
                    <option value="Current">Current</option>
                  </select>
                </div>
                <div className="flex items-center pt-6">
                  <label className="flex items-center gap-2 cursor-pointer">
                    <input
                      type="checkbox"
                      checked={formData.isPrimary}
                      onChange={(e) => setFormData({ ...formData, isPrimary: e.target.checked })}
                      className="w-4 h-4 text-primary-600 rounded focus:ring-primary-500"
                    />
                    <span className="text-sm text-gray-700">Set as primary account</span>
                  </label>
                </div>
              </div>
              <div className="flex gap-3 pt-4 border-t border-gray-200">
                <Button type="submit">
                  <Check className="mr-2 w-4 h-4" />
                  {editingId ? 'Update' : 'Add'} Account
                </Button>
                <Button type="button" variant="outline" onClick={handleCancel}>
                  <X className="mr-2 w-4 h-4" />
                  Cancel
                </Button>
              </div>
            </form>
          </CardContent>
        </Card>
      )}

      {accounts.length === 0 && !isAdding && (
        <Card>
          <CardContent className="py-12 text-center">
            <AlertCircle className="w-12 h-12 text-gray-400 mx-auto mb-4" />
            <h3 className="text-lg font-medium text-gray-900 mb-2">No Bank Accounts</h3>
            <p className="text-sm text-gray-600 mb-4">
              You need to add at least one bank account to request payouts.
            </p>
            <Button onClick={handleAdd}>
              <Plus className="mr-2 w-4 h-4" />
              Add Your First Bank Account
            </Button>
          </CardContent>
        </Card>
      )}

      {accounts.length > 0 && !isAdding && editingId === null && (
        <div className="space-y-4">
          {accounts.map((account) => (
            <Card key={account.id}>
              <CardContent className="p-6">
                <div className="flex items-start justify-between">
                  <div className="flex-1">
                    <div className="flex items-center gap-3 mb-2">
                      <h4 className="text-lg font-semibold text-gray-900">
                        {account.accountHolderName}
                      </h4>
                      {account.isPrimary && (
                        <span className="px-2 py-1 text-xs font-medium bg-primary-100 text-primary-800 rounded">
                          Primary
                        </span>
                      )}
                      {account.isVerified && (
                        <span className="px-2 py-1 text-xs font-medium bg-green-100 text-green-800 rounded flex items-center gap-1">
                          <Check className="w-3 h-3" />
                          Verified
                        </span>
                      )}
                    </div>
                    <div className="grid md:grid-cols-2 gap-2 text-sm text-gray-600">
                      <div>
                        <span className="font-medium">Account Number:</span> {account.accountNumber}
                      </div>
                      <div>
                        <span className="font-medium">Bank:</span> {account.bankName}
                      </div>
                      <div>
                        <span className="font-medium">IFSC:</span> {account.ifscCode}
                      </div>
                      {account.branchName && (
                        <div>
                          <span className="font-medium">Branch:</span> {account.branchName}
                        </div>
                      )}
                      <div>
                        <span className="font-medium">Type:</span> {account.accountType}
                      </div>
                    </div>
                  </div>
                  <div className="flex gap-2">
                    <Button
                      variant="outline"
                      size="sm"
                      onClick={() => handleEdit(account)}
                    >
                      <Edit2 className="w-4 h-4" />
                    </Button>
                    <Button
                      variant="outline"
                      size="sm"
                      onClick={() => handleDelete(account.id)}
                      className="text-red-600 hover:text-red-700 hover:border-red-300"
                    >
                      <Trash2 className="w-4 h-4" />
                    </Button>
                  </div>
                </div>
              </CardContent>
            </Card>
          ))}
        </div>
      )}
    </div>
  )
}

export default BankDetails
