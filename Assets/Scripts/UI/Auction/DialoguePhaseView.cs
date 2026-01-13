using System.Collections.Generic;
using UnityEngine;
using TMPro;
using R3;

public class DialoguePhaseView : MonoBehaviour
{
    [Header("選択肢View")]
    [SerializeField] private DialogueChoicesView choicesView;

    [Header("敵セリフ表示")]
    [SerializeField] private TextMeshProUGUI dialogueText;

    [Header("結果表示")]
    [SerializeField] private TextMeshProUGUI resultText;

    public Observable<int> OnChoiceSelected => choicesView.OnChoiceSelected;

    public void SetupChoices(List<string> labels) => choicesView.Setup(labels);

    public void HideChoices() => choicesView.Hide();

    public void ShowDialogueText(string text)
    {
        dialogueText.text = text;
        dialogueText.gameObject.SetActive(true);
    }

    public void HideDialogueText() => dialogueText.gameObject.SetActive(false);

    public void ShowResult(string message)
    {
        resultText.text = message;
        resultText.gameObject.SetActive(true);
    }

    public void HideResult()
    {
        resultText.text = "";
        resultText.gameObject.SetActive(false);
    }

    public void HideAll()
    {
        HideChoices();
        HideDialogueText();
        HideResult();
    }

    public void Show() => gameObject.SetActive(true);
    public void Hide() => gameObject.SetActive(false);
}
