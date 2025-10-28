using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Void2610.UnityTemplate;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// 全てのThemeDataを管理するScriptableObject
/// </summary>
[CreateAssetMenu(fileName = "AllThemeData", menuName = "VoidRed/All Theme Data")]
public class AllThemeData : ScriptableObject
{
    [SerializeField] private List<ThemeData> themeList = new ();
    
    // プロパティ
    public List<ThemeData> ThemeList => themeList;
    
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