# PR #30: 감정 시스템 개선 및 2번 릴·속도 업그레이드

## 개요

- 브랜치: `feature/emotion-panel-polish`
- PR: #30
- 작성일: 2026-08-08
- 작업 목적: 감정(유리관·릴) 시스템의 버그를 수정하고 신규 업그레이드 2종을 추가

## 주요 요청

1. 감정이 유리관 바닥 아이콘부터 순서대로 진행되게 (수집 순서가 아니라)
2. 유리관 아이콘이 투명하게 보이는 버그 수정
3. 같은 종류 수집물이 여러 개일 때 하나 감정 시 다른 것도 사라지는 버그 수정
4. 감정 릴을 하나 더 여는 업그레이드 (2개 동시 감정)
5. 감정 전체(시간·연출·사운드)가 빨라지는 속도 업그레이드

## AI와 논의한 내용

### 감정 순서와 낙하 랜덤성

낙하 순서는 수집순(FIFO)이나 가로 위치가 랜덤이라 바닥 안착 순서가 들쭉날쭉함을 확인. "바닥부터 감정 + 낙하 가로 범위 축소"로 결정.

### 투명 버그 원인

알파·프리팹·머티리얼·조명을 차례로 배제. 최종 원인은 아이콘과 어둠 오버레이의 정렬 순서(Sorting Order) 충돌로 판명, 아이콘 Order를 올려 해결.

### 2번 릴 동시성

단일 루프에서 레인0→레인1 순차 배정 + `_inFlightItems`로 이중 배정 방지 방식 채택.

## 주요 결정

- 감정 순서: 바닥 안착 아이콘 우선, 관 없으면 FIFO 폴백
- 투명 버그: 아이콘 Sorting Order 상향(정렬 교정)으로 해결
- 2번 릴: 업그레이드(`appraisal_second_reel`, 1회성)로 개방, 스킬트리 새 줄기
- 감정 속도: `appraisal_speed` 구매 시 1.3배 가속(초기 2배에서 하향)

## 변경된 주요 파일

- `Assets/Scripts/Core/Appraisal/AppraisalService.cs`
- `Assets/Scripts/Core/Appraisal/AppraisalTank.cs`
- `Assets/Scripts/Core/Appraisal/AppraisalReelSequencer.cs`
- `Assets/Scripts/Core/Appraisal/AppraisalConfig.cs`
- `Assets/Scripts/UI/AppraisalEffectDirector.cs`
- `Assets/Scripts/UI/AppraisalSecondReelUnlockView.cs` (신규)
- `Assets/Scripts/Upgrade/UpgradeEffects.cs`
- `Assets/Prefabs/UI/TankIcon.prefab`
- `Assets/Scenes/BaseScene.unity`

## 검증 내용

- Play 모드에서 바닥순서 소비, 투명 해소, 중복 소비 방지, 2번 릴 동시 감정, 속도 가속을 육안 확인
- 스킬트리: `robot_core → 감정 속도 → 감정 릴 증설` 순 잠금 해제 확인

## AI 활용 범위

- AI(에이전트): C# 코드 구현 및 원인 분석
- 사용자: 스킬트리 노드·커넥션·2번 릴 UI 등 인스펙터 배선, Play 모드 검증, 밸런스(속도 1.3배) 결정

## 후속 작업

- 감정 패널 UI 다듬기
- 업그레이드/탐험 이펙트 연결
