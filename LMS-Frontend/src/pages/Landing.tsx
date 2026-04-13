import { useNavigate } from 'react-router-dom'
import { useEffect, useState } from 'react'
import { getCurrentUser } from '../utils/auth'
import { motion } from 'framer-motion'
import {
  Video, Award, Globe,
  Users,
  ArrowRight, Play, GraduationCap,
  Lightbulb, BookOpen, Star, Clock, CheckCircle
} from 'lucide-react'
import Button from '../components/ui/Button'
import { Card } from '../components/ui/Card'
import { Accordion } from '../components/ui/Accordion'
import { getPlatformStats } from '../services/tutorsApi'

const Landing = () => {
  const navigate = useNavigate()
  const [studentCount, setStudentCount] = useState<number | null>(null)
  const [tutorCount, setTutorCount] = useState<number | null>(null)

  useEffect(() => {
    getPlatformStats()
      .then(stats => {
        setStudentCount(stats.studentCount)
        setTutorCount(stats.tutorCount)
      })
      .catch(() => { /* silently keep defaults */ })
  }, [])

  const handleFindTutors = () => {
    const user = getCurrentUser()
    const role = user?.role?.toLowerCase()
    if (role === 'student') {
      navigate('/student/find-tutors')
    } else {
      navigate('/find-tutors')
    }
  }

  const faqs = [
    {
      title: 'How do I get started with your product?',
      content: 'Getting started is easy! Simply sign up for an account, find a tutor, and start learning. You can book a session right away.',
    },
    {
      title: 'What payment methods do you accept?',
      content: 'We accept all major credit cards, PayPal, and bank transfers. All payments are secure and encrypted.',
    },
    {
      title: 'Is there a free trial available?',
      content: 'Yes! We offer a free trial for new users. You can explore our platform and take a few sample lessons before committing to a paid plan.',
    },
    {
      title: 'Is technical support available?',
      content: 'Absolutely! Our support team is available 24/7 to help you with any questions or issues you may have.',
    },
    {
      title: 'Is my data secure with your product?',
      content: 'We take data security very seriously. All your personal information and payment details are encrypted and stored securely. We never share your data with third parties.',
    },
  ]

  return (
    <div className="overflow-hidden bg-white">
      {/* Hero Section */}
      <section className="relative py-12 lg:py-8 bg-gradient-to-br from-secondary-50 via-white to-primary-50 overflow-hidden">
        <div className="absolute top-0 right-0 w-96 h-96 bg-secondary-200 rounded-full blur-3xl opacity-20"></div>
        <div className="absolute bottom-0 left-0 w-96 h-96 bg-secondary-200 rounded-full blur-3xl opacity-20"></div>

        <div className="relative max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="grid lg:grid-cols-2 gap-12 items-center">
            <motion.div
              initial={{ opacity: 0, x: -20 }}
              animate={{ opacity: 1, x: 0 }}
              transition={{ duration: 0.6 }}
            >
              <span className="section-tag">eLearning Platform</span>
              <h1 className="text-5xl md:text-6xl lg:text-7xl font-bold mb-6 leading-tight">
                Smart Learning Deeper & More{' '}
                <span className="text-secondary-600">-Amazing</span>
              </h1>
              <p className="text-lg text-gray-600 mb-8 leading-relaxed">
                We are passionate about empowering learners Worldwide with high-quality, accessible & engaging education through live sessions with expert tutors.
              </p>
              <div className="flex flex-col sm:flex-row gap-4">
                <Button size="lg" onClick={handleFindTutors}>
                  Find Tutors
                  <ArrowRight className="ml-2 w-5 h-5" />
                </Button>
                <button
                  onClick={() => navigate('/about-us')}
                  className="inline-flex items-center justify-center px-6 py-3 text-lg rounded-lg font-medium transition-all duration-200 bg-secondary-500 text-white hover:bg-secondary-600 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-secondary-500"
                >
                  <Play className="mr-2 w-5 h-5" />
                  How it Works
                </button>
              </div>
            </motion.div>

            <motion.div
              initial={{ opacity: 0, x: 20 }}
              animate={{ opacity: 1, x: 0 }}
              transition={{ duration: 0.6, delay: 0.2 }}
              className="relative"
            >
              <div className="relative bg-gradient-to-br from-primary-50 to-secondary-50 rounded-2xl shadow-2xl p-6 border border-primary-100">
                {/* Course Card */}
                <div className="bg-white rounded-xl shadow-md p-5 mb-4">
                  <div className="flex items-center gap-3 mb-3">
                    <div className="w-10 h-10 rounded-lg bg-primary-100 flex items-center justify-center">
                      <BookOpen className="w-5 h-5 text-primary-600" />
                    </div>
                    <div>
                      <p className="font-semibold text-gray-900 text-sm">Live 1-on-1 Session</p>
                      <p className="text-xs text-gray-500">with Expert Tutor</p>
                    </div>
                    <span className="ml-auto text-xs bg-green-100 text-green-700 px-2 py-1 rounded-full font-medium">Live</span>
                  </div>
                  <div className="flex items-center gap-4 text-xs text-gray-500">
                    <span className="flex items-center gap-1"><Clock className="w-3 h-3" /> 60 min</span>
                    <span className="flex items-center gap-1"><Star className="w-3 h-3 text-yellow-400 fill-yellow-400" /> 4.9</span>
                    <span className="flex items-center gap-1"><Users className="w-3 h-3" /> 1,200+ students</span>
                  </div>
                </div>

                {/* Progress Card */}
                <div className="bg-white rounded-xl shadow-md p-5 mb-4">
                  <p className="text-xs font-semibold text-gray-500 mb-3 uppercase tracking-wide">Your Progress</p>
                  <div className="space-y-2">
                    {[
                      { label: 'Mathematics', pct: 78 },
                      { label: 'Web Development', pct: 55 },
                      { label: 'Data Science', pct: 40 },
                    ].map(item => (
                      <div key={item.label}>
                        <div className="flex justify-between text-xs mb-1">
                          <span className="text-gray-600">{item.label}</span>
                          <span className="text-primary-600 font-medium">{item.pct}%</span>
                        </div>
                        <div className="h-1.5 bg-gray-100 rounded-full">
                          <div className="h-1.5 bg-primary-500 rounded-full" style={{ width: `${item.pct}%` }} />
                        </div>
                      </div>
                    ))}
                  </div>
                </div>

                {/* Achievement Badge */}
                <div className="bg-white rounded-xl shadow-md p-4 flex items-center gap-3">
                  <div className="w-10 h-10 rounded-full bg-yellow-100 flex items-center justify-center">
                    <Award className="w-5 h-5 text-yellow-500" />
                  </div>
                  <div className="flex-1">
                    <p className="text-sm font-semibold text-gray-900">Top Achiever</p>
                    <p className="text-xs text-gray-500">Completed 10 sessions</p>
                  </div>
                  <CheckCircle className="w-5 h-5 text-green-500" />
                </div>

                {/* Floating stat */}
                <div className="absolute -top-4 -right-4 bg-secondary-500 text-white rounded-xl px-4 py-2 shadow-lg text-sm font-semibold">
                  AI-Powered Learning
                </div>
              </div>
            </motion.div>
          </div>
        </div>
      </section>

      {/* Statistics Section */}
      <section className="py-16 bg-white border-y border-gray-100">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="grid grid-cols-1 md:grid-cols-3 gap-8 text-center">
            <div>
              <h3 className="text-4xl md:text-5xl font-bold text-gray-900 mb-2">10+</h3>
              <p className="text-gray-600">Years of Professional Experience Trainers</p>
            </div>
            <div>
              <h3 className="text-4xl md:text-5xl font-bold text-gray-900 mb-2">
                {studentCount !== null ? `${studentCount}+` : '50+'}
              </h3>
              <p className="text-gray-600">Students Enrolled</p>
            </div>
            <div>
              <h3 className="text-4xl md:text-5xl font-bold text-gray-900 mb-2">
                {tutorCount !== null ? `${tutorCount}+` : '20+'}
              </h3>
              <p className="text-gray-600">Expert Tutors & Teachers</p>
            </div>
          </div>
        </div>
      </section>

      {/* Explore Our Course Section removed */}

      {/* Growth Skill Section */}
      <section className="py-20 bg-gray-50">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="grid lg:grid-cols-2 gap-12 items-center">
            <div>
              <h2 className="text-4xl md:text-5xl font-bold text-gray-900 mb-6 leading-tight">
                Growth Skill With{' '}
                <span className="text-secondary-600">LiveExpert.AI</span> Academy & Accelerate to your Better future
              </h2>
              <p className="text-lg text-gray-600 mb-4 leading-relaxed">
                We are passionate about empowering learners Worldwide with high-quality, accessible & engaging education led by expert tutors.
              </p>
              <p className="text-lg text-gray-600 mb-8 leading-relaxed">
                Join thousands of students who have transformed their careers through our platform. Get access to expert tutors, live sessions, and personalized learning paths.
              </p>
              <Button size="lg" onClick={handleFindTutors}>
                Find Tutors
                <ArrowRight className="ml-2 w-5 h-5" />
              </Button>
            </div>
            <div className="relative">
              <div className="grid grid-cols-2 gap-4">
                <div className="bg-gradient-to-br from-primary-100 to-primary-200 rounded-xl p-8 flex items-center justify-center min-h-[200px]">
                  <div className="text-center">
                    <div className="w-20 h-20 mx-auto mb-4 rounded-full bg-white flex items-center justify-center">
                      <Users className="w-10 h-10 text-primary-600" />
                    </div>
                    <p className="font-semibold text-gray-900">Expert Tutors</p>
                  </div>
                </div>
                <div className="bg-gradient-to-br from-secondary-100 to-secondary-200 rounded-xl p-8 flex items-center justify-center min-h-[200px]">
                  <div className="text-center">
                    <div className="w-20 h-20 mx-auto mb-4 rounded-full bg-white flex items-center justify-center">
                      <Video className="w-10 h-10 text-secondary-600" />
                    </div>
                    <p className="font-semibold text-gray-900">Live Sessions</p>
                  </div>
                </div>
                <div className="bg-gradient-to-br from-secondary-100 to-secondary-200 rounded-xl p-8 flex items-center justify-center min-h-[200px]">
                  <div className="text-center">
                    <div className="w-20 h-20 mx-auto mb-4 rounded-full bg-white flex items-center justify-center">
                      <Award className="w-10 h-10 text-secondary-600" />
                    </div>
                    <p className="font-semibold text-gray-900">Certifications</p>
                  </div>
                </div>
                <div className="bg-gradient-to-br from-primary-100 to-primary-200 rounded-xl p-8 flex items-center justify-center min-h-[200px]">
                  <div className="text-center">
                    <div className="w-20 h-20 mx-auto mb-4 rounded-full bg-white flex items-center justify-center">
                      <Globe className="w-10 h-10 text-primary-600" />
                    </div>
                    <p className="font-semibold text-gray-900">Global Access</p>
                  </div>
                </div>
              </div>
              <div className="absolute -bottom-6 -right-6 bg-white rounded-lg shadow-xl p-4 border border-gray-200">
                <p className="text-sm text-gray-600 mb-1">Active tutors</p>
                <p className="text-2xl font-bold text-gray-900">
                  {tutorCount !== null ? `${tutorCount}+` : '20+'}
                </p>
              </div>
            </div>
          </div>
        </div>
      </section>

      {/* Testimonials Section */}
      <section className="py-20 bg-white">
        <div className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="text-center mb-12">
            <span className="section-tag">Testimonial</span>
            <h2 className="text-4xl font-bold text-gray-900 mb-4">See why We're rated #1 in Online Platform tech</h2>
          </div>
          <Card className="p-8">
            <p className="text-lg text-gray-700 mb-6 leading-relaxed">
              "Our dynamic educational platform offers you the tools and resources to propel yourself towards a brighter future. With expert guidance & a supportive community, you'll achieve your learning goals faster than ever before."
            </p>
            <div className="flex items-center gap-4">
              <div className="w-12 h-12 rounded-full bg-gradient-primary flex items-center justify-center text-white font-semibold">
                CL
              </div>
              <div>
                <h4 className="font-semibold text-gray-900">CodeLine</h4>
                <p className="text-sm text-gray-600">Project Manager</p>
              </div>
            </div>
          </Card>
        </div>
      </section>

      {/* FAQ Section */}
      <section className="py-20 bg-gray-50">
        <div className="max-w-3xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="text-center mb-12">
            <span className="section-tag">FAQ</span>
            <h2 className="text-4xl font-bold text-gray-900 mb-4">Frequently asked Questions</h2>
            <p className="text-lg text-gray-600">
              Have questions? We're here to help. Find answers to common questions below.
            </p>
          </div>
          <Accordion items={faqs} />
        </div>
      </section>

      {/* CTA Cards Section */}
      <section className="py-20 bg-white">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <h2 className="text-4xl font-bold text-gray-900 mb-12 text-center">What You Looking for?</h2>
          <div className="grid md:grid-cols-2 gap-6">
            <Card hover className="p-8">
              <div className="w-16 h-16 rounded-xl bg-secondary-100 flex items-center justify-center text-secondary-600 mb-6">
                <GraduationCap className="w-8 h-8" />
              </div>
              <h3 className="text-2xl font-bold text-gray-900 mb-4">Do You Want Teach Here</h3>
              <p className="text-gray-600 mb-6">
                Join our platform as a tutor and share your expertise with students worldwide. Build your teaching career with flexible scheduling and competitive earnings.
              </p>
              <Button
                variant="outline"
                className="bg-gray-900 text-white hover:bg-gray-800 border-gray-900"
                onClick={() => navigate('/register?role=tutor')}
              >
                Get Started
                <ArrowRight className="ml-2 w-5 h-5" />
              </Button>
            </Card>
            <Card hover className="p-8 bg-gradient-primary text-white">
              <div className="w-16 h-16 rounded-xl bg-white/20 flex items-center justify-center mb-6">
                <Lightbulb className="w-8 h-8" />
              </div>
              <h3 className="text-2xl font-bold mb-4">Do You Want Learn Here</h3>
              <p className="mb-6 opacity-90">
                Start your learning journey with expert tutors and live sessions. Access personalized 1-on-1 sessions and structured learning support.
              </p>
              <Button
                variant="outline"
                className="bg-white text-primary-600 hover:bg-gray-50 border-white"
                onClick={() => navigate('/register?role=student')}
              >
                Enroll Now
                <ArrowRight className="ml-2 w-5 h-5" />
              </Button>
            </Card>
          </div>
        </div>
      </section>
    </div>
  )
}

export default Landing
