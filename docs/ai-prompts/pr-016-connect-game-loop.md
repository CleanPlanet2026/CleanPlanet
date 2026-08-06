# PR #16: 게임 루프 연결 및 WebGL 활성화 준비

## 개요

- 브랜치: `feature/connect-game-loop`
- PR: #16
- 작성일: 2026-08-06
- 작업 목적: 타이틀 화면부터 탐험과 감정으로 이어지는 기본 게임 루프를 연결하고 브라우저 배포 준비를 시작한다.

## 주요 요청

1. 원격 PR의 최신 수집·배터리 기능을 기반으로 새 작업 브랜치를 구성한다.
2. 타이틀 화면과 시작 버튼을 추가하고 `GameScene`으로 전환한다.
3. 탐험에서 수집한 아이템과 감정으로 획득한 골드를 씬 사이에 유지한다.
4. 감정 화면의 배터리 HUD가 전환 탭과 겹치지 않도록 배치한다.
5. GitHub Actions에서 Unity Personal 활성화 파일을 요청할 워크플로를 추가한다.

## AI와 논의한 내용

### 씬 흐름과 런타임 상태

기존 `GameScene`의 탐험 종료 흐름이 이미 `BaseScene`으로 연결되고 수집물이 `CollectionInbox`를 통해 전달되는 것을 확인했다. 수집물은 감정 완료 시점에만 전역 대기 목록에서 제거하고, 골드는 정적 런타임 값으로 유지하는 단순한 구조를 선택했다.

### 타이틀 화면과 UI 배치

기존 `SampleScene`의 GUID를 유지한 채 `TitleScene`으로 이름을 변경했다. 타이틀과 시작 버튼은 기존 uGUI를 사용했으며, 배터리 HUD는 감정 로봇·업그레이드 탭 오른쪽에 16px 간격으로 배치했다.

### WebGL CI 준비

GitHub 호스팅 러너에서 Unity WebGL을 빌드하는 GameCI와 GitHub Pages 구성을 선택했다. 이번 PR에서는 인증 정보를 포함하지 않고 Unity Personal용 활성화 파일 요청 워크플로만 추가했다.

## 주요 결정

- 시작 버튼은 `BaseScene`을 거치지 않고 `GameScene`을 직접 로드한다.
- 골드와 미감정 수집물은 플레이 세션 동안만 유지하며 영구 저장은 포함하지 않는다.
- 감정되지 않은 수집물은 씬을 벗어나도 유지하고, 감정기에 투입된 항목만 대기 목록에서 제거한다.
- Unity 계정 정보와 라이선스는 저장소 파일이 아닌 GitHub Actions Secrets로 관리한다.

## 변경된 주요 파일

- `Assets/Scenes/TitleScene.unity`
- `Assets/Scenes/BaseScene.unity`
- `Assets/Scripts/UI/TitleScreenController.cs`
- `Assets/Scripts/Core/Currency/CurrencyWallet.cs`
- `Assets/Scripts/Core/Collection/CollectionInbox.cs`
- `Assets/Scripts/Core/Appraisal/AppraisalTank.cs`
- `ProjectSettings/EditorBuildSettings.asset`
- `.github/workflows/activation.yml`

## 검증 내용

- Unity 빌드 설정에서 `TitleScene`, `BaseScene`, `GameScene` 순서를 확인했다.
- 타이틀 버튼의 `StartGame` 이벤트와 `GameScene` 참조를 확인했다.
- `Assembly-CSharp.csproj`를 Unity 4.7.1 참조 어셈블리로 빌드해 오류 0개를 확인했다.
- `git diff --check`에서 Unity가 생성한 빈 YAML 필드의 후행 공백 경고만 발생함을 확인했다.
- Unity 에디터의 별도 최종 배치 실행은 실행 중인 에디터 프로세스와 충돌해 수행하지 못했다.

## AI 사용 범위

AI는 저장소 구조와 기존 씬 전환·수집·감정 코드를 분석하고, 타이틀 씬 생성, 런타임 상태 공유, UI 위치 조정, GitHub Actions 워크플로 작성과 정적 검증을 수행했다. 사용자는 씬 흐름, UI 위치, 글로벌 상태 전환 및 CI/CD 구축 방향을 결정했다.

## 후속 작업

- GitHub Actions에서 Unity 활성화 파일을 발급하고 라이선스 Secrets를 등록한다.
- WebGL 빌드 및 GitHub Pages 배포 워크플로를 추가한다.
- 플레이 모드에서 타이틀 → 탐험 → 감정 → 재탐험 전체 흐름을 수동 검증한다.
- 필요하면 골드와 수집물의 영구 저장 기능을 추가한다.
