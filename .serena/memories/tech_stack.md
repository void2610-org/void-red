# 技術スタック

## コアフレームワーク
- **Unity**: 6000.0.50f1
- **ターゲットプラットフォーム**: WebGL, Windows, macOS

## 主要ライブラリ

### 依存性注入（DI）
- **VContainer** (jp.hadashikick.vcontainer)
  - LifetimeScope単位でのDI管理
  - Singleton/Transient/Scopedライフタイム

### リアクティブプログラミング
- **R3** (com.cysharp.r3)
  - Observable/ReactivePropertyによる状態管理
  - イベント通知システム
  - UI更新の自動化

### 非同期処理
- **UniTask** (com.cysharp.unitask)
  - async/awaitベースの非同期処理
  - `.Forget()`によるfire-and-forget
  - UniTask.WhenAll()での並列処理

### アニメーション
- **LitMotion** (com.annulusgames.lit-motion)
  - カードアニメーション（ドロー、削除、配置、ハイライト）
  - トゥイーンベースのモーション制御
  - パフォーマンス最適化

### エディター拡張
- **Unity-SerializeReferenceExtensions** (MackySoft.SerializeReferenceExtensions)
  - SubclassSelectorアトリビュート
  - 進化条件の直感的な設定
  - カスタムエディタの簡素化

### アセット管理
- **Addressables** (com.unity.addressables 2.7.2)
  - 動的アセット読み込み
  - メモリ効率化

### UI
- **TextMeshPro** (Unity標準)
- **Universal Render Pipeline (URP)** (17.0.4)
- **UIEffect** (com.coffee.ui-effect)
- **UnmaskForUGUI** (com.coffee.unmask)

### 外部統合
- **Steamworks.NET** (com.rlabrecque.steamworks.net)
  - Steam実績・統計
- **Discord SDK** (カスタム統合)
  - Discordリッチプレゼンス
- **unityroom client** (com.unityroom.client)

### その他
- **Newtonsoft.Json** (com.unity.nuget.newtonsoft-json 3.2.1)
  - セーブデータのシリアライズ/デシリアライズ
- **Input System** (com.unity.inputsystem 1.14.2)
- **Timeline** (com.unity.timeline 1.8.9)
- **Burst Compiler** - パフォーマンス最適化

## 開発ツール
- **Unity Recorder** (5.1.3) - 録画機能
- **Unity Toolbar Extension** - エディター拡張
- **NuGet For Unity** - パッケージ管理
- **Unity Template** (com.void2610.unity-template) - プロジェクトテンプレート

## プラットフォーム固有
- macOS (Darwin): 開発環境
- AppleScript統合: unity-compile.shでのUnityエディター制御
