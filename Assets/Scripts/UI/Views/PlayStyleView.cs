using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;
using R3;
using LitMotion;
using LitMotion.Extensions;
using Cysharp.Threading.Tasks;

/// <summary>
/// プレイスタイル選択を担当するViewクラス（車輪UI）
/// </summary>
public class PlayStyleView : MonoBehaviour
{
    [Header("車輪UI要素")]
    [SerializeField] private Transform wheelContainer;
    [SerializeField] private Button wheelButton;
    [SerializeField] private Image selectLight;
    
    [Header("プレイスタイル画像")]
    [SerializeField] private Image hesitationImage;
    [SerializeField] private Image impulseImage;
    [SerializeField] private Image convictionImage;
    
    private const int PLAYSTYLE_COUNT = 3;
    private const float ROTATION_DURATION = 1f;
    private const Ease ROTATION_EASE = Ease.OutBack;
    
    public Observable<PlayStyle> PlayStyleSelected => _playStyleSelected;
    
    private readonly Subject<PlayStyle> _playStyleSelected = new();
    private readonly (PlayStyle style, float angle)[] _styleConfigs = 
    {
        (PlayStyle.Impulse, 0f),
        (PlayStyle.Hesitation, 45f),
        (PlayStyle.Conviction, -45f)
    };
    private Vector3 _selectLightOriginalWorldPosition;
    private Quaternion _selectLightOriginalWorldRotation;
    private PlayStyle _currentPlayStyle = PlayStyle.Impulse;
    private bool _isRotating;
    private int _currentIndex;
    
    /// <summary>
    /// 車輪を回転させて次のプレイスタイルを選択
    /// </summary>
    private void RotateWheel()
    {
        if (_isRotating) return;
        
        _isRotating = true;
        var previousIndex = _currentIndex;
        _currentIndex = (_currentIndex + 1) % PLAYSTYLE_COUNT;
        _currentPlayStyle = _styleConfigs[_currentIndex].style;
        UpdateImageHighlight();
        
        // 車輪を回転
        var currentAngle = _styleConfigs[previousIndex].angle;
        var targetAngle = _styleConfigs[_currentIndex].angle;
        
        LMotion.Create(currentAngle, targetAngle, ROTATION_DURATION)
            .WithEase(ROTATION_EASE)
            .BindToLocalEulerAnglesZ(wheelContainer)
            .AddTo(gameObject)
            .ToUniTask()
            .ContinueWith(() =>
            {
                _isRotating = false;
                _playStyleSelected.OnNext(_currentPlayStyle);
                
                // selectLightの位置と回転を補正
                MaintainSelectLightTransform();
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
        SetImageAlpha(GetImageForPlayStyle(_currentPlayStyle),true);
    }
    
    /// <summary>
    /// プレイスタイルに対応する画像を取得
    /// </summary>
    private Image GetImageForPlayStyle(PlayStyle playStyle)
    {
        return playStyle switch
        {
            PlayStyle.Hesitation => hesitationImage,
            PlayStyle.Impulse => impulseImage,
            PlayStyle.Conviction => convictionImage,
            _ => impulseImage
        };
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
    
    /// <summary>
    /// selectLightのワールド座標を維持
    /// </summary>
    private void MaintainSelectLightTransform()
    {
        selectLight.transform.position = _selectLightOriginalWorldPosition;
        selectLight.transform.rotation = _selectLightOriginalWorldRotation;
    }
    
    private void Awake()
    {
        // ボタンイベントの設定
        wheelButton.onClick.AddListener(RotateWheel);
        
        // 初期状態を設定
        UpdateImageHighlight();
        wheelContainer.localRotation = Quaternion.Euler(0, 0, _styleConfigs[0].angle);
    }
    
    private void Start()
    {
        // selectLightの初期位置を保存（親の回転が適用された後）
        _selectLightOriginalWorldPosition = selectLight.transform.position;
        _selectLightOriginalWorldRotation = selectLight.transform.rotation;
    }
    
    private void Update()
    {
        // selectLightの位置と回転を毎フレーム固定
        MaintainSelectLightTransform();
    }
    
    private void OnDestroy()
    {
        _playStyleSelected?.Dispose();
        wheelButton.onClick.RemoveAllListeners();
    }
}