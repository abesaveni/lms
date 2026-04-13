import * as signalR from '@microsoft/signalr'

// Get API base URL (without /api suffix for SignalR)
const getApiBaseUrl = (): string => {
  const env = (import.meta as any).env
  const base = env?.VITE_API_URL || env?.VITE_API_BASE_URL
  if (base) {
    return base.replace('/api', '')
  }
  return 'http://localhost:5128'
}

const API_BASE_URL = getApiBaseUrl()

// Version marker - NUCLEAR FIX VERSION
console.log('[SignalR] Service loaded - Version 4.0 (Hard Isolation - No Auto-Connect)')

/**
 * Create SignalR Hub connection ONLY when token exists
 * ❗ NO automatic reconnect - connections must be explicitly managed
 * ❗ Connections created ONLY inside functions - never at module scope
 */
export function createHubConnection(
  url: string,
  token: string
): signalR.HubConnection | null {
  if (!token || token.trim().length === 0) {
    console.warn('[SignalR] Connection blocked: no token')
    return null
  }

  const connection = new signalR.HubConnectionBuilder()
    .withUrl(url, {
      accessTokenFactory: () => token,
      skipNegotiation: false,
      transport: signalR.HttpTransportType.WebSockets,
    })
    .build()

  // Block retries on 401 - hard stop
  connection.onclose((error) => {
    if (error) {
      const errorMessage = error.message || String(error)
      if (errorMessage.includes('401') || errorMessage.includes('Unauthorized')) {
        console.warn('[SignalR] Connection stopped due to auth failure - no retry')
        connection.stop().catch(() => { })
      }
    }
  })

  return connection
}

/**
 * Service class for managing SignalR connections
 * Connections are created lazily and explicitly - never auto-started
 */
class SignalRService {
  private chatConnection: signalR.HubConnection | null = null
  private notificationConnection: signalR.HubConnection | null = null
  private isConnectingChat: boolean = false
  private isConnectingNotification: boolean = false

  // Get auth token from localStorage
  private getAuthToken(): string | null {
    return localStorage.getItem('token')
  }

  /**
   * Connect Chat Hub - ONLY when authenticated
   * Returns true if connection started, false if blocked
   */
  async connectChatHub(): Promise<boolean> {
    // CRITICAL: Check token FIRST - if no token, exit immediately
    const token = this.getAuthToken()
    if (!token || token.trim().length === 0) {
      console.warn('[SignalR] Chat Hub blocked: user not authenticated')
      if (this.chatConnection) {
        try {
          await this.chatConnection.stop()
        } catch (e) {
          // Ignore
        }
        this.chatConnection = null
      }
      this.isConnectingChat = false
      return false
    }

    // If already connected, don't reconnect
    if (this.chatConnection && this.chatConnection.state === signalR.HubConnectionState.Connected) {
      this.isConnectingChat = false
      return true
    }

    // Prevent multiple simultaneous connection attempts
    if (this.isConnectingChat) {
      return false
    }

    this.isConnectingChat = true

    // Stop any existing connection attempts
    if (this.chatConnection) {
      try {
        await this.chatConnection.stop()
      } catch (e) {
        // Ignore
      }
      this.chatConnection = null
    }

    // Create connection with hard isolation pattern
    this.chatConnection = createHubConnection(`${API_BASE_URL}/hubs/chat`, token)
    if (!this.chatConnection) {
      this.isConnectingChat = false
      return false
    }

    try {
      // ONE MORE CHECK before starting
      const preStartTokenCheck = this.getAuthToken()
      if (!preStartTokenCheck || preStartTokenCheck.trim().length === 0) {
        this.isConnectingChat = false
        if (this.chatConnection) {
          try {
            await this.chatConnection.stop()
          } catch (e) {
            // Ignore
          }
          this.chatConnection = null
        }
        return false
      }

      await this.chatConnection.start()
      this.isConnectingChat = false
      return true
    } catch (error) {
      this.isConnectingChat = false
      const errorMessage = String(error)
      if (errorMessage.includes('401') || errorMessage.includes('Unauthorized')) {
        // Hard stop on 401 - no retry
        if (this.chatConnection) {
          try {
            await this.chatConnection.stop()
          } catch (e) {
            // Ignore
          }
          this.chatConnection = null
        }
        return false
      }
      // Only log non-401 errors
      if (!errorMessage.includes('401') && !errorMessage.includes('Unauthorized')) {
        console.error('[SignalR] Error connecting to Chat Hub:', error)
      }
      return false
    }
  }

