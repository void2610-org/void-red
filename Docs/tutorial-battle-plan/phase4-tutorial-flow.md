# フェーズ4: チュートリアル進行実装

## 目的

`TutorialBattlePresenter` にチュートリアルバトル全体の制御を集中させる。

## 対象ファイル

- `Assets/Scripts/Game/Logic/TutorialBattlePresenter.cs`
- 必要に応じて同ファイル内の `TutorialCompetitionPhaseRunner`

## 実装タスク

### 1. クラス骨組み作成

- `BattlePresenter` 継承クラスとして `TutorialBattlePresenter` を作成する
- 必要なら `TutorialCompetitionPhaseRunner` を同ファイルに定義する
- コンストラクタでチュートリアル用 AI、競合処理、オークション処理の差し替え方針を確定する

### 2. 演出フック実装

- `HandleThemeAnnouncement()` をオーバーライドする
- `base` 実行後に `StartTutorial("BeforeThemeAnnouncement")` を呼ぶ
- `HandleAuctionResult()` をオーバーライドする
- `StartTutorial("ResultDetermination")` 後に基底処理へ渡す
- `OnAfterCardsDisplayed()` をオーバーライドする
- `StartTutorial("RewardPhase")` を呼ぶ
- `OnAfterResourceGaugesDisplayed()` をオーバーライドする
- `StartTutorial("RewardPhase2")` を呼ぶ
- `OnBeforeMemoryGrowthContinueAsync()` をオーバーライドする
- `StartTutorial("MemoryGrowthPhase")` を呼ぶ

### 3. 入札フェーズ制御実装

- `HandleBiddingPhase()` をオーバーライドする
- フェーズ開始前に `StartTutorial("BiddingPhase")` を呼ぶ
- `RunTutorialBiddingAsync()` を実装する

`RunTutorialBiddingAsync()` の分解:

1. Step 2a 感情選択
- `StartAuctionBidding(...)` を開始する
- 全カードを無効化する
- 確定ボタンを無効化する
- 強制感情が選ばれるまで待機する

2. Step 2b カード選択
- 指定カードのみ有効化する
- そのカードがクリックされるまで待機する

3. Step 2c ベット
- 必要量に達するまで入札増加を待機する
- 到達後に増加ボタンを無効化する

4. Step 2d 確定
- 確定ボタンを有効化する
- 確定イベントを待機する

### 4. 競合フェーズ制御実装

- チュートリアル AI が引き分けを起こす前提で競合フェーズを実装する
- 指定感情でのみレイズを受け付ける
- 指定回数でプレイヤー勝利に到達させる
- 競合終了後に通常フローへ復帰できる形にする

### 5. デッキ選択とカードバトル制御実装

- `GetAllowedDeckCardIndices()` をオーバーライドする
- `GetForcedCoinFlip(int round)` をオーバーライドする
- `GetForcedBattleCard(int round)` をオーバーライドする
- `GetSkillRound()` をオーバーライドする

ラウンド制御の確認観点:

- 指定ラウンドではコインフリップ結果が固定される
- 指定ラウンドでは対象カードだけ配置できる
- 指定ラウンドでのみスキルボタンが表示される

### 6. 全体整合確認

- 対話、入札、競合、デッキ選択、カードバトル、報酬演出の順で途切れず遷移することを確認する
- `TutorialBattlePresenter` 内に責務が集約されていることを確認する

## 完了条件

- `alv` 戦でチュートリアル進行が再現性ある形で動作する
- 制御ロジックが `TutorialBattlePresenter` に集中している
- 基底クラス側にチュートリアル分岐が増えていない

## 注意点

- まず入札フェーズを完成させてから競合フェーズへ進む
- 一度に全フックを実装せず、各セクションごとにコンパイル可能な状態を保つ
- 競合フェーズの実装で既存 `CompetitionPhaseRunner` の責務を壊さない
