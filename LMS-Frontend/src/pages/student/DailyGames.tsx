import { useState, useEffect } from 'react'
import { Flame, Trophy, Calendar, Star, ChevronRight, Zap, CheckCircle, Clock, RefreshCw } from 'lucide-react'
import { Card, CardContent } from '../../components/ui/Card'
import { Badge } from '../../components/ui/Badge'
import { Tabs, TabsList, TabsTrigger, TabsContent } from '../../components/ui/Tabs'
import WordScramble from '../../components/games/WordScramble'
import QuizChallenge from '../../components/games/QuizChallenge'
import MatchPairs from '../../components/games/MatchPairs'
import FillBlank from '../../components/games/FillBlank'
import TrueFalse from '../../components/games/TrueFalse'
import CodeBug from '../../components/games/CodeBug'
import {
  getTodayChallenge,
  submitChallenge,
  getChallengeStreak,
  getChallengeHistory,
  getUpcomingChallenges,
  type TodayChallengeResponse,
  type SubmitResult,
  type StreakInfo,
  type HistoryItem,
  type UpcomingChallenge,
  type ChallengeType,
} from '../../services/challengesApi'

// ─── Type label / color helpers ──────────────────────────────────────────────

const TYPE_META: Record<ChallengeType, { label: string; color: string; emoji: string }> = {
  WordScramble: { label: 'Word Scramble', color: 'bg-indigo-100 text-indigo-700', emoji: '🔤' },
  Quiz:         { label: 'Quiz',          color: 'bg-blue-100 text-blue-700',    emoji: '🧠' },
  MatchPairs:   { label: 'Match Pairs',   color: 'bg-emerald-100 text-emerald-700', emoji: '🔗' },
  FillBlank:    { label: 'Fill the Blank',color: 'bg-purple-100 text-purple-700', emoji: '✏️' },
  TrueFalse:    { label: 'True / False',  color: 'bg-amber-100 text-amber-700',  emoji: '⚖️' },
  CodeBug:      { label: 'Find the Bug',  color: 'bg-red-100 text-red-700',      emoji: '🐛' },
}

const RESULT_META = {
  Perfect: { label: 'Perfect!',    color: 'text-yellow-500', bg: 'bg-yellow-50 border-yellow-300' },
  Good:    { label: 'Great job!',  color: 'text-green-600',  bg: 'bg-green-50 border-green-300'  },
  Partial: { label: 'Not bad!',    color: 'text-blue-600',   bg: 'bg-blue-50 border-blue-300'    },
  Failed:  { label: 'Try tomorrow',color: 'text-red-500',    bg: 'bg-red-50 border-red-300'      },
}

// ─── Main component ───────────────────────────────────────────────────────────

