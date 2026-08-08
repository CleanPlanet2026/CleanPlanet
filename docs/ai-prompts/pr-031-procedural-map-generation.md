# PR #31: 타일셋 기반 절차적 스테이지 맵 생성 시스템 도입

## 개요

- 브랜치: `feature/exploration-map-expansion`
- PR: #31
- 작성일: 2026-08-08
- 작업 목적: 손으로 제작한 고정 탐험 맵을 절차적으로 생성되는 스테이지 기반 맵으로 교체하고, 스테이지 진행/전환 시스템을 도입한다.

## 주요 요청

1. 손맵 대신 타일셋 기반으로 매번 다른 탐험 맵을 절차적으로 생성하되, 플레이 가능한 연결성을 반드시 보장할 것(Phase 1~7 단계별 요구사항 문서 기반)
2. 플레이어를 따라다니는 Cinemachine 카메라 추가, 맵 경계 밖을 비추지 않도록 제한
3. 특정 Clean 수치를 넘으면 다음 스테이지로 넘어가는 버튼 — 씬 이동이 아니라 화면을 가리고 제자리에서 재생성하는 방식
4. Earth Clean 수치는 감정이 아니라 탐험 중 더미 상호작용 시 적립
5. 이동 중 새 입력이 들어오면 기존처럼 무시하지 말고 새 목적지로 방향 전환
6. Obstacle/Decoration 타일 배치 추가

## AI와 논의한 내용

### 벽/장애물 표현 방식

GridOccupancy가 임시 점유와 영구 벽을 구분하지 못하는 기존 구조를 확인하고, SetBlocked/IsBlocked를 추가해 IsOccupied에 통합하는 방식으로 기존 Pathfinder/TargetCellSelector/TrashSpawner를 코드 수정 없이 재사용하기로 결정.

### 생성 알고리즘 선택

BSP 룸/코리도 대신 셀룰러 오토마타 동굴 생성을 선택 — 기존 손맵이 열린 탐험 지형이라 이 쪽이 게임 톤에 더 맞고 구현도 단순함.

### 스테이지 전환 범위

"새 스테이지 이동은 씬 전환이 아니어야 한다"는 요청의 적용 범위를 사용자에게 확인 — StageSelectScene에서의 최초 진입은 씬 로드를 유지하고, 이미 GameScene에 있는 상태에서 다음 스테이지로 넘어갈 때만 화면을 가리고 제자리 재생성하는 것으로 확정.

### 기존 맵 시스템 삭제

절차적 생성기를 GameScene에 연결하는 과정에서, 기존 ExplorationManager가 플레이어를 여전히 옛 손맵 그리드에 등록해 새 맵을 벗어날 수 없는 문제를 발견 — 사용자 확인 후 ExplorationManager/ExplorationMap/ExplorationPortal 등 손맵 시스템 전체를 삭제.

## 주요 결정

- GridOccupancy는 최소 수정(메서드 2~3개 추가)만으로 벽 개념을 확장하고, 새 로직은 전부 Procedural 네임스페이스의 신규 클래스에 둔다.
- Earth Clean은 감정이 아니라 탐험 중 더미 상호작용(QTE 성공/실패/방사능 실패 포함) 시 적립한다.
- 스테이지 데이터는 StageConfig ScriptableObject로 관리하고, StageSelectView는 하드코딩 대신 이 배열을 순회하는 방식으로 재작성한다.
- 절차적 생성기와 StageAdvanceController가 스테이지 배열을 각자 따로 들지 않고 하나를 공유해 인스펙터 설정 불일치를 방지한다(코드 리뷰 반영).

## 변경된 주요 파일

- `Assets/Scripts/Map/Procedural/ProceduralMapGenerator.cs`
- `Assets/Scripts/Map/Procedural/StageConfig.cs`, `MapGenerationSettings.cs`, `TileSet.cs`, `MapCellType.cs`
- `Assets/Scripts/Map/Grid/GridOccupancy.cs`, `Assets/Scripts/Map/GridManager.cs`
- `Assets/Scripts/Core/Progress/EarthCleanMeter.cs`, `Assets/Scripts/Core/StageSessionState.cs`
- `Assets/Scripts/UI/StageSelectView.cs`, `StageAdvanceController.cs`
- `Assets/Scripts/Player/PlayerMovement.cs`, `PlayerClickToMove.cs`
- `Assets/Scripts/Utils/MapCameraBounds.cs`
- `Assets/Scripts/Trash/TrashSpawner.cs`, `TrashCollectionRelay.cs`
- 삭제: `Assets/Scripts/Map/ExplorationManager.cs`, `ExplorationMap.cs`, `ExplorationPortal.cs`, `ExplorationPortalVisual.cs`, `ExplorationEntryPoint.cs`, `Assets/Prefabs/Map/ExplorationMap01~05.prefab`

## 검증 내용

- 각 단계(Phase 1~7, 스테이지 전환, 장애물/장식 배치) 구현 후 사용자가 Unity 에디터에서 직접 Play 모드로 실제 동작을 확인하며 진행
- 코드 리뷰(High 수준)로 5건 발견 — StageConfig 에셋 영구 오염, 스테이지 배열 불일치 가능성, MapCameraBounds null 참조 3건은 즉시 수정. 이동 리다이렉트 실패 시 상태 정리 미흡, 극단적 설정 시 플레이어 시작 위치 소프트락 2건은 발생 조건이 드물어 이번 PR 범위에서 제외

## AI 활용 범위

AI가 요구사항 문서를 바탕으로 계획을 수립하고, Unity 에디터가 열려 있는 상태에서 씬/프리팹 파일을 직접 편집하며 전체 구현을 진행했다. 사용자는 각 단계 결과를 에디터에서 직접 플레이 테스트로 검증하고, 스테이지 전환 방식·Earth Clean 적립 시점 등 핵심 설계를 직접 결정했다.

## 후속 작업

- 밸런싱: 맵 크기/장애물 밀도/Clean 목표치 실측 조정
- Stage 3~5용 StageConfig·타일셋 추가 제작
- Earth Clean/Stage 진행도 HUD 상시 표시
- 코드 리뷰에서 낮은 우선순위로 남긴 두 건(이동 리다이렉트 실패 시 상태 정리, 극단적 설정 시 플레이어 시작 위치 안전장치)
