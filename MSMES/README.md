# MSMES - 중소기업 제조 실행 시스템 (MES)

## 프로젝트 개요

MSMES는 중소 제조업체를 위한 ASP.NET Core 8 기반 MES(Manufacturing Execution System)입니다.
수주부터 출하까지의 핵심 생산 프로세스를 클린 아키텍처(Domain / Application / Infrastructure / Web) 구조로 구현합니다.

## 기술 스택

| 구분 | 기술 |
|------|------|
| 런타임 | .NET 8 / ASP.NET Core 8 |
| 데이터베이스 | SQL Server 2019+ |
| ORM | Dapper 2.1.66 |
| 인증 | JWT Bearer (Microsoft.AspNetCore.Authentication.JwtBearer 8.0.11) |
| 비밀번호 해싱 | BCrypt.Net-Next 4.0.3 |
| API 문서 | Swashbuckle (Swagger UI) 6.9.0 |
| UI | Razor Pages |

## 아키텍처

```
Domain          (엔터티, 도메인 규칙, 인터페이스)
  ^
Application     (유스케이스 핸들러, Application 레이어 인터페이스)
  ^
Infrastructure  (Dapper 리포지터리, JWT 서비스, BCrypt)
  ^
Web             (Controllers, Razor Pages, DI 조립)
```

의존성 방향: `Web → Infrastructure → Application → Domain`

## 모듈 설명

### 수주 (SalesOrder)
고객으로부터 수주를 접수하고 관리합니다.
- 상태 흐름: `Draft → Confirmed → InProduction → Shipped → Closed`
- 핵심 API: `POST /api/sales-orders`, `GET /api/sales-orders/{no}`

### 발주 (PurchaseOrder)
원자재/부품 구매 발주를 관리합니다.
- 상태 흐름: `Draft → Issued → PartiallyReceived → Received → Closed`

### 작업지시 (WorkOrder)
생산 작업 지시를 발행하고 진행 상황을 추적합니다.
- 상태 흐름: `Planned → Released → InProgress → Completed → Closed`
- 수주와 연결(선택)하여 생산 추적 가능

### LOT 관리 (Lot Management)
생산된 LOT(제조 단위)를 추적하고 이력을 관리합니다.
- 상태 흐름: `Created → InProcess → QualityHold → Released → Shipped → Scrapped`
- 모든 상태 변경은 `LotHistories` 테이블에 기록

### 출하 (Shipment)
완성품 출하를 처리합니다.
- LOT 단위로 출하 품목 구성
- 상태 흐름: `Draft → Picking → Shipped → Delivered`

## 프로젝트 구조

```
MSMES.sln
├── src/
│   ├── MSMES.Domain/           # 엔터티, 열거형, 리포지터리 인터페이스
│   │   ├── Shared/             # Entity 기반 클래스, Money 값 객체
│   │   ├── Common/             # User, CommonCode, IUserRepository
│   │   ├── SalesOrder/
│   │   ├── PurchaseOrder/
│   │   ├── WorkOrder/
│   │   ├── LotManagement/
│   │   └── Shipment/
│   │
│   ├── MSMES.Application/      # 유스케이스 핸들러
│   │   ├── Common/             # IJwtService, IPasswordHasher (인터페이스)
│   │   ├── Auth/               # LoginHandler
│   │   ├── SalesOrder/
│   │   ├── PurchaseOrder/
│   │   ├── WorkOrder/
│   │   ├── LotManagement/
│   │   └── Shipment/
│   │
│   ├── MSMES.Infrastructure/   # 구현체 (Dapper, BCrypt, JWT)
│   │   ├── Auth/               # JwtService, BCryptPasswordHasher, JwtOptions
│   │   ├── Persistence/        # SqlConnectionFactory, NumberSequence
│   │   ├── Repositories/       # SQL* 리포지터리 구현
│   │   └── DependencyInjection.cs
│   │
│   └── MSMES.Web/              # ASP.NET Core 진입점
│       ├── Controllers/        # AuthController, SalesOrdersController
│       ├── Pages/              # Razor Pages (수주/발주/작업지시/LOT/출하)
│       ├── Program.cs
│       └── appsettings.json
│
├── tests/
│   └── MSMES.Tests/
│
└── docs/
    └── schema.sql              # DB 스키마 + 초기 데이터
```

## 실행 방법

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server 2019 이상 (또는 SQL Server Express / LocalDB)

### 1. 데이터베이스 설정

SQL Server Management Studio 또는 `sqlcmd`로 스키마를 실행합니다:

```bash
sqlcmd -S localhost -E -i docs/schema.sql
```

초기 데이터가 함께 실행됩니다:
- 관리자 계정: `admin` / `Admin1234!` (운영 환경에서 반드시 변경)
- 기본 공통코드 (역할, 각 모듈 상태값)

### 2. appsettings.json 설정

`src/MSMES.Web/appsettings.json`에서 연결 문자열과 JWT 시크릿을 설정합니다:

```json
{
  "ConnectionStrings": {
    "MSMES": "Server=localhost;Database=MSMES;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "Jwt": {
    "Issuer": "MSMES",
    "Audience": "MSMES.Client",
    "SigningKey": "YOUR_LONG_RANDOM_SECRET_AT_LEAST_32_CHARACTERS",
    "ExpiresMinutes": 480
  }
}
```

> JWT SigningKey는 최소 32자 이상의 랜덤 문자열을 사용하세요.

### 3. 실행

```bash
cd src/MSMES.Web
dotnet run
```

또는 솔루션 루트에서:

```bash
dotnet run --project src/MSMES.Web/MSMES.Web.csproj
```

### 4. API 확인

- Swagger UI: `https://localhost:{port}/swagger`
- 로그인: `POST /api/auth/login` → `{ "userId": "admin", "password": "Admin1234!" }`
- 반환된 `token`을 Swagger의 "Authorize" 버튼에 입력하여 인증 요청 가능

## 빌드

```bash
dotnet build MSMES.sln
```

## 테스트

```bash
dotnet test
```
