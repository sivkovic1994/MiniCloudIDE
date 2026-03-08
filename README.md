# CloudIDE

A cloud-based IDE that lets you write and execute **Python** and **C#** code directly in the browser. Supports user authentication, script saving, and history browsing.

![.NET Core](https://img.shields.io/badge/.NET_Core-512BD4?logo=dotnet&logoColor=white)
![React](https://img.shields.io/badge/React-61DAFB?logo=react&logoColor=black)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-4169E1?logo=postgresql&logoColor=white)
![TypeScript](https://img.shields.io/badge/TypeScript-3178C6?logo=typescript&logoColor=white)
![Python](https://img.shields.io/badge/Python-3776AB?logo=python&logoColor=white)

---

## Features

- **Code Editor** — Monaco Editor (the same editor that powers VS Code) with syntax highlighting for Python and C#
- **Code Execution** — Run Python or C# code and get output with measured execution time
- **Authentication** — Register and log in with JWT tokens
- **Script History** — Save code and load it later (requires login)
- **Keyboard Shortcut** — `F5` to quickly run code
- **Code Examples** — Built-in examples for both languages
- **Theming** — Switch between light and dark themes

---

## Technologies

### Backend

| Technology | Purpose |
|---|---|
| **ASP.NET Core** | Web API framework |
| **Entity Framework Core** | ORM for database access |
| **PostgreSQL** | Relational database |
| **ASP.NET Identity** | User management (registration, login) |
| **JWT (JSON Web Tokens)** | Authentication — stateless tokens with a 30-day expiry |
| **Roslyn (Microsoft.CodeAnalysis)** | In-process C# code execution via the scripting API |
| **Python TCP Worker** | External Python process that executes user Python code over a TCP socket |

### Frontend

| Technology | Purpose |
|---|---|
| **React** | UI framework |
| **TypeScript** | Typed JavaScript |
| **Monaco Editor** | Code editor component (same foundation as VS Code) |
| **Axios** | HTTP client for API communication |
| **React Context API** | Authentication state management |

---

## Architecture

```
┌─────────────────────────────────────────────────┐
│                   Browser                        │
│  React + Monaco Editor + Axios                   │
│  (http://localhost:3000)                         │
└──────────────────────┬──────────────────────────┘
                       │ HTTP (proxy)
┌──────────────────────▼──────────────────────────┐
│             ASP.NET Core Backend                 │
│         (https://localhost:7245)                 │
│                                                  │
│  ┌──────────────┐  ┌──────────────────────────┐ │
│  │ AuthController│  │ CodeExecutionController  │ │
│  └──────┬───────┘  └────────┬────────┬────────┘ │
│         │                   │        │           │
│    ┌────▼────┐      ┌──────▼──┐ ┌───▼────────┐ │
│    │ Identity│      │ Roslyn  │ │TCP Socket   │ │
│    │ + JWT   │      │ (C#)    │ │(Python)     │ │
│    └────┬────┘      └─────────┘ └───┬────────┘ │
│         │                           │           │
│    ┌────▼───────────────┐    ┌──────▼────────┐  │
│    │   PostgreSQL DB    │    │  worker.py     │  │
│    │ (Users, History)   │    │ (port 5555)    │  │
│    └────────────────────┘    └───────────────┘  │
└─────────────────────────────────────────────────┘
```

---

## How Code Execution Works

### C#
Uses the **Roslyn Scripting API** — code is compiled and executed in-process. Supports the `System` namespace and returns the result of the last evaluation.

### Python
The backend launches `worker.py` as a background process. Communication happens over a **TCP socket** at `127.0.0.1:5555`:
1. The backend sends the code length (4 bytes, big-endian) + the code (UTF-8)
2. The worker executes the code with `exec()` and captures stdout/stderr
3. Returns a JSON response: `{ "output": "...", "errors": "..." }`

---

## API Endpoints

| Method | Route | Description | Auth |
|---|---|---|---|
| `POST` | `/Auth/register` | Register a new user | No |
| `POST` | `/Auth/login` | Log in | No |
| `GET` | `/Auth/me` | Get current user | Yes |
| `POST` | `/CodeExecution` | Execute code | No |
| `POST` | `/CodeExecution/save` | Save a script | Yes |
| `GET` | `/CodeExecution/history/{language}` | Get script history by language | Yes |
| `GET` | `/CodeExecution/script/{id}` | Get a single script by ID | Yes |

---

## Project Structure

```
CloudIDE/
├── docker-compose.yml               # Runs everything with one command
├── MiniCloudIDE_Backend/
│   ├── Dockerfile                   # Backend Docker image
│   ├── Program.cs                   # Server configuration (CORS, JWT, DI)
│   ├── worker.py                    # Python TCP worker for code execution
│   ├── Controllers/
│   │   ├── AuthController.cs        # Register, login, /me
│   │   └── CodeExecutionController.cs  # Code execution and history
│   ├── Data/
│   │   └── AppDbContext.cs          # EF Core context (Identity + ScriptHistory)
│   ├── Models/
│   │   ├── ApplicationUser.cs       # User model (extends IdentityUser)
│   │   ├── ScriptHistory.cs         # Model for saved scripts
│   │   └── DTOs/                    # Request/Response models
│   ├── Services/
│   │   ├── PythonExecutionService.cs     # TCP communication with the Python worker
│   │   ├── PythonWorkerHostedService.cs  # Launches the worker.py process
│   │   └── ScriptHistoryService.cs       # CRUD operations for script history
│   └── Migrations/                  # EF Core migrations
└── minicloudide-frontend/
    ├── Dockerfile                   # Frontend Docker image
    ├── nginx.conf                   # Nginx config (serves app + proxies API)
    ├── package.json
    └── src/
        ├── App.tsx                  # Main entry point
        ├── components/
        │   ├── EditorWithControls.tsx   # Editor + controls + output + history
        │   └── AuthPage.tsx             # Login/Register form
        └── context/
            └── AuthProvider.tsx      # Auth context (token, user, login/logout)
```

---

## Getting Started

### Docker (recommended)

The easiest way to run the project — no need to install .NET, Node.js, Python or PostgreSQL manually.

**Prerequisites:** [Docker](https://www.docker.com/get-started)

```bash
docker-compose up --build
```

The application is available at `http://localhost:3000`.

### Manual Setup

#### Prerequisites
- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- [Node.js](https://nodejs.org/) (v18+)
- [Python 3](https://www.python.org/downloads/)
- [PostgreSQL](https://www.postgresql.org/download/)

#### 1. Database

Create a PostgreSQL database called `minicloudide`, configure the connection string in `appsettings.json`, then apply migrations:

```bash
cd MiniCloudIDE_Backend
dotnet ef database update
```

#### 2. Backend

```bash
cd MiniCloudIDE_Backend
dotnet run
```

The backend will automatically start the `worker.py` process.

#### 3. Frontend

```bash
cd minicloudide-frontend
npm install
npm start
```

The application is available at `http://localhost:3000`.
