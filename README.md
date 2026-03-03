
# BufeApp - Jedlik Büfé 🍔

A .NET MAUI cross-platform mobile application for the Jedlik school cafeteria (Büfé). This app allows students to order food from the school cafeteria without waiting in line during breaks, and includes a loyalty rewards system.

## Features

- **User Authentication**
  - Login with school email
  - User registration
  - Password reset functionality
  - Secure token-based authentication

- **Loyalty Program**
  - Earn Büfé Points with purchases
  - Track your point balance
  - Redeem rewards (free drinks, discounts, combo upgrades)
  - Progress tracking toward next reward

- **Cross-Platform Support**
  - Android
  - iOS
  - macOS (Catalyst)
  - Windows

## Technologies

- **.NET 8** - Modern cross-platform framework
- **.NET MAUI** - Multi-platform App UI framework
- **XAML** - UI markup language
- **C#** - Primary programming language
- **RESTful API** - Backend communication
- **Secure Storage** - Token and credential storage

## Project Structure

```
BufeApp/
├── App.xaml(.cs)           # Application entry point and resources
├── AppShell.xaml(.cs)      # Shell navigation configuration
├── MainPage.xaml(.cs)      # Main page
├── MauiProgram.cs          # MAUI app builder and DI configuration
├── Models/                 # Data models
│   ├── LoginResponse.cs
│   └── RegisterResponse.cs
├── Pages/                  # Application pages
│   ├── LoginPage.xaml(.cs)
│   ├── LoyaltyPage.xaml(.cs)
│   ├── PasswordResetPage.xaml(.cs)
│   └── PasswordResetConfirmPage.xaml(.cs)
├── Services/               # Business logic and API services
│   ├── ApiService.cs       # HTTP client for API calls
│   ├── StorageService.cs   # Secure storage handling
│   └── UserService.cs      # User authentication logic
├── Resources/              # App resources (images, fonts, etc.)
└── Platforms/              # Platform-specific code
```

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) with MAUI workload, or
- [Visual Studio Code](https://code.visualstudio.com/) with .NET MAUI extension

### Installation

1. Clone the repository:
   ```bash
   git clone https://github.com/Zarodolgozat-13A-BKM/BufeMAUIApp.git
   cd BufeMAUIApp
   ```

2. Restore dependencies:
   ```bash
   dotnet restore
   ```

3. Build the project:
   ```bash
   dotnet build
   ```

4. Run the application:
   ```bash
   # For Android
   dotnet build -t:Run -f net8.0-android

   # For Windows
   dotnet build -t:Run -f net8.0-windows10.0.19041.0
   ```

## API Endpoints

The app communicates with a backend API for authentication:

- `POST /account/login` - User login
- `POST /account/register` - User registration
- `POST /account/logout` - User logout

## License

This project is developed as part of a school assignment (Záródolgozat) at Jedlik Ányos Gépipari és Informatikai Technikum.
