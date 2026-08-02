# PR #7: Grid 기반 로봇 길찾기 및 이동 시스템 구현

## 개요

- 브랜치: `feature/player-movement`
- PR: #7
- 작성일: 2026-08-02
- 작업 목적: PR #6에서 구축한 Grid 좌표계/점유 시스템/Target Cell 선정 로직을 실제 로봇 이동에 연결한다. 길찾기, 셀 단위 이동, 클릭 기반 목적지 결정까지 구현해 Grid 기반 이동의 전체 흐름을 완성한다.

## 주요 요청

1. GridSystem/GridOccupancy를 활용해 로봇의 현재 위치에서 목표 셀까지 4방향 BFS/A*로 최단 경로를 계산하는 GridPathfinder 작성. 대각선 이동 제외, 점유 셀은 장애물로 처리, 도달 불가 시 무한 탐색 없이 즉시 실패 반환.
2. Pathfinder가 반환한 경로를 따라 로봇이 Cell 단위로 순차 이동하는 PlayerMovement 작성. 이동 시작 전 현재 Cell 해제, 도착 시 다음 Cell 점유. 이동 중에는 새 이동 명령 무시. Cell 중심으로만 정확히 이동/정지.
3. 플레이 중 클릭한 Grid Cell을 이동 목적지로 결정하는 로직 작성. 클릭 대상이 장애물(더미 등)이면 인접 4방향 중 로봇과 가장 가까운 빈 셀로 목적지 자동 보정.
4. 각 스크립트는 구현 계획을 먼저 제시해 사용자 리뷰와 승인을 받은 뒤에만 코드에 반영한다.

## AI와 논의한 내용

### 시작 셀과 목표 셀이 같은 경우의 점유 체크 순서

GridPathfinder 최초 구현에서 `start == goal`일 때 점유 체크(`IsOccupied(goal)`)가 먼저 실행돼, 로봇이 서 있는 자기 셀을 다시 목적지로 클릭하면 자기 자신의 점유 때문에 실패를 반환하는 버그를 리뷰 중 발견했다. `start == goal` 분기를 점유 체크보다 앞으로 옮겨 수정했다.

### PlayerMovement의 점유 관리 방식

로봇 이동 시 Occupancy를 GridOccupant(매 프레임 폴링으로 위치 변화를 감지해 자동 갱신)에 맡길지, PlayerMovement가 직접 관리할지 논의했다. GridOccupant의 자동 갱신은 "이동 시작 전 해제, 도착 시 점유"라는 요구된 2단계 타이밍과 충돌할 수 있어, PlayerMovement가 자체적으로 CurrentIndex와 점유를 관리하고 GridOccupant는 사용하지 않기로 결정했다.

### 이동 중 목적지 셀이 다른 오브젝트에 선점되는 경우

전환 구간(셀 사이 이동 중) 동안은 로봇이 어느 셀도 점유하지 않는 시점이 생기므로, 다른 오브젝트가 그 사이에 목표 셀을 선점할 수 있다. 이 경우 직전 셀로 안전 복귀 후 즉시 목적지까지 재탐색하는 방식으로 처리하기로 결정했다(대안이었던 "정지 후 안전 복귀만"보다 매끄러운 사용자 경험을 위해 재탐색을 선택).

### 클릭 목적지 결정 로직

클릭한 셀이 점유돼 있을 때 인접 셀을 찾는 로직은 PR #6에서 이미 구현된 TargetCellSelector를 그대로 재사용했다. 새로 작성한 PlayerClickToMove는 입력 감지(New Input System 마우스 좌클릭)와 목적지 결정만 담당하고, 실제 이동은 PlayerMovement.TryMoveTo를 그대로 호출한다.

### 씬 연결 방식

PlayerClickToMove의 Camera/Movement 참조를 처음에는 `[SerializeField] private` 필드로 뒀는데, GridOccupancyTester가 Object B를 런타임에 코드로 생성하는 구조라 Inspector 드래그도, 코드 대입도 불가능한 문제를 테스트 과정에서 발견했다. `[field: SerializeField] public` 프로퍼티로 바꿔 Inspector 배치와 코드 대입을 모두 지원하도록 수정했다.

