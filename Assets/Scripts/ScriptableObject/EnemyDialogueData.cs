using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class DialogueEffect
{
    [SerializeField] private DialogueActionType actionType;
    [SerializeField] private int resourceChangeAmount;

    public DialogueActionType ActionType => actionType;
    public int ResourceChangeAmount => resourceChangeAmount;
}

[Serializable]
public class EnemyDialogueResponse
{
    [SerializeField] private DialogueChoiceType targetChoice;
    [SerializeField, TextArea(2, 4)] private string dialogueText;
    [SerializeField] private DialogueEffect effect;

    public DialogueChoiceType TargetChoice => targetChoice;
    public string DialogueText => dialogueText;
    public DialogueEffect Effect => effect;
}

[Serializable]
public class PlayerDialogueOption
{
    [SerializeField] private string optionText;
    [SerializeField, TextArea(2, 4)] private string resultText;
    [SerializeField] private DialogueEffect effect;

    public string OptionText => optionText;
    public string ResultText => resultText;
    public DialogueEffect Effect => effect;
}

[Serializable]
public class EnemyDialogueInitiation
{
    [SerializeField] private DialogueChoiceType triggerChoice;
    [SerializeField, TextArea(2, 4)] private string enemyDialogueText;
    [SerializeField] private List<PlayerDialogueOption> playerOptions;

    public DialogueChoiceType TriggerChoice => triggerChoice;
    public string EnemyDialogueText => enemyDialogueText;
    public IReadOnlyList<PlayerDialogueOption> PlayerOptions => playerOptions;
}

[CreateAssetMenu(fileName = "New Enemy Dialogue", menuName = "VoidRed/Enemy Dialogue Data")]
public class EnemyDialogueData : ScriptableObject
{
    [Header("プレイヤー先攻時の敵反応（4パターン）")]
    [SerializeField] private List<EnemyDialogueResponse> responses;

    [Header("敵先攻時の対話（プレイヤーの先攻選択に基づく4パターン）")]
    [SerializeField] private List<EnemyDialogueInitiation> initiations;

    public EnemyDialogueResponse GetResponse(DialogueChoiceType playerChoice) => responses?.FirstOrDefault(r => r.TargetChoice == playerChoice);

    public EnemyDialogueInitiation GetInitiation(DialogueChoiceType playerFirstChoice) => initiations?.FirstOrDefault(i => i.TriggerChoice == playerFirstChoice);
}
