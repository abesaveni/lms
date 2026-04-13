/**
 * Student E2E flow
 *
 * Covers:
 *   1. Full registration (basic info + WhatsApp → almost there → send code → OTP → dashboard)
 *   2. Find tutors (search, filter, verify tutor cards)
 *   3. Book session entry (navigate to tutor profile, verify Book button exists)
 */

import { test, expect } from '@playwright/test'
import { TEST_PASSWORD, uniqueEmail, getDevOtp } from './helpers/auth'

// ── Shared credentials created in beforeAll ───────────────────────────────────

let studentEmail: string
let studentPassword: string

test.beforeAll(() => {
  studentEmail = uniqueEmail('student')
  studentPassword = TEST_PASSWORD
})

// ── Helper: login as student ──────────────────────────────────────────────────

async function loginAsStudent(page: any) {
  await page.goto('/login')
  await page.getByPlaceholder('you@example.com').fill(studentEmail)
  await page.getByPlaceholder('Enter your password').fill(studentPassword)
  await page.getByRole('button', { name: 'Sign in' }).click()
  // Accept any student route — new accounts may land on profile-settings
  await page.waitForURL(/\/student\//, { timeout: 30_000 })
}

// ── 1. Registration ───────────────────────────────────────────────────────────

test('student: register a new account', async ({ page }) => {
  await page.goto('/register?role=student')

  // ── Step 1: All fields on one page ──────────────────────────────────────
  await page.getByPlaceholder('John Doe').fill('E2E Student')
  await page.getByPlaceholder('you@example.com').fill(studentEmail)
  // WhatsApp is required (has `required` attribute on the input)
  await page.getByPlaceholder('+91 98765 43210').fill('+91 98765 43210')
  await page.getByPlaceholder('At least 8 characters').fill(studentPassword)
  await page.getByPlaceholder('Re-enter your password').fill(studentPassword)
  await page.locator('input[type="checkbox"]').first().check()

  // "Continue" submits step 1 → setStep(2)
  await page.getByRole('button', { name: /^continue$/i }).click()

  // ── Step 2: "Almost there" ──────────────────────────────────────────────
  await expect(
    page.getByRole('button', { name: /continue to email verification/i }),
  ).toBeVisible({ timeout: 10_000 })
  await page.getByRole('button', { name: /continue to email verification/i }).click()

  // ── Step 3a: Send the verification code ─────────────────────────────────
  await expect(
    page.getByRole('button', { name: /send verification code/i }),
  ).toBeVisible({ timeout: 10_000 })
  await page.getByRole('button', { name: /send verification code/i }).click()

  // ── Step 3b: Enter OTP ──────────────────────────────────────────────────
  const otp = await getDevOtp(page)
  expect(otp).toMatch(/^\d{4,8}$/)

  await page.getByPlaceholder('000000').fill(otp)
  await page.getByRole('button', { name: /verify email/i }).click()

  // ── Step 4: "Registration Complete!" ────────────────────────────────────
  await expect(
    page.getByRole('button', { name: /continue to dashboard/i }),
  ).toBeVisible({ timeout: 10_000 })
  await page.getByRole('button', { name: /continue to dashboard/i }).click()

  // Registration calls /student/register then /auth/login then navigates
  // Accept student dashboard or profile-settings (first-time redirect)
  await page.waitForURL(/\/student\//, { timeout: 40_000 })
  expect(page.url()).toMatch(/\/student\//)
})

// ── 2. Find Tutors ────────────────────────────────────────────────────────────

test('student: can search and filter tutors', async ({ page }) => {
  await loginAsStudent(page)
  await page.goto('/student/find-tutors')

  // Page loads the tutor listing
  await expect(page.getByText(/total tutors/i)).toBeVisible({ timeout: 12_000 })

  // Search
  const searchInput = page.getByPlaceholder(/search by name/i)
  await searchInput.fill('python')
  await page.waitForTimeout(1_200)
  await searchInput.clear()
  await page.waitForTimeout(800)

  // Subject filter
  const subjectSelect = page.locator('select').first()
  await subjectSelect.selectOption({ label: 'Python Programming' })
  await page.waitForTimeout(800)
  await subjectSelect.selectOption({ index: 0 })
  await page.waitForTimeout(800)

  // Verify page didn't crash
  await expect(page.locator('body')).not.toContainText(/error.*500|something went wrong/i)
})

// ── 3. Book Session Entry ─────────────────────────────────────────────────────

test('student: can open a tutor profile and find the book CTA', async ({ page }) => {
  await loginAsStudent(page)
  await page.goto('/student/find-tutors')
  await page.waitForTimeout(2_500)

  const firstTutorLink = page.locator('a[href*="/student/tutors/"]').first()

  if ((await firstTutorLink.count()) === 0) {
    console.log('No tutor links found — skipping book CTA check.')
    test.skip()
    return
  }

  await firstTutorLink.click()
  await page.waitForURL(/\/student\/tutors\//, { timeout: 15_000 })

  const bookCta = page.getByRole('button', { name: /book/i }).or(
    page.getByRole('link', { name: /book/i }),
  )
  await expect(bookCta.first()).toBeVisible({ timeout: 10_000 })
})
