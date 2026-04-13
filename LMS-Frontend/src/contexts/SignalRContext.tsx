import { createContext, useContext, useEffect, useState, ReactNode } from 'react'
import { signalRService } from '../services/signalr'
import { getAuthToken } from '../services/api'
import * as signalR from '@microsoft/signalr'

interface SignalRContextType {
  isConnected: boolean
  isNotificationConnected: boolean
}

const SignalRContext = createContext<SignalRContextType>({
  isConnected: false,
  isNotificationConnected: false,
})

export const useSignalR = () => useContext(SignalRContext)

interface SignalRProviderProps {
  children: ReactNode
}

export const SignalRProvider = ({ children }: SignalRProviderProps) => {
  const [isConnected, setIsConnected] = useState(false)
  const [isNotificationConnected, setIsNotificationConnected] = useState(false)

  // NO automatic connection - only provide context state
  // Connection will be initiated by pages that need it (like Inbox)
  
  // Listen for connection state changes from signalRService (only if token exists)
  useEffect(() => {
    const token = getAuthToken()
    if (!token || token.trim().length === 0) {
      // No token - don't even check connection state
      setIsConnected(false)
      setIsNotificationConnected(false)
      return
    }

    const checkConnectionState = () => {
      // Double-check token before checking state
      const currentToken = getAuthToken()
      if (!currentToken || currentToken.trim().length === 0) {
        setIsConnected(false)
        setIsNotificationConnected(false)
        return
      }

      const chatState = signalRService.getChatConnectionState()
      const notificationState = signalRService.getNotificationConnectionState()
      setIsConnected(chatState === signalR.HubConnectionState.Connected)
      setIsNotificationConnected(notificationState === signalR.HubConnectionState.Connected)
    }

    // Check connection state periodically (only if token exists)
    const interval = setInterval(checkConnectionState, 2000) // Reduced frequency

    return () => {
      clearInterval(interval)
    }
  }, [])

  // Listen for token changes (logout) - disconnect if token is removed
  useEffect(() => {
    const handleStorageChange = () => {
      const token = getAuthToken()
      
      // If no token, disconnect immediately
      if (!token || token.trim().length === 0) {
        signalRService.disconnect().catch(() => {})
        setIsConnected(false)
        setIsNotificationConnected(false)
      }
    }

    window.addEventListener('storage', handleStorageChange)
    window.addEventListener('tokenUpdated', handleStorageChange)

    return () => {
      window.removeEventListener('storage', handleStorageChange)
      window.removeEventListener('tokenUpdated', handleStorageChange)
    }
  }, [])

  return (
    <SignalRContext.Provider value={{ isConnected, isNotificationConnected }}>
      {children}
    </SignalRContext.Provider>
  )
}
