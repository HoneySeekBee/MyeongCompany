# Program.cs 추가 사항 (Role 기반 인증 Agent 병합용)

아래 내용은 다른 에이전트가 쿠키 인증을 설정한 후 Program.cs에 병합해야 합니다.

## 1. 쿠키 인증 설정

현재 Program.cs는 JwtBearer 단일 인증만 설정되어 있습니다.  
Razor Pages의 `[Authorize(Roles = "Admin")]`이 동작하려면 쿠키 인증(Cookie Authentication)이 추가되어야 합니다.

```csharp
// 기존 AddAuthentication 교체 또는 다중 스킴 추가
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme          = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.LoginPath    = "/Account/Login";
    options.LogoutPath   = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan   = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
})
.AddJwtBearer(options =>
{
    // ... 기존 JWT 설정 유지 ...
});
```

## 2. 필요한 using

```csharp
using Microsoft.AspNetCore.Authentication.Cookies;
```

## 3. Razor Pages 인가 정책 (선택사항)

```csharp
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
});
```

## 4. AdminController는 이미 Bearer 스킴 명시

`AdminController.cs`는 `[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]`으로
API 엔드포인트는 Bearer 토큰만 사용하도록 분리되어 있습니다.

## 5. 추가 서비스 등록 불필요

`IUserRepository`, `IPasswordHasher`는 `AddMsmesInfrastructure()`에 이미 등록되어 있으므로  
Program.cs에 별도 `AddScoped` 추가가 필요하지 않습니다.
