import { useState } from 'react'
import { TutorMatchComponent } from '../../components/ai/TutorMatchComponent'
import { ChurnPredictionDashboard } from '../../components/ai/ChurnPredictionDashboard'
import { FraudDetectionTool } from '../../components/ai/FraudDetectionTool'
import { DisputeResolutionTool } from '../../components/ai/DisputeResolutionTool'
import { RevenueAnalyticsDashboard } from '../../components/ai/RevenueAnalyticsDashboard'
import { SupportTriageDashboard } from '../../components/ai/SupportTriageDashboard'

const TABS = [
  { id: 'tutor-match', label: 'Tutor Match' },
  { id: 'churn', label: 'Churn Prediction' },
  { id: 'fraud', label: 'Fraud Detection' },
  { id: 'dispute', label: 'Dispute Resolution' },
  { id: 'revenue', label: 'Revenue Analytics' },
  { id: 'support', label: 'Support Triage' },
]

const AdminAITools = () => {
  const [activeTab, setActiveTab] = useState('tutor-match')

  const renderTab = () => {
    switch (activeTab) {
      case 'tutor-match': return <TutorMatchComponent />
      case 'churn': return <ChurnPredictionDashboard />
      case 'fraud': return <FraudDetectionTool />
      case 'dispute': return <DisputeResolutionTool />
      case 'revenue': return <RevenueAnalyticsDashboard />
      case 'support': return <SupportTriageDashboard />
      default: return null
    }
  }

  return (
    <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <div className="mb-6">
        <h1 className="text-3xl font-bold text-gray-900">AI Tools</h1>
        <p className="text-gray-600 mt-1">Powered by Claude AI — analytics and automation tools for admins</p>
      </div>

      <div className="border-b border-gray-200 mb-6">
        <nav className="-mb-px flex gap-1 overflow-x-auto">
          {TABS.map(tab => (
            <button
              key={tab.id}
              onClick={() => setActiveTab(tab.id)}
              className={`whitespace-nowrap px-4 py-2.5 text-sm font-medium border-b-2 transition-colors ${
                activeTab === tab.id
                  ? 'border-primary-600 text-primary-600'
                  : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
              }`}
            >
              {tab.label}
            </button>
          ))}
        </nav>
      </div>

      <div>{renderTab()}</div>
    </div>
  )
}

export default AdminAITools
