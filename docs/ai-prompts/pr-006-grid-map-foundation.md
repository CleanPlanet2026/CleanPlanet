# PR #6: 그리드 셀 점유 시스템 및 Target Cell 선정 로직 구현

## 개요

- 브랜치: `feature/player-movement`
- PR: #6
- 작성일: 2026-08-02
- 작업 목적: 한 라운드의 Map을 구성할 그리드 좌표계, 셀 점유 관리, 목표 셀 선정 로직의 최소 골격을 구현하고 Scene View에서 정합성을 확인한다.

## 주요 요청

1. Map을 셀 단위 Grid로 구성하고, 월드 좌표 ↔ Grid Index를 상호 변환하는 `GridSystem`을 `Assets/Scripts/Map/Grid/`에 신규 작성한다. 행/열 수는 맵마다 가변적이어야 하며, 부동소수점 오차로 변환이 틀어지지 않아야 한다.
2. 하나의 셀에는 하나의 오브젝트만 점유하도록 점유 테이블을 관리하는 시스템을 구성한다. Sprite Color가 다른 정사각형 오브젝트 A(검정)와 B(흰색)를 생성해 A의 생성/파괴, B의 이동에 따라 점유 상태가 즉시 갱신되는지 확인한다. 단, B를 직접 이동시키는 스크립트는 작성하지 않는다.
3. 플레이어가 선택한 더미(Object A) 기준 상하좌우 인접 셀 중 Grid 범위 내에 있고 비어 있는 셀만 후보로 남긴 뒤, 로봇(Object B)과의 Manhattan Distance가 가장 가까운 셀을 목표 셀(Target Cell)로 선정하는 로직을 `GridSystem`/`GridOccupancy`와 분리해 작성한다. 동일 거리 후보는 상→우→하→좌 순서로 고정하고, 4방향 모두 이동 불가능하면 실패 상태를 반환한다. 실제 이동 처리 로직은 작성하지 않는다.
4. Scene View에서 Grid 전체 범위, 셀 경계선, 셀 Index, 셀 중심을 확인할 수 있는 Gizmo를 추가하고, 이를 통해 드러난 Object와 Cell 크기 불일치를 Object 크기 조정으로 해결한다. Gizmo는 Editor 확인 용도로만 동작하며 기존 이동·길찾기 로직은 변경하지 않는다.
5. 각 요청은 구현 계획을 먼저 제시해 사용자 리뷰 후 승인을 받은 다음 코드에 반영한다.

## AI와 논의한 내용

### 좌표계와 점유 테이블의 분리

`GridSystem`은 MonoBehaviour에 의존하지 않는 순수 C# 클래스로 만들어 좌표 변환(`WorldToGrid`, `GridToWorldCenter`, `IsInBounds`)만 담당하게 하고, 점유 상태는 `GridOccupancy`라는 별도 클래스로 분리해 `GameObject[,]` 배열로 관리했다. 오브젝트별 점유 등록·해제·이동 추적은 `GridOccupant` MonoBehaviour가 담당해 `Register`/`Unregister`를 명시적으로 호출하고, `Update`에서 위치 변화를 감지해 점유 테이블을 자동 갱신한다.

### Target Cell 선정과 이동 로직의 분리

목표 셀 선정 로직은 이동 로직과 분리해야 한다는 제약에 따라 `TargetCellSelector`를 별도 클래스로 만들고, 기존 `GridSystem`(범위 판정)과 `GridOccupancy`(점유 조회)만 재사용하도록 했다. 동일 거리 후보 처리는 고정된 방향 배열(상→우→하→좌)을 순회하며 `dist < bestDist`(strict less-than) 비교로 먼저 찾은 후보를 유지해, 항상 동일한 우선순위 결과가 나오게 했다. 후보가 없으면 `targetIndex`를 `(-1,-1)`로, 반환값을 `false`로 처리하고 Debug.Log로 실패 원인을 남긴다.

### Gizmo 시각화와 수동 테스트 도구

Gizmo는 `UNITY_EDITOR` 전처리기로 감싸 빌드에 영향을 주지 않게 했고, 셀 경계선·점유 셀 강조·인덱스 라벨을 `GridOccupancyTester`에서 함께 그리도록 구성했다. Object B 이동 스크립트는 작성하지 않는 제약이 있어, `GridOccupancyTester`는 ContextMenu 기반으로 Object A/B 스폰·파괴·전체 셀 조회·Target Cell 선정을 수동 실행할 수 있게만 만들고, 실제 이동은 Scene 상에서 Transform을 직접 옮겨 `GridOccupant.Update()`가 추적하는 방식으로 확인했다.

## 주요 결정

- `GridSystem`은 좌표 변환만 담당하고, 점유 상태는 `GridOccupancy`로 완전히 분리한다.
- 오브젝트의 점유 등록/해제/이동 반영은 `GridOccupant` MonoBehaviour가 전담한다.
- `TargetCellSelector`는 이동 로직 없이 목표 셀 계산만 담당하며 기존 클래스만 재사용한다.
- 동일 거리 후보의 우선순위는 방향 배열 순서(상→우→하→좌)와 strict less-than 비교로 보장한다.
- 후보가 없을 때는 `(-1,-1)` + `false` 반환과 Debug.Log로 실패를 알린다.
- Gizmo와 수동 테스트 도구(`GridOccupancyTester`)는 Editor 전용으로 두고 실제 게임 플레이 로직에 영향을 주지 않는다.

## 변경된 주요 파일

- `Assets/Scripts/Map/Grid/GridSystem.cs`
- `Assets/Scripts/Map/Grid/GridOccupancy.cs`
- `Assets/Scripts/Map/Grid/GridOccupant.cs`
- `Assets/Scripts/Map/Grid/TargetCellSelector.cs`
- `Assets/Scripts/Map/Grid/GridOccupancyTester.cs`
- `Assets/Scenes/GameScene.unity`

## 검증 내용

- `GridSystem.RunTests()`로 임의 월드 좌표 10개를 Grid 변환 후 역변환해 원래 셀 중심 좌표와 일치하는지 확인했다(10/10 통과).
- `TargetCellSelector.RunTests()`로 맵 경계 제외, 점유 셀 제외, 최단 거리, 동일 거리 우선순위, 전방향 막힘 5개 케이스를 확인했다(5/5 통과).
- `GridOccupancyTester`의 ContextMenu로 Object A 생성 직후 점유 테이블 조회 결과와, Object B 파괴 직후 해당 셀이 비어있음으로 갱신되는지 확인했다.
- Scene View에서 여러 Grid 크기 설정에 따라 Gizmo(경계선, 인덱스 라벨, 점유 셀 강조)가 올바르게 갱신되는지, Object A/B의 크기가 Cell Size와 일치하는지 확인했다.

## AI 활용 범위

AI는 `GridSystem`/`GridOccupancy`/`GridOccupant`/`TargetCellSelector`/`GridOccupancyTester` 구현, Gizmo 시각화 코드 작성, 단위 테스트 작성 및 실행 로그 검증, PR 설명 작성을 지원했다. 각 기능의 목표·위치·제약·검증 기준은 사용자가 사전에 명시했고, 구현 계획에 대한 사용자 리뷰와 승인을 거친 뒤에만 코드에 반영했다.

## 후속 작업

- 실제 Player/Robot 이동 컨트롤러에서 `GridOccupant`와 `TargetCellSelector`를 연결해 실제 이동 로직을 구현한다.
- 로봇이 Target Cell로 실제 이동한 뒤 위치 정렬 상태를 Play Mode에서 확인한다.
- 임시 스프라이트를 최종 Object A/B 아트 리소스로 교체한다.
