using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Void2610.UnityTemplate;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// 全てのチュートリアルデータを管理するScriptableObject
/// </summary>
[CreateAssetMenu(fileName = "AllTutorialData", menuName = "VoidRed/All Tutorial Data")]
public class AllTutorialData : ScriptableObject
{
    [SerializeField] private List<TutorialData> tutorialList = new();

    public void RegisterAllTutorials()
    {
#if UNITY_EDITOR
        this.RegisterAssetsInSameDirectory(tutorialList, x => x.TutorialId);
#endif
    }

    /// <summary>
    /// チュートリアルIDでチュートリアルを取得
    /// </summary>
    /// <param name="tutorialId">チュートリアルID</param>
    /// <returns>指定されたIDのチュートリアルデータ</returns>
    public TutorialData GetTutorialById(string tutorialId)
    {
        return tutorialList.FirstOrDefault(tutorial => tutorial.TutorialId == tutorialId);
    }
}
