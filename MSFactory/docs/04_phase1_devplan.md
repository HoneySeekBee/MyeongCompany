# Phase 1 개발 계획서

## 개요

| 항목 | 내용 |
|------|------|
| 목표 | 창고 환경에서 바퀴 로봇 10대 자율 운반 시뮬레이션 완성 |
| 환경 | ISAAC Sim + ROS2 Humble + OmniIsaac Gym |
| 예상 기간 | 4~7개월 (ISAAC 처음 배우면서 진행 기준) |
| 학습 인프라 | 로컬 RTX 4060 (개발/테스트) + RunPod (대규모 학습) |

---

## 마일스톤 전체 흐름

```
M1          M2          M3          M4          M5          M6
환경 세팅 → 씬 제작 → 로봇 세팅 → RL 학습 → 디스패처 → 통합 테스트
```

---

## M1. 개발 환경 세팅

### 1-1. ISAAC Sim 설치

- NVIDIA Omniverse Launcher 설치
- ISAAC Sim 최신 버전 설치 (Windows 네이티브)
- 설치 후 기본 씬 열어서 GPU 렌더링 확인

**완료 기준**: ISAAC Sim 실행 후 기본 씬에서 물리 시뮬레이션(공 굴리기 등) 작동

### 1-2. Python 환경 세팅

- ISAAC Sim 내장 Python 3.10 사용 (별도 설치 불필요)
- VSCode + ISAAC Sim Python 인터프리터 연결
- OmniIsaac Gym Envs 클론 및 설치

```
git clone https://github.com/NVIDIA-Omniverse/OmniIsaacGymEnvs
```

**완료 기준**: OmniIsaac Gym 예제(CartPole 등) 실행 성공

### 1-3. WSL2 + ROS2 설치

- WSL2 + Ubuntu 22.04 설치
- ROS2 Humble 설치
- ISAAC Sim ROS2 Bridge 활성화 (Omniverse Extension에서)
- WSL2 ↔ ISAAC Sim 토픽 통신 확인

```bash
# WSL2 내에서
ros2 topic list  # ISAAC Sim 실행 중에 토픽 보이면 성공
```

**완료 기준**: ISAAC Sim 실행 상태에서 WSL2 터미널에 `/clock` 등 토픽 확인

---

## M2. 창고 씬 제작

### 2-1. 기본 레이아웃

- 바닥 평면 (60cm × 80cm 기준, 시뮬에서는 6m × 8m 스케일로 작업 후 축소)
- 외벽 4면
- 구역(Zone) 구분: A, B, C, D 최소 4개
  - 각 구역에 색상 평면 or 마커로 구분

### 2-2. 스테이션 위치 설정

- 각 구역 모서리에 스테이션 포인트 지정
- 빈 Xform 노드로 픽업/드롭 위치 마킹 (나중에 로봇이 이 좌표로 이동)

### 2-3. 물건(화물) 에셋

- 단순 박스 Mesh (5cm × 5cm × 5cm 기준)
- 물리 설정: 질량, 마찰 설정
- 스테이션에서 생성/제거로 로딩/언로딩 시뮬레이션

**완료 기준**: 씬 저장 후 재로딩 시 레이아웃 유지, 구역 구분 명확

---

## M3. 로봇 세팅

### 3-1. 바퀴 로봇 에셋 선택

아래 중 하나 선택:
- **Nova Carter** (NVIDIA 공식, 완성도 높음) — 크기가 크므로 스케일 축소 필요
- **Jetbot** (소형, 가벼움) — 실물 크기와 더 유사
- **커스텀 URDF** — 실물 설계와 1:1 매칭 원하면 직접 제작

> Phase 1에서는 Jetbot으로 시작 추천. 나중에 커스텀으로 교체 가능.

### 3-2. 물리 설정

- 질량: 실물 기준 약 50~100g
- 바퀴 마찰: 미끄러짐 없도록 설정
- 최대 속도 제한 설정

### 3-3. 센서 추가

- **TOF 센서 시뮬레이션**: ISAAC Sim의 Lidar Ray Sensor로 대체
  - 전방 / 좌측 / 우측 3방향 Ray 설정
  - 감지 거리: 실물 VL53L0X 기준 최대 2m
- **ROS2 namespace**: 각 로봇에 `/robot_0` ~ `/robot_9` 고유 네임스페이스

### 3-4. 10대 배치

- 시작 위치: 각 구역에 2~3대씩 분산 배치
- 초기 상태: 모두 대기(idle)

**완료 기준**: 10대 로봇이 씬에 배치되고, 수동 조종(키보드)으로 개별 이동 확인

---

## M4. RL 학습

### 4-1. OmniIsaac Gym 환경 구성

`WarehouseTask` 클래스 작성:

```python
class WarehouseTask(RLTask):
    def set_up_scene(self, scene):
        # 창고 씬 + 로봇 10대 로드
        pass

    def get_observations(self):
        # [자기 위치, 목적지 위치, 주변 로봇 위치, TOF 센서값]
        pass

    def calculate_metrics(self):
        # 보상 계산
        pass

    def is_done(self):
        # 목적지 도달 or 충돌 or 타임아웃
        pass
```

