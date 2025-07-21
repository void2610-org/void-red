# GitHub Copilot レビュー指示

## 基本ルール
- 全て日本語でレビューコメントを作成してください
- 一つのコメントは一つの問題に集中してください
- コメントの最初に以下のプレフィックスを付けてください：

### プレフィックスルール
- `[must]` - 絶対に修正が必要なバグ、セキュリティ問題、パフォーマンス問題
- `[recommend]` - 修正を推奨する改善点（パフォーマンス、保守性、可読性）
- `[nits]` - 細かいスタイルや命名の提案

## Unity プロジェクト固有のチェックポイント

### C#命名規則
**PascalCase (クラス名, メソッド名, public/protectedフィールド, Enum):**
- public class TestClass{}
- private void TestMethod(){}
- protected int TestField = 0;
- public enum TestEnum{}

**camelCase ([SerializeField]フィールド, ローカル変数, 仮引数):**
- [SerializeField] private int testField = 0;
- private void Sum(int firstNumber, int secondNumber)
- var sumNumber = firstNumber + secondNumber;

**_camelCase (プライベートフィールド):**
- private int _testField = 0;
- private string _testString = "test";

**UPPER_SNAKE_CASE (定数):**
- private const int TEST_CONSTANT = 0;

**IPascalCase (インターフェース):**
- public interface ITestInterface(){}

### Unityコーディングパターン
- `nullチェック`: Unityオブジェクトは`!obj`を使用（`obj != null`ではなく）
- `SerializeField`: Inspector設定されるコンポーネントはそのままNullReferenceExceptionを発生させる
- `UniTask`: 非同期処理はすべてUniTaskを使用
- `R3`: Observableパターンはすべて R3 を使用
- `LitMotion`: アニメーションはLitMotionを使用
- `VContainer`: 依存性注入はVContainerを使用

### MVPアーキテクチャルール
- PresenterはViewを制御し、Modelの状態を管理する
- ModelはR3のReactivePropertyで状態変更を通知する
- Serviceは独立したビジネスロジックを担当する
- Logicは状態を持たない静的メソッドで実装する

### パフォーマンスとメモリ使用量
- `new`でのオブジェクト生成を最小限に留める
- `string`連結ではなく`StringBuilder`や`string interpolation`を使用
- `foreach`の代わりに`for`ループを使用（ガベージコレクション回避）
- `UniTask.WhenAll`で並列処理を活用する

### セキュリティ
- シークレット情報のログ出力やコミットを禁止
- publicフィールドの代わりにpropertyを使用
- 入力値のバリデーションを必ず実装

## レビュー範囲
- バグや潜在的な問題の指摘
- Unityパフォーマンスの改善提案
- コードの可読性と保守性の向上
- アーキテクチャルールの遵守
- セキュリティの改善提案
- Unityコーディングパターンの最適化