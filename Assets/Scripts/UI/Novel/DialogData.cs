using System;
using UnityEngine;

/// <summary>
/// 1つのセリフを表すデータクラス
/// 話者、セリフ内容、SE情報などを含む
/// </summary>
[Serializable]
public class DialogData
{
    [Header("セリフ情報")]
    [SerializeField] private string speakerName;
    [SerializeField, TextArea(3, 5)] private string dialogText;
    
    [Header("画像情報")]
    [SerializeField] private string characterImageName; // キャラクター画像の名前
    
    [Header("音響効果")]
    [SerializeField] private string seClipName;
    [SerializeField] private bool playSeOnStart;
    
    [Header("表示設定")]
    [SerializeField] private float customCharSpeed; // -1の場合はデフォルト速度を使用
    [SerializeField] private bool autoAdvance = false; // 自動で次に進むかどうか
    
    /// <summary>
    /// 話者名
    /// </summary>
    public string SpeakerName => speakerName;
    
    /// <summary>
    /// セリフの内容
    /// </summary>
    public string DialogText => dialogText;
    
    /// <summary>
    /// キャラクター画像の名前
    /// </summary>
    public string CharacterImageName => characterImageName;
    
    /// <summary>
    /// 再生するSEのクリップ名
    /// </summary>
    public string SEClipName => seClipName;
    
    /// <summary>
    /// セリフ開始時にSEを再生するかどうか
    /// </summary>
    public bool PlaySEOnStart => playSeOnStart;
    
    /// <summary>
    /// カスタム文字速度（-1の場合はデフォルト速度）
    /// </summary>
    public float CustomCharSpeed => customCharSpeed;
    
    /// <summary>
    /// 自動で次に進むかどうか
    /// </summary>
    public bool AutoAdvance => autoAdvance;
    
    
    /// <summary>
    /// セリフに有効なテキストが含まれているかどうか
    /// </summary>
    public bool HasValidText => !string.IsNullOrEmpty(dialogText);
    
    /// <summary>
    /// SEが設定されているかどうか
    /// </summary>
    public bool HasSE => !string.IsNullOrEmpty(seClipName);
    
    /// <summary>
    /// デフォルト文字速度を使用するかどうか
    /// </summary>
    public bool UseDefaultCharSpeed => customCharSpeed < 0f;
    
    /// <summary>
    /// コンストラクタ
    /// </summary>
    public DialogData(string speakerName, string dialogText, string characterImageName = "", string seClipName = "", bool playSeOnStart = true)
    {
        this.speakerName = speakerName;
        this.dialogText = dialogText;
        this.characterImageName = characterImageName;
        this.seClipName = seClipName;
        this.playSeOnStart = playSeOnStart;
        this.customCharSpeed = -1f;
        this.autoAdvance = false;
    }
    
    /// <summary>
    /// カスタム文字速度を設定
    /// </summary>
    public void SetCustomCharSpeed(float speed)
    {
        this.customCharSpeed = speed;
    }
    
    /// <summary>
    /// 自動進行を設定
    /// </summary>
    public void SetAutoAdvance(bool autoAdvance)
    {
        this.autoAdvance = autoAdvance;
    }
}
