using System.Collections.Generic;
using R3;
using Cysharp.Threading.Tasks;
using System;

/// <summary>
/// プレイヤーとNPCの基底プレゼンタークラス（Presenter Layer）
/// カード管理・UI制御・ゲームロジックを統合
/// </summary>
public abstract class PlayerPresenter : IDisposable
{
    // 公開プロパティ
    public ReadOnlyReactiveProperty<CardModel> SelectedCard => _handModel.SelectedCard;
    public ReadOnlyReactiveProperty<int> MentalPower => _playerModel.MentalPower;
    public ReadOnlyReactiveProperty<int> SelectedIndex => _handModel.SelectedIndex;
    public int HandCount => _handModel.Count;
    public int MaxHandSize => _handModel.MaxHandSize;
    public DeckModel DeckModel => _deckModel;
    
    // プライベートフィールド
    private readonly PlayerModel _playerModel;
    private readonly HandModel _handModel;
    private DeckModel _deckModel;
    private readonly HandView _handView;
    private readonly CompositeDisposable _disposables = new();
    private readonly GameProgressService _gameProgressService;
    
    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="handView">手札ビュー</param>
    /// <param name="gameProgressService">ゲーム進行サービス（オプショナル）</param>
    /// <param name="maxHandSize">最大手札数</param>
    protected PlayerPresenter(HandView handView, GameProgressService gameProgressService = null, int maxHandSize = 3)
    {
        _playerModel = new PlayerModel();
        _handModel = new HandModel(maxHandSize);
        _handView = handView;
        _gameProgressService = gameProgressService;
        
        // UI制御の設定
        _handView.BindModel(_handModel);
        
        // UIイベントを購読
        _handView.OnCardClicked
            .Subscribe(_handModel.SelectCardAt)
            .AddTo(_disposables);
    }
    
    // === デッキ・手札操作 ===
    
    /// <summary>
    /// デッキを初期化
    /// </summary>
    public void InitializeDeck(List<CardData> cardDataList)
    {
        _deckModel = new DeckModel(cardDataList);
        SaveDeckChanges();
    }
    
