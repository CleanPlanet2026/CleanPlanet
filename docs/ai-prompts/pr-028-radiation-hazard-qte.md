# PR #28: 방사능 위험 연타 QTE 추가

## 개요

- 브랜치: `feature/radiation-hazard-qte`
- PR: #28
- 작성일: 2026-08-07
- 작업 목적: 일반 쓰레기 수집 중 낮은 확률로 발생하는 방사능 위험과 연타 QTE를 추가해 탐험에 긴장감과 선택 결과를 만든다.

## 주요 요청

1. 쓰레기 수집 중 낮은 확률로 방사능 위험이 발생하고, 기존 타이밍 QTE와 달리 스페이스바를 연속 입력해 회피하도록 한다.
2. 방사능 위험은 접근 전에 드러내지 않고 로봇이 더미에 도착한 시점에 판정한다.
3. 연타 QTE 실패 시 이번 탐험에서 보관 중인 수집물 일부를 잃는다.
4. 성공 시 추가 보상을 제공하고 탐험 도움말에 조작법을 안내한다.
5. 현재 기능 브랜치에서 베이스와 탐험 사이에 별도 스테이지 선택 씬을 추가한다.

## AI와 논의한 내용

### 위험 노출 방식

방사능 더미를 미리 표시하면 동일 보상의 위험 요소를 플레이어가 항상 피할 가능성이 높다고 판단했다. 따라서 모든 더미는 평소와 동일하게 보이고, 로봇 도착 시 확률로 방사능이 감지되는 방식으로 결정했다.

### 실패 손실과 성공 보상

수집물 전부를 잃는 방식은 낮은 확률에도 지나치게 불합리할 수 있어 현재 보관 수집물의 10%만 잃도록 했다. 손실은 최소 1개, 최대 3개이며 보관 수집물이 없으면 손실이 없다. 위험이 단순한 벌칙으로만 작동하지 않도록 성공 시 일반 Success 보상을 50% 늘린다.

### 기존 QTE와의 분리

타이밍 판정과 연타 판정은 입력 규칙과 UI가 달라 기존 `QteController`에 모드를 추가하지 않고 `RadiationQteController`와 `RadiationQteView`로 분리했다. `TrashInteractionController`는 로봇 도착 시 사용할 QTE만 선택한다.

## 주요 결정

- 방사능 발생 확률은 초기 5%에서 플레이 체감에 따라 15%로 높인다.
- 제한 시간은 3초, 성공 조건은 스페이스바 12회 입력이다.
- 성공 보상은 일반 Success 수집량의 150%다.
- 실패한 더미는 제거되고 해당 더미 보상은 지급하지 않는다.
- 실패 시 보관 수집물을 무작위로 10%, 최소 1개·최대 3개 잃는다.
- 손실은 수집 HUD와 저장 데이터에 즉시 반영한다.
- 테스트할 때만 Inspector의 발생 확률을 100%로 바꾸고 최종값은 15%로 유지한다.
- 베이스의 기존 출발 버튼은 배터리 조건을 유지한 채 스테이지 선택 화면으로 연결한다.
- Stage 1만 활성화하고 Stage 2·3은 향후 해금 시스템을 연결할 수 있는 잠금 카드로 표시한다.

## 변경된 주요 파일

- `Assets/Scripts/Trash/RadiationQteController.cs`
- `Assets/Scripts/UI/RadiationQteView.cs`
- `Assets/Scripts/Trash/TrashInteractionController.cs`
- `Assets/Scripts/Trash/TrashCollectionRelay.cs`
- `Assets/Scripts/Core/Collection/CollectionInbox.cs`
- `Assets/Scripts/UI/ExplorationCollectionHudView.cs`
- `Assets/Scripts/UI/ExplorationTutorialView.cs`
- `Assets/Scenes/GameScene.unity`
- `Assets/Scenes/StageSelectScene.unity`
- `Assets/Scenes/BaseScene.unity`
- `Assets/Scripts/UI/StageSelectView.cs`
- `Assets/Scripts/Core/Robot/ExplorationLauncher.cs`
- `ProjectSettings/EditorBuildSettings.asset`
- `docs/PROJECT_PROGRESS.md`

## 검증 내용

- Unity 포함 .NET Framework 4.7.1 참조로 `Assembly-CSharp.csproj` 빌드: 오류 0개
- `git diff --check` 통과
- `GameScene` 직렬화 오브젝트 ID 중복 0건
- `GameScene` 내부 참조 누락 0건
- 새 스크립트 GUID와 씬 컴포넌트 참조가 일치함을 확인
- `BaseScene`과 `StageSelectScene` 직렬화 오브젝트 ID 및 내부 참조를 정적으로 확인
- 실제 입력 속도, 성공·실패 연출과 수집물 손실 HUD는 Unity Play Mode 검증 필요

## AI 활용 범위

사용자는 숨겨진 방사능 판정, 연타 QTE, 실패 시 수집물 손실이라는 핵심 콘셉트와 구현 진행을 결정하고, 같은 기능 브랜치에서 스테이지 선택 씬을 추가하도록 요청했다. AI는 기존 QTE·수집 저장·HUD와 베이스 출발 구조를 분석하고 별도 연타 컨트롤러, 런타임 UI, 보상·손실 흐름, 스테이지 선택 화면, 씬 참조와 문서를 구현했다.

## 후속 작업

- 발생 확률을 임시로 100%로 설정해 성공·실패·수집물 0개 상태를 Unity Play Mode에서 검증한다.
- 실제 플레이를 통해 3초·12회 입력 난이도와 15% 발생 확률을 추가 조정한다.
- 필요하면 방사능 경고 및 성공·실패 전용 효과음을 추가한다.
- Stage 2 이후의 해금 조건과 실제 탐험 씬을 연결한다.
