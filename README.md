<h1 align="center"> 🔐 Laser Activation Web </h1>

<div align="center">

A laser device activation management system based on ASP.NET Core Blazor, with ECDSA-P256 signing, JWT authentication, and Redis session management.

[![GitHub license](https://img.shields.io/badge/license-MIT-blue.svg?style=flat-square)](LICENSE)
[![dotnet](https://img.shields.io/badge/.NET-10.0-512bd4.svg?style=flat-square)](https://dotnet.microsoft.com/)
[![mysql](https://img.shields.io/badge/MySQL-5.7+-4479a1.svg?style=flat-square)](https://www.mysql.com/)
[![redis](https://img.shields.io/badge/Redis-6.0+-dc382d.svg?style=flat-square)](https://redis.io/)

<p>
    <span>English</span> |  
    <a href="README.zh-CN.md">中文</a>
</p>
</div>

- 🔑 ECDSA-P256 digital signature for device activation codes
- 🛡️ JWT + Argon2 password hashing + Redis token session management
- 📦 Supports V1 (plaintext) and V2 (SHA1 integrity check) activation formats
- 🖥️ Blazor Server interactive UI with SkiaSharp captcha
- 📋 Activation record management with duplicate detection
- 👥 Role-based access control (Admin / User)
- 📊 Login audit log tracking
- 🔒 HWID files are kept in memory only, never written to disk

## 🏗️ Architecture

```
Laser.Activation.Web/
├── Controllers/          # API endpoints (Auth, Activation, Records)
├── Components/Pages/     # Blazor Server pages
├── Services/             # Business logic
│   ├── AuthService                # JWT generation, Argon2 credential validation
│   ├── EcdsaActivationService     # ECDSA-P256 signing, V1/V2 format
│   ├── CaptchaService             # SkiaSharp captcha image generation
│   └── RedisTokenService          # FreeRedis token CRUD
├── Data/                 # EF Core DbContext (Pomelo MySQL)
└── Models/               # Entity models & DTOs
```

## 🚀 Quick Start

### Prerequisites

- .NET 10 SDK
- MySQL 5.7+ 
- Redis 6.0+

### 1. Create Database

```sql
CREATE DATABASE lead_license CHARACTER SET utf8mb4;
```

Then execute the table creation script:

```bash
mysql -u root -p lead_license < create_tables.sql
```

### 2. Configure Connection Strings

Edit `Laser.Activation.Web/Laser.Activation.Web/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=lead_license;User=root;Password=your_password;"
  },
  "Jwt": {
    "Key": "YourSecretKeyAtLeast32Characters!",
    "Issuer": "Laser.Activation.Web",
    "Audience": "Laser.Activation.Web",
    "ExpireHours": 1
  },
  "Redis": {
    "ConnectionString": "127.0.0.1:6379,defaultDatabase=0"
  },
  "Auth": {
    "DefaultAdminPassword": "admin123"
  }
}
```

### 3. Run

```bash
cd Laser.Activation.Web
dotnet run --project Laser.Activation.Web/Laser.Activation.Web.csproj
```

The application will:
- Seed a default `admin` user (password from `Auth:DefaultAdminPassword`) if no users exist
- Clear all Redis JWT tokens on startup

Navigate to `http://localhost:5000` and log in with `admin` / `admin123`.

## 📖 Features

### Device Activation

| Version | Format | Description |
| :------ | :----- | :---------- |
| V1 | Plaintext | Standard ECDSA signature from HWID |
| V2 | `{body},{sha1}` | Enhanced format with SHA1 integrity checksum |

- Supports file upload (`.req`) or direct text paste for HWID input
- Duplicate HWID detection: shows previous record info with copy/download options
- All fields required: Project Name, Department, Person, Version
- HWID data is processed in memory only, never persisted to disk

### Authentication & Authorization

| Component | Technology |
| :-------- | :--------- |
| Password Hashing | Argon2 (`Isopoh.Cryptography.Argon2`) |
| Token | JWT Bearer (HMAC-SHA256) |
| Session Store | Redis via FreeRedis (TTL matches token expiry) |
| Captcha | SkiaSharp (4-char with noise lines/dots/random rotation) |
| Login Audit | Only successful logins are recorded |

**Redis key design:**

| Key | Value | TTL |
| :-- | :---- | :-- |
| `jwt:{jti}` | `userId` | Token expiry |
| `jwt_user:{userId}` | `Set<jti>` | Token expiry |

### Role-Based Access

| Feature | Admin | User |
| :------ | :---: | :--: |
| Device Activation | ✅ | ✅ |
| Activation Records | ✅ | ✅ |
| User Management | ✅ | ❌ |
| Create User | ✅ | ❌ |
| Login Logs | ✅ | ❌ |

### API Endpoints

| Method | Endpoint | Auth | Description |
| :----- | :------- | :--: | :---------- |
| POST | `/api/auth/login` | ❌ | Login, returns JWT |
| GET | `/api/auth/validate` | ✅ | Validate current token |
| POST | `/api/activation/activate` | ✅ | Activate device |
| GET | `/api/activation/download/{id}` | ✅ | Download activation file |
| GET | `/api/records` | ✅ | List activation records |
| DELETE | `/api/records/{id}` | ✅ | Delete activation record |

## 🛠️ Tech Stack

| Category | Technology |
| :------- | :--------- |
| Framework | ASP.NET Core 10 Blazor Server |
| Database | MySQL 5.7+ (Pomelo EF Core) |
| Cache/Session | Redis (FreeRedis) |
| Authentication | JWT Bearer + Argon2 |
| Captcha | SkiaSharp 3.x |
| Signing | ECDSA P-256 (System.Security.Cryptography) |

## 📦 NuGet Packages

| Package | Version | Description |
| :------ | :------ | :---------- |
| [FreeRedis](https://www.nuget.org/packages/FreeRedis) | 1.5.5 | Redis client for token session storage |
| [Isopoh.Cryptography.Argon2](https://www.nuget.org/packages/Isopoh.Cryptography.Argon2) | 2.0.0 | Argon2 password hashing |
| [Microsoft.AspNetCore.Authentication.JwtBearer](https://www.nuget.org/packages/Microsoft.AspNetCore.Authentication.JwtBearer) | 10.0.5 | JWT Bearer authentication middleware |
| [Microting.EntityFrameworkCore.MySql.Json.Microsoft](https://www.nuget.org/packages/Microting.EntityFrameworkCore.MySql.Json.Microsoft) | 10.0.5 | EF Core MySQL provider (Pomelo) |
| [SkiaSharp](https://www.nuget.org/packages/SkiaSharp) | 3.119.2 | Captcha image generation |
| [Microsoft.AspNetCore.Components.WebAssembly.Server](https://www.nuget.org/packages/Microsoft.AspNetCore.Components.WebAssembly.Server) | 10.0.5 | Blazor WebAssembly host support |

## 📄 Project Structure

```
Laser.Activation.Web/
├── create_tables.sql                      # Database creation script
├── Laser.Activation.Web.slnx              # Solution file
└── Laser.Activation.Web/
    └── Laser.Activation.Web/
        ├── Program.cs                     # DI, middleware, startup seed
        ├── appsettings.json               # Configuration
        ├── Models/                        # Entities & DTOs
        ├── Data/AppDbContext.cs           # EF Core DbContext
        ├── Services/                      # Business services
        ├── Controllers/                   # API controllers
        ├── Components/Pages/              # Blazor pages
        └── wwwroot/js/auth.js             # Client-side auth module
```

## 🗄 License

[MIT](LICENSE)
