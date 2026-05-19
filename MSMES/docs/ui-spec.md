# MSMES UI/UX 스펙

본 문서는 MSMES(ASP.NET Core 8) 웹 UI의 디자인 시스템, 레이아웃, 페이지별 구성, 인터랙션 규칙을 정의한다. 모든 신규 모듈(재고/품질/설비/공정/대시보드) 및 기존 모듈(수주/발주/작업지시/LOT/출하)에 공통 적용한다.

---

## 1. 디자인 시스템

### 1.1 프레임워크 / 라이브러리
- **CSS 프레임워크**: Bootstrap 5.3 (CDN 또는 wwwroot/lib)
- **아이콘**: Bootstrap Icons 1.11+
- **차트**: Chart.js 4.x
- **테이블**: DataTables 1.13 (Bootstrap5 테마)
- **알림**: Bootstrap Toast
- **날짜선택**: flatpickr (한국어 로케일)

### 1.2 색상 팔레트

| 토큰 | HEX | 용도 |
|---|---|---|
| `--ms-primary` | `#1a73e8` | 주요 액션, 링크, KPI 강조 (구글블루) |
| `--ms-primary-hover` | `#1557b0` | hover 상태 |
| `--ms-success` | `#28a745` | 정상/완료/가동 상태 뱃지 |
| `--ms-warning` | `#ffc107` | 부족/대기/점검 상태 뱃지 |
| `--ms-danger` | `#dc3545` | 소진/실패/고장 상태 뱃지 |
| `--ms-info` | `#17a2b8` | 진행중/조정 상태 뱃지 |
| `--ms-dark` | `#1e2a3a` | 사이드바 배경 |
| `--ms-dark-2` | `#27374d` | 사이드바 hover / active |
| `--ms-bg` | `#f5f7fa` | 메인 컨텐츠 배경 |
| `--ms-card` | `#ffffff` | 카드 배경 |
| `--ms-text` | `#212529` | 본문 텍스트 |
| `--ms-muted` | `#6c757d` | 보조 텍스트 |
| `--ms-border` | `#e3e6ea` | 경계선 |

### 1.3 타이포그래피
- **폰트**: `'Noto Sans KR', system-ui, -apple-system, 'Segoe UI', sans-serif` (Google Fonts CDN)
- **본문**: 14px / line-height 1.5
- **제목 H1**: 22px / 600
- **제목 H2**: 18px / 600
- **제목 H3**: 16px / 600
- **숫자(KPI)**: 28px / 700, 색상 `--ms-primary`
- **레이블/캡션**: 12px / 400, 색상 `--ms-muted`

### 1.4 간격 / 모서리
- 기본 패딩: 16px (카드 내부 20px)
- 카드 border-radius: 8px
- 그림자: `0 1px 3px rgba(0,0,0,0.06), 0 1px 2px rgba(0,0,0,0.04)`

---

## 2. 전체 레이아웃

```
+---------------------------------------------------------------+
| 사이드바 260px (dark)  |  헤더 56px (white, sticky)            |
|                         +-------------------------------------+
|  로고 (MSMES)           |  ☰  페이지 타이틀     [user] 로그아웃 |
|  -------------          +-------------------------------------+
|  [대시보드]              |                                     |
|  [수주관리]              |   메인 컨텐츠 (padding 24px)         |
|  [발주관리]              |   배경: --ms-bg                     |
|  [작업지시]              |                                     |
|  [공정관리] *NEW          |   <카드들...>                       |
|  [LOT 관리]              |                                     |
|  [재고관리] *NEW          |                                     |
|  [품질관리] *NEW          |                                     |
|  [설비관리] *NEW          |                                     |
|  [출하관리]              |                                     |
|  [시스템관리]             |                                     |
+---------------------------------------------------------------+
```

### 2.1 사이드바
- 너비: 260px 고정 (모바일 < 768px에서는 오프캔버스로 전환)
- 배경: `--ms-dark`, 텍스트 `#cfd8e3`
- 활성 메뉴: 좌측 4px 강조 바 `--ms-primary` + 배경 `--ms-dark-2`
- 메뉴 그룹 구분선: `rgba(255,255,255,0.06)`
- 각 메뉴 항목: `<i class="bi bi-...">` 아이콘 + 라벨, 높이 44px

### 2.2 헤더
- 높이 56px, 흰색 배경, 하단 1px 경계선
- 좌측: 토글 버튼(모바일에서만 표시) + 페이지 타이틀
- 우측: 알림 벨(unread 카운트 뱃지), 사용자 아바타 + 이름, 로그아웃 링크

