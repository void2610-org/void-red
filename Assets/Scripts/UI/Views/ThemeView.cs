using TMPro;
using UnityEngine;
using LitMotion;
using LitMotion.Extensions;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine.EventSystems;
using Void2610.UnityTemplate;

/// <summary>
/// テーマ表示を担当するViewクラス
/// マウスオーバー時にキーワードを表示
/// </summary>
public class ThemeView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private TextMeshProUGUI themeText;
    [SerializeField] private KeywordTextView keywordTextPrefab;
    [SerializeField] private Transform keywordContainer;

    private readonly List<KeywordTextView> _keywordViews = new();

    /// <summary>
    /// テーマとキーワードを表示
    /// </summary>
    /// <param name="themeData">テーマデータ</param>
    public void DisplayThemeWithKeywords(ThemeData themeData)
    {
        // テーマタイトルを表示
        themeText.TypewriterAnimation(themeData.Title, skipOnClick:false).Forget();

        // 既存のキーワードViewをクリア
        ClearKeywords();

        // キーワードViewを生成
        foreach (var keyword in themeData.Keywords)
        {
            var keywordView = Instantiate(keywordTextPrefab, keywordContainer);
            var pos = Random.insideUnitCircle * 250f; // ランダムな位置に配置
            keywordView.transform.localPosition = new Vector3(pos.x, pos.y, 0);
            keywordView.SetKeyword(keyword);
            _keywordViews.Add(keywordView);
        }
    }

    /// <summary>
    /// キーワードViewをクリア
    /// </summary>
    private void ClearKeywords()
    {
        foreach (var view in _keywordViews)
            if (view) Destroy(view.gameObject);
        _keywordViews.Clear();
    }

    /// <summary>
    /// マウスカーソルが乗ったときの処理
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        foreach (var view in _keywordViews)
            view.FadeIn();
    }

    /// <summary>
    /// マウスカーソルが離れたときの処理
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        foreach (var view in _keywordViews)
            view.FadeOut();
    }

    private void OnDestroy()
    {
        ClearKeywords();
    }
}