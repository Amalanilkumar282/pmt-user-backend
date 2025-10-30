# Authentication System Documentation

## Overview
This is a complete JWT-based authentication system for the PMT User Backend API using .NET 8 and CQRS pattern with MediatR.

## Features
? **User Login** with email and password  
? **JWT Access Token** generation (60 minutes expiration)  
? **Refresh Token** generation (7 days expiration)  
? **Token Refresh** endpoint to get new access tokens  
? **Logout** to revoke all user tokens  
? **Password Hashing** using BCrypt (work factor 12)  
? **User Claims** in JWT (userId, email, name, role, etc.)  
? **Swagger Integration** with Bearer token authentication  

## Database Setup

### 1. Create refresh_tokens Table
Run the SQL script to create the refresh_tokens table:

```bash
psql -h your-host -U your-user -d your-database -f Database/create_refresh_tokens_table.sql
```

Or execute the SQL directly in your Supabase SQL Editor:
- Navigate to your Supabase project
- Go to SQL Editor
- Copy and paste the content from `Database/create_refresh_tokens_table.sql`
- Run the query

## API Endpoints

### 1. Login
**POST** `/api/auth/login`

**Request Body:**
```json
{
  "email": "user@example.com",
  "password": "your-password"
}
```

**Response (200 OK):**
```json
{
  "data": {
    "userId": 1,
    "email": "user@example.com",
    "name": "John Doe",
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "base64-encoded-refresh-token",
    "accessTokenExpires": "2025-01-31T15:30:00Z",
    "refreshTokenExpires": "2025-02-07T14:30:00Z",
    "isActive": true,
    "isSuperAdmin": false
  },
  "succeeded": true,
  "message": "Login successful",
  "errors": null
}
```

### 2. Refresh Token
**POST** `/api/auth/refresh`

**Request Body:**
```json
{
  "refreshToken": "your-refresh-token"
}
```

**Response (200 OK):**
```json
{
  "data": {
    "userId": 1,
    "email": "user@example.com",
    "name": "John Doe",
    "accessToken": "new-access-token",
    "refreshToken": "new-refresh-token",
    "accessTokenExpires": "2025-01-31T16:30:00Z",
    "refreshTokenExpires": "2025-02-07T15:30:00Z",
    "isActive": true,
    "isSuperAdmin": false
  },
  "succeeded": true,
  "message": "Token refreshed successfully",
  "errors": null
}
```

### 3. Logout
**POST** `/api/auth/logout`

**Headers:**
```
Authorization: Bearer your-access-token
```

**Response (200 OK):**
```json
{
  "data": true,
  "succeeded": true,
  "message": "Logout successful",
  "errors": null
}
```

### 4. Get Current User
**GET** `/api/auth/me`

**Headers:**
```
Authorization: Bearer your-access-token
```

**Response (200 OK):**
```json
{
  "data": {
    "userId": "1",
    "email": "user@example.com",
    "name": "John Doe",
    "isSuperAdmin": false,
    "isActive": true,
    "roles": ["User"]
  },
  "succeeded": true,
  "message": "User information retrieved successfully",
  "errors": null
}
```

## Configuration

### appsettings.json
```json
{
  "Jwt": {
    "SecretKey": "YourSuperSecretKeyForJWTTokenGenerationThatIsAtLeast32CharactersLong!",
    "Issuer": "PMT_User_Backend",
    "Audience": "PMT_User_Frontend",
    "AccessTokenExpirationMinutes": "60",
    "RefreshTokenExpirationDays": "7"
  }
}
```

**Important:** Change the `SecretKey` in production to a secure random string!

## Usage Examples

### Angular/TypeScript Example

```typescript
// Login
async login(email: string, password: string) {
  const response = await this.http.post<any>('/api/auth/login', {
    email,
    password
  }).toPromise();
  
  // Store tokens
  localStorage.setItem('accessToken', response.data.accessToken);
  localStorage.setItem('refreshToken', response.data.refreshToken);
  
  return response.data;
}

// Add token to requests
intercept(req: HttpRequest<any>, next: HttpHandler) {
  const token = localStorage.getItem('accessToken');
  
  if (token) {
    req = req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`
      }
    });
  }
  
  return next.handle(req).pipe(
    catchError(err => {
      if (err.status === 401) {
        // Token expired, try to refresh
        return this.refreshToken();
      }
      return throwError(err);
    })
  );
}

// Refresh token
async refreshToken() {
  const refreshToken = localStorage.getItem('refreshToken');
  
  const response = await this.http.post<any>('/api/auth/refresh', {
    refreshToken
  }).toPromise();
  
  localStorage.setItem('accessToken', response.data.accessToken);
  localStorage.setItem('refreshToken', response.data.refreshToken);
  
  return response.data;
}

