import { useEffect, useState } from 'react'
import { Search, Filter, Trash2, UserCheck, UserX } from 'lucide-react'
import Button from '../../components/ui/Button'
import Input from '../../components/ui/Input'
import { Card, CardHeader, CardTitle, CardContent } from '../../components/ui/Card'
import { Badge } from '../../components/ui/Badge'
import { Avatar } from '../../components/ui/Avatar'
import {
  getAdminUsers,
  activateAdminUser,
  deactivateAdminUser,
  deleteAdminUser,
  createAdminUser,
  AdminUser,
} from '../../services/adminApi'

const UserManagement = () => {
  const [searchQuery, setSearchQuery] = useState('')
  const [filterRole, setFilterRole] = useState('all')
  const [users, setUsers] = useState<AdminUser[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    const loadUsers = async () => {
      setIsLoading(true)
      setError(null)
      try {
        const response = await getAdminUsers({
          page: 1,
          pageSize: 50,
          role: filterRole === 'all' ? undefined : filterRole,
        })
        setUsers(response.items || [])
      } catch (err: any) {
        setError(err.message || 'Failed to fetch users')
      } finally {
        setIsLoading(false)
      }
    }
    loadUsers()
  }, [filterRole])

  const handleAddUser = async () => {
    const username = prompt('Enter username')
    if (!username) return
    const email = prompt('Enter email')
    if (!email) return
    const role = prompt('Enter role (Admin/Tutor/Student)', 'Student') || 'Student'

    try {
      await createAdminUser({ username, email, role })
      const response = await getAdminUsers({ page: 1, pageSize: 50 })
      setUsers(response.items || [])
    } catch (err: any) {
      setError(err.message || 'Failed to create user')
    }
  }

  const handleToggleStatus = async (user: AdminUser) => {
    try {
      if (user.isActive) {
        await deactivateAdminUser(user.id)
      } else {
        await activateAdminUser(user.id)
      }
      const response = await getAdminUsers({
        page: 1,
        pageSize: 50,
        role: filterRole === 'all' ? undefined : filterRole,
      })
      setUsers(response.items || [])
    } catch (err: any) {
      setError(err.message || 'Failed to update status')
    }
  }

  const handleDeleteUser = async (userId: string) => {
    if (!confirm('Are you sure you want to delete this user?')) return
    try {
      await deleteAdminUser(userId)
      setUsers((prev) => prev.filter((u) => u.id !== userId))
    } catch (err: any) {
      setError(err.message || 'Failed to delete user')
    }
  }

  const filteredUsers = users.filter((user) => {
    const query = searchQuery.toLowerCase()
    return (
      user.username.toLowerCase().includes(query) ||
      user.email.toLowerCase().includes(query)
    )
  })

  return (
    <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <div className="mb-8">
        <div className="flex items-center justify-between">
          <div>
            <h1 className="text-3xl font-bold text-gray-900 mb-2">User Management</h1>
            <p className="text-gray-600">Manage all platform users</p>
          </div>
          <Button onClick={handleAddUser}>
            <UserCheck className="mr-2 w-5 h-5" />
            Add User
          </Button>
        </div>
      </div>

      {/* Filters */}
      <Card className="mb-6">
        <CardContent className="pt-6">
          <div className="flex flex-col md:flex-row gap-4">
            <div className="flex-1 relative">
              <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 w-5 h-5" />
              <Input
                placeholder="Search by name or email..."
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
                className="pl-10"
              />
            </div>
            <div className="flex gap-2">
              <select
                value={filterRole}
                onChange={(e) => setFilterRole(e.target.value)}
                className="px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500"
              >
                <option value="all">All Roles</option>
                <option value="student">Students</option>
                <option value="tutor">Tutors</option>
                <option value="admin">Admins</option>
              </select>
              <Button variant="outline">
                <Filter className="mr-2 w-5 h-5" />
                Filters
              </Button>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Users Table */}
      <Card>
        <CardHeader>
          <CardTitle>Users ({filteredUsers.length})</CardTitle>
        </CardHeader>
        <CardContent>
          {error && (
            <div className="mb-4 text-sm text-red-600">{error}</div>
          )}
          <div className="overflow-x-auto">
            <table className="w-full">
              <thead>
                <tr className="border-b border-gray-200">
                  <th className="text-left py-3 px-4 text-sm font-semibold text-gray-700">User</th>
                  <th className="text-left py-3 px-4 text-sm font-semibold text-gray-700">Role</th>
                  <th className="text-left py-3 px-4 text-sm font-semibold text-gray-700">Verification</th>
                  <th className="text-left py-3 px-4 text-sm font-semibold text-gray-700">Login Status</th>
                  <th className="text-left py-3 px-4 text-sm font-semibold text-gray-700">Joined</th>
                  <th className="text-left py-3 px-4 text-sm font-semibold text-gray-700">Actions</th>
                </tr>
              </thead>
              <tbody>
                {isLoading ? (
                  <tr>
                    <td colSpan={5} className="py-6 text-center text-gray-500">
                      Loading users...
                    </td>
                  </tr>
                ) : filteredUsers.length === 0 ? (
                  <tr>
                    <td colSpan={5} className="py-6 text-center text-gray-500">
                      No users found
                    </td>
                  </tr>
                ) : (
                  filteredUsers.map((user) => (
                    <tr key={user.id} className="border-b border-gray-100 hover:bg-gray-50">
                      <td className="py-4 px-4">
                        <div className="flex items-center gap-3">
                          <Avatar name={user.username} size="sm" />
                          <div>
                            <p className="font-medium text-gray-900">{user.username}</p>
                            <p className="text-sm text-gray-600">{user.email}</p>
                          </div>
                        </div>
                      </td>
                      <td className="py-4 px-4">
                        <Badge variant={user.role === 'Tutor' ? 'info' : 'default'}>
                          {user.role}
                        </Badge>
                      </td>
                      <td className="py-4 px-4">
                        {user.role === 'Tutor' ? (
                          <Badge variant={
                            user.verificationStatus === 'Approved' ? 'success' : 
                            user.verificationStatus === 'Pending' ? 'warning' : 'default'
                          }>
                            {user.verificationStatus || 'Not Started'}
                          </Badge>
                        ) : (
                          <span className="text-gray-400">-</span>
                        )}
                      </td>
                      <td className="py-4 px-4">
                        <Badge variant={user.isActive ? 'success' : 'error'}>
                          {user.isActive ? 'Active' : 'Inactive'}
                        </Badge>
                      </td>
                      <td className="py-4 px-4 text-sm text-gray-600">
                        {new Date(user.createdAt).toLocaleDateString()}
                      </td>
                      <td className="py-4 px-4">
                        <div className="flex items-center gap-2">
                          <button
                            className="p-2 hover:bg-gray-100 rounded-lg transition-colors"
                            onClick={() => handleToggleStatus(user)}
                            title={user.isActive ? 'Deactivate user' : 'Activate user'}
                          >
                            {user.isActive ? (
                              <UserX className="w-4 h-4 text-red-600" />
                            ) : (
                              <UserCheck className="w-4 h-4 text-green-600" />
                            )}
                          </button>
                          <button
                            className="p-2 hover:bg-gray-100 rounded-lg transition-colors"
                            onClick={() => handleDeleteUser(user.id)}
                            title="Delete user"
                          >
                            <Trash2 className="w-4 h-4 text-red-600" />
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
    </div>
  )
}

export default UserManagement
