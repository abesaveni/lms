/**
 * Consent Enforcement Utility
 * 
 * This utility ensures that analytics and marketing scripts only load
 * when the user has given explicit consent.
 */

import { hasCookieCategoryConsent, getCookieConsentFromStorage } from '../services/consentApi'

/**
 * Load analytics script only if consent is given
 */
export const loadAnalyticsScript = (scriptUrl: string, scriptId: string): void => {
  if (typeof window === 'undefined' || typeof document === 'undefined') {
    return
  }

  if (!hasCookieCategoryConsent('analytics')) {
    console.log('Analytics script not loaded - consent not given')
    return
  }

  // Check if script already exists
  if (document.getElementById(scriptId)) {
    return
  }

  const script = document.createElement('script')
  script.id = scriptId
  script.src = scriptUrl
  script.async = true
  document.head.appendChild(script)
}

/**
 * Load marketing script only if consent is given
 */
export const loadMarketingScript = (scriptUrl: string, scriptId: string): void => {
  if (typeof window === 'undefined' || typeof document === 'undefined') {
    return
  }

  if (!hasCookieCategoryConsent('marketing')) {
    console.log('Marketing script not loaded - consent not given')
    return
  }

  // Check if script already exists
  if (document.getElementById(scriptId)) {
    return
  }

  const script = document.createElement('script')
  script.id = scriptId
  script.src = scriptUrl
  script.async = true
  document.head.appendChild(script)
}

/**
 * Remove analytics script if consent is revoked
 */
export const removeAnalyticsScript = (scriptId: string): void => {
  const script = document.getElementById(scriptId)
  if (script) {
    script.remove()
  }
}

/**
 * Remove marketing script if consent is revoked
 */
export const removeMarketingScript = (scriptId: string): void => {
  const script = document.getElementById(scriptId)
  if (script) {
    script.remove()
  }
}

/**
 * Initialize consent enforcement
 * Listens for consent updates and loads/removes scripts accordingly
 */
export const initializeConsentEnforcement = (): void => {
  if (typeof window === 'undefined') {
    return
  }

  // Listen for consent updates
  window.addEventListener('cookieConsentUpdated', (event: any) => {
    const consent = event.detail || getCookieConsentFromStorage()
    
    if (!consent) {
      return
    }

    // Handle analytics consent
    if (consent.analytics) {
      // Load Google Analytics (replace with your GA4 measurement ID)
      const GA_MEASUREMENT_ID = (import.meta as any).env?.VITE_GA_MEASUREMENT_ID
      if (GA_MEASUREMENT_ID) {
        // Google Analytics 4
        loadAnalyticsScript(`https://www.googletagmanager.com/gtag/js?id=${GA_MEASUREMENT_ID}`, 'ga-script')
        
        // Initialize gtag
        if (!window.gtag) {
          window.dataLayer = window.dataLayer || []
          window.gtag = function() {
            if (window.dataLayer) {
              window.dataLayer.push(arguments)
            }
          }
          window.gtag('js', new Date())
          window.gtag('config', GA_MEASUREMENT_ID, {
            anonymize_ip: true,
            cookie_flags: 'SameSite=None;Secure'
          })
        }
      }
      
      // Add other analytics scripts here (e.g., Mixpanel, Amplitude, etc.)
      // loadAnalyticsScript('https://cdn.mixpanel.com/mixpanel.js', 'mixpanel-script')
      
      console.log('Analytics consent granted - scripts loaded')
    } else {
      // Remove analytics scripts
      removeAnalyticsScript('ga-script')
      removeAnalyticsScript('mixpanel-script')
      
      // Clear analytics data
      if (window.gtag) {
        window.gtag = undefined
      }
      if (window.dataLayer) {
        window.dataLayer = []
      }
      
      console.log('Analytics consent revoked - scripts removed')
    }

    // Handle marketing consent
    if (consent.marketing) {
      // Load Facebook Pixel (replace with your Pixel ID)
      const FB_PIXEL_ID = (import.meta as any).env?.VITE_FB_PIXEL_ID
      if (FB_PIXEL_ID) {
        loadMarketingScript(`https://connect.facebook.net/en_US/fbevents.js`, 'fb-pixel-script')
        
        // Initialize Facebook Pixel
        if (!window.fbq) {
          const fbqFunction = function(...args: any[]) {
            const fbqObj = window.fbq as any
            if (fbqObj?.callMethod) {
              fbqObj.callMethod.apply(fbqObj, args)
            } else if (fbqObj?.queue) {
              fbqObj.queue.push(args)
            }
          } as any
          fbqFunction.push = fbqFunction
          fbqFunction.loaded = true
          fbqFunction.version = '2.0'
          fbqFunction.queue = []
          window.fbq = fbqFunction
          if (window.fbq) {
            window.fbq('init', FB_PIXEL_ID)
            window.fbq('track', 'PageView')
          }
        }
      }
      
      // Add other marketing scripts here (e.g., LinkedIn Insight Tag, Twitter Pixel, etc.)
      // loadMarketingScript('https://snap.licdn.com/li.lms-analytics/insight.min.js', 'linkedin-script')
      
      console.log('Marketing consent granted - scripts loaded')
    } else {
      // Remove marketing scripts
      removeMarketingScript('fb-pixel-script')
      removeMarketingScript('linkedin-script')
      
      // Clear marketing data
      if (window.fbq) {
        window.fbq = undefined
      }
      
      console.log('Marketing consent revoked - scripts removed')
    }
  })

  // Check initial consent on page load (after DOM is ready)
  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', () => {
      const consent = getCookieConsentFromStorage()
      if (consent) {
        loadInitialScripts(consent)
      }
    })
  } else {
    const consent = getCookieConsentFromStorage()
    if (consent) {
      loadInitialScripts(consent)
    }
  }
}

