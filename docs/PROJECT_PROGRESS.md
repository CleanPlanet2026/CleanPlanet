# CleanPlanet 프로젝트 진행 현황

> 기준: 2026-08-06, `origin/main` (`929a810`)
>
> 저장소의 코드, 씬, 에셋, Build Settings, GitHub Actions와 병합된 PR을 기준으로 정리한 스냅샷이다. 아직 병합되지 않은 작업은 별도로 표시한다.

## 한눈에 보기

현재 프로젝트는 `타이틀 → 탐험 → 쓰레기 수집/QTE → 베이스 복귀 → 수집물 감정 → 골드 획득 → 업그레이드 구매 → 재탐험`으로 이어지는 기본 게임 루프가 연결된 플레이 가능한 프로토타입이다. 세 씬이 빌드에 등록되어 있고, GitHub Actions가 `main`의 WebGL 빌드와 GitHub Pages 배포를 자동화한다.

핵심 루프는 동작하지만 성장 효과가 실제 탐험 능력치에 적용되지는 않으며, 골드·수집물·배터리·업그레이드 상태는 영구 저장되지 않는다. 자동화 테스트도 아직 없다. 현재 별도 PR에서는 WebGL 한글 폰트 누락을 수정하고 있다.

## 개발 및 배포 환경

- Unity `6000.5.6f1`
- Universal Render Pipeline `17.6.0`
- Input System `1.20.0`
- 2D 탑다운 방치형·증분형 게임
- Build Settings 등록 순서: `TitleScene`, `BaseScene`, `GameScene`
- WebGL 빌드: GitHub Actions의 `Build and deploy WebGL`
- 배포: `main` push 또는 수동 실행 시 GitHub Pages에 배포
- 라이선스: Unity Personal 정보를 GitHub Actions Secrets로 주입
- WebGL Decompression Fallback 활성화

## 전체 플레이 흐름

1. `TitleScene`에서 **시작하기** 버튼을 누르면 `GameScene`을 불러온다.
2. `GameScene`에서 로봇이 그리드를 이동하고 쓰레기 더미에 접근한다.
3. 원형 QTE 결과에 따라 수집물을 획득하고 `CollectionInbox`에 보관한다.
4. 탐험 종료 버튼을 3초간 누르거나 `R` 키를 누르면 `BaseScene`으로 이동한다.
5. 배터리가 모두 소진된 경우 경고를 표시한 뒤 자동으로 `BaseScene`으로 이동한다.
6. `BaseScene`의 감정 탱크가 미감정 수집물을 받아 감정하고 골드를 지급한다.
7. 업그레이드 탭에서 골드를 사용해 구매 가능한 노드를 강화할 수 있다.
8. 배터리가 기준치까지 충전되면 탐험 시작 버튼 또는 `R` 키로 `GameScene`에 재진입한다.

## 구현 현황

| 영역 | 상태 | 현재 구현 | 남은 핵심 작업 |
| --- | --- | --- | --- |
| 타이틀 | 완료 | 게임 제목, 시작 버튼, `GameScene` 전환 | 최종 아트·연출 보강 |
| 그리드 맵 | 프로토타입 완료 | 좌표 변환, 셀 점유, 목표 셀 선택, 4방향 최단 경로 | 테스트용 부트스트랩 정리 |
| 로봇 이동 | 프로토타입 완료 | 클릭 이동, 장애물 인접 셀 보정, 이동 중 재탐색 | 실패 피드백과 조작 감각 개선 |
| 쓰레기 생성·수집 | 기본 루프 완료 | 4종 더미 스폰, 접근, 원형 QTE, 결과별 보상, 더미 제거 | 스폰 규칙·드랍률·QTE 밸런스 조정 |
| 수집물 전달 | 완료 | 씬 전환 간 미감정 수집물 유지, 감정 탱크 병합, 획득 팝업 | 인벤토리 조회·관리 UI |
| 감정 로봇 | 기본 루프 완료 | 수집물 투입, 가중치 배수, 릴 연출, 지급 확정 | 밸런스 및 연출 마무리 |
| 재화 | 런타임 완료 | 감정 보상 지급, HUD, 업그레이드 비용 차감 | 영구 저장과 경제 밸런스 |
| 감정 이펙트 | 시연 가능 | 6개 티어, 코인·사운드·배너·플래시·카메라 흔들림 | 고빈도 상황 성능 측정 |
| 배터리 탐험 루프 | 완료 | 탐험 중 소모, 베이스 충전, HUD, 저전력 복귀, 출발 제한 | 수치 밸런스와 업그레이드 연동 |
| 베이스 UI | 프로토타입 완료 | 감정/업그레이드 탭, 배터리·골드 HUD | 최종 레이아웃·반응형 점검 |
| 업그레이드 | 런타임 구매 완료 | 16개 노드, 상세 정보, 선행 조건, 골드 차감, 레벨 상태 | 실제 능력치 효과 적용, 데이터 분리, 영구 저장 |
| WebGL CI/CD | 구축 완료 | GitHub 호스팅 러너 빌드, Pages artifact 업로드·배포 | 배포 회귀 확인과 빌드 시간 최적화 |
| 한글 WebGL 폰트 | 진행 중 | 타이틀·베이스 화면의 한글 지원 폰트 교체 작업 | 관련 PR 병합 후 실브라우저 재검증 |
| 저장/불러오기 | 미구현 | 플레이 세션 동안만 상태 유지 | 재화·수집물·배터리·업그레이드 영속화 |
| 자동화 테스트 | 미구축 | 그리드 순수 로직의 수동 자체 테스트 메서드 | Edit Mode·Play Mode 테스트와 CI 연동 |

