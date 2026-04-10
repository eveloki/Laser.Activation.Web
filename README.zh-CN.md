<h1 align="center"> 🔐 Laser Activation Web </h1>

<div align="center">

基于 ASP.NET Core Blazor 的激光设备激活管理系统，采用 ECDSA-P256 签名、JWT 认证和 Redis 会话管理。

[![GitHub license](https://img.shields.io/badge/license-MIT-blue.svg?style=flat-square)](LICENSE)
[![dotnet](https://img.shields.io/badge/.NET-10.0-512bd4.svg?style=flat-square)](https://dotnet.microsoft.com/)
[![mysql](https://img.shields.io/badge/MySQL-5.7+-4479a1.svg?style=flat-square)](https://www.mysql.com/)
[![redis](https://img.shields.io/badge/Redis-6.0+-dc382d.svg?style=flat-square)](https://redis.io/)

<p align="center">
    <a href="README.md">English</a> |   
    <span>中文</span>
</p>
</div>

- 🔑 ECDSA-P256 数字签名生成设备激活码
- 🛡️ JWT + Argon2 密码哈希 + Redis 令牌会话管理
- 📦 支持 V1（明文）和 V2（SHA1 完整性校验）两种激活格式
- 🖥️ Blazor Server 交互式界面，集成 SkiaSharp 验证码
- 📋 激活记录管理，支持重复激活检测
- 👥 基于角色的访问控制（Admin / User）
- 📊 登录审计日志追踪
- 🔒 HWID 文件仅在内存中处理，不会写入磁盘

## 🏗️ 系统架构

```
Laser.Activation.Web/
├── Controllers/          # API 接口（认证、激活、记录）
├── Components/Pages/     # Blazor Server 页面
├── Services/             # 业务逻辑
│   ├── AuthService                # JWT 生成、Argon2 凭证验证
│   ├── EcdsaActivationService     # ECDSA-P256 签名、V1/V2 格式
│   ├── CaptchaService             # SkiaSharp 验证码图片生成
│   └── RedisTokenService          # FreeRedis 令牌增删查
├── Data/                 # EF Core 数据库上下文（Pomelo MySQL）
└── Models/               # 实体模型与 DTO
```

## 🚀 快速入门

### 环境要求

- .NET 10 SDK
- MySQL 5.7+
- Redis 6.0+

### 1. 创建数据库

```sql
CREATE DATABASE lead_license CHARACTER SET utf8mb4;
```

执行建表脚本：

```bash
mysql -u root -p lead_license < create_tables.sql
```

### 2. 配置连接字符串

编辑 `Laser.Activation.Web/Laser.Activation.Web/appsettings.json`：

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=lead_license;User=root;Password=your_password;"
  },
  "Jwt": {
    "Key": "你的密钥至少32个字符!",
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

### 3. 运行

```bash
cd Laser.Activation.Web
dotnet run --project Laser.Activation.Web/Laser.Activation.Web.csproj
```

应用启动时会自动：
- 若无用户则创建默认 `admin` 用户（密码来自 `Auth:DefaultAdminPassword`）
- 清除 Redis 中所有 JWT 令牌

打开浏览器访问 `http://localhost:5000`，使用 `admin` / `admin123` 登录。

## 📖 功能特性

### 设备激活

| 版本 | 格式 | 说明 |
| :--- | :--- | :--- |
| V1 | 明文 | 标准 ECDSA 签名，直接使用 HWID 生成激活码 |
| V2 | `{body},{sha1}` | 增强格式，输入输出均包含 SHA1 校验和 |

- 支持文件上传（`.req`）或直接粘贴 HWID 文本
- 重复 HWID 检测：显示上次激活信息，可直接复制或下载激活码
- 所有字段均为必填：项目名称、部门名称、负责人、版本号
- HWID 数据仅在内存中处理，不会保存到服务器磁盘

### 认证与授权

| 组件 | 技术 |
| :--- | :--- |
| 密码哈希 | Argon2（`Isopoh.Cryptography.Argon2`） |
| 令牌 | JWT Bearer（HMAC-SHA256） |
| 会话存储 | Redis 通过 FreeRedis（TTL 与令牌过期时间一致） |
| 验证码 | SkiaSharp（4位字符 + 干扰线/噪点/随机旋转） |
| 登录审计 | 仅记录成功的登录 |

**Redis 键设计：**

| 键 | 值 | TTL |
| :-- | :-- | :-- |
| `jwt:{jti}` | `userId` | 令牌过期时间 |
| `jwt_user:{userId}` | `Set<jti>` | 令牌过期时间 |

### 角色权限

| 功能 | 管理员 | 普通用户 |
| :--- | :---: | :---: |
| 设备激活 | ✅ | ✅ |
| 激活记录 | ✅ | ✅ |
| 用户管理 | ✅ | ❌ |
| 创建用户 | ✅ | ❌ |
| 登录日志 | ✅ | ❌ |

### API 接口

| 方法 | 端点 | 认证 | 说明 |
| :--- | :--- | :--: | :--- |
| POST | `/api/auth/login` | ❌ | 登录，返回 JWT |
| GET | `/api/auth/validate` | ✅ | 验证当前令牌 |
| POST | `/api/activation/activate` | ✅ | 激活设备 |
| GET | `/api/activation/download/{id}` | ✅ | 下载激活授权文件 |
| GET | `/api/records` | ✅ | 获取激活记录列表 |
| DELETE | `/api/records/{id}` | ✅ | 删除激活记录 |

## 🛠️ 技术栈

| 分类 | 技术 |
| :--- | :--- |
| 框架 | ASP.NET Core 10 Blazor Server |
| 数据库 | MySQL 5.7+（Pomelo EF Core） |
| 缓存/会话 | Redis（FreeRedis） |
| 认证 | JWT Bearer + Argon2 |
| 验证码 | SkiaSharp 3.x |
| 签名 | ECDSA P-256（System.Security.Cryptography） |

## 📄 项目结构

```
Laser.Activation.Web/
├── create_tables.sql                      # 数据库建表脚本
├── Laser.Activation.Web.slnx              # 解决方案文件
└── Laser.Activation.Web/
    └── Laser.Activation.Web/
        ├── Program.cs                     # 依赖注入、中间件、启动初始化
        ├── appsettings.json               # 配置文件
        ├── Models/                        # 实体与 DTO
        ├── Data/AppDbContext.cs           # EF Core 数据库上下文
        ├── Services/                      # 业务服务
        ├── Controllers/                   # API 控制器
        ├── Components/Pages/              # Blazor 页面
        └── wwwroot/js/auth.js             # 客户端认证模块
```

## 🗄 许可证

[MIT](LICENSE)
