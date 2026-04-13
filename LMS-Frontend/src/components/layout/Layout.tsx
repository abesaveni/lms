import { ReactNode, useEffect } from 'react'
import { useLocation } from 'react-router-dom'
import Header from './Header'
import Footer from './Footer'
import CookieBanner from '../consent/CookieBanner'

interface LayoutProps {
  children: ReactNode
  showFooter?: boolean
}

const Layout = ({ children, showFooter = true }: LayoutProps) => {
  const location = useLocation()

  useEffect(() => {
    // Scroll to top when route changes
    window.scrollTo({ top: 0, left: 0, behavior: 'instant' })
  }, [location.pathname])

  return (
    <div className="min-h-screen flex flex-col">
      <Header />
      <main className="flex-1">{children}</main>
      {showFooter && <Footer />}
      <CookieBanner />
    </div>
  )
}

export default Layout