export default function DailyGames() {
  const [todayData, setTodayData] = useState<TodayChallengeResponse | null>(null)
  const [streak, setStreak] = useState<StreakInfo | null>(null)
  const [history, setHistory] = useState<HistoryItem[]>([])
  const [upcoming, setUpcoming] = useState<UpcomingChallenge[]>([])
  const [loading, setLoading] = useState(true)
  const [submitting, setSubmitting] = useState(false)
  const [submitResult, setSubmitResult] = useState<SubmitResult | null>(null)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    loadAll()
  }, [])

  const loadAll = async () => {
    setLoading(true)
    setError(null)
    try {
      const [td, sk, hs, up] = await Promise.all([
        getTodayChallenge(),
        getChallengeStreak(),
        getChallengeHistory(1, 10),
        getUpcomingChallenges(7),
      ])
      setTodayData(td)
      setStreak(sk)
      setHistory(hs.data)
      setUpcoming(up)
    } catch (e: any) {
      setError(e.message || 'Failed to load daily challenge')
    } finally {
      setLoading(false)
    }
  }

  const handleSubmit = async (answerJson: string, timeTaken: number) => {
    if (!todayData) return
    setSubmitting(true)
    try {
      const result = await submitChallenge(todayData.challenge.id, answerJson, timeTaken)
      setSubmitResult(result)
      setStreak(result.streak)
      // Reload history
      const hs = await getChallengeHistory(1, 10)
      setHistory(hs.data)
    } catch (e: any) {
      setError(e.message || 'Failed to submit answer')
    } finally {
      setSubmitting(false)
    }
  }

  if (loading) {
    return (
      <div className="max-w-3xl mx-auto px-4 py-12 flex items-center justify-center">
        <RefreshCw className="w-8 h-8 animate-spin text-indigo-600" />
      </div>
    )
  }

  return (
    <div className="max-w-3xl mx-auto px-4 sm:px-6 py-8 space-y-8">
      {/* Header */}
      <div>
        <h1 className="text-3xl font-bold text-gray-900 flex items-center gap-3">
          <span className="text-4xl">🎮</span>
          Daily Challenges
        </h1>
        <p className="text-gray-500 mt-1">A new puzzle every day. Build your streak and earn XP.</p>
      </div>

      {/* Streak Cards */}
      <div className="grid grid-cols-2 sm:grid-cols-4 gap-3">
        <StreakCard
          icon={<Flame className="w-5 h-5 text-orange-500" />}
          label="Current Streak"
          value={`${streak?.currentStreak ?? 0} 🔥`}
          sub="days"
          color="bg-orange-50"
        />
        <StreakCard
          icon={<Trophy className="w-5 h-5 text-yellow-500" />}
          label="Best Streak"
          value={String(streak?.longestStreak ?? 0)}
          sub="days"
          color="bg-yellow-50"
        />
        <StreakCard
          icon={<CheckCircle className="w-5 h-5 text-green-500" />}
          label="Completed"
          value={String(streak?.totalCompleted ?? 0)}
          sub="challenges"
          color="bg-green-50"
        />
        <StreakCard
          icon={<Zap className="w-5 h-5 text-indigo-500" />}
          label="Total XP"
          value={String(streak?.totalXpEarned ?? 0)}
          sub="points"
          color="bg-indigo-50"
        />
      </div>

      {error && (
        <div className="p-4 bg-red-50 border border-red-200 rounded-xl text-red-700 text-sm">{error}</div>
      )}

      <Tabs defaultValue="today">
        <TabsList>
          <TabsTrigger value="today">Today's Challenge</TabsTrigger>
          <TabsTrigger value="upcoming">Upcoming</TabsTrigger>
          <TabsTrigger value="history">History</TabsTrigger>
        </TabsList>

        {/* ── TODAY ─────────────────────────────────────────────────────── */}
        <TabsContent value="today">
          {!todayData ? (
            <Card>
              <CardContent className="py-12 text-center text-gray-500">
                No challenge scheduled for today. Check back tomorrow!
              </CardContent>
            </Card>
          ) : (
            <div className="space-y-4">
              {/* Challenge header */}
              <Card>
                <CardContent className="pt-5">
                  <div className="flex items-start justify-between gap-3 flex-wrap">
                    <div>
                      <div className="flex items-center gap-2 mb-1 flex-wrap">
                        <span className={`text-xs font-semibold px-2.5 py-0.5 rounded-full ${TYPE_META[todayData.challenge.type].color}`}>
                          {TYPE_META[todayData.challenge.type].emoji} {TYPE_META[todayData.challenge.type].label}
                        </span>
                        <Badge variant={todayData.challenge.difficulty === 'Easy' ? 'success' : todayData.challenge.difficulty === 'Hard' ? 'error' : 'warning'}>
                          {todayData.challenge.difficulty}
                        </Badge>
                        <span className="text-xs bg-gray-100 text-gray-500 px-2 py-0.5 rounded-full">
                          #{todayData.challenge.tag}
                        </span>
                      </div>
                      <h2 className="text-xl font-bold text-gray-900">{todayData.challenge.title}</h2>
                      <p className="text-gray-500 text-sm mt-0.5">{todayData.challenge.description}</p>
                    </div>
                    <div className="flex items-center gap-1.5 bg-indigo-50 px-3 py-1.5 rounded-full">
                      <Star className="w-4 h-4 text-indigo-500" />
                      <span className="text-sm font-bold text-indigo-700">+{todayData.challenge.xpReward} XP</span>
                    </div>
                  </div>
                </CardContent>
              </Card>

              {/* Already completed — show result */}
              {(todayData.alreadyCompleted || submitResult) ? (
                <ResultPanel
                  attempt={submitResult
                    ? { score: submitResult.score, result: submitResult.result, timeTakenSeconds: todayData.attempt?.timeTakenSeconds ?? 0 }
                    : todayData.attempt}
                  correctJson={submitResult?.correctAnswerJson}
                  streak={submitResult?.streak ?? streak}
                  challengeType={todayData.challenge.type}
                />
              ) : (
                /* Game area */
                <Card>
                  <CardContent className="pt-5">
                    <GameComponent
                      type={todayData.challenge.type}
                      contentJson={todayData.challenge.contentJson}
                      onSubmit={handleSubmit}
                      disabled={submitting}
                    />
                    {submitting && (
                      <div className="flex items-center gap-2 justify-center mt-4 text-indigo-600">
                        <RefreshCw className="w-4 h-4 animate-spin" />
                        Checking your answer…
                      </div>
                    )}
                  </CardContent>
                </Card>
              )}
            </div>
          )}
        </TabsContent>

        {/* ── UPCOMING ──────────────────────────────────────────────────── */}
        <TabsContent value="upcoming">
          <div className="space-y-2">
            {upcoming.length === 0 ? (
              <Card>
                <CardContent className="py-10 text-center text-gray-500">No upcoming challenges found.</CardContent>
              </Card>
            ) : (
              upcoming.map(ch => {
                const meta = TYPE_META[ch.type]
                const dateLabel = ch.isToday ? 'Today' : new Date(ch.challengeDate).toLocaleDateString('en-IN', { weekday: 'short', month: 'short', day: 'numeric' })
                return (
                  <div key={ch.id}
                    className={`flex items-center justify-between p-4 rounded-xl border ${ch.isToday ? 'border-indigo-300 bg-indigo-50' : 'border-gray-200 bg-white'}`}>
                    <div className="flex items-center gap-3">
                      <span className="text-2xl">{meta.emoji}</span>
                      <div>
                        <p className="font-semibold text-gray-900 text-sm">{ch.title}</p>
                        <div className="flex items-center gap-2 mt-0.5">
                          <span className={`text-xs font-medium px-2 py-0.5 rounded-full ${meta.color}`}>{meta.label}</span>
                          <span className="text-xs text-gray-400">#{ch.tag}</span>
                        </div>
                      </div>
                    </div>
                    <div className="text-right shrink-0 ml-3">
                      <p className="text-xs font-semibold text-gray-600">{dateLabel}</p>
                      <p className="text-xs text-indigo-600 font-medium mt-0.5">+{ch.xpReward} XP</p>
                    </div>
                  </div>
                )
              })
            )}
          </div>
        </TabsContent>

        {/* ── HISTORY ───────────────────────────────────────────────────── */}
        <TabsContent value="history">
          {history.length === 0 ? (
            <Card>
              <CardContent className="py-12 text-center">
                <Trophy className="w-12 h-12 mx-auto mb-3 text-gray-300" />
                <p className="text-gray-500">No challenges completed yet.</p>
                <p className="text-gray-400 text-sm mt-1">Complete today's challenge to start your streak!</p>
              </CardContent>
            </Card>
          ) : (
            <div className="space-y-2">
              {history.map(item => {
                const meta = TYPE_META[item.challenge.type]
                const resMeta = RESULT_META[item.result]
                return (
                  <div key={item.id} className="flex items-center gap-4 p-4 bg-white rounded-xl border border-gray-200 hover:border-gray-300 transition-colors">
                    <span className="text-2xl">{meta.emoji}</span>
                    <div className="flex-1 min-w-0">
                      <p className="font-semibold text-gray-900 text-sm truncate">{item.challenge.title}</p>
                      <div className="flex items-center gap-2 mt-0.5 flex-wrap">
                        <span className={`text-xs px-2 py-0.5 rounded-full ${meta.color}`}>{meta.label}</span>
                        <span className="text-xs text-gray-400">
                          {new Date(item.completedAt).toLocaleDateString('en-IN', { month: 'short', day: 'numeric' })}
                        </span>
                        <span className="flex items-center gap-1 text-xs text-gray-400">
                          <Clock className="w-3 h-3" />{item.timeTakenSeconds}s
                        </span>
                      </div>
                    </div>
                    <div className="text-right shrink-0">
                      <p className={`font-bold text-sm ${resMeta.color}`}>{item.score}%</p>
                      <p className="text-xs text-indigo-600 font-medium">+{Math.round(item.challenge.xpReward * item.score / 100)} XP</p>
                    </div>
                  </div>
                )
              })}
            </div>
          )}
        </TabsContent>
      </Tabs>
    </div>
  )
}

