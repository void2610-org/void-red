using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;
using YujiAp.UnityToolbarExtension.Editor;

/// <summary>
/// ツールバーにEventSystemで選択中のGameObjectを表示
/// </summary>
public class ToolbarExtensionSelectedGameObject : IToolbarElement
{
    public ToolbarElementLayoutType DefaultLayoutType => ToolbarElementLayoutType.RightSideLeftAlign;

    private Label _label;

    public VisualElement CreateElement()
    {
        var container = new VisualElement();
        container.style.flexDirection = FlexDirection.Row;
        container.style.alignItems = Align.Center;

        // プレフィックスラベル
        var prefixLabel = new Label("Selected: ");
        prefixLabel.style.color = new Color(0.7f, 0.7f, 0.7f);
        container.Add(prefixLabel);

        // 選択中オブジェクト名ラベル
        _label = new Label("None");
        _label.style.minWidth = 100;
        _label.style.maxWidth = 200;
        _label.style.overflow = Overflow.Hidden;
        _label.style.textOverflow = TextOverflow.Ellipsis;
        container.Add(_label);

        // 更新ループを登録
        EditorApplication.update += UpdateLabel;

        return container;
    }

    private void UpdateLabel()
    {
        if (_label == null) return;

        var selected = EventSystem.current?.currentSelectedGameObject;
        _label.text = selected ? selected.name : "None";
    }
}
