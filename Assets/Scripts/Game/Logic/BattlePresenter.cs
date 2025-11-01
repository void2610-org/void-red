using R3;
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using VContainer.Unity;
using UnityEngine;
using Void2610.UnityTemplate;

public class BattlePresenter: IStartable, ISceneInitializable
{
    private readonly BattleUIPresenter _battleUIPresenter;
    private readonly Player _player;
    private readonly Enemy _enemy;
    private readonly CardNarrationService _cardNarrationService;
    private readonly GameProgressService _gameProgressService;
    private readonly PersonalityLogService _personalityLogService;
    private readonly SceneTransitionManager _sceneTransitionManager;
    private readonly AllEnemyData _allEnemyData;
    private readonly AllThemeData _allThemeData;
    private readonly DiscordService _discordService;

    private readonly UniTaskCompletionSource _initializationComplete = new();

    private EnemyData _currentEnemyData;
    private ThemeData _currentTheme;
    private PlayerMove _playerMove;
    private PlayerMove _enemyMove;
    
    private int _playerWins;
    private int _enemyWins;
    private bool _playerCollapse;
    private bool _npcCollapse;
    private int _currentTurnNumber;
    private readonly List<ThemeData> _wonThemes = new();
    private readonly ReactiveProperty<GameState> _currentGameState = new(GameState.ThemeAnnouncement);
    
    public ReadOnlyReactiveProperty<GameState> CurrentGameState => _currentGameState;

    /// <summary>
    /// シーンの初期化完了を待つ（ISceneInitializable実装）
    /// </summary>
    public UniTask WaitForInitializationAsync() => _initializationComplete.Task;

    /// <summary>
    /// コンストラクタ（依存性注入）
    /// </summary>
    public BattlePresenter(
        BattleUIPresenter battleUIPresenter,
        Player player,
        Enemy enemy,
        CardNarrationService cardNarrationService,
        GameProgressService gameProgressService,
        PersonalityLogService personalityLogService,
        SceneTransitionManager sceneTransitionManager,
        AllEnemyData allEnemyData,
        AllThemeData allThemeData,
        DiscordService discordService)
    {
        _battleUIPresenter = battleUIPresenter;
        _player = player;
        _enemy = enemy;
        _cardNarrationService = cardNarrationService;
        _gameProgressService = gameProgressService;
        _personalityLogService = personalityLogService;
        _sceneTransitionManager = sceneTransitionManager;
        _allEnemyData = allEnemyData;
        _allThemeData = allThemeData;
        _discordService = discordService;

        // 崩壊フラグを初期化
        _playerCollapse = false;
        _npcCollapse = false;
        _currentTurnNumber = 0;
    }
    
    public void Start()
    {
        // UIPresenterにBattlePresenterを設定（循環依存を避けるため）
        _battleUIPresenter.SetBattlePresenter(this);

        InitializeGameAsync().Forget();
        BgmManager.Instance.PlayBGMBySceneType(BgmType.Battle);
    }

    /// <summary>
    /// ゲーム初期化（非同期）
    /// 完了後にSceneReadyNotifierに通知
    /// </summary>
    private async UniTaskVoid InitializeGameAsync()
    {
        await InitializeGame(true);
        _initializationComplete.TrySetResult();
        await StartGame();
    }
    
