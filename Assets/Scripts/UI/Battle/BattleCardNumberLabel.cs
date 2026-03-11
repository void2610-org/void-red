using TMPro;
using UnityEngine;

/// <summary>
/// バトル用カード数字の表示を担当する
/// </summary>
public class BattleCardNumberLabel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI numberText;

    /// <summary>数字表示のON/OFFを切り替える</summary>
    /// <param name="isVisible">表示する場合はtrue</param>
    public void SetVisible(bool isVisible) => numberText.gameObject.SetActive(isVisible);

    /// <summary>表示中の数字を更新する</summary>
    /// <param name="number">表示する数字</param>
    public void SetNumber(int number) => numberText.text = number.ToString();
}
