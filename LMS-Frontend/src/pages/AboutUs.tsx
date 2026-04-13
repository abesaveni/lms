import { motion } from 'framer-motion'
import { Users, GraduationCap, Globe, Target, Heart, Zap, Shield } from 'lucide-react'
import { Card } from '../components/ui/Card'
import Button from '../components/ui/Button'
import { useNavigate } from 'react-router-dom'
import { ArrowRight } from 'lucide-react'

const AboutUs = () => {
  const navigate = useNavigate()

  const values = [
    {
      icon: <Target className="w-8 h-8" />,
      title: 'Our Mission',
      description: 'To make quality education accessible to everyone, connecting learners with expert tutors worldwide.',
    },
    {
      icon: <Heart className="w-8 h-8" />,
      title: 'Our Vision',
      description: 'To become the leading platform for personalized online learning, empowering millions of learners.',
    },
    {
      icon: <Zap className="w-8 h-8" />,
      title: 'Innovation',
      description: 'We continuously innovate to provide the best learning experience with cutting-edge technology.',
    },
    {
      icon: <Shield className="w-8 h-8" />,
      title: 'Trust & Quality',
      description: 'All our tutors are verified to ensure the best learning outcomes.',
    },
  ]

  const stats = [
    { label: 'Active Students', value: '50,000+', icon: <Users className="w-6 h-6" /> },
    { label: 'Expert Tutors', value: '500+', icon: <GraduationCap className="w-6 h-6" /> },
    { label: 'Countries Served', value: '50+', icon: <Globe className="w-6 h-6" /> },
  ]

  const steps = [
    {
      number: '1',
      title: 'Find Your Tutor',
      description: 'Browse through our verified expert tutors. Filter by subject, price, rating, and availability to find the perfect match for your learning goals.',
    },
    {
      number: '2',
      title: 'Book a Session',
      description: 'Choose between 1-on-1 sessions or group sessions. Book sessions and learn at your own pace.',
    },
    {
      number: '3',
      title: 'Start Learning',
      description: 'Join live sessions with interactive whiteboards, screen sharing, and real-time collaboration. Track your progress in real time.',
    },
    {
      number: '4',
      title: 'Keep Improving',
      description: 'Get feedback from tutors and keep building your skills with every session.',
    },
  ]

  return (
    <div className="overflow-hidden">
      {/* Hero Section */}
      <section className="relative py-20 lg:py-32 bg-gradient-to-br from-primary-50 via-white to-secondary-50">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <motion.div
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ duration: 0.6 }}
            className="text-center max-w-4xl mx-auto"
          >
            <h1 className="text-5xl md:text-6xl font-bold mb-6">
              About <span className="text-primary-600">LiveExpert.AI</span>
            </h1>
            <p className="text-xl md:text-2xl text-gray-600 mb-8 leading-relaxed">
              We are passionate about empowering learners worldwide with high-quality, accessible & engaging education.
            </p>
          </motion.div>
        </div>
      </section>

      {/* Stats Section */}
      <section className="py-16 bg-white border-y border-gray-100">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="grid grid-cols-2 md:grid-cols-4 gap-8 text-center">
            {stats.map((stat, idx) => (
              <motion.div
                key={idx}
                initial={{ opacity: 0, y: 20 }}
                whileInView={{ opacity: 1, y: 0 }}
                viewport={{ once: true }}
                transition={{ delay: idx * 0.1 }}
              >
                <div className="flex justify-center mb-4 text-primary-600">
                  {stat.icon}
                </div>
                <h3 className="text-4xl md:text-5xl font-bold text-gray-900 mb-2">{stat.value}</h3>
                <p className="text-gray-600">{stat.label}</p>
              </motion.div>
            ))}
          </div>
        </div>
      </section>

      {/* Mission & Vision */}
      <section className="py-20 bg-gray-50">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="text-center mb-12">
            <h2 className="text-4xl font-bold text-gray-900 mb-4">Our Values</h2>
            <p className="text-xl text-gray-600">What drives us every day</p>
          </div>
          <div className="grid md:grid-cols-2 lg:grid-cols-4 gap-6">
            {values.map((value, idx) => (
              <motion.div
                key={idx}
                initial={{ opacity: 0, y: 20 }}
                whileInView={{ opacity: 1, y: 0 }}
                viewport={{ once: true }}
                transition={{ delay: idx * 0.1 }}
              >
                <Card hover className="text-center h-full">
                  <div className="w-16 h-16 mx-auto mb-4 rounded-xl bg-primary-100 flex items-center justify-center text-primary-600">
                    {value.icon}
                  </div>
                  <h3 className="text-xl font-semibold mb-2">{value.title}</h3>
                  <p className="text-gray-600">{value.description}</p>
                </Card>
              </motion.div>
            ))}
          </div>
        </div>
      </section>

      {/* How It Works */}
      <section className="py-20 bg-white">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="text-center mb-12">
            <h2 className="text-4xl font-bold text-gray-900 mb-4">How It Works</h2>
            <p className="text-xl text-gray-600">Get started in 4 simple steps</p>
          </div>
          <div className="grid md:grid-cols-2 lg:grid-cols-4 gap-8">
            {steps.map((step, idx) => (
              <motion.div
                key={idx}
                initial={{ opacity: 0, y: 20 }}
                whileInView={{ opacity: 1, y: 0 }}
                viewport={{ once: true }}
                transition={{ delay: idx * 0.1 }}
                className="relative"
              >
                <Card hover className="h-full">
                  <div className="w-12 h-12 rounded-full bg-gradient-primary text-white flex items-center justify-center text-xl font-bold mb-4">
                    {step.number}
                  </div>
                  <h3 className="text-xl font-semibold mb-2">{step.title}</h3>
                  <p className="text-gray-600">{step.description}</p>
                </Card>
              </motion.div>
            ))}
          </div>
        </div>
      </section>

      {/* Why Choose Us */}
      <section className="py-20 bg-gray-50">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="grid lg:grid-cols-2 gap-12 items-center">
            <div>
              <h2 className="text-4xl font-bold text-gray-900 mb-6">
                Why Choose <span className="text-primary-600">LiveExpert.AI</span>?
              </h2>
              <div className="space-y-6">
                <div>
                  <h3 className="text-xl font-semibold text-gray-900 mb-2">Expert Tutors</h3>
                  <p className="text-gray-600">
                    All our tutors are verified professionals with years of experience in their fields. 
                    They undergo a rigorous verification process to ensure quality.
                  </p>
                </div>
                <div>
                  <h3 className="text-xl font-semibold text-gray-900 mb-2">Flexible Learning</h3>
                  <p className="text-gray-600">
                    Choose from 1-on-1 sessions or group sessions. Learn at your own pace 
                    and schedule sessions that fit your lifestyle.
                  </p>
                </div>
                <div>
                  <h3 className="text-xl font-semibold text-gray-900 mb-2">Interactive Platform</h3>
                  <p className="text-gray-600">
                    Our platform features live video sessions with interactive whiteboards, screen sharing, 
                    and real-time collaboration tools for an engaging learning experience.
                  </p>
                </div>
                <div>
                  <h3 className="text-xl font-semibold text-gray-900 mb-2">Personalized Feedback</h3>
                  <p className="text-gray-600">
                    Get actionable feedback and learning guidance from tutors to keep improving.
                  </p>
                </div>
              </div>
            </div>
            <div className="relative">
              <div className="bg-gradient-to-br from-primary-100 to-secondary-100 rounded-2xl p-12">
                <div className="text-center">
                  <div className="w-32 h-32 mx-auto mb-6 rounded-full bg-white flex items-center justify-center shadow-xl">
                    <GraduationCap className="w-16 h-16 text-primary-600" />
                  </div>
                  <h3 className="text-2xl font-bold text-gray-900 mb-4">Join Thousands of Learners</h3>
                  <p className="text-gray-700 mb-6">
                    Start your learning journey today and join our community of successful learners.
                  </p>
                  <Button size="lg" onClick={() => navigate('/register')}>
                    Get Started
                    <ArrowRight className="ml-2 w-5 h-5" />
                  </Button>
                </div>
              </div>
            </div>
          </div>
        </div>
      </section>

      {/* CTA Section */}
      <section className="py-20 bg-gradient-primary text-white">
        <div className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 text-center">
          <h2 className="text-4xl md:text-5xl font-bold mb-4">Ready to Start Learning?</h2>
          <p className="text-xl mb-8 opacity-90">
            Join thousands of students already learning with LiveExpert.AI
          </p>
          <div className="flex flex-col sm:flex-row gap-4 justify-center">
            <Button size="lg" variant="secondary" onClick={() => navigate('/find-tutors')}>
              Find Tutors
              <ArrowRight className="ml-2 w-5 h-5" />
            </Button>
            <Button size="lg" variant="outline" className="bg-white text-primary-600 hover:bg-gray-50" onClick={() => navigate('/register')}>
              Get Started Free
              <ArrowRight className="ml-2 w-5 h-5" />
            </Button>
          </div>
        </div>
      </section>
    </div>
  )
}

export default AboutUs
