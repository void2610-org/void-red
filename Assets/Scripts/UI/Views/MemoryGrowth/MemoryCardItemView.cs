using UnityEngine;

/// <summary>
/// CardViewをラップして円形配置用の機能を提供するView
/// </summary>
public class MemoryCardItemView : MonoBehaviour
{
    [SerializeField] private CardView cardView;

    private RectTransform _rectTransform;

    /// <summary>
    /// カード情報で初期化
    /// </summary>
    public void Initialize(CardAcquisitionInfo cardInfo) => cardView.Initialize(cardInfo.Card.Data);

    /// <summary>
    /// CardModelで初期化（既存のAcquiredCardsリスト用）
    /// </summary>
    public void Initialize(CardModel cardModel) => cardView.Initialize(cardModel.Data);

    /// <summary>
    /// 円形配置用の位置・スケール・描画順を設定
    /// </summary>
    public void SetCircularPosition(Vector2 position, float scale, int sortOrder)
    {
        _rectTransform.anchoredPosition = position;
        _rectTransform.localScale = Vector3.one * scale;
        transform.SetSiblingIndex(sortOrder);
    }

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
    }
}
