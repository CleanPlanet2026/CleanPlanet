# PR #8: 감정로봇 시스템 (수집물 감정 · 재화 지급 · 티어 이펙트)

## 개요

- 브랜치: `feature/emotion-robot`
- PR: #8
- 작성일: 2026-08-02
- 작업 목적: 감정 결과(payout)를 실제 재화로 지급하고, 지급 규모에 따라 코인·사운드·화면 연출을 티어별로 재생하는 이펙트 시스템을 추가한다. (감정 코어·릴·유리관 등 선행 작업이 함께 포함된 브랜치)

## 주요 요청

1. 감정 결과에 맞게 재화가 나오고, 재화 이펙트와 인터페이스에 적용되게 한다.
2. 지급액 크기에 따라 코인이 더 많이 터지는 등 규모감이 느껴져야 한다.
3. 지급액 구간을 티어로 나누고, 각 티어별로 코인 수·사운드·화면 연출을 다르게 한다.
4. 코인이 많은 티어는 "펑 터진 뒤 분수처럼 상승"하는 연출로 바꾼다.
5. 각 티어 이펙트를 즉시 확인할 수 있는 디버그 기능을 넣는다.

## AI와 논의한 내용

### 지급액 티어 구간

- 수집물 기본가치(고철 10·유리 40·전자 150·보석 600)와 배수 확률표(x1~x16)를 근거로 실제 payout 분포(10~9,600)를 산출했다.
- 티어 개수와 경계값을 여러 차례 조정한 끝에, 6단계(푼돈/쏠쏠/두둑/대박/초대박/잭팟)로 확정했다.

### 코인 연출 방식

- 초기 선형 계산(payout÷N)이 규모감을 못 살려, 티어별 코인 수 지정 방식으로 전환했다.
- 폭발 연출을 "다 같이 터진 뒤 코인마다 상승 시차를 두어 분수처럼 올라가는" 형태로 정했고, 코인이 들어오는 시간을 티어 오디오 길이에 맞췄다.
- 코인 수가 많을수록 폭발 반경이 넓어지도록 개수에 비례한 반경 스케일을 적용했다.

### 티어 데이터 관리 구조

- 티어 데이터가 씬에 직렬화되어 값 변경 시 반복 재설정이 필요한 문제가 있었다.
- 이를 `AppraisalTierTable` ScriptableObject 애셋으로 분리해, 밸런스 조정이 애셋 수정만으로 끝나도록 리팩터했다.

## 주요 결정

- 티어는 6단계, 경계값 100 / 500 / 1,000 / 3,000 / 7,000 (사용자 확정)
- 티어별 코인 수 2 / 6 / 20 / 60 / 150 / 400, 상위 4개 티어는 폭발 연출 (사용자 확정)
- 사운드 매핑: 코인음(길이별)·good·jackpot을 티어별로 배정 (사용자 확정)
- UI 텍스트는 프로젝트 관례에 따라 legacy `UnityEngine.UI.Text` 사용 (사용자 확정)
- 디버그는 지갑에 표본 payout을 주입해 실제 파이프라인으로 이펙트를 재생하는 방식 (사용자 확정)
- 티어 데이터를 ScriptableObject 애셋으로 분리 (구현 과정에서 확정)

## 변경된 주요 파일

- `Assets/Scripts/Core/Currency/CurrencyWallet.cs`
- `Assets/Scripts/Core/Appraisal/AppraisalDriver.cs`, `AppraisalTank.cs`
- `Assets/Scripts/UI/AppraisalTier.cs`, `AppraisalTierTable.cs`, `AppraisalEffectDirector.cs`
- `Assets/Scripts/UI/CoinBurst.cs`, `CoinBurstSpawner.cs`, `CurrencyHudView.cs`
- `Assets/Scripts/UI/ScreenFlash.cs`, `TierBanner.cs`, `AppraisalDebugPanel.cs`
- `Assets/Scripts/Utils/CameraShake.cs`
- `Assets/ScriptableObjects/Appraisal/AppraisalTierTable.asset`
- `Assets/Prefabs/UI/CoinBurst.prefab`, `Assets/Art/Sprites/coin.png`, `Assets/Audio/SFX/*`
- `Assets/Scenes/BaseScene.unity`

## 검증 내용

- Unity 에디터 컴파일 무에러 확인
- 디버그 패널로 티어별 코인 수·사운드·화면흔들림·플래시·배너 재생 확인
- 실제 감정 흐름(유리관 → 릴 → 지급 → 이펙트) 동작 확인

## AI 활용 범위

- AI가 지원: 기존 코드 구조 파악, 티어/이펙트 시스템의 C# 구현, ScriptableObject 리팩터, 프롬프트 정리 및 구현 에이전트 실행 관리, 에셋(스프라이트·오디오) 임포트 설정.
- 사용자가 직접 수행: 씬·프리팹 배선, 티어 밸런스 값과 경계·코인 수 확정, 사운드 파일 준비, 에디터 Play 검증.

## 후속 작업

- 이번 PR 범위에 포함하지 않음: 업그레이드 시스템으로의 재화 소비 연동, 세이브/로드, 정식 밸런스 튜닝.
