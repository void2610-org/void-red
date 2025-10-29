using System.Collections.Generic;
using UnityEngine;
using Void2610.UnityTemplate;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// 全てのHelpDataを管理するScriptableObject
/// </summary>
[CreateAssetMenu(fileName = "AllHelpData", menuName = "VoidRed/All Help Data")]
public class AllHelpData : ScriptableObject
{
    [SerializeField] private List<HelpData> helpList = new();

    // プロパティ
    public List<HelpData> HelpList => helpList;
    public int Count => helpList.Count;

    /// <summary>
    /// 同じディレクトリ内の全てのHelpDataを自動的に登録
    /// </summary>
    public void RegisterAllHelps()
    {
#if UNITY_EDITOR
        this.RegisterAssetsInSameDirectory(helpList, x => x.name);
#endif
    }

    /// <summary>
    /// インデックスでヘルプを取得
    /// </summary>
    /// <param name="index">ヘルプのインデックス</param>
    /// <returns>指定されたインデックスのヘルプデータ</returns>
    public HelpData GetHelpByIndex(int index)
    {
        if (index < 0 || index >= helpList.Count) return null;
        return helpList[index];
    }
}
