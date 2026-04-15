import { useEffect } from 'react'
import { BrowserRouter as Router, Routes, Route } from 'react-router-dom'
import Layout from './components/layout/Layout'
import DashboardLayout from './components/layout/DashboardLayout'
import ProtectedRoute from './components/auth/ProtectedRoute'
import { SignalRProvider } from './contexts/SignalRContext'
import { isCurrentTokenExpired, logout } from './utils/auth'
import Landing from './pages/Landing'
import Login from './pages/auth/Login'
import ForgotPassword from './pages/auth/ForgotPassword'
import ResetPassword from './pages/auth/ResetPassword'
import Register from './pages/auth/Register'
import Sessions from './pages/Sessions'
import AboutUs from './pages/AboutUs'
import JoinUs from './pages/JoinUs'
import StudentDashboard from './pages/student/Dashboard'
import FindTutors from './pages/FindTutors'
import StudentFindTutors from './pages/student/FindTutors'
import TutorProfile from './pages/student/TutorProfile'
import MySessions from './pages/student/MySessions'
import StudentProfile from './pages/student/Profile'
import StudentProfileSettings from './pages/student/ProfileSettings'
import StudentInbox from './pages/student/Inbox'
import StudentWalletBonuses from './pages/student/WalletBonuses'
import StudentReferralProgram from './pages/student/ReferralProgram'
import TutorDashboard from './pages/tutor/Dashboard'
import TutorSessions from './pages/tutor/Sessions'
import TutorProfilePage from './pages/tutor/Profile'
import TutorProfileSettings from './pages/tutor/ProfileSettings'
import TutorInbox from './pages/tutor/Inbox'
import AdminDashboard from './pages/admin/Dashboard'
import UserManagement from './pages/admin/UserManagement'
import Financials from './pages/admin/Financials'
import ApiSettings from './pages/admin/APISettings'
import Settings from './pages/admin/Settings'
import AdminInbox from './pages/admin/Inbox'
import TutorEarningsEnhanced from './pages/tutor/EarningsEnhanced'
import TutorOnboarding from './pages/tutor/Onboarding'
import BookSession from './pages/student/BookSession'
import FinancialDashboard from './pages/admin/FinancialDashboard'
import AdminManagement from './pages/admin/AdminManagement'
import WhatsAppCampaigns from './pages/admin/WhatsAppCampaigns'
import TutorVerification from './pages/admin/TutorVerification'
import PayoutManagement from './pages/admin/PayoutManagement'
import ConsentManagement from './pages/admin/ConsentManagement'
import BlogManagement from './pages/admin/BlogManagement'
import SubjectManagement from './pages/admin/SubjectManagement'
import CreateSession from './pages/tutor/CreateSession'
import JoinSession from './pages/session/JoinSession'
import VerificationPending from './pages/tutor/VerificationPending'
import ConnectCalendar from './pages/calendar/ConnectCalendar'
import Blogs from './pages/Blogs'
import TermsAndConditions from './pages/TermsAndConditions'
import PrivacyPolicy from './pages/PrivacyPolicy'
import Notifications from './pages/Notifications'
import ScrollToTop from './components/ScrollToTop'
import ResumeBuilder from './pages/student/ResumeBuilder'
import LexiChatbot from './components/chatbot/LexiChatbot'
import { PaywallModal } from './components/subscription/PaywallModal'
import { TrialBanner } from './components/subscription/TrialBanner'
import AITools from './pages/student/AITools'
import MockInterview from './pages/student/MockInterview'
import CareerPath from './pages/student/CareerPath'
import LinkedInOptimizer from './pages/student/LinkedinOptimizer'
import ProjectIdeas from './pages/student/ProjectIdeas'
import CodeReview from './pages/student/CodeReview'
import PortfolioGenerator from './pages/student/PortfolioGenerator'
import DailyQuiz from './pages/student/DailyQuiz'
import Flashcards from './pages/student/Flashcards'
import AssignmentHelper from './pages/student/AssignmentHelper'
import StudySchedule from './pages/student/StudySchedule'
import WellnessCheckin from './pages/student/WellnessCheckin'
import Subscription from './pages/student/Subscription'
import BrowseCourses from './pages/student/BrowseCourses'
import CourseDetailPage from './pages/student/CourseDetail'
import MyEnrollments from './pages/student/MyEnrollments'
import BillingHistory from './pages/student/BillingHistory'
import DailyGames from './pages/student/DailyGames'
import TutorMyCourses from './pages/tutor/MyCourses'
import CreateEditCourse from './pages/tutor/CreateEditCourse'
import CourseEnrollments from './pages/tutor/CourseEnrollments'
import SubjectRates from './pages/tutor/SubjectRates'
import AdminAITools from './pages/admin/AITools'
import CouponManagement from './pages/admin/CouponManagement'

