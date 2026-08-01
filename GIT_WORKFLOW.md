# Git 작업 흐름

## 브랜치

- `main`(보호 브랜치)
- `feature/<name>`
- `fix/<name>`
- `refactor/<name>`
- `docs/<name>`

## 커밋

컨벤셔널 커밋 형식을 사용한다.

예시:

```text
feat: 쓰레기 생성 기능 추가
fix: null 참조 오류 해결
refactor: 게임 관리자 단순화
```

## PR

- 하나의 PR에는 하나의 기능만 포함한다.
- 프로젝트가 빌드되는지 확인한다.
- PR의 변경 범위를 작게 유지한다.
- 변경 사항을 간단히 설명한다.
- AI를 사용한 경우 `docs/ai-prompts/`에 협업 기록을 정확히 한 개 추가한다.
- `docs/ai-prompts/README.md`에 정의된 형식을 따른다.

## 병합

- 스쿼시 병합을 사용한다.
- 병합 후 브랜치를 삭제한다.
