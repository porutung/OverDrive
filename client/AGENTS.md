# Repository Guidelines

## Project Structure & Module Organization
- Unity version is locked to 6000.2.7f2 (`ProjectSettings/ProjectVersion.txt`); open scenes with **InitScene**, **MainScene**, or **UIEnvironment** under `Assets/Scenes`.
- Core scripts live in `Assets/Scripts` subdivided into `GamePlay`, `UI`, `AssetsLoader`, and `Util`; align new assemblies with these folders.
- Shared assets reside in `Assets/Bundles`, while Addressables lives under `Assets/AddressableAssetsData`. Avoid editing anything in `Library/` or `Temp/`, as Unity regenerates those.

## Build, Test, and Development Commands
- `unity-editor -projectPath "$(pwd)"` launches the project in the installed editor.
- `unity-editor -batchmode -projectPath "$(pwd)" -runTests -testPlatform PlayMode -testResults Logs/PlayModeResults.xml` runs play mode tests headlessly; swap `PlayMode` for `EditMode` to cover editor scripts.
- Rebuild Addressables via Unity: `Window > Asset Management > Addressables > Build > New Build` before shipping scene or prefab updates.

## Coding Style & Naming Conventions
- Follow Unity C# style: Allman braces, four-space indentation, and PascalCase for public APIs. Private or serialized fields use `_camelCase` (`[SerializeField] private CanvasManager _canvasManager;`).
- Prefer `nameof(...)` in logs and event IDs, and keep MonoBehaviours lean by delegating to services (use `ServiceLocator` where appropriate).
- Keep comments concise; document complex behaviours with XML doc comments to match existing patterns.

## Testing Guidelines
- Use the Unity Test Framework. Place edit mode suites in `Assets/Tests/EditMode` and play mode suites in `Assets/Tests/PlayMode`.
- Name fixtures `FeatureNameTests.cs` and keep assertions deterministic, especially for UI controllers in `Assets/Scripts/UI/System`.
- Store headless test outputs in `Logs/` for CI pipelines; regenerate when gameplay or UI logic changes.

## Commit & Pull Request Guidelines
- Write imperative commit subjects, often in Korean (e.g., `패키지 파일 업데이트`), and split unrelated work across multiple commits.
- Pull requests should summarize changes, link Jira or GitHub issues, include validation or reproduction steps, and attach screenshots/GIFs for UI.
- Before requesting review, confirm play mode smoke tests, rebuild Addressables if assets changed, and ensure bundle sources under `Assets/Bundles/...` remain in sync.

## Asset & Addressables Tips
- Store new UI prefabs in `Assets/Bundles/Prefab/UI` and use Git LFS for large binaries (check `.gitattributes` before committing).
- Update Addressable groups through the Addressables window and verify label assignments align with scene loading expectations.

## 프로젝트 분석 및 확장 제안

### 구조 요약
- 씬은 `Assets/Scenes`에 `InitScene`, `MainScene`, `UIEnvironment` 등이 존재하며, 메인 게임 흐름은 `MainScene`에서 구성됩니다.
- 게임 플레이 스크립트는 `Assets/Scripts/GamePlay`에 모여 있으며, `PlayerCarController.cs`, `PatternSpawner.cs`, `AdvancedChaseCamera.cs`, `RoadScroller.cs` 등이 핵심 루프를 담당합니다.
- 차량 스펙과 장애물 패턴은 각각 `CarStats`, `ObstaclePattern` ScriptableObject(`Assets/Scripts/GamePlay/Scriptable`)로 분리돼 데이터 기반 설계가 가능합니다.
- UI는 `Assets/Scripts/UI` 아래 MVVM 패턴으로 구성됐으며, `ServiceLocator.cs`와 `UIManager.cs`가 페이지·팝업 로딩을 담당하고 `PlayerInfoView`가 속도/콤보/연료/Nitro 정보를 표시합니다.
- 이펙트 관련 스크립트(`SpeedLineEffect.cs`, `SlipsstreamEffect.cs`)와 Input 시스템(`InputHandler.cs`, `MoveCommand.cs`)이 독립적으로 유지돼 후속 확장에 우호적인 구조입니다.

### 현재 구현된 핵심 시스템
- `PlayerCarController`는 차선 이동, 슬립스트림 감지, 아슬아슬 회피 부스트, Nitro 콤보, 연료 소모/보급, 충돌 반응까지 통합 관리합니다.
- `PatternSpawner`는 `ObstaclePattern` 리스트를 순환하며 NPC 차량을 소환하고, 일정 간격마다 `FuelController` 프리팹을 등장시켜 생존 루프를 형성합니다.
- `AdvancedChaseCamera`와 속도선/모션블러는 부스트·Nitro 상태에서 속도감을 강화하며, 카메라 흔들림으로 충돌 피드백을 제공합니다.
- UI 레이어가 Service Locator를 통해 느슨하게 연결돼 있어 향후 HUD 확장이나 새 페이지 추가가 비교적 간단합니다.