function App() {
  // Check token expiration periodically and on mount
  useEffect(() => {
    const checkTokenExpiration = async () => {
      if (isCurrentTokenExpired()) {
        const token = localStorage.getItem('token')
        // Only logout if token exists (means it's expired, not missing)
        if (token) {
          console.warn('Token expired. Logging out...')
          await logout()
        }
      }
    }

    // Check immediately
    checkTokenExpiration()

    // Check every 5 minutes
    const interval = setInterval(checkTokenExpiration, 5 * 60 * 1000)

    return () => clearInterval(interval)
  }, [])

  return (
    <SignalRProvider>
      <Router>
        <ScrollToTop />
        {/* Global: paywall modal listens for 402 events from anywhere */}
        <PaywallModal />
        {/* Lexi chatbot: always mounted for student pages */}
        <LexiChatbot />
        <Routes>
          <Route path="/" element={<Layout><Landing /></Layout>} />
          <Route path="/login" element={<Login />} />
          <Route path="/forgot-password" element={<ForgotPassword />} />
          <Route path="/reset-password" element={<ResetPassword />} />
          <Route path="/register" element={<Register />} />

          {/* Global Routes (No login required) */}
          <Route path="/find-tutors" element={<Layout><FindTutors /></Layout>} />
          <Route path="/join-us" element={<JoinUs />} />
          <Route path="/sessions" element={<Layout><Sessions /></Layout>} />
          <Route path="/blogs" element={<Layout><Blogs /></Layout>} />
          <Route path="/about-us" element={<Layout><AboutUs /></Layout>} />
          <Route path="/terms-and-conditions" element={<Layout><TermsAndConditions /></Layout>} />
          <Route path="/privacy-policy" element={<Layout><PrivacyPolicy /></Layout>} />

          {/* Student Routes */}
          <Route path="/student/dashboard" element={<ProtectedRoute requiredRole="student"><DashboardLayout role="student"><StudentDashboard /></DashboardLayout></ProtectedRoute>} />
          <Route path="/student/find-tutors" element={<ProtectedRoute requiredRole="student"><DashboardLayout role="student"><StudentFindTutors /></DashboardLayout></ProtectedRoute>} />
          <Route path="/student/tutors/:id" element={<ProtectedRoute requiredRole="student"><DashboardLayout role="student"><TutorProfile /></DashboardLayout></ProtectedRoute>} />
          <Route path="/student/my-sessions" element={<ProtectedRoute requiredRole="student"><DashboardLayout role="student"><MySessions /></DashboardLayout></ProtectedRoute>} />
          <Route path="/student/profile" element={<ProtectedRoute requiredRole="student"><DashboardLayout role="student"><StudentProfile /></DashboardLayout></ProtectedRoute>} />
          <Route path="/student/profile-settings" element={<ProtectedRoute requiredRole="student"><DashboardLayout role="student"><StudentProfileSettings /></DashboardLayout></ProtectedRoute>} />
          <Route path="/student/inbox" element={<ProtectedRoute requiredRole="student"><DashboardLayout role="student"><StudentInbox /></DashboardLayout></ProtectedRoute>} />
          <Route path="/student/wallet" element={<ProtectedRoute requiredRole="student"><DashboardLayout role="student"><StudentWalletBonuses /></DashboardLayout></ProtectedRoute>} />
          <Route path="/student/referrals" element={<ProtectedRoute requiredRole="student"><DashboardLayout role="student"><StudentReferralProgram /></DashboardLayout></ProtectedRoute>} />
          <Route path="/student/book-session" element={<ProtectedRoute requiredRole="student"><DashboardLayout role="student"><BookSession /></DashboardLayout></ProtectedRoute>} />
          <Route path="/student/book-session/:sessionId" element={<ProtectedRoute requiredRole="student"><DashboardLayout role="student"><BookSession /></DashboardLayout></ProtectedRoute>} />
          <Route path="/sessions/:sessionId/book" element={<ProtectedRoute requiredRole="student"><DashboardLayout role="student"><BookSession /></DashboardLayout></ProtectedRoute>} />
          <Route path="/student/notifications" element={<ProtectedRoute requiredRole="student"><DashboardLayout role="student"><Notifications /></DashboardLayout></ProtectedRoute>} />
          <Route path="/student/resume-builder" element={<ProtectedRoute requiredRole="student"><DashboardLayout role="student"><TrialBanner /><ResumeBuilder /></DashboardLayout></ProtectedRoute>} />
          <Route path="/student/ai-assistant" element={<ProtectedRoute requiredRole="student"><DashboardLayout role="student"><TrialBanner /><div className="p-6 text-center text-gray-500 text-sm">Use the Lexi chat bubble in the bottom-right corner.</div></DashboardLayout></ProtectedRoute>} />
          <Route path="/student/ai-tools" element={<ProtectedRoute requiredRole="student"><DashboardLayout role="student"><TrialBanner /><AITools /></DashboardLayout></ProtectedRoute>} />
          <Route path="/student/mock-interview" element={<ProtectedRoute requiredRole="student"><DashboardLayout role="student"><TrialBanner /><MockInterview /></DashboardLayout></ProtectedRoute>} />
          <Route path="/student/career-path" element={<ProtectedRoute requiredRole="student"><DashboardLayout role="student"><TrialBanner /><CareerPath /></DashboardLayout></ProtectedRoute>} />
          <Route path="/student/linkedin-optimizer" element={<ProtectedRoute requiredRole="student"><DashboardLayout role="student"><TrialBanner /><LinkedInOptimizer /></DashboardLayout></ProtectedRoute>} />
          <Route path="/student/project-ideas" element={<ProtectedRoute requiredRole="student"><DashboardLayout role="student"><TrialBanner /><ProjectIdeas /></DashboardLayout></ProtectedRoute>} />
          <Route path="/student/code-review" element={<ProtectedRoute requiredRole="student"><DashboardLayout role="student"><TrialBanner /><CodeReview /></DashboardLayout></ProtectedRoute>} />
          <Route path="/student/portfolio-generator" element={<ProtectedRoute requiredRole="student"><DashboardLayout role="student"><TrialBanner /><PortfolioGenerator /></DashboardLayout></ProtectedRoute>} />
          <Route path="/student/daily-quiz" element={<ProtectedRoute requiredRole="student"><DashboardLayout role="student"><TrialBanner /><DailyQuiz /></DashboardLayout></ProtectedRoute>} />
          <Route path="/student/flashcards" element={<ProtectedRoute requiredRole="student"><DashboardLayout role="student"><TrialBanner /><Flashcards /></DashboardLayout></ProtectedRoute>} />
          <Route path="/student/assignment-helper" element={<ProtectedRoute requiredRole="student"><DashboardLayout role="student"><TrialBanner /><AssignmentHelper /></DashboardLayout></ProtectedRoute>} />
          <Route path="/student/study-schedule" element={<ProtectedRoute requiredRole="student"><DashboardLayout role="student"><TrialBanner /><StudySchedule /></DashboardLayout></ProtectedRoute>} />
          <Route path="/student/wellness" element={<ProtectedRoute requiredRole="student"><DashboardLayout role="student"><TrialBanner /><WellnessCheckin /></DashboardLayout></ProtectedRoute>} />
          <Route path="/student/subscription" element={<ProtectedRoute requiredRole="student"><DashboardLayout role="student"><Subscription /></DashboardLayout></ProtectedRoute>} />
          <Route path="/student/courses" element={<ProtectedRoute requiredRole="student"><DashboardLayout role="student"><BrowseCourses /></DashboardLayout></ProtectedRoute>} />
          <Route path="/student/courses/:id" element={<ProtectedRoute requiredRole="student"><DashboardLayout role="student"><CourseDetailPage /></DashboardLayout></ProtectedRoute>} />
          <Route path="/student/my-enrollments" element={<ProtectedRoute requiredRole="student"><DashboardLayout role="student"><MyEnrollments /></DashboardLayout></ProtectedRoute>} />
          <Route path="/student/billing" element={<ProtectedRoute requiredRole="student"><DashboardLayout role="student"><BillingHistory /></DashboardLayout></ProtectedRoute>} />
          <Route path="/student/daily-games" element={<ProtectedRoute requiredRole="student"><DashboardLayout role="student"><DailyGames /></DashboardLayout></ProtectedRoute>} />

          {/* Tutor Routes */}
          <Route path="/tutor/dashboard" element={<ProtectedRoute requiredRole="tutor"><DashboardLayout role="tutor"><TutorDashboard /></DashboardLayout></ProtectedRoute>} />
          <Route path="/tutor/verification-pending" element={<ProtectedRoute requiredRole="tutor"><DashboardLayout role="tutor"><VerificationPending /></DashboardLayout></ProtectedRoute>} />
          <Route path="/tutor/sessions" element={<ProtectedRoute requiredRole="tutor"><DashboardLayout role="tutor"><TutorSessions /></DashboardLayout></ProtectedRoute>} />
          <Route path="/tutor/sessions/create" element={<ProtectedRoute requiredRole="tutor"><DashboardLayout role="tutor"><CreateSession /></DashboardLayout></ProtectedRoute>} />
          <Route path="/tutor/profile" element={<ProtectedRoute requiredRole="tutor"><DashboardLayout role="tutor"><TutorProfilePage /></DashboardLayout></ProtectedRoute>} />
          <Route path="/tutor/profile-settings" element={<ProtectedRoute requiredRole="tutor"><DashboardLayout role="tutor"><TutorProfileSettings /></DashboardLayout></ProtectedRoute>} />
          <Route path="/tutor/inbox" element={<ProtectedRoute requiredRole="tutor"><DashboardLayout role="tutor"><TutorInbox /></DashboardLayout></ProtectedRoute>} />
          <Route path="/tutor/earnings" element={<ProtectedRoute requiredRole="tutor"><DashboardLayout role="tutor"><TutorEarningsEnhanced /></DashboardLayout></ProtectedRoute>} />
          <Route path="/tutor/onboarding" element={<ProtectedRoute requiredRole="tutor"><DashboardLayout role="tutor"><TutorOnboarding /></DashboardLayout></ProtectedRoute>} />
          <Route path="/tutor/notifications" element={<ProtectedRoute requiredRole="tutor"><DashboardLayout role="tutor"><Notifications /></DashboardLayout></ProtectedRoute>} />
          <Route path="/tutor/courses" element={<ProtectedRoute requiredRole="tutor"><DashboardLayout role="tutor"><TutorMyCourses /></DashboardLayout></ProtectedRoute>} />
          <Route path="/tutor/courses/create" element={<ProtectedRoute requiredRole="tutor"><DashboardLayout role="tutor"><CreateEditCourse /></DashboardLayout></ProtectedRoute>} />
          <Route path="/tutor/courses/edit/:id" element={<ProtectedRoute requiredRole="tutor"><DashboardLayout role="tutor"><CreateEditCourse /></DashboardLayout></ProtectedRoute>} />
          <Route path="/tutor/courses/:courseId/enrollments" element={<ProtectedRoute requiredRole="tutor"><DashboardLayout role="tutor"><CourseEnrollments /></DashboardLayout></ProtectedRoute>} />
          <Route path="/tutor/subject-rates" element={<ProtectedRoute requiredRole="tutor"><DashboardLayout role="tutor"><SubjectRates /></DashboardLayout></ProtectedRoute>} />

          {/* Calendar Routes */}
          <Route path="/calendar/connect" element={<Layout><ConnectCalendar /></Layout>} />

          {/* Session Routes */}
          <Route path="/session/:sessionId/join" element={<Layout><JoinSession /></Layout>} />

          {/* Admin Routes */}
          <Route path="/admin/dashboard" element={<ProtectedRoute requiredRole="admin"><DashboardLayout role="admin"><AdminDashboard /></DashboardLayout></ProtectedRoute>} />
          <Route path="/admin/users" element={<ProtectedRoute requiredRole="admin"><DashboardLayout role="admin"><UserManagement /></DashboardLayout></ProtectedRoute>} />
          <Route path="/admin/financials" element={<ProtectedRoute requiredRole="admin"><DashboardLayout role="admin"><Financials /></DashboardLayout></ProtectedRoute>} />
          <Route path="/admin/financial-dashboard" element={<ProtectedRoute requiredRole="admin"><DashboardLayout role="admin"><FinancialDashboard /></DashboardLayout></ProtectedRoute>} />
          <Route path="/admin/admin-management" element={<ProtectedRoute requiredRole="admin"><DashboardLayout role="admin"><AdminManagement /></DashboardLayout></ProtectedRoute>} />
          <Route path="/admin/tutor-verification" element={<ProtectedRoute requiredRole="admin"><DashboardLayout role="admin"><TutorVerification /></DashboardLayout></ProtectedRoute>} />
          <Route path="/admin/payouts" element={<ProtectedRoute requiredRole="admin"><DashboardLayout role="admin"><PayoutManagement /></DashboardLayout></ProtectedRoute>} />
          <Route path="/admin/whatsapp-campaigns" element={<ProtectedRoute requiredRole="admin"><DashboardLayout role="admin"><WhatsAppCampaigns /></DashboardLayout></ProtectedRoute>} />
          <Route path="/admin/api-settings" element={<ProtectedRoute requiredRole="admin"><DashboardLayout role="admin"><ApiSettings /></DashboardLayout></ProtectedRoute>} />
          <Route path="/admin/settings" element={<ProtectedRoute requiredRole="admin"><DashboardLayout role="admin"><Settings /></DashboardLayout></ProtectedRoute>} />
          <Route path="/admin/consents" element={<ProtectedRoute requiredRole="admin"><DashboardLayout role="admin"><ConsentManagement /></DashboardLayout></ProtectedRoute>} />
          <Route path="/admin/blogs" element={<ProtectedRoute requiredRole="admin"><DashboardLayout role="admin"><BlogManagement /></DashboardLayout></ProtectedRoute>} />
          <Route path="/admin/subjects" element={<ProtectedRoute requiredRole="admin"><DashboardLayout role="admin"><SubjectManagement /></DashboardLayout></ProtectedRoute>} />
          <Route path="/admin/inbox" element={<ProtectedRoute requiredRole="admin"><DashboardLayout role="admin"><AdminInbox /></DashboardLayout></ProtectedRoute>} />
          <Route path="/admin/ai-tools" element={<ProtectedRoute requiredRole="admin"><DashboardLayout role="admin"><AdminAITools /></DashboardLayout></ProtectedRoute>} />
          <Route path="/admin/coupons" element={<ProtectedRoute requiredRole="admin"><DashboardLayout role="admin"><CouponManagement /></DashboardLayout></ProtectedRoute>} />
          <Route path="/admin/notifications" element={<ProtectedRoute requiredRole="admin"><DashboardLayout role="admin"><Notifications /></DashboardLayout></ProtectedRoute>} />
        </Routes>
      </Router>
    </SignalRProvider>
  )
}

export default App
