# PR #15: 쓰레기 수집물 감정 연동과 배터리 탐험 루프 추가

## 개요

- 브랜치: `feature/collectible-collection`
- PR: #15
- 작성일: 2026-08-06
- 작업 목적: 그리드 수집(GameScene)과 감정/재화(BaseScene)가 분리된 채로 남아있던 핵심 게임 루프를 연결하고, "탐험 → 배터리 소진/중단 → 베이스 복귀 → 충전 후 재출발"의 반복 가능한 플레이 사이클을 완성한다.

## 주요 요청

**수집 → 감정 연동**
1. 쓰레기 수집(GameScene)과 감정(BaseScene) 연결.
2. 더미 종류(일반/유리/전자/보석)별 드랍 계승 구조 개편.
3. 수집물 획득 시 이름/개수를 우측에 팝업으로 표시.

**배터리 탐험 루프**
4. 배터리 소진/중단 시 베이스 복귀, 베이스에서 재충전해야 재출발 가능한 기본 흐름.
5. 배터리 잔량 바 + 퍼센트 UI(좌상단).
6. 배터리 소진 시 경고 UI 후 베이스 복귀, 소진값 그대로 베이스에서 충전 시작.
7. 탐험 중단/출발 버튼(우측 하단, 3초 홀드 확정) 추가.

**버그 수정**
8. 배터리 바 Fill이 시각적으로 줄어들지 않는 문제(2차례).
9. 탐험 중단 버튼 클릭 시 로봇이 엉뚱하게 이동하는 문제.
10. QTE 진행 중 그리드 클릭으로 이동 가능한 문제.
11. 홀드 버튼을 누른 채 유지하면 씬이 무한 전환되는 문제, 탐험 출발 직후에도 입력이 이어져 로봇이 버튼 쪽으로 이동하는 문제.

**기타**
12. 현재 진행 상황 리뷰 및 다음 우선순위 파악.
13. 커밋 및 PR 준비.

## AI와 논의한 내용

### 씬 구조 (GameScene ↔ BaseScene)

`AppraisalTank`(감정)는 BaseScene에만, `TrashInteractionController`(수집/QTE)는 GameScene에만 있고 씬 전환 코드가 전혀 없어 단순 연결이 아니라 구조 결정이 필요했다. "한 씬으로 합치기" / "두 씬 유지 + 전환 + 영속 데이터 전달" / "연결 로직만 우선 작성" 세 가지를 제시했고, 사용자가 두 번째(두 씬 유지)를 선택했다. `CollectionInbox`(정적 클래스)로 GameScene에서 모은 수집물을 들고 있다가 BaseScene 진입 시 `AppraisalTank`의 기존 스폰 목록에 병합하는 방식으로 구현했다.

### 더미별 드랍 가중치

기존 `GeneralTrashPile`/`GlassTrashPile` 데이터에 이미 있던 패턴(자기 등급 가중치 1, 하위 등급 가중치 3)을 근거로, AI가 이를 일반화한 계승 구조(`OwnItems` + `PreviousTier` 참조)를 제안하고 그대로 적용했다. 정확한 밸런스 수치 자체는 사용자 확인이 필요한 상태로 남겨뒀다.

### 씬 UI를 AI가 직접 작성할지 여부

PR #14(QTE 원형 UI)에서 씬 YAML 직접 편집이 여러 차례 문제를 일으켰던 전례가 있어, 이번에는 이미 검증된 기존 패턴(Canvas/Image/Text 구조, BaseScene의 Currency HUD 등)을 그대로 재사용하는 조건으로 AI가 직접 씬 YAML을 작성했다. 그 과정에서 `Image.Type` enum 값(Filled=3)을 Sliced=1로 잘못 기입한 버그가 발생했고, 사용자가 Play 모드에서 발견해 리포트한 뒤 AI가 원인을 특정해 수정했다.

### 씬 전환 중 입력 유지 문제

홀드 버튼을 누른 채로 씬이 전환되면 새 씬의 버튼/클릭 이동이 물리적으로 계속 눌려있는 마우스 상태를 새 입력으로 오인하는 문제를 사용자가 실제 플레이로 발견했다. AI가 원인(씬마다 새로 생성되는 EventSystem/InputAction이 이전 프레스 상태를 이어받는 문제)을 분석하고, 완전히 뗐다가 다시 눌러야 인정되는 `PointerGate` 정적 게이트로 해결했다.

