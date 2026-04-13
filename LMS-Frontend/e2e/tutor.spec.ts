/**
 * Tutor E2E flow
 *
 * Covers:
 *   1. Full tutor registration — 6 steps:
 *        1 Account → 2 Resume/Profile → 3 Profile Details → 4 Resume Upload
 *        → 5 Email Verification → 6 Submit
 *   2. Onboarding — 3 steps (account confirmed → profile → govt ID → verification pending)
 */

import { test, expect } from '@playwright/test'
import path from 'path'
import fs from 'fs'
import { fileURLToPath } from 'url'
import { TEST_PASSWORD, uniqueEmail, getDevOtp } from './helpers/auth'

const __filename = fileURLToPath(import.meta.url)
const __dirname = path.dirname(__filename)

// ── Fixtures ──────────────────────────────────────────────────────────────────

const FIXTURE_DIR = path.join(__dirname, 'fixtures')
const GOVT_ID_PATH = path.join(FIXTURE_DIR, 'govt-id.png')
const RESUME_PATH = path.join(FIXTURE_DIR, 'resume.txt')

// 1×1 white pixel PNG
const TINY_PNG_B64 =
  'iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z8BQDwADhQGAWjR9awAAAABJRU5ErkJggg=='

// ── Shared state ──────────────────────────────────────────────────────────────

let tutorEmail: string
let tutorPassword: string

test.beforeAll(() => {
  tutorEmail = uniqueEmail('tutor')
  tutorPassword = TEST_PASSWORD

  if (!fs.existsSync(FIXTURE_DIR)) fs.mkdirSync(FIXTURE_DIR, { recursive: true })
  if (!fs.existsSync(GOVT_ID_PATH))
    fs.writeFileSync(GOVT_ID_PATH, Buffer.from(TINY_PNG_B64, 'base64'))
  // resume.txt is checked into fixtures — but create it if missing
  if (!fs.existsSync(RESUME_PATH))
    fs.writeFileSync(RESUME_PATH, 'E2E Test Resume\nSkills: TypeScript, React\nExperience: 5 years')
})

// ── 1. Registration ───────────────────────────────────────────────────────────

