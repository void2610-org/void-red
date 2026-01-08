using UnityEngine;
using UnityEngine.UI;
using R3;
using LitMotion;
using LitMotion.Extensions;
using Cysharp.Threading.Tasks;
using Void2610.UnityTemplate;

/// <summary>
/// プレイスタイル選択を担当するViewクラス（車輪UI）
/// </summary>
public class PlayStyleView : MonoBehaviour
{
    [Header("車輪UI要素")]
    [SerializeField] private Transform wheelContainer;
    [SerializeField] private Button wheelButton;
    
    [Header("プレイスタイル画像")]
    [SerializeField] private Image hesitationImage;
    [SerializeField] private Image impulseImage;
    [SerializeField] private Image convictionImage;
    
    private const int PLAYSTYLE_COUNT = 3;
    private const float ROTATION_DURATION = 1f;
    private const Ease ROTATION_EASE = Ease.OutBack;
    
    
    private bool _isRotating;
    private int _currentIndex;
    
    /// <summary>
    /// 車輪を回転させて次のプレイスタイルを選択
    /// </summary>
    public void RotateWheel()
    {
        if (_isRotating) return;

        SeManager.Instance.PlaySe("WheelOfPlayStyle");
        
        _isRotating = true;
        var previousIndex = _currentIndex;
        _currentIndex = (_currentIndex + 1) % PLAYSTYLE_COUNT;
        // _currentPlayStyle = _styleConfigs[_currentIndex].style;
        UpdateImageHighlight();
        
        // 車輪を回転
        // var currentAngle = _styleConfigs[previousIndex].angle;
        // var targetAngle = _styleConfigs[_currentIndex].angle;
        
        LMotion.Create(0f, 90f, ROTATION_DURATION)
            .WithEase(ROTATION_EASE)
            .BindToLocalEulerAnglesZ(wheelContainer)
            .AddTo(gameObject)
            .ToUniTask()
            .ContinueWith(() =>
            {
                _isRotating = false;
                // _playStyleSelected.OnNext(_currentPlayStyle);
            }).Forget();
    }
    
    /// <summary>
    /// 選択されているプレイスタイルの画像をハイライト
    /// </summary>
    private void UpdateImageHighlight()
    {
        // 全ての画像を透明に
        SetImageAlpha(hesitationImage, false);
        SetImageAlpha(impulseImage, false);
        SetImageAlpha(convictionImage, false);
        
        // 選択中の画像を不透明に
        // SetImageAlpha(GetImageForPlayStyle(_currentPlayStyle),true);
    }
    
    /// <summary>
    /// 画像の透明度を設定
    /// </summary>
    private void SetImageAlpha(Image image, bool isSelected)
    {
        if (!image) return;
        var color = image.color;
        color.a = isSelected ? 1.0f : 0.3f;
        image.color = color;
    }

    private void Awake()
    {
        // ボタンイベントの設定
        wheelButton.OnClickAsObservable()
            .Subscribe(_ => RotateWheel())
            .AddTo(this);
        
        // 初期状態を設定
        UpdateImageHighlight();
        // wheelContainer.localRotation = Quaternion.Euler(0, 0, _styleConfigs[0].angle);
    }
    
    private void OnDestroy()
    {
        // _playStyleSelected?.Dispose();
    }
}