### 테스트 하네스 연결과 남은 작업

GridOccupancyTester(Editor 테스트용 스크립트)가 Object B 생성 시 PlayerMovement/PlayerClickToMove를 연결하도록 임시로 배선했다. 정식 게임 플레이용 부트스트랩(GridManager 등)은 아직 없어 후속 작업으로 남겨두기로 했다.

## 주요 결정

- PlayerMovement는 GridOccupant를 사용하지 않고 CurrentIndex/Occupancy를 자체 관리한다.
- 이동 중 목적지 셀이 선점되면 안전 복귀 후 즉시 재탐색한다.
- PlayerClickToMove는 입력 감지와 목적지 결정만 담당하고, 인접 셀 탐색은 기존 TargetCellSelector를 그대로 재사용한다.
- Grid/Occupancy는 GridOccupant와 동일하게 외부에서 할당하는 방식(공개 프로퍼티)을 유지한다.
- 테스트 배선은 GridOccupancyTester에 임시로 두고, 정식 부트스트랩 분리는 후속 작업으로 미룬다.

## 변경된 주요 파일

- `Assets/Scripts/Map/Grid/GridPathfinder.cs` (신규)
- `Assets/Scripts/Player/PlayerMovement.cs` (Rigidbody2D 자유 이동 → Grid 경로 추종 이동으로 전면 교체)
- `Assets/Scripts/Player/PlayerClickToMove.cs` (신규)
- `Assets/Scripts/Map/Grid/GridOccupancyTester.cs` (테스트 배선 추가)
- `Assets/Scripts/Player/PlayerTrashCollector.cs`, `TrashInventory.cs`, `Assets/Scripts/Trash/TrashItem.cs`, `TrashSpawner.cs` (삭제)

## 검증 내용

- `GridPathfinder.RunTests()`로 우회 경로, 미로형 장애물, 완전 차단, 반복 실행 동일성, 50x50 그리드 성능(약 0.3ms) 5개 케이스를 확인했다(5/5 통과).
- Play Mode에서 로봇이 더미를 우회해 목표 셀까지 정확히 이동하고, 최종 위치가 셀 중심과 정확히 일치하며, 이동 시작 전/도착 시 Occupancy가 갱신되고 잔여 점유가 남지 않는 것을 확인했다.
- 이동 중 새 이동 명령이 무시되는지 확인했다.
- 경로 중간 셀이 다른 오브젝트에 선점되는 상황을 강제로 재현해, 로봇이 안전하게 복귀 후 재탐색으로 목적지까지 도달하고 점유 정보가 일관되게 유지되는 것을 확인했다.
- PlayerClickToMove의 목적지 결정 로직(빈 셀/점유 셀/후보 없음 3가지 경우)을 확인했다.
- 기존 GridPathfinder 테스트 결과가 이후 변경(PlayerMovement, PlayerClickToMove 작업)에도 동일하게 유지되는 것을 재확인했다.

## AI 활용 범위

AI는 GridPathfinder/PlayerMovement/PlayerClickToMove 구현, GridOccupancyTester 연결 코드 작성, 단위 테스트 및 수동 Play Mode 검증, 커밋 메시지 및 PR 설명 작성을 지원했다. 각 기능의 목표·위치·제약·검증 기준은 사용자가 사전에 명시했고, 점유 관리 방식(GridOccupant 미사용)과 선점 시 처리 방식(즉시 재탐색)은 AI가 제시한 선택지 중 사용자가 직접 결정했다. 구현 계획에 대한 사용자 리뷰와 승인을 거친 뒤에만 코드에 반영했다.

## 후속 작업

- GridOccupancyTester에 임시로 배선한 PlayerMovement/PlayerClickToMove 연결을 정식 게임 플레이용 부트스트랩 스크립트로 분리한다.
- 클릭 기반 이동의 실제 마우스 입력 경로(InputAction 트리거)는 자동화 테스트 환경 제약으로 라이브 검증하지 못했으므로, 실제 플레이 환경에서 재확인이 필요하다.
- 이동 가능한 인접 Cell이 없을 때의 사용자 피드백은 현재 Debug.Log뿐이며, 필요 시 UI 이벤트로 확장한다.