// ─── Sub-components ───────────────────────────────────────────────────────────

function StreakCard({ icon, label, value, sub, color }: {
  icon: React.ReactNode; label: string; value: string; sub: string; color: string
}) {
  return (
    <div className={`${color} rounded-xl p-3.5`}>
      <div className="flex items-center gap-1.5 mb-1">{icon}<span className="text-xs text-gray-500 font-medium">{label}</span></div>
      <p className="text-xl font-bold text-gray-900">{value}</p>
      <p className="text-xs text-gray-400">{sub}</p>
    </div>
  )
}

function GameComponent({ type, contentJson, onSubmit, disabled }: {
  type: ChallengeType
  contentJson: string
  onSubmit: (a: string, t: number) => void
  disabled: boolean
}) {
  switch (type) {
    case 'WordScramble': return <WordScramble contentJson={contentJson} onSubmit={onSubmit} disabled={disabled} />
    case 'Quiz':         return <QuizChallenge contentJson={contentJson} onSubmit={onSubmit} disabled={disabled} />
    case 'MatchPairs':   return <MatchPairs contentJson={contentJson} onSubmit={onSubmit} disabled={disabled} />
    case 'FillBlank':    return <FillBlank contentJson={contentJson} onSubmit={onSubmit} disabled={disabled} />
    case 'TrueFalse':    return <TrueFalse contentJson={contentJson} onSubmit={onSubmit} disabled={disabled} />
    case 'CodeBug':      return <CodeBug contentJson={contentJson} onSubmit={onSubmit} disabled={disabled} />
    default:             return <p className="text-gray-500">Unknown challenge type.</p>
  }
}

