# PR #26: README 타이틀 이미지 및 배포 링크 추가

## 개요

- 브랜치: `docs/readme-deploy-link`
- PR: #26
- 작성일: 2026-08-07
- 작업 목적: 저장소 README에서 프로젝트 화면과 WebGL 배포 링크를 바로 확인할 수 있게 한다.

## 주요 요청

1. README의 기여자 목록을 제거한다.
2. 프로젝트 타이틀 화면 이미지를 README에 추가한다.
3. 실제 플레이 가능한 배포 링크를 README에 추가한다.
4. 프로젝트 진행 현황 문서를 최신 상태로 갱신하고 이를 필수 작업 지침에 추가한다.

## AI와 논의한 내용

### README 상단 구성

프로젝트의 `TitleBackground.png`가 2048×1152 타이틀 배경 에셋임을 확인하고 README 상단 이미지로 선택했다. GitHub Pages 설정을 조회해 공개 WebGL 주소가 `https://cleanplanet2026.github.io/CleanPlanet/`임을 확인했다.

### 병합 이후 변경 분리

PR #25가 병합된 뒤 기존 브랜치에 추가된 README 수정은 `main`에 포함되지 않았다. 병합된 `origin/main`에서 새 문서 브랜치를 만들고 README 최종 수정만 별도 PR로 분리했다.

### 진행 현황 유지 규칙

PR #25에서 추가된 탐험 캐릭터, 쓰레기 외형과 이동·수집·QTE 효과음이 진행 현황 문서에 누락된 것을 확인했다. 구현 상태와 검증 내용을 갱신하고, 기능·게임 흐름·개발 환경·검증 상태에 영향을 주는 모든 PR이 진행 현황 문서를 함께 검토하도록 `AGENTS.md`에 규칙을 추가했다.

## 주요 결정

- 기여자 섹션 대신 타이틀 이미지와 플레이 링크를 README 첫 부분에 배치한다.
- 이전 AI 기록은 수정하지 않고 PR #26 전용 기록을 정확히 한 개 추가한다.
- 진행 현황에 영향을 주는 변경은 같은 PR에서 `docs/PROJECT_PROGRESS.md`에 반영한다.

## 변경된 주요 파일

- `README.md`
- `AGENTS.md`
- `docs/PROJECT_PROGRESS.md`
- `docs/ai-prompts/pr-026-readme-deploy-link.md`

## 검증 내용

- GitHub Pages API에서 공개 배포 주소를 확인했다.
- README가 참조하는 타이틀 이미지 경로가 저장소에 존재함을 확인했다.
- 진행 현황의 기준 커밋과 PR #25 병합 상태를 원격 `main`에서 확인했다.
- `git diff --check`로 공백 오류가 없음을 확인했다.

## AI 사용 범위

사용자가 README의 최종 구성을 결정했다. AI는 원격 PR 병합 상태 확인, 타이틀 에셋과 배포 주소 검증, README 및 협업 기록 작성과 Git 작업을 수행했다.

## 후속 작업

- 없음.
