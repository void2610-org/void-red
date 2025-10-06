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

    public void SetKeyword(KeywordType keyword)
    {
        _keywordText.text = keyword.GetJapaneseName();
    }
    
    public void FadeIn()
    {
        if (_fadeMotion.IsActive()) _fadeMotion.Cancel();
        _fadeMotion = _keywordText.FadeIn(0.3f);
    }

    public void FadeOut()
    {
        if (_fadeMotion.IsActive()) _fadeMotion.Cancel();
        _fadeMotion = _keywordText.FadeOut(0.3f);
    }
    
    private void Awake()
    {
        _keywordText = GetComponent<TextMeshProUGUI>();
        _fadeMotion = _keywordText.FadeOut(0f);
    }
}
