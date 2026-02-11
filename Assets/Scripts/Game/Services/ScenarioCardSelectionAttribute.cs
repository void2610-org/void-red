using System;

/// <summary>
/// シナリオごのカード選択メソッドを識別するAttribute
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public class ScenarioCardSelectionAttribute : Attribute
{
    /// <summary>
    /// 対応するシナリオID
    /// </summary>
    public string ScenarioId { get; }

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="scenarioId">シナリオID</param>
    public ScenarioCardSelectionAttribute(string scenarioId)
    {
        ScenarioId = scenarioId;
    }
}