**관측 공간** (로봇 1대 기준):
```
[자기 x, 자기 y, 자기 heading,
 목적지 x, 목적지 y,
 TOF 전방, TOF 좌, TOF 우,
 주변 로봇 상대 위치 × 9대]
```

**행동 공간**:
```
[선속도 (-1 ~ 1), 각속도 (-1 ~ 1)]  # Continuous
```

### 4-2. 1단계: 단일 로봇 목적지 이동

- 로봇 1대, 빈 환경
- 목표: 지정된 목적지까지 이동
- 보상: 목적지 거리 감소 +0.1 / 도달 +100 / 타임아웃 -10
- 학습: 로컬 RTX 4060에서 진행
- 확인: 안정적으로 목적지 도달하면 다음 단계

**완료 기준**: 에피소드 성공률 90% 이상

### 4-3. 2단계: 충돌 회피 추가

- 로봇 3~5대, 랜덤 위치에서 출발
- 보상에 충돌 패널티 추가: 충돌 시 -50, 에피소드 종료
- TOF 센서값 관측에 포함
- 확인: 충돌 없이 각자 목적지 도달

**완료 기준**: 충돌률 5% 이하

### 4-4. 3단계: 10대 멀티 에이전트 확장

- 10대 동시 학습
- 병렬 환경 수 늘려서 RunPod에서 학습
- RunPod 설정:
  - GPU: RTX 4090 24GB or A100 40GB
  - Docker: `nvcr.io/nvidia/isaac-sim` 공식 이미지 사용
  - 학습 완료 후 체크포인트 다운로드

**완료 기준**: 10대 동시 운행 시 충돌률 5% 이하, 평균 도달 시간 안정

---

## M5. 태스크 디스패처 구현

### 5-1. 디스패처 코어 로직

```python
class TaskDispatcher:
    def __init__(self, num_robots=10):
        self.task_queue = deque()       # FIFO 큐
        self.robot_status = {}          # {robot_id: 'idle' | 'busy'}

    def add_task(self, from_zone, to_zone):
        # 태스크 큐에 추가 (최대 10개)
        pass

    def assign_tasks(self):
        # 빈 로봇에 큐 앞에서부터 배정
        pass

    def on_robot_idle(self, robot_id):
        # 로봇 임무 완료 → 다음 태스크 배정
        pass
```

### 5-2. ROS2 인터페이스 연결

| 토픽 / 서비스 | 방향 | 내용 |
|--------------|------|------|
| `/task/add` | 외부 → 디스패처 | 새 태스크 추가 |
| `/robot_N/cmd_vel` | 디스패처 → 로봇 | 이동 명령 |
| `/robot_N/status` | 로봇 → 디스패처 | idle / busy / arrived |
| `/factory/status` | 디스패처 → 외부 | 전체 현황 |

### 5-3. 스테이션 로봇 연동

- 바퀴 로봇 도착 이벤트 → 스테이션 로봇 로딩/언로딩 트리거
- 완료 이벤트 → 바퀴 로봇 출발 허가

**완료 기준**: 태스크 10개 동시 투입 시 데드락 없이 순서대로 완료

---

## M6. 통합 테스트

### 6-1. 기본 시나리오

```
시나리오 1: A→B 물건 5개 이동
시나리오 2: A→B 3개 + C→D 3개 동시
시나리오 3: 10개 태스크 연속 투입
```

### 6-2. 성능 측정 항목

| 항목 | 목표 |
|------|------|
| 태스크 완료율 | 95% 이상 |
| 충돌 발생률 | 5% 이하 |
| 평균 운반 시간 | 최적 경로의 1.5배 이내 |
| 데드락 발생 | 0회 |

### 6-3. ROS2 통합 확인

- WSL2 터미널에서 `/factory/status` 토픽으로 실시간 현황 모니터링
- rqt_graph로 전체 노드/토픽 연결 구조 시각화

**완료 기준**: 3개 시나리오 모두 성능 목표 달성

---

## 참고 리소스

| 항목 | 내용 |
|------|------|
| ISAAC Sim 공식 문서 | docs.omniverse.nvidia.com/isaacsim |
| OmniIsaac Gym Envs | github.com/NVIDIA-Omniverse/OmniIsaacGymEnvs |
| ISAAC Sim ROS2 Bridge | NVIDIA 공식 튜토리얼 참고 |
| ROS2 Humble 설치 | docs.ros.org/en/humble |
| RunPod ISAAC Docker | nvcr.io/nvidia/isaac-sim 공식 이미지 |

---

## 체크리스트

- [ ] M1. 개발 환경 세팅 완료
- [ ] M2. 창고 씬 제작 완료
- [ ] M3. 로봇 10대 세팅 완료
- [ ] M4-1. 단일 로봇 이동 학습 완료
- [ ] M4-2. 충돌 회피 학습 완료
- [ ] M4-3. 10대 멀티 에이전트 학습 완료
- [ ] M5. 태스크 디스패처 구현 완료
- [ ] M6. 통합 테스트 완료