// Logout
async logout() {
  await this.http.post('/api/auth/logout', {}).toPromise();
  localStorage.removeItem('accessToken');
  localStorage.removeItem('refreshToken');
}
```

## Security Features

1. **Password Hashing**: BCrypt with work factor 12 (highly secure, resistant to brute-force)
2. **Token Expiration**: Access tokens expire in 60 minutes
3. **Refresh Token Rotation**: Old refresh tokens are revoked when new ones are issued
4. **Token Revocation**: All user tokens are revoked on logout
5. **Account Status Check**: Inactive users cannot log in
6. **HTTPS**: Should be used in production
7. **CORS**: Configured for Angular frontend

## Testing with Swagger

1. Start the application
2. Navigate to `/swagger`
3. Click the **Authorize** button (top right)
4. Login first using `/api/auth/login` endpoint
5. Copy the `accessToken` from the response
6. Click **Authorize** and enter: `Bearer your-access-token`
7. Now you can test protected endpoints

## Working with Existing Users

**Good News:** Your authentication system will work with your existing users table!

The system uses BCrypt for password hashing, which is compatible with your existing passwords that were hashed using `BCrypt.Net.BCrypt.HashPassword(password)`.

### Testing with Existing Users
You can directly test login with any existing user in your database:

```json
POST /api/auth/login
{
  "email": "existing-user@example.com",
  "password": "their-actual-password"
}
```

### Creating New Users
If you need to create new test users, you can hash passwords using BCrypt:

**C# Example:**
```csharp
using BCrypt.Net;

// Hash a password
var hashedPassword = BCrypt.HashPassword("Test@123", workFactor: 12);
Console.WriteLine(hashedPassword);
// Output: $2a$12$... (60 character hash)
```

**SQL Example:**
```sql
-- Insert new user with BCrypt hashed password
INSERT INTO users (email, password_hash, name, is_active, is_super_admin, created_at)
VALUES (
  'newuser@example.com', 
  '$2a$12$YourBCryptHashedPasswordHere', 
  'New User', 
  true, 
  false, 
  NOW()
);
```

**Online BCrypt Generator:**
You can also use online tools to generate BCrypt hashes:
- https://bcrypt-generator.com/
- Use work factor/rounds: 12
- Copy the generated hash to your database

## Error Handling

The API returns consistent error responses:

```json
{
  "data": null,
  "succeeded": false,
  "message": "Invalid email or password",
  "errors": null
}
```

Common error messages:
- "Invalid email or password" - Wrong credentials or user doesn't exist
- "Account is inactive. Please contact administrator." - User account is disabled
- "Invalid refresh token" - Token not found or invalid
- "Refresh token has expired or been revoked" - Token is no longer valid
- "User not found" - User doesn't exist in refresh token context
- "Account is inactive" - User account is disabled (during token refresh)
- "Invalid user session" - Token is invalid or missing (during logout)

## Architecture

### CQRS Pattern
- **Commands**: `LoginCommand`, `RefreshTokenCommand`, `LogoutCommand`
- **Handlers**: Located in `Application/Handler/Auth/`
- **DTOs**: `LoginRequestDto`, `LoginResponseDto`, `RefreshTokenRequestDto`

### Services
- **IJwtTokenService**: Token generation and validation
- **IPasswordHashService**: Password hashing and verification using BCrypt

### Repositories
- **IRefreshTokenRepository**: Manage refresh tokens in database
- **IUserRepository**: User data access

## Next Steps

1. ? **BCrypt Package Installed** - Compatible with existing passwords
2. **Run the SQL script** to create the refresh_tokens table
3. **Test with existing users** - Your current users should work immediately
4. **Test the login endpoint** in Swagger with existing credentials
5. **Integrate with your frontend** application
6. **Change the JWT SecretKey** in production (currently using a placeholder)
7. **Enable HTTPS** in production
8. **Add rate limiting** for login attempts (optional)
9. **Add email verification** (optional)
10. **Add two-factor authentication** (optional)

## Troubleshooting

### Authentication Issues
- **"Invalid email or password"**: Check if user exists and is_active is true
- **Password verification fails**: Ensure password is correct and hash is valid BCrypt format
- **Token validation fails**: Check JWT SecretKey is consistent and token hasn't expired

### Build Errors
- Ensure all NuGet packages are restored (BCrypt.Net-Next should be installed)
- Check that .NET 8 SDK is installed
- Rebuild the solution

### Database Connection
- Verify connection string in appsettings.json
- Check Supabase credentials
- Ensure refresh_tokens table exists

### Token Issues
- Check JWT SecretKey is set and at least 32 characters
- Verify token expiration settings
- Check system clock is synchronized
- Ensure refresh_tokens table was created successfully

## Support

For issues or questions, please check:
1. Build errors in Visual Studio
2. Database logs in Supabase
3. Application logs in Console/Debug output
4. Swagger documentation at `/swagger`

## Password Hashing Details

### BCrypt Work Factor
The system uses BCrypt with work factor 12, which means:
- 2^12 (4,096) iterations
- Takes ~250ms to hash a password
- Highly resistant to brute-force attacks
- Industry standard for password security

### Why BCrypt?
- **Adaptive**: Work factor can be increased as computers get faster
- **Salted**: Each password has a unique salt
- **Slow**: Intentionally slow to prevent brute-force attacks
- **Battle-tested**: Used by many major applications

---

**Version**: 1.1  
**Last Updated**: 2025-01-31  
**Author**: GitHub Copilot  
**Changes**: Updated to use BCrypt password hashing for compatibility with existing user database
