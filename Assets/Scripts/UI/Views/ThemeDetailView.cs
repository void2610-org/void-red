using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

/// <summary>
/// テーマ詳細情報を表示するモーダルViewクラス
/// </summary>
public class ThemeDetailView : BaseWindowView
{
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;

    /// <summary>
    /// テーマ詳細を表示
    /// </summary>
    /// <param name="themeData">表示するテーマデータ</param>
    public void ShowThemeDetail(ThemeData themeData)
    {
        // テーマ詳細情報を設定
        UpdateThemeDisplay(themeData);

        // パネルを表示
        Show();
    }

    /// <summary>
    /// テーマ表示を更新
    /// </summary>
    /// <param name="themeData">表示するテーマデータ</param>
    private void UpdateThemeDisplay(ThemeData themeData)
    {
        titleText.text = $"「{themeData.Title}」";
        descriptionText.text = themeData.Description;
    }
}
