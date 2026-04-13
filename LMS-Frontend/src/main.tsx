import React from 'react'
import ReactDOM from 'react-dom/client'
import App from './App.tsx'
import './index.css'
import ErrorBoundary from './components/ErrorBoundary'
import { initializeConsentEnforcement } from './utils/consentEnforcement'

const rootElement = document.getElementById('root')
if (!rootElement) {
  throw new Error('Root element not found')
}

// Render the app first
ReactDOM.createRoot(rootElement).render(
  <React.StrictMode>
    <ErrorBoundary>
      <App />
    </ErrorBoundary>
  </React.StrictMode>,
)

// Initialize consent enforcement after React has rendered
if (typeof window !== 'undefined') {
  // Use setTimeout to ensure DOM is fully ready
  setTimeout(() => {
    try {
      initializeConsentEnforcement()
    } catch (error) {
      console.error('Failed to initialize consent enforcement:', error)
    }
  }, 0)
}
