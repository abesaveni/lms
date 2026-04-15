import { useEffect, useState } from 'react'
import { Save, Upload, Eye, EyeOff } from 'lucide-react'
import { Card, CardHeader, CardTitle, CardContent } from '../../components/ui/Card'
import Button from '../../components/ui/Button'
import Input from '../../components/ui/Input'
import { Tabs, TabsList, TabsTrigger, TabsContent } from '../../components/ui/Tabs'
import ConsentManagement from '../../components/consent/ConsentManagement'
import NotificationPreferences from '../../components/settings/NotificationPreferences'
import BankDetails from '../../components/settings/BankDetails'
import { getTutorProfile, updateTutorProfile, changePassword } from '../../services/tutorApi'
import { getCurrentUser } from '../../utils/auth'

const TutorProfileSettings = () => {
  const [showPassword, setShowPassword] = useState(false)
  const [loading, setLoading] = useState(true)
  const [saving, setSaving] = useState(false)
  const [profileMessage, setProfileMessage] = useState<{ type: 'success' | 'error'; text: string } | null>(null)
  const [passwordMessage, setPasswordMessage] = useState<{ type: 'success' | 'error'; text: string } | null>(null)
  const [formData, setFormData] = useState({
    firstName: '',
    lastName: '',
    email: '',
    phone: '',
    bio: '',
    education: '',
    headline: '',
    hourlyRate1on1: 0,
    hourlyRateGroup: 0,
    location: '',
    memberSince: '',
    profilePictureUrl: '',
    profilePictureBase64: '',
    profilePictureFileName: '',
    currentPassword: '',
    newPassword: '',
    confirmPassword: '',
    language: 'English',
    timezone: 'UTC',
  })

  useEffect(() => {
    const fetchProfile = async () => {
      try {
        const data = await getTutorProfile()
        setFormData({
          ...formData,
          firstName: data.firstName || '',
          lastName: data.lastName || '',
          email: data.email || '',
          phone: data.phoneNumber || '',
          bio: data.bio || '',
          education: data.education || '',
          headline: data.headline || '',
          hourlyRate1on1: data.hourlyRate || 0,
          hourlyRateGroup: data.hourlyRateGroup || 0,
          location: data.location || '',
          memberSince: data.memberSince ? new Date(data.memberSince).toLocaleDateString('en-IN', { year: 'numeric', month: 'long', day: 'numeric' }) : '',
          profilePictureUrl: data.profilePictureUrl || '',
          language: data.languages || 'English',
          timezone: 'UTC',
        })
      } catch (error) {
        console.error('Failed to fetch profile:', error)
      } finally {
        setLoading(false)
      }
    }
    fetchProfile()
  }, [])

  const handleSaveProfile = async () => {
    setSaving(true)
    setProfileMessage(null)
    try {
      await updateTutorProfile({
        firstName: formData.firstName,
        lastName: formData.lastName,
        bio: formData.bio,
        headline: formData.headline,
        education: formData.education,
        hourlyRate: formData.hourlyRate1on1,
        hourlyRateGroup: formData.hourlyRateGroup,
        phoneNumber: formData.phone,
        profilePictureUrl: formData.profilePictureUrl,
        profilePictureBase64: formData.profilePictureBase64,
        profilePictureFileName: formData.profilePictureFileName,
        languages: formData.language,
        language: formData.language,
        timezone: formData.timezone,
        location: formData.location,
      })
      
      // Clear base64 after successful save to avoid re-sending it
      setFormData(prev => ({
        ...prev,
        profilePictureBase64: '',
        profilePictureFileName: ''
      }))
      
      setProfileMessage({ type: 'success', text: 'Profile updated successfully!' })

      // Update the user in localStorage so the header/sidebar reflect the new image immediately
      const currentUser = getCurrentUser();
      if (currentUser) {
        const updatedUser = { 
          ...currentUser, 
          profileImage: formData.profilePictureUrl // This is the new image (either Base64 preview or current URL)
        };
        localStorage.setItem('user', JSON.stringify(updatedUser));
        // Dispatch event to notify Header and Sidebar
        window.dispatchEvent(new Event('tokenUpdated'));
      }
    } catch (error: any) {
      console.error('Update failed:', error)
      setProfileMessage({ type: 'error', text: error?.message || 'Failed to update profile' })
    } finally {
      setSaving(false)
    }
  }

  const handleUpdatePassword = async () => {
    setPasswordMessage(null)

    if (!formData.currentPassword || !formData.newPassword || !formData.confirmPassword) {
      setPasswordMessage({ type: 'error', text: 'All password fields are required' })
      return
    }

    if (formData.newPassword.length < 8) {
      setPasswordMessage({ type: 'error', text: 'New password must be at least 8 characters' })
      return
    }

    if (!/(?=.*[a-z])(?=.*[A-Z])(?=.*\d)/.test(formData.newPassword)) {
      setPasswordMessage({ type: 'error', text: 'New password must contain at least one uppercase letter, one lowercase letter, and one number' })
      return
    }

    if (formData.newPassword !== formData.confirmPassword) {
      setPasswordMessage({ type: 'error', text: 'New passwords do not match' })
      return
    }

    setSaving(true)
    try {
      await changePassword({
        currentPassword: formData.currentPassword,
        newPassword: formData.newPassword
      })
      setPasswordMessage({ type: 'success', text: 'Password updated successfully! Use your new password on next login.' })
      setFormData(prev => ({
        ...prev,
        currentPassword: '',
        newPassword: '',
        confirmPassword: ''
      }))
    } catch (error: any) {
      console.error('Password update failed:', error)
      setPasswordMessage({ type: 'error', text: error.message || 'Failed to update password' })
    } finally {
      setSaving(false)
    }
  }

  const compressImage = (file: File): Promise<string> => {
    return new Promise((resolve, reject) => {
      const reader = new FileReader();
      reader.readAsDataURL(file);
      reader.onload = (event) => {
        const img = new Image();
        img.src = event.target?.result as string;
        img.onload = () => {
          const canvas = document.createElement('canvas');
          const MAX_WIDTH = 800;
          const MAX_HEIGHT = 800;
          let width = img.width;
          let height = img.height;

          if (width > height) {
            if (width > MAX_WIDTH) {
              height *= MAX_WIDTH / width;
              width = MAX_WIDTH;
            }
          } else {
            if (height > MAX_HEIGHT) {
              width *= MAX_HEIGHT / height;
              height = MAX_HEIGHT;
            }
          }

          canvas.width = width;
          canvas.height = height;
          const ctx = canvas.getContext('2d');
          ctx?.drawImage(img, 0, 0, width, height);
          
          // Compress to JPEG with 0.7 quality
          const dataUrl = canvas.toDataURL('image/jpeg', 0.7);
          resolve(dataUrl);
        };
        img.onerror = (err) => reject(err);
      };
      reader.onerror = (err) => reject(err);
    });
  };

  const handleImageUpload = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0]
    if (!file) return

    try {
      // Compress and then set in formData
      const compressedBase64 = await compressImage(file);
      setFormData({
        ...formData,
        profilePictureUrl: compressedBase64, // Local preview
        profilePictureBase64: compressedBase64,
        profilePictureFileName: file.name
      });
    } catch (error) {
      console.error('Compression failed:', error);
      // Fallback to regular Base64 if compression fails
      const reader = new FileReader();
      reader.onloadend = () => {
        setFormData({
          ...formData,
          profilePictureUrl: reader.result as string,
          profilePictureBase64: reader.result as string,
          profilePictureFileName: file.name
        });
      };
      reader.readAsDataURL(file);
    }
  }

  if (loading) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-primary-600"></div>
      </div>
    )
  }

  return (
    <div className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <div className="mb-8">
        <h1 className="text-3xl font-bold text-gray-900 mb-2">Profile Settings</h1>
        <p className="text-gray-600">Manage your tutor profile and account settings</p>
      </div>

      <Tabs defaultValue="profile">
        <TabsList>
          <TabsTrigger value="profile">Profile</TabsTrigger>
          <TabsTrigger value="pricing">Pricing</TabsTrigger>
          <TabsTrigger value="bank">Bank Details</TabsTrigger>
          <TabsTrigger value="password">Password</TabsTrigger>
          <TabsTrigger value="preferences">Preferences</TabsTrigger>
          <TabsTrigger value="privacy">Privacy & Consents</TabsTrigger>
        </TabsList>

        <TabsContent value="profile">
          <Card>
            <CardHeader>
              <CardTitle>Profile Information</CardTitle>
            </CardHeader>
            <CardContent className="space-y-6">
              <div className="flex items-center gap-6">
                {formData.profilePictureUrl ? (
                  <img src={formData.profilePictureUrl} alt="Profile" className="w-24 h-24 rounded-full object-cover border-4 border-primary-100" />
                ) : (
                  <div className="w-24 h-24 rounded-full bg-gradient-primary flex items-center justify-center text-white text-2xl font-bold">
                    {formData.firstName?.[0]}{formData.lastName?.[0]}
                  </div>
                )}
                <div>
                  <input
                    type="file"
                    id="profile-pic"
                    className="hidden"
                    accept="image/*"
                    onChange={handleImageUpload}
                  />
                  <Button variant="outline" size="sm" onClick={() => document.getElementById('profile-pic')?.click()}>
                    <Upload className="mr-2 w-4 h-4" />
                    Upload Photo
                  </Button>
                  <p className="text-xs text-gray-500 mt-2">JPG, PNG or GIF. Max size 2MB</p>
                </div>
              </div>
              <div className="grid md:grid-cols-2 gap-4">
                <Input
                  label="First Name"
                  value={formData.firstName}
                  onChange={(e) => setFormData({ ...formData, firstName: e.target.value })}
                />
                <Input
                  label="Last Name"
                  value={formData.lastName}
                  onChange={(e) => setFormData({ ...formData, lastName: e.target.value })}
                />
                <Input
                  label="Email"
                  type="email"
                  value={formData.email}
                  disabled
                  className="bg-gray-50"
                />
                <Input
                  label="Phone Number"
                  value={formData.phone}
                  onChange={(e) => setFormData({ ...formData, phone: e.target.value })}
                />
                <Input
                  label="Headline"
                  value={formData.headline}
                  onChange={(e) => setFormData({ ...formData, headline: e.target.value })}
                  className="md:col-span-2"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1.5">
                  Bio
                </label>
                <textarea
                  className="w-full px-4 py-2.5 rounded-lg border border-gray-300 focus:outline-none focus:ring-2 focus:ring-primary-500"
                  rows={4}
                  value={formData.bio}
                  onChange={(e) => setFormData({ ...formData, bio: e.target.value })}
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1.5">
                  Education
                </label>
                <Input
                  value={formData.education}
                  onChange={(e) => setFormData({ ...formData, education: e.target.value })}
                />
              </div>
              <Input
                label="Location (City, Country)"
                value={formData.location}
                onChange={(e) => setFormData({ ...formData, location: e.target.value })}
                placeholder="e.g. Mumbai, India"
              />
              {formData.memberSince && (
                <div className="flex items-center gap-2 text-sm text-gray-500 bg-gray-50 px-4 py-2.5 rounded-lg border border-gray-200">
                  <span className="font-medium text-gray-700">Member since:</span>
                  <span>{formData.memberSince}</span>
                </div>
              )}
              {profileMessage && (
                <div className={`p-3 rounded-lg text-sm font-medium ${profileMessage.type === 'success' ? 'bg-green-50 text-green-700 border border-green-200' : 'bg-red-50 text-red-700 border border-red-200'}`}>
                  {profileMessage.text}
                </div>
              )}
              <div className="pt-4 border-t border-gray-200">
                <Button onClick={handleSaveProfile} disabled={saving}>
                  <Save className="mr-2 w-5 h-5" />
                  {saving ? 'Saving...' : 'Save Changes'}
                </Button>
              </div>
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="pricing">
          <Card>
            <CardHeader>
              <CardTitle>Pricing Settings</CardTitle>
            </CardHeader>
            <CardContent className="space-y-6">
              <div className="grid sm:grid-cols-2 gap-6 max-w-lg">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1.5">
                    1-on-1 Hourly Rate (₹)
                  </label>
                  <div className="relative">
                    <span className="absolute left-4 top-1/2 transform -translate-y-1/2 text-gray-500 font-medium">₹</span>
                    <Input
                      type="number"
                      value={formData.hourlyRate1on1}
                      onChange={(e) => setFormData({ ...formData, hourlyRate1on1: Number(e.target.value) })}
                      className="pl-8"
                      min="0"
                    />
                  </div>
                  <p className="text-xs text-gray-500 mt-1.5">Your default 1-on-1 session rate shown on your public profile.</p>
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1.5">
                    Group Session Rate (₹)
                  </label>
                  <div className="relative">
                    <span className="absolute left-4 top-1/2 transform -translate-y-1/2 text-gray-500 font-medium">₹</span>
                    <Input
                      type="number"
                      value={formData.hourlyRateGroup}
                      onChange={(e) => setFormData({ ...formData, hourlyRateGroup: Number(e.target.value) })}
                      className="pl-8"
                      min="0"
                    />
                  </div>
                  <p className="text-xs text-gray-500 mt-1.5">Your default group session rate shown on your public profile.</p>
                </div>
              </div>
              {profileMessage && (
                <div className={`p-3 rounded-lg text-sm font-medium ${profileMessage.type === 'success' ? 'bg-green-50 text-green-700 border border-green-200' : 'bg-red-50 text-red-700 border border-red-200'}`}>
                  {profileMessage.text}
                </div>
              )}
              <div className="pt-4 border-t border-gray-200">
                <Button onClick={handleSaveProfile} disabled={saving}>
                  <Save className="mr-2 w-5 h-5" />
                  {saving ? 'Updating...' : 'Update Pricing'}
                </Button>
              </div>
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="bank">
          <Card>
            <CardHeader>
              <CardTitle>Bank Account Details</CardTitle>
            </CardHeader>
            <CardContent>
              <BankDetails />
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="password">
          <Card>
            <CardHeader>
              <CardTitle>Change Password</CardTitle>
            </CardHeader>
            <CardContent className="space-y-6">
              {passwordMessage && (
                <div className={`px-4 py-3 rounded-lg text-sm font-medium ${passwordMessage.type === 'success' ? 'bg-green-50 text-green-700 border border-green-200' : 'bg-red-50 text-red-700 border border-red-200'}`}>
                  {passwordMessage.text}
                </div>
              )}
              <div className="bg-blue-50 border border-blue-200 rounded-lg px-4 py-3 text-sm text-blue-700">
                <p className="font-medium mb-1">Password requirements:</p>
                <ul className="list-disc list-inside space-y-0.5">
                  <li>At least 8 characters</li>
                  <li>At least one uppercase letter (A–Z)</li>
                  <li>At least one lowercase letter (a–z)</li>
                  <li>At least one number (0–9)</li>
                </ul>
              </div>
              <div className="relative">
                <Input
                  label="Current Password"
                  type={showPassword ? 'text' : 'password'}
                  value={formData.currentPassword}
                  onChange={(e) => { setPasswordMessage(null); setFormData({ ...formData, currentPassword: e.target.value }) }}
                  className="pr-10"
                />
                <button
                  onClick={() => setShowPassword(!showPassword)}
                  className="absolute right-3 top-8 text-gray-500 hover:text-gray-700"
                >
                  {showPassword ? <EyeOff className="w-5 h-5" /> : <Eye className="w-5 h-5" />}
                </button>
              </div>
              <Input
                label="New Password"
                type="password"
                value={formData.newPassword}
                onChange={(e) => { setPasswordMessage(null); setFormData({ ...formData, newPassword: e.target.value }) }}
              />
              <Input
                label="Confirm New Password"
                type="password"
                value={formData.confirmPassword}
                onChange={(e) => { setPasswordMessage(null); setFormData({ ...formData, confirmPassword: e.target.value }) }}
              />
              <div className="pt-4 border-t border-gray-200">
                <Button onClick={handleUpdatePassword} disabled={saving}>
                  <Save className="mr-2 w-5 h-5" />
                  {saving ? 'Updating...' : 'Update Password'}
                </Button>
              </div>
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="preferences">
          <Card>
            <CardHeader>
              <CardTitle>Teaching Preferences</CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              <NotificationPreferences />
              <div className="border-t border-gray-200 pt-4"></div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Language
                </label>
                <select 
                  className="w-full px-4 py-2.5 rounded-lg border border-gray-300 focus:outline-none focus:ring-2 focus:ring-primary-500"
                  value={formData.language}
                  onChange={(e) => setFormData({ ...formData, language: e.target.value })}
                >
                  <option value="English">English</option>
                  <option value="Spanish">Spanish</option>
                  <option value="French">French</option>
                </select>
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Timezone
                </label>
                <select 
                  className="w-full px-4 py-2.5 rounded-lg border border-gray-300 focus:outline-none focus:ring-2 focus:ring-primary-500"
                  value={formData.timezone}
                  onChange={(e) => setFormData({ ...formData, timezone: e.target.value })}
                >
                  <option value="UTC">UTC</option>
                  <option value="America/New_York">America/New_York</option>
                  <option value="Europe/London">Europe/London</option>
                </select>
              </div>
              {profileMessage && (
                <div className={`p-3 rounded-lg text-sm font-medium ${profileMessage.type === 'success' ? 'bg-green-50 text-green-700 border border-green-200' : 'bg-red-50 text-red-700 border border-red-200'}`}>
                  {profileMessage.text}
                </div>
              )}
              <div className="pt-4 border-t border-gray-200">
                <Button onClick={handleSaveProfile} disabled={saving}>
                  <Save className="mr-2 w-5 h-5" />
                  Save Preferences
                </Button>
              </div>
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="privacy">
          <ConsentManagement />
        </TabsContent>
      </Tabs>
    </div>
  )
}

export default TutorProfileSettings
