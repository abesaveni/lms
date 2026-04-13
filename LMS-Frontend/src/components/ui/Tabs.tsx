import { ReactNode, createContext, useContext, useState } from 'react'
import { clsx } from 'clsx'

interface TabsContextType {
  activeTab: string
  setActiveTab: (tab: string) => void
}

const TabsContext = createContext<TabsContextType | undefined>(undefined)

interface TabsProps {
  children: ReactNode
  defaultValue: string
  value?: string
  onValueChange?: (value: string) => void
}

export const Tabs = ({ children, defaultValue, value, onValueChange }: TabsProps) => {
  const [internalActiveTab, setInternalActiveTab] = useState(defaultValue)
  
  const activeTab = value !== undefined ? value : internalActiveTab
  const setActiveTab = (tab: string) => {
    if (value === undefined) {
      setInternalActiveTab(tab)
    }
    onValueChange?.(tab)
  }

  return (
    <TabsContext.Provider value={{ activeTab, setActiveTab }}>
      <div className="w-full">{children}</div>
    </TabsContext.Provider>
  )
}

export const TabsList = ({ children, className }: { children: ReactNode; className?: string }) => {
  return (
    <div className={clsx('flex border-b border-gray-200 mb-4', className)}>
      {children}
    </div>
  )
}

export const TabsTrigger = ({ value, children, onClick }: { value: string; children: ReactNode; onClick?: () => void }) => {
  const context = useContext(TabsContext)
  if (!context) throw new Error('TabsTrigger must be used within Tabs')

  const { activeTab, setActiveTab } = context
  const isActive = activeTab === value

  return (
    <button
      onClick={() => {
        setActiveTab(value)
        onClick?.()
      }}
      className={clsx(
        'px-4 py-2 text-sm font-medium transition-colors border-b-2',
        isActive
          ? 'border-primary-500 text-primary-600'
          : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
      )}
    >
      {children}
    </button>
  )
}

export const TabsContent = ({ value, children }: { value: string; children: ReactNode }) => {
  const context = useContext(TabsContext)
  if (!context) throw new Error('TabsContent must be used within Tabs')

  const { activeTab } = context
  if (activeTab !== value) return null

  return <div>{children}</div>
}
