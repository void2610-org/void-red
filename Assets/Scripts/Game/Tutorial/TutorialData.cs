using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// チュートリアルの個別ステップデータ
/// メッセージとマスク領域の座標情報を保持
/// </summary>
[Serializable]
public class TutorialStep
{
    [TextArea(3, 5)]
    [SerializeField] private string message;
    [SerializeField] private bool isPlayerDialog;

    [SerializeField] private Vector2 maskPosition;
    [SerializeField] private Vector2 maskSize = new(200, 100);

    public string Message => message;
    public bool IsProtagonist => isPlayerDialog;
    public Vector2 MaskPosition => maskPosition;
    public Vector2 MaskSize => maskSize;
}

/// <summary>
/// チュートリアルデータを管理するScriptableObject
/// 複数のチュートリアルステップをリストで保持
/// </summary>
[CreateAssetMenu(fileName = "TutorialData", menuName = "VoidRed/Tutorial Data")]
public class TutorialData : ScriptableObject
{
    [SerializeField] private List<TutorialStep> steps = new();

    public string TutorialId => this.name;
    public List<TutorialStep> Steps => steps;
    public int StepCount => steps?.Count ?? 0;

    public TutorialStep GetStep(int index)
    {
        if (index < 0 || index >= StepCount) return null;
        return steps[index];
    }
}
