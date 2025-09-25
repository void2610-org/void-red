using UnityEngine;
using UnityEngine.UI;
using R3;

/// <summary>
/// プレイボタン管理を担当するViewクラス
/// </summary>
public class PlayButtonView : MonoBehaviour
{
    [SerializeField] private Button playButton;

    public Observable<Unit> PlayButtonClicked => playButton.OnClickAsObservable();
    public void Show() => playButton.gameObject.SetActive(true);
    public void Hide() => playButton.gameObject.SetActive(false);
    
    private void Awake()
    {
        Hide(); // 初期状態では非表示
    }
}