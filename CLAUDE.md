# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Unity Project Overview

void-red is a Unity card game project using Unity 6000.0.50f1 with VContainer for dependency injection, R3 for reactive programming, LitMotion for animations, UniTask for async operations, and Unity-SerializeReferenceExtensions for simplified editor customization.

## Development Workflow

1. Make code changes
2. Use unity-compile.sh to verify compilation
3. Run format check with `dotnet format analyzers FormatCheck.csproj --verify-no-changes --severity warn`
4. Fix any format violations
5. Run code quality check by `unity-code-quality-checker` subagent
6. Fix issues reported by the code quality checker
7. Report results to the user

**YAGNI (You Aren't Gonna Need It) 原則:**
- 将来必要になるかもしれない機能を先回りして実装しない
- 現在の要求を満たす最小限の実装に留める
- 過度な抽象化や汎用化を避ける
- 実際に必要になった時点で機能を追加する

**KISS (Keep It Simple, Stupid) 原則:**
- 可能な限りシンプルで理解しやすいコードを書く
- 複雑な設計パターンは本当に必要な場合のみ使用
- 1つのクラス・メソッドには1つの責任のみを持たせる
- 誰でも理解できる明快な実装を優先する

## Response Guidelines

- SerializeFieldで設定されるべきコンポーネントのnullチェックは不要。コンポーネントが正しく設定されていることを前提としたコードを記述する。
- プログラム内の全てのコメントは日本語で記述してください (from user's CLAUDE.md)
- Respond in Japanese and avoid excessive comments
- When refactoring, implement clean replacements rather than maintaining backward compatibility
- Always follow YAGNI and KISS principles - implement only what is needed now, keep it simple

## Core Commands

```bash
# Compile and check errors
./unity-tools/unity-compile.sh trigger . && sleep 3 && ./unity-tools/unity-compile.sh check .

# Check compilation errors only
./unity-tools/unity-compile.sh check .

# Format check (code style + custom analyzers UNA0001-UNA0004)
dotnet format style FormatCheck.csproj --verify-no-changes --severity warn
dotnet format analyzers FormatCheck.csproj --verify-no-changes --severity warn
```

## Key Design Principles
- **明確なファイル構造**: 機能別ディレクトリで保守性向上
- **責任の明確化**: Stats, Services, Logic, Coreの分離
- **Unity-First**: MonoBehaviourパターンの自然な活用
- **リアクティブ設計**: R3による状態変更の伝播

## Critical Implementation Details

### VContainer Setup
```csharp
// RootLifetimeScope.cs - Cross-scene services
builder.Register<SaveDataManager>(Lifetime.Singleton);
builder.Register<SceneTransitionService>(Lifetime.Singleton);
builder.Register<GameProgressService>(Lifetime.Singleton);

// BattleLifetimeScope.cs - Battle-specific dependencies
builder.RegisterInstance(player);
builder.RegisterInstance(enemy);
builder.Register<CardPoolService>(Lifetime.Singleton);
builder.Register<ThemeService>(Lifetime.Singleton);
builder.RegisterEntryPoint<GameManager>();
builder.RegisterComponentInHierarchy<UIPresenter>();

// HomeLifetimeScope.cs - Home scene dependencies  
builder.RegisterComponentInHierarchy<HomeUIPresenter>();
```

### Card Animation System
CardView contains all animations using LitMotion:
- PlayDrawAnimation: Deck to hand
- PlayRemoveAnimation: Normal removal or collapse effect
- PlayArrangeAnimation: Hand positioning
- PlayReturnToDeckAnimation: Hand to deck
- SetHighlight: Selection state

### Scene Architecture
- **TitleScene**: Entry point with TitleUIPresenter
- **HomeScene**: Hub scene with HomeUIPresenter for navigation
- **BattleScene**: Main game scene with Player, Enemy, UIPresenter components (renamed from MainScene)
- **NovelScene**: Story scene with NovelUIPresenter for narrative content

### Scene Transition System
The project uses a centralized SceneTransitionService for type-safe scene management and data passing:

```csharp
// SceneType enum with extension methods for type safety
public enum SceneType { Title, Home, Battle, Novel }

// Base transition data class
public abstract class SceneTransitionData
{
    public abstract SceneType TargetScene { get; }
    public SceneType ReturnScene { get; set; } = SceneType.Home;
    public virtual string GetDebugInfo() => $"Target: {TargetScene}, Return: {ReturnScene}";
}

// Specific transition data for battles
public class BattleTransitionData : SceneTransitionData
{
    public override SceneType TargetScene => SceneType.Battle;
    public EnemyData TargetEnemy { get; set; }  // Required for battle scenes
}

// Scene transition service usage
await _sceneTransitionService.TransitionToScene(battleData);
var receivedData = _sceneTransitionService.GetTransitionData<BattleTransitionData>();
```

### Scene Flow
- Title → Home → Battle/Novel → Home (post-battle/story return)
- All scenes use SceneTransitionService for navigation
- Data persistence across scene changes via transition data objects

## Dependencies

Key packages from manifest.json:
- VContainer (hadashiA/VContainer) - 依存性注入
- R3 (Cysharp/R3) - リアクティブプログラミング
- UniTask (Cysharp/UniTask) - 非同期処理
- LitMotion (AnnulusGames/LitMotion) - アニメーション
- Unity Template (void2610/my-unity-template) - プロジェクトテンプレート
- Unity-SerializeReferenceExtensions (mackysoft/Unity-SerializeReferenceExtensions) - SubclassSelector
