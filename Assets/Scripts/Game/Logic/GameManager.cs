using R3;
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using VContainer.Unity;
using UnityEngine;

public class GameManager: IStartable, IDisposable
{
    public ReadOnlyReactiveProperty<GameState> CurrentState => _currentState;
    
    private readonly CardPoolService _cardPoolService;
    private readonly ThemeService _themeService;
    private readonly UIPresenter _uiPresenter;
    private readonly Player _player;
    private readonly Enemy _enemy;
    private readonly CardNarrationService _cardNarrationService;
    private readonly GameProgressService _gameProgressService;
    private readonly SceneTransitionManager _sceneTransitionManager;
    private readonly AllEnemyData _allEnemyData;
    private readonly ReactiveProperty<GameState> _currentState = new (GameState.ThemeAnnouncement);
    private readonly ReactiveProperty<ThemeData> _currentTheme = new (null);
    private readonly CompositeDisposable _disposables = new();
    private PlayerMove _playerMove;
    private PlayerMove _npcMove;
    private bool _isProcessing;
    
    // バトル勝利数管理
    private int _playerWins;
    private int _enemyWins;
    private const int WINS_TO_VICTORY = 3;
    
    // 崩壊判定用メンバー変数
    private bool _playerCollapse;
    private bool _npcCollapse;
    
    // 現在の敵データ
    private EnemyData _currentEnemyData;

    /// <summary>
    /// コンストラクタ（依存性注入）
    /// </summary>
    public GameManager(
        CardPoolService cardPoolService,
        ThemeService themeService,
        UIPresenter uiPresenter,
        Player player,
        Enemy enemy,
        CardNarrationService cardNarrationService,
        GameProgressService gameProgressService,
        SceneTransitionManager sceneTransitionManager,
        AllEnemyData allEnemyData)
    {
        _cardPoolService = cardPoolService;
        _themeService = themeService;
        _uiPresenter = uiPresenter;
        _player = player;
        _enemy = enemy;
        _cardNarrationService = cardNarrationService;
        _gameProgressService = gameProgressService;
        _sceneTransitionManager = sceneTransitionManager;
        _allEnemyData = allEnemyData;

        // 崩壊フラグを初期化
        _playerCollapse = false;
        _npcCollapse = false;
    }
    
    public void Start()
    {
        InitializeGame(true).Forget();
        // リトライボタンのイベント
        _uiPresenter.RetryButtonClicked.Subscribe(
            _ => _sceneTransitionManager.TransitionToSceneWithFade(SceneType.Battle).Forget())
            .AddTo(_disposables);
        // タイトルボタンのイベント
        _uiPresenter.TitleButtonClicked.Subscribe(
            _ => _sceneTransitionManager.TransitionToSceneWithFade(SceneType.Title).Forget())
            .AddTo(_disposables);
    }
    
    /// <summary>
    /// ゲームを初期化
    /// </summary>
    /// <param name="isInitialStart">ゲームの最初の起動かどうか</param>
    private async UniTaskVoid InitializeGame(bool isInitialStart)
    {
        await _cardNarrationService.InitializeAsync();
        await UniTask.Delay(500);
        
        // バトル勝利数をリセット
        ResetBattleWins();
        // 敵の統計をリセット
        _gameProgressService.ResetEnemyStats();
        
        // GameProgressServiceから敵データを取得
        var currentNode = _gameProgressService.GetCurrentNode();
        if (currentNode is not BattleNode battleNode)
        {
            Debug.LogError("[GameManager] 現在のノードがBattleNodeではありません");
            return;
        }
        
        _currentEnemyData = _allEnemyData.GetEnemyById(battleNode.EnemyId);
        
        // 人格ログ: チャプター開始
        _gameProgressService.StartChapter(_currentEnemyData);
        
        // セーブデータは既にGameProgressServiceで自動ロード済み
        if (isInitialStart)
        {
            // プレイヤーの精神力を復元
            _player.SetMentalPower(_gameProgressService.GetPlayerMentalPower());
        }
        
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
        _uiPresenter.InitializeEnemy(_currentEnemyData);
        await _uiPresenter.ShowEnemy();
        
        // 敵情報をアナウンス
        await _uiPresenter.ShowAnnouncement(_currentEnemyData.EnemyName, 1.5f);
        
        // カードデッキを初期化
        // プレイヤーデッキ: セーブデータから復元を試行、失敗時はランダム生成
        if (!_player.LoadDeckFromSaveData())
        {
            var playerDeck = _cardPoolService.GetRandomCards(5);
            _player.InitializeDeck(playerDeck);
        }
        
        // 敵デッキ: 固定デッキを使用
        var enemyDeck = new List<CardData>(_currentEnemyData.InitialDeck);
        _enemy.InitializeDeck(enemyDeck);
        
        _player.DrawCardsWithDelay(3, 300).Forget();
        await UniTask.Delay(200);
        await _enemy.DrawCardsWithDelay(3, 300);
        
        // エネミーのカードを非インタラクティブに設定
        _enemy.SetHandInteractable(false);
        
        // 敵がアルヴならチュートリアルを表示
        if (_currentEnemyData.EnemyId == "E001") 
            await _uiPresenter.StartTutorial();
        
        // ゲーム開始
        ChangeState(GameState.ThemeAnnouncement);
    }
    
