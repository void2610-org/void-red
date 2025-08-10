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
    [SerializeField] private Vector3 selectLightOriginalWorldPosition;
    
    [Header("プレイスタイル画像")]
    [SerializeField] private Image hesitationImage;
    [SerializeField] private Image impulseImage;
    [SerializeField] private Image convictionImage;
    
    [Header("表示設定")]
    [SerializeField] private float rotationDuration = 0.5f;
    [SerializeField] private Ease rotationEase = Ease.OutBack;
    
    public Observable<PlayStyle> PlayStyleSelected => _playStyleSelected;
    
    private readonly Subject<PlayStyle> _playStyleSelected = new();
    private PlayStyle _currentPlayStyle = PlayStyle.Impulse;
    private bool _isRotating;
    private int _currentAngleIndex;
    
    // 各プレイスタイルの角度
    private readonly float[] _angles = { 0f, 45f, -45f };
    private readonly PlayStyle[] _playStyles = { PlayStyle.Impulse, PlayStyle.Hesitation, PlayStyle.Conviction };
    
    /// <summary>
    /// 車輪を回転させて次のプレイスタイルを選択
    /// </summary>
    private void RotateWheel()
    {
        if (_isRotating) return;
        
        _isRotating = true;
        _currentAngleIndex = (_currentAngleIndex + 1) % 3;
        _currentPlayStyle = _playStyles[_currentAngleIndex];
        UpdateImageHighlight();
        
        // 車輪を回転
        var currentRotation = Quaternion.Euler(0, 0, _angles[(_currentAngleIndex + 2) % 3]);
        var targetRotation = Quaternion.Euler(0, 0, _angles[_currentAngleIndex]);
        
        LMotion.Create(currentRotation, targetRotation, rotationDuration)
            .WithEase(rotationEase)
            .BindToRotation(wheelContainer)
            .AddTo(gameObject)
            .ToUniTask()
            .ContinueWith(() =>
            {
                _isRotating = false;
                _playStyleSelected.OnNext(_currentPlayStyle);
            }).Forget();
    }
    
    /// <summary>
    /// 選択されているプレイスタイルの画像をハイライト
    /// </summary>
    private void UpdateImageHighlight()
    {
        // 全ての画像を透明に
        SetImageAlpha(hesitationImage, 0.3f);
        SetImageAlpha(impulseImage, 0.3f);
        SetImageAlpha(convictionImage, 0.3f);
        
        // 選択中の画像を不透明に
        var selectedImage = _currentPlayStyle switch
        {
            PlayStyle.Hesitation => hesitationImage,
            PlayStyle.Impulse => impulseImage,
            PlayStyle.Conviction => convictionImage,
            _ => impulseImage
        };
        
        SetImageAlpha(selectedImage, 1.0f);
    }
    
    /// <summary>
    /// 画像の透明度を設定
    /// </summary>
    private void SetImageAlpha(Image image, float alpha)
    {
        if (!image) return;
        var color = image.color;
        color.a = alpha;
        image.color = color;
    }
    
    private void Awake()
    {
        // ボタンイベントの設定
        wheelButton.onClick.AddListener(RotateWheel);
        // 初期状態を設定
        UpdateImageHighlight();
        if (wheelContainer)
            wheelContainer.localRotation = Quaternion.Euler(0, 0, _angles[0]);
    }

    private void Update()
    {
        if (selectLight)
        {
            // ワールド座標で位置を固定
            selectLight.transform.position = selectLightOriginalWorldPosition;
            selectLight.transform.rotation = Quaternion.identity;
        }
    }
    
    private void OnDestroy()
    {
        _playStyleSelected?.Dispose();
        
        if (wheelButton)
        {
            wheelButton.onClick.RemoveAllListeners();
        }
    }
}