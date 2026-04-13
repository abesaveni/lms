import { useNavigate } from 'react-router-dom'
import { useEffect, useState } from 'react'
import { getCurrentUser } from '../utils/auth'
import { motion } from 'framer-motion'
import {
  Video, Award, Globe, Users, ArrowRight, Play,
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
    if (role === 'student') {
      navigate('/student/find-tutors')
    } else {
      navigate('/find-tutors')
    }
  }

  const faqs = [
    {
      title: 'How do I get started?',
      content: 'Sign up in seconds, browse expert tutors, and book your first live session instantly. No setup required.',
    },
    {
      title: 'What payment methods do you accept?',
      content: 'We accept all major credit cards, UPI, and net banking. All payments are secure and encrypted.',
    },
    {
      title: 'Is there a free trial available?',
      content: 'Yes! New users get 100 bonus credits on signup to explore the platform and take their first session.',
    },
    {
      title: 'Is technical support available?',
      content: 'Our support team is available 24/7. Chat with us anytime from the app.',
    },
    {
      title: 'Is my data secure?',
      content: 'Absolutely. All data is encrypted end-to-end. We never share your information with third parties.',
    },
  ]

  const features = [
    { icon: Brain, label: 'AI-Matched Tutors', desc: 'Our AI finds the perfect tutor for your learning style', color: 'from-violet-500 to-purple-600' },
    { icon: Video, label: 'Live 1-on-1 Sessions', desc: 'Real-time interactive sessions with HD video', color: 'from-cyan-500 to-blue-600' },
    { icon: Zap, label: 'Instant Booking', desc: 'Book a session in under 60 seconds', color: 'from-orange-500 to-pink-600' },
    { icon: Shield, label: 'Verified Experts', desc: 'Every tutor is background-checked and vetted', color: 'from-emerald-500 to-teal-600' },
    { icon: TrendingUp, label: 'Track Progress', desc: 'AI-powered insights on your learning journey', color: 'from-fuchsia-500 to-violet-600' },
    { icon: Globe, label: 'Learn Anywhere', desc: 'Access world-class tutors from any device', color: 'from-blue-500 to-indigo-600' },
  ]

  return (
    <div className="overflow-hidden bg-[#060612]">

      {/* ── HERO ─────────────────────────────────────────────────────────── */}
      <section className="relative min-h-screen flex items-center py-20 overflow-hidden">
        {/* Animated gradient blobs */}
        <div className="absolute inset-0 overflow-hidden pointer-events-none">
          <div className="absolute top-[-20%] right-[-10%] w-[600px] h-[600px] rounded-full bg-violet-600 opacity-20 blur-[120px] animate-pulse" />
          <div className="absolute bottom-[-20%] left-[-10%] w-[500px] h-[500px] rounded-full bg-cyan-500 opacity-15 blur-[100px] animate-pulse" style={{ animationDelay: '1s' }} />
          <div className="absolute top-[40%] left-[40%] w-[300px] h-[300px] rounded-full bg-fuchsia-600 opacity-10 blur-[80px] animate-pulse" style={{ animationDelay: '2s' }} />
        </div>

        {/* Grid overlay */}
        <div className="absolute inset-0 bg-[linear-gradient(rgba(255,255,255,0.02)_1px,transparent_1px),linear-gradient(90deg,rgba(255,255,255,0.02)_1px,transparent_1px)] bg-[size:64px_64px] pointer-events-none" />

        <div className="relative max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 w-full">
          <div className="grid lg:grid-cols-2 gap-16 items-center">

            {/* Left */}
            <motion.div
              initial={{ opacity: 0, y: 30 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ duration: 0.7 }}
            >
              <motion.div
                initial={{ opacity: 0, scale: 0.9 }}
                animate={{ opacity: 1, scale: 1 }}
                transition={{ delay: 0.2 }}
                className="inline-flex items-center gap-2 bg-white/5 border border-white/10 rounded-full px-4 py-2 mb-8 backdrop-blur-sm"
              >
                <Sparkles className="w-4 h-4 text-violet-400" />
                <span className="text-sm text-white/70 font-medium">AI-Powered Learning Platform</span>
              </motion.div>

              <h1 className="text-5xl md:text-6xl lg:text-7xl font-black mb-6 leading-[1.05] tracking-tight">
                <span className="text-white">Learn Smarter</span>
                <br />
                <span className="bg-gradient-to-r from-violet-400 via-fuchsia-400 to-cyan-400 bg-clip-text text-transparent">
                  with AI Tutors
                </span>
              </h1>

              <p className="text-lg text-white/50 mb-10 leading-relaxed max-w-lg">
                Connect with verified expert tutors in real-time. Personalized 1-on-1 sessions powered by AI matching — built for the next generation of learners.
              </p>

              <div className="flex flex-col sm:flex-row gap-4 mb-12">
                <motion.button
                  whileHover={{ scale: 1.03 }}
                  whileTap={{ scale: 0.97 }}
                  onClick={handleFindTutors}
                  className="inline-flex items-center justify-center gap-2 px-8 py-4 rounded-2xl font-semibold text-base bg-gradient-to-r from-violet-600 to-fuchsia-600 text-white shadow-lg shadow-violet-500/25 hover:shadow-violet-500/40 transition-all duration-200"
                >
                  Find Tutors
                  <ArrowRight className="w-5 h-5" />
                </motion.button>
                <motion.button
                  whileHover={{ scale: 1.03 }}
                  whileTap={{ scale: 0.97 }}
                  onClick={() => navigate('/about-us')}
                  className="inline-flex items-center justify-center gap-2 px-8 py-4 rounded-2xl font-semibold text-base bg-white/5 border border-white/10 text-white hover:bg-white/10 backdrop-blur-sm transition-all duration-200"
                >
                  <Play className="w-4 h-4" />
                  How it Works
                </motion.button>
              </div>

              {/* Social proof */}
              <div className="flex items-center gap-6">
                <div className="flex -space-x-3">
                  {['V', 'A', 'R', 'K', 'S'].map((l, i) => (
                    <div key={i} className="w-9 h-9 rounded-full border-2 border-[#060612] flex items-center justify-center text-xs font-bold text-white"
                      style={{ background: `hsl(${i * 60 + 240}, 70%, 55%)` }}>
                      {l}
                    </div>
                  ))}
                </div>
                <div>
                  <div className="flex items-center gap-1 mb-0.5">
                    {[...Array(5)].map((_, i) => <Star key={i} className="w-3.5 h-3.5 text-yellow-400 fill-yellow-400" />)}
                  </div>
                  <p className="text-xs text-white/50">
                    <span className="text-white font-semibold">{studentCount ?? '50'}+ students</span> trust LiveExpert.AI
                  </p>
                </div>
              </div>
            </motion.div>

            {/* Right — Dashboard Preview */}
            <motion.div
              initial={{ opacity: 0, y: 30 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ duration: 0.7, delay: 0.3 }}
              className="relative"
            >
              <div className="relative bg-white/5 backdrop-blur-xl border border-white/10 rounded-3xl p-6 shadow-2xl">
                {/* Top bar */}
                <div className="flex items-center justify-between mb-6">
                  <div>
                    <p className="text-xs text-white/40 font-medium uppercase tracking-widest mb-1">Dashboard</p>
                    <p className="text-white font-semibold">Welcome back 👋</p>
                  </div>
                  <div className="flex items-center gap-2 bg-emerald-500/10 border border-emerald-500/20 rounded-full px-3 py-1.5">
                    <span className="w-2 h-2 rounded-full bg-emerald-400 animate-pulse" />
                    <span className="text-xs text-emerald-400 font-medium">Session Live</span>
                  </div>
                </div>

                {/* Active session card */}
                <div className="bg-gradient-to-br from-violet-600/20 to-fuchsia-600/20 border border-violet-500/20 rounded-2xl p-4 mb-4">
                  <div className="flex items-center gap-3 mb-4">
                    <div className="w-10 h-10 rounded-xl bg-violet-500/20 border border-violet-500/30 flex items-center justify-center">
                      <BookOpen className="w-5 h-5 text-violet-400" />
                    </div>
                    <div className="flex-1">
                      <p className="text-white font-semibold text-sm">Advanced Mathematics</p>
                      <p className="text-white/40 text-xs">with Dr. Sarah K.</p>
                    </div>
                    <div className="text-right">
                      <p className="text-xs text-white/40">Ends in</p>
                      <p className="text-violet-400 font-bold text-sm">32:15</p>
                    </div>
                  </div>
                  <div className="flex items-center gap-4 text-xs text-white/40">
                    <span className="flex items-center gap-1"><Clock className="w-3 h-3" />60 min</span>
                    <span className="flex items-center gap-1"><Star className="w-3 h-3 text-yellow-400 fill-yellow-400" />4.9</span>
                    <span className="flex items-center gap-1 ml-auto"><CheckCircle className="w-3 h-3 text-emerald-400" /><span className="text-emerald-400">Verified Expert</span></span>
                  </div>
                </div>

                {/* Progress */}
                <div className="bg-white/5 border border-white/5 rounded-2xl p-4 mb-4">
                  <div className="flex items-center justify-between mb-4">
                    <p className="text-xs font-semibold text-white/60 uppercase tracking-widest">Weekly Progress</p>
                    <span className="text-xs text-violet-400 font-medium">+12% this week</span>
                  </div>
                  <div className="space-y-3">
                    {[
                      { label: 'Mathematics', pct: 78, color: 'from-violet-500 to-fuchsia-500' },
                      { label: 'Web Dev', pct: 62, color: 'from-cyan-500 to-blue-500' },
                      { label: 'Data Science', pct: 45, color: 'from-orange-500 to-pink-500' },
                    ].map(item => (
                      <div key={item.label}>
                        <div className="flex justify-between text-xs mb-1.5">
                          <span className="text-white/60">{item.label}</span>
                          <span className="text-white font-medium">{item.pct}%</span>
                        </div>
                        <div className="h-1.5 bg-white/5 rounded-full overflow-hidden">
                          <div className={`h-full bg-gradient-to-r ${item.color} rounded-full`} style={{ width: `${item.pct}%` }} />
                        </div>
                      </div>
                    ))}
                  </div>
                </div>

                {/* AI Insight */}
                <div className="bg-gradient-to-r from-cyan-500/10 to-violet-500/10 border border-cyan-500/20 rounded-2xl p-4 flex items-center gap-3">
                  <div className="w-9 h-9 rounded-xl bg-cyan-500/20 flex items-center justify-center flex-shrink-0">
                    <Brain className="w-4 h-4 text-cyan-400" />
                  </div>
                  <div>
                    <p className="text-white text-xs font-semibold mb-0.5">AI Insight</p>
                    <p className="text-white/40 text-xs">You learn best in 45-min focused sessions</p>
                  </div>
                </div>
              </div>

              {/* Floating badges */}
              <motion.div
                animate={{ y: [-4, 4, -4] }}
                transition={{ duration: 3, repeat: Infinity }}
                className="absolute -top-5 -right-4 bg-gradient-to-r from-violet-600 to-fuchsia-600 text-white rounded-2xl px-4 py-2 shadow-lg shadow-violet-500/30 text-sm font-bold"
              >
                ✦ AI-Powered
              </motion.div>
              <motion.div
                animate={{ y: [4, -4, 4] }}
                transition={{ duration: 3.5, repeat: Infinity }}
                className="absolute -bottom-5 -left-4 bg-white/10 backdrop-blur-xl border border-white/20 text-white rounded-2xl px-4 py-2 shadow-lg text-sm font-medium"
              >
                🎯 {tutorCount ?? '20'}+ Expert Tutors
              </motion.div>
            </motion.div>
          </div>
        </div>
      </section>

      {/* ── STATS ───────────────────────────────────────────────────────── */}
      <section className="py-16 border-t border-white/5">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="grid grid-cols-1 md:grid-cols-3 gap-8">
            {[
              { value: '10+', label: 'Years of Experience', icon: Award },
              { value: studentCount !== null ? `${studentCount}+` : '50+', label: 'Students Enrolled', icon: Users },
              { value: tutorCount !== null ? `${tutorCount}+` : '20+', label: 'Expert Tutors', icon: GraduationCap },
            ].map((stat, i) => (
              <motion.div
                key={i}
                initial={{ opacity: 0, y: 20 }}
                whileInView={{ opacity: 1, y: 0 }}
                viewport={{ once: true }}
                transition={{ delay: i * 0.1 }}
                className="text-center p-8 bg-white/3 border border-white/5 rounded-2xl"
              >
                <stat.icon className="w-7 h-7 text-violet-400 mx-auto mb-4" />
                <p className="text-5xl font-black text-white mb-2">{stat.value}</p>
                <p className="text-white/40 text-sm">{stat.label}</p>
              </motion.div>
            ))}
          </div>
        </div>
      </section>

      {/* ── FEATURES ────────────────────────────────────────────────────── */}
      <section className="py-24">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <motion.div
            initial={{ opacity: 0, y: 20 }}
            whileInView={{ opacity: 1, y: 0 }}
            viewport={{ once: true }}
            className="text-center mb-16"
          >
            <div className="inline-flex items-center gap-2 bg-violet-500/10 border border-violet-500/20 rounded-full px-4 py-2 mb-6">
              <Sparkles className="w-4 h-4 text-violet-400" />
              <span className="text-sm text-violet-300 font-medium">Everything you need</span>
            </div>
            <h2 className="text-4xl md:text-5xl font-black text-white mb-4">
              Built for the <span className="bg-gradient-to-r from-violet-400 to-cyan-400 bg-clip-text text-transparent">next gen</span>
            </h2>
            <p className="text-white/40 text-lg max-w-xl mx-auto">
              AI-powered tools and live expert sessions — everything a modern learner needs to level up fast.
            </p>
          </motion.div>

          <div className="grid md:grid-cols-2 lg:grid-cols-3 gap-5">
            {features.map((f, i) => (
              <motion.div
                key={i}
                initial={{ opacity: 0, y: 20 }}
                whileInView={{ opacity: 1, y: 0 }}
                viewport={{ once: true }}
                transition={{ delay: i * 0.07 }}
                whileHover={{ y: -4 }}
                className="group p-6 bg-white/3 border border-white/8 rounded-2xl hover:bg-white/6 hover:border-white/15 transition-all duration-300 cursor-default"
              >
                <div className={`w-12 h-12 rounded-2xl bg-gradient-to-br ${f.color} flex items-center justify-center mb-4 shadow-lg`}>
                  <f.icon className="w-6 h-6 text-white" />
                </div>
                <h3 className="text-white font-bold text-lg mb-2">{f.label}</h3>
                <p className="text-white/40 text-sm leading-relaxed">{f.desc}</p>
              </motion.div>
            ))}
          </div>
        </div>
      </section>

      {/* ── HOW IT WORKS ────────────────────────────────────────────────── */}
      <section className="py-24 relative overflow-hidden">
        <div className="absolute inset-0 bg-gradient-to-br from-violet-950/50 to-[#060612] pointer-events-none" />
        <div className="relative max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <motion.div
            initial={{ opacity: 0, y: 20 }}
            whileInView={{ opacity: 1, y: 0 }}
            viewport={{ once: true }}
            className="text-center mb-16"
          >
            <h2 className="text-4xl md:text-5xl font-black text-white mb-4">
              Go from zero to <span className="bg-gradient-to-r from-fuchsia-400 to-violet-400 bg-clip-text text-transparent">expert</span>
            </h2>
            <p className="text-white/40 text-lg">Three steps. That's all it takes.</p>
          </motion.div>

          <div className="grid md:grid-cols-3 gap-8">
            {[
              { step: '01', title: 'Create Account', desc: 'Sign up free and get 100 bonus credits instantly. No credit card required.', icon: Zap },
              { step: '02', title: 'Find Your Tutor', desc: 'Browse AI-matched experts or search by subject. Read reviews and book instantly.', icon: Brain },
              { step: '03', title: 'Start Learning', desc: 'Join your HD live session, track progress, and watch your skills skyrocket.', icon: TrendingUp },
            ].map((item, i) => (
              <motion.div
                key={i}
                initial={{ opacity: 0, y: 20 }}
                whileInView={{ opacity: 1, y: 0 }}
                viewport={{ once: true }}
                transition={{ delay: i * 0.15 }}
                className="relative p-8 bg-white/3 border border-white/8 rounded-3xl"
              >
                <div className="text-6xl font-black text-white/5 absolute top-6 right-6">{item.step}</div>
                <div className="w-14 h-14 rounded-2xl bg-gradient-to-br from-violet-500 to-fuchsia-600 flex items-center justify-center mb-6 shadow-lg shadow-violet-500/20">
                  <item.icon className="w-7 h-7 text-white" />
                </div>
                <h3 className="text-white font-bold text-xl mb-3">{item.title}</h3>
                <p className="text-white/40 leading-relaxed">{item.desc}</p>
              </motion.div>
            ))}
          </div>
        </div>
      </section>

      {/* ── TESTIMONIAL ─────────────────────────────────────────────────── */}
      <section className="py-24">
        <div className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8">
          <motion.div
            initial={{ opacity: 0, y: 20 }}
            whileInView={{ opacity: 1, y: 0 }}
            viewport={{ once: true }}
            className="text-center mb-16"
          >
            <div className="inline-flex items-center gap-2 bg-yellow-500/10 border border-yellow-500/20 rounded-full px-4 py-2 mb-6">
              <Star className="w-4 h-4 text-yellow-400 fill-yellow-400" />
              <span className="text-sm text-yellow-300 font-medium">Rated #1 Online Platform</span>
            </div>
            <h2 className="text-4xl md:text-5xl font-black text-white">
              Students <span className="bg-gradient-to-r from-yellow-400 to-orange-400 bg-clip-text text-transparent">love</span> us
            </h2>
          </motion.div>

          <motion.div
            initial={{ opacity: 0, scale: 0.97 }}
            whileInView={{ opacity: 1, scale: 1 }}
            viewport={{ once: true }}
            className="bg-white/5 border border-white/10 rounded-3xl p-10 backdrop-blur-sm"
          >
            <div className="flex gap-1 mb-6">
              {[...Array(5)].map((_, i) => <Star key={i} className="w-5 h-5 text-yellow-400 fill-yellow-400" />)}
            </div>
            <p className="text-xl text-white/80 mb-8 leading-relaxed font-light">
              "LiveExpert.AI completely changed how I learn. The AI matching found me the perfect tutor within seconds — someone who actually understood my learning style. I went from failing calculus to acing my exams in just 4 weeks."
            </p>
            <div className="flex items-center gap-4">
              <div className="w-12 h-12 rounded-full bg-gradient-to-br from-violet-500 to-fuchsia-600 flex items-center justify-center text-white font-bold text-lg">
                P
              </div>
              <div>
                <h4 className="font-bold text-white">Priya Sharma</h4>
                <p className="text-sm text-white/40">Engineering Student, IIT Delhi</p>
              </div>
              <div className="ml-auto hidden sm:block">
                <MessageSquare className="w-8 h-8 text-white/10" />
              </div>
            </div>
          </motion.div>
        </div>
      </section>

      {/* ── FAQ ─────────────────────────────────────────────────────────── */}
      <section className="py-24 bg-white/2">
        <div className="max-w-3xl mx-auto px-4 sm:px-6 lg:px-8">
          <motion.div
            initial={{ opacity: 0, y: 20 }}
            whileInView={{ opacity: 1, y: 0 }}
            viewport={{ once: true }}
            className="text-center mb-16"
          >
            <h2 className="text-4xl md:text-5xl font-black text-white mb-4">
              Got <span className="bg-gradient-to-r from-cyan-400 to-blue-400 bg-clip-text text-transparent">questions?</span>
            </h2>
            <p className="text-white/40 text-lg">Everything you need to know before getting started.</p>
          </motion.div>
          <div className="[&_.accordion-item]:bg-white/3 [&_.accordion-item]:border-white/8 [&_.accordion-item]:rounded-2xl [&_.accordion-item]:mb-3 [&_.accordion-title]:text-white [&_.accordion-content]:text-white/50">
            <Accordion items={faqs} />
          </div>
        </div>
      </section>

      {/* ── CTA ─────────────────────────────────────────────────────────── */}
      <section className="py-24">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <motion.div
            initial={{ opacity: 0, y: 20 }}
            whileInView={{ opacity: 1, y: 0 }}
            viewport={{ once: true }}
            className="text-center mb-12"
          >
            <h2 className="text-4xl md:text-5xl font-black text-white mb-4">Ready to level up?</h2>
            <p className="text-white/40 text-lg">Join as a student or share your expertise as a tutor.</p>
          </motion.div>

          <div className="grid md:grid-cols-2 gap-6">
            {/* Tutor card */}
            <motion.div
              initial={{ opacity: 0, x: -20 }}
              whileInView={{ opacity: 1, x: 0 }}
              viewport={{ once: true }}
              whileHover={{ y: -4 }}
              className="relative overflow-hidden p-10 bg-white/5 border border-white/10 rounded-3xl group cursor-pointer"
              onClick={() => navigate('/register?role=tutor')}
            >
              <div className="absolute top-0 right-0 w-64 h-64 bg-cyan-500/10 rounded-full blur-3xl group-hover:opacity-150 transition-opacity" />
              <div className="relative">
                <div className="w-14 h-14 rounded-2xl bg-gradient-to-br from-cyan-500 to-blue-600 flex items-center justify-center mb-6 shadow-lg">
                  <GraduationCap className="w-7 h-7 text-white" />
                </div>
                <h3 className="text-2xl font-black text-white mb-3">Teach on LiveExpert.AI</h3>
                <p className="text-white/40 mb-8 leading-relaxed">
                  Share your expertise, set your own schedule, and earn competitive income. Join our growing community of top educators.
                </p>
                <div className="inline-flex items-center gap-2 px-6 py-3 rounded-2xl bg-white/10 border border-white/15 text-white font-semibold hover:bg-white/15 transition-all">
                  Become a Tutor
                  <ArrowRight className="w-4 h-4" />
                </div>
              </div>
            </motion.div>

            {/* Student card */}
            <motion.div
              initial={{ opacity: 0, x: 20 }}
              whileInView={{ opacity: 1, x: 0 }}
              viewport={{ once: true }}
              whileHover={{ y: -4 }}
              className="relative overflow-hidden p-10 rounded-3xl group cursor-pointer"
              style={{ background: 'linear-gradient(135deg, #6d28d9 0%, #7c3aed 40%, #a21caf 100%)' }}
              onClick={() => navigate('/register?role=student')}
            >
              <div className="absolute top-0 right-0 w-64 h-64 bg-white/10 rounded-full blur-3xl" />
              <div className="absolute bottom-0 left-0 w-48 h-48 bg-fuchsia-400/20 rounded-full blur-2xl" />
              <div className="relative">
                <div className="w-14 h-14 rounded-2xl bg-white/20 flex items-center justify-center mb-6">
                  <Lightbulb className="w-7 h-7 text-white" />
                </div>
                <h3 className="text-2xl font-black text-white mb-3">Start Learning Today</h3>
                <p className="text-white/70 mb-8 leading-relaxed">
                  Get matched with the perfect tutor in seconds. Live 1-on-1 sessions, AI insights, and 100 free credits on signup.
                </p>
                <div className="inline-flex items-center gap-2 px-6 py-3 rounded-2xl bg-white text-violet-700 font-bold hover:bg-white/90 transition-all shadow-lg">
                  Enroll Free
                  <ArrowRight className="w-4 h-4" />
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
