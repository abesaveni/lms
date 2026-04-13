/**
 * Admin E2E flow
 *
 * Covers:
 *   1. Admin login → redirect to /admin/dashboard
 *   2. Navigate to tutor verification page
 *   3. Approve first pending tutor (if any)
 *   4. Verify the tutor moves to the Verified tab
 *
 * Prerequisites:
 *   - Backend running on http://localhost:5128
 *   - Frontend dev server running on http://localhost:5173
 *   - Admin account exists (set E2E_ADMIN_EMAIL / E2E_ADMIN_PASSWORD env vars,
 *     or defaults below will be used)
 *
 * Note:
 *   If no pending tutors exist, the approval test is skipped gracefully.
 *   Run tutor.spec.ts first to ensure a pending tutor is present.
 */

import { test, expect } from '@playwright/test'
import { ADMIN_EMAIL, ADMIN_PASSWORD } from './helpers/auth'

// ── 1. Login ──────────────────────────────────────────────────────────────────

test('admin: login and reach dashboard', async ({ page }) => {
  await page.goto('/login')

  await page.getByPlaceholder('you@example.com').fill(ADMIN_EMAIL)
  await page.getByPlaceholder('Enter your password').fill(ADMIN_PASSWORD)
  await page.getByRole('button', { name: 'Sign in' }).click()

  // Admin should be redirected to /admin/dashboard
  await page.waitForURL(/\/admin\/dashboard/, { timeout: 20_000 })
  await expect(page).toHaveURL(/\/admin\/dashboard/)

  // Verify admin dashboard has some expected content
  await expect(
    page.getByText(/dashboard|admin|overview/i).first(),
  ).toBeVisible({ timeout: 8_000 })
})

// ── 2. Tutor Verification Page ────────────────────────────────────────────────

test('admin: can view tutor verification page', async ({ page }) => {
  // Log in
  await page.goto('/login')
  await page.getByPlaceholder('you@example.com').fill(ADMIN_EMAIL)
  await page.getByPlaceholder('Enter your password').fill(ADMIN_PASSWORD)
  await page.getByRole('button', { name: 'Sign in' }).click()
  await page.waitForURL(/\/admin\/dashboard/, { timeout: 20_000 })

  // Navigate via sidebar link (SPA navigation) rather than full-page goto when possible
  const sidebarLink = page.locator('a[href*="tutor-verification"]').first()
  if (await sidebarLink.isVisible({ timeout: 3_000 }).catch(() => false)) {
    await sidebarLink.click()
    await page.waitForURL(/\/admin\/tutor-verification/, { timeout: 10_000 })
  } else {
    await page.goto('/admin/tutor-verification')
    await page.waitForURL(/\/admin\/tutor-verification/, { timeout: 10_000 })
  }

  // Wait for React to finish rendering (API calls to settle)
  await page.waitForLoadState('networkidle')

  // Confirm the h1 page heading renders (sidebar link is a <span>, not a heading)
  await expect(page.getByRole('heading', { name: /tutor verification/i })).toBeVisible({ timeout: 10_000 })

  // Both tab buttons should be present (custom Tabs renders plain <button>, no role="tab")
  const pendingTab = page.getByRole('button', { name: /pending/i }).first()
  await expect(pendingTab).toBeVisible({ timeout: 5_000 })
})

// ── 3. Approve a Pending Tutor ────────────────────────────────────────────────

test('admin: approve first pending tutor', async ({ page }) => {
  // Log in
  await page.goto('/login')
  await page.getByPlaceholder('you@example.com').fill(ADMIN_EMAIL)
  await page.getByPlaceholder('Enter your password').fill(ADMIN_PASSWORD)
  await page.getByRole('button', { name: 'Sign in' }).click()
  await page.waitForURL(/\/admin\/dashboard/, { timeout: 20_000 })

  await page.goto('/admin/tutor-verification')

  // Click the Pending tab to make sure we're on it
  const pendingTabBtn = page.getByRole('tab', { name: /pending/i })
    .or(page.getByRole('button', { name: /pending/i }))
  if (await pendingTabBtn.first().isVisible({ timeout: 5_000 }).catch(() => false)) {
    await pendingTabBtn.first().click()
  }
  await page.waitForTimeout(1_500) // let the list load

  // Find the first Approve button
  const approveBtn = page.getByRole('button', { name: /approve/i }).first()
  const hasPending = await approveBtn.isVisible({ timeout: 5_000 }).catch(() => false)

  if (!hasPending) {
    console.log('No pending tutors — skipping approval step.')
    test.skip()
    return
  }

  await approveBtn.click()

  // Confirmation dialog: "Approve this tutor? They will be able to create sessions."
  const confirmBtn = page
    .getByRole('button', { name: /approve/i })
    .or(page.getByRole('button', { name: /confirm/i }))
    .last()

  if (await confirmBtn.isVisible({ timeout: 3_000 }).catch(() => false)) {
    await confirmBtn.click()
  }

  // The tutor card should disappear from the Pending list (or show success toast)
  await page.waitForTimeout(2_000)

  // Switch to Verified tab and confirm at least one approved tutor exists
  const verifiedTabBtn = page.getByRole('tab', { name: /verified/i })
    .or(page.getByRole('button', { name: /verified/i }))
  await verifiedTabBtn.first().click()
  await page.waitForTimeout(1_500)

  // Verified list should have at least one entry
  const verifiedCount = await page.locator('[class*="card"], [class*="Card"]').count()
  expect(verifiedCount).toBeGreaterThan(0)
})

// ── 4. Reject a Pending Tutor ─────────────────────────────────────────────────

test('admin: reject a pending tutor with a reason', async ({ page }) => {
  // Log in
  await page.goto('/login')
  await page.getByPlaceholder('you@example.com').fill(ADMIN_EMAIL)
  await page.getByPlaceholder('Enter your password').fill(ADMIN_PASSWORD)
  await page.getByRole('button', { name: 'Sign in' }).click()
  await page.waitForURL(/\/admin\/dashboard/, { timeout: 20_000 })

  await page.goto('/admin/tutor-verification')
  const pendingTabBtnR = page.getByRole('tab', { name: /pending/i })
    .or(page.getByRole('button', { name: /pending/i }))
  if (await pendingTabBtnR.first().isVisible({ timeout: 5_000 }).catch(() => false)) {
    await pendingTabBtnR.first().click()
  }
  await page.waitForTimeout(1_500)

  const rejectBtn = page.getByRole('button', { name: /reject/i }).first()
  const hasPending = await rejectBtn.isVisible({ timeout: 5_000 }).catch(() => false)

  if (!hasPending) {
    console.log('No pending tutors — skipping reject step.')
    test.skip()
    return
  }

  await rejectBtn.click()

  // A prompt or modal asking for a rejection reason
  const reasonInput = page
    .getByPlaceholder(/reason/i)
    .or(page.locator('textarea').first())

  if (await reasonInput.isVisible({ timeout: 3_000 }).catch(() => false)) {
    await reasonInput.fill('Profile information is incomplete for E2E test.')
  }

  // Submit rejection
  const submitRejectBtn = page
    .getByRole('button', { name: /reject|submit|confirm/i })
    .last()

  if (await submitRejectBtn.isVisible({ timeout: 3_000 }).catch(() => false)) {
    await submitRejectBtn.click()
  }

  await page.waitForTimeout(2_000)
  // Page should still be on tutor-verification without errors
  await expect(page).toHaveURL(/\/admin\/tutor-verification/)
})