## 주요 결정

- `CollectionInbox`/`RobotBattery`/`PointerGate`는 모두 Unity 컴포넌트가 아닌 정적 클래스로, 씬 전환 간 상태를 유지한다. 단 일반 정적 필드이므로 Play 모드 종료·게임 재시작 시 초기화되며, 디스크에 영구 저장되지는 않는다.
- `TrashPileType`은 자기 등급 수집물(`OwnItems`)과 이전 등급 참조(`PreviousTier`)만 갖도록 재작성해 하위 등급 목록을 중복 나열하지 않는다.
- 탐험 중단/출발은 3초 홀드 버튼(`HoldToConfirmButton`, 재사용 가능한 위젯)으로 구현하고, 키 입력(R)도 함께 지원한다.
- 배터리가 소진되면 즉시 전환하지 않고 경고 패널을 2초간 보여준 뒤 베이스로 전환한다.
- 탐험 출발은 배터리가 기준치(기본 100%, 조절 가능) 이상일 때만 허용한다.

## 변경된 주요 파일

- `Assets/Scripts/Core/Collection/CollectionInbox.cs`, `Assets/Scripts/Trash/TrashCollectionRelay.cs` (신규)
- `Assets/Scripts/Core/Appraisal/AppraisalTank.cs` (인박스 병합)
- `Assets/Scripts/Trash/TrashPileType.cs` (등급 계승 구조로 재작성), 관련 4종 더미 `.asset`/`.prefab`
- `Assets/Scripts/Core/Robot/RobotBattery.cs`, `BatteryDrainer.cs`, `BatteryCharger.cs`, `ExplorationReturnTrigger.cs`, `ExplorationLauncher.cs`, `LowBatteryWarning.cs` (신규)
- `Assets/Scripts/UI/BatteryHudView.cs`, `HoldToConfirmButton.cs`, `CollectiblePopupView.cs` (신규)
- `Assets/Scripts/Utils/PointerGate.cs` (신규)
- `Assets/Scripts/Player/PlayerClickToMove.cs` (UI 클릭 가드, `PointerGate` 체크)
- `Assets/Scripts/Trash/TrashInteractionController.cs` (QTE 중 이동 잠금)
- `Assets/Scenes/GameScene.unity`, `Assets/Scenes/BaseScene.unity` (EventSystem, Battery HUD Canvas, 홀드 버튼, 경고 패널, 수집물 팝업 UI 배선)
- `ProjectSettings/EditorBuildSettings.asset` (GameScene을 Build Settings에 등록)

## 검증 내용

- 씬 YAML 직접 편집 후 fileID 중복 여부와 문서 블록 수 증가량을 스크립트로 정적 대조.
- Unity 에디터가 세션 내내 열려 있어 파일 변경이 정상적으로 재임포트되는지 간접 확인.
- 사용자가 Unity 에디터 Play 모드에서 실제 플레이하며 검증했고, 그 과정에서 발견한 버그 4건(Fill 타입 오류, UI 클릭이 그리드 이동으로 새는 문제, QTE 중 이동 가능, 씬 전환 중 입력 유지)을 리포트해 AI가 원인 분석 후 수정.
- 자동화 테스트는 추가하지 않음(기존에도 없었음).

## AI 활용 범위

AI가 전체 아키텍처 설계(정적 상태 클래스 도입, 이벤트 기반 시스템 연결), 코드 구현, 씬 YAML 직접 작성(Canvas/Image/Text/EventSystem UI 배선 포함), 커밋 및 PR 생성을 수행했다. 씬 구조 유지 방식은 AI가 선택지를 제시하고 사용자가 결정했으며, 배터리 임계값 등 세부 수치는 AI가 기존 패턴에 근거해 기본값을 제안했다. 실제 플레이 검증은 전부 사용자가 Unity 에디터에서 수행했고, 그 과정에서 발견된 버그는 모두 사용자가 리포트한 뒤 AI가 수정했다.

## 후속 작업

- QTE 원형 UI 최종 시각 마무리(구간 표시, 회전 보정값 튜닝) — PR #14부터 이월.
- 업그레이드 실제 능력치 적용 및 재화/진행도 영구 저장·불러오기.
- Unity Test Framework 기반 자동화 테스트 도입.
- 배터리 소모/충전 속도, 드랍 가중치 등 수치 밸런스 튜닝.