### 2.3 반응형
- ≥ 1200px: 사이드바 + 메인
- 768~1199: 사이드바 축소(아이콘만, 64px)
- < 768: 사이드바 숨김, 토글 시 오프캔버스

---

## 3. 대시보드 페이지 (`/Dashboard`)

### 3.1 KPI 카드 (상단 6개, 2행 × 3열 또는 1행 × 6열)

각 카드 구성: 상단 레이블(12px muted) → 큰 숫자(28px primary) → 부가지표(녹/적 화살표)

| # | 카드 | 표시값 | 비고 |
|---|---|---|---|
| 1 | 오늘의 생산실적 | `TodayProduced / TodayTarget` (예: 1,048/1,000) | 달성률 % 진행바 |
| 2 | 수주 잔고 | 건수 + 합계 금액 | OpenSalesOrder |
| 3 | 재고 현황 | 정상/부족/소진 (3개 컬러 점) | InventoryStatus 합산 |
| 4 | 설비 가동률 | `Running / Total` % | 가동/정지/점검/고장 미니 막대 |
| 5 | 최근 7일 불량률 | `Recent7DaysDefectRate %` | 전주 대비 화살표 |
| 6 | 작업지시 현황 | 계획/진행/완료 | 3개 컬러 뱃지 |

### 3.2 중단 차트 영역 (2열)

- **좌(8/12)**: 일자별 생산실적 라인차트 (Chart.js `line`)
  - X축: 최근 7일
  - 데이터셋: 생산수량(primary), 불량수량(danger)
  - 호버 툴팁: 한국어 날짜 + 수량
- **우(4/12)**: 설비 현황 도넛차트 (Chart.js `doughnut`)
  - 4 슬라이스: Running(success) / Stopped(secondary) / Maintenance(warning) / Breakdown(danger)
  - 가운데 라벨: 가동률 %

### 3.3 하단 영역 (2열)

- **좌(7/12)**: 최근 작업지시 테이블 — 작업지시번호 / 품목 / 수량 / 상태 / 등록일 (10건)
- **우(5/12)**: 재고부족 알림 카드 — 적색 헤더 "재고부족 N건", LowStock + OutOfStock 리스트 (품목명, 현재고 / 안전재고)

---

## 4. 모듈 공통 - 목록 페이지

### 4.1 레이아웃
```
[페이지 타이틀]                                  [+ 등록]
+--------------------------------------------------+
| 검색필터 카드                                     |
|  [기간 from~to] [상태 ▼] [키워드] [검색] [초기화]  |
+--------------------------------------------------+
| DataTable                                        |
|  ┌────┬──────┬──────┬───────┬──────┬─────────┐  |
|  │ No │ 코드 │ 명칭 │ 상태  │ 일자 │ 액션    │  |
|  ├────┼──────┼──────┼───────┼──────┼─────────┤  |
|  │ 1  │ ...  │ ...  │ 뱃지  │ ...  │ ✎ 🗑    │  |
|  └────┴──────┴──────┴───────┴──────┴─────────┘  |
|                              [< 1 2 3 ... >]    |
+--------------------------------------------------+
```

### 4.2 상태 뱃지 규칙

| 상태 | 클래스 | 색상 |
|---|---|---|
| 정상 / 가동 / 완료 / Passed | `badge bg-success` | 녹색 |
| 부족 / 점검 / 대기 / Pending | `badge bg-warning text-dark` | 황색 |
| 소진 / 고장 / 실패 / Failed | `badge bg-danger` | 적색 |
| 정지 / 조정 / ConditionalPass | `badge bg-secondary` | 회색 |
| 진행중 / 입고 | `badge bg-info` | 청록 |

### 4.3 액션 버튼
- 등록: `btn btn-primary` (상단 우측), `<i class="bi bi-plus-lg"></i> 등록`
- 행 액션: 아이콘 버튼 `<i class="bi bi-pencil"></i>` / `<i class="bi bi-trash"></i>`
- 삭제 전 confirm 모달

### 4.4 DataTable 설정
- 페이지 사이즈: 10 / 25 / 50
- 정렬 가능 컬럼: 코드, 일자, 상태
- 한국어 언어팩 적용 (`/lib/datatables/i18n/ko.json`)
- 서버사이드 페이징: 100건 이상 데이터 시 활성

---

## 5. 등록 / 수정 모달