    /// <summary>
    /// ゲームを初期化
    /// </summary>
    /// <param name="isInitialStart">ゲームの最初の起動かどうか</param>
    private async UniTask InitializeGame(bool isInitialStart)
    {
        await _cardNarrationService.InitializeAsync();
        await UniTask.Delay(500);
        
        // 人格ログデータをセーブデータからロード
        _personalityLogService.SetPersonalityLogData(_gameProgressService.GetPersonalityLogData());
        
        // GameProgressServiceから敵データを取得
        var currentNode = _gameProgressService.GetCurrentNode();
        if (currentNode is not BattleNode battleNode)
        {
            Debug.LogError("[GameManager] 現在のノードがBattleNodeではありません");
            return;
        }
        
        _currentEnemyData = _allEnemyData.GetEnemyById(battleNode.EnemyId);
        if (!_currentEnemyData)
        {
            Debug.LogError("[GameManager] 敵データが見つかりません");
            return;
        }
        
        // Discord Rich Presence更新（バトル開始）
        _discordService?.SetState("対戦相手", _currentEnemyData.EnemyName);
        
        // 人格ログ: チャプター開始
        _personalityLogService.StartChapter(_currentEnemyData);
        
        // 次の敵への進行の場合は手札を戻す演出
        if (!isInitialStart)
        {
            var returnTasks = new UniTask[2];
            if (_player.HandCount > 0)
                returnTasks[0] = _player.ReturnHandToDeck();
            else
                returnTasks[0] = UniTask.CompletedTask;
                
            if (_enemy.HandCount > 0)
                returnTasks[1] = _enemy.ReturnHandToDeck();
            else
                returnTasks[1] = UniTask.CompletedTask;
            
            await UniTask.WhenAll(returnTasks);
            await UniTask.Delay(300);
        }
        
        // 敵を初期化して表示
        _enemy.SetEnemyData(_currentEnemyData);
        _battleUIPresenter.InitializeEnemy(_currentEnemyData);
        await _battleUIPresenter.ShowEnemy();
        
        // 敵情報をアナウンス
        await _battleUIPresenter.ShowAnnouncement(_currentEnemyData.EnemyName, 1.5f);
        
        // セーブデータからデッキを初期化
        _player.LoadDeckFromSaveData();
        
        // 敵デッキ: 固定デッキを使用
        var enemyDeck = new List<CardData>(_currentEnemyData.InitialDeck);
        _enemy.InitializeDeck(enemyDeck);
        
    }

    private async UniTask StartGame()
    {
        await UniTask.Delay(1000);
        
        _player.DrawCardsWithDelay(3, 300).Forget();
        await UniTask.Delay(200);
        await _enemy.DrawCardsWithDelay(3, 300);

        // ゲーム開始
        ChangeState(GameState.ThemeAnnouncement).Forget();
    }
    
    /// <summary>
    /// ステートを変更
    /// </summary>
    private async UniTask ChangeState(GameState newState)
    {
        _currentGameState.Value = newState;
        
        switch (newState)
        {
            case GameState.ThemeAnnouncement:
                _playerCollapse = false; // 崩壊フラグリセット
                _npcCollapse = false; // 崩壊フラグリセット
                _battleUIPresenter.ResetEnemyToDefault().Forget();
                await HandleThemeAnnouncement();
                break;
            case GameState.PlayerCardSelection:
                HandlePlayerCardSelection().Forget();
                break;
            case GameState.EnemyCardSelection:
                await HandleEnemyCardSelection();
                break;
            case GameState.Evaluation:
                await HandleEvaluation();
                break;
            case GameState.ResultDisplay:
                await HandleResultDisplay();
                break;
            case GameState.BattleEnd:
                await HandleBattleEnd();
                break;
            case GameState.GameOver:
                HandleGameOver();
                break;
        }
    }
    
    /// <summary>
    /// お題発表フェーズ
    /// </summary>
    private async UniTask HandleThemeAnnouncement()
    {
        _currentTurnNumber++;

        // 人格ログ: ターン開始
        _personalityLogService.StartTurn();

        // ターン番号に基づいてテーマを順番に選択（1ターン目 = index 0, 2ターン目 = index 1, 3ターン目 = index 2）
        _currentTheme = _currentEnemyData.Themes[_currentTurnNumber - 1];

        await _battleUIPresenter.SetTheme(_currentTheme, _currentTurnNumber == 3);

        // 初回ターン かつ 敵がアルヴならチュートリアルを表示
        if (_currentTurnNumber == 1 && _currentEnemyData.EnemyId == "alv")
            await _battleUIPresenter.StartBattleTutorial();

        // テーマ詳細を表示して閉じられるまで待機
        await _battleUIPresenter.ShowThemeDetailAndWait();

        // 会話シーケンスを表示してからカード選択へ
        await ShowThemeDialoguesAsync();

        ChangeState(GameState.PlayerCardSelection).Forget();
    }

