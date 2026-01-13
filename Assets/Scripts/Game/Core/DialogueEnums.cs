public enum DialogueChoiceType
{
    Provoke,   // 挑発
    Empathize, // 共感
    Persuade,  // 説得
    Silence    // 沈黙
}

public enum DialogueActionType
{
    TargetChange,    // 対象変更
    ResourceChange,  // リソース増減
    BluffStrengthen  // ブラフ強化（失敗時のペナルティ）
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
