import { useEffect, useState } from 'react'
import { Plus, Edit2, Trash2, X, Search, BookOpen } from 'lucide-react'
import { Card, CardContent } from '../../components/ui/Card'
import Button from '../../components/ui/Button'
import { getAdminSubjects, createSubject, updateSubject, deleteSubject, Subject } from '../../services/adminApi'

const SubjectManagement = () => {
  const [subjects, setSubjects] = useState<Subject[]>([])
  const [loading, setLoading] = useState(true)
  const [searchQuery, setSearchQuery] = useState('')
  const [isModalOpen, setIsModalOpen] = useState(false)
  const [editingSubject, setEditingSubject] = useState<Subject | null>(null)
  
  const [formData, setFormData] = useState({
    name: '',
    description: '',
    isActive: true
  })

  const fetchSubjects = async () => {
    try {
      setLoading(true)
      const data = await getAdminSubjects()
      setSubjects(data)
    } catch (err: any) {
      alert(err.message || 'Failed to fetch subjects')
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    fetchSubjects()
  }, [])

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    try {
      if (editingSubject) {
        await updateSubject(editingSubject.id, formData)
      } else {
        await createSubject(formData)
      }
      setIsModalOpen(false)
      setEditingSubject(null)
      setFormData({ name: '', description: '', isActive: true })
      fetchSubjects()
    } catch (err: any) {
      alert(err.message || 'Action failed')
    }
  }

  const handleDelete = async (id: string) => {
    if (!window.confirm('Are you sure you want to delete this subject?')) return
    try {
      await deleteSubject(id)
      fetchSubjects()
    } catch (err: any) {
      alert(err.message || 'Failed to delete')
    }
  }

  const openEdit = (subject: Subject) => {
    setEditingSubject(subject)
    setFormData({
      name: subject.name,
      description: subject.description || '',
      isActive: subject.isActive
    })
    setIsModalOpen(true)
  }

  const filteredSubjects = subjects.filter(s => 
    s.name.toLowerCase().includes(searchQuery.toLowerCase()) ||
    (s.description?.toLowerCase().includes(searchQuery.toLowerCase()))
  )

  return (
    <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8 space-y-6">
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Subject Management</h1>
          <p className="text-gray-600">Manage academic subjects available for tutors</p>
        </div>
        <Button onClick={() => {
          setEditingSubject(null)
          setFormData({ name: '', description: '', isActive: true })
          setIsModalOpen(true)
        }} className="flex items-center gap-2">
          <Plus className="w-4 h-4" />
          Add New Subject
        </Button>
      </div>

      <Card>
        <CardContent className="p-4">
          <div className="relative mb-6">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-5 h-5 text-gray-400" />
            <input
              type="text"
              placeholder="Search subjects..."
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              className="w-full pl-10 pr-4 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500"
            />
          </div>

          <div className="overflow-x-auto">
            <table className="w-full text-left border-collapse">
              <thead>
                <tr className="border-b text-sm font-semibold text-gray-600">
                  <th className="py-3 px-4">Subject Name</th>
                  <th className="py-3 px-4">Description</th>
                  <th className="py-3 px-4">Status</th>
                  <th className="py-3 px-4 text-right">Actions</th>
                </tr>
              </thead>
              <tbody>
                {loading ? (
                  <tr><td colSpan={4} className="py-8 text-center text-gray-500">Loading...</td></tr>
                ) : filteredSubjects.length === 0 ? (
                  <tr><td colSpan={4} className="py-8 text-center text-gray-500">No subjects found</td></tr>
                ) : (
                  filteredSubjects.map(subject => (
                    <tr key={subject.id} className="border-b hover:bg-gray-50 transition-colors">
                      <td className="py-3 px-4">
                        <div className="flex items-center gap-3">
                          <div className="p-2 bg-primary-50 rounded-lg">
                            <BookOpen className="w-5 h-5 text-primary-600" />
                          </div>
                          <span className="font-medium text-gray-900">{subject.name}</span>
                        </div>
                      </td>
                      <td className="py-3 px-4 text-gray-600 max-w-xs truncate">
                        {subject.description || '—'}
                      </td>
                      <td className="py-3 px-4">
                        <span className={`px-2 py-1 rounded-full text-xs font-medium ${
                          subject.isActive 
                            ? 'bg-green-100 text-green-700' 
                            : 'bg-red-100 text-red-700'
                        }`}>
                          {subject.isActive ? 'Active' : 'Inactive'}
                        </span>
                      </td>
                      <td className="py-3 px-4 text-right">
                        <div className="flex justify-end gap-2">
                          <button 
                            onClick={() => openEdit(subject)}
                            className="p-1.5 text-gray-400 hover:text-primary-600 hover:bg-primary-50 rounded-lg transition-colors"
                          >
                            <Edit2 className="w-4 h-4" />
                          </button>
                          <button 
                            onClick={() => handleDelete(subject.id)}
                            className="p-1.5 text-gray-400 hover:text-red-600 hover:bg-red-50 rounded-lg transition-colors"
                          >
                            <Trash2 className="w-4 h-4" />
                          </button>
                        </div>
                      </td>
                    </tr>
                  ))
                )}
              </tbody>
            </table>
          </div>
        </CardContent>
      </Card>

      {/* Modal */}
      {isModalOpen && (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/50">
          <div className="bg-white rounded-xl shadow-xl w-full max-w-md">
            <div className="p-6 border-b flex items-center justify-between">
              <h2 className="text-xl font-bold">{editingSubject ? 'Edit Subject' : 'Add Subject'}</h2>
              <button onClick={() => setIsModalOpen(false)} className="p-1 hover:bg-gray-100 rounded-lg">
                <X className="w-5 h-5" />
              </button>
            </div>
            <form onSubmit={handleSubmit} className="p-6 space-y-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Subject Name</label>
                <input
                  type="text"
                  required
                  value={formData.name}
                  onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                  className="w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-primary-500 outline-none"
                  placeholder="e.g. Mathematics"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Description</label>
                <textarea
                  value={formData.description}
                  onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                  className="w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-primary-500 outline-none"
                  rows={3}
                  placeholder="Brief description of the subject..."
                />
              </div>
              <div className="flex items-center gap-2">
                <input
                  type="checkbox"
                  id="isActive"
                  checked={formData.isActive}
                  onChange={(e) => setFormData({ ...formData, isActive: e.target.checked })}
                  className="w-4 h-4 text-primary-600 rounded focus:ring-primary-500"
                />
                <label htmlFor="isActive" className="text-sm text-gray-700 font-medium cursor-pointer">
                  Is Active
                </label>
              </div>
              <div className="pt-4 flex gap-3">
                <Button type="button" variant="outline" fullWidth onClick={() => setIsModalOpen(false)}>
                  Cancel
                </Button>
                <Button type="submit" fullWidth>
                  {editingSubject ? 'Save Changes' : 'Create Subject'}
                </Button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  )
}

export default SubjectManagement
