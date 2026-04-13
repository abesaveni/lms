import { useState, useEffect } from 'react'
import { Save, Upload, Eye, EyeOff } from 'lucide-react'
import { Card, CardHeader, CardTitle, CardContent } from '../../components/ui/Card'
import Button from '../../components/ui/Button'
import Input from '../../components/ui/Input'
import { Tabs, TabsList, TabsTrigger, TabsContent } from '../../components/ui/Tabs'
import { updateProfile, getCurrentUserProfile, UserProfileDto } from '../../services/usersApi'
import { changePassword } from '../../services/studentApi'
import { getMediaUrl } from '../../services/api'



import ConsentManagement from '../../components/consent/ConsentManagement'
import NotificationPreferences from '../../components/settings/NotificationPreferences'

const ProfileSettings = () => {
  const [showPassword, setShowPassword] = useState(false)
  const [formData, setFormData] = useState({
    name: '',
    email: '',
    phone: '',
    bio: '',
    dateOfBirth: '',
    location: '',
    profilePictureBase64: '',
    profilePictureFileName: '',
    currentPassword: '',
    newPassword: '',
    confirmPassword: '',
    language: 'English',
    timezone: 'UTC',
  })
  
  const [profile, setProfile] = useState<UserProfileDto | null>(null)
  const [isSaving, setIsSaving] = useState(false)
  const [isUploading, setIsUploading] = useState(false)
  const [loading, setLoading] = useState(true)


  useEffect(() => {
    const fetchProfile = async () => {
      try {
        const data = await getCurrentUserProfile()
        setProfile(data)
        setFormData(prev => ({
          ...prev,
          name: `${data.firstName || ''} ${data.lastName || ''}`.trim() || data.username || '',
          email: data.email || '',
          phone: data.phoneNumber || '',
          bio: data.bio || '',
          dateOfBirth: data.dateOfBirth ? data.dateOfBirth.split('T')[0] : '', // Format for date input
          location: data.location || '',
          language: data.language || 'English',
          timezone: data.timezone || 'UTC',
        }))
      } catch (err) {
        console.error(err)
      } finally {
        setLoading(false)
      }
    }

    fetchProfile()
  }, [])

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
      setIsUploading(true)
      const compressedBase64 = await compressImage(file);
      // Immediately set the preview in the local profile state
      setProfile(prev => prev ? { ...prev, profileImageUrl: compressedBase64 } : null);
      
      // Store Base64 and FileName in formData to be sent during handleSaveProfile
      setFormData(prev => ({
        ...prev,
        profilePictureBase64: compressedBase64,
        profilePictureFileName: file.name
      }));
    } catch (err: any) {
      console.error('Compression failed:', err);
      // Fallback
      const reader = new FileReader();
      reader.onloadend = () => {
        const base64 = reader.result as string;
        setProfile(prev => prev ? { ...prev, profileImageUrl: base64 } : null);
        setFormData(prev => ({
          ...prev,
          profilePictureBase64: base64,
          profilePictureFileName: file.name
        }));
        setIsUploading(false)
      };
      reader.readAsDataURL(file);
      return;
    } finally {
      setIsUploading(false)
    }

  }

  const handleSaveProfile = async () => {
    try {
      setIsSaving(true)
      const parts = formData.name.split(' ')
      const firstName = parts[0] || ''
      const lastName = parts.slice(1).join(' ') || ''
      
      await updateProfile({
        firstName,
        lastName,
        phoneNumber: formData.phone,
        bio: formData.bio,
        dateOfBirth: formData.dateOfBirth ? new Date(formData.dateOfBirth).toISOString() : undefined,
        location: formData.location || undefined,
        profilePictureBase64: formData.profilePictureBase64 || undefined,
        profilePictureFileName: formData.profilePictureFileName || undefined,
        language: formData.language,
        timezone: formData.timezone
      })

      
      // Update the user in localStorage so the header/sidebar reflect the new image immediately
      const { getCurrentUser } = await import('../../utils/auth')
      const currentUser = getCurrentUser();
      if (currentUser) {
        // Use the current preview URL (Base64) for immediate update, 
        // Backend will eventually return the saved URL on next fetch
        const updatedUser = { 
          ...currentUser, 
          profileImage: profile?.profileImageUrl, 
          username: formData.name 
        };

        localStorage.setItem('user', JSON.stringify(updatedUser));
        // Dispatch event to notify Header and Sidebar
        window.dispatchEvent(new Event('tokenUpdated'));
        window.dispatchEvent(new Event('profileUpdated'));
      }


      alert('Profile updated successfully')
      
      // Clear base64 after successful save
      setFormData(prev => ({
        ...prev,
        profilePictureBase64: '',
        profilePictureFileName: ''
      }))
      
    } catch (err: any) {
      alert(err.message || 'Failed to update profile')
    } finally {
      setIsSaving(false)
    }
  }
  
  const handleUpdatePassword = async () => {
    if (!formData.currentPassword || !formData.newPassword || !formData.confirmPassword) {
      alert('All password fields are required')
      return
    }
    
    if (formData.newPassword !== formData.confirmPassword) {
      alert('New passwords do not match')
      return
    }
    
    try {
      setIsSaving(true)
      await changePassword({
        currentPassword: formData.currentPassword,
        newPassword: formData.newPassword
      })
      alert('Password updated successfully')
      setFormData(prev => ({
        ...prev,
        currentPassword: '',
        newPassword: '',
        confirmPassword: ''
      }))
    } catch (err: any) {
      alert(err.message || 'Failed to update password')
    } finally {
      setIsSaving(false)
    }
  }

  if (loading) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-primary-600"></div>
      </div>
    )
  }

  const currentProfileImageUrl = getMediaUrl(profile?.profileImageUrl)

  return (
    <div className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <div className="mb-8">
        <h1 className="text-3xl font-bold text-gray-900 mb-2">Profile Settings</h1>
        <p className="text-gray-600">Manage your account settings and preferences</p>
      </div>

      <Tabs defaultValue="profile">
        <TabsList>
          <TabsTrigger value="profile">Profile</TabsTrigger>
          <TabsTrigger value="password">Password</TabsTrigger>
          <TabsTrigger value="preferences">Preferences</TabsTrigger>
          <TabsTrigger value="privacy">Privacy</TabsTrigger>
        </TabsList>

        <TabsContent value="profile">
          <Card>
            <CardHeader>
              <CardTitle>Personal Information</CardTitle>
            </CardHeader>
            <CardContent className="space-y-6">
              <div className="flex items-center gap-6">
                {currentProfileImageUrl ? (
                   <img src={currentProfileImageUrl} alt="Profile" className="w-24 h-24 rounded-full object-cover border-4 border-primary-100 shadow-sm" />
                ) : (
                   <div className="w-24 h-24 rounded-full bg-gradient-primary flex items-center justify-center text-white text-2xl font-bold">
                     {formData.name ? formData.name.substring(0, 2).toUpperCase() : 'JD'}
                   </div>
                )}
                <div>
                  <input 
                    type="file" 
                    id="profile-pic"
                    className="hidden" 
                    accept="image/jpeg,image/png,image/gif" 
                    onChange={handleImageUpload} 
                  />
                  <Button variant="outline" size="sm" onClick={() => document.getElementById('profile-pic')?.click()} disabled={isSaving}>
                    <Upload className="mr-2 w-4 h-4" />
                    {isUploading ? 'Uploading...' : 'Upload Photo'}
                  </Button>
                  <p className="text-xs text-gray-500 mt-2">JPG, PNG or GIF. Max size 2MB</p>
                </div>
              </div>
              <div className="grid md:grid-cols-2 gap-4">
                <Input
                  label="Full Name"
                  value={formData.name}
                  onChange={(e) => setFormData({ ...formData, name: e.target.value })}
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
                  label="Date of Birth"
                  type="date"
                  value={formData.dateOfBirth}
                  onChange={(e) => setFormData({ ...formData, dateOfBirth: e.target.value })}
                />
                <Input
                  label="Location"
                  value={formData.location}
                  onChange={(e) => setFormData({ ...formData, location: e.target.value })}
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
              <div className="pt-4 border-t border-gray-200">
                <Button onClick={handleSaveProfile} disabled={isSaving}>
                  <Save className="mr-2 w-5 h-5" />
                  {isSaving ? 'Saving...' : 'Save Changes'}
                </Button>
              </div>
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="password">
          <Card>
            <CardHeader>
              <CardTitle>Change Password</CardTitle>
            </CardHeader>
            <CardContent className="space-y-6">
              <div className="relative">
                <Input
                  label="Current Password"
                  type={showPassword ? 'text' : 'password'}
                  value={formData.currentPassword}
                  onChange={(e) => setFormData({ ...formData, currentPassword: e.target.value })}
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
                onChange={(e) => setFormData({ ...formData, newPassword: e.target.value })}
              />
              <Input
                label="Confirm New Password"
                type="password"
                value={formData.confirmPassword}
                onChange={(e) => setFormData({ ...formData, confirmPassword: e.target.value })}
              />
              <div className="pt-4 border-t border-gray-200">
                <Button onClick={handleUpdatePassword} disabled={isSaving}>
                  <Save className="mr-2 w-5 h-5" />
                  {isSaving ? 'Updating...' : 'Update Password'}
                </Button>
              </div>

            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="preferences">
          <Card>
            <CardHeader>
              <CardTitle>Preferences</CardTitle>
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
              <div className="pt-4 border-t border-gray-200">
                <Button onClick={handleSaveProfile} disabled={isSaving}>
                  <Save className="mr-2 w-5 h-5" />
                  {isSaving ? 'Saving...' : 'Save Preferences'}
                </Button>
              </div>
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="privacy">
          <div className="space-y-6">
            <Card>
              <CardHeader>
                <CardTitle>Privacy Settings</CardTitle>
              </CardHeader>
              <CardContent className="space-y-4">
                <div className="flex items-center justify-between p-4 border border-gray-200 rounded-lg">
                  <div>
                    <p className="font-medium text-gray-900">Profile Visibility</p>
                    <p className="text-sm text-gray-600">Make your profile visible to tutors</p>
                  </div>
                  <label className="relative inline-flex items-center cursor-pointer">
                    <input type="checkbox" className="sr-only peer" defaultChecked />
                    <div className="w-11 h-6 bg-gray-200 peer-focus:outline-none peer-focus:ring-4 peer-focus:ring-primary-300 rounded-full peer peer-checked:after:translate-x-full peer-checked:after:border-white after:content-[''] after:absolute after:top-[2px] after:left-[2px] after:bg-white after:border-gray-300 after:border after:rounded-full after:h-5 after:w-5 after:transition-all peer-checked:bg-primary-600"></div>
                  </label>
                </div>
                <div className="flex items-center justify-between p-4 border border-gray-200 rounded-lg">
                  <div>
                    <p className="font-medium text-gray-900">Email Notifications</p>
                    <p className="text-sm text-gray-600">Receive email updates</p>
                  </div>
                  <label className="relative inline-flex items-center cursor-pointer">
                    <input type="checkbox" className="sr-only peer" defaultChecked />
                    <div className="w-11 h-6 bg-gray-200 peer-focus:outline-none peer-focus:ring-4 peer-focus:ring-primary-300 rounded-full peer peer-checked:after:translate-x-full peer-checked:after:border-white after:content-[''] after:absolute after:top-[2px] after:left-[2px] after:bg-white after:border-gray-300 after:border after:rounded-full after:h-5 after:w-5 after:transition-all peer-checked:bg-primary-600"></div>
                  </label>
                </div>
              </CardContent>
            </Card>

            {/* Consent Management */}
            <ConsentManagement />
          </div>
        </TabsContent>
      </Tabs>
    </div>
  )
}

export default ProfileSettings
