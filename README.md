# CleanPlanet

쓰레기를 수집하고 재활용해 황폐한 세계를 발전시키는 2D 탑다운 방치형·증분형 게임입니다.

로봇과 함께 탐험 지역의 쓰레기를 모으고, 베이스에서 수집물을 감정해 골드를 획득하세요. 획득한 골드로 로봇을 업그레이드하면 더 오래, 더 빠르게 탐험할 수 있습니다.

## 게임 흐름

```text
쓰레기 선택 → 로봇 이동 → QTE 수집 → 베이스 복귀
→ 수집물 감정 → 골드 획득 → 로봇 업그레이드 → 다시 탐험
```

## 주요 기능

- 그리드 기반 클릭 이동과 경로 탐색
- 방향별 로봇 애니메이션과 이동 효과음
- 등급별 쓰레기 더미와 무작위 수집물
- 실패·성공·대성공으로 구분되는 원형 QTE
- 배터리 소모, 충전 및 자동 베이스 복귀
- 수집물 감정과 배수 기반 골드 보상
- 이동·배터리·수집·탐험·감정 업그레이드
- 게임 진행 상황 자동 저장
- WebGL 자동 빌드 및 GitHub Pages 배포

## 조작법

| 조작 | 기능 |
| --- | --- |
| 마우스 왼쪽 클릭 | 쓰레기 더미 선택 및 이동 |
| `Space` | QTE 판정 |
| `R` | 탐험 시작 또는 복귀 |
| 탐험 종료 버튼 | 베이스로 조기 복귀 |

## 개발 환경

- Unity `6000.5.6f1`
- Universal Render Pipeline `17.6.0`
- Unity Input System `1.20.0`
- 주요 씬: `TitleScene`, `BaseScene`, `GameScene`

## 실행 방법

1. 저장소를 클론합니다.
2. Unity Hub에서 프로젝트 폴더를 엽니다.
3. Unity Editor `6000.5.6f1`을 사용합니다.
4. `Assets/Scenes/TitleScene.unity`를 열고 Play를 실행합니다.

## 프로젝트 구조

```text
Assets/
  Art/                 스프라이트, 애니메이션, 머티리얼
  Audio/               BGM 및 효과음
  Prefabs/             플레이어, 쓰레기, UI, 이펙트 프리팹
  Scenes/              게임 씬
  Scripts/             시스템별 C# 스크립트
  ScriptableObjects/   업그레이드 및 게임 밸런스 데이터
```

## 기여자

- [hoyadong1](https://github.com/hoyadong1)
- [boroboro01](https://github.com/boroboro01)
- [UHANKNAG](https://github.com/UHANKNAG)

## 관련 문서

- [프로젝트 진행 현황](docs/PROJECT_PROGRESS.md)
- [개발 지침](AGENTS.md)
- [Git 작업 흐름](GIT_WORKFLOW.md)
- [AI 협업 기록 작성 규칙](docs/ai-prompts/README.md)
