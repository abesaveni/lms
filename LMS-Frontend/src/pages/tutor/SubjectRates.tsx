import { useState, useEffect } from 'react'
import { Plus, Trash2, Save, DollarSign } from 'lucide-react'
import { Card, CardHeader, CardTitle, CardContent } from '../../components/ui/Card'
import Button from '../../components/ui/Button'
import Input from '../../components/ui/Input'
import { getMySubjectRates, updateSubjectRates, SubjectRate } from '../../services/courseApi'

const SubjectRates = () => {
  const [rates, setRates] = useState<SubjectRate[]>([])
  const [loading, setLoading] = useState(true)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [success, setSuccess] = useState(false)

  useEffect(() => {
    getMySubjectRates()
      .then(res => setRates(res.data))
      .catch(err => setError(err.message || 'Failed to load rates'))
      .finally(() => setLoading(false))
  }, [])

  const addRate = () => {
    setRates(prev => [...prev, { subjectName: '', hourlyRate: 0, trialRate: 0 }])
  }

  const removeRate = (i: number) => {
    setRates(prev => prev.filter((_, idx) => idx !== i))
  }

  const updateRate = (i: number, field: keyof SubjectRate, value: string | number) => {
    setRates(prev => prev.map((r, idx) => idx === i ? { ...r, [field]: value } : r))
  }

  const handleSave = async () => {
    const valid = rates.every(r => r.subjectName.trim() && r.hourlyRate > 0)
    if (!valid) {
      setError('All rates must have a subject name and hourly rate > 0')
      return
    }
    setSaving(true)
    setError(null)
    try {
      await updateSubjectRates(rates)
      setSuccess(true)
      setTimeout(() => setSuccess(false), 3000)
    } catch (err: any) {
      setError(err.message || 'Failed to save rates')
    } finally {
      setSaving(false)
    }
  }

  if (loading) {
    return (
      <div className="flex items-center justify-center h-64">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary-600" />
      </div>
    )
  }

  return (
    <div className="max-w-3xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <div className="mb-8">
        <h1 className="text-3xl font-bold text-gray-900">Subject Rates</h1>
        <p className="text-gray-500 mt-1">Set your hourly rates per subject shown on your tutor profile</p>
      </div>

      {error && (
        <div className="mb-6 p-4 bg-red-50 border border-red-200 rounded-lg text-red-700 text-sm">{error}</div>
      )}
      {success && (
        <div className="mb-6 p-4 bg-green-50 border border-green-200 rounded-lg text-green-700 text-sm">
          Rates saved successfully!
        </div>
      )}

      <Card>
        <CardHeader>
          <div className="flex items-center justify-between">
            <CardTitle className="flex items-center gap-2">
              <DollarSign className="w-5 h-5 text-primary-600" />
              Per-Subject Rates
            </CardTitle>
            <Button variant="outline" size="sm" onClick={addRate} className="flex items-center gap-1.5">
              <Plus className="w-3.5 h-3.5" /> Add Subject
            </Button>
          </div>
        </CardHeader>
        <CardContent>
          {rates.length === 0 ? (
            <div className="text-center py-10 text-gray-400">
              <DollarSign className="w-8 h-8 mx-auto mb-2 opacity-40" />
              <p className="text-sm">No rates set yet. Add your subjects and hourly rates.</p>
            </div>
          ) : (
            <div className="space-y-4">
              <div className="grid grid-cols-12 gap-3 text-xs font-medium text-gray-500 uppercase px-1">
                <span className="col-span-4">Subject</span>
                <span className="col-span-3">Hourly Rate (₹)</span>
                <span className="col-span-3">Trial Rate (₹)</span>
                <span className="col-span-2"></span>
              </div>
              {rates.map((rate, i) => (
                <div key={i} className="grid grid-cols-12 gap-3 items-center">
                  <div className="col-span-4">
                    <Input
                      value={rate.subjectName}
                      onChange={e => updateRate(i, 'subjectName', e.target.value)}
                      placeholder="e.g., Python"
                      className="text-sm"
                    />
                  </div>
                  <div className="col-span-3">
                    <Input
                      type="number" min={0}
                      value={rate.hourlyRate}
                      onChange={e => updateRate(i, 'hourlyRate', parseFloat(e.target.value) || 0)}
                      className="text-sm"
                    />
                  </div>
                  <div className="col-span-3">
                    <Input
                      type="number" min={0}
                      value={rate.trialRate || ''}
                      onChange={e => updateRate(i, 'trialRate', parseFloat(e.target.value) || 0)}
                      placeholder="Optional"
                      className="text-sm"
                    />
                  </div>
                  <div className="col-span-2 flex justify-end">
                    <button
                      onClick={() => removeRate(i)}
                      className="text-red-400 hover:text-red-600 p-1.5 rounded-md hover:bg-red-50"
                    >
                      <Trash2 className="w-4 h-4" />
                    </button>
                  </div>
                </div>
              ))}
            </div>
          )}

          {rates.length > 0 && (
            <div className="mt-6 pt-4 border-t flex justify-end">
              <Button onClick={handleSave} disabled={saving} className="flex items-center gap-2">
                <Save className="w-4 h-4" />
                {saving ? 'Saving...' : 'Save Rates'}
              </Button>
            </div>
          )}
        </CardContent>
      </Card>

      <div className="mt-4 p-4 bg-blue-50 border border-blue-200 rounded-lg text-sm text-blue-700">
        <strong>Tip:</strong> These rates are displayed on your public tutor profile to help students understand your pricing before booking a session.
      </div>
    </div>
  )
}

export default SubjectRates