    /// <summary>
    /// テーマ会話を順次表示
    /// </summary>
    private async UniTask ShowThemeDialoguesAsync()
    {
        if (_currentTheme == null || _currentTheme.Dialogues == null)
        {
            await UniTask.Delay(300);
            ChangeState(GameState.PlayerCardSelection).Forget();
            return;
        }

        // 各会話を順次表示
        foreach (var dialogue in _currentTheme.Dialogues)
        {
            if (string.IsNullOrEmpty(dialogue.Message)) continue;

            if (dialogue.IsPlayer)
                await _battleUIPresenter.ShowNarration(dialogue.Message, autoAdvance: true);
            else
                await _battleUIPresenter.ShowEnemyNarration(dialogue.Message, autoAdvance: true);
        }

        await UniTask.Delay(300);
    }
    
    /// <summary>
    /// プレイヤーカード選択フェーズ
    /// </summary>
    private async UniTask HandlePlayerCardSelection()
    {
        // カード選択を待つ
        while (true)
        {
            await UniTask.Yield();
            
            var selectedCard = _player.SelectedCard.CurrentValue;
            if (selectedCard == null) continue;
            // カードが選択されたらプレイボタンを有効化
            _battleUIPresenter.SetPlayButtonInteractable(true);
            break;
        }

        // プレイボタンが押されるのを待つ
        try
        {
            await _battleUIPresenter.PlayButtonClicked.FirstAsync();
        }
        catch (InvalidOperationException)
        {
            // PlayButtonが破棄された場合は処理を中断
            return;
        }

        _battleUIPresenter.SetPlayButtonInteractable(false);
        // 選択されたカードを再取得
        var finalSelectedCard = _player.SelectedCard.CurrentValue;
        if (finalSelectedCard == null) return;
        
        _battleUIPresenter.UpdateEnemySprite(finalSelectedCard.Data.Attribute).Forget();
        
        // プレイヤーの手を作成
        var playStyle = _battleUIPresenter.GetSelectedPlayStyle();
        
        // 精神力を消費
        var mentalBet = _battleUIPresenter.GetMentalBetValue();
        _player.ConsumeMentalPower(mentalBet);
        _playerMove = new PlayerMove(finalSelectedCard.Data, playStyle, mentalBet);
        
        // 選択されたカードを閲覧済みとして記録
        _gameProgressService.RecordCardView(finalSelectedCard.Data);
        
        // 人格ログ: プレイヤームーブ記録
        _personalityLogService.LogPlayerMove(_playerMove, _player.MentalPower.CurrentValue);
        
        await UniTask.Delay(500);
        ChangeState(GameState.EnemyCardSelection).Forget();
    }
    
    /// <summary>
    /// 敵カード選択フェーズ
    /// </summary>
    private async UniTask HandleEnemyCardSelection()
    {
        await UniTask.Delay(1000);
        
        // AIでカードを選択
        var npcCard = _enemy.GetRandomCardFromHand();
        _enemy.SelectCard(npcCard);
        // NPCの手を作成（NPCもランダムなプレイスタイルと精神ベットを選択）
        var npcPlayStyle = _enemy.DecidePlayStyle();
        var npcMentalBet = UnityEngine.Random.Range(1, Mathf.Min(6, _enemy.MentalPower.CurrentValue + 1)); // NPCの精神力範囲内でベット
        
        // NPCの精神力を消費
        _enemy.ConsumeMentalPower(npcMentalBet);
        _enemyMove = new PlayerMove(npcCard.Data, npcPlayStyle, npcMentalBet);
        
        // 敵のカードも閲覧済みとして記録
        _gameProgressService.RecordCardView(npcCard.Data);
        
        // 人格ログ: 敵ムーブ記録
        _personalityLogService.LogEnemyMove(_enemyMove, _enemy.MentalPower.CurrentValue);
        
        // プレイヤーのカードプレイ前ナレーションを表示（実際の語り内容）
        var narrationContent = _cardNarrationService.GetNarration(_playerMove.SelectedCard, NarrationType.PrePlay, _playerMove.PlayStyle);
        var displayContent = string.IsNullOrEmpty(narrationContent) ? "..." : narrationContent;
        await _battleUIPresenter.ShowNarration(displayContent, true);
        
        // 少し間を置いてから評価フェーズに移行
        await UniTask.Delay(500);
        // 結果表示の背景を表示
        await _battleUIPresenter.ShowBlackOverlay();
        
        // 評価フェーズへ
        ChangeState(GameState.Evaluation).Forget();
    }
    