    /// <summary>
    /// セーブデータからデッキを復元
    /// </summary>
    /// <returns>復元に成功したかどうか</returns>
    public bool LoadDeckFromSaveData()
    {
        var savedCardModels = _gameProgressService.ConvertDeckToCardModels();
        if (savedCardModels.Count > 0)
        {
            _deckModel = new DeckModel(savedCardModels);
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// デッキが空かどうか（山札のみ）
    /// </summary>
    public bool IsDeckEmpty => _deckModel?.IsEmpty ?? true;
    
    /// <summary>
    /// 使用可能なカードが全て崩壊したかどうか（ゲームオーバー条件）
    /// </summary>
    public bool IsAllCardsCollapsed => _deckModel?.IsActiveCardsEmpty ?? true;
    
    public async UniTask DrawCardsWithDelay(int count, int intervalMs)
    {
        for (var i = 0; i < count; i++)
        {
            DrawCard();
            // 最後のカード以外は間隔を挟む
            if (i < count - 1) await UniTask.Delay(intervalMs);
        }
        
        // 最後のカードのアニメーション完了を待つ
        await UniTask.Delay(500);
    }
    
    /// <summary>
    /// カードを引く（1枚ずつの処理に最適化）
    /// </summary>
    public void DrawCard(int count = 1)
    {
        for (var i = 0; i < count; i++)
        {
            if (IsDeckEmpty || HandCount >= MaxHandSize)
                break;
                
            var cardModel = _deckModel?.DrawCard();
            if (cardModel == null || !_handModel.TryAddCard(cardModel))
            {
                // 手札に追加できなかった場合はデッキに戻す
                if (cardModel != null) _deckModel?.ReturnCard(cardModel);
                break;
            }
        }
    }
    
    /// <summary>
    /// カードが引けるかどうかチェック
    /// </summary>
    /// <param name="count">引こうとする枚数</param>
    /// <returns>引けるかどうか</returns>
    public bool CanDrawCards(int count = 1)
    {
        return !IsDeckEmpty && HandCount + count <= MaxHandSize;
    }
    
    // === カード選択・操作 ===
    
    /// <summary>
    /// ランダムなカードを手札から取得 (敵の行動用)
    /// </summary>
    public CardModel GetRandomCardFromHand() => _handModel.GetRandomCard();
    
    /// <summary>
    /// カードを選択
    /// </summary>
    public void SelectCard(CardModel cardModel) => _handModel.SelectCard(cardModel);
    
    /// <summary>
    /// インデックスでカードを選択
    /// </summary>
    public void SelectCardAt(int index) => _handModel.SelectCardAt(index);
    
    /// <summary>
    /// カード選択を解除
    /// </summary>
    public void DeselectCard() => _handModel.DeselectCard();
    
    /// <summary>
    /// ランダムなカードを選択
    /// </summary>
    /// <returns>選択に成功したかどうか</returns>
    public bool SelectRandomCard()
    {
        var randomCard = _handModel.GetRandomCard();
        if (randomCard == null) return false;
        
        _handModel.SelectCard(randomCard);
        return true;
    }
    
    // === カードプレイ ===
    
    /// <summary>
    /// 選択されたカードをプレイ
    /// </summary>
    /// <param name="shouldCollapse">崩壊させるかどうか</param>
    /// <returns>プレイに成功したかどうか</returns>
    public bool PlaySelectedCard(bool shouldCollapse)
    {
        var selectedCard = SelectedCard.CurrentValue;
        if (selectedCard == null) return false;
        
        // 手札から削除
        if (!_handModel.TryRemoveCard(selectedCard)) return false;
        
        if (shouldCollapse)
        {
            // 崩壊する場合はActiveCardsから削除してCollapsedCardsに移動
            _deckModel.CollapseCard(selectedCard);
        }
        else
        {
            // 崩壊しない場合はデッキに戻す
            _deckModel.ReturnCard(selectedCard);
        }
        
        // デッキの変更をセーブ
        SaveDeckChanges();
        
        return true;
    }
    
    /// <summary>
    /// 選択されたカードを手札から削除（デッキに戻さない）
    /// </summary>
    /// <returns>削除されたカードデータ</returns>
    public CardData RemoveSelectedCard()
    {
        var selectedCard = SelectedCard.CurrentValue;
        if (selectedCard == null) return null;
        
        // 手札から削除
        if (_handModel.TryRemoveCard(selectedCard))
            return selectedCard.Data;
        
        return null;
    }
    
    /// <summary>
    /// 指定したカードをデッキに戻す
    /// </summary>
    /// <param name="card">戻すカード</param>
    public void ReturnCardToDeck(CardData card)
    {
        if (card)
        {
            // 新しいCardModelを作成してデッキに戻す
            var cardModel = new CardModel(card);
            _deckModel?.ReturnCard(cardModel);
            SaveDeckChanges();
        }
    }
    
    /// <summary>
    /// 指定したカードをプレイ
    /// </summary>
    /// <param name="card">プレイするカード</param>
    /// <param name="shouldCollapse">崩壊させるかどうか</param>
    /// <returns>プレイに成功したかどうか</returns>
    public bool PlayCard(CardModel card, bool shouldCollapse)
    {
        if (!_handModel.HasCard(card)) return false;
        
        // カードを選択してからプレイ
        _handModel.SelectCard(card);
        return PlaySelectedCard(shouldCollapse);
    }
    
    /// <summary>
    /// カードがプレイ可能かどうかチェック
    /// </summary>
    /// <param name="card">チェックするカード</param>
    /// <returns>プレイ可能かどうか</returns>
    public bool CanPlayCard(CardModel card) => _handModel.HasCard(card);
    
    /// <summary>
    /// 選択したカードを崩壊させる（UI制御付き）
    /// </summary>
    public async UniTask CollapseSelectedCard()
    {
        var selectedIndex = SelectedIndex.CurrentValue;
        
        // ビジネスロジック実行
        if (PlaySelectedCard(true))
        {
            // 崩壊アニメーションを再生
            if (selectedIndex >= 0)
            {
                await _handView.PlayCollapseAnimation(selectedIndex); // awaitで待機
            }
        }
    }
    
    // === 精神力管理 ===
    
    /// <summary>
    /// 精神力を消費
    /// </summary>
    /// <param name="amount">消費量</param>
    /// <returns>消費に成功したかどうか</returns>
    public bool ConsumeMentalPower(int amount) => _playerModel.TryConsumeMentalPower(amount);
    
    /// <summary>
    /// 精神力を回復
    /// </summary>
    /// <param name="amount">回復量</param>
    public void RestoreMentalPower(int amount) => _playerModel.RestoreMentalPower(amount);
    
    /// <summary>
    /// 精神力が足りるかチェック
    /// </summary>
    /// <param name="requiredAmount">必要な精神力</param>
    /// <returns>足りるかどうか</returns>
    public bool HasEnoughMentalPower(int requiredAmount) => _playerModel.MentalPower.CurrentValue >= requiredAmount;
    
    /// <summary>
    /// 精神力を設定
    /// </summary>
    /// <param name="value">設定する精神力</param>
    public void SetMentalPower(int value) => _playerModel.SetMentalPower(value);
    
    // === セーブデータ管理 ===
    
    /// <summary>
    /// カードを進化・劣化で置き換え
    /// </summary>
    /// <param name="oldCard">置き換える元のカード</param>
    /// <param name="newCard">新しいカード</param>
    /// <returns>置き換えに成功したかどうか</returns>
    public bool ReplaceCard(CardModel oldCard, CardData newCard)
    {
        if (_deckModel == null) return false;
        
        var success = _deckModel.ReplaceCard(oldCard, newCard);
        if (success)
        {
            SaveDeckChanges();
        }
        return success;
    }
    
    /// <summary>
    /// デッキの変更をGameProgressServiceに通知してセーブ
    /// </summary>
    protected void SaveDeckChanges()
    {
        if (_gameProgressService != null && _deckModel != null)
        {
            // 全カード（AllCards）をセーブデータに反映
            _gameProgressService.UpdateDeckFromCardModels(_deckModel.AllCards);
        }
    }
    
    /// <summary>
    /// デッキ表示用のカード情報を取得
    /// </summary>
    public (List<CardModel> allCards, List<CardModel> activeCards, List<CardModel> collapsedCards) GetDeckDisplayData()
    {
        if (_deckModel == null)
        {
            return (new List<CardModel>(), new List<CardModel>(), new List<CardModel>());
        }
        
        return (_deckModel.AllCards, _deckModel.ActiveCards, _deckModel.CollapsedCards);
    }
    
    // === 手札・デッキリセット ===
    
    /// <summary>
    /// 手札をデッキに戻す（UI制御付き）
    /// </summary>
    public async UniTask ReturnHandToDeck()
    {
        // デッキに戻すアニメーション
        await _handView.ReturnCardsToDeck();
        
        // ビジネスロジック実行
        ReturnAllHandToDeck();
    }
    
    /// <summary>
    /// 手札を全てデッキに戻す
    /// </summary>
    /// <returns>戻したカードのリスト</returns>
    private void ReturnAllHandToDeck()
    {
        var handCards = _handModel.GetAllCards();
        _handModel.Clear();
        _deckModel?.ReturnCards(handCards);
    }
    
    /// <summary>
    /// プレイヤーの状態をリセット
    /// </summary>
    public void ResetPlayerState()
    {
        _handModel.DeselectCard();
        _handModel.Clear();
        _playerModel.SetMentalPower(GameConstants.MAX_MENTAL_POWER);
    }
    
    // === UI制御 ===
    
    /// <summary>
    /// 手札のインタラクション状態を設定
    /// </summary>
    public void SetHandInteractable(bool interactable) => _handView.SetInteractable(interactable);
    
    /// <summary>
    /// リソースの解放
    /// </summary>
    public virtual void Dispose()
    {
        _disposables?.Dispose();
        _playerModel?.Dispose();
    }
}