function ResultPanel({ attempt, correctJson, streak, challengeType }: {
  attempt: { score: number; result: string; timeTakenSeconds: number } | null
  correctJson?: string
  streak?: StreakInfo | null
  challengeType: ChallengeType
}) {
  if (!attempt) return null
  const result = (attempt.result ?? 'Failed') as keyof typeof RESULT_META
  const meta = RESULT_META[result] ?? RESULT_META.Failed
  const score = attempt.score ?? 0

  // Parse correct answer for display
  let correctDisplay: React.ReactNode = null
  if (correctJson) {
    try {
      const parsed = JSON.parse(correctJson)
      if (challengeType === 'WordScramble' && parsed.word) {
        correctDisplay = <p>The word was: <strong className="text-emerald-700 text-lg tracking-widest">{parsed.word}</strong></p>
      } else if ((challengeType === 'Quiz' || challengeType === 'CodeBug') && parsed.correct !== undefined) {
        correctDisplay = <p>Correct option index: <strong>{String.fromCharCode(65 + parsed.correct)}</strong></p>
      } else if (challengeType === 'FillBlank' && parsed.answer) {
        correctDisplay = <p>The answer was: <strong className="text-emerald-700">{parsed.answer}</strong></p>
      } else if (challengeType === 'TrueFalse' && parsed.answers) {
        correctDisplay = (
          <div>
            <p className="font-medium mb-1.5">Correct answers:</p>
            <div className="flex flex-wrap gap-2">
              {Object.entries(parsed.answers as Record<string, boolean>).map(([id, val]) => (
                <span key={id} className={`px-2 py-0.5 rounded-full text-xs font-semibold ${val ? 'bg-green-100 text-green-700' : 'bg-red-100 text-red-700'}`}>
                  #{id}: {val ? 'True' : 'False'}
                </span>
              ))}
            </div>
          </div>
        )
      } else if (challengeType === 'MatchPairs' && parsed.matches) {
        correctDisplay = <p className="text-xs text-gray-500">See correct pairings above.</p>
      }
    } catch { /* ignore */ }
  }

  return (
    <Card className={`border-2 ${meta.bg}`}>
      <CardContent className="pt-6 space-y-5">
        {/* Score ring */}
        <div className="flex items-center gap-6">
          <div className="relative w-20 h-20 flex-shrink-0">
            <svg className="w-20 h-20 -rotate-90" viewBox="0 0 80 80">
              <circle cx="40" cy="40" r="34" fill="none" stroke="#e5e7eb" strokeWidth="8" />
              <circle cx="40" cy="40" r="34" fill="none"
                stroke={score === 100 ? '#f59e0b' : score >= 70 ? '#22c55e' : score >= 40 ? '#3b82f6' : '#ef4444'}
                strokeWidth="8" strokeLinecap="round"
                strokeDasharray={`${2 * Math.PI * 34 * score / 100} ${2 * Math.PI * 34 * (1 - score / 100)}`}
              />
            </svg>
            <span className="absolute inset-0 flex items-center justify-center font-bold text-lg text-gray-900">{score}%</span>
          </div>
          <div>
            <p className={`text-2xl font-bold ${meta.color}`}>{meta.label}</p>
            <p className="text-gray-500 text-sm mt-0.5 flex items-center gap-1.5">
              <Clock className="w-3.5 h-3.5" /> Completed in {attempt.timeTakenSeconds}s
            </p>
            {streak && (
              <div className="flex items-center gap-3 mt-2">
                <span className="flex items-center gap-1 text-sm font-semibold text-orange-600">
                  <Flame className="w-4 h-4" />{streak.currentStreak} day streak
                </span>
                {score >= 40 && (
                  <span className="flex items-center gap-1 text-sm font-semibold text-indigo-600">
                    <Zap className="w-4 h-4" />+{Math.round(10 * score / 100)} XP earned
                  </span>
                )}
              </div>
            )}
          </div>
        </div>

        {/* Correct answer reveal */}
        {correctDisplay && (
          <div className="p-3 bg-white/70 rounded-lg border border-emerald-200 text-sm text-gray-700">
            {correctDisplay}
          </div>
        )}

        <div className="flex items-center gap-2 p-3 bg-white/70 rounded-lg border border-gray-200 text-sm text-gray-600">
          <Calendar className="w-4 h-4 text-indigo-500 flex-shrink-0" />
          Come back tomorrow for a new challenge and keep your streak going!
        </div>

        {/* Next challenge teaser */}
        <div className="text-xs text-gray-400 flex items-center gap-1">
          <ChevronRight className="w-3.5 h-3.5" />
          Check the <strong>Upcoming</strong> tab to see what's next.
        </div>
      </CardContent>
    </Card>
  )
}