## 시스템별 상세

### 그리드, 이동과 쓰레기 수집

- `GridSystem`이 월드 좌표와 셀 인덱스 변환 및 범위 검사를 담당한다.
- `GridOccupancy`와 `GridOccupant`가 셀 점유를 관리한다.
- `TargetCellSelector`가 점유 대상을 클릭했을 때 접근 가능한 빈 인접 셀을 고른다.
- `GridPathfinder`가 점유 셀을 피해 4방향 최단 경로를 계산한다.
- `PlayerMovement`와 `PlayerClickToMove`가 클릭 목적지 이동과 경로 재탐색을 처리한다.
- `TrashSpawner`가 일반·유리·전자·보석 4종 더미를 빈 셀에 생성한다.
- `TrashInteractionController`가 더미 선택, 로봇 도착, QTE, 보상 지급과 더미 제거를 이벤트로 연결한다.
- QTE 중에는 이동 입력을 잠그며 UI 클릭이 그리드 이동으로 전달되지 않도록 차단한다.
- `TrashPileType`은 자기 등급 수집물과 이전 등급 참조를 사용해 하위 등급 드랍을 계승한다.

### 수집물, 감정과 재화

- `CollectibleData` ScriptableObject 16개가 Common·Uncommon·Rare·Epic 등급으로 구성되어 있다.
- `TrashCollectionRelay`가 수집 보상을 정적 `CollectionInbox`에 적립한다.
- `AppraisalTank`가 `BaseScene` 진입 시 대기 수집물을 가져오고, 실제 투입된 항목만 인박스에서 제거한다.
- `AppraisalCore`가 가중치 기반으로 `x1`, `x2`, `x4`, `x8`, `x16` 배수를 결정한다.
- `AppraisalDriver`가 확정 지급액을 정적 골드 값을 사용하는 `CurrencyWallet`에 더한다.
- 감정 결과는 6개 티어로 분류되며 코인, 사운드, 배너, 플래시와 카메라 흔들림이 달라진다.
- 감정 탱크 아이콘은 시작 시 생성한 풀을 재사용하지만 코인과 획득 팝업은 런타임에 생성한다.

### 배터리와 씬 전환

- `RobotBattery`가 씬 사이에서 공유되는 배터리 잔량을 관리한다.
- `BatteryDrainer`는 탐험 중 잔량을 소모하고 `BatteryCharger`는 베이스에서 충전한다.
- `ExplorationReturnTrigger`와 `LowBatteryWarning`이 수동·자동 베이스 복귀를 담당한다.
- `ExplorationLauncher`는 기본적으로 완전 충전된 경우에만 재탐험을 허용한다.
- `HoldToConfirmButton`은 탐험 시작·종료 시 3초 홀드를 요구한다.
- `PointerGate`가 씬 전환 전후에 이어진 포인터 입력으로 버튼이나 이동이 중복 실행되는 것을 막는다.

### 베이스 화면과 업그레이드

- `BaseViewController`가 감정 로봇과 업그레이드 패널을 전환한다.
- 배터리 HUD는 두 전환 탭의 오른쪽에 배치되어 있다.
- 스킬 트리는 드래그 이동, 확대·축소, 화면 맞춤과 초기화를 지원한다.
- 16개 노드에 상태, 설명, 현재·다음 효과, 비용과 선행 조건이 직렬화되어 있다.
- `SkillTreeDetailController`가 구매 가능 여부를 계산하고 `CurrencyWallet.TrySpend`로 비용을 차감한다.
- `UpgradeRuntimeState`는 현재 씬에 살아 있는 컨트롤러 내부 상태이므로 `BaseScene`을 다시 로드하면 초기화된다.
- 표시된 업그레이드 효과는 아직 로봇 이동, 배터리, 수집 또는 감정 수치에 반영되지 않는다.

## 상태 유지 범위

| 데이터 | 씬 전환 간 유지 | 게임 재시작 후 유지 | 비고 |
| --- | --- | --- | --- |
| 미감정 수집물 | 예 | 아니요 | 정적 `CollectionInbox` |
| 골드 | 예 | 아니요 | `CurrencyWallet`의 정적 값 |
| 배터리 | 예 | 아니요 | 정적 `RobotBattery` |
| 업그레이드 레벨 | 아니요 | 아니요 | `BaseScene` 컨트롤러의 런타임 객체 |

현재 `PlayerPrefs`, 파일 또는 서버를 사용하는 저장 시스템은 없다.

## 씬과 콘텐츠 상태

### `TitleScene`

- Build Index 0의 시작 씬
- `Clean Planet` 타이틀과 시작하기 버튼
- 버튼 선택 시 `GameScene` 직접 로드

