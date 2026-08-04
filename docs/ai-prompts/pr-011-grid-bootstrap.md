# PR #11: Grid 부트스트랩 매니저 분리 및 Player 배선

## 개요

- 브랜치: `feature/grid-bootstrap`
- PR: #11
- 작성일: 2026-08-04
- 작업 목적: PR #7에서 GridOccupancyTester(테스트용 스크립트)에 임시로 배선했던 Grid/Player 연결 로직을 정식 게임플레이용 부트스트랩(GridManager)으로 분리한다.

## 주요 요청

1. PR #7의 후속 작업으로 남아있던 "GridOccupancyTester에 임시로 배선한 PlayerMovement/PlayerClickToMove 연결을 정식 게임 플레이용 부트스트랩 스크립트로 분리" 작업을 진행한다.
2. 큰 방향(GridManager 신규 도입, GridOccupancyTester의 역할 축소)에 대한 계획을 먼저 제시해 리뷰를 받은 뒤 구현한다.
3. 구현된 Player Movement 로직을 리뷰하고, 목표/요구사항/제약사항/완료조건 형식의 스펙과 대조해 부족한 부분을 파악한다.

## AI와 논의한 내용

### GridManager와 GridOccupancyTester의 Grid 인스턴스 공유 여부

GridOccupancyTester가 자체적으로 새 GridSystem/GridOccupancy를 생성하면, 더미 오브젝트(Object A)가 실제 플레이어 이동에 장애물로 반영되지 않는 문제가 있어, GridOccupancyTester가 GridManager를 인스펙터로 참조해 동일한 Grid/Occupancy 인스턴스를 공유하도록 결정했다.

### Awake 실행 순서 문제

GridManager와 GridOccupancyTester가 서로 다른 GameObject의 컴포넌트이므로 Awake 호출 순서가 보장되지 않는다. Grid/Occupancy를 Awake에서 즉시 생성하는 대신 지연 생성(lazy) 프로퍼티로 변경해, 어느 쪽이 먼저 Awake되어도 항상 동일한 인스턴스를 안전하게 참조하도록 했다.

### 테스트용 Object B/DestroyObjectB 메뉴 처리

기존 GridOccupancyTester는 Object B(플레이어 스탠드인)를 코드로 즉석 생성해 PlayerMovement/PlayerClickToMove를 붙였다. 이 책임을 GridManager로 옮기면서 Object B 생성 코드와 `DestroyObjectB` 테스트 메뉴는 제거했고, `SelectTargetCell` 테스트는 `GridManager.Player.CurrentIndex`를 로봇 위치로 참조하도록 변경했다.

### GridOccupancyTester.cs 삭제 여부

사용자가 삭제 시 영향 범위를 질문해, PlayerMovement/PlayerClickToMove/GridManager 등 실제 이동 로직은 GridOccupancyTester를 참조하지 않아 삭제해도 게임플레이에 영향이 없음을 확인했다. 다만 이번 PR에서는 스크립트 파일 자체는 유지하고, GameScene의 관련 GameObject만 정리하기로 결정했다.

### 코드 리뷰에서 발견된 항목

전체 이동 흐름(부트스트랩 → 클릭 입력 → 목적지 결정 → 경로 탐색 → Cell 단위 이동)을 리뷰한 결과 세 가지를 확인했다.

1. `PlayerMovement.Register()`가 시작 셀 점유에 실패해도 로그 없이 조용히 `_registered = false`로 남아, 이후 이동 실패 원인을 추적하기 어려웠다.
2. `PlayerClickToMove.Movement`가 인스펙터에 노출돼 있었지만 `GridManager.Awake()`가 항상 코드로 덮어써 실제로는 인스펙터 배선이 무의미했다.
3. 사용자가 제시한 목표/요구사항/제약사항/완료조건 스펙과 대조했을 때, 이동 완료를 알리는 이벤트 기반 구조(`OnRobotArrived`)가 전혀 없어 스펙의 완료조건과 제약사항(도착 후 로직을 직접 호출하지 않고 이벤트만 발행)을 충족하지 못했다.

### OnRobotArrived 이벤트 추가