  /**
   * Connect Notification Hub - ONLY when authenticated
   * Returns true if connection started, false if blocked
   */
  async connectNotificationHub(): Promise<boolean> {
    // CRITICAL: Check token FIRST - if no token, exit immediately
    const token = this.getAuthToken()
    if (!token || token.trim().length === 0) {
      console.warn('[SignalR] Notification Hub blocked: user not authenticated')
      if (this.notificationConnection) {
        try {
          await this.notificationConnection.stop()
        } catch (e) {
          // Ignore
        }
        this.notificationConnection = null
      }
      this.isConnectingNotification = false
      return false
    }

    // If already connected, don't reconnect
    if (this.notificationConnection && this.notificationConnection.state === signalR.HubConnectionState.Connected) {
      this.isConnectingNotification = false
      return true
    }

    // Prevent multiple simultaneous connection attempts
    if (this.isConnectingNotification) {
      return false
    }

    this.isConnectingNotification = true

    // Stop any existing connection attempts
    if (this.notificationConnection) {
      try {
        await this.notificationConnection.stop()
      } catch (e) {
        // Ignore
      }
      this.notificationConnection = null
    }

    // Create connection with hard isolation pattern
    this.notificationConnection = createHubConnection(`${API_BASE_URL}/hubs/notifications`, token)
    if (!this.notificationConnection) {
      this.isConnectingNotification = false
      return false
    }

    try {
      // ONE MORE CHECK before starting
      const preStartTokenCheck = this.getAuthToken()
      if (!preStartTokenCheck || preStartTokenCheck.trim().length === 0) {
        this.isConnectingNotification = false
        if (this.notificationConnection) {
          try {
            await this.notificationConnection.stop()
          } catch (e) {
            // Ignore
          }
          this.notificationConnection = null
        }
        return false
      }

      await this.notificationConnection.start()
      this.isConnectingNotification = false
      return true
    } catch (error) {
      this.isConnectingNotification = false
      const errorMessage = String(error)
      if (errorMessage.includes('401') || errorMessage.includes('Unauthorized')) {
        // Hard stop on 401 - no retry
        if (this.notificationConnection) {
          try {
            await this.notificationConnection.stop()
          } catch (e) {
            // Ignore
          }
          this.notificationConnection = null
        }
        return false
      }
      // Only log non-401 errors
      if (!errorMessage.includes('401') && !errorMessage.includes('Unauthorized')) {
        console.error('[SignalR] Error connecting to Notification Hub:', error)
      }
      return false
    }
  }

  // Chat Hub methods
  async sendMessage(conversationId: string, content: string): Promise<void> {
    if (this.chatConnection && this.chatConnection.state === signalR.HubConnectionState.Connected) {
      const guid = conversationId.includes('-') ? conversationId : this.convertToGuid(conversationId)
      await this.chatConnection.invoke('SendMessage', guid, content)
    }
  }

  async markAsRead(messageId: string): Promise<void> {
    if (this.chatConnection && this.chatConnection.state === signalR.HubConnectionState.Connected) {
      const guid = messageId.includes('-') ? messageId : this.convertToGuid(messageId)
      await this.chatConnection.invoke('MarkAsRead', guid)
    }
  }

  async userTyping(conversationId: string): Promise<void> {
    if (this.chatConnection && this.chatConnection.state === signalR.HubConnectionState.Connected) {
      const guid = conversationId.includes('-') ? conversationId : this.convertToGuid(conversationId)
      await this.chatConnection.invoke('UserTyping', guid)
    }
  }

  // Helper to convert string to GUID format if needed
  private convertToGuid(id: string): string {
    if (id.match(/^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i)) {
      return id
    }
    return id
  }

  // Chat Hub event listeners
  onMessageReceived(callback: (message: any) => void): void {
    this.chatConnection?.on('ReceiveMessage', callback)
  }

  onUserTyping(callback: (data: any) => void): void {
    this.chatConnection?.on('UserTyping', callback)
  }

  // Notification Hub methods
  async markNotificationAsRead(notificationId: string): Promise<void> {
    if (this.notificationConnection && this.notificationConnection.state === signalR.HubConnectionState.Connected) {
      const guid = notificationId.includes('-') ? notificationId : this.convertToGuid(notificationId)
      await this.notificationConnection.invoke('MarkNotificationAsRead', guid)
    }
  }

  // Notification Hub event listeners
  onNotificationReceived(callback: (notification: any) => void): void {
    this.notificationConnection?.on('NotificationReceived', callback)
  }

  onNotificationMarkedAsRead(callback: (notificationId: string) => void): void {
    this.notificationConnection?.on('NotificationMarkedAsRead', callback)
  }

  // Disconnect all connections
  async disconnect(): Promise<void> {
    this.isConnectingChat = false
    this.isConnectingNotification = false

    if (this.chatConnection) {
      try {
        await this.chatConnection.stop()
      } catch (e) {
        // Ignore errors
      }
      this.chatConnection = null
    }
    if (this.notificationConnection) {
      try {
        await this.notificationConnection.stop()
      } catch (e) {
        // Ignore errors
      }
      this.notificationConnection = null
    }
  }

  // Get connection state
  getChatConnectionState(): signalR.HubConnectionState | null {
    return this.chatConnection?.state || null
  }

  getNotificationConnectionState(): signalR.HubConnectionState | null {
    return this.notificationConnection?.state || null
  }
}

// Export singleton instance - but connections are created lazily and explicitly
export const signalRService = new SignalRService()