### `GameScene`

- 그리드, 로봇 클릭 이동, 4종 쓰레기 더미, 원형 QTE
- 수집물 획득 팝업과 배터리 HUD
- 탐험 종료 홀드 버튼과 저전력 경고
- 수동 또는 배터리 소진 시 `BaseScene` 복귀

### `BaseScene`

- 감정 탱크와 릴 UI, 감정 결과 이펙트
- 감정/업그레이드 탭과 16노드 스킬 트리
- 골드·배터리 HUD와 배터리 충전
- 완전 충전 후 재탐험 홀드 버튼

## 빌드와 배포

- `.github/workflows/deploy-webgl.yml`이 `main` push와 `workflow_dispatch`에서 실행된다.
- `game-ci/unity-builder@v4`가 Ubuntu GitHub 호스팅 러너에서 WebGL을 빌드한다.
- Unity 라이선스·이메일·비밀번호는 저장소가 아닌 GitHub Actions Secrets를 사용한다.
- 빌드 결과 `build/WebGL/WebGL`을 GitHub Pages artifact로 업로드하고 배포한다.
- Unity `Library` 캐시를 사용해 후속 빌드 시간을 줄인다.
- 실제 워크플로 완료와 배포 페이지 접근은 사용자 확인 기록이 있으나, 저장소 내부 자동 테스트는 실행하지 않는다.

## 확인된 검증 기록

- 그리드 좌표 변환 자체 테스트 10/10, 목표 셀 선택 5/5, 경로 탐색 5/5 통과 기록
- Play Mode에서 이동, 점유 갱신과 경로 재탐색 확인 기록
- Play Mode에서 쓰레기 선택 → 이동 → QTE → 보상 → 팝업 흐름 확인 기록
- Play Mode에서 탐험 종료/배터리 소진 → 베이스 → 충전 → 재탐험 흐름 확인 기록
- 감정 → 골드 지급 → 티어 이펙트와 업그레이드 구매 흐름 구현 및 코드 배선 확인
- GitHub Actions WebGL 빌드 및 GitHub Pages 배포 완료에 대한 사용자 확인 기록

위 결과는 수동 검증과 기존 PR 기록을 기반으로 한다. Unity Test Framework용 독립 테스트 어셈블리와 CI 테스트 잡은 없다.

## 주요 기술 부채와 위험

1. 업그레이드가 표시와 구매까지만 연결되어 실제 게임 능력치에 영향을 주지 않는다.
2. 업그레이드 상태는 `BaseScene` 재진입 시 초기화되지만 차감한 골드는 유지되어 플레이 흐름상 불일치가 생긴다.
3. 모든 주요 진행 상태가 메모리에만 있어 브라우저 새로고침이나 게임 재시작 시 사라진다.
4. 핵심 루프의 회귀를 잡는 자동화 테스트가 없고 검증이 수동 플레이에 의존한다.
5. `GridOccupancyTester`가 런타임 구성까지 담당해 테스트 도구와 실제 부트스트랩의 책임이 섞여 있다.
6. WebGL에서 기본 폰트의 한글 글리프가 누락되는 문제가 있으며 수정 PR이 아직 `main`에 병합되지 않았다.
7. 코인·수집 팝업처럼 런타임 생성되는 UI는 플레이 빈도가 높아질 때 프로파일링이 필요하다.

## 권장 다음 작업 순서

1. WebGL 한글 폰트 수정 PR을 병합하고 타이틀·베이스·게임 화면의 모든 한글을 브라우저에서 재검증한다.
2. 업그레이드 레벨을 씬 전환 간 유지하고 각 효과를 이동·배터리·수집·감정 수치에 연결한다.
3. 골드, 미감정 수집물, 배터리와 업그레이드 진행도를 저장·불러오기 한다.
4. 타이틀부터 재탐험까지 핵심 루프를 Play Mode 자동화 테스트로 추가한다.
5. 그리드 순수 로직을 Edit Mode 테스트로 옮기고 WebGL 워크플로에 테스트 잡을 추가한다.
6. `GridOccupancyTester`에서 정식 게임 초기화 책임을 분리한다.
7. 실제 플레이 데이터를 바탕으로 배터리, 드랍률, QTE와 경제 밸런스를 조정한다.
8. 최종 아트·사운드·반응형 UI와 WebGL 성능을 다듬는다.

## 관련 문서

- `AGENTS.md`: 프로젝트 구조와 개발 원칙
- `GIT_WORKFLOW.md`: 브랜치, 커밋과 PR 규칙
- `docs/ai-prompts/pr-014-collectible-collection.md`: 쓰레기 수집과 원형 QTE
- `docs/ai-prompts/pr-015-collection-battery-loop.md`: 수집물 감정 연동과 배터리 탐험 루프
- `docs/ai-prompts/pr-016-connect-game-loop.md`: 타이틀과 전체 게임 루프 연결
- `docs/ai-prompts/pr-017-webgl-pages-deploy.md`: WebGL 빌드 및 Pages 배포
- PR #18: WebGL 한글 폰트 수정 작업(병합 전)
