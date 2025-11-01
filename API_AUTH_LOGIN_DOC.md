# API Documentation: POST /api/auth/login

This document describes the authentication API endpoint for logging in users. Use this for frontend integration to authenticate users and obtain JWT tokens for subsequent requests.

---

## POST /api/auth/login

### Purpose

Authenticates a user with email and password. Returns a JWT token and user details on success.

### Request

- **Method:** POST
- **URL:** `/api/auth/login`
- **Headers:**
  - `Content-Type: application/json`
- **Body:**

```json
{
  "email": "user@example.com",
  "password": "yourPassword123"
}
```

### Response

- **Status 200 OK**

```json
{
  "succeeded": true,
  "statusCode": 200,
  "data": {
    "userId": "user-guid",
    "email": "user@example.com",
    "name": "Jane Doe",
    "accessToken": "<jwt-token>",
    "refreshToken": "<refresh-token>",
    "avatarUrl": "https://.../avatar.png"
  }
}
```

### Error Responses

- **400 Bad Request**: Missing or invalid email/password
- **401 Unauthorized**: Invalid credentials
- **429 Too Many Requests**: Rate limit exceeded
- **500 Internal Server Error**: Unexpected server error

**Error Format:**

```json
{
  "succeeded": false,
  "statusCode": <error_code>,
  "message": "<error_message>"
}
```

---

## Usage Notes

- On success, store the `accessToken` and `refreshToken` for authenticated requests.
- All subsequent API calls require the `Authorization: Bearer <accessToken>` header.
- Error messages are user-friendly and suitable for frontend display.
- Passwords must be sent securely over HTTPS.

---

**Last Updated:** November 1, 2025
