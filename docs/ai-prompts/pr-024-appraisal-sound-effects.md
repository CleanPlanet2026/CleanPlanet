# PR #24: 감정 사운드·연출 강화와 씬 독립 감정 서비스

## 개요

- 브랜치: `feature/appraisal-sound-effects`
- PR: #24
- 작성일: 2026-08-07
- 작업 목적: 감정 사운드·연출을 강화하고, 감정 로직을 씬에 종속되지 않는 영속 서비스로 분리한다.

## 주요 요청

1. 감정 릴에 회전 틱 사운드와 정착 클랭크 사운드를 넣는다.
2. 티어별 사운드를 재배치한다.
3. 업그레이드 구매음과 감정 입장음을 추가한다.
4. 다른 씬에 있어도 감정이 자동 진행되어 골드가 쌓이게 한다.
5. 유리관 에셋과 로봇·쓰레기 스프라이트를 추가한다.
6. 관 안에서 쌓이고 소비되는 연출을 다듬는다.

## AI와 논의한 내용

### 감정 시스템 씬 독립화

베이스 씬 UI에 묶여 있던 감정 계산·지급을 영속 서비스로 분리했다.
릴·탱크·이펙트는 서비스 이벤트를 구독하는 표시 전담 뷰로 전환했다.
카탈로그는 Preloaded Assets로, 골드는 정적 경로로 씬 없이 지급되게 했다.

### 유리관 연출

위 스폰 지점에서 시간차로 떨어뜨려 물리로 쌓게 했다.
아이콘 충돌체를 원에서 박스로 바꿔 촘촘히 붙게 했다.
감정 시 아이콘이 아래로 미끄러지며 옅어지는 소비 연출을 넣었다.
전자 부품 아이콘이 투명하던 것은 스프라이트가 조각으로 잘려 파편을 참조한 탓이라 통짜 스프라이트로 수정했다.

## 주요 결정

- 감정 진행 페이스는 `AppraisalConfig`가 소유한다.
- 릴 스핀 시간은 뷰가 소유해 페이스와 분리한다.
- 지급이 서비스로 이동해 `AppraisalCore`와 `AppraisalDriver`는 제거한다.
- 고철음을 4티어로 옮긴다.
- 5티어에 새 사운드를 넣는다.
- 5·6티어에 박수 사운드를 넣되, 6티어 박수는 제거한다.
- 업그레이드음은 구매 성공 시에만 재생한다.
- 감정 입장음은 화면이 보일 때만 재생한다.
- 파이프에서 아이콘을 빼는 시점을 감정 시작에 맞춘다.

## 변경된 주요 파일

- `Assets/Scripts/Core/Appraisal/AppraisalService.cs`
- `Assets/Scripts/Core/Appraisal/AppraisalConfig.cs`
- `Assets/Scripts/Core/Appraisal/AppraisalTank.cs`
- `Assets/Scripts/Core/Appraisal/AppraisalTankIcon.cs`
- `Assets/Scripts/Core/Appraisal/AppraisalReelSequencer.cs`
- `Assets/Scripts/Core/Appraisal/AppraisalReel.cs`
- `Assets/Scripts/Core/Appraisal/AppraisalDisplay.cs`
- `Assets/Scripts/Core/Currency/CurrencyWallet.cs`
- `Assets/Scripts/Core/Collection/CollectionInbox.cs`
- `Assets/Scripts/UI/AppraisalEffectDirector.cs`
- `Assets/Scripts/UI/AppraisalTier.cs`
- `Assets/Scripts/UI/SkillTreeDetailController.cs`
- `Assets/Scripts/UI/CollectibleInboxDebugButton.cs`
- `Assets/ScriptableObjects/Appraisal/AppraisalTierTable.asset`
- `Assets/ScriptableObjects/Appraisal/AppraisalConfig.asset`
- `Assets/Scenes/BaseScene.unity`
- `ProjectSettings/ProjectSettings.asset`
- `Assets/Prefabs/UI/TankIcon.prefab`
- `Assets/Audio/SFX/tick.mp3`
- `Assets/Audio/SFX/clank.mp3`
- `Assets/Audio/SFX/great.mp3`
- `Assets/Audio/SFX/applause.mp3`
- `Assets/Audio/SFX/upgrade.mp3`
- `Assets/Audio/SFX/input.mp3`
- `Assets/Art/Sprites/Appraisal/pipe.png`
- `Assets/Art/Sprites/UI/factory.png`
- `Assets/Art/Sprites/Items/electric/item_electric_1.png`
- `Assets/Art/Sprites/Robot/`
- `Assets/Art/Sprites/Trashpiles/`

## 검증 내용

- Unity `6000.5.6f1`에서 스크립트 컴파일 오류 0개를 확인했다.
- 감정 씬 독립 리팩토링 후 빌드 성공을 확인했다.
- 전자 부품 아이콘이 통짜 스프라이트를 참조하도록 정적 확인했다.
- 씬 독립 진행, 화면 전환, 각종 사운드, 유리관 연출은 사용자가 Play 모드에서 확인했다.

## AI 활용 범위

AI는 감정 시스템 구조 분석과 서비스 분리 설계·구현을 담당했다.
AI는 사운드·연출 코드 작성과 에셋 임포트·스프라이트 수정, 버그 진단을 수행했다.
사용자는 사운드 배치와 티어 구성, 6티어 박수 제거, 연출 방향을 결정했다.
사용자는 씬 인스펙터 배선과 Play 모드 최종 확인을 수행했다.

## 후속 작업

### 미해결 버그

- Play 모드를 껐다 켤 때마다 유리관 아이콘 일부가 투명하게 표시된다.
- 스프라이트 데이터 문제는 아님을 확인했다(전자 부품만 조각 참조였고 수정 완료).
- 런타임 원인이 미확정이다.
- 확인 항목 1: 떨어지는 순간부터 투명한지.
- 확인 항목 2: 감정 소비 시에만 반짝 투명해지는지.
- 확인 항목 3: 감정 종료 후에도 투명한 채 자리를 차지하는지.
- 확인 항목 4: 투명한 대상이 반투명 아트인 유리 종류인지.

### 남은 기능 작업

- 감정 부분 UI 개선.
- 탐험 부분 에셋 연결.
- 탐험 부분 이펙트 연결.
- 업그레이드 부분 이펙트 연결.

### 밸런스·연출 다듬기

- 수집물이 과도하게 쌓일 때의 처리 방법을 정한다.
- 씬 재진입 시 낙하 없이 이미 쌓인 상태 그대로 유지되게 한다.