`PlayerMovement`에 `event Action<Vector2Int> OnRobotArrived`를 추가했다. `MoveAlongPath` 코루틴이 목표 셀까지 정상적으로 도달했을 때만 발행하고, 선점으로 인한 재탐색 실패 등 이동이 중단되는 경로에서는 발행하지 않기로 결정했다 — "도착"과 "중단"을 구분해 구독자가 실제 도착 시점만 신뢰할 수 있도록 하기 위함이다.

## 주요 결정

- Grid 설정과 Grid/Occupancy 인스턴스 소유권은 GridManager로 이전한다.
- GridManager는 Player 참조(PlayerMovement/PlayerClickToMove)를 인스펙터 직렬화 필드로 받아 배선한다.
- GridOccupancyTester는 GridManager를 참조해 동일 Grid/Occupancy를 공유하고, 더미 스폰과 셀 조회 등 순수 디버그 기능만 유지한다.
- GridOccupancyTester.cs 스크립트 파일은 이번 PR에서 삭제하지 않고 유지한다.
- `PlayerMovement.Register()`가 시작 셀 점유에 실패하면 경고 로그를 남긴다.
- `PlayerClickToMove.Movement`는 `[field: SerializeField]`를 제거해 코드 전용(GridManager가 배선) 필드로 전환한다.
- `PlayerMovement.OnRobotArrived` 이벤트를 추가해, 목표 셀 도착을 직접 호출이 아닌 이벤트 발행 방식으로 통지한다.

## 변경된 주요 파일

- `Assets/Scripts/Map/GridManager.cs` (신규)
- `Assets/Scripts/Map/Grid/GridOccupancyTester.cs` (Object B 배선 제거, GridManager 참조로 변경)
- `Assets/Scenes/GameScene.unity` (GridManager GameObject 추가, 기존 GridOccupancyTester 테스트 GameObject 제거, RobotTest 프리팹 인스턴스에 컴포넌트 배선)
- `Assets/Prefabs/Player/RobotTest.prefab` (PlayerMovement/PlayerClickToMove 컴포넌트 추가)
- `Assets/Scripts/Player/PlayerMovement.cs` (등록 실패 로그, `OnRobotArrived` 이벤트 추가)
- `Assets/Scripts/Player/PlayerClickToMove.cs` (`Movement` 필드 SerializeField 제거)

## 검증 내용

- 코드 리뷰 수준에서 컴파일 가능 여부와 참조 관계(Grid/Occupancy 공유, Awake 순서 문제)를 확인했다.
- Unity 에디터에서 사용자가 GridManager와 RobotTest 프리팹 간 인스펙터 참조(Camera, Movement, PlayerMovement, PlayerClickToMove)를 직접 연결했고, 씬 파일에 값이 정상 반영된 것을 확인했다.
- 실제 Play Mode에서의 클릭 기반 이동 동작은 아직 라이브로 검증되지 않았다(PR 테스트 플랜에 남겨둠).
- 전체 이동 흐름을 코드 리뷰로 재점검해 등록 실패 무로그, Movement 배선 중복, 이벤트 시스템 부재 3가지를 확인하고 반영했다.

## AI 활용 범위

AI는 GridManager 설계 제안(Grid 소유권 이전, 지연 생성 프로퍼티를 통한 Awake 순서 문제 해결), GridOccupancyTester 리팩터링, GameScene.unity의 GridOccupancyTester GameObject 제거(YAML 직접 편집), Player Movement 로직 코드 리뷰 및 스펙 대조 분석, `OnRobotArrived` 이벤트 설계·구현, 커밋 메시지 및 PR 생성을 지원했다. GridManager의 Player 인스펙터 슬롯 연결과 RobotTest 프리팹에 컴포넌트를 배치하는 작업은 사용자가 Unity 에디터에서 직접 수행했다. GridOccupancyTester.cs 삭제 여부, Object B 처리 방식, 리뷰에서 발견된 개선점 반영 여부 등 설계 방향은 AI가 제시한 선택지를 사용자가 검토하고 진행을 승인하는 방식으로 결정했다.

## 후속 작업

- 실제 Play Mode에서 클릭 기반 이동이 정상 동작하는지 라이브 검증이 필요하다.
- 이동 가능한 인접 Cell이 없을 때의 사용자 피드백은 현재 Debug.Log뿐이며, 필요 시 UI 이벤트로 확장한다.
- GridOccupancyTester.cs 스크립트 파일 자체의 삭제 여부는 별도로 결정한다.
