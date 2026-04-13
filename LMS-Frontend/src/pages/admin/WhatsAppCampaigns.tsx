import { useEffect, useState } from 'react'
import { Plus, Upload, Send, Users, Calendar } from 'lucide-react'
import { Card, CardHeader, CardTitle, CardContent } from '../../components/ui/Card'
import Button from '../../components/ui/Button'
import Input from '../../components/ui/Input'
import { Badge } from '../../components/ui/Badge'
import { Tabs, TabsList, TabsTrigger, TabsContent } from '../../components/ui/Tabs'
import { getWhatsAppCampaigns, createWhatsAppCampaign, WhatsAppCampaign } from '../../services/adminApi'

const WhatsAppCampaigns = () => {
  const [campaignName, setCampaignName] = useState('')
  const [message, setMessage] = useState('')
  const [numbers, setNumbers] = useState<string[]>([])
  const [numberInput, setNumberInput] = useState('')
  const [selectedFile, setSelectedFile] = useState<File | null>(null)
  const [campaigns, setCampaigns] = useState<WhatsAppCampaign[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    const loadCampaigns = async () => {
      setIsLoading(true)
      setError(null)
      try {
        const data = await getWhatsAppCampaigns()
        setCampaigns(data)
      } catch (err: any) {
        setError(err.message || 'Failed to load campaigns')
      } finally {
        setIsLoading(false)
      }
    }
    loadCampaigns()
  }, [])

  const handleAddNumber = () => {
    if (numberInput.trim() && !numbers.includes(numberInput.trim())) {
      setNumbers([...numbers, numberInput.trim()])
      setNumberInput('')
    }
  }

  const handleRemoveNumber = (num: string) => {
    setNumbers(numbers.filter((n) => n !== num))
  }

  const handleFileUpload = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0]
    if (file) {
      setSelectedFile(file)
    }
  }

  const handleCreateCampaign = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!campaignName || !message || (numbers.length === 0 && !selectedFile)) {
      alert('Please fill all fields and add at least one number')
      return
    }
    setError(null)
    try {
      const formData = new FormData()
      formData.append('Name', campaignName)
      formData.append('Message', message)
      if (numbers.length > 0) {
        formData.append('PhoneNumbers', JSON.stringify(numbers))
      }
      if (selectedFile) {
        formData.append('PhoneNumberFile', selectedFile)
      }

      await createWhatsAppCampaign(formData)

      setCampaignName('')
      setMessage('')
      setNumbers([])
      setNumberInput('')
      setSelectedFile(null)

      const data = await getWhatsAppCampaigns()
      setCampaigns(data)
    } catch (err: any) {
      setError(err.message || 'Failed to create campaign')
    }
  }

  return (
    <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <div className="mb-8">
        <h1 className="text-3xl font-bold text-gray-900 mb-2">WhatsApp Campaigns</h1>
        <p className="text-gray-600">Create and manage WhatsApp messaging campaigns</p>
      </div>

      <Tabs defaultValue="create">
        <TabsList>
          <TabsTrigger value="create">Create Campaign</TabsTrigger>
          <TabsTrigger value="history">Campaign History</TabsTrigger>
        </TabsList>

        <TabsContent value="create">
          <Card>
            <CardHeader>
              <CardTitle>Create New Campaign</CardTitle>
            </CardHeader>
            <CardContent>
              {error && <div className="mb-4 text-sm text-red-600">{error}</div>}
              <form onSubmit={handleCreateCampaign} className="space-y-6">
                <Input
                  label="Campaign Name"
                  value={campaignName}
                  onChange={(e) => setCampaignName(e.target.value)}
                  placeholder="e.g., Welcome Campaign, Session Reminder"
                  required
                />

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">
                    Message Content
                  </label>
                  <textarea
                    value={message}
                    onChange={(e) => setMessage(e.target.value)}
                    rows={6}
                    className="w-full px-4 py-2.5 rounded-lg border border-gray-300 focus:outline-none focus:ring-2 focus:ring-primary-500"
                    placeholder="Enter your WhatsApp message here..."
                    required
                  />
                  <p className="mt-2 text-sm text-gray-500">
                    {message.length} characters (WhatsApp limit: 4096 characters)
                  </p>
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">
                    Recipient Numbers
                  </label>
                  <div className="flex gap-2 mb-4">
                    <Input
                      type="tel"
                      value={numberInput}
                      onChange={(e) => setNumberInput(e.target.value)}
                      placeholder="+91 98765 43210"
                      className="flex-1"
                      onKeyPress={(e) => {
                        if (e.key === 'Enter') {
                          e.preventDefault()
                          handleAddNumber()
                        }
                      }}
                    />
                    <Button type="button" onClick={handleAddNumber}>
                      <Plus className="mr-2 w-4 h-4" />
                      Add
                    </Button>
                  </div>

                  <div className="mb-4">
                    <label className="flex items-center gap-2 p-4 border-2 border-dashed border-gray-300 rounded-lg cursor-pointer hover:border-primary-500 transition-colors">
                      <Upload className="w-5 h-5 text-gray-400" />
                      <span className="text-sm text-gray-600">
                        {selectedFile ? selectedFile.name : 'Upload CSV file with numbers'}
                      </span>
                      <input
                        type="file"
                        accept=".csv"
                        onChange={handleFileUpload}
                        className="hidden"
                      />
                    </label>
                    <p className="mt-2 text-xs text-gray-500">
                      CSV format: One phone number per line (e.g., +919876543210)
                    </p>
                  </div>

                  {numbers.length > 0 && (
                    <div className="p-4 bg-gray-50 rounded-lg">
                      <div className="flex items-center justify-between mb-3">
                        <span className="text-sm font-medium text-gray-700">
                          {numbers.length} number(s) added
                        </span>
                        <Button
                          type="button"
                          size="sm"
                          variant="ghost"
                          onClick={() => setNumbers([])}
                        >
                          Clear All
                        </Button>
                      </div>
                      <div className="flex flex-wrap gap-2">
                        {numbers.map((num, idx) => (
                          <Badge key={idx} variant="default" className="flex items-center gap-1">
                            {num}
                            <button
                              type="button"
                              onClick={() => handleRemoveNumber(num)}
                              className="ml-1 hover:text-red-600"
                            >
                              ×
                            </button>
                          </Badge>
                        ))}
                      </div>
                    </div>
                  )}
                </div>

                <div className="flex gap-3">
                  <Button type="submit">
                    <Send className="mr-2 w-5 h-5" />
                    Create & Send Campaign
                  </Button>
                  <Button type="button" variant="outline">
                    Save as Draft
                  </Button>
                </div>
              </form>
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="history">
          <Card>
            <CardHeader>
              <CardTitle>Campaign History</CardTitle>
            </CardHeader>
            <CardContent>
              {isLoading ? (
                <div className="text-gray-500">Loading campaigns...</div>
              ) : campaigns.length === 0 ? (
                <div className="text-gray-500">No campaigns found</div>
              ) : (
                <div className="space-y-4">
                  {campaigns.map((campaign) => (
                    <div
                      key={campaign.id}
                      className="p-6 border border-gray-200 rounded-lg hover:border-gray-300 transition-colors"
                    >
                      <div className="flex items-start justify-between mb-4">
                        <div>
                          <h3 className="text-lg font-semibold text-gray-900 mb-1">
                            {campaign.name}
                          </h3>
                          <p className="text-sm text-gray-600 mb-2">{campaign.message}</p>
                          <div className="flex items-center gap-4 text-sm text-gray-500">
                            <div className="flex items-center gap-1">
                              <Calendar className="w-4 h-4" />
                              {new Date(campaign.createdAt).toLocaleDateString()}
                            </div>
                            <div className="flex items-center gap-1">
                              <Users className="w-4 h-4" />
                              {campaign.totalRecipients} recipients
                            </div>
                            <div className="flex items-center gap-1">
                              <Send className="w-4 h-4" />
                              {campaign.sentCount} sent
                            </div>
                          </div>
                        </div>
                        <Badge
                          variant={campaign.status === 'Completed' ? 'success' : 'warning'}
                        >
                          {campaign.status}
                        </Badge>
                      </div>
                      <div className="text-xs text-gray-500">
                        Created by {campaign.createdBy || 'System'}
                      </div>
                    </div>
                  ))}
                </div>
              )}
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>
    </div>
  )
}

export default WhatsAppCampaigns
