# PR #22: 게임 루프 UX, 업그레이드 효과와 영구 저장 완성

## 개요

- 브랜치: `feature/integrate-game-ui`
- PR: #22
- 작성일: 2026-08-06
- 작업 목적: PR #21 이후의 게임 루프 UX를 정리하고 모든 업그레이드를 실제 시스템에 연결하며 주요 진행 데이터를 영구 저장한다.

## 주요 요청

1. 탐험과 베이스 사이 이동, 타이틀 복귀, 탐험 도움말과 수집 현황을 플레이어가 이해하기 쉽게 개선한다.
2. 이동·배터리·감정·수집 업그레이드 효과를 실제 게임 수치에 반영한다.
3. 구현되지 않은 분해 개념을 현재 게임 루프에 맞는 탐사 숙련도로 교체한다.
4. 골드, 수집물, 업그레이드, 배터리와 베이스 도달 상태를 게임 재실행 후에도 복원한다.
5. 현재 프로젝트 진행 상황을 다시 점검하고 문서화한다.

## AI와 논의한 내용

### 탐험 전환 UX

탐험 시작과 종료 모두에 길게 누르기를 요구하면 반복 플레이가 불편하고 기능 의도를 설명하기 어렵다는 점을 검토했다. 시작은 일반 클릭으로 유지하고, 진행 중 수집물을 두고 복귀하는 탐험 종료에만 확인창을 사용하는 것으로 결정했다.

### 업그레이드 역할 구분

기존 5개 트리 중 분해는 실제 시스템이 없어 효과를 연결할 대상이 없었다. 별도의 분해 시스템을 억지로 추가하지 않고 탐험에서 쓰레기의 양과 품질을 높이는 탐사 숙련도로 교체했다. 수납 제한을 두지 않기로 한 결정에 따라 압축 수납은 QTE 판정 범위를 넓히는 수거 스캐너로 교체했다.

### 저장 방식

해커톤 범위에서는 서버나 계정 연동보다 브라우저에서도 작동하는 로컬 단일 슬롯 자동 저장이 적합하다고 판단했다. 배터리는 매 프레임 변하므로 매 변경마다 저장소에 쓰지 않고 메모리에 반영한 뒤 5초 간격, 씬 전환, 일시정지와 종료 시 저장하도록 했다. 수집물은 표시 이름이 아니라 ScriptableObject 에셋 이름을 ID로 저장하고 베이스의 카탈로그에서 실제 에셋 참조를 복원한다.

## 주요 결정

- 탐험 시작은 즉시 실행하고 탐험 종료는 확인창에서 확정한다.
- 탐험 HUD에 이번 탐험의 수집물 총합과 종류별 개수를 표시한다.
- 최초 탐험 안내와 다시 열 수 있는 도움말 버튼을 제공한다.
- 베이스에 도달한 저장 기록이 있으면 타이틀에서 베이스 이동 버튼을 표시한다.
- 업그레이드 상태는 공용 런타임 상태로 관리하고 실제 게임 수치에서 읽는다.
- 이동, 배터리, 감정, 수집, 탐사 5개 트리를 모두 실제 효과에 연결한다.
- 분해 트리는 광역 탐지, 신호 증폭, 정밀 스캔으로 구성된 탐사 숙련도로 교체한다.
- 골드, 미감정 수집물, 업그레이드, 배터리와 베이스 도달 여부를 버전이 포함된 단일 저장 데이터로 관리한다.
- 감정 도중 종료해도 수집물이 손실되지 않도록 골드 지급 확정 시 수집물을 차감한다.
- 타이틀 설정창에서 두 번 눌러 진행 데이터를 초기화할 수 있게 한다.
- 음악·효과음 설정과 튜토리얼 확인 여부는 기존 별도 `PlayerPrefs`를 유지한다.

## 변경된 주요 파일

- `Assets/Scenes/TitleScene.unity`
- `Assets/Scenes/GameScene.unity`
- `Assets/Scenes/BaseScene.unity`
- `Assets/Scripts/UI/TitleScreenController.cs`
- `Assets/Scripts/UI/TitleAudioSettingsView.cs`
- `Assets/Scripts/UI/BaseMenuView.cs`
- `Assets/Scripts/UI/ExplorationTutorialView.cs`
- `Assets/Scripts/UI/ExplorationCollectionHudView.cs`
- `Assets/Scripts/UI/SkillTreeDetailController.cs`
- `Assets/Scripts/Upgrade/UpgradeRuntimeState.cs`
- `Assets/Scripts/Upgrade/UpgradeEffects.cs`
- `Assets/Scripts/Trash/TrashSpawner.cs`
- `Assets/Scripts/Trash/TrashPileType.cs`
- `Assets/Scripts/Trash/QteController.cs`
- `Assets/Scripts/Core/Appraisal/AppraisalCore.cs`
- `Assets/Scripts/Core/Persistence/GameSaveData.cs`
- `Assets/Scripts/Core/Persistence/GameSaveSystem.cs`
- `Assets/Scripts/Core/Persistence/GameSaveRunner.cs`
- `docs/PROJECT_PROGRESS.md`
- `docs/ai-prompts/pr-022-game-loop-progression.md`

## 검증 내용

- Unity `6000.5.6f1` 에디터에서 스크립트 어셈블리가 오류 없이 갱신되는 것을 확인했다.
- Unity 포함 .NET Framework 4.7.1 참조를 사용한 `Assembly-CSharp.csproj` 빌드에서 오류 0개를 확인했다.
- `git diff --check`를 통과했다.
- 수집물 16종이 저장 복원 카탈로그에 각각 한 번씩 연결된 것을 확인했다.
- 업그레이드 ID, 선행 조건, 연결선과 실제 효과 조회 ID가 일치하는지 정적 확인했다.
- 기존 PR #21 AI 협업 기록은 `main` 상태로 유지하고 이 PR 전용 기록만 변경 범위에 포함했다.

## AI 활용 범위

AI는 기존 씬과 스크립트 구조를 분석하고 UI 흐름, 업그레이드 효과, 탐사 트리, 저장 구조를 제안·구현했다. 또한 씬 직렬화 데이터와 수집물 카탈로그를 갱신하고 컴파일 및 Git 검증을 수행했다. 사용자는 UX 방향, 수납 제한 제외, 분해 트리 교체와 영구 저장 도입을 결정했다.

## 후속 작업

- Unity Play Mode와 WebGL에서 전체 게임 루프를 회귀 테스트한다.
- WebGL 새로고침 후 모든 저장 항목의 복원을 확인한다.
- 업그레이드 비용과 효과 수치를 실제 플레이 시간에 맞춰 조정한다.
- 임시 버튼 효과음을 최종 오디오 에셋으로 교체한다.
- Edit Mode/Play Mode 자동 테스트를 CI에 추가한다.