### 5.1 구조 (Bootstrap Modal `modal-lg`)
```
+------------------------------------------+
| 제목 (등록 / 수정 / 상세보기)         [×] |
+------------------------------------------+
| <form id="form-...">                      |
|   <필드 그리드 row g-3>                   |
|     col-md-6: 필수항목 (* 표시)           |
|     col-md-6: 선택항목                    |
|   </필드 그리드>                          |
|   <invalid-feedback>                      |
| </form>                                  |
+------------------------------------------+
| [취소]                          [저장]    |
+------------------------------------------+
```

### 5.2 폼 유효성 검사 (client-side)
- `needs-validation` Bootstrap 패턴 사용
- 필수 필드: `required` 속성 + `<div class="invalid-feedback">필수 입력입니다</div>`
- 숫자/날짜 타입 enforce
- 저장 버튼 클릭 → `form.checkValidity()` 통과 후 fetch POST
- 실패 시 첫 invalid 필드로 스크롤

### 5.3 모듈별 모달 필드

**재고 트랜잭션 등록**
- 품목코드 (select, 필수), 창고코드 (select, 필수), 거래유형 (Receipt/Issue/Adjustment, radio, 필수), 수량 (number, 필수, > 0), 참조번호 (text), 비고 (textarea)

**품질 검사 등록**
- LOT번호 (select 검색, 필수), 검사항목 (text, 필수), 검사수량 (number, 필수), 불량수량 (number, ≤ 검사수량), 불량유형 (select), 결과 (Pending/Passed/Failed/ConditionalPass, 필수), 검사자 (text, 필수), 비고

**설비 점검 등록**
- 설비코드 (select, 필수), 점검유형 (정기/긴급/예방, 필수), 점검내용 (textarea, 필수), 점검자 (text, 필수), 다음점검예정일 (date)

**공정 실적 등록**
- 작업지시번호 (select, 필수), 공정코드 (select, 필수), 작업자 (text, 필수), 생산수량 (number, 필수), 불량수량 (number), 시작시간 (datetime-local, 필수), 종료시간 (datetime-local)

### 5.4 Toast 알림
- 위치: 우측 상단 fixed
- 성공: `bg-success` "저장되었습니다"
- 실패: `bg-danger` "저장 실패: {message}"
- 자동 사라짐: 4초

---

## 6. 모듈별 페이지 URL 매핑

| 모듈 | 목록 | 등록/수정 |
|---|---|---|
| 대시보드 | `/Dashboard` | - |
| 수주관리 | `/SalesOrder` | Modal |
| 발주관리 | `/PurchaseOrder` | Modal |
| 작업지시 | `/WorkOrder` | Modal |
| 공정관리 | `/Process` | Modal (공정정의 / 실적) |
| LOT 관리 | `/Lot` | Modal |
| 재고관리 | `/Inventory` | Modal (트랜잭션) |
| 품질관리 | `/Quality` (탭: 검사 / 불량유형 / 통계) | Modal |
| 설비관리 | `/Equipment` (탭: 설비 / 점검이력) | Modal |
| 출하관리 | `/Shipment` | Modal |

---

## 7. 차트 / 통계 페이지

### 7.1 품질 통계 (`/Quality?tab=stats`)
- 기간 필터 (기본: 최근 30일)
- KPI: 검사건수, 불량건수, 불량률
- 일자별 불량률 라인차트
- 불량유형별 막대차트 (상위 5)

### 7.2 생산 실적 집계 (`/Process?tab=results`)
- 기간 필터
- 공정별 생산량 막대차트
- 작업자별 생산성 표

---

## 8. 접근성 / 국제화
- 모든 버튼 `aria-label` 명시
- 색상만으로 상태 구분 금지 → 뱃지 텍스트 함께 표기
- 한국어 우선, 라벨/메시지 리소스 파일 분리 (`Resources/SharedResource.ko.resx`)
- 키보드 네비게이션: 모달 Esc 닫기, Enter 저장

---

## 9. 코딩 컨벤션
- Razor Pages 또는 MVC + 부분뷰(`_Modal.cshtml`)
- 공통 레이아웃: `Views/Shared/_Layout.cshtml`
- 사이드바 부분뷰: `_Sidebar.cshtml`
- 공통 스크립트: `wwwroot/js/site.js` (Toast, confirm dialog, fetch wrapper)
- API 호출: `/api/{module}/...` JSON, CSRF 토큰 헤더 포함

---

## 10. 향후 확장 고려
- 다크모드 토글 (`data-bs-theme="dark"`)
- 권한별 메뉴 가시성 제어
- SignalR 기반 실시간 KPI 푸시 (대시보드 30초 폴링 → 실시간 전환)
