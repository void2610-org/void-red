using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;
using R3;

/// <summary>
/// 精神ベット管理を担当するViewクラス
/// </summary>
public class MentalBetView : MonoBehaviour
{
    [SerializeField] private Button mentalBetPlusButton;
    [SerializeField] private Button mentalBetMinusButton;
    [SerializeField] private TextMeshProUGUI mentalBetValueText;
    [SerializeField] private Image betStateImage;
    
    [Header("ベット状態スプライト")]
    [SerializeField] private Sprite lowBetSprite;    // 低ベット時のスプライト（1-3）
    [SerializeField] private Sprite midBetSprite;    // 中ベット時のスプライト（4-7）
    [SerializeField] private Sprite highBetSprite;   // 高ベット時のスプライト（8-10）
    
    public Observable<int> MentalBetChanged => _mentalBetChanged;
    
    private readonly Subject<int> _mentalBetChanged = new();
    private int _currentBetValue = 1;
    
    /// <summary>
    /// 精神ベット表示を更新
    /// </summary>
    public void UpdateDisplay(int betValue, int currentMentalPower, int minBet, int maxBet)
    {
        _currentBetValue = betValue;
        mentalBetValueText.text = betValue.ToString();
        
        // ボタンの有効/無効を切り替え
        mentalBetPlusButton.interactable = (betValue < maxBet && betValue < currentMentalPower);
        mentalBetMinusButton.interactable = (betValue > minBet);
        
        // ベット値に応じてスプライトを変更
        UpdateBetSprite(betValue);
    }
    
    /// <summary>
    /// ベット値に応じてスプライトを更新
    /// </summary>
    private void UpdateBetSprite(int betValue)
    {
        if (!betStateImage) return;
        
        if (betValue <= 3)
        {
            betStateImage.sprite = lowBetSprite;
        }
        else if (betValue <= 5)
        {
            betStateImage.sprite = midBetSprite;
        }
        else
        {
            betStateImage.sprite = highBetSprite;
        }
    }
    
    private void Awake()
    {
        // ボタンイベントの設定
        mentalBetPlusButton.OnClickAsObservable()
            .Subscribe(_ => _mentalBetChanged.OnNext(1))
            .AddTo(this);
        mentalBetMinusButton.OnClickAsObservable()
            .Subscribe(_ => _mentalBetChanged.OnNext(-1))
            .AddTo(this);
    }
    
    private void OnDestroy()
    {
        _mentalBetChanged?.Dispose();
    }
}