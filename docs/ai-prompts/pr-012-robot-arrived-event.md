# PR #12: PlayerMovement에 OnRobotArrived 이벤트 추가

## 개요

- 브랜치: `feature/robot-arrived-event`
- PR: #12
- 작성일: 2026-08-04
- 작업 목적: 사용자가 제시한 목표/요구사항/제약사항/완료조건 형식의 스펙과 PR #11의 Player Movement 구현을 대조한 결과, 이동 완료를 알리는 이벤트 기반 구조가 없다는 gap을 확인하고 이를 보완한다.

## 주요 요청

1. 구현된 Player Movement 로직을 사용자가 제시한 스펙(목표/요구사항/제약사항/완료조건)과 대조해 부족한 부분을 확인한다.
2. 그중 이동 완료를 알리는 이벤트 기반 구조(`OnRobotArrived`)를 추가한다.

## AI와 논의한 내용

### 이벤트 발행 시점

`MoveAlongPath` 코루틴에는 두 가지 종료 경로가 있다 — (1) 목표 셀까지 정상 도달, (2) 선점으로 인한 재탐색 실패 등으로 중단. 이벤트는 (1) 정상 도달 시에만 발행하고, (2) 중단 경로에서는 발행하지 않기로 했다. 구독자가 이벤트를 받으면 항상 "실제로 도착했다"고 신뢰할 수 있어야 하기 때문이다.

### 이벤트 시그니처

`event Action<Vector2Int>`로 도착한 Cell 인덱스를 함께 전달하도록 했다. 프로젝트 내 기존 이벤트 패턴(`AppraisalReelSequencer.OnPayoutConfirmed`)과 동일하게 `event Action<T>` 형태를 따랐다.

## 주요 결정

- `PlayerMovement`에 `public event Action<Vector2Int> OnRobotArrived`를 추가한다.
- `MoveAlongPath`가 while 루프를 정상 종료했을 때(=finalGoal 도달)만 발행한다.
- 이동 중단 경로(`yield break`)에서는 발행하지 않는다.

## 변경된 주요 파일

- `Assets/Scripts/Player/PlayerMovement.cs` (`OnRobotArrived` 이벤트 추가 및 발행)

## 검증 내용

- 코드 리뷰 수준에서 이벤트 발행 시점(정상 도착 vs 중단 경로)이 의도대로 분기되는지 확인했다.
- 실제 Unity 에디터 Play Mode에서 이벤트 구독자가 정상적으로 콜백을 받는지는 아직 라이브로 검증되지 않았다(PR 테스트 플랜에 남겨둠).

## AI 활용 범위

AI는 기존 Player Movement 구현을 사용자가 제시한 스펙 형식과 대조해 gap을 분석하고, `OnRobotArrived` 이벤트의 발행 시점·시그니처를 설계해 구현했다. 이벤트를 추가하기로 한 결정과 발행 시점(정상 도착 시에만)은 AI가 제시한 분석과 제안을 사용자가 검토하고 승인하는 방식으로 확정했다.

## 후속 작업

- 실제 Play Mode에서 `OnRobotArrived` 구독자가 정상 동작하는지 라이브 검증이 필요하다.