    /// <summary>
    /// 評価フェーズ
    /// </summary>
    private async UniTask HandleEvaluation()
    {
        // スコアを計算（テーマ倍率 × 精神ベット × PlayStyle相性）
        var playerScore = ScoreCalculator.CalculateScore(_playerMove, _enemyMove, _currentTheme);
        var npcScore = ScoreCalculator.CalculateScore(_enemyMove, _playerMove, _currentTheme);
        
        // 評価結果をスコア専用Viewで同時表示
        await _battleUIPresenter.ShowScores(playerScore, npcScore);
        
        // カード崩壊判定
        _playerCollapse = CollapseJudge.ShouldCollapse(_playerMove);
        _npcCollapse = CollapseJudge.ShouldCollapse(_enemyMove);

        // 崩壊結果を表示
        if (_playerCollapse || _npcCollapse)
        {
            string collapseMessage;
            if (_playerCollapse && _npcCollapse)
                collapseMessage = "プレイヤーとNPCのカードが崩壊した";
            else if (_playerCollapse)
                collapseMessage = "プレイヤーのカードが崩壊した";
            else
                collapseMessage = "対戦相手のカードが崩壊した";

            // 崩壊演出を実行
            var tasks = new List<UniTask> { _battleUIPresenter.ShowAnnouncement(collapseMessage, 1.0f) };

            if (_playerCollapse) tasks.Add(_player.CollapseSelectedCard());
            if (_npcCollapse) tasks.Add(_enemy.CollapseSelectedCard());

            await UniTask.WhenAll(tasks);
        }
        
        // 結果表示フェーズに移行
        await UniTask.Delay(500);
        ChangeState(GameState.ResultDisplay).Forget();
    }
    
