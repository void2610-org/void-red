public enum DialogueChoiceType
{
    /// <summary> 挑発 </summary>
    Provoke,
    /// <summary> 共感 </summary>
    Empathize,
    /// <summary> 説得 </summary>
    Persuade,
    /// <summary> 沈黙 </summary>
    Silence
}

public enum DialogueActionType
{
    /// <summary> 対象変更 </summary>
    TargetChange,
    /// <summary> リソース増減 </summary>
    ResourceChange,
    /// <summary> ブラフ強化（失敗時のペナルティ） </summary>
    BluffStrengthen
}

public static class DialogueChoiceTypeExtensions
{
    public static string ToJapaneseName(this DialogueChoiceType choice) => choice switch
    {
        DialogueChoiceType.Provoke => "挑発",
        DialogueChoiceType.Empathize => "共感",
        DialogueChoiceType.Persuade => "説得",
        DialogueChoiceType.Silence => "沈黙",
        _ => "不明"
    };
}
