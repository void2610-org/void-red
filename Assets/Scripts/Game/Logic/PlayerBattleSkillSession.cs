using Cysharp.Threading.Tasks;
using R3;

/// <summary>
/// 1ラウンド中のプレイヤースキル発動フローを管理する
/// </summary>
public sealed class PlayerBattleSkillSession : System.IDisposable
{
    public bool ShouldApplyDeferredSkill { get; private set; }

    private readonly BattleUIPresenter _battleUIPresenter;
    private readonly CardBattleHandler _handler;
    private readonly BattleDeckModel _playerDeck;
    private readonly EmotionType _playerSkill;
    private readonly CompositeDisposable _disposables = new();

    private bool _isSkillActivatedBeforePlacement;

    public PlayerBattleSkillSession(
        BattleUIPresenter battleUIPresenter,
        CardBattleHandler handler,
        BattleDeckModel playerDeck,
        EmotionType playerSkill)
    {
        _battleUIPresenter = battleUIPresenter;
        _handler = handler;
        _playerDeck = playerDeck;
        _playerSkill = playerSkill;
    }

    /// <summary>現在のスキルが対象選択UIを必要とするかを返す</summary>
    private bool RequiresTargetSelection() => _playerSkill == EmotionType.Sadness;

    /// <summary>
    /// ラウンド中のスキル監視を開始する
    /// </summary>
    public void BeginListening()
    {
        _battleUIPresenter.OnBattleFieldCardChanged
            .Subscribe(OnBattleFieldCardChanged)
            .AddTo(_disposables);

        _battleUIPresenter.OnSkillActivated
            .Subscribe(_ => OnSkillActivated())
            .AddTo(_disposables);
    }

    /// <summary>
    /// カード確定後に予約済みスキルを処理する
    /// </summary>
    public async UniTask CompleteCardPlacementAsync()
    {
        if (!_isSkillActivatedBeforePlacement) return;

        if (BattleSkillExecutor.ShouldDeferUntilReveal(_playerSkill))
        {
            // Fearは開示直前まで適用を遅らせる
            ShouldApplyDeferredSkill = true;
            return;
        }

        await ApplySkillAsync(_handler.PlayerCard);
    }

    /// <summary>
    /// 開示直前まで遅らせたスキルを適用する
    /// </summary>
    public void ApplyDeferredSkill()
    {
        if (!ShouldApplyDeferredSkill) return;

        BattleSkillExecutor.Execute(_playerSkill, _handler.PlayerCard, _handler.EnemyCard, _playerDeck, _handler, isPlayerSide: true);
        _battleUIPresenter.RefreshBattleCardNumbers();
        ShouldApplyDeferredSkill = false;
    }

    /// <summary>
    /// 仮置きカード変更時に予約中の補正表示を更新する
    /// </summary>
    private void OnBattleFieldCardChanged(CardModel card)
    {
        _handler.PreviewPlayerNextCardEffects(card);
        _battleUIPresenter.RefreshBattleCardNumbers();
    }

    /// <summary>
    /// スキルボタン押下時に、選択状況に応じた処理へ振り分ける
    /// </summary>
    private void OnSkillActivated()
    {
        var selectedBattleCard = _battleUIPresenter.SelectedBattleCard;
        if (selectedBattleCard != null)
        {
            ActivateWithSelectedCard(selectedBattleCard);
            return;
        }

        ActivateWithoutSelectedCard();
    }

    /// <summary>
    /// カード仮置き後に押されたスキルを処理する
    /// </summary>
    private void ActivateWithSelectedCard(CardModel selectedBattleCard)
    {
        if (!_handler.TryConsumePlayerSkill()) return;

        if (RequiresTargetSelection()) ApplySkillAsync(selectedBattleCard).Forget();
        else if (BattleSkillExecutor.ShouldDeferUntilReveal(_playerSkill))
        {
            _isSkillActivatedBeforePlacement = true;
        }
        else ApplySkillAsync(selectedBattleCard).Forget();

        NotifySkillActivated(false);
    }

    /// <summary>
    /// カード未選択で押されたスキルを即時発動または予約する
    /// </summary>
    private void ActivateWithoutSelectedCard()
    {
        if (!_handler.TryConsumePlayerSkill()) return;

        if (RequiresTargetSelection()) ApplySkillAsync(null).Forget();
        else if (BattleSkillExecutor.CanActivateWithoutSelectedCard(_playerSkill))
            ApplySkillAsync(null).Forget();
        else
        {
            // カード依存スキルはカード確定後まで予約する
            _isSkillActivatedBeforePlacement = true;
        }

        NotifySkillActivated(true);
    }

    /// <summary>
    /// 現在の状況に対してスキル効果を適用し、表示を更新する
    /// </summary>
    private async UniTask ApplySkillAsync(CardModel selectedBattleCard)
    {
        var targetCardForSadness = await SelectTargetCardAsync();

        if (_playerSkill == EmotionType.Anticipation && selectedBattleCard != null)
        {
            // 仮置きしたカード自身は「残りカード」に含めない
            selectedBattleCard.IsUsed = true;
        }

        BattleSkillExecutor.Execute(
            _playerSkill,
            selectedBattleCard,
            _handler.EnemyCard,
            _playerDeck,
            _handler,
            isPlayerSide: true,
            targetCardForSadness);

        if (_playerSkill == EmotionType.Anticipation && selectedBattleCard != null)
            selectedBattleCard.IsUsed = false;

        _handler.PreviewPlayerNextCardEffects(selectedBattleCard);
        _battleUIPresenter.RefreshBattleCardNumbers();
    }

    /// <summary>対象選択が必要なスキルなら、選択UIを開いて結果カードを返す</summary>
    private async UniTask<CardModel> SelectTargetCardAsync()
    {
        if (!RequiresTargetSelection()) return null;

        return await _battleUIPresenter.WaitForTargetCardSelectionAsync("カードを選択してください", _playerDeck.GetAvailableCards());
    }

    /// <summary>
    /// スキル発動後のUI表示とログ出力を行う
    /// </summary>
    private void NotifySkillActivated(bool isBeforePlacement)
    {
        _battleUIPresenter.SetSkillButtonVisible(false);
        _battleUIPresenter.SetBattleInstruction($"{_playerSkill.ToJapaneseName()}スキル発動！");

        var suffix = isBeforePlacement ? "（カード選択前）" : "";
        UnityEngine.Debug.Log($"[PlayerBattleSkillSession] プレイヤーがスキル発動{suffix}: {_playerSkill}");
    }

    /// <summary>
    /// ラウンド中に購読したイベントを解放する
    /// </summary>
    public void Dispose() => _disposables.Dispose();
}
