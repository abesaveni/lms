import { useState, useEffect } from 'react'
import { Shield, Cookie, Calendar, User, Search } from 'lucide-react'
import { Card, CardHeader, CardTitle, CardContent } from '../../components/ui/Card'
import Button from '../../components/ui/Button'
import Input from '../../components/ui/Input'
import { Badge } from '../../components/ui/Badge'
import { Tabs, TabsList, TabsTrigger, TabsContent } from '../../components/ui/Tabs'
import { apiGet } from '../../services/api'

interface CookieConsentAdmin {
  id: string
  userId?: string
  userEmail?: string
  necessary: boolean
  functional: boolean
  analytics: boolean
  marketing: boolean
  ipAddress?: string
  userAgent?: string
  consentGivenAt: string
  consentUpdatedAt?: string
}

interface UserConsentAdmin {
  id: string
  userId: string
  userEmail: string
  consentType: 'GoogleLogin' | 'GoogleCalendar'
  consentTypeName: string
  granted: boolean
  grantedAt?: string
  revokedAt?: string
  ipAddress?: string
  userAgent?: string
}

interface PaginatedResult<T> {
  items: T[]
  totalCount: number
  page: number
  pageSize: number
  totalPages: number
}

const ConsentManagement = () => {
  const [cookieConsents, setCookieConsents] = useState<CookieConsentAdmin[]>([])
  const [userConsents, setUserConsents] = useState<UserConsentAdmin[]>([])
  const [cookiePage, setCookiePage] = useState(1)
  const [userPage, setUserPage] = useState(1)
  const [cookieTotalPages, setCookieTotalPages] = useState(1)
  const [userTotalPages, setUserTotalPages] = useState(1)
  const [searchEmail, setSearchEmail] = useState('')
  useEffect(() => {
    loadCookieConsents()
    loadUserConsents()
  }, [cookiePage, userPage])

  const loadCookieConsents = async () => {
    try {
      const response = await apiGet<{
        success: boolean
        data: PaginatedResult<CookieConsentAdmin>
      }>(`/admin/consents/cookies?page=${cookiePage}&pageSize=20${searchEmail ? `&userId=${searchEmail}` : ''}`)
      
      if (response.success && response.data) {
        setCookieConsents(response.data.items)
        setCookieTotalPages(response.data.totalPages)
      }
    } catch (error) {
      console.error('Failed to load cookie consents:', error)
    }
  }

  const loadUserConsents = async () => {
    try {
      const response = await apiGet<{
        success: boolean
        data: PaginatedResult<UserConsentAdmin>
      }>(`/admin/consents/user?page=${userPage}&pageSize=20${searchEmail ? `&userId=${searchEmail}` : ''}`)
      
      if (response.success && response.data) {
        setUserConsents(response.data.items)
        setUserTotalPages(response.data.totalPages)
      }
    } catch (error) {
      console.error('Failed to load user consents:', error)
    }
  }

  const handleSearch = () => {
    setCookiePage(1)
    setUserPage(1)
    loadCookieConsents()
    loadUserConsents()
  }

  return (
    <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <div className="mb-8">
        <h1 className="text-3xl font-bold text-gray-900 mb-2">Consent Management</h1>
        <p className="text-gray-600">View and audit all user consents</p>
      </div>

      {/* Search */}
      <Card className="mb-6">
        <CardContent className="pt-6">
          <div className="flex gap-3">
            <div className="flex-1">
              <Input
                placeholder="Search by user email..."
                value={searchEmail}
                onChange={(e) => setSearchEmail(e.target.value)}
                onKeyPress={(e) => e.key === 'Enter' && handleSearch()}
              />
            </div>
            <Button onClick={handleSearch}>
              <Search className="w-4 h-4 mr-2" />
              Search
            </Button>
          </div>
        </CardContent>
      </Card>

      <Tabs defaultValue="cookies">
        <TabsList>
          <TabsTrigger value="cookies">
            <Cookie className="w-4 h-4 mr-2" />
            Cookie Consents
          </TabsTrigger>
          <TabsTrigger value="user">
            <Shield className="w-4 h-4 mr-2" />
            Google Consents
          </TabsTrigger>
        </TabsList>

        <TabsContent value="cookies">
          <Card>
            <CardHeader>
              <CardTitle>Cookie Consents ({cookieConsents.length})</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="overflow-x-auto">
                <table className="w-full">
                  <thead>
                    <tr className="border-b border-gray-200">
                      <th className="text-left py-3 px-4 text-sm font-semibold text-gray-700">User</th>
                      <th className="text-left py-3 px-4 text-sm font-semibold text-gray-700">Necessary</th>
                      <th className="text-left py-3 px-4 text-sm font-semibold text-gray-700">Functional</th>
                      <th className="text-left py-3 px-4 text-sm font-semibold text-gray-700">Analytics</th>
                      <th className="text-left py-3 px-4 text-sm font-semibold text-gray-700">Marketing</th>
                      <th className="text-left py-3 px-4 text-sm font-semibold text-gray-700">Given At</th>
                      <th className="text-left py-3 px-4 text-sm font-semibold text-gray-700">IP Address</th>
                    </tr>
                  </thead>
                  <tbody>
                    {cookieConsents.length === 0 ? (
                      <tr>
                        <td colSpan={7} className="text-center py-12 text-gray-500">
                          No cookie consents found
                        </td>
                      </tr>
                    ) : (
                      cookieConsents.map((consent) => (
                        <tr key={consent.id} className="border-b border-gray-100 hover:bg-gray-50">
                          <td className="py-4 px-4">
                            <div>
                              <p className="text-sm font-medium text-gray-900">
                                {consent.userEmail || 'Anonymous'}
                              </p>
                              {consent.userId && (
                                <p className="text-xs text-gray-500">{consent.userId.substring(0, 8)}...</p>
                              )}
                            </div>
                          </td>
                          <td className="py-4 px-4">
                            <Badge variant={consent.necessary ? 'success' : 'error'}>
                              {consent.necessary ? 'Yes' : 'No'}
                            </Badge>
                          </td>
                          <td className="py-4 px-4">
                            <Badge variant={consent.functional ? 'success' : 'error'}>
                              {consent.functional ? 'Yes' : 'No'}
                            </Badge>
                          </td>
                          <td className="py-4 px-4">
                            <Badge variant={consent.analytics ? 'success' : 'error'}>
                              {consent.analytics ? 'Yes' : 'No'}
                            </Badge>
                          </td>
                          <td className="py-4 px-4">
                            <Badge variant={consent.marketing ? 'success' : 'error'}>
                              {consent.marketing ? 'Yes' : 'No'}
                            </Badge>
                          </td>
                          <td className="py-4 px-4 text-sm text-gray-600">
                            {new Date(consent.consentGivenAt).toLocaleString()}
                          </td>
                          <td className="py-4 px-4 text-sm text-gray-600 font-mono">
                            {consent.ipAddress || '-'}
                          </td>
                        </tr>
                      ))
                    )}
                  </tbody>
                </table>
              </div>
              {cookieTotalPages > 1 && (
                <div className="flex items-center justify-between mt-4">
                  <Button
                    variant="outline"
                    onClick={() => setCookiePage((p) => Math.max(1, p - 1))}
                    disabled={cookiePage === 1}
                  >
                    Previous
                  </Button>
                  <span className="text-sm text-gray-600">
                    Page {cookiePage} of {cookieTotalPages}
                  </span>
                  <Button
                    variant="outline"
                    onClick={() => setCookiePage((p) => Math.min(cookieTotalPages, p + 1))}
                    disabled={cookiePage === cookieTotalPages}
                  >
                    Next
                  </Button>
                </div>
              )}
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="user">
          <Card>
            <CardHeader>
              <CardTitle>Google OAuth Consents ({userConsents.length})</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="overflow-x-auto">
                <table className="w-full">
                  <thead>
                    <tr className="border-b border-gray-200">
                      <th className="text-left py-3 px-4 text-sm font-semibold text-gray-700">User</th>
                      <th className="text-left py-3 px-4 text-sm font-semibold text-gray-700">Consent Type</th>
                      <th className="text-left py-3 px-4 text-sm font-semibold text-gray-700">Status</th>
                      <th className="text-left py-3 px-4 text-sm font-semibold text-gray-700">Granted At</th>
                      <th className="text-left py-3 px-4 text-sm font-semibold text-gray-700">Revoked At</th>
                      <th className="text-left py-3 px-4 text-sm font-semibold text-gray-700">IP Address</th>
                    </tr>
                  </thead>
                  <tbody>
                    {userConsents.length === 0 ? (
                      <tr>
                        <td colSpan={6} className="text-center py-12 text-gray-500">
                          No user consents found
                        </td>
                      </tr>
                    ) : (
                      userConsents.map((consent) => (
                        <tr key={consent.id} className="border-b border-gray-100 hover:bg-gray-50">
                          <td className="py-4 px-4">
                            <div>
                              <p className="text-sm font-medium text-gray-900">{consent.userEmail}</p>
                              <p className="text-xs text-gray-500">{consent.userId.substring(0, 8)}...</p>
                            </div>
                          </td>
                          <td className="py-4 px-4">
                            <div className="flex items-center gap-2">
                              {consent.consentType === 'GoogleCalendar' ? (
                                <Calendar className="w-4 h-4 text-primary-600" />
                              ) : (
                                <User className="w-4 h-4 text-primary-600" />
                              )}
                              <span className="text-sm font-medium text-gray-900">
                                {consent.consentTypeName}
                              </span>
                            </div>
                          </td>
                          <td className="py-4 px-4">
                            <Badge variant={consent.granted ? 'success' : 'error'}>
                              {consent.granted ? 'Granted' : 'Revoked'}
                            </Badge>
                          </td>
                          <td className="py-4 px-4 text-sm text-gray-600">
                            {consent.grantedAt ? new Date(consent.grantedAt).toLocaleString() : '-'}
                          </td>
                          <td className="py-4 px-4 text-sm text-gray-600">
                            {consent.revokedAt ? new Date(consent.revokedAt).toLocaleString() : '-'}
                          </td>
                          <td className="py-4 px-4 text-sm text-gray-600 font-mono">
                            {consent.ipAddress || '-'}
                          </td>
                        </tr>
                      ))
                    )}
                  </tbody>
                </table>
              </div>
              {userTotalPages > 1 && (
                <div className="flex items-center justify-between mt-4">
                  <Button
                    variant="outline"
                    onClick={() => setUserPage((p) => Math.max(1, p - 1))}
                    disabled={userPage === 1}
                  >
                    Previous
                  </Button>
                  <span className="text-sm text-gray-600">
                    Page {userPage} of {userTotalPages}
                  </span>
                  <Button
                    variant="outline"
                    onClick={() => setUserPage((p) => Math.min(userTotalPages, p + 1))}
                    disabled={userPage === userTotalPages}
                  >
                    Next
                  </Button>
                </div>
              )}
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>
    </div>
  )
}

export default ConsentManagement
