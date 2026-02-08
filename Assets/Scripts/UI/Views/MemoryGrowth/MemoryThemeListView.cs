using System.Collections.Generic;
using System.Linq;
using LitMotion;
using R3;
using UnityEngine;
using Void2610.UnityTemplate;

/// <summary>
/// 記憶テーマリストビュー
/// 左側パネルの獲得済みテーマ一覧を表示
/// </summary>
public class MemoryThemeListView : MonoBehaviour
{
    [SerializeField] private List<MemoryThemeListItemView> items = new();

    [Header("アニメーション設定")]
    [SerializeField] private float staggerDelay = 0.05f;
    [SerializeField] private float animDuration = 0.2f;
    [SerializeField] private float slideOffset = 50f;

    /// <summary>
    /// テーマ選択イベント
    /// </summary>
    public Observable<AcquiredTheme> OnThemeSelected => _onThemeSelected;

    private readonly Subject<AcquiredTheme> _onThemeSelected = new();
    private readonly CompositeDisposable _itemDisposables = new();
    private readonly List<MotionHandle> _animHandles = new();
    private MemoryThemeListItemView _selectedItem;

    /// <summary>
    /// テーマリストを初期化
    /// </summary>
    /// <param name="themes">獲得済みテーマリスト</param>
    public void Initialize(IReadOnlyList<AcquiredTheme> themes)
    {
        _itemDisposables.Clear();
        _selectedItem = null;

        // テーマがある部分は初期化、ない部分は未解放表示
        for (var i = 0; i < items.Count; i++)
        {
            var item = items[i];
            if (!item) continue;

            if (i < themes.Count)
            {
                item.Initialize(themes[i]);
                item.OnClicked.Subscribe(_ => SelectTheme(item)).AddTo(_itemDisposables);
            }
            else
            {
                item.SetLocked();
            }
        }

        PlayEnterAnimation();
    }

    /// <summary>
    /// アイテムを選択状態にする
    /// </summary>
    private void SelectTheme(MemoryThemeListItemView item)
    {
        // 前の選択を解除
        if (_selectedItem)
            _selectedItem.SetSelected(false);

        // 新しい選択を設定
        _selectedItem = item;
        _selectedItem.SetSelected(true);

        // イベント発火
        _onThemeSelected.OnNext(item.Theme);
    }

    private void PlayEnterAnimation()
    {
        _animHandles.CancelAll();
        Canvas.ForceUpdateCanvases();

        var targets = items
            .Where(item => item && item.gameObject.activeSelf)
            .Select(item => ((RectTransform)item.transform, item.gameObject.GetOrAddComponent<CanvasGroup>()))
            .ToList();
        targets.StaggeredSlideIn(new Vector2(0, slideOffset), animDuration, staggerDelay, _animHandles);
    }

    private void OnDestroy()
    {
        _animHandles.CancelAll();
        _itemDisposables.Dispose();
        _onThemeSelected?.Dispose();
    }
}