/**
 * Load initial scripts based on consent
 */
const loadInitialScripts = (consent: any): void => {
  if (typeof window === 'undefined') {
    return
  }

  if (consent.analytics) {
      const GA_MEASUREMENT_ID = (import.meta as any).env?.VITE_GA_MEASUREMENT_ID
      if (GA_MEASUREMENT_ID) {
        loadAnalyticsScript(`https://www.googletagmanager.com/gtag/js?id=${GA_MEASUREMENT_ID}`, 'ga-script')
        if (!window.gtag) {
          window.dataLayer = window.dataLayer || []
          window.gtag = function() {
            if (window.dataLayer) {
              window.dataLayer.push(arguments)
            }
          }
          window.gtag('js', new Date())
          window.gtag('config', GA_MEASUREMENT_ID, {
            anonymize_ip: true,
            cookie_flags: 'SameSite=None;Secure'
          })
        }
      }
    }
    if (consent.marketing) {
      const FB_PIXEL_ID = (import.meta as any).env?.VITE_FB_PIXEL_ID
      if (FB_PIXEL_ID) {
        loadMarketingScript(`https://connect.facebook.net/en_US/fbevents.js`, 'fb-pixel-script')
        if (!window.fbq) {
          const fbqFunction = function(...args: any[]) {
            const fbqObj = window.fbq as any
            if (fbqObj?.callMethod) {
              fbqObj.callMethod.apply(fbqObj, args)
            } else if (fbqObj?.queue) {
              fbqObj.queue.push(args)
            }
          } as any
          fbqFunction.push = fbqFunction
          fbqFunction.loaded = true
          fbqFunction.version = '2.0'
          fbqFunction.queue = []
          window.fbq = fbqFunction
          if (window.fbq) {
            window.fbq('init', FB_PIXEL_ID)
            window.fbq('track', 'PageView')
          }
        }
      }
    }
}

// TypeScript declarations for global objects
declare global {
  interface Window {
    gtag?: (...args: any[]) => void
    dataLayer?: any[]
    fbq?: {
      (...args: any[]): void
      callMethod?: (...args: any[]) => void
      push?: any
      loaded?: boolean
      version?: string
      queue?: any[]
    }
  }
}
