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
    private readonly GameProgressService _gameProgressService;
    private readonly SceneTransitionManager _sceneTransitionManager;
    private readonly AllEnemyData _allEnemyData;
    private readonly DiscordService _discordService;

    private readonly UniTaskCompletionSource _initializationComplete = new();

    private EnemyData _currentEnemyData;
    private ThemeData _currentTheme;
    
    private int _playerWins;
    private int _enemyWins;
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
        GameProgressService gameProgressService,
        SceneTransitionManager sceneTransitionManager,
        AllEnemyData allEnemyData,
        DiscordService discordService)
    {
        _battleUIPresenter = battleUIPresenter;
        _player = player;
        _enemy = enemy;
        _gameProgressService = gameProgressService;
        _sceneTransitionManager = sceneTransitionManager;
        _allEnemyData = allEnemyData;
        _discordService = discordService;

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
        await UniTask.Delay(500);
        
        // GameProgressServiceから敵データを取得
        var currentNode = _gameProgressService.GetCurrentNode();
        if (currentNode is not BattleNode battleNode)
        {
            Debug.LogError("[GameManager] 現在のノードがBattleNodeではありません");
            await _sceneTransitionManager.TransitionToSceneWithFade(SceneType.Home);
            return;
        }
        
        _currentEnemyData = _allEnemyData.GetEnemyById(battleNode.EnemyId);
        if (!_currentEnemyData)
        {
            Debug.LogError("[GameManager] 敵データが見つかりません");
            await _sceneTransitionManager.TransitionToSceneWithFade(SceneType.Home);
            return;
        }
        
        // Discord Rich Presence更新（バトル開始）
        _discordService?.SetState("対戦相手", _currentEnemyData.EnemyName);
        
        // 敵を初期化して表示
        _enemy.SetEnemyData(_currentEnemyData);
        _battleUIPresenter.InitializeEnemy(_currentEnemyData);
        await _battleUIPresenter.ShowEnemy();
        
        // 敵情報をアナウンス
        await _battleUIPresenter.ShowAnnouncement(_currentEnemyData.EnemyName, 1.5f);
        
    }

    private async UniTask StartGame()
    {
        await UniTask.Delay(1000);
        
        // _player.DrawCardsWithDelay(3, 300).Forget();
        await UniTask.Delay(200);
        // await _enemy.DrawCardsWithDelay(3, 300);

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
        }
    }
    
    /// <summary>
    /// お題発表フェーズ
    /// </summary>
    private async UniTask HandleThemeAnnouncement()
    {
        _currentTurnNumber++;

        // ターン番号に基づいてテーマを順番に選択（1ターン目 = index 0, 2ターン目 = index 1, 3ターン目 = index 2）
        // _currentTheme = _currentEnemyData.Themes[_currentTurnNumber - 1];

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
        if (!_currentTheme)
        {
            await UniTask.Delay(300);
            ChangeState(GameState.PlayerCardSelection).Forget();
            return;
        }

        // 各会話を順次表示
        // foreach (var dialogue in _currentTheme.Dialogues)
        // {
        //     if (string.IsNullOrEmpty(dialogue.Message)) continue;
        //
        //     if (dialogue.IsPlayer)
        //         await _battleUIPresenter.ShowPlayerNarration(dialogue.Message, autoAdvance: true);
        //     else
        //         await _battleUIPresenter.ShowEnemyNarration(dialogue.Message, autoAdvance: true);
        // }

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
            
            // カードが選択されたらプレイボタンを有効化
            _battleUIPresenter.SetPlayButtonInteractable(true);
            _battleUIPresenter.SetCardDetailButtonInteractable(true);
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
        _battleUIPresenter.SetCardDetailButtonInteractable(false);
        
        // _battleUIPresenter.UpdateEnemySprite(finalSelectedCard.Data.Attribute).Forget();
        
        // 精神力を消費
        var mentalBet = _battleUIPresenter.GetMentalBetValue();
        _player.ConsumeMentalPower(mentalBet);
        // _playerMove = new PlayerMove(finalSelectedCard.Data, playStyle, mentalBet);
        
        // 選択されたカードを閲覧済みとして記録
        // _gameProgressService.RecordCardView(finalSelectedCard.Data);
        
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
        // _enemy.SelectCard(npcCard);
        // NPCの手を作成（NPCもランダムなプレイスタイルと精神ベットを選択）
        var npcMentalBet = UnityEngine.Random.Range(1, Mathf.Min(6, _enemy.MentalPower.CurrentValue + 1)); // NPCの精神力範囲内でベット
        
        // NPCの精神力を消費
        _enemy.ConsumeMentalPower(npcMentalBet);
        // _enemyMove = new PlayerMove(npcCard.Data, npcPlayStyle, npcMentalBet);
        
        // 敵のカードも閲覧済みとして記録
        // _gameProgressService.RecordCardView(npcCard.Data);
        
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
        // 評価結果をスコア専用Viewで同時表示
        // await _battleUIPresenter.ShowScores(playerScore, npcScore);
        
        // 結果表示フェーズに移行
        await UniTask.Delay(500);
        ChangeState(GameState.ResultDisplay).Forget();
    }
    
    /// <summary>
    /// 勝敗表示フェーズ
    /// </summary>
    private async UniTask HandleResultDisplay()
    {

        // スコア引き分けもランダム決着
        var playerWon = UnityEngine.Random.Range(0, 2) == 0;
        var result = playerWon ? "あなたの勝利\n（引き分け→ランダム決着）" : "相手の勝利\n（引き分け→ランダム決着）";

        // 結果を表示（スコアと内訳付き）
        // await _battleUIPresenter.ShowWinLoseResult(result, playerWon, playerScore, npcScore, _playerMove, _enemyMove, _currentTheme);
        await UniTask.Delay(500);
        await _battleUIPresenter.HideBlackOverlay();

        // プレイヤー勝利時は現在のテーマを記録
        if (playerWon) _wonThemes.Add(_currentTheme);

        // すべての場合で勝利数をカウントするように変更
        if (UpdateWinsAndCheckBattleEnd(playerWon))
        {
            // 3勝に達した場合、カード処理をスキップしてバトル終了へ
            ChangeState(GameState.BattleEnd).Forget();
            return;
        }
        
        // カードをプレイ（崩壊しない）
        // _player.PlaySelectedCard(false);
        // カードをプレイ（崩壊しない）
        // _enemy.PlaySelectedCard(false);
        
        // カード使用後の処理完了を待つ
        await UniTask.Delay(1000);
        
        // 両プレイヤーの手札をデッキに戻す
        // await UniTask.WhenAll(_player.ReturnHandToDeck(), _enemy.ReturnHandToDeck());
        
        // _player.DrawCardsWithDelay(3, 300).Forget();
        await UniTask.Delay(200);
        // await _enemy.DrawCardsWithDelay(3, 300);
        
        // 新しいラウンドの準備時間
        await UniTask.Delay(1000);
        ChangeState(GameState.ThemeAnnouncement).Forget();
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
        
        // プレイヤーのデッキ変更をセーブ（バトル終了時のみ）
        // _player.SaveDeckChanges();
        
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
}
