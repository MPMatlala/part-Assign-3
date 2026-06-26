 CyberBot – Assignment 3
### Cybersecurity Awareness WPF Desktop Application

---

## Overview

CyberBot is a Windows Presentation Foundation (WPF) desktop application built with C# targeting **.NET Framework 4.7.2**. It serves as an interactive cybersecurity awareness tool designed to educate users through a conversational chatbot interface, a quiz engine, a task assistant, and a persistent activity log — all unified under a single tabbed UI.

---

## Features

The application is organized across **four tabs**:

### 1. NLP Chat Interface
An intelligent chatbot powered by natural language processing that answers cybersecurity-related questions and guides users on topics such as phishing, password safety, data privacy, and more.

### 2. Cybersecurity Quiz
An interactive quiz module that tests users' knowledge of cybersecurity concepts. Questions are presented dynamically and results are tracked per session.

### 3. Task Assistant
A productivity tool that allows users to create, view, and manage cybersecurity-related tasks and reminders to help them stay on top of digital safety practices.

### 4. Activity Log
A persistent log that records all user interactions within the application, providing a history of quiz attempts, chat sessions, and task activity.

---

## Assets Carried Over from Part 2

- **Logo** – The CyberBot branding logo is displayed within the application UI.
- **Audio Greeting** – An audio greeting plays on application startup, carried over from the Part 2 implementation.

---

## Technology Stack

| Component | Details |
|---|---|
| Language | C# |
| Framework | .NET Framework 4.7.2 |
| UI Framework | WPF (Windows Presentation Foundation) |
| Database | MySQL |
| DB Access Layer | `DatabaseHelper.cs` (custom helper class) |
| IDE | Visual Studio |

---

## Database

The application integrates with a **MySQL** database for persistent data storage. All database operations are abstracted through the `DatabaseHelper.cs` class, which handles connections, queries, and data retrieval.

Make sure a MySQL server is running and the connection string inside `DatabaseHelper.cs` is configured to match your local environment before running the application.

---

## Project Structure

```
CyberBot/
├── Assets/
│   ├── logo.png                  # CyberBot logo (carried from Part 2)
│   └── greeting.mp3              # Audio greeting (carried from Part 2)
├── Helpers/
│   └── DatabaseHelper.cs         # MySQL database access layer
├── Views/
│   ├── ChatTab.xaml              # NLP Chat Interface tab
│   ├── QuizTab.xaml              # Cybersecurity Quiz tab
│   ├── TaskTab.xaml              # Task Assistant tab
│   └── ActivityLogTab.xaml       # Activity Log tab
├── MainWindow.xaml               # Main window with tab layout
├── MainWindow.xaml.cs            # Code-behind for main window
└── App.xaml                      # Application entry point
```

> Note: Folder structure may vary slightly based on your Visual Studio project layout.

---

## Setup & Running the Application

1. **Clone or open** the project in Visual Studio.
2. **Configure the database** – Open `DatabaseHelper.cs` and update the connection string with your MySQL server credentials:
   ```csharp
   string connectionString = "Server=localhost;Database=cyberbot_db;User ID=root;Password=yourpassword;";
   ```
3. **Create the database schema** – Run the provided SQL script (if included) to set up the required tables.
4. **Build the solution** – Use `Build > Build Solution` or press `Ctrl+Shift+B`.
5. **Run the application** – Press `F5` or click the **Start** button in Visual Studio.

---

## UI Theme

The application uses a consistent **brown and amber color scheme** throughout all tabs, providing a warm and professional look and feel.

---

## Requirements

- Windows OS
- Visual Studio 2019 or later
- .NET Framework 4.7.2
- MySQL Server (local or remote)
- MySQL Connector/NET (NuGet package)

---

## Author

**Messiah**
Software Development – Cybersecurity Awareness Chatbot Assignment 3
