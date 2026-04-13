import { useEffect } from 'react'
import { Card, CardContent } from '../components/ui/Card'

const TermsAndConditions = () => {
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
              <h1 className="text-4xl font-bold text-gray-900 mb-4">Terms and Conditions</h1>
              <p className="text-xl text-gray-600">Empowering learners worldwide</p>
              <p className="text-gray-500 mt-2">
                We are dedicated to providing personalized online tutoring experiences that unlock every learner's potential.
              </p>
            </div>

            <div className="mt-8 space-y-6">
              <p className="text-gray-700 leading-relaxed">
                By accessing or using our platform, you agree to comply with and be bound by these Terms and Conditions. Please read them carefully. If you do not agree with these terms, you must not use our services.
              </p>

              <section>
                <h2 className="text-2xl font-semibold text-gray-900 mt-8 mb-4">1. User Accounts</h2>
                <p className="text-gray-700 leading-relaxed">
                  You must provide accurate, complete, and current information during registration. Failure to do so may result in suspension or termination of your account. You are responsible for maintaining the confidentiality of your account information and password. You must notify us immediately of any unauthorized use of your account.
                </p>
              </section>

              <section>
                <h2 className="text-2xl font-semibold text-gray-900 mt-8 mb-4">2. User Conduct</h2>
                <p className="text-gray-700 leading-relaxed mb-3">
                  Users agree not to engage in the following activities:
                </p>
                <ul className="list-disc list-inside space-y-2 text-gray-700 ml-4">
                  <li>Harassment or abuse of other users</li>
                  <li>Fraudulent activities or impersonation</li>
                  <li>Uploading inappropriate, offensive, or illegal content</li>
                  <li>Disrupting the platform's functionality</li>
                </ul>
              </section>

              <section>
                <h2 className="text-2xl font-semibold text-gray-900 mt-8 mb-4">3. Pricing Changes</h2>
                <p className="text-gray-700 leading-relaxed">
                  We reserve the right to change pricing at any time. Users will be notified of any changes in advance.
                </p>
              </section>

              <section>
                <h2 className="text-2xl font-semibold text-gray-900 mt-8 mb-4">4. Payments and Refunds</h2>
                <div className="space-y-4">
                  <div>
                    <h3 className="text-xl font-semibold text-gray-800 mb-2">Payment Terms:</h3>
                    <p className="text-gray-700 leading-relaxed">
                      Payments for tutoring sessions must be made through our platform. Razorpay is the only payment gateway used.
                    </p>
                  </div>
                  <div>
                    <h3 className="text-xl font-semibold text-gray-800 mb-2">Refund Policy:</h3>
                    <ul className="list-disc list-inside space-y-2 text-gray-700 ml-4">
                      <li>The tutor cancels the session.</li>
                      <li>The session did not occur due to technical issues on the platform.</li>
                    </ul>
                  </div>
                  <div>
                    <h3 className="text-xl font-semibold text-gray-800 mb-2">Rescheduling:</h3>
                    <p className="text-gray-700 leading-relaxed">
                      Sessions can be rescheduled up to 24 hours in advance through the platform.
                    </p>
                  </div>
                </div>
              </section>

              <section>
                <h2 className="text-2xl font-semibold text-gray-900 mt-8 mb-4">5. Tutor Responsibilities</h2>
                <div className="space-y-3">
                  <div>
                    <h3 className="text-xl font-semibold text-gray-800 mb-2">Profile Accuracy:</h3>
                    <p className="text-gray-700 leading-relaxed">
                      Tutors must provide accurate and up-to-date information in their profiles. Misrepresentation may result in account suspension.
                    </p>
                  </div>
                  <div>
                    <h3 className="text-xl font-semibold text-gray-800 mb-2">Session Delivery:</h3>
                    <p className="text-gray-700 leading-relaxed">
                      Tutors are expected to deliver sessions professionally and punctually.
                    </p>
                  </div>
                  <div>
                    <h3 className="text-xl font-semibold text-gray-800 mb-2">Compliance with Laws:</h3>
                    <p className="text-gray-700 leading-relaxed">
                      Tutors must comply with all relevant educational and legal requirements in their region.
                    </p>
                  </div>
                </div>
              </section>

              <section>
                <h2 className="text-2xl font-semibold text-gray-900 mt-8 mb-4">6. Student Responsibilities</h2>
                <div className="space-y-3">
                  <div>
                    <h3 className="text-xl font-semibold text-gray-800 mb-2">Participation:</h3>
                    <p className="text-gray-700 leading-relaxed">
                      Students are expected to attend sessions on time and actively participate.
                    </p>
                  </div>
                  <div>
                    <h3 className="text-xl font-semibold text-gray-800 mb-2">Feedback:</h3>
                    <p className="text-gray-700 leading-relaxed">
                      Students are encouraged to provide constructive feedback and rate their tutors after each session.
                    </p>
                  </div>
                </div>
              </section>

              <section>
                <h2 className="text-2xl font-semibold text-gray-900 mt-8 mb-4">7. Referral Program (Students Only)</h2>
                <div className="space-y-3">
                  <p className="text-gray-700 leading-relaxed">
                    The referral program is available only to student accounts. Tutors are not eligible for referral
                    bonuses.
                  </p>
                  <p className="text-gray-700 leading-relaxed">
                    Referral codes are optional during student signup and must be entered exactly as shown. Referral
                    bonuses are credited only after the referred student books a session with a tutor.
                  </p>
                </div>
              </section>

              <section>
                <h2 className="text-2xl font-semibold text-gray-900 mt-8 mb-4">8. Cancellation Policy</h2>
                <div className="space-y-4">
                  <div>
                    <h3 className="text-xl font-semibold text-gray-800 mb-2">Student Cancellations</h3>
                    <div className="ml-4 space-y-2">
                      <div>
                        <h4 className="font-semibold text-gray-700 mb-1">Up to 24 Hours Before the Scheduled Session:</h4>
                        <p className="text-gray-700 leading-relaxed">
                          Students can cancel or reschedule a session without any penalty if done up to 24 hours before the scheduled session start time. No charges will be applied.
                        </p>
                      </div>
                      <div>
                        <h4 className="font-semibold text-gray-700 mb-1">Within 24 Hours of the Scheduled Session:</h4>
                        <p className="text-gray-700 leading-relaxed">
                          Cancellations made within 24 hours of the scheduled session will incur a cancellation fee of [specified amount]. This fee compensates the tutor for the short notice and lost opportunity to fill the time slot.
                        </p>
                      </div>
                    </div>
                  </div>
                  <div>
                    <h3 className="text-xl font-semibold text-gray-800 mb-2">Tutor Cancellations</h3>
                    <div className="ml-4">
                      <h4 className="font-semibold text-gray-700 mb-1">Tutor-Initiated Cancellations:</h4>
                      <p className="text-gray-700 leading-relaxed">
                        Tutors are expected to maintain their schedules accurately. If a tutor needs to cancel a session, they must notify the student as soon as possible. Tutors who frequently cancel sessions may face penalties, including suspension or removal from the platform.
                      </p>
                    </div>
                  </div>
                  <div>
                    <h3 className="text-xl font-semibold text-gray-800 mb-2">No-Shows</h3>
                    <div className="ml-4 space-y-2">
                      <div>
                        <h4 className="font-semibold text-gray-700 mb-1">Student absence:</h4>
                        <p className="text-gray-700 leading-relaxed">
                          If a student does not show up for a session without prior notice, the session fee will still be charged, and the tutor will be paid for their time.
                        </p>
                      </div>
                      <div>
                        <h4 className="font-semibold text-gray-700 mb-1">Tutor absence:</h4>
                        <p className="text-gray-700 leading-relaxed">
                          If a tutor does not show up for a session without prior notice, the student will be fully refunded, and the tutor may face penalties, including suspension or removal from the platform.
                        </p>
                      </div>
                    </div>
                  </div>
                </div>
              </section>

              <section>
                <h2 className="text-2xl font-semibold text-gray-900 mt-8 mb-4">9. Rescheduling Policy</h2>
                <p className="text-gray-700 leading-relaxed">
                  Students and tutors can reschedule a session up to 24 hours before the scheduled time without any penalty. Rescheduling within 24 hours of the session may be treated as a cancellation, and the respective fees may apply.
                </p>
              </section>

              <section>
                <h2 className="text-2xl font-semibold text-gray-900 mt-8 mb-4">10. Refund Policy</h2>
                <div className="space-y-3">
                  <div>
                    <h3 className="text-xl font-semibold text-gray-800 mb-2">Refund Eligibility:</h3>
                    <p className="text-gray-700 leading-relaxed mb-2">
                      Refunds are issued in the following situations:
                    </p>
                    <ul className="list-disc list-inside space-y-2 text-gray-700 ml-4">
                      <li>Tutor cancels a session.</li>
                      <li>Session did not occur due to technical issues on the platform.</li>
                    </ul>
                  </div>
                  <div>
                    <h3 className="text-xl font-semibold text-gray-800 mb-2">Refund Process:</h3>
                    <p className="text-gray-700 leading-relaxed">
                      Refund requests must be submitted within [specified timeframe] after the scheduled session. The platform will review and process eligible refunds within [specified timeframe].
                    </p>
                  </div>
                </div>
              </section>

              <section>
                <h2 className="text-2xl font-semibold text-gray-900 mt-8 mb-4">11. Policy Changes</h2>
                <div>
                  <h3 className="text-xl font-semibold text-gray-800 mb-2">Right to Modify:</h3>
                  <p className="text-gray-700 leading-relaxed">
                    The platform reserves the right to modify the cancellation policy at any time. Users will be notified of any changes in advance. Continued use of the platform after any such changes constitutes acceptance of the new terms.
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

export default TermsAndConditions
