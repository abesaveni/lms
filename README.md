# LiveExpert.AI - Integrated Learning Management System

Welcome to the **LiveExpert.AI** platform. This repository contains both the .NET Backend and the React Frontend for a complete, production-ready Learning Management System.

## 🏗️ Project Structure

```text
.
├── LMS Backend/         # .NET 10.0 Clean Architecture Backend
├── LMS-Frontend/        # React + Vite + TypeScript Frontend
└── run-dev.sh           # Unified starter script
```

---

## 🚀 Quick Start (Run Both Together)

We've provided a script to start both services with a single command.

1.  **Open your terminal** in the root directory.
2.  **Run the script**:
    ```bash
    ./run-dev.sh
    ```
    *This will start the Backend on port 5128 and the Frontend on port 5173.*

---

## 📡 Backend Setup (`LMS Backend`)

### Prerequisites
- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- SQLite (included)

### Manual Run
```bash
cd "LMS Backend/src/LiveExpert.API"
dotnet run
```
- **API URL**: `http://localhost:5128`
- **Swagger Documentation**: `http://localhost:5128/swagger`

---

## 💻 Frontend Setup (`LMS-Frontend`)

### Prerequisites
- [Node.js](https://nodejs.org/) (v18+)
- npm or yarn

### Manual Run
```bash
cd "LMS-Frontend"
npm install
npm run dev
```
- **App URL**: `http://localhost:5173`

---

## 🛠️ Key Integrated Features

- **Auth**: Fully integrated JWT Authentication.
- **Real-time**: SignalR Hubs configured for Chat, Notifications, and Sessions.
- **Admin**: Complete User Management, API Credentials Management, and Booking Verification.
- **Payments**: Razorpay configuration ready in Admin API settings.
- **Communications**: WhatsApp Business API and Google/Outlook Calendar integration ready.

---

## 🎯 Important Notes
- The Platform is configured for **SQLite** by default for zero-setup local development.
- The `.env` in the frontend is already mapped to the backend port `5128`.
- CORS policy on the backend allows requests from the frontend port `5173`.

---

**Developed by LiveExpert.AI Development Team** 🚀✨