test('tutor: register a new account (6-step flow)', async ({ page }) => {
  await page.goto('/register?role=tutor')

  // ── Step 1 of 6: Account ────────────────────────────────────────────────
  await page.getByPlaceholder('John Doe').fill('E2E Tutor')
  await page.getByPlaceholder('you@example.com').fill(tutorEmail)
  await page.getByPlaceholder('At least 8 characters').fill(tutorPassword)
  await page.getByPlaceholder('Re-enter your password').fill(tutorPassword)
  await page.locator('input[type="checkbox"]').first().check()

  await page.getByRole('button', { name: /create tutor account/i }).click()

  // ── Step 2 of 6: Resume / Profile ───────────────────────────────────────
  await expect(page.getByText(/fill details manually/i)).toBeVisible({ timeout: 10_000 })
  await page.getByText(/fill details manually/i).click()
  await page.getByRole('button', { name: /^continue$/i }).click()

  // ── Step 3 of 6: Profile Details ────────────────────────────────────────
  const professionInput = page.getByPlaceholder('e.g., Vocal Coach, Fitness Trainer, UI/UX Designer')
  await expect(professionInput).toBeVisible({ timeout: 10_000 })
  await professionInput.fill('Software Engineer')

  await page.getByPlaceholder('Singing, Breathwork, Stage Performance').fill('TypeScript, React, Node.js')

  const experienceTextarea = page.getByPlaceholder(/describe your professional experience/i)
  await experienceTextarea.fill('5 years of full-stack development experience in web technologies.')

  const bioTextarea = page.getByPlaceholder(/background and style|tell students about/i)
  if (await bioTextarea.isVisible({ timeout: 2_000 }).catch(() => false)) {
    await bioTextarea.fill('Experienced engineer passionate about teaching modern web development.')
  }

  await page.getByRole('button', { name: /^continue$/i }).click()

  // ── Step 4 of 6: Resume Upload (required) ───────────────────────────────
  await expect(
    page.getByRole('button', { name: /continue to email verification/i }),
  ).toBeVisible({ timeout: 10_000 })

  // Upload the resume file — the file input for step 4
  const resumeInput = page.locator('input[type="file"]').last()
  await resumeInput.setInputFiles(RESUME_PATH)
  await page.waitForTimeout(500)

  await page.getByRole('button', { name: /continue to email verification/i }).click()

  // ── Step 5 of 6: Email Verification ─────────────────────────────────────
  // Two sub-states: !codeSent → "Send Verification Code", codeSent → OTP input

  const sendCodeBtn = page.getByRole('button', { name: /send verification code/i })
  if (await sendCodeBtn.isVisible({ timeout: 8_000 }).catch(() => false)) {
    await sendCodeBtn.click()
  }

  // OTP box appears in dev mode
  const otp = await getDevOtp(page)
  expect(otp).toMatch(/^\d{4,8}$/)

  await page.getByPlaceholder('000000').fill(otp)
  await page.getByRole('button', { name: /verify email/i }).click()

  // ── Step 6 of 6: Submit for Verification ────────────────────────────────
  await expect(
    page.getByRole('button', { name: /submit profile for verification/i }),
  ).toBeVisible({ timeout: 10_000 })
  await page.getByRole('button', { name: /submit profile for verification/i }).click()

  // Should redirect to a tutor route (dashboard, verification-pending, or onboarding)
  await page.waitForURL(/\/tutor\//, { timeout: 40_000 })
  expect(page.url()).toMatch(/\/tutor\//)
})

// ── 2. Onboarding ─────────────────────────────────────────────────────────────

test('tutor: complete onboarding and reach verification pending', async ({ page }) => {
  await page.goto('/login')
  await page.getByPlaceholder('you@example.com').fill(tutorEmail)
  await page.getByPlaceholder('Enter your password').fill(tutorPassword)
  await page.getByRole('button', { name: 'Sign in' }).click()
  await page.waitForURL(/\/tutor\//, { timeout: 30_000 })

  await page.goto('/tutor/onboarding')
  await expect(page).toHaveURL(/\/tutor\/onboarding/)

  // ── Onboarding Step 1: Account Created ──────────────────────────────────
  await expect(page.getByText('Account Created!')).toBeVisible({ timeout: 8_000 })
  await page.getByRole('button', { name: /continue to profile/i }).click()

  // ── Onboarding Step 2: Profile (resume upload or manual) ────────────────
  await expect(
    page.getByText(/build your profile from resume|your tutor profile/i).first(),
  ).toBeVisible({ timeout: 8_000 })

  // Switch to manual entry if on upload mode
  const fillManuallyLink = page.getByText(/fill in manually/i)
  if (await fillManuallyLink.isVisible({ timeout: 3_000 }).catch(() => false)) {
    await fillManuallyLink.click()
  }

  // Fill required fields — use placeholder because labels lack htmlFor association
  const skillsField = page.getByPlaceholder(/react.*typescript.*python|e\.g\.,.*react/i)
  await skillsField.waitFor({ state: 'visible', timeout: 8_000 })
  await skillsField.fill('TypeScript, React, Teaching')

  const expField = page.getByPlaceholder(/describe your professional experience/i)
  await expField.fill('5 years of software engineering and online teaching.')

  await page.getByRole('button', { name: /continue/i }).click()

  // ── Onboarding Step 3: Identity Verification ─────────────────────────────
  await expect(page.getByText('Identity Verification')).toBeVisible({ timeout: 8_000 })

  await page.locator('input[type="file"]').setInputFiles(GOVT_ID_PATH)
  await expect(page.getByText('govt-id.png')).toBeVisible({ timeout: 5_000 })

  await page.getByRole('button', { name: /submit for verification/i }).click()

  // Should land on verification-pending
  await page.waitForURL(/\/tutor\/verification-pending/, { timeout: 30_000 })
  await expect(page).toHaveURL(/\/tutor\/verification-pending/)
  await expect(
    page.getByText(/pending|under review|submitted/i).first(),
  ).toBeVisible({ timeout: 8_000 })
})
