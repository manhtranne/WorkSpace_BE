# WorkSpace Authentication API Documentation

## Tổng quan
Hệ thống authentication của WorkSpace sử dụng JWT (JSON Web Token) và Refresh Token để xác thực người dùng. Hệ thống hỗ trợ đăng nhập bằng email và mật khẩu.

## Các Endpoint

### 1. Đăng nhập (Login)
**POST** `/api/accounts/login`

**Request Body:**
```json
{
  "email": "user@example.com",
  "password": "password123"
}
```

**Response:**
```json
{
  "data": {
    "id": "1",
    "userName": "username",
    "email": "user@example.com",
    "roles": ["Basic"],
    "isVerified": true,
    "jWToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "refresh_token_string"
  },
  "message": "Authenticated username",
  "succeeded": true
}
```

### 2. Đăng ký (Register)
**POST** `/api/accounts/register`

**Request Body:**
```json
{
  "firstName": "John",
  "lastName": "Doe",
  "email": "john.doe@example.com",
  "userName": "johndoe",
  "password": "password123",
  "confirmPassword": "password123"
}
```

### 3. Xác thực Email
**GET** `/api/accounts/confirm-email?userId={userId}&code={code}`

### 4. Quên mật khẩu
**POST** `/api/accounts/forgot-password`

**Request Body:**
```json
{
  "email": "user@example.com"
}
```

### 5. Đặt lại mật khẩu
**POST** `/api/accounts/reset-password`

**Request Body:**
```json
{
  "email": "user@example.com",
  "token": "reset_token",
  "password": "newpassword123",
  "confirmPassword": "newpassword123"
}
```

### 6. Làm mới Token
**POST** `/api/accounts/refresh-token`

**Request Body:**
```json
{
  "refreshToken": "refresh_token_string"
}
```

**Response:**
```json
{
  "data": {
    "id": "1",
    "userName": "username",
    "email": "user@example.com",
    "roles": ["Basic"],
    "isVerified": true,
    "jWToken": "new_jwt_token",
    "refreshToken": "new_refresh_token"
  },
  "message": "Token refreshed successfully",
  "succeeded": true
}
```

### 7. Đăng xuất (Logout)
**POST** `/api/accounts/logout`

**Request Body:**
```json
"refresh_token_string"
```

**Response:**
```json
{
  "data": "Token revoked successfully",
  "message": null,
  "succeeded": true
}
```

### 8. Thu hồi Token
**POST** `/api/accounts/revoke-token`

**Request Body:**
```json
"refresh_token_string"
```

## Cách sử dụng JWT Token

1. **Lưu trữ token**: Lưu JWT token và refresh token từ response đăng nhập
2. **Sử dụng JWT**: Gửi JWT token trong header `Authorization: Bearer {jwt_token}` cho các API yêu cầu xác thực
3. **Làm mới token**: Khi JWT hết hạn, sử dụng refresh token để lấy JWT mới
4. **Đăng xuất**: Gửi refresh token đến endpoint logout để thu hồi token

## Bảo mật

- JWT token có thời hạn 60 phút (có thể cấu hình trong appsettings.json)
- Refresh token có thời hạn 7 ngày
- Tất cả các request đều được log với IP address
- Hệ thống có validation cho tất cả input
- Sử dụng HTTPS trong production

## Cấu hình

Các cấu hình JWT trong `appsettings.json`:
```json
{
  "JWTSettings": {
    "Key": "your-secret-key",
    "Issuer": "CoreIdentity",
    "Audience": "CoreIdentityUser",
    "DurationInMinutes": 60
  }
}
```

## Lỗi thường gặp

- **401 Unauthorized**: Token không hợp lệ hoặc đã hết hạn
- **400 Bad Request**: Dữ liệu đầu vào không hợp lệ
- **404 Not Found**: Email không tồn tại
- **403 Forbidden**: Tài khoản chưa được xác thực email

## Ví dụ sử dụng với JavaScript

```javascript
// Đăng nhập
const loginResponse = await fetch('/api/accounts/login', {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({
    email: 'user@example.com',
    password: 'password123'
  })
});

const loginData = await loginResponse.json();
localStorage.setItem('jwtToken', loginData.data.jWToken);
localStorage.setItem('refreshToken', loginData.data.refreshToken);

// Sử dụng JWT cho các API khác
const apiResponse = await fetch('/api/some-protected-endpoint', {
  headers: {
    'Authorization': `Bearer ${localStorage.getItem('jwtToken')}`
  }
});

// Làm mới token khi cần
const refreshResponse = await fetch('/api/accounts/refresh-token', {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({
    refreshToken: localStorage.getItem('refreshToken')
  })
});

// Đăng xuất
await fetch('/api/accounts/logout', {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json'
  },
  body: JSON.stringify(localStorage.getItem('refreshToken'))
});
```
