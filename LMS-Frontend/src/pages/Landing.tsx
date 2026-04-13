import { useNavigate } from 'react-router-dom'
import { useEffect, useState } from 'react'
import { getCurrentUser } from '../utils/auth'
import { motion } from 'framer-motion'
import {
  Video, Globe, ArrowRight, Play,
  GraduationCap, Lightbulb, BookOpen, Star, Clock,
  CheckCircle, Sparkles, Zap, Brain, TrendingUp, Shield, MessageSquare
} from 'lucide-react'
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
      .catch(() => {})
  }, [])

  const handleFindTutors = () => {
    const user = getCurrentUser()
    const role = user?.role?.toLowerCase()
    if (role === 'student') navigate('/student/find-tutors')
    else navigate('/find-tutors')
  }

  const faqs = [
    { title: 'How do I get started?', content: 'Sign up free, browse expert tutors, and book your first live session instantly. No setup required.' },
    { title: 'What payment methods do you accept?', content: 'We accept all major credit cards, UPI, and net banking. All payments are secure and encrypted.' },
    { title: 'Is there a free trial?', content: 'Yes! New users get 100 bonus credits on signup to take their first session free.' },
    { title: 'Is technical support available?', content: 'Our support team is available 24/7 via live chat directly from the app.' },
    { title: 'Is my data secure?', content: 'All data is encrypted end-to-end. We never share your personal information with third parties.' },
  ]

  const features = [
    { icon: Brain, label: 'AI-Matched Tutors', desc: 'Smart matching based on your learning style and goals', color: '#7c3aed' },
    { icon: Video, label: 'Live 1-on-1 Sessions', desc: 'Real-time HD video sessions with interactive tools', color: '#0891b2' },
    { icon: Zap, label: 'Instant Booking', desc: 'Find and book a session in under 60 seconds', color: '#d97706' },
    { icon: Shield, label: 'Verified Experts', desc: 'Every tutor is background-checked and skill-verified', color: '#059669' },
    { icon: TrendingUp, label: 'Progress Tracking', desc: 'AI-powered insights on your learning journey', color: '#be185d' },
    { icon: Globe, label: 'Learn Anywhere', desc: 'Access world-class tutors from any device, anytime', color: '#2563eb' },
  ]

  return (
    <div className="overflow-hidden" style={{ background: '#08090f', fontFamily: "'Inter', system-ui, sans-serif" }}>

      {/* ── HERO ─────────────────────────────────────────────────────────── */}
      <section className="relative py-16 lg:py-20 overflow-hidden">
        {/* Background glows */}
        <div className="absolute top-0 right-0 w-[480px] h-[480px] rounded-full opacity-[0.12] blur-[100px] pointer-events-none" style={{ background: 'radial-gradient(circle, #7c3aed, transparent)' }} />
        <div className="absolute bottom-0 left-0 w-[360px] h-[360px] rounded-full opacity-[0.08] blur-[80px] pointer-events-none" style={{ background: 'radial-gradient(circle, #0891b2, transparent)' }} />

        <div className="relative max-w-7xl mx-auto px-6 lg:px-8">
          <div className="grid lg:grid-cols-2 gap-12 items-center">

            {/* Left */}
            <motion.div initial={{ opacity: 0, y: 20 }} animate={{ opacity: 1, y: 0 }} transition={{ duration: 0.55 }}>
              <div className="inline-flex items-center gap-2 rounded-full px-3.5 py-1.5 mb-6 text-xs font-semibold tracking-wide" style={{ background: 'rgba(124,58,237,0.12)', border: '1px solid rgba(124,58,237,0.25)', color: '#a78bfa' }}>
                <Sparkles className="w-3.5 h-3.5" />
                AI-Powered Learning Platform
              </div>

              <h1 className="font-black leading-tight mb-5" style={{ fontSize: 'clamp(2.2rem, 5vw, 3.5rem)', color: '#ffffff', letterSpacing: '-0.02em' }}>
                Expert Tutors.<br />
                <span style={{ background: 'linear-gradient(90deg, #a78bfa, #f0abfc, #67e8f9)', WebkitBackgroundClip: 'text', WebkitTextFillColor: 'transparent' }}>
                  Real Results.
                </span>
              </h1>

              <p className="mb-8 leading-relaxed" style={{ fontSize: '1rem', color: 'rgba(255,255,255,0.55)', maxWidth: '440px' }}>
                Connect with verified experts for live 1-on-1 sessions. AI-matched tutoring built for students who want to learn faster and smarter.
              </p>

              <div className="flex flex-wrap gap-3 mb-10">
                <motion.button
                  whileHover={{ scale: 1.02 }} whileTap={{ scale: 0.98 }}
                  onClick={handleFindTutors}
                  className="inline-flex items-center gap-2 px-6 py-3 rounded-xl font-semibold text-sm text-white transition-all"
                  style={{ background: 'linear-gradient(135deg, #7c3aed, #a21caf)', boxShadow: '0 4px 24px rgba(124,58,237,0.35)' }}
                >
                  Find Tutors <ArrowRight className="w-4 h-4" />
                </motion.button>
                <motion.button
                  whileHover={{ scale: 1.02 }} whileTap={{ scale: 0.98 }}
                  onClick={() => navigate('/about-us')}
                  className="inline-flex items-center gap-2 px-6 py-3 rounded-xl font-semibold text-sm transition-all"
                  style={{ background: 'rgba(255,255,255,0.06)', border: '1px solid rgba(255,255,255,0.1)', color: 'rgba(255,255,255,0.8)' }}
                >
                  <Play className="w-3.5 h-3.5" /> How it Works
                </motion.button>
              </div>

              {/* Trust row */}
              <div className="flex items-center gap-4">
                <div className="flex -space-x-2">
                  {['#7c3aed','#0891b2','#d97706','#059669','#be185d'].map((c, i) => (
                    <div key={i} className="w-8 h-8 rounded-full border-2 flex items-center justify-center text-xs font-bold text-white" style={{ background: c, borderColor: '#08090f' }}>
                      {['V','A','K','R','S'][i]}
                    </div>
                  ))}
                </div>
                <div>
                  <div className="flex gap-0.5 mb-0.5">
                    {[...Array(5)].map((_, i) => <Star key={i} className="w-3 h-3 fill-yellow-400 text-yellow-400" />)}
                  </div>
                  <p className="text-xs" style={{ color: 'rgba(255,255,255,0.4)' }}>
                    <span style={{ color: '#fff', fontWeight: 600 }}>{studentCount ?? '50'}+ students</span> already learning
                  </p>
                </div>
              </div>
            </motion.div>

            {/* Right — Dashboard card */}
            <motion.div initial={{ opacity: 0, y: 20 }} animate={{ opacity: 1, y: 0 }} transition={{ duration: 0.55, delay: 0.15 }} className="relative">
              <div className="rounded-2xl p-5" style={{ background: 'rgba(255,255,255,0.04)', border: '1px solid rgba(255,255,255,0.08)', backdropFilter: 'blur(20px)' }}>

                {/* Header */}
                <div className="flex items-center justify-between mb-4">
                  <div>
                    <p className="text-xs font-semibold mb-0.5" style={{ color: 'rgba(255,255,255,0.35)', letterSpacing: '0.08em', textTransform: 'uppercase' }}>Live Dashboard</p>
                    <p className="text-sm font-semibold text-white">Your Learning Hub</p>
                  </div>
                  <div className="flex items-center gap-1.5 px-3 py-1.5 rounded-full text-xs font-semibold" style={{ background: 'rgba(16,185,129,0.12)', border: '1px solid rgba(16,185,129,0.2)', color: '#34d399' }}>
                    <span className="w-1.5 h-1.5 rounded-full bg-emerald-400 animate-pulse inline-block" />
                    Session Live
                  </div>
                </div>

                {/* Active session */}
                <div className="rounded-xl p-4 mb-3" style={{ background: 'rgba(124,58,237,0.1)', border: '1px solid rgba(124,58,237,0.18)' }}>
                  <div className="flex items-center gap-3 mb-3">
                    <div className="w-9 h-9 rounded-lg flex items-center justify-center flex-shrink-0" style={{ background: 'rgba(124,58,237,0.2)' }}>
                      <BookOpen className="w-4 h-4" style={{ color: '#a78bfa' }} />
                    </div>
                    <div className="flex-1 min-w-0">
                      <p className="text-sm font-semibold text-white truncate">Advanced Web Development</p>
                      <p className="text-xs" style={{ color: 'rgba(255,255,255,0.4)' }}>with Dr. Sarah K.</p>
                    </div>
                    <div className="text-right flex-shrink-0">
                      <p className="text-xs" style={{ color: 'rgba(255,255,255,0.35)' }}>Ends in</p>
                      <p className="text-sm font-bold" style={{ color: '#a78bfa' }}>32:15</p>
                    </div>
                  </div>
                  <div className="flex items-center gap-3 text-xs" style={{ color: 'rgba(255,255,255,0.4)' }}>
                    <span className="flex items-center gap-1"><Clock className="w-3 h-3" /> 60 min</span>
                    <span className="flex items-center gap-1"><Star className="w-3 h-3 fill-yellow-400 text-yellow-400" /> 4.9</span>
                    <span className="flex items-center gap-1 ml-auto" style={{ color: '#34d399' }}><CheckCircle className="w-3 h-3" /> Verified</span>
                  </div>
                </div>

                {/* Progress */}
                <div className="rounded-xl p-4 mb-3" style={{ background: 'rgba(255,255,255,0.03)', border: '1px solid rgba(255,255,255,0.06)' }}>
                  <div className="flex items-center justify-between mb-3">
                    <p className="text-xs font-semibold" style={{ color: 'rgba(255,255,255,0.5)', letterSpacing: '0.06em', textTransform: 'uppercase' }}>Weekly Progress</p>
                    <span className="text-xs font-medium" style={{ color: '#a78bfa' }}>+12% this week</span>
                  </div>
                  <div className="space-y-2.5">
                    {[
                      { label: 'Web Development', pct: 72, color: '#7c3aed' },
                      { label: 'Data Science', pct: 55, color: '#0891b2' },
                      { label: 'English Literature', pct: 38, color: '#d97706' },
                    ].map(item => (
                      <div key={item.label}>
                        <div className="flex justify-between text-xs mb-1">
                          <span style={{ color: 'rgba(255,255,255,0.65)' }}>{item.label}</span>
                          <span className="font-semibold text-white">{item.pct}%</span>
                        </div>
                        <div className="h-1.5 rounded-full" style={{ background: 'rgba(255,255,255,0.06)' }}>
                          <div className="h-full rounded-full transition-all" style={{ width: `${item.pct}%`, background: item.color }} />
                        </div>
                      </div>
                    ))}
                  </div>
                </div>

                {/* AI insight */}
                <div className="rounded-xl p-3.5 flex items-center gap-3" style={{ background: 'rgba(8,145,178,0.08)', border: '1px solid rgba(8,145,178,0.18)' }}>
                  <div className="w-8 h-8 rounded-lg flex items-center justify-center flex-shrink-0" style={{ background: 'rgba(8,145,178,0.2)' }}>
                    <Brain className="w-4 h-4" style={{ color: '#67e8f9' }} />
                  </div>
                  <div>
                    <p className="text-xs font-semibold text-white mb-0.5">AI Insight</p>
                    <p className="text-xs" style={{ color: 'rgba(255,255,255,0.45)' }}>You learn best in 45-min focused sessions</p>
                  </div>
                </div>
              </div>

              {/* Floating tag */}
              <motion.div animate={{ y: [-3, 3, -3] }} transition={{ duration: 3, repeat: Infinity }}
                className="absolute -top-4 -right-3 px-3.5 py-2 rounded-xl text-xs font-bold text-white shadow-lg"
                style={{ background: 'linear-gradient(135deg, #7c3aed, #a21caf)', boxShadow: '0 4px 20px rgba(124,58,237,0.4)' }}>
                ✦ AI-Powered
              </motion.div>
            </motion.div>
          </div>
        </div>
      </section>

      {/* ── STATS ───────────────────────────────────────────────────────── */}
      <section className="py-10 border-t border-b" style={{ borderColor: 'rgba(255,255,255,0.06)' }}>
        <div className="max-w-7xl mx-auto px-6 lg:px-8">
          <div className="grid grid-cols-3 gap-6 text-center">
            {[
              { value: '10+', label: 'Years Experience' },
              { value: studentCount !== null ? `${studentCount}+` : '50+', label: 'Students Enrolled' },
              { value: tutorCount !== null ? `${tutorCount}+` : '20+', label: 'Expert Tutors' },
            ].map((s, i) => (
              <motion.div key={i} initial={{ opacity: 0, y: 12 }} whileInView={{ opacity: 1, y: 0 }} viewport={{ once: true }} transition={{ delay: i * 0.08 }}>
                <p className="font-black mb-1" style={{ fontSize: 'clamp(1.8rem, 4vw, 2.8rem)', color: '#fff', letterSpacing: '-0.02em' }}>{s.value}</p>
                <p className="text-sm" style={{ color: 'rgba(255,255,255,0.4)' }}>{s.label}</p>
              </motion.div>
            ))}
          </div>
        </div>
      </section>

      {/* ── FEATURES ────────────────────────────────────────────────────── */}
      <section className="py-16">
        <div className="max-w-7xl mx-auto px-6 lg:px-8">
          <motion.div initial={{ opacity: 0, y: 16 }} whileInView={{ opacity: 1, y: 0 }} viewport={{ once: true }} className="mb-12">
            <p className="text-xs font-semibold mb-3 uppercase tracking-widest" style={{ color: '#a78bfa' }}>Platform Features</p>
            <h2 className="font-black mb-3" style={{ fontSize: 'clamp(1.8rem, 4vw, 2.6rem)', color: '#fff', letterSpacing: '-0.02em', maxWidth: '500px' }}>
              Everything you need to learn faster
            </h2>
            <p className="text-sm leading-relaxed" style={{ color: 'rgba(255,255,255,0.45)', maxWidth: '420px' }}>
              AI-powered tools and live expert sessions — the complete toolkit for modern learners.
            </p>
          </motion.div>

          <div className="grid sm:grid-cols-2 lg:grid-cols-3 gap-4">
            {features.map((f, i) => (
              <motion.div key={i} initial={{ opacity: 0, y: 16 }} whileInView={{ opacity: 1, y: 0 }} viewport={{ once: true }} transition={{ delay: i * 0.06 }}
                className="p-5 rounded-2xl transition-all duration-200 hover:translate-y-[-2px]"
                style={{ background: 'rgba(255,255,255,0.03)', border: '1px solid rgba(255,255,255,0.07)' }}>
                <div className="w-10 h-10 rounded-xl flex items-center justify-center mb-4" style={{ background: `${f.color}22` }}>
                  <f.icon className="w-5 h-5" style={{ color: f.color }} />
                </div>
                <h3 className="text-sm font-bold text-white mb-1.5">{f.label}</h3>
                <p className="text-xs leading-relaxed" style={{ color: 'rgba(255,255,255,0.45)' }}>{f.desc}</p>
              </motion.div>
            ))}
          </div>
        </div>
      </section>

      {/* ── HOW IT WORKS ────────────────────────────────────────────────── */}
      <section className="py-16 border-t" style={{ borderColor: 'rgba(255,255,255,0.06)' }}>
        <div className="max-w-7xl mx-auto px-6 lg:px-8">
          <motion.div initial={{ opacity: 0, y: 16 }} whileInView={{ opacity: 1, y: 0 }} viewport={{ once: true }} className="text-center mb-12">
            <p className="text-xs font-semibold mb-3 uppercase tracking-widest" style={{ color: '#67e8f9' }}>How it works</p>
            <h2 className="font-black" style={{ fontSize: 'clamp(1.8rem, 4vw, 2.6rem)', color: '#fff', letterSpacing: '-0.02em' }}>
              Start learning in 3 steps
            </h2>
          </motion.div>

          <div className="grid md:grid-cols-3 gap-5">
            {[
              { step: '01', title: 'Create Account', desc: 'Sign up free and get 100 bonus credits instantly. No credit card needed.', icon: Zap, color: '#7c3aed' },
              { step: '02', title: 'Find Your Tutor', desc: 'Browse AI-matched experts by subject, read reviews, and book instantly.', icon: Brain, color: '#0891b2' },
              { step: '03', title: 'Start Learning', desc: 'Join your HD live session, track progress, and level up your skills.', icon: TrendingUp, color: '#059669' },
            ].map((item, i) => (
              <motion.div key={i} initial={{ opacity: 0, y: 16 }} whileInView={{ opacity: 1, y: 0 }} viewport={{ once: true }} transition={{ delay: i * 0.12 }}
                className="relative p-6 rounded-2xl"
                style={{ background: 'rgba(255,255,255,0.03)', border: '1px solid rgba(255,255,255,0.07)' }}>
                <span className="absolute top-5 right-5 text-5xl font-black" style={{ color: 'rgba(255,255,255,0.04)', lineHeight: 1 }}>{item.step}</span>
                <div className="w-11 h-11 rounded-xl flex items-center justify-center mb-4" style={{ background: `${item.color}20` }}>
                  <item.icon className="w-5 h-5" style={{ color: item.color }} />
                </div>
                <h3 className="text-sm font-bold text-white mb-2">{item.title}</h3>
                <p className="text-xs leading-relaxed" style={{ color: 'rgba(255,255,255,0.45)' }}>{item.desc}</p>
              </motion.div>
            ))}
          </div>
        </div>
      </section>

      {/* ── TESTIMONIAL ─────────────────────────────────────────────────── */}
      <section className="py-16">
        <div className="max-w-3xl mx-auto px-6 lg:px-8">
          <motion.div initial={{ opacity: 0, y: 16 }} whileInView={{ opacity: 1, y: 0 }} viewport={{ once: true }}
            className="rounded-2xl p-8"
            style={{ background: 'rgba(255,255,255,0.04)', border: '1px solid rgba(255,255,255,0.08)' }}>
            <div className="flex gap-1 mb-5">
              {[...Array(5)].map((_, i) => <Star key={i} className="w-4 h-4 fill-yellow-400 text-yellow-400" />)}
            </div>
            <p className="text-base leading-relaxed mb-6" style={{ color: 'rgba(255,255,255,0.75)', fontStyle: 'italic' }}>
              "LiveExpert.AI completely changed how I learn. The AI matching found me the perfect tutor within minutes — someone who truly understood my learning style. I went from failing to acing my exams in 4 weeks."
            </p>
            <div className="flex items-center gap-3">
              <div className="w-10 h-10 rounded-full flex items-center justify-center text-sm font-bold text-white" style={{ background: 'linear-gradient(135deg, #7c3aed, #a21caf)' }}>P</div>
              <div>
                <p className="text-sm font-semibold text-white">Priya Sharma</p>
                <p className="text-xs" style={{ color: 'rgba(255,255,255,0.4)' }}>Engineering Student</p>
              </div>
              <MessageSquare className="w-5 h-5 ml-auto" style={{ color: 'rgba(255,255,255,0.1)' }} />
            </div>
          </motion.div>
        </div>
      </section>

      {/* ── FAQ ─────────────────────────────────────────────────────────── */}
      <section className="py-16 border-t" style={{ borderColor: 'rgba(255,255,255,0.06)' }}>
        <div className="max-w-2xl mx-auto px-6 lg:px-8">
          <motion.div initial={{ opacity: 0, y: 16 }} whileInView={{ opacity: 1, y: 0 }} viewport={{ once: true }} className="text-center mb-10">
            <h2 className="font-black mb-2" style={{ fontSize: 'clamp(1.6rem, 3.5vw, 2.2rem)', color: '#fff', letterSpacing: '-0.02em' }}>
              Frequently asked questions
            </h2>
            <p className="text-sm" style={{ color: 'rgba(255,255,255,0.4)' }}>Everything you need to know before getting started.</p>
          </motion.div>
          <Accordion items={faqs} />
        </div>
      </section>

      {/* ── CTA ─────────────────────────────────────────────────────────── */}
      <section className="py-16">
        <div className="max-w-7xl mx-auto px-6 lg:px-8">
          <motion.div initial={{ opacity: 0, y: 16 }} whileInView={{ opacity: 1, y: 0 }} viewport={{ once: true }} className="text-center mb-10">
            <h2 className="font-black mb-2" style={{ fontSize: 'clamp(1.8rem, 4vw, 2.6rem)', color: '#fff', letterSpacing: '-0.02em' }}>What are you here for?</h2>
            <p className="text-sm" style={{ color: 'rgba(255,255,255,0.4)' }}>Join as a student or start teaching as a tutor.</p>
          </motion.div>

          <div className="grid md:grid-cols-2 gap-5 max-w-4xl mx-auto">
            {/* Tutor */}
            <motion.div initial={{ opacity: 0, x: -16 }} whileInView={{ opacity: 1, x: 0 }} viewport={{ once: true }}
              whileHover={{ y: -3 }}
              className="p-8 rounded-2xl cursor-pointer transition-all duration-200"
              style={{ background: 'rgba(255,255,255,0.04)', border: '1px solid rgba(255,255,255,0.09)' }}
              onClick={() => navigate('/register?role=tutor')}>
              <div className="w-11 h-11 rounded-xl flex items-center justify-center mb-5" style={{ background: 'rgba(8,145,178,0.15)' }}>
                <GraduationCap className="w-5 h-5" style={{ color: '#67e8f9' }} />
              </div>
              <h3 className="text-lg font-bold text-white mb-2">Teach on LiveExpert.AI</h3>
              <p className="text-sm leading-relaxed mb-6" style={{ color: 'rgba(255,255,255,0.45)' }}>
                Share your expertise, set your own schedule, and earn competitive income from students worldwide.
              </p>
              <div className="inline-flex items-center gap-2 text-sm font-semibold" style={{ color: '#67e8f9' }}>
                Get Started <ArrowRight className="w-4 h-4" />
              </div>
            </motion.div>

            {/* Student */}
            <motion.div initial={{ opacity: 0, x: 16 }} whileInView={{ opacity: 1, x: 0 }} viewport={{ once: true }}
              whileHover={{ y: -3 }}
              className="p-8 rounded-2xl cursor-pointer transition-all duration-200 relative overflow-hidden"
              style={{ background: 'linear-gradient(135deg, #4f1d96 0%, #6d28d9 50%, #7e22ce 100%)', border: '1px solid rgba(167,139,250,0.2)' }}
              onClick={() => navigate('/register?role=student')}>
              <div className="absolute top-0 right-0 w-48 h-48 rounded-full pointer-events-none" style={{ background: 'rgba(255,255,255,0.05)', filter: 'blur(40px)' }} />
              <div className="relative">
                <div className="w-11 h-11 rounded-xl flex items-center justify-center mb-5" style={{ background: 'rgba(255,255,255,0.15)' }}>
                  <Lightbulb className="w-5 h-5 text-white" />
                </div>
                <h3 className="text-lg font-bold text-white mb-2">Start Learning Today</h3>
                <p className="text-sm leading-relaxed mb-6" style={{ color: 'rgba(255,255,255,0.65)' }}>
                  Get AI-matched with the perfect tutor. Live sessions, progress tracking, and 100 free credits on signup.
                </p>
                <div className="inline-flex items-center gap-2 px-5 py-2.5 rounded-xl text-sm font-bold transition-all" style={{ background: '#fff', color: '#6d28d9' }}>
                  Enroll Free <ArrowRight className="w-4 h-4" />
                </div>
              </div>
            </motion.div>
          </div>
        </div>
      </section>

    </div>
  )
}

export default Landing
