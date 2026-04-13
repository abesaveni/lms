import { useState } from 'react'
import { Key, Eye, EyeOff, Save, Plus, Trash2 } from 'lucide-react'
import { Card, CardHeader, CardTitle, CardContent } from '../../components/ui/Card'
import Button from '../../components/ui/Button'
import Input from '../../components/ui/Input'

const ApiSettings = () => {
  const [showKeys, setShowKeys] = useState<Record<string, boolean>>({})

  const apiServices = [
    {
      id: 1,
      name: 'Google Calendar OAuth',
      provider: 'GoogleCalendar',
      keys: [
        { name: 'Client ID', value: 'xxxxxxxxxxxxx.apps.googleusercontent.com', type: 'text', keyName: 'ClientId' },
        { name: 'Client Secret', value: 'xxxxxxxxxxxxxxxxxxxx', type: 'password', keyName: 'ClientSecret' },
      ],
      description: 'Required for tutors to connect Google Calendar and create Meet links',
    },
    {
      id: 2,
      name: 'Google OAuth',
      provider: 'GoogleOAuth',
      keys: [
        { name: 'Client ID', value: 'xxxxxxxxxxxxx.apps.googleusercontent.com', type: 'text', keyName: 'ClientId' },
        { name: 'Client Secret', value: 'xxxxxxxxxxxxxxxxxxxx', type: 'password', keyName: 'ClientSecret' },
      ],
      description: 'For user authentication with Google Sign-In',
    },
    {
      id: 3,
      name: 'Razorpay',
      provider: 'Razorpay',
      keys: [
        { name: 'API Key', value: 'rzp_live_xxxxxxxxxxxxx', type: 'text', keyName: 'ApiKey' },
        { name: 'Secret Key', value: 'xxxxxxxxxxxxxxxxxxxx', type: 'password', keyName: 'SecretKey' },
      ],
    },
    {
      id: 4,
      name: 'Stripe',
      provider: 'Stripe',
      keys: [
        { name: 'Publishable Key', value: 'pk_live_xxxxxxxxxxxxx', type: 'text', keyName: 'PublishableKey' },
        { name: 'Secret Key', value: 'sk_live_xxxxxxxxxxxxx', type: 'password', keyName: 'SecretKey' },
      ],
    },
    {
      id: 5,
      name: 'Email Service (SMTP)',
      provider: 'Email',
      keys: [
        { name: 'SMTP Host', value: 'smtp.gmail.com', type: 'text', keyName: 'SmtpHost' },
        { name: 'SMTP Port', value: '587', type: 'text', keyName: 'SmtpPort' },
        { name: 'Username', value: 'noreply@liveexpert.ai', type: 'text', keyName: 'Username' },
        { name: 'Password', value: '********', type: 'password', keyName: 'Password' },
      ],
    },
    {
      id: 6,
      name: 'WhatsApp',
      provider: 'WhatsApp',
      keys: [
        { name: 'API Key', value: 'xxxxxxxxxxxxxxxxxxxx', type: 'password', keyName: 'ApiKey' },
        { name: 'API URL', value: 'https://api.whatsapp.com', type: 'text', keyName: 'ApiUrl' },
      ],
      description: 'For sending WhatsApp messages and campaign notifications',
    },
  ]

  const toggleKeyVisibility = (serviceId: number, keyIndex: number) => {
    const key = `${serviceId}-${keyIndex}`
    setShowKeys({ ...showKeys, [key]: !showKeys[key] })
  }

  return (
    <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <div className="mb-8">
        <h1 className="text-3xl font-bold text-gray-900 mb-2">API Settings</h1>
        <p className="text-gray-600">Manage API keys and service configurations</p>
      </div>

      <div className="space-y-6">
        {apiServices.map((service) => (
          <Card key={service.id}>
            <CardHeader>
              <div className="flex items-center justify-between">
                <div className="flex items-center gap-3">
                  <div className="w-10 h-10 rounded-lg bg-primary-100 flex items-center justify-center text-primary-600">
                    <Key className="w-5 h-5" />
                  </div>
                  <CardTitle>{service.name}</CardTitle>
                </div>
                <Button size="sm" variant="outline">
                  <Plus className="mr-2 w-4 h-4" />
                  Add Key
                </Button>
              </div>
            </CardHeader>
            <CardContent>
              <div className="space-y-4">
                {service.keys.map((key, idx) => (
                  <div key={idx} className="flex items-center gap-4">
                    <div className="flex-1">
                      <Input
                        label={key.name}
                        type={key.type === 'password' && !showKeys[`${service.id}-${idx}`] ? 'password' : 'text'}
                        value={key.value}
                        readOnly
                        className="bg-gray-50"
                      />
                    </div>
                    {key.type === 'password' && (
                      <button
                        onClick={() => toggleKeyVisibility(service.id, idx)}
                        className="mt-6 p-2 hover:bg-gray-100 rounded-lg transition-colors"
                      >
                        {showKeys[`${service.id}-${idx}`] ? (
                          <EyeOff className="w-5 h-5 text-gray-500" />
                        ) : (
                          <Eye className="w-5 h-5 text-gray-500" />
                        )}
                      </button>
                    )}
                    <button className="mt-6 p-2 hover:bg-gray-100 rounded-lg transition-colors text-red-600">
                      <Trash2 className="w-5 h-5" />
                    </button>
                  </div>
                ))}
                <div className="pt-4 border-t border-gray-200">
                  <Button size="sm">
                    <Save className="mr-2 w-4 h-4" />
                    Save Changes
                  </Button>
                </div>
              </div>
            </CardContent>
          </Card>
        ))}
      </div>
    </div>
  )
}

export default ApiSettings
