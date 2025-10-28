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
    private readonly List<Vector2> _keywordPositions = new();

    private ThemeData _themeData;
    private MotionHandle _lensFlareMotionHandle;
    private bool _isKeywordsVisible;

    /// <summary>
    /// テーマとキーワードを表示
    /// </summary>
    /// <param name="themeData">テーマデータ</param>
    public async UniTask DisplayThemeWithKeywords(ThemeData themeData)
    {
        // BGMボリュームを下げる
        await BgmManager.Instance.DuckVolume();

        SeManager.Instance.PlaySe("ThemeAppearance");
        _themeData = themeData;
        themeText.TypewriterAnimation(themeData.Title, skipOnClick:false).Forget();
        await UniTask.Delay(4000);

        // BGMボリュームを元に戻す
        BgmManager.Instance.RestoreVolume().Forget();

        // 既存のキーワードViewをクリア
        ClearKeywords();

        // キーワードViewを生成（最遠点配置）
        foreach (var keyword in themeData.Keywords)
        {
            var keywordView = Instantiate(keywordTextPrefab, keywordContainer);
            var pos = GetFarthestPosition(220f, 30);
            keywordView.transform.localPosition = new Vector3(pos.x, pos.y, 0);
            keywordView.SetKeyword(keyword);
            _keywordViews.Add(keywordView);
            _keywordPositions.Add(pos);
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
        _keywordPositions.Clear();
    }

    /// <summary>
    /// マウスカーソルが乗ったときの処理
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!_themeData) return;
        
        foreach (var view in _keywordViews)
            view.FadeIn();
        
        if (_lensFlareMotionHandle.IsActive()) _lensFlareMotionHandle.Cancel();

        _lensFlareMotionHandle = LMotion.Create(0f, 1f, 0.3f)
            .WithEase(Ease.OutCubic)
            .Bind(v => VolumeController.Instance.SetScreenSpaceLensFlareIntensity(v))
            .AddTo(this);
    }

    /// <summary>
    /// マウスカーソルが離れたときの処理
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        if (!_themeData) return;

        foreach (var view in _keywordViews)
            view.FadeOut();

        if (_lensFlareMotionHandle.IsActive()) _lensFlareMotionHandle.Cancel();

        _lensFlareMotionHandle = LMotion.Create(1f, 0f, 0.3f)
            .WithEase(Ease.OutCubic)
            .Bind(v => VolumeController.Instance.SetScreenSpaceLensFlareIntensity(v))
            .AddTo(this);
    }

    /// <summary>
    /// キーワード表示をトグル（InputSystemアクション用）
    /// </summary>
    public void ToggleKeywords()
    {
        if (!_themeData) return;

        _isKeywordsVisible = !_isKeywordsVisible;

        if (_isKeywordsVisible)
        {
            foreach (var view in _keywordViews)
                view.FadeIn();

            if (_lensFlareMotionHandle.IsActive()) _lensFlareMotionHandle.Cancel();
            _lensFlareMotionHandle = LMotion.Create(0f, 1f, 0.3f)
                .WithEase(Ease.OutCubic)
                .Bind(v => VolumeController.Instance.SetScreenSpaceLensFlareIntensity(v))
                .AddTo(this);
        }
        else
        {
            foreach (var view in _keywordViews)
                view.FadeOut();

            if (_lensFlareMotionHandle.IsActive()) _lensFlareMotionHandle.Cancel();
            _lensFlareMotionHandle = LMotion.Create(1f, 0f, 0.3f)
                .WithEase(Ease.OutCubic)
                .Bind(v => VolumeController.Instance.SetScreenSpaceLensFlareIntensity(v))
                .AddTo(this);
        }
    }
    
    /// <summary>
    /// 既存のキーワードから最も離れた位置を取得
    /// </summary>
    private Vector2 GetFarthestPosition(float placementRadius, int attempts)
    {
        // 最初のキーワードはランダムな位置
        if (_keywordPositions.Count == 0)
            return Random.insideUnitCircle * placementRadius;

        var bestPosition = Vector2.zero;
        var maxMinDistance = 0f;

        // 複数の候補位置を試す
        for (var i = 0; i < attempts; i++)
        {
            var candidatePos = Random.insideUnitCircle * placementRadius;

            // 既存の全キーワードとの最小距離を計算
            var minDistance = _keywordPositions
                .Select(existingPos => Vector2.Distance(candidatePos, existingPos))
                .Prepend(float.MaxValue).Min();

            // 最小距離が最大になる候補を選択
            if (minDistance > maxMinDistance)
            {
                maxMinDistance = minDistance;
                bestPosition = candidatePos;
            }
        }

        return bestPosition;
    }

    private void OnDestroy()
    {
        ClearKeywords();
        if (_lensFlareMotionHandle.IsActive()) _lensFlareMotionHandle.Cancel();
    }
}