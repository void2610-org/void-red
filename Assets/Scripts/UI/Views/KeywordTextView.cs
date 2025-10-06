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
    [Header("設定")]
    [SerializeField] private float floatSpeed = 1f;
    [SerializeField] private float floatRange = 10f;

    private TextMeshProUGUI _keywordText;
    private RectTransform _rectTransform;
    private MotionHandle _fadeMotion;
    private Vector2 _initialPosition;
    private float _randomSeedX;
    private float _randomSeedY;

    public void SetKeyword(KeywordType keyword)
    {
        _keywordText.text = keyword.GetJapaneseName();

        // 初期位置を保存
        _initialPosition = _rectTransform.anchoredPosition;

        // ランダムシード（各キーワードで異なる揺れパターン）
        _randomSeedX = Random.Range(0f, 1000f);
        _randomSeedY = Random.Range(0f, 1000f);
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
    
    private void Update()
    {
        // PerlinNoiseで自然な揺れを生成
        var time = Time.time * floatSpeed;
        var offsetX = (Mathf.PerlinNoise(_randomSeedX + time, 0f) - 0.5f) * 2f * floatRange;
        var offsetY = (Mathf.PerlinNoise(0f, _randomSeedY + time) - 0.5f) * 2f * floatRange;

        _rectTransform.anchoredPosition = _initialPosition + new Vector2(offsetX, offsetY);
    }

    private void Awake()
    {
        _keywordText = GetComponent<TextMeshProUGUI>();
        _rectTransform = GetComponent<RectTransform>();
        _fadeMotion = _keywordText.FadeOut(0f);
    }

    private void OnDestroy()
    {
        if (_fadeMotion.IsActive()) _fadeMotion.Cancel();
    }
}
