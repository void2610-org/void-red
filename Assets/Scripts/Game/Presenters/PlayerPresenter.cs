using System;
using System.Collections.Generic;
using R3;

/// <summary>
/// プレイヤーとNPCの基底プレゼンタークラス
/// 感情リソース管理とオークション関連データを統合
/// </summary>
public abstract class PlayerPresenter : IDisposable
{
    // === 公開プロパティ ===

    public IReadOnlyDictionary<EmotionType, int> EmotionResources => _playerModel.EmotionResources;
    public IReadOnlyList<CardModel> Cards => _cards;
    public BidModel Bids => _bids;
    public IReadOnlyList<CardModel> WonCards => _wonCards;
    public ReadOnlyReactiveProperty<float> PainLevel => _painLevel;
    public ReadOnlyReactiveProperty<float> DilutionLevel => _dilutionLevel;

    // === プライベートフィールド ===

    private readonly PlayerModel _playerModel;
    private readonly GameProgressService _gameProgressService;

    private readonly List<CardModel> _cards = new();
    private readonly BidModel _bids = new();
    private readonly List<CardModel> _wonCards = new();
    private readonly ReactiveProperty<float> _painLevel = new(0f);
    private readonly ReactiveProperty<float> _dilutionLevel = new(0f);

    protected PlayerPresenter(GameProgressService gameProgressService = null)
    {
        _playerModel = new PlayerModel();
        _gameProgressService = gameProgressService;
    }

    // === 感情リソース管理 ===

    public bool TryConsumeEmotion(EmotionType emotion, int amount)
        => _playerModel.TryConsumeEmotion(emotion, amount);

    public void AddEmotion(EmotionType emotion, int amount)
        => _playerModel.AddEmotion(emotion, amount);

    public int GetEmotionAmount(EmotionType emotion)
        => _playerModel.GetEmotionAmount(emotion);

    // === カード管理 ===

    public void AddWonCard(CardModel card) => _wonCards.Add(card);

    public void ClearWonCards() => _wonCards.Clear();

    // === 状態ゲージ管理 ===
    public void SetPainLevel(float value) => _painLevel.Value = UnityEngine.Mathf.Clamp01(value);
    public void SetDilutionLevel(float value) => _dilutionLevel.Value = UnityEngine.Mathf.Clamp01(value);

    public void SetCards(IEnumerable<CardData> cardDataList)
    {
        _cards.Clear();
        foreach (var cardData in cardDataList)
        {
            _cards.Add(new CardModel(cardData));
        }
    }

    // === 状態管理 ===

    public virtual void InitializeAuctionData()
    {
        _cards.Clear();
        _bids.Clear();
        _wonCards.Clear();
    }

    public void ResetPlayerState()
    {
        _playerModel.ResetEmotionResources();
        InitializeAuctionData();
    }

    public virtual void Dispose()
    {
        _playerModel.Dispose();
        _painLevel.Dispose();
        _dilutionLevel.Dispose();
    }
}
