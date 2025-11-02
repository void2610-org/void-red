using R3;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.InputSystem;

/// <summary>
/// ノベルシーンのキーバインド設定
/// InputSystemアクションを購読してViewやNovelUIPresenterに処理を委譲
/// </summary>
public static class NovelKeyBindings
{
    /// <summary>
    /// キーバインドを設定（NovelUIPresenterから呼び出される）
    /// </summary>
    public static void Setup(
        InputActionsProvider inputActionsProvider,
        NovelPresenter novelPresenter,
        CompositeDisposable disposables)
    {
        var dialogView = UnityEngine.Object.FindAnyObjectByType<DialogView>();
        var itemGetEffectView = UnityEngine.Object.FindAnyObjectByType<ItemGetEffectView>();

        // オートモードをトグル
        inputActionsProvider.Novel.Auto.OnPerformedAsObservable()
            .Where(_ => !BaseWindowView.HasActiveWindows)
            .Subscribe(_ => dialogView.ToggleAutoMode())
            .AddTo(disposables);

        // スキップ
        inputActionsProvider.Novel.Skip.OnPerformedAsObservable()
            .Where(_ => !BaseWindowView.HasActiveWindows)
            .Subscribe(_ => novelPresenter.RequestSkipAllDialogs())
            .AddTo(disposables);

        // ダイアログ/アイテム取得演出を進める
        inputActionsProvider.Novel.Advance.OnPerformedAsObservable()
            .Subscribe(_ =>
            {
                // マウスクリックの場合、UIボタン上かチェック
                if (IsPointerOverUI()) return;

                // アイテム取得演出が表示中ならそちらを優先
                if (itemGetEffectView && itemGetEffectView.IsShowing)
                {
                    itemGetEffectView.OnClick();
                }
                // ウィンドウが開いていない場合はダイアログを進める
                else if (!BaseWindowView.HasActiveWindows)
                {
                    dialogView.OnClick();
                }
            })
            .AddTo(disposables);
    }

    /// <summary>
    /// 現在のポインター位置にUIがあるかチェック
    /// </summary>
    private static bool IsPointerOverUI()
    {
        if (!EventSystem.current) return false;
        if (Mouse.current == null) return false;

        var pointerPosition = Mouse.current.position.ReadValue();
        var pointerEventData = new PointerEventData(EventSystem.current) { position = pointerPosition };
        var raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerEventData, raycastResults);

        // UIがヒットした場合はtrue
        var res = raycastResults.Where(o => o.gameObject.name != "ClickAreaButton").ToList();
        return res.Count > 0;
    }
}
