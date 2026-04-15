import { useState, useEffect } from 'react'
import { Plus, Tag, CheckCircle2, XCircle, Loader2, Percent, DollarSign } from 'lucide-react'
import { motion, AnimatePresence } from 'framer-motion'
import { CouponDto, CreateCouponDto, createCoupon, toggleCoupon } from '../../services/couponsApi'
import { apiGet } from '../../services/api'
import Button from '../../components/ui/Button'
import Input from '../../components/ui/Input'
import { Badge } from '../../components/ui/Badge'

// Extend CouponDto for admin listing
interface AdminCouponDto extends CouponDto {}

const CouponManagement = () => {
  const [coupons, setCoupons] = useState<AdminCouponDto[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [showForm, setShowForm] = useState(false)
  const [isCreating, setIsCreating] = useState(false)
  const [createError, setCreateError] = useState<string | null>(null)
  const [createSuccess, setCreateSuccess] = useState<string | null>(null)

  const [form, setForm] = useState<CreateCouponDto>({
    code: '',
    description: '',
    discountType: 0,        // 0 = Percentage, 1 = Flat
    discountValue: 10,
    maxDiscountAmount: undefined,
    minOrderAmount: undefined,
    maxUses: undefined,
    expiresAt: '',
    tutorId: undefined,
  })

  const loadCoupons = async () => {
    setIsLoading(true)
    setError(null)
    try {
      const res = await apiGet<any>('/coupons')
      if (res.success && Array.isArray(res.data)) {
        setCoupons(res.data)
      } else {
        // Backend may not have a list endpoint yet — show empty gracefully
        setCoupons([])
      }
    } catch {
      setCoupons([])
    } finally {
      setIsLoading(false)
    }
  }

  useEffect(() => { loadCoupons() }, [])

  const setField = (k: keyof CreateCouponDto, v: any) => setForm(f => ({ ...f, [k]: v }))

  const handleCreate = async (e: React.FormEvent) => {
    e.preventDefault()
    setIsCreating(true)
    setCreateError(null)
    setCreateSuccess(null)
    try {
      const payload: CreateCouponDto = {
        ...form,
        code: form.code.trim().toUpperCase(),
        maxDiscountAmount: form.maxDiscountAmount || undefined,
        minOrderAmount: form.minOrderAmount || undefined,
        maxUses: form.maxUses || undefined,
        expiresAt: form.expiresAt || undefined,
      }
      const created = await createCoupon(payload)
      setCoupons(prev => [created, ...prev])
      setCreateSuccess(`Coupon "${created.code}" created successfully!`)
      setShowForm(false)
      setForm({ code: '', description: '', discountType: 0, discountValue: 10, expiresAt: '' })
    } catch (err: any) {
      setCreateError(err.message || 'Failed to create coupon')
    } finally {
      setIsCreating(false)
    }
  }

  const handleToggle = async (coupon: AdminCouponDto) => {
    try {
      await toggleCoupon(coupon.id, !coupon.isActive)
      setCoupons(prev => prev.map(c => c.id === coupon.id ? { ...c, isActive: !c.isActive } : c))
    } catch (err: any) {
      alert(err.message || 'Failed to update coupon')
    }
  }

  return (
    <div className="max-w-5xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      {/* Header */}
      <div className="flex items-center justify-between mb-8">
        <div>
          <h1 className="text-2xl font-black text-gray-900 flex items-center gap-2">
            <Tag className="w-6 h-6 text-indigo-500" />
            Coupon Management
          </h1>
          <p className="text-sm text-gray-500 mt-1">Create and manage discount codes for students</p>
        </div>
        <Button onClick={() => setShowForm(s => !s)} className="gap-2">
          <Plus className="w-4 h-4" />
          {showForm ? 'Cancel' : 'New Coupon'}
        </Button>
      </div>

      {/* Success / Error banners */}
      {createSuccess && (
        <div className="mb-4 p-3 bg-green-50 border border-green-200 rounded-xl text-green-700 text-sm font-semibold flex items-center gap-2">
          <CheckCircle2 className="w-4 h-4" />{createSuccess}
        </div>
      )}

      {/* Create Form */}
      <AnimatePresence>
        {showForm && (
          <motion.div
            initial={{ opacity: 0, height: 0 }}
            animate={{ opacity: 1, height: 'auto' }}
            exit={{ opacity: 0, height: 0 }}
            className="overflow-hidden mb-6"
          >
            <div className="bg-white rounded-2xl border border-gray-100 shadow-sm p-6">
              <h2 className="text-sm font-bold text-gray-800 mb-4 uppercase tracking-wider">Create New Coupon</h2>
              {createError && <p className="text-sm text-red-500 mb-3">{createError}</p>}
              <form onSubmit={handleCreate} className="space-y-4">
                <div className="grid sm:grid-cols-2 gap-4">
                  <div>
                    <label className="block text-xs font-semibold text-gray-600 mb-1">Code *</label>
                    <input type="text" value={form.code} onChange={(e) => setField('code', e.target.value.toUpperCase())}
                      placeholder="e.g. WELCOME20" required maxLength={50}
                      className="w-full px-3 py-2.5 rounded-lg border border-gray-300 focus:outline-none focus:ring-2 focus:ring-indigo-400 text-sm font-mono uppercase"
                    />
                  </div>
                  <div>
                    <label className="block text-xs font-semibold text-gray-600 mb-1">Description</label>
                    <input type="text" value={form.description || ''}
                      onChange={(e) => setField('description', e.target.value)}
                      placeholder="e.g. New user welcome offer"
                      className="w-full px-3 py-2.5 rounded-lg border border-gray-300 focus:outline-none focus:ring-2 focus:ring-indigo-400 text-sm"
                    />
                  </div>
                </div>

                <div className="grid sm:grid-cols-3 gap-4">
                  <div>
                    <label className="block text-xs font-semibold text-gray-600 mb-1">Discount Type *</label>
                    <select value={form.discountType} onChange={(e) => setField('discountType', Number(e.target.value))}
                      className="w-full px-3 py-2.5 rounded-lg border border-gray-300 focus:outline-none focus:ring-2 focus:ring-indigo-400 text-sm">
                      <option value={0}>Percentage (%)</option>
                      <option value={1}>Flat Amount (₹)</option>
                    </select>
                  </div>
                  <div>
                    <label className="block text-xs font-semibold text-gray-600 mb-1">
                      {form.discountType === 0 ? 'Discount %' : 'Flat Amount (₹)'} *
                    </label>
                    <input type="number" min="0" value={form.discountValue}
                      onChange={(e) => setField('discountValue', parseFloat(e.target.value) || 0)}
                      required
                      className="w-full px-3 py-2.5 rounded-lg border border-gray-300 focus:outline-none focus:ring-2 focus:ring-indigo-400 text-sm"
                    />
                  </div>
                  {form.discountType === 0 && (
                    <div>
                      <label className="block text-xs font-semibold text-gray-600 mb-1">Max Discount Cap (₹)</label>
                      <input type="number" min="0" value={form.maxDiscountAmount || ''}
                        onChange={(e) => setField('maxDiscountAmount', parseFloat(e.target.value) || undefined)}
                        placeholder="Optional"
                        className="w-full px-3 py-2.5 rounded-lg border border-gray-300 focus:outline-none focus:ring-2 focus:ring-indigo-400 text-sm"
                      />
                    </div>
                  )}
                </div>

                <div className="grid sm:grid-cols-3 gap-4">
                  <div>
                    <label className="block text-xs font-semibold text-gray-600 mb-1">Min Order Amount (₹)</label>
                    <input type="number" min="0" value={form.minOrderAmount || ''}
                      onChange={(e) => setField('minOrderAmount', parseFloat(e.target.value) || undefined)}
                      placeholder="Optional"
                      className="w-full px-3 py-2.5 rounded-lg border border-gray-300 focus:outline-none focus:ring-2 focus:ring-indigo-400 text-sm"
                    />
                  </div>
                  <div>
                    <label className="block text-xs font-semibold text-gray-600 mb-1">Max Total Uses</label>
                    <input type="number" min="1" value={form.maxUses || ''}
                      onChange={(e) => setField('maxUses', parseInt(e.target.value) || undefined)}
                      placeholder="Unlimited"
                      className="w-full px-3 py-2.5 rounded-lg border border-gray-300 focus:outline-none focus:ring-2 focus:ring-indigo-400 text-sm"
                    />
                  </div>
                  <div>
                    <label className="block text-xs font-semibold text-gray-600 mb-1">Expires At</label>
                    <input type="datetime-local" value={form.expiresAt || ''}
                      onChange={(e) => setField('expiresAt', e.target.value)}
                      min={new Date().toISOString().slice(0, 16)}
                      className="w-full px-3 py-2.5 rounded-lg border border-gray-300 focus:outline-none focus:ring-2 focus:ring-indigo-400 text-sm"
                    />
                  </div>
                </div>

                <div className="flex gap-3 pt-2">
                  <Button type="submit" isLoading={isCreating}>Create Coupon</Button>
                  <Button type="button" variant="outline" onClick={() => setShowForm(false)}>Cancel</Button>
                </div>
              </form>
            </div>
          </motion.div>
        )}
      </AnimatePresence>

      {/* Coupons List */}
      {isLoading ? (
        <div className="flex items-center justify-center py-20">
          <Loader2 className="w-8 h-8 animate-spin text-indigo-500" />
        </div>
      ) : error ? (
        <div className="text-center py-12 text-red-500 text-sm">{error}</div>
      ) : coupons.length === 0 ? (
        <div className="text-center py-20 bg-white rounded-2xl border border-dashed border-gray-200">
          <Tag className="w-10 h-10 text-gray-200 mx-auto mb-3" />
          <p className="text-sm font-bold text-gray-700">No coupons yet</p>
          <p className="text-xs text-gray-400 mt-1">Create your first coupon to start offering discounts</p>
          <Button className="mt-4" size="sm" onClick={() => setShowForm(true)}>Create Coupon</Button>
        </div>
      ) : (
        <div className="bg-white rounded-2xl border border-gray-100 shadow-sm overflow-hidden">
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead className="bg-gray-50 border-b border-gray-100">
                <tr>
                  {['Code', 'Type', 'Value', 'Used', 'Expires', 'Status', 'Actions'].map(h => (
                    <th key={h} className="text-left text-xs font-bold text-gray-500 uppercase tracking-wider px-4 py-3">{h}</th>
                  ))}
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-50">
                {coupons.map(coupon => (
                  <tr key={coupon.id} className="hover:bg-gray-50/50 transition-colors">
                    <td className="px-4 py-3">
                      <span className="font-mono font-bold text-gray-900 bg-gray-100 px-2 py-0.5 rounded">{coupon.code}</span>
                      {coupon.description && <p className="text-xs text-gray-400 mt-0.5">{coupon.description}</p>}
                    </td>
                    <td className="px-4 py-3">
                      {coupon.discountType === 'Percentage'
                        ? <span className="flex items-center gap-1 text-indigo-600 font-semibold"><Percent className="w-3.5 h-3.5" />Percentage</span>
                        : <span className="flex items-center gap-1 text-green-600 font-semibold"><DollarSign className="w-3.5 h-3.5" />Flat</span>}
                    </td>
                    <td className="px-4 py-3 font-bold text-gray-900">
                      {coupon.discountType === 'Percentage' ? `${coupon.discountValue}%` : `₹${coupon.discountValue}`}
                      {coupon.maxDiscountAmount && <span className="text-xs text-gray-400 ml-1">(max ₹{coupon.maxDiscountAmount})</span>}
                    </td>
                    <td className="px-4 py-3 text-gray-600">
                      {coupon.usedCount}{coupon.maxUses ? `/${coupon.maxUses}` : ''}
                    </td>
                    <td className="px-4 py-3 text-gray-500 text-xs">
                      {coupon.expiresAt ? new Date(coupon.expiresAt).toLocaleDateString() : '—'}
                    </td>
                    <td className="px-4 py-3">
                      <Badge variant={coupon.isActive ? 'success' : 'default'}
                        className={coupon.isActive ? 'bg-green-50 text-green-700 border-green-100' : 'bg-gray-100 text-gray-500'}>
                        {coupon.isActive ? 'Active' : 'Inactive'}
                      </Badge>
                    </td>
                    <td className="px-4 py-3">
                      <button
                        onClick={() => handleToggle(coupon)}
                        className={`flex items-center gap-1 text-xs font-semibold px-2.5 py-1.5 rounded-lg border transition-all ${
                          coupon.isActive
                            ? 'text-red-600 border-red-100 bg-red-50 hover:bg-red-100'
                            : 'text-green-600 border-green-100 bg-green-50 hover:bg-green-100'
                        }`}
                      >
                        {coupon.isActive
                          ? <><XCircle className="w-3.5 h-3.5" />Deactivate</>
                          : <><CheckCircle2 className="w-3.5 h-3.5" />Activate</>}
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}
    </div>
  )
}

export default CouponManagement
