import { apiGet, apiPost } from './api'

// ─── Types ────────────────────────────────────────────────────────────────────

export type ChallengeType =
  | 'WordScramble'
  | 'Quiz'
  | 'MatchPairs'
  | 'FillBlank'
  | 'TrueFalse'
  | 'CodeBug'

export type AttemptResult = 'Perfect' | 'Good' | 'Partial' | 'Failed'

export interface DailyChallenge {
  id: string
  title: string
  description: string
  type: ChallengeType
  contentJson: string
  xpReward: number
  difficulty: string
  tag: string
  challengeDate: string
}

export interface AttemptSummary {
  score: number
  result: AttemptResult
  timeTakenSeconds: number
  completedAt: string
}

export interface TodayChallengeResponse {
  challenge: DailyChallenge
  alreadyCompleted: boolean
  attempt: AttemptSummary | null
}

export interface SubmitResult {
  score: number
  result: AttemptResult
  xpEarned: number
  correctAnswerJson: string
  streak: StreakInfo
}

export interface StreakInfo {
  currentStreak: number
  longestStreak: number
  totalCompleted: number
  totalXpEarned: number
  lastCompletedDate: string | null
  completedToday: boolean
}

export interface HistoryItem {
  id: string
  score: number
  result: AttemptResult
  timeTakenSeconds: number
  completedAt: string
  challenge: {
    id: string
    title: string
    tag: string
    difficulty: string
    type: ChallengeType
    xpReward: number
    challengeDate: string
  }
}

export interface HistoryResponse {
  data: HistoryItem[]
  total: number
  page: number
  pageSize: number
}

export interface UpcomingChallenge {
  id: string
  title: string
  tag: string
  difficulty: string
  type: ChallengeType
  xpReward: number
  challengeDate: string
  isToday: boolean
}

// ─── API Functions ────────────────────────────────────────────────────────────

export const getTodayChallenge = (): Promise<TodayChallengeResponse> =>
  apiGet<TodayChallengeResponse>('/challenges/today')

export const submitChallenge = (
  id: string,
  answerJson: string,
  timeTakenSeconds: number
): Promise<SubmitResult> =>
  apiPost<SubmitResult>(`/challenges/${id}/submit`, { answerJson, timeTakenSeconds })

export const getChallengeStreak = (): Promise<StreakInfo> =>
  apiGet<StreakInfo>('/challenges/streak')

export const getChallengeHistory = (
  page = 1,
  pageSize = 10
): Promise<HistoryResponse> =>
  apiGet<HistoryResponse>(`/challenges/history?page=${page}&pageSize=${pageSize}`)

export const getUpcomingChallenges = (days = 7): Promise<UpcomingChallenge[]> =>
  apiGet<UpcomingChallenge[]>(`/challenges/upcoming?days=${days}`)
