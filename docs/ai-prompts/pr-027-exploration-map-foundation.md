# PR #27: 탐험 맵 확장 기반과 5개 구역 추가

## 개요

- 브랜치: `feature/exploration-map-foundation`
- PR: #27
- 작성일: 2026-08-07
- 작업 목적: 단일 탐험 공간을 확장 가능한 맵 프리팹 구조로 전환하고, 포탈로 왕복 가능한 5개 탐험 구역을 구성한다.

## 주요 요청

1. 탐험에서 여러 공간을 돌아다니고 조건에 따라 다음 스테이지를 해금할 수 있는 확장 가능한 구조를 설계한다.
2. 기존 맵과 어울리는 추가 배경을 제작하고 시각적으로 확인 가능한 양방향 포탈을 연결한다.
3. 맵 전환 이후 쓰레기 QTE가 동작하지 않는 문제와 탐험 수집물이 베이스로 전달되지 않는 문제를 수정한다.
4. Stage 1 후보 맵을 5개까지 확장하고 현재 진행 상태를 문서화한다.

## AI와 논의한 내용

### 맵과 스테이지 구조

탐험 공통 UI와 플레이어는 `GameScene`에 유지하고, 실제 탐험 공간만 프리팹으로 교체하는 구조를 선택했다. 방문한 맵 인스턴스는 비활성화해 맵별 쓰레기 상태를 유지하고, 현재 맵만 활성화하도록 했다. 스테이지 선택과 해금 시스템은 이번 PR 범위에서 제외하고 후속 작업으로 남겼다.

### 맵 개수와 연결 방식

초기에는 Stage 1을 3×3 구조의 9개 맵으로 확장하는 방안을 검토했다. 반복 콘텐츠와 WebGL 메모리·다운로드 비용을 고려해 우선 5개 맵에서 멈추고 플레이 밀도와 맵별 차별화를 먼저 확인하기로 결정했다.

현재 연결은 다음과 같다.

```text
Map01 — Map02 — Map03
  │
Map04 — Map05
```

### 전환 관련 오류

쓰레기 더미가 맵 루트 밖에 생성되어 이전 맵 오브젝트가 남는 문제를 확인했고, 쓰레기를 각 맵의 `TrashSpawner` 하위에 생성하도록 수정했다. 감정 서비스가 탐험 씬에서도 수집물을 소비할 수 있던 흐름과 감정 설정 참조 초기화 순서를 정리해 베이스에서만 감정과 보상 지급이 시작되도록 했다.

## 주요 결정

- 탐험 맵은 씬을 늘리지 않고 `ExplorationMap` 프리팹으로 관리한다.
- `ExplorationManager`가 맵 로드, 캐시, 활성 전환과 진입 위치 배치를 담당한다.
- 포탈은 목적지 맵 ID와 목적지 진입점 ID를 명시적으로 연결한다.
- 포탈 위치에는 런타임 생성 방식의 발광 링을 표시한다.
- Map01~Map05를 현재 Stage 1 후보 범위로 유지하고 추가 맵 제작은 플레이 검증 이후 결정한다.
- 베이스 스테이지 선택 UI와 해금 조건은 별도 후속 PR로 분리한다.

## 변경된 주요 파일

- `Assets/Scripts/Map/ExplorationManager.cs`
- `Assets/Scripts/Map/ExplorationMap.cs`
- `Assets/Scripts/Map/ExplorationPortal.cs`
- `Assets/Scripts/Map/ExplorationEntryPoint.cs`
- `Assets/Scripts/Map/ExplorationPortalVisual.cs`
- `Assets/Scripts/Map/GridManager.cs`
- `Assets/Scripts/Player/PlayerClickToMove.cs`
- `Assets/Scripts/Trash/TrashSpawner.cs`
- `Assets/Scripts/Core/Appraisal/AppraisalService.cs`
- `Assets/Scripts/Core/Appraisal/AppraisalTank.cs`
- `Assets/Prefabs/Map/ExplorationMap01.prefab` ~ `ExplorationMap05.prefab`
- `Assets/Art/Sprites/Environment/ExplorationBackground02.png` ~ `ExplorationBackground05.png`
- `Assets/Scenes/GameScene.unity`
- `Assets/Scenes/BaseScene.unity`
- `docs/PROJECT_PROGRESS.md`

## 검증 내용

- Unity 포함 .NET Framework 4.7.1 참조로 `Assembly-CSharp.csproj` 빌드: 오류 0개
- `git diff --check` 통과
- Map01~Map05 프리팹의 중복 오브젝트 ID 및 끊어진 내부 참조 없음
- 8개 포탈의 목적지 맵과 진입점 연결이 모두 유효함을 정적으로 확인
- 추가 배경 4장의 규격이 1672×941임을 확인
- 사용자가 Map03까지의 포탈 왕복과 쓰레기 QTE 기본 동작을 플레이로 확인
- Unity 배치 실행은 환경의 `BuildReportRestService` 소켓 초기화 충돌로 완료하지 못해 다섯 맵 전체의 Play Mode 및 WebGL 회귀 검증은 후속 작업으로 남김

## AI 사용 범위

AI는 기존 맵·이동·쓰레기·감정 코드를 분석하고, 맵 프리팹 전환 구조와 포탈 연결을 구현했다. 추가 탐험 배경을 기존 픽셀 아트의 분위기와 규격에 맞춰 생성하고 Unity 에셋 참조를 구성했다. 사용자는 맵 확장 방향, 단계별 제작 수량, 5개 맵에서 우선 멈추는 결정과 실제 플레이 결과를 확인했다.

## 후속 작업

- Map01~Map05 전체 포탈 왕복과 쓰레기 상태를 Unity Play Mode 및 WebGL에서 회귀 검증한다.
- Stage 1 완료 조건과 다음 스테이지 해금 조건을 정의한다.
- 베이스에 스테이지 선택 UI를 추가한다.
- 맵별 쓰레기 구성, 보상과 특별 목표를 차별화한다.