    /// <summary>
    /// ステートを変更
    /// </summary>
    private void ChangeState(GameState newState)
    {
        _currentState.Value = newState;
        switch (newState)
        {
            case GameState.ThemeAnnouncement:
                _isProcessing = false; // フラグリセット
                _playerCollapse = false; // 崩壊フラグリセット
                _npcCollapse = false; // 崩壊フラグリセット
                // 敵をデフォルト立ち絵に戻す
                _uiPresenter.ResetEnemyToDefault().Forget();
                HandleThemeAnnouncement();
                break;
            case GameState.PlayerCardSelection:
                _isProcessing = false; // フラグリセット
                HandlePlayerCardSelection();
                break;
            case GameState.EnemyCardSelection:
                _isProcessing = false; // フラグリセット
                HandleEnemyCardSelection();
                break;
            case GameState.Evaluation:
                HandleEvaluation();
                break;
            case GameState.ResultDisplay:
                HandleResultDisplay();
                break;
            case GameState.BattleEnd:
                HandleBattleEnd();
                break;
            case GameState.GameOver:
                HandleGameOver();
                break;
        }
    }
    
    /// <summary>
    /// お題発表フェーズ
    /// </summary>
    private void HandleThemeAnnouncement()
    {
        // 人格ログ: ターン開始
        _gameProgressService.StartTurn();
        
        // ランダムなお題を選択
        _currentTheme.Value = _themeService.GetRandomTheme();
        _uiPresenter.SetTheme(_currentTheme.Value);
        
        DelayedStateChangeAsync(GameState.PlayerCardSelection, 0.3f).Forget();
    }
    
    /// <summary>
    /// プレイヤーカード選択フェーズ
    /// </summary>
    private void HandlePlayerCardSelection()
    {
        // プレイヤーの操作を待つ（カード選択とプレイボタン）
        WaitForPlayerActionAsync().Forget();
    }
    
    /// <summary>
    /// プレイヤーのアクションを待つ
    /// </summary>
    private async UniTask WaitForPlayerActionAsync()
    {
        // カード選択を待つ
        while (true)
        {
            await UniTask.Yield();
            
            // ゲーム状態が変わったら終了
            if (_currentState.Value != GameState.PlayerCardSelection)
                return;
            
            var selectedCard = _player.SelectedCard.CurrentValue;
            if (selectedCard == null) continue;
            // カードが選択されたらプレイボタンを表示
            _uiPresenter.ShowPlayButton();
            break;
        }
        
        // プレイボタンが押されるのを待つ
        try
        {
            await _uiPresenter.PlayButtonClicked.FirstAsync();
        }
        catch (InvalidOperationException)
        {
            // PlayButtonが破棄された場合は処理を中断
            return;
        }
        
        _uiPresenter.HidePlayButton();
        // 選択されたカードを再取得
        var finalSelectedCard = _player.SelectedCard.CurrentValue;
        if (finalSelectedCard == null) return;
        
        _uiPresenter.UpdateEnemySprite(finalSelectedCard.Data.Attribute).Forget();
        
        // プレイヤーの手を作成
        var playStyle = _uiPresenter.GetSelectedPlayStyle();
        
        // 精神力を消費
        var mentalBet = _uiPresenter.GetMentalBetValue();
        _player.ConsumeMentalPower(mentalBet);
        _playerMove = new PlayerMove(finalSelectedCard.Data, playStyle, mentalBet);
        
        // 人格ログ: プレイヤームーブ記録
        _gameProgressService.LogPlayerMove(_playerMove, _player.MentalPower.CurrentValue);
        
        await UniTask.Delay(500);
        ChangeState(GameState.EnemyCardSelection);
    }
    
