import { Link } from 'react-router-dom'
import { Facebook, Twitter, Instagram, Linkedin } from 'lucide-react'
import logoImage from '../../assets/logo.png'

const Footer = () => {
  const companyLinks = [
    { label: 'Home', path: '/' },
    { label: 'Find Tutors', path: '/find-tutors' },
    { label: 'About Us', path: '/about-us' },
    { label: 'Become a Tutor', path: '/join-us?role=tutor' },
    { label: 'Privacy Policy', path: '/privacy-policy' },
    { label: 'Terms & Conditions', path: '/terms-and-conditions' },
  ]

  return (
    <footer className="bg-gray-900 text-gray-300">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        <div className="grid grid-cols-1 md:grid-cols-3 gap-8 mb-6">
          {/* Brand Column */}
          <div>
            <Link to="/" className="inline-block mb-4">
              <img 
                src={logoImage} 
                alt="LiveExpert.AI Logo" 
                className="h-10 w-auto"
              />
            </Link>
            <p className="text-sm text-gray-400 mb-4">
              Empowering learners worldwide with expert-led live sessions.
            </p>
            <div className="flex gap-4">
              <a href="#" className="hover:text-primary-400 transition-colors">
                <Facebook className="w-5 h-5" />
              </a>
              <a href="#" className="hover:text-primary-400 transition-colors">
                <Twitter className="w-5 h-5" />
              </a>
              <a href="#" className="hover:text-primary-400 transition-colors">
                <Instagram className="w-5 h-5" />
              </a>
              <a href="#" className="hover:text-primary-400 transition-colors">
                <Linkedin className="w-5 h-5" />
              </a>
            </div>
          </div>

          {/* Company Info Column */}
          <div>
            <h3 className="text-white font-semibold mb-4">Quick Links</h3>
            <ul className="space-y-2">
              {companyLinks.slice(0, 5).map((link) => (
                <li key={link.path}>
                  <Link to={link.path} className="text-sm hover:text-primary-400 transition-colors">
                    {link.label}
                  </Link>
                </li>
              ))}
            </ul>
          </div>

          {/* Contact Column */}
          <div>
            <h3 className="text-white font-semibold mb-4">Contact Us</h3>
            <ul className="space-y-2 text-sm text-gray-400">
              <li>Email: support@liveexpert.ai</li>
            </ul>
          </div>
        </div>

        {/* Copyright */}
        <div className="border-t border-gray-800 pt-6">
          <div className="flex flex-col md:flex-row justify-between items-center gap-4 text-sm text-gray-400">
            <p>© 2026 LiveExpert.AI. All rights reserved.</p>
          </div>
        </div>
      </div>
    </footer>
  )
}

export default Footer
