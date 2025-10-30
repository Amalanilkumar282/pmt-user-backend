# Simple Authentication System Implementation Guide
## Using BCrypt Password Hashing in CQRS Architecture

---

## Table of Contents
1. [Overview](#overview)
2. [Architecture Components](#architecture-components)
3. [Authentication Flow](#authentication-flow)
4. [Implementation Details](#implementation-details)
5. [Security Considerations](#security-considerations)
6. [API Endpoints](#api-endpoints)
7. [Database Schema](#database-schema)
8. [Code Structure](#code-structure)
9. [Testing Strategy](#testing-strategy)

---

## Overview

This document outlines the implementation of a simple authentication system for the BACKEND_CQRS project using BCrypt for password hashing. The implementation follows the CQRS (Command Query Responsibility Segregation) pattern already established in the project and does **not** include role-based or permission-based authorization - focusing solely on user authentication (login and registration).

### Key Technologies
- **BCrypt.Net-Next**: Industry-standard password hashing library
- **JWT (JSON Web Tokens)**: Stateless authentication tokens
- **MediatR**: CQRS pattern implementation
- **Entity Framework Core**: Data access with PostgreSQL

---

## Architecture Components

The authentication system integrates into the existing CQRS architecture with the following layers:

```
???????????????????????????????????????????????????????????????
?                      API Layer                               ?
?  - AuthController (Login, Register endpoints)                ?
???????????????????????????????????????????????????????????????
                            ?
???????????????????????????????????????????????????????????????
?                   Application Layer                          ?
?  Commands:                      Queries:                     ?
?  - RegisterUserCommand          - LoginUserQuery             ?
?  - LoginUserCommand (optional)                               ?
?                                                               ?
?  Handlers:                                                    ?
?  - RegisterUserCommandHandler                                ?
?  - LoginUserQueryHandler                                     ?
?                                                               ?
?  DTOs:                                                        ?
?  - RegisterUserDto, LoginRequestDto, AuthResponseDto         ?
???????????????????????????????????????????????????????????????
                            ?
???????????????????????????????????????????????????????????????
?                   Infrastructure Layer                       ?
?  Services:                                                    ?
?  - IPasswordHasher (BCrypt implementation)                   ?
?  - IJwtTokenGenerator (JWT token creation)                   ?
?                                                               ?
?  Repositories:                                                ?
?  - IUserRepository (existing, extended if needed)            ?
???????????????????????????????????????????????????????????????
                            ?
???????????????????????????????????????????????????????????????
?                      Domain Layer                            ?
?  Entities:                                                    ?
?  - Users (existing entity with password_hash field)          ?
???????????????????????????????????????????????????????????????
```

---

## Authentication Flow

### 1. User Registration Flow

```
????????????         ????????????????         ???????????????????????
?  Client  ?         ? AuthController?         ? RegisterUserHandler ?
????????????         ????????????????         ???????????????????????
      ?                     ?                             ?
      ? POST /api/auth/     ?                             ?
      ?    register         ?                             ?
      ?????????????????????>?                             ?
      ?                     ?                             ?
      ?                     ? RegisterUserCommand         ?
      ?                     ?????????????????????????????>?
      ?                     ?                             ?
      ?                     ?                             ???????????
      ?                     ?                             ? 1. Validate email?
      ?                     ?                             ? 2. Check if user exists?
      ?                     ?                             ? 3. Hash password (BCrypt)?
      ?                     ?                             ? 4. Create user entity?
      ?                     ?                             ? 5. Save to database?
      ?                     ?                             ? 6. Generate JWT token?
      ?                     ?                             ?<?????????
      ?                     ?                             ?
      ?                     ?  AuthResponseDto            ?
      ?                     ?<?????????????????????????????
      ?                     ?                             ?
      ?  200 OK + JWT Token ?                             ?
      ?<?????????????????????                             ?
      ?                     ?                             ?
```

**Steps Explained:**
1. User submits registration form with email, password, and name
2. Controller receives request and creates `RegisterUserCommand`
3. MediatR routes command to `RegisterUserCommandHandler`
4. Handler validates the input data
5. Handler checks if email already exists in database
6. Password is hashed using BCrypt with automatic salt generation
7. New `Users` entity is created with hashed password
8. Entity is saved to PostgreSQL database via repository
9. JWT token is generated with user ID and email claims
10. Response contains user data and JWT token

### 2. User Login Flow

```
????????????         ????????????????         ???????????????????
?  Client  ?         ? AuthController?         ? LoginUserHandler?
????????????         ????????????????         ???????????????????
      ?                     ?                             ?
      ? POST /api/auth/     ?                             ?
      ?    login            ?                             ?
      ?????????????????????>?                             ?
      ?                     ?                             ?
      ?                     ? LoginUserQuery              ?
      ?                     ?????????????????????????????>?
      ?                     ?                             ?
      ?                     ?                             ???????????
      ?                     ?                             ? 1. Find user by email?
      ?                     ?                             ? 2. Verify password (BCrypt)?
      ?                     ?                             ? 3. Check if active?
      ?                     ?                             ? 4. Update last_login?
      ?                     ?                             ? 5. Generate JWT token?
      ?                     ?                             ?<?????????
      ?                     ?                             ?
      ?                     ?  AuthResponseDto            ?
      ?                     ?<?????????????????????????????
      ?                     ?                             ?
      ?  200 OK + JWT Token ?                             ?
      ?<?????????????????????                             ?
      ?                     ?                             ?
```

**Steps Explained:**
1. User submits login credentials (email and password)
2. Controller receives request and creates `LoginUserQuery`
3. MediatR routes query to `LoginUserQueryHandler`
4. Handler finds user by email in database
5. BCrypt verifies submitted password against stored hash
6. Handler checks if user account is active (`is_active = true`)
7. Updates `last_login` timestamp in database
8. JWT token is generated with user claims
9. Response contains user data and JWT token

---

## Implementation Details

### 1. BCrypt Password Hashing

**Why BCrypt?**
- **Adaptive**: Automatically salts passwords
- **Slow by design**: Resistant to brute-force attacks
- **Configurable work factor**: Can increase difficulty over time
- **Industry standard**: Widely tested and proven secure

**How BCrypt Works:**

```
Plain Password: "MySecurePass123"
                    ?
        BCrypt Hash Function
        (with automatic salt)
                    ?
Hashed Password: "$2a$11$vE7Lj9Z8..."
                 ?  ?  ?
                 ?  ?  ??> Salt + Hash
                 ?  ?????> Work Factor (2^11 iterations)
                 ????????> BCrypt Version
```

**Code Example:**
```csharp
// Hashing a password (during registration)
string hashedPassword = BCrypt.Net.BCrypt.HashPassword(plainPassword, workFactor: 11);

// Verifying a password (during login)
bool isValid = BCrypt.Net.BCrypt.Verify(plainPassword, storedHash);
```

**Work Factor Explanation:**
- Work factor of 11 means 2^11 = 2,048 iterations
- Higher = more secure but slower
- Recommended: 10-12 for modern applications
- Can be increased over time as hardware improves

### 2. JWT Token Generation

**JWT Structure:**
```
Header.Payload.Signature

Example:
eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1c2VySWQiOiIxMjMiLCJlbWFpbCI6InRlc3RAZXhhbXBsZS5jb20iLCJleHAiOjE2OTk5OTk5OTl9.signature
```

**Token Contains:**
- **Header**: Algorithm and token type
- **Payload**: User claims (userId, email, name, expiration)
- **Signature**: HMAC-SHA256 signature for verification

**Token Lifecycle:**
```
Token Generation (Login/Register)
        ?
Client stores token (localStorage/sessionStorage)
        ?
Client includes token in Authorization header
        ?
Server validates token on protected endpoints
        ?
Token expires after configured time (e.g., 24 hours)
```

**Code Example:**
```csharp
var claims = new[]
{
    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
    new Claim(ClaimTypes.Email, user.Email),
    new Claim(ClaimTypes.Name, user.Name)
};

var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

var token = new JwtSecurityToken(
    issuer: "BACKEND_CQRS",
    audience: "Angular-Client",
    claims: claims,
    expires: DateTime.UtcNow.AddHours(24),
    signingCredentials: credentials
);

return new JwtSecurityTokenHandler().WriteToken(token);
```

### 3. Database Interaction

The existing `Users` entity already has the necessary field:

```csharp
[Column("password_hash")]
public string PasswordHash { get; set; }
```

**Registration - Database Insert:**
```sql
INSERT INTO users (email, password_hash, name, is_active, created_at, updated_at)
VALUES ('user@example.com', '$2a$11$vE7Lj9Z8...', 'John Doe', true, NOW(), NOW());
```

**Login - Database Query:**
```sql
SELECT id, email, password_hash, name, is_active, last_login
FROM users
WHERE email = 'user@example.com' AND deleted_at IS NULL;
```

**Update Last Login:**
```sql
UPDATE users
SET last_login = NOW(), updated_at = NOW()
WHERE id = 123;
```

---

## Security Considerations

### 1. Password Requirements
- **Minimum length**: 8 characters
- **Recommended**: Include uppercase, lowercase, numbers, and special characters
- **Validation**: Implemented in Command validators using FluentValidation

### 2. Email Validation
- Valid email format check
- Uniqueness constraint in database
- Case-insensitive comparison

### 3. Password Storage
- **NEVER** store plain-text passwords
- Always use BCrypt hashing with salt
- Password hash stored in `password_hash` column

### 4. Token Security
- JWT secret key stored in `appsettings.json` (use environment variables in production)
- Token expiration set to reasonable time (24 hours recommended)
- HTTPS required in production to prevent token interception
- Tokens should be stored securely on client (httpOnly cookies preferred over localStorage)

### 5. Rate Limiting
- Implement rate limiting on login endpoint to prevent brute-force attacks
- Example: 5 failed attempts per IP per 15 minutes

### 6. Account Security
- Check `is_active` flag before allowing login
- Check `deleted_at IS NULL` to exclude deleted accounts
- Optional: Implement account lockout after multiple failed login attempts

### 7. CORS Configuration
Already configured for Angular dev:
```csharp
policy.WithOrigins("http://localhost:4200")
```

---

## API Endpoints

### 1. Register User

**Endpoint:** `POST /api/auth/register`

**Request Body:**
```json
{
  "email": "john.doe@example.com",
  "password": "SecurePass123!",
  "name": "John Doe"
}
```

**Response (Success - 200 OK):**
```json
{
  "success": true,
  "data": {
    "userId": 123,
    "email": "john.doe@example.com",
    "name": "John Doe",
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "expiresAt": "2024-01-15T10:30:00Z"
  },
  "message": "User registered successfully",
  "statusCode": 200
}
```

**Response (Error - 400 Bad Request):**
```json
{
  "success": false,
  "data": null,
  "message": "Email already exists",
  "statusCode": 400,
  "errors": ["Email 'john.doe@example.com' is already registered"]
}
```

### 2. Login User

**Endpoint:** `POST /api/auth/login`

**Request Body:**
```json
{
  "email": "john.doe@example.com",
  "password": "SecurePass123!"
}
```

**Response (Success - 200 OK):**
```json
{
  "success": true,
  "data": {
    "userId": 123,
    "email": "john.doe@example.com",
    "name": "John Doe",
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "expiresAt": "2024-01-15T10:30:00Z"
  },
  "message": "Login successful",
  "statusCode": 200
}
```

**Response (Error - 401 Unauthorized):**
```json
{
  "success": false,
  "data": null,
  "message": "Invalid email or password",
  "statusCode": 401,
  "errors": ["Authentication failed"]
}
```

### 3. Protected Endpoint Example

Any existing endpoint can be protected by adding `[Authorize]` attribute:

**Request Header:**
```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

---

## Database Schema

The existing `users` table already supports authentication:

```sql
CREATE TABLE users (
    id SERIAL PRIMARY KEY,
    email VARCHAR(255) NOT NULL UNIQUE,
    password_hash VARCHAR(255),          -- BCrypt hash stored here
    name VARCHAR(255),
    avatar_url VARCHAR(500),
    is_active BOOLEAN DEFAULT true,      -- Account status check
    is_super_admin BOOLEAN DEFAULT false,
    last_login TIMESTAMP WITH TIME ZONE, -- Updated on each login
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    version INTEGER,
    deleted_at TIMESTAMP WITH TIME ZONE, -- Soft delete check
    jira_id VARCHAR(255),
    type VARCHAR(50)
);

CREATE INDEX idx_users_email ON users(email);
CREATE INDEX idx_users_deleted_at ON users(deleted_at) WHERE deleted_at IS NULL;
```

**Key Columns for Authentication:**
- `email`: Unique identifier for login
- `password_hash`: BCrypt hashed password
- `is_active`: Enable/disable user accounts
- `deleted_at`: Soft delete (NULL = active account)
- `last_login`: Track user activity

---

## Code Structure

### File Organization

```
BACKEND_CQRS.Domain/
??? Entities/
?   ??? Users.cs (existing - already has password_hash)
??? Persistance/
    ??? IUserRepository.cs (existing - may extend with FindByEmail)

BACKEND_CQRS.Application/
??? Command/
?   ??? Auth/
?       ??? RegisterUserCommand.cs
??? Query/
?   ??? Auth/
?       ??? LoginUserQuery.cs
??? Handler/
?   ??? Auth/
?       ??? RegisterUserCommandHandler.cs
?       ??? LoginUserQueryHandler.cs
??? Dto/
?   ??? Auth/
?       ??? RegisterUserDto.cs
?       ??? LoginRequestDto.cs
?       ??? AuthResponseDto.cs
??? Validators/
    ??? Auth/
        ??? RegisterUserCommandValidator.cs
        ??? LoginUserQueryValidator.cs

BACKEND_CQRS.Infrastructure/
??? Services/
?   ??? PasswordHasher/
?   ?   ??? IPasswordHasher.cs
?   ?   ??? BcryptPasswordHasher.cs
?   ??? TokenGenerator/
?       ??? IJwtTokenGenerator.cs
?       ??? JwtTokenGenerator.cs
??? Repository/
    ??? UserRepository.cs (existing - extend if needed)

BACKEND_CQRS.Api/
??? Controllers/
?   ??? AuthController.cs
??? appsettings.json (add JWT configuration)
```

### Component Responsibilities

#### 1. **RegisterUserCommand** (Application Layer)
```csharp
public class RegisterUserCommand : IRequest<ApiResponse<AuthResponseDto>>
{
    public string Email { get; set; }
    public string Password { get; set; }
    public string Name { get; set; }
}
```

#### 2. **RegisterUserCommandHandler** (Application Layer)
- Validates input using FluentValidation
- Checks if email already exists
- Calls `IPasswordHasher.HashPassword()` to hash password
- Creates new `Users` entity with hashed password
- Saves user via repository
- Calls `IJwtTokenGenerator.GenerateToken()` to create JWT
- Returns `AuthResponseDto` with user data and token

#### 3. **LoginUserQuery** (Application Layer)
```csharp
public class LoginUserQuery : IRequest<ApiResponse<AuthResponseDto>>
{
    public string Email { get; set; }
    public string Password { get; set; }
}
```

#### 4. **LoginUserQueryHandler** (Application Layer)
- Finds user by email via repository
- Calls `IPasswordHasher.VerifyPassword()` to check password
- Validates user is active and not deleted
- Updates `last_login` timestamp
- Generates JWT token
- Returns `AuthResponseDto` with user data and token

#### 5. **IPasswordHasher & BcryptPasswordHasher** (Infrastructure Layer)
```csharp
public interface IPasswordHasher
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string hashedPassword);
}

public class BcryptPasswordHasher : IPasswordHasher
{
    private const int WorkFactor = 11;
    
    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);
    }
    
    public bool VerifyPassword(string password, string hashedPassword)
    {
        return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
    }
}
```

#### 6. **IJwtTokenGenerator & JwtTokenGenerator** (Infrastructure Layer)
```csharp
public interface IJwtTokenGenerator
{
    string GenerateToken(Users user);
}

public class JwtTokenGenerator : IJwtTokenGenerator
{
    // Implementation using JwtSecurityTokenHandler
    // Configurable via IOptions<JwtSettings>
}
```

#### 7. **AuthController** (API Layer)
```csharp
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterUserCommand command)
    {
        var result = await _mediator.Send(command);
        return result.Success ? Ok(result) : BadRequest(result);
    }
    
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginUserQuery query)
    {
        var result = await _mediator.Send(query);
        return result.Success ? Ok(result) : Unauthorized(result);
    }
}
```

---

## Testing Strategy

### 1. Unit Tests

**Password Hashing Tests:**
```csharp
[Test]
public void HashPassword_ShouldReturnDifferentHashesForSamePassword()
{
    var hasher = new BcryptPasswordHasher();
    var hash1 = hasher.HashPassword("password123");
    var hash2 = hasher.HashPassword("password123");
    
    Assert.AreNotEqual(hash1, hash2); // Different salts
    Assert.True(hasher.VerifyPassword("password123", hash1));
    Assert.True(hasher.VerifyPassword("password123", hash2));
}
```

**Registration Handler Tests:**
```csharp
[Test]
public async Task Handle_WithValidData_ShouldCreateUserAndReturnToken()
{
    // Arrange
    var command = new RegisterUserCommand 
    { 
        Email = "test@example.com",
        Password = "SecurePass123",
        Name = "Test User"
    };
    
    // Act
    var result = await _handler.Handle(command, CancellationToken.None);
    
    // Assert
    Assert.True(result.Success);
    Assert.NotNull(result.Data.Token);
    Assert.Equal("test@example.com", result.Data.Email);
}

[Test]
public async Task Handle_WithExistingEmail_ShouldReturnError()
{
    // Test duplicate email handling
}
```

**Login Handler Tests:**
```csharp
[Test]
public async Task Handle_WithCorrectCredentials_ShouldReturnToken()
{
    // Test successful login
}

[Test]
public async Task Handle_WithWrongPassword_ShouldReturnUnauthorized()
{
    // Test failed login
}

[Test]
public async Task Handle_WithInactiveUser_ShouldReturnError()
{
    // Test inactive account
}
```

### 2. Integration Tests

**Full Registration Flow:**
```csharp
[Test]
public async Task RegisterUser_ShouldStoreHashedPasswordInDatabase()
{
    // Test end-to-end registration with database
}
```

**Full Login Flow:**
```csharp
[Test]
public async Task LoginUser_ShouldUpdateLastLoginTimestamp()
{
    // Test end-to-end login with database
}
```

### 3. API Tests (Postman/Integration)

**Test Scenarios:**
1. Register new user ? Verify 200 OK with token
2. Register duplicate email ? Verify 400 Bad Request
3. Login with valid credentials ? Verify 200 OK with token
4. Login with wrong password ? Verify 401 Unauthorized
5. Login with non-existent email ? Verify 401 Unauthorized
6. Access protected endpoint without token ? Verify 401 Unauthorized
7. Access protected endpoint with valid token ? Verify 200 OK

---

## Configuration

### appsettings.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Host=...;Database=postgres;..."
  },
  "JwtSettings": {
    "SecretKey": "your-super-secret-key-min-32-characters-long!",
    "Issuer": "BACKEND_CQRS",
    "Audience": "Angular-Client",
    "ExpirationHours": 24
  },
  "PasswordSettings": {
    "RequireMinimumLength": 8,
    "RequireUppercase": false,
    "RequireDigit": false,
    "RequireSpecialCharacter": false,
    "BcryptWorkFactor": 11
  },
  "AllowedHosts": "*"
}
```

**?? Production Security:**
- Store `JwtSettings:SecretKey` in environment variables or Azure Key Vault
- Use strong, randomly generated secret keys (minimum 32 characters)
- Enable HTTPS only in production
- Rotate secret keys periodically

---

## NuGet Packages Required

Add these packages to respective projects:

### BACKEND_CQRS.Infrastructure
```bash
dotnet add package BCrypt.Net-Next --version 4.0.3
dotnet add package System.IdentityModel.Tokens.Jwt --version 7.0.3
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer --version 8.0.0
```

### BACKEND_CQRS.Application
```bash
dotnet add package FluentValidation --version 11.9.0
dotnet add package FluentValidation.DependencyInjectionExtensions --version 11.9.0
```

---

## Middleware Configuration (Program.cs)

```csharp
// Add Authentication & Authorization services
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
            ValidAudience = builder.Configuration["JwtSettings:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:SecretKey"]))
        };
    });

builder.Services.AddAuthorization();

// ... existing services ...

var app = builder.Build();

// Add Authentication & Authorization middleware (ORDER MATTERS!)
app.UseHttpsRedirection();
app.UseCors("AllowAngularDev");

app.UseAuthentication(); // ? Add this BEFORE UseAuthorization
app.UseAuthorization();

app.MapControllers();
```

---

## Error Handling

### Common Error Scenarios

| Scenario | HTTP Status | Response Message |
|----------|-------------|------------------|
| Registration - Email exists | 400 Bad Request | "Email already exists" |
| Registration - Invalid email | 400 Bad Request | "Invalid email format" |
| Registration - Weak password | 400 Bad Request | "Password does not meet requirements" |
| Login - Wrong credentials | 401 Unauthorized | "Invalid email or password" |
| Login - Inactive account | 403 Forbidden | "Account is inactive" |
| Login - Deleted account | 401 Unauthorized | "Invalid email or password" |
| Protected endpoint - No token | 401 Unauthorized | "Authorization header missing" |
| Protected endpoint - Invalid token | 401 Unauthorized | "Invalid token" |
| Protected endpoint - Expired token | 401 Unauthorized | "Token expired" |

### Security Best Practice: Generic Error Messages

For login failures, always return the same generic message ("Invalid email or password") whether the email doesn't exist or the password is wrong. This prevents attackers from discovering valid email addresses through enumeration attacks.

---

## Example: Complete Request/Response Flow

### Registration Example

**1. Client Request:**
```http
POST /api/auth/register HTTP/1.1
Host: localhost:5000
Content-Type: application/json

{
  "email": "jane.smith@example.com",
  "password": "MySecure123!",
  "name": "Jane Smith"
}
```

**2. Backend Processing:**
```
AuthController receives request
    ?
Creates RegisterUserCommand
    ?
MediatR sends to RegisterUserCommandHandler
    ?
Validator checks:
  - Email format ?
  - Password length ?
  - Email uniqueness ?
    ?
BcryptPasswordHasher.HashPassword("MySecure123!")
  ? Returns: "$2a$11$N9qo8uLOickgx2ZMRZoMye..."
    ?
Create Users entity:
  {
    Email: "jane.smith@example.com",
    PasswordHash: "$2a$11$N9qo8uLOickgx2ZMRZoMye...",
    Name: "Jane Smith",
    IsActive: true,
    CreatedAt: 2024-01-14T10:00:00Z
  }
    ?
UserRepository.AddAsync(user)
    ?
Database INSERT executed
    ?
JwtTokenGenerator.GenerateToken(user)
  ? Returns: "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
    ?
Return ApiResponse with AuthResponseDto
```

**3. Server Response:**
```http
HTTP/1.1 200 OK
Content-Type: application/json

{
  "success": true,
  "data": {
    "userId": 45,
    "email": "jane.smith@example.com",
    "name": "Jane Smith",
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1c2VySWQiOiI0NSIsImVtYWlsIjoiamFuZS5zbWl0aEBleGFtcGxlLmNvbSIsIm5hbWUiOiJKYW5lIFNtaXRoIiwiZXhwIjoxNzA1MzIwMDAwfQ.abc123xyz",
    "expiresAt": "2024-01-15T10:00:00Z"
  },
  "message": "User registered successfully",
  "statusCode": 200
}
```

**4. Database State:**
```sql
SELECT * FROM users WHERE id = 45;

id | email                   | password_hash              | name        | is_active | created_at
---|-------------------------|----------------------------|-------------|-----------|-------------------
45 | jane.smith@example.com  | $2a$11$N9qo8uLOick... | Jane Smith  | true      | 2024-01-14 10:00:00
```

---

## Workflow Summary

### What Happens During Registration:
1. ? User submits email, password, and name
2. ? System validates input format
3. ? System checks email is unique
4. ? BCrypt hashes password with automatic salt
5. ? User record created with hashed password
6. ? JWT token generated with user claims
7. ? Token and user data returned to client
8. ? Client stores token for subsequent requests

### What Happens During Login:
1. ? User submits email and password
2. ? System finds user by email
3. ? BCrypt verifies password against stored hash
4. ? System checks user is active and not deleted
5. ? Last login timestamp updated
6. ? JWT token generated
7. ? Token and user data returned to client

### What Happens on Protected Endpoint Access:
1. ? Client sends request with `Authorization: Bearer {token}` header
2. ? JWT middleware validates token signature
3. ? JWT middleware checks token expiration
4. ? JWT middleware extracts user claims
5. ? Request proceeds with authenticated user context
6. ? Controller accesses user ID via `User.Claims`

---

## Next Steps (After Basic Auth is Working)

This implementation provides basic authentication. Future enhancements could include:

### Phase 2 - Enhanced Security:
- Email verification (send confirmation email)
- Password reset functionality
- Account lockout after failed attempts
- Two-factor authentication (2FA)
- Refresh tokens for long-lived sessions

### Phase 3 - Role-Based Authorization:
- Integrate with existing `roles` and `permissions` tables
- Add role claims to JWT tokens
- Implement `[Authorize(Roles = "Admin")]` attributes
- Permission-based access control

### Phase 4 - Advanced Features:
- OAuth2 integration (Google, Microsoft login)
- Session management
- User profile management
- Audit logging for authentication events

---

## Conclusion

This simple authentication system provides a solid foundation for user registration and login using industry-standard BCrypt password hashing and JWT tokens. It integrates seamlessly with your existing CQRS architecture and can be extended with roles and permissions in the future.

**Key Benefits:**
? Secure password storage with BCrypt  
? Stateless authentication with JWT  
? Follows CQRS pattern  
? Easy to test and maintain  
? No complex role/permission logic initially  
? Ready for future enhancements  

---

**Document Version:** 1.0  
**Last Updated:** January 2024  
**Author:** Development Team  
**Status:** Implementation Guide - Ready for Development