    /// <summary>
    /// 勝敗表示フェーズ
    /// </summary>
    private async UniTask HandleResultDisplay()
    {
        var playerScore = ScoreCalculator.CalculateScore(_playerMove, _enemyMove, _currentTheme);
        var npcScore = ScoreCalculator.CalculateScore(_enemyMove, _playerMove, _currentTheme);

        // 崩壊結果を考慮した勝敗決定
        string result;
        bool playerWon;

        if (_playerCollapse && _npcCollapse)
        {
            // ランダムで勝敗を決定
            playerWon = UnityEngine.Random.Range(0, 2) == 0;
            result = playerWon ? "あなたの勝利\n（引き分け→ランダム決着）" : "相手の勝利\n（引き分け→ランダム決着）";
        }
        else if (_playerCollapse)
        {
            result = "相手の勝利（あなたのカード崩壊）";
            playerWon = false;
        }
        else if (_npcCollapse)
        {
            result = "あなたの勝利（相手カード崩壊）";
            playerWon = true;
        }
        else
        {
            // 崩壊がない場合は従来のスコア比較
            if (playerScore > npcScore)
            {
                result = "あなたの勝利";
                playerWon = true;
            }
            else if (npcScore > playerScore)
            {
                result = "相手の勝利";
                playerWon = false;
            }
            else
            {
                // スコア引き分けもランダム決着
                playerWon = UnityEngine.Random.Range(0, 2) == 0;
                result = playerWon ? "あなたの勝利\n（引き分け→ランダム決着）" : "相手の勝利\n（引き分け→ランダム決着）";
            }
        }

        // 結果を表示（スコアと内訳付き）
        await _battleUIPresenter.ShowWinLoseResult(result, playerWon, playerScore, npcScore, _playerMove, _enemyMove, _currentTheme);
        await UniTask.Delay(500);
        await _battleUIPresenter.HideBlackOverlay();

        // プレイヤー勝利時は現在のテーマを記録
        if (playerWon) _wonThemes.Add(_currentTheme);
        
        // 勝敗確定後のナレーション（プレイヤーの勝敗に基づく）
        var playerNarrationType = playerScore > npcScore ? NarrationType.PostBattleWin : NarrationType.PostBattleLose;
        var postBattleNarration = _cardNarrationService.GetNarration(_playerMove.SelectedCard, playerNarrationType, _playerMove.PlayStyle);
        var displayNarration = string.IsNullOrEmpty(postBattleNarration) ? "..." : postBattleNarration;
        await _battleUIPresenter.ShowNarration(displayNarration, autoAdvance: true);
        
        // 敵の勝敗確定後のナレーション
        var enemyNarrationType = playerScore > npcScore ? NarrationType.PostBattleLoseEnemy : NarrationType.PostBattleWinEnemy;
        var enemyPostBattleNarration = _cardNarrationService.GetNarration(_playerMove.SelectedCard, enemyNarrationType, _playerMove.PlayStyle);
        var enemyDisplayNarration = string.IsNullOrEmpty(enemyPostBattleNarration) ? "..." : enemyPostBattleNarration;
        await _battleUIPresenter.ShowEnemyNarration(enemyDisplayNarration, true);
        
        // ゲーム結果を統計に記録（進化チェック前に実行）
        _gameProgressService.RecordPlayerGameResult(playerWon, _playerMove, _playerCollapse);

        // すべての場合で勝利数をカウントするように変更
        if (UpdateWinsAndCheckBattleEnd(playerWon))
        {
            // 3勝に達した場合、カード処理をスキップしてバトル終了へ
            ChangeState(GameState.BattleEnd).Forget();
            return;
        }
        
        // 使用したカードをプレイ
        if (_playerCollapse)
        {
            // 崩壊処理前にカード情報を取得
            var playerCollapseCard = _player.SelectedCard.CurrentValue;
            
            if (playerCollapseCard != null)
                _personalityLogService.LogCardCollapse("player", playerCollapseCard.Data);
        }
        else
        {
            // プレイヤーカードの進化処理
            // var selectedCard = _player.SelectedCard.CurrentValue;
            // var playerCardAfterEvolution = _gameProgressService.CheckPlayerCardEvolution(selectedCard.Data);
            
            // FIXME: 現段階では進化機能は無効化
            // 元のカードと異なる場合は進化が発生
            // if (playerCardAfterEvolution != selectedCard.Data)
            // {
            //     _player.ReplaceCard(selectedCard, playerCardAfterEvolution);
            //     
            //     // 人格ログ: プレイヤーカード進化イベント記録
            //     _personalityLogService.LogCardEvolution("player", selectedCard.Data, playerCardAfterEvolution);
            //     await _battleUIPresenter.ShowAnnouncement($"プレイヤーの {selectedCard.Data.CardName} が {playerCardAfterEvolution.CardName} に変化");
            // }
            
            // FIXME: 現段階では共鳴機能も無効化
            // 共鳴チェック
            // if (_currentEnemyData && _currentEnemyData.ResonanceCard && selectedCard.Data == _currentEnemyData.ResonanceCard)
            // {
            //     // 人格ログ: 共鳴イベント記録
            //     _personalityLogService.LogResonance("Player", selectedCard.Data);
            //     await _battleUIPresenter.ShowAnnouncement($"共鳴発生: {selectedCard.Data.CardName}");
            // }
            
            // カードをプレイ（崩壊しない）
            _player.PlaySelectedCard(false);
        }

        if (_npcCollapse)
        {
            var npcCollapseCard = _enemy.SelectedCard.CurrentValue;
            // 実際の崩壊処理を実行
            _enemy.PlaySelectedCard(true);
            
            if (npcCollapseCard != null)
                _personalityLogService.LogCardCollapse("enemy", npcCollapseCard.Data);
        }
        else
        {
            // カードをプレイ（崩壊しない）
            _enemy.PlaySelectedCard(false);
        }
        
        // カード使用後の処理完了を待つ
        await UniTask.Delay(1000);
        
        // 両プレイヤーの手札をデッキに戻す
        await UniTask.WhenAll(_player.ReturnHandToDeck(), _enemy.ReturnHandToDeck());
        
        _player.DrawCardsWithDelay(3, 300).Forget();
        await UniTask.Delay(200);
        await _enemy.DrawCardsWithDelay(3, 300);
        
        // 新しいラウンドの準備時間
        await UniTask.Delay(1000);
        
        // 人格ログ: ターン終了
        _personalityLogService.EndTurn();
        
        // ゲームオーバー条件をチェック
        ChangeState(CheckGameOverConditions() ? GameState.GameOver : GameState.ThemeAnnouncement).Forget();
    }
    
