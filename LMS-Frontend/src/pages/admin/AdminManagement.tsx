import { useEffect, useState } from 'react'
import { UserPlus, Trash2, Shield, Mail, Phone } from 'lucide-react'
import { Card, CardHeader, CardTitle, CardContent } from '../../components/ui/Card'
import Button from '../../components/ui/Button'
import Input from '../../components/ui/Input'
import { Badge } from '../../components/ui/Badge'
import { Avatar } from '../../components/ui/Avatar'
import { getAdminUsers, createAdminUser, deleteAdminUser, AdminUser } from '../../services/adminApi'

const AdminManagement = () => {
  const [showAddForm, setShowAddForm] = useState(false)
  const [formData, setFormData] = useState({
    name: '',
    email: '',
    phone: '',
    password: '',
  })
  const [admins, setAdmins] = useState<AdminUser[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    const loadAdmins = async () => {
      setIsLoading(true)
      setError(null)
      try {
        const response = await getAdminUsers({ page: 1, pageSize: 50, role: 'Admin' })
        setAdmins(response.items || [])
      } catch (err: any) {
        setError(err.message || 'Failed to fetch admins')
      } finally {
        setIsLoading(false)
      }
    }
    loadAdmins()
  }, [])

  const handleAddAdmin = async (e: React.FormEvent) => {
    e.preventDefault()
    setError(null)
    try {
      await createAdminUser({
        username: formData.name,
        email: formData.email,
        phoneNumber: formData.phone,
        role: 'Admin',
        password: formData.password,
      })
      const response = await getAdminUsers({ page: 1, pageSize: 50, role: 'Admin' })
      setAdmins(response.items || [])
      setShowAddForm(false)
      setFormData({ name: '', email: '', phone: '', password: '' })
    } catch (err: any) {
      setError(err.message || 'Failed to add admin')
    }
  }

  const handleDeleteAdmin = async (id: string) => {
    if (!confirm('Are you sure you want to remove this admin?')) return
    setError(null)
    try {
      await deleteAdminUser(id)
      setAdmins((prev) => prev.filter((admin) => admin.id !== id))
    } catch (err: any) {
      setError(err.message || 'Failed to remove admin')
    }
  }

  return (
    <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <div className="mb-8 flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold text-gray-900 mb-2">Admin Management</h1>
          <p className="text-gray-600">Manage admin users and permissions</p>
        </div>
        <Button onClick={() => setShowAddForm(!showAddForm)}>
          <UserPlus className="mr-2 w-5 h-5" />
          Add Admin
        </Button>
      </div>

      {error && (
        <div className="mb-6 p-4 bg-red-50 border border-red-200 rounded-lg text-sm text-red-600">
          {error}
        </div>
      )}

      {/* Add Admin Form */}
      {showAddForm && (
        <Card className="mb-6">
          <CardHeader>
            <CardTitle>Add New Admin</CardTitle>
          </CardHeader>
          <CardContent>
            <form onSubmit={handleAddAdmin} className="space-y-4">
              <div className="grid md:grid-cols-2 gap-4">
                <Input
                  label="Full Name"
                  value={formData.name}
                  onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                  placeholder="John Doe"
                  required
                />
                <Input
                  label="Email"
                  type="email"
                  value={formData.email}
                  onChange={(e) => setFormData({ ...formData, email: e.target.value })}
                  placeholder="admin@liveexpert.ai"
                  required
                />
              </div>
              <div className="grid md:grid-cols-2 gap-4">
                <Input
                  label="Phone Number"
                  type="tel"
                  value={formData.phone}
                  onChange={(e) => setFormData({ ...formData, phone: e.target.value })}
                  placeholder="+1 234 567 8900"
                  required
                />
                <Input
                  label="Temporary Password"
                  type="password"
                  value={formData.password}
                  onChange={(e) => setFormData({ ...formData, password: e.target.value })}
                  placeholder="Admin will change on first login"
                  required
                />
              </div>
              <div className="flex gap-3">
                <Button type="submit">Add Admin</Button>
                <Button type="button" variant="outline" onClick={() => setShowAddForm(false)}>
                  Cancel
                </Button>
              </div>
            </form>
          </CardContent>
        </Card>
      )}

      {/* Admins List */}
      <Card>
        <CardHeader>
          <CardTitle>All Admins</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="overflow-x-auto">
            <table className="w-full">
              <thead>
                <tr className="border-b border-gray-200">
                  <th className="text-left py-3 px-4 text-sm font-semibold text-gray-700">Admin</th>
                  <th className="text-left py-3 px-4 text-sm font-semibold text-gray-700">Contact</th>
                  <th className="text-left py-3 px-4 text-sm font-semibold text-gray-700">Role</th>
                  <th className="text-left py-3 px-4 text-sm font-semibold text-gray-700">Added</th>
                  <th className="text-left py-3 px-4 text-sm font-semibold text-gray-700">Actions</th>
                </tr>
              </thead>
              <tbody>
                {isLoading ? (
                  <tr>
                    <td colSpan={5} className="py-6 text-center text-gray-500">
                      Loading admins...
                    </td>
                  </tr>
                ) : admins.length === 0 ? (
                  <tr>
                    <td colSpan={5} className="py-6 text-center text-gray-500">
                      No admins found
                    </td>
                  </tr>
                ) : admins.map((admin) => (
                  <tr key={admin.id} className="border-b border-gray-100 hover:bg-gray-50">
                    <td className="py-4 px-4">
                      <div className="flex items-center gap-3">
                        <Avatar name={admin.username} size="sm" />
                        <div>
                          <p className="font-medium text-gray-900">{admin.username}</p>
                          <p className="text-sm text-gray-500">ID: {admin.id}</p>
                        </div>
                      </div>
                    </td>
                    <td className="py-4 px-4">
                      <div className="space-y-1">
                        <div className="flex items-center gap-2 text-sm text-gray-600">
                          <Mail className="w-4 h-4" />
                          {admin.email}
                        </div>
                        <div className="flex items-center gap-2 text-sm text-gray-600">
                          <Phone className="w-4 h-4" />
                          {admin.phoneNumber || 'N/A'}
                        </div>
                      </div>
                    </td>
                    <td className="py-4 px-4">
                      <Badge variant={admin.role === 'Admin' ? 'info' : 'default'}>
                        <Shield className="w-3 h-3 mr-1" />
                        {admin.role}
                      </Badge>
                    </td>
                    <td className="py-4 px-4 text-sm text-gray-600">
                      <div>
                        <p>{new Date(admin.createdAt).toLocaleDateString()}</p>
                        <p className="text-xs text-gray-500">by System</p>
                      </div>
                    </td>
                    <td className="py-4 px-4">
                      <button
                        onClick={() => handleDeleteAdmin(admin.id)}
                        className="p-2 text-red-600 hover:bg-red-50 rounded-lg transition-colors"
                        title="Remove Admin"
                      >
                        <Trash2 className="w-5 h-5" />
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </CardContent>
      </Card>
    </div>
  )
}

export default AdminManagement
