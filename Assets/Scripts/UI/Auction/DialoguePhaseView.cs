using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class DialoguePhaseView : BasePhaseView
{
    [SerializeField] private DialogueChoicesView choicesView;
    [SerializeField] private DialogueCutInView cutInView;

    [Header("立ち絵")]
    [SerializeField] private DialoguePortraitView portraitView;
    [SerializeField] private Sprite playerPortraitSprite;

    private static readonly string[] _playerDialogueTexts =
    {
        "その記憶、こっちが暴いてやる",
        "その気持ち、少しは分かる",
        "落ち着いて話をしよう",
        "今は黙って見極める",
    };

    private static readonly string[] _enemyDialogueTexts =
    {
        "その挑発、面白いね",
        "分かったつもりで来るんだ",
        "言葉で変えられると思う？",
        "沈黙も答えのうちか",
    };

    private Sprite _enemyPortraitSprite;
    private Sprite _enemyCutInSprite;

    public void HideChoices() => choicesView.Hide();
    public UniTask HidePlayerDialogueAsync() => UniTask.CompletedTask;
    public UniTask HideEnemyDialogueAsync() => UniTask.CompletedTask;

    public UniTask HideAllAsync()
    {
        HideChoices();
        return UniTask.CompletedTask;
    }

    /// <summary>
    /// 敵データで初期化する
    /// </summary>
    /// <param name="enemyData">立ち絵とカットインに使う敵データ</param>
    public void Initialize(EnemyData enemyData)
    {
        _enemyPortraitSprite = enemyData.DefaultSprite;
        _enemyCutInSprite = enemyData.CutInSprite;
    }

    /// <summary>
    /// オークション中にカード対話を1本だけ再生する
    /// </summary>
    /// <param name="card">対話対象のカード</param>
    /// <param name="enemyData">対話演出に使う敵データ</param>
    public async UniTask ShowCardDialogueAsync(CardModel card, EnemyData enemyData)
    {
        Initialize(enemyData);
        Show();
        await HideAllAsync();

        choicesView.Show();

        try
        {
            var selectedIndex = await choicesView.WaitForSelectionAsync();
            HideChoices();
            await ShowPlayerDialogueAsync(BuildPlayerDialogueText(card, selectedIndex));
            await ShowEnemyDialogueAsync(BuildEnemyDialogueText(card, selectedIndex));
        }
        catch (OperationCanceledException)
        {
            return;
        }
        finally
        {
            Hide();
        }
    }

    public override void Show()
    {
        portraitView.SetPortraitImmediate(playerPortraitSprite);
        base.Show();
        portraitView.SlideIn();
    }

    public override void Hide()
    {
        portraitView.SlideOut();
        base.Hide();
    }

    public async UniTask ShowPlayerDialogueAsync(string text)
    {
        portraitView.SlideOut();
        await cutInView.PlayPlayerCutInAsync(text);
    }

    public async UniTask ShowEnemyDialogueAsync(string text)
    {
        portraitView.SlideOut();
        await cutInView.PlayCutInAsync(_enemyPortraitSprite, _enemyCutInSprite, text);
    }

    /// <summary>
    /// プレイヤー側のカットイン文言を組み立てる
    /// </summary>
    /// <param name="card">選択されたカード</param>
    /// <param name="selectedIndex">選ばれた選択肢の番号</param>
    /// <returns>プレイヤーのカットイン文言</returns>
    private static string BuildPlayerDialogueText(CardModel card, int selectedIndex) =>
        $"{card.Data.CardName}に向けて、{_playerDialogueTexts[selectedIndex]}";

    /// <summary>
    /// 敵側のカットイン文言を組み立てる
    /// </summary>
    /// <param name="card">選択されたカード</param>
    /// <param name="selectedIndex">選ばれた選択肢の番号</param>
    /// <returns>敵のカットイン文言</returns>
    private static string BuildEnemyDialogueText(CardModel card, int selectedIndex) =>
        $"{card.Data.CardName}を見て、{_enemyDialogueTexts[selectedIndex]}";
}