    /// <summary>
    /// ゲームオーバー条件をチェック
    /// </summary>
    /// <returns>ゲームオーバー条件を満たしているかどうか</returns>
    private bool CheckGameOverConditions()
    {
        // プレイヤーの精神力が0になったか
        if (_player.MentalPower.CurrentValue <= 0) return true;
        // プレイヤーの全カードが崩壊したかどうか
        if (_player.IsAllCardsCollapsed) return true;
        
        return false;
    }

    /// <summary>
    /// 勝利数を更新してバトル終了をチェック
    /// </summary>
    /// <param name="isPlayerWon"></param>
    /// <returns>バトルが終了したかどうか</returns>
    private bool UpdateWinsAndCheckBattleEnd(bool isPlayerWon)
    {
        if (isPlayerWon) _playerWins++;
        else _enemyWins++;

        return _currentTurnNumber >= GameConstants.BATTLE_TURNS;
    }
    
    /// <summary>
    /// バトル終了フェーズ
    /// </summary>
    private async UniTask HandleBattleEnd()
    {
        await UniTask.Delay(500);
        
        // Volumeエフェクトを全てデフォルトに戻す
        VolumeController.Instance.ResetToDefault();

        // 3ターン終了後、勝利数が多い方が勝利（同数の場合はプレイヤー勝利）
        var playerWon = _playerWins >= _enemyWins;

        _battleUIPresenter.ShowBattleResult(playerWon, _playerWins, _enemyWins, _wonThemes);
        
        // 敵がアルヴならチュートリアルを表示
        if (_currentEnemyData.EnemyId == "alv")
            await _battleUIPresenter.StartResultTutorial();
        
        await _battleUIPresenter.WaitForBattleResultClose();
        
        // 人格ログ: チャプター完了
        _personalityLogService.CompleteChapter();
        // 人格ログデータをGameProgressServiceに更新（セーブのため）
        _gameProgressService.UpdatePersonalityLogData(_personalityLogService.GetPersonalityLogData());
        // プレイヤーのデッキ変更をセーブ（バトル終了時のみ）
        _player.SaveDeckChanges();
        
        // 現在のノード情報を一旦キャッシュ
        var currentNode = _gameProgressService.GetCurrentNode();
        
        // 現在のバトル結果を記録(ここでノード進行する)
        _gameProgressService.RecordBattleResultAndSave(playerWon);
        Debug.Log($"[GameManager] バトル完了: {(playerWon ? "勝利" : "敗北")} - ストーリー進行");
        
        // ノード設定に基づいてシーン遷移
        await UniTask.Delay(1000);
        if (currentNode.ReturnToHome)
        {
            // ホームに戻る
            await _sceneTransitionManager.TransitionToSceneWithFade(SceneType.Home);
        }
        else
        {
            // 次のノードへ直接遷移
            var nextScene = _gameProgressService.GetNextSceneType();
            await _sceneTransitionManager.TransitionToSceneWithFade(nextScene);
        }
    }
    
    /// <summary>
    /// ゲームオーバーフェーズ
    /// </summary>
    private void HandleGameOver()
    {
        // ゲームオーバーの理由を判定
        var gameOverReason = "";
        if (_player.MentalPower.CurrentValue <= 0)
            gameOverReason = "精神力が0になりました";
        else if (_player.IsAllCardsCollapsed)
            gameOverReason = "すべてのカードが崩壊しました";

        // ゲームオーバー画面を表示
        _battleUIPresenter.ShowGameOverScreen(gameOverReason);
    }
}