    /// <summary>
    /// 敵カード選択フェーズ
    /// </summary>
    private void HandleEnemyCardSelection()
    {
        NpcThinkAndSelectAsync().Forget();
    }
    
    /// <summary>
    /// NPCの思考と選択
    /// </summary>
    private async UniTask NpcThinkAndSelectAsync()
    {
        // 思考時間を待つ（NPCが考えているように見せる）
        await UniTask.Delay(1000);
        
        // AIでカードを選択
        var npcCard = _enemy.GetRandomCardFromHand();
        _enemy.SelectCard(npcCard);
        // NPCの手を作成（NPCもランダムなプレイスタイルと精神ベットを選択）
        var npcPlayStyle = (PlayStyle)UnityEngine.Random.Range(0, 3);
        var npcMentalBet = UnityEngine.Random.Range(1, Mathf.Min(6, _enemy.MentalPower.CurrentValue + 1)); // NPCの精神力範囲内でベット
        
        // NPCの精神力を消費
        _enemy.ConsumeMentalPower(npcMentalBet);
        _npcMove = new PlayerMove(npcCard.Data, npcPlayStyle, npcMentalBet);
        
        // 人格ログ: 敵ムーブ記録
        _gameProgressService.LogEnemyMove(_npcMove, _enemy.MentalPower.CurrentValue);
        
        // 結果表示の背景を表示
        await _uiPresenter.ShowBlackOverlay();
        
        // プレイヤーのカードプレイ前ナレーションを表示（実際の語り内容）
        var narrationContent = _cardNarrationService.GetNarration(_playerMove.SelectedCard, NarrationType.PrePlay, _playerMove.PlayStyle);
        var displayContent = string.IsNullOrEmpty(narrationContent) ? "..." : narrationContent;
        await _uiPresenter.ShowNarration(displayContent);
        
        // 少し間を置いてから評価フェーズに移行
        await UniTask.Delay(500);
        
        // 評価フェーズへ
        ChangeState(GameState.Evaluation);
    }
    
    /// <summary>
    /// 評価フェーズ
    /// </summary>
    private void HandleEvaluation()
    {
        if (_isProcessing) return; // 既に処理中の場合はスキップ
        EvaluationAsync(_playerMove, _npcMove).Forget();
    }
    
    /// <summary>
    /// 評価処理
    /// </summary>
    private async UniTask EvaluationAsync(PlayerMove playerMove, PlayerMove npcMove)
    {
        _isProcessing = true; // 処理開始フラグ
        
        // スコアを計算（テーマ倍率 × 精神ベット）
        var currentTheme = _currentTheme.CurrentValue;
        if (!currentTheme) return;
        
        var playerScore = ScoreCalculator.CalculateScore(playerMove, currentTheme);
        var npcScore = ScoreCalculator.CalculateScore(npcMove, currentTheme);
        
        // 評価結果をスコア専用Viewで同時表示
        await _uiPresenter.ShowScores(playerScore, npcScore);
        
        // カード崩壊判定
        _playerCollapse = CollapseJudge.ShouldCollapse(_playerMove);
        _npcCollapse = CollapseJudge.ShouldCollapse(_npcMove);

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
            var tasks = new List<UniTask> { _uiPresenter.ShowAnnouncement(collapseMessage, 1.0f) };

            if (_playerCollapse) tasks.Add(_player.CollapseSelectedCard());
            if (_npcCollapse) tasks.Add(_enemy.CollapseSelectedCard());

            await UniTask.WhenAll(tasks);
        }
        
