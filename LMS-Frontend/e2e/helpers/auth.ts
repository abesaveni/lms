import { type Page, expect } from '@playwright/test'

// ── Credentials ──────────────────────────────────────────────────────────────

export const TEST_PASSWORD = 'TestPass123!'

/** Admin credentials — override via environment variables in CI */
export const ADMIN_EMAIL = process.env.E2E_ADMIN_EMAIL || 'superadmin@liveexpert.ai'
export const ADMIN_PASSWORD = process.env.E2E_ADMIN_PASSWORD || 'SuperAdmin@2026!'

// ── Utilities ─────────────────────────────────────────────────────────────────

/** Generate a unique email so each test run creates a fresh user */
export function uniqueEmail(prefix: string): string {
  return `${prefix}.${Date.now()}@e2etest.invalid`
}

/**
 * Read the OTP shown in the dev-mode blue info box.
 * The backend returns devOtp when email is not configured.
 * Selector: the <span> with class font-mono inside the bg-blue-50 box.
 */
export async function getDevOtp(page: Page): Promise<string> {
  const otpSpan = page.locator('.bg-blue-50 .font-mono').first()
  await otpSpan.waitFor({ state: 'visible', timeout: 15_000 })
  const otp = await otpSpan.innerText()
  return otp.trim()
}

/**
 * Log in through the UI.
 * Waits for redirect to a role-specific dashboard route.
 */
export async function loginViaUI(
  page: Page,
  email: string,
  password: string,
): Promise<void> {
  await page.goto('/login')
  await page.getByPlaceholder('you@example.com').fill(email)
  await page.getByPlaceholder('Enter your password').fill(password)
  await page.getByRole('button', { name: 'Sign in' }).click()
  await page.waitForURL(/\/(student|tutor|admin)\//, { timeout: 20_000 })
}

/**
 * Inject a pre-built auth state directly into localStorage.
 * Use this in beforeEach to skip the login UI when the test does not
 * need to exercise the login flow itself.
 */
export async function injectAuthState(
  page: Page,
  token: string,
  user: Record<string, unknown>,
): Promise<void> {
  await page.addInitScript(
    ({ token, user }) => {
      localStorage.setItem('token', token)
      localStorage.setItem('user', JSON.stringify(user))
    },
    { token, user },
  )
}
