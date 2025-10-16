# 🚀 Quick Test - Auto Login After Register

## ✨ Tính năng mới: Đăng ký = Tự động đăng nhập!

Sau khi đăng ký thành công, API sẽ **tự động trả về JWT Token** - không cần login lại!

---

## 📝 Test trong Postman

### 1. Đăng ký User Mới

**URL:**
```
POST https://localhost:7105/api/accounts/register
```

**Headers:**
```
Content-Type: application/json
```

**Body:**
```json
{
  "userName": "alice123",
  "email": "alice@test.com",
  "password": "Alice123!",
  "confirmPassword": "Alice123!"
}
```

### 2. Response (Đã có Token luôn!) 🎉

```json
{
  "id": "9",
  "userName": "alice123",
  "email": "alice@test.com",
  "roles": [
    "Basic"
  ],
  "isVerified": true,
  "jwToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJhbGljZTEyMyIsImp0aSI6IjM4ZTc3YmM3LWM3NmMtNGVmOS1iZWY1LTJmOGQzMzZiZDNiYyIsImVtYWlsIjoiYWxpY2VAdGVzdC5jb20iLCJ1aWQiOiI5IiwiaXAiOiI6OjEiLCJyb2xlcyI6IkJhc2ljIiwibmJmIjoxNzM5NjY0MDAwLCJleHAiOjE3Mzk2Njc2MDAsImlhdCI6MTczOTY2NDAwMCwiaXNzIjoiQ29yZUlkZW50aXR5IiwiYXVkIjoiQ29yZUlkZW50aXR5VXNlciJ9.xxx"
}
```

### 3. Dùng Token ngay lập tức! ✅

Copy `jwToken` và dùng cho các API khác:

**Authorization:**
```
Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

---

## 🎯 Flow mới:

```
1. User điền form đăng ký
   ↓
2. POST /api/accounts/register
   ↓
3. Nhận ngay JWT Token
   ↓
4. Redirect to Dashboard (đã đăng nhập)
```

**KHÔNG CẦN login lại!** 🎉

---

## 📊 So sánh Flow cũ vs mới:

### ❌ Flow cũ (2 bước):
```
1. POST /register → "User created"
2. POST /login → Get token
```

### ✅ Flow mới (1 bước):
```
1. POST /register → Get token luôn!
```

---

## 🧪 Test Cases

### ✅ Case 1: Đăng ký thành công
```json
{
  "userName": "bob123",
  "email": "bob@test.com",
  "password": "Bob123!",
  "confirmPassword": "Bob123!"
}
```

**Response 200 OK:**
```json
{
  "id": "10",
  "userName": "bob123",
  "email": "bob@test.com",
  "roles": ["Basic"],
  "isVerified": true,
  "jwToken": "..."
}
```

### ❌ Case 2: Username đã tồn tại
```json
{
  "userName": "alice123",
  "email": "different@test.com",
  "password": "Pass123!",
  "confirmPassword": "Pass123!"
}
```

**Response 400 Bad Request:**
```json
{
  "message": "Username 'alice123' is already taken."
}
```

### ❌ Case 3: Email đã tồn tại
```json
{
  "userName": "newalice",
  "email": "alice@test.com",
  "password": "Pass123!",
  "confirmPassword": "Pass123!"
}
```

**Response 400 Bad Request:**
```json
{
  "message": "Email alice@test.com is already registered."
}
```

### ❌ Case 4: Password không đủ mạnh
```json
{
  "userName": "charlie",
  "email": "charlie@test.com",
  "password": "weak",
  "confirmPassword": "weak"
}
```

**Response 400 Bad Request:**
```json
{
  "message": "One or more validation errors occurred.",
  "errors": [
    "Password must contain at least one uppercase letter, one lowercase letter, one number and one special character"
  ]
}
```

---

## 🔐 Token Info

- **Token Type:** JWT (JSON Web Token)
- **Expiration:** 60 phút (1 giờ)
- **Refresh Token:** Có sẵn trong response (dùng để refresh khi token hết hạn)
- **Role mặc định:** Basic

---

## 💡 Lưu ý Frontend

### Sau khi nhận response:
```javascript
// Lưu token vào localStorage/sessionStorage
localStorage.setItem('token', response.jwToken);
localStorage.setItem('user', JSON.stringify({
  id: response.id,
  userName: response.userName,
  email: response.email,
  roles: response.roles
}));

// Redirect to Dashboard
window.location.href = '/dashboard';
```

### Không cần call API login nữa!

---

## 🚀 Quick Copy Data

```json
{"userName": "user1", "email": "user1@test.com", "password": "User123!", "confirmPassword": "User123!"}
```

```json
{"userName": "user2", "email": "user2@test.com", "password": "User123!", "confirmPassword": "User123!"}
```

```json
{"userName": "user3", "email": "user3@test.com", "password": "User123!", "confirmPassword": "User123!"}
```

---

## 🎨 Best Practices

✅ **DO:**
- Lưu token vào localStorage/sessionStorage
- Set Authorization header cho các request tiếp theo
- Hiển thị thông tin user từ response
- Redirect về dashboard sau register

❌ **DON'T:**
- Không lưu password
- Không call login API sau register
- Không để token expire mà không refresh

---

## 🔄 Refresh Token Flow

Khi token hết hạn (sau 60 phút):

```
POST /api/accounts/refresh-token
{
  "refreshToken": "..."
}

Response:
{
  "id": "9",
  "jwToken": "new_token...",
  ...
}
```

Lưu token mới và tiếp tục!

---

Happy Coding! 🎉

