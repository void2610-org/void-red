using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 記憶テーマリストのアイテムビュー
/// 左側パネルの各テーマ表示を担当
/// </summary>
public class MemoryThemeListItemView : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private TextMeshProUGUI themeText;

    public AcquiredTheme Theme { get; private set; }

    private readonly Subject<Unit> _onClicked = new();

    /// <summary>
    /// クリックイベント
    /// </summary>
    public Observable<Unit> OnClicked => _onClicked;

    /// <summary>
    /// アイテムを初期化
    /// </summary>
    /// <param name="theme">獲得テーマ</param>
    public void Initialize(AcquiredTheme theme)
    {
        Theme = theme;
        button.interactable = true;
        UpdateDisplay();
    }

    /// <summary>
    /// 未解放状態として表示
    /// </summary>
    public void SetLocked()
    {
        Theme = null;
        themeText.text = "???";
        button.interactable = false;
    }

    /// <summary>
    /// 選択状態を設定
    /// </summary>
    /// <param name="isSelected">選択されているか</param>
    // 選択状態に応じた視覚的フィードバック
    public void SetSelected(bool isSelected) => themeText.fontStyle = isSelected ? FontStyles.Bold : FontStyles.Normal;

    /// <summary>
    /// 表示を更新
    /// </summary>
    private void UpdateDisplay()
    {
        if (Theme == null) return;
        themeText.text = $"{Theme.ThemeName}: {Theme.AcquiredCards.Count}枚";
    }

    private void Awake()
    {
        button.OnClickAsObservable()
            .Subscribe(_ => _onClicked.OnNext(Unit.Default))
            .AddTo(this);
    }

    private void OnDestroy()
    {
        _onClicked?.Dispose();
    }
}
