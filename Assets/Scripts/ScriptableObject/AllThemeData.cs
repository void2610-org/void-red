using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Void2610.UnityTemplate;

#if UNITY_EDITOR
#endif

/// <summary>
/// 全てのThemeDataを管理するScriptableObject
/// </summary>
[CreateAssetMenu(fileName = "AllThemeData", menuName = "VoidRed/All Theme Data")]
public class AllThemeData : ScriptableObject
{
    [SerializeField] private List<ThemeData> themeList = new();

    // プロパティ
    public List<ThemeData> ThemeList => themeList;

    /// <summary>
    /// IDからテーマを取得
    /// </summary>
    /// <param name="themeId">テーマID</param>
    /// <returns>該当するThemeData、見つからない場合はnull</returns>
    public ThemeData GetThemeById(string themeId) => themeList.FirstOrDefault(theme => theme.ThemeId == themeId);

    /// <summary>
    /// 同じディレクトリ内の全てのThemeDataを自動的に登録
    /// </summary>
    public void RegisterAllThemes()
    {
#if UNITY_EDITOR
        this.RegisterAssetsInSameDirectory(themeList, x => x.name);
#endif
    }
}
