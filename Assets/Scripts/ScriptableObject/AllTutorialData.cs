using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Void2610.UnityTemplate;

#if UNITY_EDITOR
#endif

/// <summary>
/// 全てのチュートリアルデータを管理するScriptableObject
/// </summary>
[CreateAssetMenu(fileName = "AllTutorialData", menuName = "VoidRed/All Tutorial Data")]
public class AllTutorialData : ScriptableObject
{
    [SerializeField] private bool enableTutorial = true;
    [SerializeField] private List<TutorialData> tutorialList = new();

    public bool EnableTutorial => enableTutorial;

    /// <summary>
    /// チュートリアルIDでチュートリアルを取得
    /// </summary>
    /// <param name="tutorialId">チュートリアルID</param>
    /// <returns>指定されたIDのチュートリアルデータ</returns>
    public TutorialData GetTutorialById(string tutorialId) => tutorialList.FirstOrDefault(tutorial => tutorial.TutorialId == tutorialId);

    public void RegisterAllTutorials()
    {
#if UNITY_EDITOR
        this.RegisterAssetsInSameDirectory(tutorialList, x => x.TutorialId);
#endif
    }
}
