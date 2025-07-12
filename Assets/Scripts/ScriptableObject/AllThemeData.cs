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
    public int Count => themeList.Count;
    
    /// <summary>
    /// 同じディレクトリ内の全てのThemeDataを自動的に登録
    /// </summary>
    public void RegisterAllThemes()
    {
#if UNITY_EDITOR
        this.RegisterAssetsInSameDirectory(themeList, x => x.name);
#endif
    }
    
    /// <summary>
    /// ランダムなテーマを取得
    /// </summary>
    public ThemeData GetRandomTheme()
    {
        if (themeList.Count == 0) return null;
        return themeList[Random.Range(0, themeList.Count)];
    }
    
    /// <summary>
    /// 複数のランダムなテーマを取得（重複なし）
    /// </summary>
    public List<ThemeData> GetRandomThemes(int count)
    {
        if (count >= themeList.Count)
        {
            return new List<ThemeData>(themeList);
        }
        
        var shuffled = new List<ThemeData>(themeList);
        for (int i = 0; i < shuffled.Count; i++)
        {
            var temp = shuffled[i];
            var randomIndex = Random.Range(i, shuffled.Count);
            shuffled[i] = shuffled[randomIndex];
            shuffled[randomIndex] = temp;
        }
        
        return shuffled.Take(count).ToList();
    }
}