### 부족한 부분과 개선 필요 항목
- **스테이지 진행과 목표**: 현재는 무한 진행 구조에 가깝고 거리/목표 지점·클리어 조건이 정의돼 있지 않습니다. 포트폴리오용이라면 스테이지별 목표(예: 거리, 생존 시간, 특정 NPC 패턴 클리어)를 명확히 제시하는 시스템이 필요합니다.
- **난이도/패턴 다양성**: `PatternSpawner`가 패턴을 랜덤 선택하는 단순 구조라 스테이지·차량 종류에 따른 패턴 변화가 없습니다. 차종·스테이지 태그 기반의 패턴 풀 관리가 요구됩니다.
- **경제/업그레이드 루프 부재**: 골드, 업그레이드, 차량 해금 등 메타 진행이 전혀 구현되지 않았습니다. 포인트/골드 획득과 함께 `CarStats`를 업그레이드하거나 새로운 차를 구매하는 흐름이 필요합니다.
- **임시 이펙트 자산**: 속도선, 라인 이펙트 등이 임시 그래픽으로 남아 있어 완성도 높은 VFX/사운드 연출과 UI 애니메이션 추가가 요구됩니다.
- **데이터 보존 및 테스트**: 진행도 저장, 업그레이드 상태 유지가 없고 테스트 코드도 없는 상태입니다. 포트폴리오에서는 간단한 저장/로딩 및 핵심 기능에 대한 테스트(예: Nitro 콤보 계산, 패턴 로직)가 있으면 신뢰도가 올라갑니다.

### 추가 기능 아이디어 및 구체화
- **스테이지 관리 시스템**: `StageData` ScriptableObject를 신설해 목표 거리, 제한 시간, 등장할 패턴 세트, 등장 NPC 차량 풀을 정의하고, `StageManager`가 목표 달성/실패 조건을 판정하도록 구성합니다.
- **NPC 패턴 다변화**: 스테이지·차량 타입별로 패턴을 레이어링하고, 이동형 장애물, 차선 변경형 NPC, 느린 화물차 등 속도 차이를 활용해 회피 리듬을 만듭니다.
- **골드 & 보상 루프**: `CoinPickup` 프리팹을 추가해 `PatternSpawner`가 패턴과 함께 골드 라인을 배치하고, `PlayerInfoView`에 골드 HUD를 보강합니다. 획득 골드는 경기 종료 시 정산되어 저장됩니다.
- **업그레이드/차량 해금**: `CarStats`를 확장하거나 신규 ScriptableObject(`CarUpgrade`, `GarageCar`)로 차의 스펙을 정의해 상점 UI와 연결합니다. 단기적으로는 Nitro 지속 시간, 부스트 속도, 연료 탱크 등을 업그레이드 항목으로 제시할 수 있습니다.
- **가속 패드 & 콤보 연계**: 트랙 위에 `SpeedPad` 트리거를 배치해 일시 가속을 제공하고, 패드를 밟을 때 콤보/니트로 게이지를 추가 보정해 회피 루트 설계의 재미를 높입니다.
- **시각/청각 피드백 강화**: Nitro 발동·콤보 달성 시 카메라 전환, UI 하이라이트, 파티클, 사운드 큐를 체계적으로 묶어 플레이 감각을 강화합니다.

### 구현 우선순위 제안
- 1단계: `StageManager`, `StageData`, `StageHudView` 등을 도입해 스테이지 목표와 진행도를 HUD에 표시하고, 패턴 스폰 로직을 스테이지 의존형으로 리팩터링합니다 (`PatternSpawner.cs`, `ObstaclePattern` 확장).
- 2단계: 골드/점수 시스템을 `PlayerInfoModel`에 통합하고, 골드 픽업 프리팹 및 UI 표시를 구현합니다. 경기 종료 시 정산 화면을 추가합니다.
- 3단계: 업그레이드/차량 해금 UI 및 데이터 구조를 설계하고, `CarStats` 갱신과 저장 로직(PlayerPrefs 또는 간단한 JSON)을 연결합니다.
- 4단계: Nitro·가속 패드·콤보 효과를 시각화할 파티클/VFX와 카메라 연출을 정리하고, 임시 이펙트를 대체할 아트 리소스를 준비합니다.
- 5단계: 핵심 시스템에 대해 Play Mode 테스트를 작성해 콤보 누적, Nitro 발동 조건, 스테이지 클리어 판정 등이 리팩터링 후에도 안정적으로 동작하는지 검증합니다.
