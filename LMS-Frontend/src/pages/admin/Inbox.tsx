import { useState, useEffect, useRef } from 'react'
import { Send, Search, Paperclip, Smile, Loader2, AlertCircle } from 'lucide-react'
import { Avatar } from '../../components/ui/Avatar'
import { Badge } from '../../components/ui/Badge'
import Input from '../../components/ui/Input'
import { signalRService } from '../../services/signalr'
import { getConversations, getMessages, ConversationDto, MessageDto } from '../../services/messagesApi'
import { getCurrentUser } from '../../utils/auth'
import { useSignalR } from '../../contexts/SignalRContext'

interface Message {
  id: string
  content: string
  senderId: string
  senderName: string
  senderRole?: string
  timestamp: Date
  isRead: boolean
}

interface Conversation {
  id: string
  participantId: string
  participantName: string
  participantRole?: string
  participantAvatar?: string
  lastMessage: string
  lastMessageTime: Date
  unreadCount: number
}

const AdminInbox = () => {
  const { isConnected } = useSignalR()
  const currentUser = getCurrentUser()
  const currentUserId = currentUser?.id || ''
  
  const [conversations, setConversations] = useState<Conversation[]>([])
  const [selectedConversation, setSelectedConversation] = useState<string | null>(null)
  const [messages, setMessages] = useState<Message[]>([])
  const [messageInput, setMessageInput] = useState('')
  const [isTyping, setIsTyping] = useState(false)
  const [isLoading, setIsLoading] = useState(true)
  const [isLoadingMessages, setIsLoadingMessages] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [searchQuery, setSearchQuery] = useState('')
  const messagesEndRef = useRef<HTMLDivElement>(null)
  const typingTimeoutRef = useRef<ReturnType<typeof setTimeout> | null>(null)

  // Load conversations on mount
  useEffect(() => {
    const loadConversations = async () => {
      try {
        setIsLoading(true)
        setError(null)
        const response = await getConversations({ page: 1, pageSize: 50 })
        
        // Transform API response to component format
        const transformedConversations: Conversation[] = response.items.map((conv: ConversationDto) => {
          // Determine which user is the other participant
          const otherUserId = conv.user1Id === currentUserId ? conv.user2Id : conv.user1Id
          const otherUserName = conv.user1Id === currentUserId ? conv.user2Name : conv.user1Name
          const otherUserAvatar = conv.user1Id === currentUserId ? conv.user2Avatar : conv.user1Avatar
          
          return {
            id: conv.id,
            participantId: otherUserId,
            participantName: otherUserName || 'Unknown User',
            participantAvatar: otherUserAvatar,
            participantRole: 'User', // Could be enhanced to get from user profile
            lastMessage: conv.lastMessage || '',
            lastMessageTime: conv.lastMessageTime ? new Date(conv.lastMessageTime) : new Date(conv.createdAt),
            unreadCount: conv.unreadCount || 0,
          }
        })
        
        setConversations(transformedConversations)
        
        // Auto-select first conversation if available
        if (transformedConversations.length > 0 && !selectedConversation) {
          setSelectedConversation(transformedConversations[0].id)
        }
      } catch (err: any) {
        console.error('Failed to load conversations:', err)
        setError(err.message || 'Failed to load conversations')
      } finally {
        setIsLoading(false)
      }
    }
    
    loadConversations()
  }, [currentUserId])

  // Initialize SignalR connection when inbox is opened - EXPLICIT AUTH CHECK
  useEffect(() => {
    // HARD CHECK: Must have token before ANY connection attempt
    const token = localStorage.getItem('token')
    if (!token || token.trim().length === 0) {
      // NO TOKEN = NO CONNECTION (hard stop)
      return
    }

    let isMounted = true
    let connectionTimeout: ReturnType<typeof setTimeout> | null = null

    const initializeSignalR = async () => {
      // Double-check token before connecting
      const currentToken = localStorage.getItem('token')
      if (!currentToken || currentToken.trim().length === 0) {
        return
      }

      if (!isMounted) return

      try {
        // Connect SignalR hubs when inbox is opened - explicit calls
        await signalRService.connectChatHub()
        await signalRService.connectNotificationHub()
      } catch (error) {
        // Silently fail - don't spam console
        const errorMsg = String(error)
        if (!errorMsg.includes('401') && !errorMsg.includes('Unauthorized')) {
          console.error('[SignalR] Failed to connect:', error)
        }
      }
    }

    // Delay connection slightly to avoid immediate retries
    connectionTimeout = setTimeout(initializeSignalR, 500)

    // Cleanup: disconnect when leaving inbox
    return () => {
      isMounted = false
      if (connectionTimeout) {
        clearTimeout(connectionTimeout)
      }
      // Don't disconnect on unmount - keep connection alive during navigation
    }
  }, []) // Only run once when inbox is opened

  // Set up SignalR message listeners
  useEffect(() => {
    if (!isConnected) return

    // Set up message listeners (backend sends 'ReceiveMessage' event)
    const handleMessageReceived = (message: any) => {
      // Backend sends: { id, conversationId, senderId, content, createdAt, senderName }
      const conversationIdStr = message.conversationId?.toString() || message.conversationId
      if (conversationIdStr === selectedConversation) {
        // Check if message already exists (avoid duplicates)
        setMessages((prev) => {
          if (prev.some(m => m.id === message.id)) {
            return prev
          }
          return [
            ...prev,
            {
              id: message.id,
              content: message.content,
              senderId: message.senderId,
              senderName: message.senderId === currentUserId ? 'You' : (message.senderName || 'User'),
              senderRole: message.senderRole,
              timestamp: new Date(message.createdAt || message.timestamp),
              isRead: false,
            },
          ]
        })
        
        // Update conversation's last message
        setConversations((prev) =>
          prev.map((conv) =>
            conv.id === conversationIdStr
              ? {
                  ...conv,
                  lastMessage: message.content,
                  lastMessageTime: new Date(message.createdAt || message.timestamp),
                  unreadCount: message.senderId === currentUserId ? conv.unreadCount : conv.unreadCount + 1,
                }
              : conv
          )
        )
      } else {
        // Message for another conversation - update unread count
        setConversations((prev) =>
          prev.map((conv) =>
            conv.id === conversationIdStr
              ? { ...conv, unreadCount: conv.unreadCount + 1 }
              : conv
          )
        )
      }
    }

    const handleUserTyping = (data: any) => {
      const conversationIdStr = data.conversationId?.toString() || data.conversationId
      if (conversationIdStr === selectedConversation && data.userId !== currentUserId) {
        setIsTyping(true)
        if (typingTimeoutRef.current) {
          clearTimeout(typingTimeoutRef.current)
        }
        typingTimeoutRef.current = setTimeout(() => setIsTyping(false), 3000)
      }
    }

    signalRService.onMessageReceived(handleMessageReceived)
    signalRService.onUserTyping(handleUserTyping)

    return () => {
      // Cleanup handled by SignalR service
    }
  }, [isConnected, selectedConversation, currentUserId])

  // Load messages when conversation is selected
  useEffect(() => {
    if (!selectedConversation) {
      setMessages([])
      return
    }

    const loadMessages = async () => {
      try {
        setIsLoadingMessages(true)
        setError(null)
        const response = await getMessages(selectedConversation, { page: 1, pageSize: 100 })
        
        // Transform API response to component format
        const transformedMessages: Message[] = response.items.map((msg: MessageDto) => ({
          id: msg.id,
          content: msg.content,
          senderId: msg.senderId,
          senderName: msg.senderId === currentUserId ? 'You' : (msg.senderName || 'User'),
          senderRole: 'User', // Could be enhanced to get from user profile
          timestamp: new Date(msg.createdAt),
          isRead: msg.isRead,
        }))
        
        // Sort by timestamp (oldest first)
        transformedMessages.sort((a, b) => a.timestamp.getTime() - b.timestamp.getTime())
        
        setMessages(transformedMessages)
        
        // Mark messages as read
        const unreadMessages = transformedMessages.filter(m => !m.isRead && m.senderId !== currentUserId)
        for (const msg of unreadMessages) {
          try {
            await signalRService.markAsRead(msg.id)
          } catch (err) {
            console.error('Failed to mark message as read:', err)
          }
        }
        
        // Update conversation unread count
        setConversations((prev) =>
          prev.map((conv) =>
            conv.id === selectedConversation ? { ...conv, unreadCount: 0 } : conv
          )
        )
      } catch (err: any) {
        console.error('Failed to load messages:', err)
        setError(err.message || 'Failed to load messages')
      } finally {
        setIsLoadingMessages(false)
      }
    }
    
    loadMessages()
  }, [selectedConversation, currentUserId])

  useEffect(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' })
  }, [messages])

  const handleSendMessage = async () => {
    if (!messageInput.trim() || !selectedConversation || !isConnected) return

    const messageContent = messageInput.trim()
    setMessageInput('')

    // Optimistically add message to UI
    const tempId = `temp-${Date.now()}`
    const newMessage: Message = {
      id: tempId,
      content: messageContent,
      senderId: currentUserId,
      senderName: 'You',
      senderRole: 'Admin',
      timestamp: new Date(),
      isRead: false,
    }

    setMessages((prev) => [...prev, newMessage])

    try {
      // Send via SignalR (backend will save and broadcast)
      await signalRService.sendMessage(selectedConversation, messageContent)
      
      // Update conversation's last message
      setConversations((prev) =>
        prev.map((conv) =>
          conv.id === selectedConversation
            ? {
                ...conv,
                lastMessage: messageContent,
                lastMessageTime: new Date(),
              }
            : conv
        )
      )
    } catch (err: any) {
      console.error('Failed to send message:', err)
      setError(err.message || 'Failed to send message')
      // Remove optimistic message on error
      setMessages((prev) => prev.filter((m) => m.id !== tempId))
      setMessageInput(messageContent) // Restore input
    }
  }

  const handleTyping = () => {
    if (selectedConversation) {
      signalRService.userTyping(selectedConversation)
    }
  }

  const selectedConv = conversations.find((c) => c.id === selectedConversation)
  
  // Filter conversations based on search
  const filteredConversations = conversations.filter((conv) =>
    conv.participantName.toLowerCase().includes(searchQuery.toLowerCase()) ||
    conv.lastMessage.toLowerCase().includes(searchQuery.toLowerCase())
  )

  return (
    <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <div className="mb-8">
        <div className="flex items-center justify-between">
          <div>
            <h1 className="text-3xl font-bold text-gray-900 mb-2">Admin Inbox</h1>
            <p className="text-gray-600">Chat with students and tutors</p>
          </div>
          {!isConnected && (
            <div className="flex items-center gap-2 text-yellow-600">
              <AlertCircle className="w-5 h-5" />
              <span className="text-sm">Connecting...</span>
            </div>
          )}
        </div>
      </div>

      {error && (
        <div className="mb-4 p-4 bg-red-50 border border-red-200 rounded-lg text-red-700">
          {error}
        </div>
      )}

      <div className="grid lg:grid-cols-3 gap-6 h-[calc(100vh-200px)]">
        {/* Conversations List */}
        <div className="lg:col-span-1 border border-gray-200 rounded-lg overflow-hidden bg-white">
          <div className="p-4 border-b border-gray-200">
            <div className="relative">
              <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 w-5 h-5" />
              <Input
                placeholder="Search conversations..."
                className="pl-10"
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
              />
            </div>
          </div>
          <div className="overflow-y-auto h-[calc(100%-80px)]">
            {isLoading ? (
              <div className="flex items-center justify-center p-8">
                <Loader2 className="w-6 h-6 animate-spin text-primary-600" />
              </div>
            ) : filteredConversations.length === 0 ? (
              <div className="p-8 text-center text-gray-500">
                <p>No conversations found</p>
              </div>
            ) : (
              filteredConversations.map((conv) => (
                <div
                  key={conv.id}
                  onClick={() => setSelectedConversation(conv.id)}
                  className={`p-4 border-b border-gray-100 cursor-pointer hover:bg-gray-50 transition-colors ${
                    selectedConversation === conv.id ? 'bg-primary-50 border-l-4 border-l-primary-500' : ''
                  }`}
                >
                  <div className="flex items-start gap-3">
                    <Avatar name={conv.participantName} size="md" />
                    <div className="flex-1 min-w-0">
                      <div className="flex items-center justify-between mb-1">
                        <h3 className="font-semibold text-gray-900 truncate">{conv.participantName}</h3>
                        {conv.unreadCount > 0 && (
                          <Badge variant="info">{conv.unreadCount}</Badge>
                        )}
                      </div>
                      {conv.participantRole && (
                        <div className="flex items-center gap-2 mb-1">
                          <Badge variant="default" className="text-xs">{conv.participantRole}</Badge>
                        </div>
                      )}
                      <p className="text-sm text-gray-600 truncate">{conv.lastMessage}</p>
                      <p className="text-xs text-gray-500 mt-1">
                        {conv.lastMessageTime.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}
                      </p>
                    </div>
                  </div>
                </div>
              ))
            )}
          </div>
        </div>

        {/* Chat Area */}
        <div className="lg:col-span-2 flex flex-col border border-gray-200 rounded-lg bg-white">
          {selectedConversation ? (
            <>
              <div className="p-4 border-b border-gray-200 flex items-center justify-between">
                <div className="flex items-center gap-3">
                  <Avatar name={selectedConv?.participantName || ''} size="md" />
                  <div>
                    <h3 className="font-semibold text-gray-900">{selectedConv?.participantName}</h3>
                    {selectedConv?.participantRole && (
                      <p className="text-sm text-gray-500">{selectedConv.participantRole}</p>
                    )}
                    {isTyping && (
                      <p className="text-sm text-gray-500 italic">typing...</p>
                    )}
                  </div>
                </div>
              </div>

              <div className="flex-1 overflow-y-auto p-4 space-y-4">
                {isLoadingMessages ? (
                  <div className="flex items-center justify-center p-8">
                    <Loader2 className="w-6 h-6 animate-spin text-primary-600" />
                  </div>
                ) : messages.length === 0 ? (
                  <div className="flex items-center justify-center p-8 text-gray-500">
                    <p>No messages yet. Start the conversation!</p>
                  </div>
                ) : (
                  messages.map((message) => (
                    <div
                      key={message.id}
                      className={`flex ${message.senderId === currentUserId ? 'justify-end' : 'justify-start'}`}
                    >
                      <div
                        className={`max-w-[70%] rounded-lg px-4 py-2 ${
                          message.senderId === currentUserId
                            ? 'bg-primary-500 text-white'
                            : 'bg-gray-100 text-gray-900'
                        }`}
                      >
                        <p className="text-sm">{message.content}</p>
                        <p
                          className={`text-xs mt-1 ${
                            message.senderId === currentUserId ? 'text-primary-100' : 'text-gray-500'
                          }`}
                        >
                          {message.timestamp.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}
                        </p>
                      </div>
                    </div>
                  ))
                )}
                <div ref={messagesEndRef} />
              </div>

              <div className="p-4 border-t border-gray-200">
                <div className="flex items-center gap-2">
                  <button className="p-2 hover:bg-gray-100 rounded-lg transition-colors">
                    <Paperclip className="w-5 h-5 text-gray-500" />
                  </button>
                  <input
                    type="text"
                    value={messageInput}
                    onChange={(e) => {
                      setMessageInput(e.target.value)
                      handleTyping()
                    }}
                    onKeyPress={(e) => {
                      if (e.key === 'Enter' && !e.shiftKey) {
                        e.preventDefault()
                        handleSendMessage()
                      }
                    }}
                    placeholder={isConnected ? "Type a message..." : "Connecting..."}
                    disabled={!isConnected}
                    className="flex-1 px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500 disabled:bg-gray-100 disabled:cursor-not-allowed"
                  />
                  <button className="p-2 hover:bg-gray-100 rounded-lg transition-colors">
                    <Smile className="w-5 h-5 text-gray-500" />
                  </button>
                  <button
                    onClick={handleSendMessage}
                    disabled={!isConnected || !messageInput.trim()}
                    className="p-2 bg-primary-500 text-white rounded-lg hover:bg-primary-600 transition-colors disabled:bg-gray-300 disabled:cursor-not-allowed"
                  >
                    <Send className="w-5 h-5" />
                  </button>
                </div>
              </div>
            </>
          ) : (
            <div className="flex-1 flex items-center justify-center text-gray-500">
              <p>Select a conversation to start chatting</p>
            </div>
          )}
        </div>
      </div>
    </div>
  )
}

export default AdminInbox
