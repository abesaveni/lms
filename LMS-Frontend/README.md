# LiveExpert.AI Frontend

A modern, production-ready React.js frontend application for the LiveExpert.AI Learning Management System.

## 🚀 Tech Stack

- **React.js** (Functional Components with TypeScript)
- **Vite** (Build tool)
- **Tailwind CSS** (Styling)
- **React Router** (Routing)
- **Framer Motion** (Animations)
- **Lucide React** (Icons)

## 📦 Installation

1. Install dependencies:
```bash
npm install
```

2. Start development server:
```bash
npm run dev
```

3. Build for production:
```bash
npm run build
```

## 🎨 Design System

### Colors
- **Primary**: Soft Orange/Peach (`#ff6b35` - `#ff8555`)
- **Secondary**: Teal/Green (`#14b8a6` - `#2dd4bf`)
- **Accent**: Purple/Blue (`#a855f7` - `#c084fc`)

### Typography
- Font Family: Inter (Google Fonts)
- Responsive font sizes with clear hierarchy

## 📁 Project Structure

```
src/
├── components/
│   ├── ui/              # Reusable UI components
│   ├── domain/          # Domain-specific components
│   └── layout/          # Layout components (Header, Footer)
├── pages/
│   ├── Landing.tsx      # Home/Landing page
│   ├── auth/           # Authentication pages
│   ├── student/        # Student pages
│   ├── tutor/          # Tutor pages
│   └── admin/          # Admin pages
├── App.tsx             # Main app component with routing
└── main.tsx            # Entry point
```

## 🎯 Features

### Landing Page
- Hero section with gradient backgrounds
- Trust indicators (stats)
- Features section
- How it works (3 steps)
- Popular courses
- Testimonials
- FAQ accordion
- Final CTA

### Authentication
- Login with role selection
- Multi-step registration
- Password strength indicator
- Social login options (UI only)

### Student Experience
- Dashboard with stats and upcoming sessions
- Find tutors with filters
- Tutor profile view
- My sessions (upcoming/completed/cancelled)
- My courses with progress tracking

### Tutor Experience
- Dashboard with earnings and stats
- Session management
- Course creation UI

### Admin Experience
- Dashboard with platform statistics
- User management
- Analytics overview

## 🎨 Component Library

### Core Components
- `Button` - Primary, secondary, ghost, outline variants
- `Input` - Text inputs with labels and error states
- `Card` - Card container with hover effects
- `Modal` - Modal dialog component
- `Badge` - Status badges
- `Avatar` - User avatars
- `Tabs` - Tab navigation
- `Accordion` - Expandable FAQ sections
- `Skeleton` - Loading placeholders
- `EmptyState` - Empty state displays

### Domain Components
- `CourseCard` - Course display card
- `TutorCard` - Tutor profile card
- `StatsCard` - Statistics display card

## 📱 Responsive Design

- Mobile-first approach
- Breakpoints: sm (640px), md (768px), lg (1024px), xl (1280px)
- Touch-friendly buttons and interactions
- Collapsible sidebars on mobile
- Bottom sheets for filters on mobile

## 🔌 Backend Integration

The frontend is designed to be API-ready. All data fetching will be integrated with the .NET backend later. Currently, the UI uses mock data for demonstration.

## 🚦 Routes

- `/` - Landing page
- `/login` - Login page
- `/register` - Registration page
- `/student/dashboard` - Student dashboard
- `/student/find-tutors` - Find tutors
- `/student/tutors/:username` - Tutor profile
- `/student/my-sessions` - My sessions
- `/student/my-courses` - My courses
- `/tutor/dashboard` - Tutor dashboard
- `/tutor/sessions` - Tutor sessions
- `/tutor/courses/create` - Create course
- `/admin/dashboard` - Admin dashboard
- `/admin/users` - User management

## 🎯 Next Steps

1. Install dependencies: `npm install`
2. Start development: `npm run dev`
3. Integrate with .NET backend APIs
4. Add authentication context/state management
5. Implement real data fetching
6. Add form validation libraries if needed
7. Set up error boundaries
8. Add loading states for API calls

## 📝 Notes

- All components are production-ready and follow best practices
- Design system is consistent throughout
- Mobile-responsive and accessible
- Ready for backend API integration
- No hardcoded business logic - all data will come from APIs
