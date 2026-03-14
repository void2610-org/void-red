# チュートリアルバトル実装プラン

対象ドキュメントは `Docs/tutorial-battle-plan/tutorial-battle-player-restrictions.md`。

このディレクトリでは、実装をフェーズごとに分けて管理する。
依存関係の強い作業を先に片付け、`TutorialBattlePresenter` 実装時の手戻りを減らす構成にしている。

## フェーズ一覧

- [phase1-foundation.md](./phase1-foundation.md): 基盤拡張
- [phase2-ui-control-api.md](./phase2-ui-control-api.md): UI 制御 API 追加
- [phase3-data-and-di.md](./phase3-data-and-di.md): チュートリアルデータと DI 切り替え
- [phase4-tutorial-flow.md](./phase4-tutorial-flow.md): チュートリアル進行実装
- [phase5-verification.md](./phase5-verification.md): 検証と整形

## 推奨実装順

1. フェーズ1を完了し、`BattlePresenter` と周辺ロジックの拡張点を固定する
2. フェーズ2で UI 側の操作制約 API を追加する
3. フェーズ3で `TutorialBattlePresenter` を登録可能な状態にする
4. フェーズ4でチュートリアル進行本体を実装する
5. フェーズ5でコンパイル、整形、動作確認を行う

## 実装方針

- `BattlePresenter` 本体に `EnemyId` チェックを残さない
- チュートリアル特有の制御は `TutorialBattlePresenter` に寄せる
- UI には状態機械を持ち込まず、必要最小限の操作制御 API のみ追加する
- YAGNI と KISS を優先し、今回必要な制御だけを実装する