        // 結果表示フェーズに移行
        await UniTask.Delay(500);
        _isProcessing = false; // フラグリセット
        ChangeState(GameState.ResultDisplay);
    }
    
    /// <summary>
    /// 勝敗表示フェーズ
    /// </summary>
    private void HandleResultDisplay()
    {
        if (_isProcessing) return; // 既に処理中の場合はスキップ
        ResultDisplayAsync().Forget();
    }
    
    /// <summary>
    /// 結果表示処理
    /// </summary>
    private async UniTask ResultDisplayAsync()
    {
        _isProcessing = true;
        
        var currentTheme = _currentTheme.CurrentValue;
        
        var playerScore = ScoreCalculator.CalculateScore(_playerMove, currentTheme);
        var npcScore = ScoreCalculator.CalculateScore(_npcMove, currentTheme);

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
        
        // 結果を表示
        await _uiPresenter.ShowWinLoseResult(result, playerWon);
        
        // 勝敗確定後のナレーション（プレイヤーの勝敗に基づく）
        var playerNarrationType = playerScore > npcScore ? NarrationType.PostBattleWin : NarrationType.PostBattleLose;
        var postBattleNarration = _cardNarrationService.GetNarration(_playerMove.SelectedCard, playerNarrationType, _playerMove.PlayStyle);
        var displayNarration = string.IsNullOrEmpty(postBattleNarration) ? "..." : postBattleNarration;
        await _uiPresenter.ShowNarration(displayNarration);
        
        // 敵の勝敗確定後のナレーション
        var enemyNarrationType = playerScore > npcScore ? NarrationType.PostBattleLoseEnemy : NarrationType.PostBattleWinEnemy;
        var enemyPostBattleNarration = _cardNarrationService.GetNarration(_playerMove.SelectedCard, enemyNarrationType, _playerMove.PlayStyle);
        var enemyDisplayNarration = string.IsNullOrEmpty(enemyPostBattleNarration) ? "..." : enemyPostBattleNarration;
        await _uiPresenter.ShowEnemyNarration(enemyDisplayNarration, 3f);
        
        // ゲーム結果を統計に記録（進化チェック前に実行）
        _gameProgressService.RecordPlayerGameResult(playerWon, _playerMove, _playerCollapse);
        _gameProgressService.EnemyStats.RecordGameResult(!playerWon, _npcMove, _npcCollapse);
        
        await _uiPresenter.HideBlackOverlay();

        // すべての場合で勝利数をカウントするように変更
        if (UpdateWinsAndCheckBattleEnd(playerWon))
        {
            // 3勝に達した場合、カード処理をスキップしてバトル終了へ
            _isProcessing = false;
            ChangeState(GameState.BattleEnd);
            return;
        }
        
        // 使用したカードをプレイ
        if (_playerCollapse)
        {
            // 崩壊処理前にカード情報を取得
            var playerCollapseCard = _player.SelectedCard.CurrentValue;
            
            // 実際の崩壊処理を実行
            await _player.CollapseSelectedCard();
            
            if (playerCollapseCard != null)
                _gameProgressService.LogCardCollapse("player", playerCollapseCard.Data);
        }
        else
        {
            // プレイヤーカードの進化処理（InstanceIdを保持）
            var selectedCard = _player.SelectedCard.CurrentValue;
            if (selectedCard != null)
            {
                // 進化チェック
                var playerCardAfterEvolution = _gameProgressService.CheckPlayerCardEvolution(selectedCard.Data);
                if (playerCardAfterEvolution != selectedCard.Data)
                {
                    // カードを進化後のデータで置換（InstanceIdは保持）
                    _player.ReplaceCard(selectedCard, playerCardAfterEvolution);
                    
                    // 人格ログ: プレイヤーカード進化イベント記録
                    _gameProgressService.LogCardEvolution("player", selectedCard.Data, playerCardAfterEvolution);
                    await _uiPresenter.ShowAnnouncement($"プレイヤーの {selectedCard.Data.CardName} が {playerCardAfterEvolution.CardName} に変化");
                }
                
                // 共鳴チェック
                if (_currentEnemyData && _currentEnemyData.ResonanceCard && selectedCard.Data == _currentEnemyData.ResonanceCard)
                {
                    // 人格ログ: 共鳴イベント記録
                    _gameProgressService.LogResonance("Player", selectedCard.Data);
                    await _uiPresenter.ShowAnnouncement($"共鳴発生: {selectedCard.Data.CardName}");
                }
                
                // カードをプレイ（崩壊しない）
                _player.PlaySelectedCard(false);
            }
        }

        if (_npcCollapse)
        {
            var npcCollapseCard = _enemy.SelectedCard.CurrentValue;
            // 実際の崩壊処理を実行
            _enemy.PlaySelectedCard(true);
            
            if (npcCollapseCard != null)
                _gameProgressService.LogCardCollapse("enemy", npcCollapseCard.Data);
        }
        else
        {
            // NPCカードの進化処理（InstanceIdを保持）
            var selectedEnemyCard = _enemy.SelectedCard.CurrentValue;
            if (selectedEnemyCard != null)
            {
                // 即時進化チェック
                var npcCardAfterEvolution = _gameProgressService.EnemyStats.CheckCardEvolution(selectedEnemyCard.Data);
                // 進化結果をアナウンス
                if (npcCardAfterEvolution != selectedEnemyCard.Data)
                {
                    // カードを進化後のデータで置換（InstanceIdは保持）
                    _enemy.ReplaceCard(selectedEnemyCard, npcCardAfterEvolution);
                    
                    // 人格ログ: NPCカード進化イベント記録
                    _gameProgressService.LogCardEvolution("enemy", selectedEnemyCard.Data, npcCardAfterEvolution);
                    await _uiPresenter.ShowAnnouncement($"対戦相手の {selectedEnemyCard.Data.CardName} が {npcCardAfterEvolution.CardName} に変化");
                }
                
                // カードをプレイ（崩壊しない）
                _enemy.PlaySelectedCard(false);
            }
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
        _gameProgressService.EndTurn();
        
        _isProcessing = false;
        
        // ゲームオーバー条件をチェック
        ChangeState(CheckGameOverConditions() ? GameState.GameOver : GameState.ThemeAnnouncement);
    }
    
    /// <summary>
    /// ゲームオーバー条件をチェック
    /// </summary>
    /// <returns>ゲームオーバー条件を満たしているかどうか</returns>
    private bool CheckGameOverConditions()
    {
        // プレイヤーの精神力が0になったか
        if (_player.MentalPower.CurrentValue <= 0)
        {
            return true;
        }
        
        // プレイヤーの全カードが崩壊したかどうか
        if (_player.IsAllCardsCollapsed)
        {
            return true;
        }
        
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
        
        // 3勝に達したかチェック
        return _playerWins >= WINS_TO_VICTORY || _enemyWins >= WINS_TO_VICTORY;
    }
    
    /// <summary>
    /// バトル終了フェーズ
    /// </summary>
    private void HandleBattleEnd()
    {
        if (_isProcessing) return;
        HandleBattleEndAsync().Forget();
    }
    
    /// <summary>
    /// バトル終了処理
    /// </summary>
    private async UniTask HandleBattleEndAsync()
    {
        _isProcessing = true;
        
        var battleResult = "";
        if (_playerWins >= WINS_TO_VICTORY)
            battleResult = $"バトルに勝利しました！ ({_playerWins}-{_enemyWins})";
        else if (_enemyWins >= WINS_TO_VICTORY)
            battleResult = $"バトルに敗北しました... ({_playerWins}-{_enemyWins})";
        
        await _uiPresenter.ShowAnnouncement(battleResult, 3f);
        
        // 人格ログ: チャプター完了
        _gameProgressService.CompleteChapter();
        
        // バトル結果をGameProgressServiceに報告してストーリー進行（セーブ前に実行）
        var playerWon = _playerWins >= WINS_TO_VICTORY;
        
        // プレイヤーのデッキ変更をセーブ（バトル終了時のみ）
        _player.SaveDeckChanges();
        
        var currentNode = _gameProgressService.GetCurrentNode();
        
        // 現在のバトル結果を記録
        _gameProgressService.UpdatePlayerMentalPower(_player.MentalPower.CurrentValue);
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
        
        _isProcessing = false;
    }
    
    /// <summary>
    /// バトル勝利数をリセット
    /// </summary>
    private void ResetBattleWins()
    {
        _playerWins = 0;
        _enemyWins = 0;
    }
    
    /// <summary>
    /// ゲームオーバーフェーズ
    /// </summary>
    private void HandleGameOver()
    {
        if (_isProcessing) return;
        HandleGameOverAsync().Forget();
    }
    
    /// <summary>
    /// ゲームオーバー処理
    /// </summary>
    private async UniTask HandleGameOverAsync()
    {
        _isProcessing = true;
        
        // ゲームオーバーの理由を判定
        var gameOverReason = "";
        if (_player.MentalPower.CurrentValue <= 0)
            gameOverReason = "精神力が0になりました";
        else if (_player.IsAllCardsCollapsed)
            gameOverReason = "すべてのカードが崩壊しました";
        
        // ゲームオーバー画面を表示
        await _uiPresenter.ShowGameOverScreen(gameOverReason);
    }
    
    /// <summary>
    /// 遅延してステート変更
    /// </summary>
    private async UniTask DelayedStateChangeAsync(GameState newState, float delay)
    {
        await UniTask.Delay((int)(delay * 1000));
        ChangeState(newState);
    }
    
    /// <summary>
    /// リソースの解放
    /// </summary>
    public void Dispose()
    {
        _disposables?.Dispose();
        _currentState?.Dispose();
        _currentTheme?.Dispose();
    }
}
