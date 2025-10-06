using TMPro;
using UnityEngine;
using LitMotion;
using LitMotion.Extensions;
using Void2610.UnityTemplate;

/// <summary>
/// 単一キーワードを表示するViewクラス
/// フェードアニメーション機能を提供
/// </summary>
[RequireComponent(typeof(TextMeshProUGUI))]
public class KeywordTextView : MonoBehaviour
{
    private TextMeshProUGUI _keywordText;
    private MotionHandle _fadeMotion;

    /// <summary>
    /// キーワードを設定
    /// </summary>
    /// <param name="keyword">表示するキーワード</param>
    public void SetKeyword(KeywordType keyword)
    {
        _keywordText.text = keyword.GetJapaneseName();

        // 初期状態は完全に透明
        var color = _keywordText.color;
        color.a = 0f;
        _keywordText.color = color;
    }

    /// <summary>
    /// フェードイン処理
    /// </summary>
    public void FadeIn()
    {
        _fadeMotion.Cancel();
        _fadeMotion = _keywordText.FadeIn(0.3f);
    }

    /// <summary>
    /// フェードアウト処理
    /// </summary>
    public void FadeOut()
    {
        _fadeMotion.Cancel();
        _fadeMotion = _keywordText.FadeOut(0.3f);
    }
    
    private void Awake()
    {
        _keywordText = GetComponent<TextMeshProUGUI>();
    }
}
