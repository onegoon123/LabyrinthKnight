##  프로젝트 개요

**미궁기사 키우기**는 Unity로 개발된 방치형 RPG 게임입니다.

- **장르**: 방치형 RPG
- **개발 엔진**: Unity
- **주요 기능**: 캐릭터 육성, 던전 탐험, 장비 시스템, 동료 시스템

---

##  프로젝트 구조

```
Scripts/
├── Core/                 # 코어 시스템 (GameManager, IdleRPGController)
├── Player/               # 플레이어 컨트롤러, 스탯, 공격 파티클
├── Enemies/              # 적 AI, 스포너
├── Companions/           # 동반자 시스템, 컨트롤러
├── Inventory/            # 인벤토리, 장비, 아이템 드롭
├── UI/                   # UI 패널 (던전, 업그레이드, 업적, 설정 등)
├── Systems/              # 상점, 세이브, 풀 매니저, 씬 페이드
├── Dialogue/             # 대화 시스템, 스크립트 처리
├── Environment/          # 던전 맵 관리자
├── Managers/             # 던전/타이틀 매니저
├── Interfaces/           # 전투 타겟 인터페이스
└── Data/                 # 게임 데이터, CSV 리더, 스탯 데이터
```

---

##  주요 기능

### 캐릭터 시스템
- 플레이어 스탯 시스템 (`PlayerStats`)
- 캐릭터 컨트롤러 (`PlayerController`)
- 공격 파티클 풀링 (`AttackParticlePool`)

### 동반자 시스템
- 동반자 컨트롤러 및 시스템 (`CompanionController`, `CompanionSystem`)
- 동반자 UI 및 해금 팝업

### 장비 & 인벤토리
- 장비 시스템 (`EquipmentSystem`)
- 아이템 드롭 및 획득 팝업
- 장착된 아이템 UI

### 던전 시스템
- 던전 데이터 및 리스트 UI
- 적 스포너 (`EnemySpawner`)
- 스테이지 테마 데이터

### UI 시스템
- 네비게이션 컨트롤러 (`NavigationController`)
- 업그레이드 패널 (`UpgradePanel`)
- 업적 시스템 (`AchievementsPanel`)
- 세팅 패널 (`SettingsPanel`)

### 시스템
- 게임 세이브 시스템 (`GameSaveSystem`)
- 상점 시스템 (`ShopSystem`)
- 장비 제작 시스템 (`EquipmentCraftingSystem`)
- 오브젝트 풀링 (`PoolManager`)

---

##  기술 스택

- **Unity Engine**
- **C#**

---

##  비고

이 리포지토리는 포트폴리오 목적으로 제작되었으며, 게임의 핵심 코드만 포함하고 있습니다. 유료 에셋 및 리소스는 별도로 관리되고 있습니다.
