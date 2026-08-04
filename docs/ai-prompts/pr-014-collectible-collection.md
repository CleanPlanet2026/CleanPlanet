# PR #14: 쓰레기 더미 수집 시스템 및 원형 QTE 구현

## 개요

- 브랜치: `feature/collectible-collection`
- PR: #14
- 작성일: 2026-08-05
- 작업 목적: 그리드 이동(PR #11, #12)과 감정/재화 시스템(PR #8) 사이에 비어있던 "쓰레기 수집" 루프를 채운다. 플레이어가 맵 위 쓰레기 더미를 클릭 → 로봇 자동 이동 → QTE 판정 → 수집물 차등 지급까지 이어지는 핵심 게임플레이를 구현한다.

## 주요 요청

1. 쓰레기 더미 생성(랜덤 셀, 종류별 스폰 빈도·드롭 성향), 선택, 자동 이동, 감지, QTE, 이벤트 기반 보상 지급까지 전체 시스템 구현.
2. QTE는 처음엔 타이밍 바 → 원형 반복 방식을 거쳐, 최종적으로 Dead by Daylight의 Skill Check와 동일하게 동작하도록 재작성(Needle 단일 회전, Success/Great Zone 매 판 랜덤 배치, Space 단발 입력 판정, Update 기반/GC Alloc 없음/LINQ 미사용).
3. 더미 종류별로 프리팹과 드롭 테이블을 분리(가중치 기반 추첨).

## AI와 논의한 내용

### 기존 시스템과의 통합 지점

`TargetCellSelector`(PR #6에서 "더미" 개념으로 이미 설계됨)를 더미 인접 셀 선택에 그대로 재사용했다. `PlayerClickToMove`는 클릭 셀의 점유자가 `TrashPile`이고 이동이 실제로 시작됐을 때만 `OnTrashSelected`를 발행하도록 최소 범위로만 확장했다.

### 시스템 간 결합도

`QteController`는 `TrashPile`을 전혀 모르고, `TrashPile`도 QTE를 모른다. `TrashInteractionController`가 둘 사이의 유일한 연결점으로, 선택→도착→QTE 시작→보상 지급/제거를 이벤트로만 조율한다.

### QTE UI 시행착오와 원인 분석

원형 UI를 구현하는 과정에서 여러 문제를 순차적으로 발견하고 고쳤다.

- Unity UI `Image` 스크립트 참조를 손으로 잘못 작성해(`fileID` 오기입) 컴포넌트가 깨진 문제 — 프로젝트 내 기존 Image 사용 사례에서 정확한 값을 확인해 수정.
- `QteCanvas`의 RectTransform `m_LocalScale`이 `{0,0,0}`으로 되어 있어 자식 전체가 렌더링되지 않던 문제.
- `QteUI` 스크립트가 자신이 붙어있는 GameObject를 `_root`로 지정해, `OnEnable`에서 그 오브젝트를 비활성화하는 순간 자기 자신도 함께 비활성화되어 이벤트 구독이 즉시 해제되던 문제 — `QteUI`를 별도의 상시 활성 오브젝트(`QTEController`)로 옮겨 해결.
- Success/Great Zone을 `SpriteRenderer`로 제작했다가 `Image`로 되돌리는 과정에서, 각 Zone이 자기 자신의 중심을 축으로 회전해 링 중심을 도는 게 아니라 엉뚱한 지점을 맴도는 문제 — Needle/SuccessZone/GreatZone마다 링 중심에 위치한 빈 RectTransform(Pivot)을 만들고, 이미지가 아닌 그 Pivot을 회전시키는 구조로 변경해 이미지 자신의 Pivot·스프라이트 모양과 무관하게 항상 정확한 중심을 축으로 돌도록 만들었다.
- 스프라이트의 기본 방향이 "위쪽=0도" 기준과 다를 수 있어, 보정용 회전 오프셋 필드를 추가했다.

## 주요 결정

- `TrashPileType`(ScriptableObject)로 더미 종류별 스폰 가중치와 드롭 테이블을 관리하고, `WeightedRandom` 공용 유틸로 추첨 로직을 재사용한다.
- QTE 판정 구간(Success/Great)은 매 `StartQte()` 호출마다 새로 랜덤 배치한다.
- QTE는 코루틴이 아닌 `Update()` 기반으로 구현하고, `Random.Range` 호출은 판정 시작 시 한 번만 발생해 GC Alloc이 없도록 한다.
- QTE UI의 회전축 문제는 "이미지 자체의 Pivot을 맞추는 방식" 대신 "빈 Pivot 부모를 두고 그것을 회전시키는 방식"으로 구조를 바꿔 근본적으로 해결한다. Ring(배경)은 정적으로 유지하고 회전하지 않는다.
- QTE 원형 UI의 최종 시각적 마무리(구간 표시 확인, 회전 보정값 튜닝)는 이번 PR 범위에서 제외하고 후속 작업으로 넘긴다.

## 변경된 주요 파일

- `Assets/Scripts/Trash/TrashPile.cs` (신규) — 셀 점유, 보상 계산, 제거
- `Assets/Scripts/Trash/TrashPileType.cs` (신규) — 더미 종류 ScriptableObject
- `Assets/Scripts/Trash/TrashSpawner.cs` (신규) — 가중치 기반 스폰
- `Assets/Scripts/Trash/TrashInteractionController.cs` (신규) — 선택→도착→QTE→보상 조율
- `Assets/Scripts/Trash/QteController.cs` (신규) — 원형 QTE 판정 로직
- `Assets/Scripts/Trash/QteUI.cs` (신규) — 원형 QTE 시각화
- `Assets/Scripts/Utils/WeightedRandom.cs` (신규) — 가중치 랜덤 추첨 공용 유틸
- `Assets/Scripts/Player/PlayerClickToMove.cs` (수정) — TrashPile 클릭 감지 및 `OnTrashSelected` 발행 추가
- `Assets/Scenes/GameScene.unity` — TrashSpawner/TrashInteractionController/QteController/QTE UI 계층 배선
- `Assets/Prefabs/Trash/GeneralTrashPile.prefab`, `GlassTrashPile.prefab` (신규)
- `Assets/ScriptableObjects/TrashPile/GeneralTrashPile.asset`, `GlassTrashPile.asset` (신규)

## 검증 내용

- 진단 로그(`TrashInteractionController`)로 더미 선택 → 도착 → QTE 시작까지 실제 Play Mode에서 정상 동작하는 것을 확인했다.
- QTE 판정 로직(원형 반복 → Dead by Daylight 방식 재작성) 자체는 코드 리뷰 수준으로 점검했으나, 최종 버전의 Play Mode 판정 테스트(성공/대성공/실패 각각)는 아직 진행하지 못했다.
- QTE 원형 UI는 여러 차례 문제를 발견·수정했지만, 최종 시각적 확인(구간이 실제로 정확한 위치에 보이는지)은 아직 완료되지 않았다.

## AI 활용 범위

AI는 전체 아키텍처 설계(이벤트 기반 시스템 분리), `TrashPile`/`TrashSpawner`/`TrashPileType`/`TrashInteractionController`/`QteController`/`QteUI` 구현, 여러 차례의 QTE UI 버그 진단 및 수정(Unity 씬 YAML 직접 편집 포함), 커밋 및 PR 생성을 지원했다. QTE 메커니즘 자체(원형 반복 → Dead by Daylight 방식)는 사용자가 상세 스펙 문서로 요구사항을 제시했고, AI는 그 스펙에 맞춰 구현했다. Zone 회전 구조(이미지 자체 회전 vs 빈 Pivot 부모 회전) 등 설계 선택은 AI가 제시한 대안을 사용자가 직접 결정했다. 씬의 UI 계층(Canvas/RectTransform/Image)은 AI가 YAML을 직접 작성했으나 시각적 검증은 어디까지나 사용자가 Unity 에디터에서 수행했다.

## 후속 작업

- QTE 원형 UI 최종 시각적 마무리 (구간 표시, 회전 보정값 튜닝, Play Mode 최종 확인)
- `TrashPileType` 나머지 종류(현재 General/Glass 2종만 존재, 계획은 4종) 및 대응 프리팹 추가
- QTE 실제 판정(성공/대성공/실패) Play Mode 테스트
- 수집물 보상을 기존 Appraisal Tank 파이프라인과 연동할지 여부 (PR #11 이후 미결정 상태로 남아있음)
