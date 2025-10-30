# BCrypt Authentication System - Technical Documentation

## Table of Contents

1. [Overview](#overview)
2. [Architecture](#architecture)
3. [Key Components](#key-components)
4. [Authentication Flow](#authentication-flow)
5. [Password Security with BCrypt](#password-security-with-bcrypt)
6. [JWT Token Generation](#jwt-token-generation)
7. [Refresh Tokens](#refresh-tokens)
8. [Angular Frontend Implementation](#angular-frontend-implementation)
9. [Database Schema](#database-schema)
10. [Implementation Details](#implementation-details)
11. [Security Considerations](#security-considerations)
12. [API Endpoints](#api-endpoints)
13. [Testing Guide](#testing-guide)
14. [Troubleshooting](#troubleshooting)

---

## Overview

The BACKEND_CQRS project implements a secure authentication system using:

- **BCrypt** for password hashing (industry-standard, adaptive hashing algorithm)
- **JWT (JSON Web Tokens)** for stateless authentication
- **CQRS Pattern** with MediatR for clean separation of concerns
- **.NET 8** with Entity Framework Core for data access

### Why BCrypt?

BCrypt is specifically designed for password hashing and provides:
- **Adaptive complexity**: Can be made slower as computers get faster (via work factor)
- **Automatic salt generation**: Each password gets a unique salt
- **Resistance to rainbow tables**: Pre-computed hash tables are ineffective
- **Resistance to brute force**: Intentionally slow to compute

---

## Architecture

### Layered Architecture

```
???????????????????????????????????????????????????????????????
?                     Client Application                       ?
?                  (Web/Mobile/Desktop App)                    ?
???????????????????????????????????????????????????????????????
                          ? HTTP Request (JSON)
                          ?
???????????????????????????????????????????????????????????????
?                    API LAYER (Controllers)                   ?
?  ????????????????                                            ?
?  ?AuthController?  - Validates DTOs                          ?
?  ????????????????  - Returns HTTP responses                  ?
???????????????????????????????????????????????????????????????
          ? Sends Command via MediatR
          ?
???????????????????????????????????????????????????????????????
?              APPLICATION LAYER (CQRS Handlers)               ?
?  ??????????????????????    ??????????????????????            ?
?  ?RegisterCommand     ?    ?LoginCommand        ?            ?
?  ?Handler             ?    ?Handler             ?            ?
?  ??????????????????????    ??????????????????????            ?
?         ?                          ?                          ?
?         ????????????????????????????                          ?
?                   ? Uses Services                             ?
?????????????????????????????????????????????????????????????????
                    ?
???????????????????????????????????????????????????????????????
?              INFRASTRUCTURE LAYER (Services)                 ?
?  ???????????????????      ????????????????????              ?
?  ?PasswordService  ?      ?  TokenService    ?              ?
?  ? - HashPassword  ?      ? - GenerateToken  ?              ?
?  ? - VerifyPassword?      ?                  ?              ?
?  ???????????????????      ????????????????????              ?
?           ? BCrypt.Net               ? JWT                    ?
?????????????????????????????????????????????????????????????????
            ?                          ?
???????????????????????????????????????????????????????????????
?                  DATA ACCESS LAYER (EF Core)                 ?
?  ????????????????                                            ?
?  ? AppDbContext ?  - Entity Framework Core                   ?
?  ????????????????  - PostgreSQL Provider                     ?
???????????????????????????????????????????????????????????????
          ?
???????????????????????????????????????????????????????????????
?                    PostgreSQL Database                       ?
?  ????????????????????????????????????????????????            ?
?  ?  users table                                 ?            ?
?  ?  - id (PK)                                   ?            ?
?  ?  - email (unique)                            ?            ?
?  ?  - password_hash (BCrypt hash)               ?            ?
?  ?  - name, avatar_url, is_active, etc.         ?            ?
?  ????????????????????????????????????????????????            ?
???????????????????????????????????????????????????????????????
```

---

## Key Components

### 1. Domain Layer

#### Users Entity (`BACKEND_CQRS.Domain/Entities/Users.cs`)

```csharp
[Table("users")]
public class Users
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public string Email { get; set; }
    
    public string PasswordHash { get; set; }  // BCrypt hash stored here
    
    public string Name { get; set; }
    public string AvatarUrl { get; set; }
    public bool? IsActive { get; set; }
    public bool? IsSuperAdmin { get; set; }
    public DateTimeOffset? LastLogin { get; set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }  // Soft delete support
    
    // Navigation properties
    public ICollection<ProjectMembers> ProjectMembers { get; set; }
}
```

**Key Points:**
- `PasswordHash`: Stores the BCrypt-hashed password (never plain text)
- `IsActive`: Controls account activation status
- `IsSuperAdmin`: Flag for administrative privileges
- `DeletedAt`: Supports soft deletion (logical delete)

### 2. Application Layer

#### DTOs (Data Transfer Objects)

**LoginRequestDto**
```csharp
public class LoginRequestDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }
    
    [Required]
    public string Password { get; set; }  // Plain text, only in transit over HTTPS
}
```

**RegisterRequestDto**
```csharp
public class RegisterRequestDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }
    
    [Required]
    [MinLength(6)]
    public string Password { get; set; }  // Minimum 6 characters
    
    [Required]
    public string Name { get; set; }
    
    public string? AvatarUrl { get; set; }
}
```

**AuthResponseDto**
```csharp
public class AuthResponseDto
{
    public int UserId { get; set; }
    public string Email { get; set; }
    public string Name { get; set; }
    public string Token { get; set; }        // JWT token
    public DateTime ExpiresAt { get; set; }  // Token expiration
}
```

#### Commands

**RegisterCommand**
```csharp
public class RegisterCommand : IRequest<AuthResponseDto>
{
    public string Email { get; set; }
    public string Password { get; set; }
    public string Name { get; set; }
    public string? AvatarUrl { get; set; }
}
```

**LoginCommand**
```csharp
public class LoginCommand : IRequest<AuthResponseDto>
{
    public string Email { get; set; }
    public string Password { get; set; }
}
```

### 3. Infrastructure Layer

#### PasswordService (`BACKEND_CQRS.Infrastructure/Services/PasswordService.cs`)

```csharp
public class PasswordService : IPasswordService
{
    public string HashPassword(string password)
    {
        // BCrypt with work factor 12 (2^12 = 4,096 iterations)
        return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
    }

    public bool VerifyPassword(string password, string passwordHash)
    {
        // Compares plain password with BCrypt hash
        return BCrypt.Net.BCrypt.Verify(password, passwordHash);
    }
}
```

**Work Factor Explanation:**
- Work factor of 12 means 2^12 = 4,096 iterations
- Higher work factor = slower hashing = more secure
- Can be increased over time as hardware improves
- Current recommendation: 10-12 for web applications

#### TokenService (`BACKEND_CQRS.Infrastructure/Services/TokenService.cs`)

```csharp
public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;

    public string GenerateToken(Users user)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"];
        var issuer = jwtSettings["Issuer"];
        var audience = jwtSettings["Audience"];
        var expiryMinutes = int.Parse(jwtSettings["ExpiryMinutes"] ?? "60");

        // Create claims (user information embedded in token)
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.Name ?? user.Email),
            new Claim("is_super_admin", user.IsSuperAdmin?.ToString() ?? "false")
        };

        // Create signing credentials
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // Generate token
        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
```

---

## Authentication Flow

### Registration Flow

```
????????????                                                     ????????????
?  Client  ?                                                     ?  Server  ?
????????????                                                     ????????????
      ?                                                               ?
      ?  POST /api/auth/register                                      ?
      ?  {                                                            ?
      ?    "email": "user@example.com",                               ?
      ?    "password": "SecurePass123",                               ?
      ?    "name": "John Doe"                                         ?
      ?  }                                                            ?
      ?????????????????????????????????????????????????????????????????
      ?                                                               ?
      ?                                    ????????????????????????   ?
      ?                                    ? 1. Validate DTO      ?   ?
      ?                                    ????????????????????????   ?
      ?                                               ?               ?
      ?                                    ????????????????????????   ?
      ?                                    ? 2. Check if email    ?   ?
      ?                                    ?    already exists    ?   ?
      ?                                    ????????????????????????   ?
      ?                                               ?               ?
      ?                                    ????????????????????????   ?
      ?                                    ? 3. Hash password     ?   ?
      ?                                    ?    using BCrypt      ?   ?
      ?                                    ?    (work factor 12)  ?   ?
      ?                                    ????????????????????????   ?
      ?                                               ?               ?
      ?           Password: "SecurePass123"          ?               ?
      ?                      ?                        ?               ?
      ?                      ?                        ?               ?
      ?           BCrypt.HashPassword()               ?               ?
      ?                      ?                        ?               ?
      ?                      ?                        ?               ?
      ?     Hash: "$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/r3vXQ..."  ?
      ?                                               ?               ?
      ?                                    ????????????????????????   ?
      ?                                    ? 4. Create new user   ?   ?
      ?                                    ?    in database       ?   ?
      ?                                    ????????????????????????   ?
      ?                                               ?               ?
      ?                                    ????????????????????????   ?
      ?                                    ? 5. Generate JWT      ?   ?
      ?                                    ?    token             ?   ?
      ?                                    ????????????????????????   ?
      ?                                               ?               ?
      ?  200 OK                                       ?               ?
      ?  {                                                            ?
      ?    "userId": 1,                                               ?
      ?    "email": "user@example.com",                               ?
      ?    "name": "John Doe",                                        ?
      ?    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6...",                ?
      ?    "expiresAt": "2024-01-15T14:30:00Z"                        ?
      ?  }                                                            ?
      ?????????????????????????????????????????????????????????????????
      ?                                                               ?
```

**Step-by-Step Process:**

1. **Client sends registration request** with email, password, and name
2. **Controller validates** the DTO (email format, password length, etc.)
3. **Handler checks** if email already exists in database
4. **PasswordService hashes** the password using BCrypt
   - Original: `"SecurePass123"`
   - Hashed: `"$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/r3vXQ..."`
5. **New user record** is created with hashed password
6. **TokenService generates** JWT token with user claims
7. **Response returned** with user info and token

### Login Flow

```
????????????                                                     ????????????
?  Client  ?                                                     ?  Server  ?
????????????                                                     ????????????
      ?                                                               ?
      ?  POST /api/auth/login                                         ?
      ?  {                                                            ?
      ?    "email": "user@example.com",                               ?
      ?    "password": "SecurePass123"                                ?
      ?  }                                                            ?
      ?????????????????????????????????????????????????????????????????
      ?                                                               ?
      ?                                    ????????????????????????   ?
      ?                                    ? 1. Find user by      ?   ?
      ?                                    ?    email             ?   ?
      ?                                    ????????????????????????   ?
      ?                                               ?               ?
      ?                                    ????????????????????????   ?
      ?                                    ? 2. Check if active   ?   ?
      ?                                    ?    and not deleted   ?   ?
      ?                                    ????????????????????????   ?
      ?                                               ?               ?
      ?                                    ????????????????????????   ?
      ?                                    ? 3. Verify password   ?   ?
      ?                                    ?    using BCrypt      ?   ?
      ?                                    ????????????????????????   ?
      ?                                               ?               ?
      ?           Input: "SecurePass123"             ?               ?
      ?           Stored: "$2a$12$LQv3c1y..."        ?               ?
      ?                      ?                        ?               ?
      ?                      ?                        ?               ?
      ?           BCrypt.Verify(input, stored)        ?               ?
      ?                      ?                        ?               ?
      ?                      ?                        ?               ?
      ?                   ? Match!                    ?               ?
      ?                                               ?               ?
      ?                                    ????????????????????????   ?
      ?                                    ? 4. Update last_login ?   ?
      ?                                    ????????????????????????   ?
      ?                                               ?               ?
      ?                                    ????????????????????????   ?
      ?                                    ? 5. Generate JWT      ?   ?
      ?                                    ?    token             ?   ?
      ?                                    ????????????????????????   ?
      ?                                               ?               ?
      ?  200 OK                                       ?               ?
      ?  {                                                            ?
      ?    "userId": 1,                                               ?
      ?    "email": "user@example.com",                               ?
      ?    "name": "John Doe",                                        ?
      ?    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6...",                ?
      ?    "expiresAt": "2024-01-15T14:30:00Z"                        ?
      ?  }                                                            ?
      ?????????????????????????????????????????????????????????????????
      ?                                                               ?
```

**Step-by-Step Process:**

1. **Client sends login request** with email and password
2. **Handler finds user** in database by email
3. **Check account status**: Active and not deleted
4. **PasswordService verifies** password against stored hash
   - Uses BCrypt.Verify() to compare
   - Hash contains salt, so same password produces different hashes
5. **Update last_login** timestamp
6. **TokenService generates** fresh JWT token
7. **Response returned** with token for subsequent requests

### Authenticated Request Flow

```
????????????                                                     ????????????
?  Client  ?                                                     ?  Server  ?
????????????                                                     ????????????
      ?                                                               ?
      ?  GET /api/projects                                            ?
      ?  Headers:                                                     ?
      ?    Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6...      ?
      ?????????????????????????????????????????????????????????????????
      ?                                                               ?
      ?                                    ????????????????????????   ?
      ?                                    ? 1. Extract JWT from  ?   ?
      ?                                    ?    Authorization     ?   ?
      ?                                    ?    header            ?   ?
      ?                                    ????????????????????????   ?
      ?                                               ?               ?
      ?                                    ????????????????????????   ?
      ?                                    ? 2. Validate JWT      ?   ?
      ?                                    ?    - Signature       ?   ?
      ?                                    ?    - Expiration      ?   ?
      ?                                    ?    - Issuer/Audience ?   ?
      ?                                    ????????????????????????   ?
      ?                                               ?               ?
      ?                                    ????????????????????????   ?
      ?                                    ? 3. Extract claims    ?   ?
      ?                                    ?    - UserId          ?   ?
      ?                                    ?    - Email           ?   ?
      ?                                    ?    - Roles, etc.     ?   ?
      ?                                    ????????????????????????   ?
      ?                                               ?               ?
      ?                                    ????????????????????????   ?
      ?                                    ? 4. Execute request   ?   ?
      ?                                    ?    with user context ?   ?
      ?                                    ????????????????????????   ?
      ?                                               ?               ?
      ?  200 OK                                       ?               ?
      ?  { ...projects data... }                                      ?
      ?????????????????????????????????????????????????????????????????
      ?                                                               ?
```

---

## Password Security with BCrypt

### BCrypt Hash Format

A BCrypt hash looks like this:
```
$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/r3vXQ.vWsLSqUbhI6
```

Breaking it down:
```
$2a    $12    $LQv3c1yqBWVHxkd0LHAkCO    Yz6TtxMQJqhN8/r3vXQ.vWsLSqUbhI6
 ?      ?              ?                              ?
 ?      ?              ?                              ?? Hash (31 chars)
 ?      ?              ?? Salt (22 chars)
 ?      ?? Work factor (2^12 = 4,096 rounds)
 ?? Algorithm identifier (2a = BCrypt)
```

**Components:**
- **Algorithm**: `$2a$` indicates BCrypt variant
- **Work Factor**: `12` = 2^12 = 4,096 iterations
- **Salt**: Random 22-character string (embedded in hash)
- **Hash**: The actual password hash (31 characters)

### How BCrypt Works

1. **Salt Generation**
   ```
   Random Salt: "LQv3c1yqBWVHxkd0LHAkCO"
   ```

2. **Key Derivation**
   ```
   Input: Password + Salt
   Process: Blowfish cipher, 2^12 iterations
   Output: Hash
   ```

3. **Storage**
   ```
   Database stores complete hash string including:
   - Algorithm version
   - Work factor
   - Salt
   - Hash
   ```

4. **Verification**
   ```csharp
   // When user logs in:
   inputPassword = "SecurePass123"
   storedHash = "$2a$12$LQv3c1yqBWVHxkd0LHAkCO..."
   
   // BCrypt extracts salt from storedHash
   // Hashes inputPassword with same salt and work factor
   // Compares resulting hash with stored hash
   
   bool isValid = BCrypt.Verify(inputPassword, storedHash);
   ```

### Security Benefits

**1. Rainbow Table Resistance**
- Each password gets unique salt
- Pre-computed hash tables are useless
- Same password produces different hashes

Example:
```
User 1: Password "hello123" ? $2a$12$abc...def123
User 2: Password "hello123" ? $2a$12$xyz...ghi456
                                      ? Different salts, different hashes
```

**2. Brute Force Resistance**
- Work factor makes hashing intentionally slow
- With factor 12: ~180ms per hash attempt
- Makes brute force attacks impractical

Example attack:
```
Attempt rate: 5 hashes/second (due to work factor)
Password space: 10^8 (8-char alphanumeric)
Time to crack: 10^8 / 5 / 60 / 60 / 24 / 365 ? 634 years
```

**3. Adaptive Security**
- Work factor can be increased over time
- As hardware improves, increase work factor
- Existing hashes remain valid

```csharp
// Current: work factor 12 (~180ms)
BCrypt.HashPassword(password, 12);

// Future: work factor 13 (~360ms)
BCrypt.HashPassword(password, 13);

// Can rehash passwords on login with new factor
```

---

## JWT Token Generation

### Token Structure

A JWT token has three parts separated by dots:

```
eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c
 ?                                      ?                                                                                        ?
 ?? Header (Algorithm & Token Type)     ?? Payload (Claims/User Data)                                                           ?? Signature
```

### Header (Base64 encoded)
```json
{
  "alg": "HS256",
  "typ": "JWT"
}
```

### Payload (Base64 encoded)
```json
{
  "nameid": "1",
  "email": "user@example.com",
  "name": "John Doe",
  "is_super_admin": "false",
  "nbf": 1704294000,
  "exp": 1704297600,
  "iat": 1704294000,
  "iss": "YourAppName",
  "aud": "YourAppUsers"
}
```

**Standard Claims:**
- `nameid`: User ID (ClaimTypes.NameIdentifier)
- `email`: User email
- `name`: User display name
- `nbf`: Not Before (token not valid before this time)
- `exp`: Expiration time (token expires)
- `iat`: Issued At
- `iss`: Issuer (your application)
- `aud`: Audience (who can use this token)

**Custom Claims:**
- `is_super_admin`: Admin flag

### Signature

```
HMACSHA256(
  base64UrlEncode(header) + "." + base64UrlEncode(payload),
  secret_key
)
```

The signature ensures:
- Token hasn't been tampered with
- Token was issued by your server
- Token integrity

---

## Refresh Tokens

### What are Refresh Tokens?

- Long-lived tokens used to obtain new access tokens
- Improve user experience: No need to re-login frequently
- Enhances security: Access tokens can have short lifetimes

### Refresh Token Flow

```
????????????                 ????????????
?  Client  ?                 ?  Server  ?
????????????                 ????????????
      ?                           ?
      ?  POST /api/auth/refresh   ?
      ?  {                        ?
      ?    "refreshToken": "..."  ?
      ?  }                        ?
      ??????????????????????????? ?
      ?                           ?
      ?                ???????????????????????
      ?                ? 1. Validate refresh ?
      ?                ?    token            ?
      ?                ???????????????????????
      ?                           ?
      ?                ??????????????????????
      ?                ? 2. Generate new    ?
      ?                ?    access token    ?
      ?                ??????????????????????
      ?                           ?
      ?  200 OK                   ?
      ?  {                        ?
      ?    "accessToken": "..."   ?
      ?  }                        ?
      ?????????????????????????????
      ?                           ?
```

**Steps:**
1. Client sends refresh token to server
2. Server validates the refresh token
3. If valid, server generates a new access token (and optionally a new refresh token)
4. Client uses the new access token for authenticated requests

### Database Schema Changes

#### Users Table

- Add `refresh_token` column: Stores the current refresh token
- Add `refresh_token_expires` column: Stores refresh token expiration time

```sql
ALTER TABLE users
ADD COLUMN refresh_token VARCHAR(255),
ADD COLUMN refresh_token_expires TIMESTAMP WITH TIME ZONE;
```

#### Token Revocation

- Optionally, implement token revocation (e.g., on password change)
- Store revoked tokens in a separate table or blacklist
- Check against revoked tokens during authentication

```sql
CREATE TABLE revoked_tokens (
    id SERIAL PRIMARY KEY,
    token VARCHAR(255) NOT NULL,
    user_id INT REFERENCES users(id),
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);
```

---

## Angular Frontend Implementation

### Auth Service

```typescript
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = 'http://localhost:5000/api/auth';

  constructor(private http: HttpClient, private router: Router) {}

  register(email: string, password: string, name: string) {
    return this.http.post(`${this.apiUrl}/register`, {
      email,
      password,
      name
    });
  }

  login(email: string, password: string) {
    return this.http.post(`${this.apiUrl}/login`, {
      email,
      password
    });
  }

  refreshToken() {
    const refreshToken = this.getRefreshToken();
    return this.http.post(`${this.apiUrl}/refresh`, {
      refreshToken
    });
  }

  private getRefreshToken(): string | null {
    return localStorage.getItem('refresh_token');
  }
}
```

### Components

#### Login Component

```typescript
import { Component } from '@angular/core';
import { AuthService } from '../auth.service';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html'
})
export class LoginComponent {
  email: string = '';
  password: string = '';

  constructor(private authService: AuthService) {}

  onSubmit() {
    this.authService.login(this.email, this.password).subscribe(
      (response: any) => {
        // Store tokens in local storage
        localStorage.setItem('access_token', response.token);
        localStorage.setItem('refresh_token', response.refreshToken);
      },
      (error) => {
        console.error('Login error', error);
      }
    );
  }
}
```

#### Register Component

```typescript
import { Component } from '@angular/core';
import { AuthService } from '../auth.service';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html'
})
export class RegisterComponent {
  email: string = '';
  password: string = '';
  name: string = '';

  constructor(private authService: AuthService) {}

  onSubmit() {
    this.authService.register(this.email, this.password, this.name).subscribe(
      (response) => {
        console.log('Registration successful', response);
      },
      (error) => {
        console.error('Registration error', error);
      }
    );
  }
}
```

### Best Practices

- **Secure Storage**: Store tokens in secure storage (e.g., HttpOnly cookies or secure Angular services)
- **Short-lived Access Tokens**: Keep access token lifetime short (e.g., 15 minutes)
- **Long-lived Refresh Tokens**: Refresh tokens can be valid for days or weeks
- **Revoke on Logout**: Revoke refresh tokens on user logout
- **Rotation**: Optionally, implement refresh token rotation (new refresh token with each access token)
- **Secure Transmission**: Always use HTTPS to protect tokens in transit

---

## Database Schema

### Users Table

```sql
CREATE TABLE users (
    id SERIAL PRIMARY KEY,
    email VARCHAR(255) NOT NULL UNIQUE,
    password_hash VARCHAR(255),              -- BCrypt hash stored here
    name VARCHAR(255),
    avatar_url VARCHAR(500),
    is_active BOOLEAN DEFAULT TRUE,
    is_super_admin BOOLEAN DEFAULT FALSE,
    last_login TIMESTAMP WITH TIME ZONE,
    created_at TIMESTAMP WITH TIME ZONE,
    updated_at TIMESTAMP WITH TIME ZONE,
    version INTEGER DEFAULT 1,
    deleted_at TIMESTAMP WITH TIME ZONE,     -- For soft deletes
    jira_id VARCHAR(100),
    type VARCHAR(50),
    refresh_token VARCHAR(255),
    refresh_token_expires TIMESTAMP WITH TIME ZONE
);

CREATE INDEX idx_users_email ON users(email);
CREATE INDEX idx_users_deleted_at ON users(deleted_at);
```

**Key Fields:**
- `password_hash`: Stores BCrypt hash (60 characters)
- `is_active`: Account activation status
- `is_super_admin`: Admin privileges flag
- `deleted_at`: NULL = active, NOT NULL = soft deleted
- `refresh_token`: Stores refresh token
- `refresh_token_expires`: Stores refresh token expiration time

### Example Data

```sql
INSERT INTO users (email, password_hash, name, is_active, created_at, refresh_token, refresh_token_expires)
VALUES (
    'john.doe@example.com',
    '$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/r3vXQ.vWsLSqUbhI6',
    'John Doe',
    TRUE,
    NOW(),
    'sample_refresh_token',
    NOW() + INTERVAL '7 days'
);
```

---

## Implementation Details

### Configuration (appsettings.json)

```json
{
  "JwtSettings": {
    "SecretKey": "YourSuperSecretKeyThatIsAtLeast32CharactersLong!",
    "Issuer": "PMT-Backend",
    "Audience": "PMT-Users",
    "ExpiryMinutes": "60"
  },
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=pmt_db;Username=postgres;Password=***"
  }
}
```

**Important:**
- `SecretKey`: Must be at least 256 bits (32+ characters)
- `SecretKey`: NEVER commit to source control (use User Secrets or environment variables)
- `ExpiryMinutes`: Token lifetime (60 = 1 hour)

### Service Registration (Program.cs)

```csharp
// Add services
builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<ITokenService, TokenService>();

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"];

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(secretKey)
            ),
            ClockSkew = TimeSpan.Zero  // No tolerance for expired tokens
        };
    });

// Add authorization
builder.Services.AddAuthorization();

// In middleware pipeline
app.UseAuthentication();  // MUST come before UseAuthorization()
app.UseAuthorization();
```

### Protecting Endpoints

```csharp
[ApiController]
[Route("api/[controller]")]
public class ProjectsController : ControllerBase
{
    // Anyone can access (no authentication required)
    [HttpGet("public")]
    [AllowAnonymous]
    public IActionResult GetPublicProjects()
    {
        return Ok("Public data");
    }

    // Requires authentication
    [HttpGet]
    [Authorize]
    public IActionResult GetProjects()
    {
        // Extract user ID from JWT claims
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        
        return Ok($"Projects for user {userId}");
    }

    // Requires authentication AND super admin
    [HttpDelete("{id}")]
    [Authorize]
    public IActionResult DeleteProject(Guid id)
    {
        var isSuperAdmin = User.FindFirst("is_super_admin")?.Value == "true";
        
        if (!isSuperAdmin)
        {
            return Forbid();  // 403 Forbidden
        }
        
        // Delete logic
        return NoContent();
    }
}
```

---

## Security Considerations

### 1. Password Policy

**Implemented:**
- Minimum 6 characters (via `[MinLength(6)]`)

**Recommendations to Add:**
```csharp
public class PasswordValidator
{
    public static bool IsValid(string password, out string error)
    {
        if (password.Length < 8)
        {
            error = "Password must be at least 8 characters";
            return false;
        }
        
        if (!password.Any(char.IsUpper))
        {
            error = "Password must contain uppercase letter";
            return false;
        }
        
        if (!password.Any(char.IsLower))
        {
            error = "Password must contain lowercase letter";
            return false;
        }
        
        if (!password.Any(char.IsDigit))
        {
            error = "Password must contain a number";
            return false;
        }
        
        if (!password.Any(ch => !char.IsLetterOrDigit(ch)))
        {
            error = "Password must contain special character";
            return false;
        }
        
        error = null;
        return true;
    }
}
```

### 2. Rate Limiting

Prevent brute force attacks by limiting login attempts:

```csharp
// Install: Install-Package AspNetCoreRateLimit

// In Program.cs
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(options =>
{
    options.GeneralRules = new List<RateLimitRule>
    {
        new RateLimitRule
        {
            Endpoint = "POST:/api/auth/login",
            Period = "1m",
            Limit = 5  // Max 5 login attempts per minute
        }
    };
});
```

### 3. Account Lockout

Lock account after failed attempts:

```csharp
public class Users
{
    public int FailedLoginAttempts { get; set; }
    public DateTime? LockoutEnd { get; set; }
}

// In LoginCommandHandler
if (user.LockoutEnd > DateTime.UtcNow)
{
    throw new UnauthorizedAccessException(
        $"Account locked until {user.LockoutEnd}"
    );
}

if (!_passwordService.VerifyPassword(request.Password, user.PasswordHash))
{
    user.FailedLoginAttempts++;
    
    if (user.FailedLoginAttempts >= 5)
    {
        user.LockoutEnd = DateTime.UtcNow.AddMinutes(15);
    }
    
    await _context.SaveChangesAsync();
    throw new UnauthorizedAccessException("Invalid credentials");
}

// Reset on successful login
user.FailedLoginAttempts = 0;
user.LockoutEnd = null;
```

### 4. HTTPS Only

**Always use HTTPS in production:**

```csharp
// In Program.cs
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
    app.UseHttpsRedirection();
}
```

### 5. Secure JWT Secret

**Never hardcode secrets:**

```bash
# Use .NET User Secrets in development
dotnet user-secrets init
dotnet user-secrets set "JwtSettings:SecretKey" "your-secret-key"

# Use environment variables in production
export JwtSettings__SecretKey="your-production-secret"
```

### 6. Token Refresh

Implement refresh tokens for long-lived sessions:

```csharp
public class Users
{
    public string RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiry { get; set; }
}

// Refresh endpoint
[HttpPost("refresh")]
public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
{
    var user = await _context.Users
        .FirstOrDefaultAsync(u => u.RefreshToken == request.RefreshToken);
    
    if (user == null || user.RefreshTokenExpiry < DateTime.UtcNow)
    {
        return Unauthorized();
    }
    
    var newToken = _tokenService.GenerateToken(user);
    return Ok(new { token = newToken });
}
```

### 7. Email Verification

Add email verification for new accounts:

```csharp
public class Users
{
    public bool EmailVerified { get; set; }
    public string EmailVerificationToken { get; set; }
}

// Send verification email on registration
// Verify before allowing login
```

---

## API Endpoints

### POST /api/auth/register

**Register a new user**

**Request:**
```http
POST /api/auth/register HTTP/1.1
Content-Type: application/json

{
  "email": "john.doe@example.com",
  "password": "SecurePass123!",
  "name": "John Doe",
  "avatarUrl": "https://example.com/avatar.jpg"
}
```

**Response (200 OK):**
```json
{
  "userId": 1,
  "email": "john.doe@example.com",
  "name": "John Doe",
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAt": "2024-01-15T14:30:00Z"
}
```

**Error Response (400 Bad Request):**
```json
{
  "message": "User with this email already exists"
}
```

**Validation Errors:**
```json
{
  "errors": {
    "Email": ["The Email field is not a valid e-mail address."],
    "Password": ["The field Password must be a string with a minimum length of 6."]
  }
}
```

### POST /api/auth/login

**Authenticate existing user**

**Request:**
```http
POST /api/auth/login HTTP/1.1
Content-Type: application/json

{
  "email": "john.doe@example.com",
  "password": "SecurePass123!"
}
```

**Response (200 OK):**
```json
{
  "userId": 1,
  "email": "john.doe@example.com",
  "name": "John Doe",
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAt": "2024-01-15T14:30:00Z"
}
```

**Error Response (401 Unauthorized):**
```json
{
  "message": "Invalid email or password"
}
```

```json
{
  "message": "User account is inactive"
}
```

### Using the Token

**Protected endpoint request:**
```http
GET /api/projects HTTP/1.1
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Without token (401 Unauthorized):**
```http
GET /api/projects HTTP/1.1

Response:
{
  "status": 401,
  "message": "Unauthorized"
}
```

---

## Testing Guide

### 1. Manual Testing with Postman

**Register User:**
```
POST http://localhost:5000/api/auth/register
Content-Type: application/json

{
  "email": "test@example.com",
  "password": "Test123!",
  "name": "Test User"
}
```

**Login:**
```
POST http://localhost:5000/api/auth/login
Content-Type: application/json

{
  "email": "test@example.com",
  "password": "Test123!"
}
```

**Copy token from response, then:**
```
GET http://localhost:5000/api/projects
Authorization: Bearer {paste-token-here}
```

### 2. Unit Tests

```csharp
[Fact]
public void HashPassword_ShouldReturnValidBCryptHash()
{
    // Arrange
    var passwordService = new PasswordService();
    var password = "TestPassword123";

    // Act
    var hash = passwordService.HashPassword(password);

    // Assert
    Assert.NotNull(hash);
    Assert.StartsWith("$2a$12$", hash);
    Assert.True(passwordService.VerifyPassword(password, hash));
}

[Fact]
public void VerifyPassword_WithWrongPassword_ShouldReturnFalse()
{
    // Arrange
    var passwordService = new PasswordService();
    var hash = passwordService.HashPassword("CorrectPassword");

    // Act
    var result = passwordService.VerifyPassword("WrongPassword", hash);

    // Assert
    Assert.False(result);
}

[Fact]
public async Task RegisterCommand_WithExistingEmail_ShouldThrowException()
{
    // Arrange
    // ... setup context with existing user

    // Act & Assert
    await Assert.ThrowsAsync<InvalidOperationException>(
        () => handler.Handle(command, CancellationToken.None)
    );
}
```

### 3. Integration Tests

```csharp
[Fact]
public async Task Login_WithValidCredentials_ShouldReturnToken()
{
    // Arrange
    var client = _factory.CreateClient();
    var request = new
    {
        email = "test@example.com",
        password = "Test123!"
    };

    // Act
    var response = await client.PostAsJsonAsync("/api/auth/login", request);

    // Assert
    response.EnsureSuccessStatusCode();
    var result = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
    Assert.NotNull(result.Token);
}
```

### 4. Testing BCrypt Performance

```csharp
var stopwatch = Stopwatch.StartNew();
var hash = BCrypt.Net.BCrypt.HashPassword("test", workFactor: 12);
stopwatch.Stop();

Console.WriteLine($"Hash time (factor 12): {stopwatch.ElapsedMilliseconds}ms");
// Expected: ~150-250ms
```

---

## Troubleshooting

### Common Issues

#### 1. "401 Unauthorized" on Protected Endpoints

**Problem:** Token not being sent or validated

**Solutions:**
```csharp
// Check header format
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6...
// NOT: Authorization: eyJhbGciOiJIUzI1NiIsInR5cCI6...

// Verify middleware order in Program.cs
app.UseAuthentication();  // Must be before UseAuthorization
app.UseAuthorization();
app.MapControllers();

// Check JWT configuration
ValidateIssuerSigningKey = true,
IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
```

#### 2. "BCrypt.Net not found"

**Problem:** Package not installed

**Solution:**
```bash
dotnet add package BCrypt.Net-Next
```

#### 3. "Invalid email or password" on Correct Credentials

**Problem:** Password hashing/verification issue

**Debug:**
```csharp
// In LoginCommandHandler, add logging
_logger.LogInformation($"User found: {user != null}");
_logger.LogInformation($"Password hash: {user?.PasswordHash}");
_logger.LogInformation($"Verification result: {_passwordService.VerifyPassword(request.Password, user.PasswordHash)}");
```

**Check:**
- Password was hashed during registration
- Hash is being stored in database
- Correct password is being sent

#### 4. Token Expires Immediately

**Problem:** Clock skew or wrong expiry configuration

**Solution:**
```csharp
// Set appropriate expiry time
var token = new JwtSecurityToken(
    expires: DateTime.UtcNow.AddMinutes(60),  // Use UtcNow, not Now
    // ...
);

// Adjust clock skew if needed
ClockSkew = TimeSpan.FromMinutes(5)  // Allow 5 minutes tolerance
```

#### 5. CORS Issues

**Problem:** Frontend can't call auth endpoints

**Solution:**
```csharp
// In Program.cs
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// In middleware
app.UseCors("AllowFrontend");
```

### Debugging Tips

**1. Enable detailed logging:**
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore.Authentication": "Debug",
      "Microsoft.AspNetCore.Authorization": "Debug"
    }
  }
}
```

**2. Decode JWT token:**
Visit https://jwt.io and paste your token to see claims

**3. Test password hashing:**
```csharp
var password = "test123";
var hash = BCrypt.Net.BCrypt.HashPassword(password);
Console.WriteLine($"Hash: {hash}");
Console.WriteLine($"Verify: {BCrypt.Net.BCrypt.Verify(password, hash)}");
```

**4. Check database:**
```sql
SELECT email, password_hash, is_active, deleted_at 
FROM users 
WHERE email = 'test@example.com';
```

---

## Summary

### What We've Implemented

? **Secure password storage** using BCrypt with work factor 12  
? **User registration** with automatic password hashing  
? **User login** with password verification  
? **JWT token generation** for stateless authentication  
? **Refresh tokens** for long-lived sessions  
? **Protected endpoints** using `[Authorize]` attribute  
? **CQRS pattern** with clean separation of concerns  
? **Account management** (active status, soft delete, last login)

### Key Takeaways

1. **Never store plain passwords** - Always use BCrypt or similar
2. **BCrypt handles salting** - Each hash is unique
3. **Work factor is adjustable** - Increase as hardware improves
4. **JWT is stateless** - Server doesn't store session
5. **Claims contain user info** - No database lookup needed for each request
6. **HTTPS is essential** - Protects passwords in transit
7. **Token expiration matters** - Balance security vs UX

### Next Steps

Consider implementing:
- Email verification
- Password reset functionality
- Refresh token rotation
- Two-factor authentication (2FA)
- Role-based authorization
- Permission-based authorization
- Account lockout after failed attempts
- Rate limiting on auth endpoints
- Audit logging for security events

---

## References

- [BCrypt.Net Documentation](https://github.com/BcryptNet/bcrypt.net)
- [JWT.IO](https://jwt.io)
- [OWASP Password Storage Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/Password_Storage_Cheat_Sheet.html)
- [Microsoft Authentication Documentation](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/)
- [NIST Password Guidelines](https://pages.nist.gov/800-63-3/sp800-63b.html)

---

**Document Version:** 1.1  
**Last Updated:** January 2024  
**Author:** PMT Backend Team
