# PR #17: WebGL 빌드 및 GitHub Pages 배포 자동화

## 개요

- 브랜치: `feature/webgl-pages-deploy`
- PR: #17
- 작성일: 2026-08-06
- 작업 목적: `main`의 Unity 프로젝트를 GitHub 호스팅 러너에서 WebGL로 빌드하고 GitHub Pages에 자동 배포한다.

## 주요 요청

1. 게임을 브라우저에서 바로 실행할 수 있는 CI/CD 절차를 단계별로 구축한다.
2. 로컬 PC가 아닌 GitHub Actions에서 WebGL 빌드를 수행한다.
3. Unity Personal 라이선스를 GitHub Actions Secrets로 안전하게 제공한다.

## AI와 논의한 내용

### Unity Personal 활성화 방식

폐기된 `unity-request-activation-file@v2` 실행이 실패한 뒤 GameCI v4 문서를 다시 확인했다. 최신 방식에 따라 Unity Hub가 로컬에서 생성한 `.ulf` 내용을 `UNITY_LICENSE`로 등록하고 Unity 계정 정보는 별도 Secret으로 관리하도록 변경했다.

### WebGL 호스팅

별도 서버 없이 사용할 수 있는 GitHub Pages를 선택했다. Pages에서 압축 응답 헤더를 직접 설정하지 않아도 동작하도록 Unity WebGL Decompression Fallback을 활성화했다.

## 주요 결정

- `main` push와 수동 실행에서 배포 워크플로가 동작한다.
- Unity 버전은 `ProjectSettings/ProjectVersion.txt`에서 자동 감지한다.
- GameCI 빌드 출력 `build/WebGL/WebGL`을 Pages artifact로 배포한다.
- Unity 라이선스와 계정 정보는 저장소에 포함하지 않고 GitHub Actions Secrets만 사용한다.
- 폐기된 활성화 파일 요청 워크플로는 제거한다.

## 변경된 주요 파일

- `.github/workflows/deploy-webgl.yml`
- `.github/workflows/activation.yml`
- `ProjectSettings/ProjectSettings.asset`

## 검증 내용

- Python PyYAML로 배포 워크플로 구문을 파싱했다.
- GameCI 공식 문서에서 `buildName`과 기본 `buildsPath` 조합의 출력 경로를 확인했다.
- GitHub Actions Secrets `UNITY_LICENSE`, `UNITY_EMAIL`, `UNITY_PASSWORD` 등록 여부를 이름만 확인했다.
- GitHub Pages 배포 소스가 Actions로 설정된 것을 사용자에게 확인받았다.
- 실제 WebGL 빌드와 배포는 이 PR이 `main`에 병합된 후 워크플로 실행으로 검증한다.

## AI 사용 범위

AI는 최신 GameCI 및 GitHub Pages 문서를 확인하고, 라이선스 등록 절차 안내, 저장소 Secret 이름 확인, 배포 워크플로와 Unity WebGL 설정 변경 및 정적 검증을 수행했다. 사용자는 Unity Personal 활성화와 GitHub Secrets 및 Pages 저장소 설정을 직접 완료했다.

## 후속 작업

- PR 병합 후 `Build and deploy WebGL` 워크플로 결과를 확인한다.
- 최초 배포 URL에서 타이틀부터 게임 씬까지 브라우저 플레이를 검증한다.
- Unity `6000.5.6f1` GameCI 이미지가 없으면 지원되는 Unity 버전 또는 self-hosted runner를 검토한다.
