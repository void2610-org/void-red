using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// キャラクター記憶ビュー
/// 右側パネルでキャラクターとカードを表示
/// </summary>
public class MemoryDetailView : BaseWindowView
{
    [Header("カード表示")]
    [SerializeField] private Transform cardContainer;
    [SerializeField] private MemoryCardItemView cardItemPrefab;
    [SerializeField] private Transform centerThemeImage; // 中央のテーマ表示Image

    [Header("円形配置パラメータ")]
    [SerializeField] private float radiusX = 300f;
    [SerializeField] private float radiusY = 100f;
    [SerializeField] private float minScale = 0.7f;
    [SerializeField] private float maxScale = 1.0f;

    private readonly List<MemoryCardItemView> _cardViews = new();

    /// <summary>
    /// 既存テーマのカードを表示（リストクリック時）
    /// </summary>
    public async UniTask ShowAndWaitClose(AcquiredTheme theme)
    {
        foreach (var cardView in _cardViews.Where(cardView => cardView)) Destroy(cardView.gameObject);
        _cardViews.Clear();

        ShowCardsInCircle(theme.AcquiredCards);
        base.Show();

        await closeButton.OnClickAsync();
        base.Hide();
    }

    /// <summary>
    /// カードを円形に配置して表示
    /// </summary>
    private void ShowCardsInCircle(IReadOnlyList<CardModel> cards)
    {
        var count = cards.Count;
        if (count == 0) return;

        // 円形配置の計算用リスト（深度でソート用）
        var cardPositions = new List<(int index, float angle, float depth)>();

        // 2アイテムの場合は左右配置（0から開始）、それ以外は上から開始
        var startAngle = count == 2 ? 0f : Mathf.PI / 2f;

        for (var i = 0; i < count; i++)
        {
            var angle = startAngle - (2f * Mathf.PI / count) * i;
            // 深度: sin(angle)が-1（下）のとき手前、1（上）のとき奥
            // normalizedDepth: 0=手前, 1=奥
            var normalizedDepth = (Mathf.Sin(angle) + 1f) / 2f;
            cardPositions.Add((i, angle, normalizedDepth));
        }

        // 奥から手前順にソート（描画順用）
        var sortedByDepth = cardPositions.OrderByDescending(p => p.depth).ToList();

        // カード生成と配置
        for (var i = 0; i < count; i++)
        {
            var cardView = Instantiate(cardItemPrefab, cardContainer);
            cardView.Initialize(cards[i]);
            _cardViews.Add(cardView);
        }

        // 描画順: 奥側カード → centerThemeImage → 手前側カード
        var backCardCount = sortedByDepth.Count(p => p.depth > 0.5f);
        var backIndex = 0;
        var frontIndex = 0;

        foreach (var (index, angle, depth) in sortedByDepth)
        {
            var cardView = _cardViews[index];

            // 楕円上の位置を計算
            var x = radiusX * Mathf.Cos(angle);
            var y = radiusY * Mathf.Sin(angle);
            var position = new Vector2(x, y);

            // スケール: 奥（depth=1）が小さく、手前（depth=0）が大きい
            var scale = Mathf.Lerp(maxScale, minScale, depth);

            // 奥側（depth > 0.5）はcenterより前、手前側はcenterより後
            var sortOrder = depth > 0.5f
                ? backIndex++
                : backCardCount + 1 + frontIndex++;

            cardView.SetCircularPosition(position, scale, sortOrder);
        }

        // centerThemeImageを奥側カードと手前側カードの間に配置
        centerThemeImage.SetSiblingIndex(backCardCount);
    }
}
