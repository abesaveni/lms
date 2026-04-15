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
  // Increments whenever the token changes so the polling effect restarts
  const [tokenVersion, setTokenVersion] = useState(0)

  // NO automatic connection - only provide context state
  // Connection will be initiated by pages that need it (like Inbox)

  // Listen for token changes (login/logout) - restart polling when token appears, disconnect when removed
  useEffect(() => {
    const handleTokenChange = () => {
      const token = getAuthToken()
      if (!token || token.trim().length === 0) {
        signalRService.disconnect().catch(() => {})
        setIsConnected(false)
        setIsNotificationConnected(false)
      }
      // Bump version to re-trigger the polling effect regardless
      setTokenVersion((v) => v + 1)
    }

    window.addEventListener('storage', handleTokenChange)
    window.addEventListener('tokenUpdated', handleTokenChange)

    return () => {
      window.removeEventListener('storage', handleTokenChange)
      window.removeEventListener('tokenUpdated', handleTokenChange)
    }
  }, [])

  // Poll connection state — re-runs whenever tokenVersion changes so it starts
  // even when no token existed at mount (e.g. user logged in after app loaded)
  useEffect(() => {
    const token = getAuthToken()
    if (!token || token.trim().length === 0) {
      setIsConnected(false)
      setIsNotificationConnected(false)
      return
    }

    const checkConnectionState = () => {
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

    // Run once immediately, then every 500 ms
    checkConnectionState()
    const interval = setInterval(checkConnectionState, 500)

    return () => {
      clearInterval(interval)
    }
  }, [tokenVersion])

  return (
    <SignalRContext.Provider value={{ isConnected, isNotificationConnected }}>
      {children}
    </SignalRContext.Provider>
  )
}
