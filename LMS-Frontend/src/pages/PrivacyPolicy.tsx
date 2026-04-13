import { useEffect } from 'react'
import { Card, CardContent } from '../components/ui/Card'

const PrivacyPolicy = () => {
  useEffect(() => {
    // Scroll to top when component mounts
    window.scrollTo({ top: 0, left: 0, behavior: 'instant' })
  }, [])

  return (
    <div className="min-h-screen bg-gray-50 py-12">
      <div className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8">
        <Card>
          <CardContent className="prose prose-lg max-w-none py-8 px-6">
            <div className="text-center mb-8">
              <h1 className="text-4xl font-bold text-gray-900 mb-4">Privacy Policy</h1>
              <p className="text-xl text-gray-600">Empowering learners worldwide</p>
              <p className="text-gray-500 mt-2">
                We are dedicated to providing personalized online tutoring experiences that unlock every learner's potential.
              </p>
            </div>

            <div className="mt-8 space-y-6">
              <p className="text-gray-700 leading-relaxed">
                We are committed to protecting your privacy. This Privacy Policy explains how we collect, use, and share your personal data when you use our website and services.
              </p>

              <section>
                <h2 className="text-2xl font-semibold text-gray-900 mt-8 mb-4">1. Information We Collect</h2>
                <ul className="list-disc list-inside space-y-2 text-gray-700 ml-4">
                  <li><strong>Personal Information:</strong> Name, email address, phone number, date of birth, and payment details.</li>
                  <li><strong>Profile Information:</strong> Tutors' qualifications, experience, subjects taught, hourly rates, and availability.</li>
                  <li><strong>Usage Information:</strong> Browsing history, search queries, session bookings, and page visits.</li>
                  <li><strong>Technical Information:</strong> IP address, browser type, device information, and operating system.</li>
                  <li><strong>Communication Information:</strong> Customer support inquiries, feedback forms, and chat messages.</li>
                </ul>
              </section>

              <section>
                <h2 className="text-2xl font-semibold text-gray-900 mt-8 mb-4">2. How We Use Your Information</h2>
                <ul className="list-disc list-inside space-y-2 text-gray-700 ml-4">
                  <li><strong>Provide Services:</strong> Operate and maintain our platform, process transactions, manage accounts, and facilitate tutoring sessions.</li>
                  <li><strong>Personalization:</strong> Recommend tutors and content based on interests and learning goals.</li>
                  <li><strong>Communication:</strong> Notify you about account updates, session reminders, and policy changes.</li>
                  <li><strong>Marketing:</strong> Send promotional materials and newsletters with your consent.</li>
                  <li><strong>Analytics:</strong> Monitor performance and improve functionality and user experience.</li>
                  <li><strong>Security:</strong> Protect against fraud and unauthorized access.</li>
                </ul>
              </section>

              <section>
                <h2 className="text-2xl font-semibold text-gray-900 mt-8 mb-4">3. How We Share Your Information</h2>
                <ul className="list-disc list-inside space-y-2 text-gray-700 ml-4">
                  <li><strong>With Tutors:</strong> Share students contact details and learning goals to facilitate sessions.</li>
                  <li><strong>With Students:</strong> Share tutors profiles to help students choose suitable tutors.</li>
                  <li>We do not store credit card information on our site.</li>
                </ul>
              </section>

              <section>
                <h2 className="text-2xl font-semibold text-gray-900 mt-8 mb-4">4. Your Rights</h2>
                <ul className="list-disc list-inside space-y-2 text-gray-700 ml-4">
                  <li><strong>Access and Correction:</strong> Update your personal information through account settings.</li>
                  <li><strong>Deletion:</strong> Request deletion of your data, subject to legal exceptions.</li>
                  <li><strong>Opt-Out:</strong> Unsubscribe from marketing communications.</li>
                  <li><strong>Data Portability:</strong> Request a copy of your data in a machine-readable format.</li>
                </ul>
              </section>

              <section>
                <h2 className="text-2xl font-semibold text-gray-900 mt-8 mb-4">5. Cookies and Tracking Technologies</h2>
                <p className="text-gray-700 leading-relaxed">
                  We use cookies to enhance your experience. Manage your cookie preferences through browser settings.
                </p>
              </section>

              <section>
                <h2 className="text-2xl font-semibold text-gray-900 mt-8 mb-4">6. Third-Party Links</h2>
                <p className="text-gray-700 leading-relaxed">
                  We are not responsible for the privacy practices of third-party websites linked from our platform. Review their privacy policies before sharing information.
                </p>
              </section>

              <section>
                <h2 className="text-2xl font-semibold text-gray-900 mt-8 mb-4">7. Changes to This Privacy Policy</h2>
                <p className="text-gray-700 leading-relaxed">
                  We may update this policy. Significant changes will be posted on our website.
                </p>
              </section>

              <section>
                <h2 className="text-2xl font-semibold text-gray-900 mt-8 mb-4">8. Data Protection and Security</h2>
                <div className="space-y-4 text-gray-700 leading-relaxed">
                  <p>
                    We use TLS/HTTPS to protect all data in transit, and AES-256 encryption to protect sensitive information at rest. OAuth tokens and user identifiers are encrypted before being stored in our SQL database, which is hosted on AWS EC2 servers and backed by AWS S3 storage located in the Europe (Stockholm) region (eu-north-1). Access to production systems and user data is strictly limited to authorized personnel who use multi-factor authentication and least-privilege access controls.
                  </p>
                  <p>
                    We retain user data for as long as the user's account remains active. When a user requests deletion, all personal data — including OAuth tokens — is permanently removed within 2–3 days, except where additional retention is required for legal or security reasons. Users may revoke our access to their Google account at any time through their Google Account settings or by disconnecting Google services within our app.
                  </p>
                  <p>
                    OAuth tokens are used only to perform actions requested by the user (such as creating or updating calendar events) and are never shared with third parties or used for any other purpose.
                  </p>
                  <p>
                    For any privacy-related questions, please contact us at{' '}
                    <a href="mailto:support@liveexpert.ai" className="text-primary-600 hover:text-primary-700 underline">
                      support@liveexpert.ai
                    </a>.
                  </p>
                </div>
              </section>
            </div>

            <div className="mt-12 pt-8 border-t border-gray-200 text-center">
              <p className="text-gray-600">
                Last updated: {new Date().toLocaleDateString('en-US', { year: 'numeric', month: 'long', day: 'numeric' })}
              </p>
            </div>
          </CardContent>
        </Card>
      </div>
    </div>
  )
}

export default PrivacyPolicy
