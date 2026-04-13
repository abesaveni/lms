import { useNavigate } from 'react-router-dom'
import { ArrowRight, GraduationCap, Lightbulb } from 'lucide-react'
import { Card } from '../components/ui/Card'
import Button from '../components/ui/Button'

const JoinUs = () => {
  const navigate = useNavigate()

  return (
    <div className="min-h-screen bg-white py-16 relative">
      <button
        onClick={() => navigate('/')}
        className="absolute top-4 left-4 flex items-center gap-2 text-gray-600 hover:text-gray-900 transition-colors"
      >
        <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M10 19l-7-7m0 0l7-7m-7 7h18" />
        </svg>
        <span className="text-sm font-medium">Back to Home</span>
      </button>
      <div className="max-w-5xl mx-auto px-4 sm:px-6 lg:px-8">
        <div className="text-center mb-10">
          <h1 className="text-4xl font-bold text-gray-900 mb-3">Join LiveExpert.AI</h1>
          <p className="text-gray-600">
            Do you want to teach here or learn here? Choose your path to continue.
          </p>
        </div>

        <div className="grid md:grid-cols-2 gap-6">
          <Card hover className="p-8">
            <div className="w-16 h-16 rounded-xl bg-secondary-100 flex items-center justify-center text-secondary-600 mb-6">
              <GraduationCap className="w-8 h-8" />
            </div>
            <h3 className="text-2xl font-bold text-gray-900 mb-4">Teach Here</h3>
            <p className="text-gray-600 mb-6">
              Join as a tutor and share your expertise with students worldwide. Build your teaching
              career with flexible scheduling and competitive earnings.
            </p>
            <Button
              variant="outline"
              className="bg-gray-900 text-white hover:bg-gray-800 border-gray-900"
              onClick={() => navigate('/register?role=tutor')}
            >
              Continue as Tutor
              <ArrowRight className="ml-2 w-5 h-5" />
            </Button>
          </Card>

          <Card hover className="p-8 bg-gradient-primary text-white">
            <div className="w-16 h-16 rounded-xl bg-white/20 flex items-center justify-center mb-6">
              <Lightbulb className="w-8 h-8" />
            </div>
            <h3 className="text-2xl font-bold mb-4">Learn Here</h3>
            <p className="mb-6 opacity-90">
              Start your learning journey with expert tutors and live sessions. Access
              personalized 1-on-1 sessions and structured learning support.
            </p>
            <Button
              variant="outline"
              className="bg-white text-primary-600 hover:bg-gray-50 border-white"
              onClick={() => navigate('/register?role=student')}
            >
              Continue as Student
              <ArrowRight className="ml-2 w-5 h-5" />
            </Button>
          </Card>
        </div>
      </div>
    </div>
  )
}

export default JoinUs
