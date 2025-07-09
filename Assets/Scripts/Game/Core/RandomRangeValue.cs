using System;
using UnityEngine;

/// <summary>
/// ランダム範囲または固定値を管理するクラス
/// </summary>
[Serializable]
public class RandomRangeValue
{
    [SerializeField] private bool useRandomRange;
    [SerializeField] private int fixedValue = 1;
    [Space]
    [SerializeField] private int minValue = 1;
    [SerializeField] private int maxValue = 5;
    
    private int? _cachedValue;
    
    /// <summary>
    /// 現在の値を取得（ランダムの場合は初回のみ決定）
    /// </summary>
    public int Value
    {
        get
        {
            if (!useRandomRange) return fixedValue;
            
            _cachedValue ??= UnityEngine.Random.Range(minValue, maxValue + 1);
            return _cachedValue.Value;
        }
    }
    
    /// <summary>
    /// 説明文用の文字列を取得
    /// </summary>
    public string GetDisplayString()
    {
        if (useRandomRange && !_cachedValue.HasValue)
            return $"{minValue}～{maxValue}";
        
        return Value.ToString();
    }
    
    /// <summary>
    /// Inspector表示用プロパティ
    /// </summary>
    public bool UseRandomRange => useRandomRange;
    public int FixedValue => fixedValue;
    public int MinValue => minValue;
    public int MaxValue => maxValue;
    
    /// <summary>
    /// コンストラクタ（固定値）
    /// </summary>
    public RandomRangeValue(int fixedValue)
    {
        this.useRandomRange = false;
        this.fixedValue = fixedValue;
    }
    
    /// <summary>
    /// コンストラクタ（ランダム範囲）
    /// </summary>
    public RandomRangeValue(int minValue, int maxValue)
    {
        this.useRandomRange = true;
        this.minValue = minValue;
        this.maxValue = maxValue;
    }
    
    /// <summary>
    /// デフォルトコンストラクタ（Unity用）
    /// </summary>
    public RandomRangeValue()
    {
        useRandomRange = false;
        fixedValue = 1;
        minValue = 1;
        maxValue = 5;
    }
}

/// <summary>
/// ランダム範囲または固定値を管理するクラス（float版）
/// </summary>
[Serializable]
public class RandomRangeFloat
{
    [SerializeField] private bool useRandomRange;
    [Range(0, 100)]
    [SerializeField] private float fixedValue = 60f;
    [Space]
    [Range(0, 100)]
    [SerializeField] private float minValue = 50f;
    [Range(0, 100)]
    [SerializeField] private float maxValue = 80f;
    
    private float? _cachedValue;
    
    /// <summary>
    /// 現在の値を取得（ランダムの場合は初回のみ決定）
    /// </summary>
    public float Value
    {
        get
        {
            if (!useRandomRange) return fixedValue;
            
            _cachedValue ??= UnityEngine.Random.Range(minValue, maxValue);
            return _cachedValue.Value;
        }
    }
    
    /// <summary>
    /// 説明文用の文字列を取得
    /// </summary>
    public string GetDisplayString()
    {
        if (useRandomRange && !_cachedValue.HasValue)
            return $"{minValue:F0}～{maxValue:F0}";
        
        return $"{Value:F0}";
    }
    
    /// <summary>
    /// Inspector表示用プロパティ
    /// </summary>
    public bool UseRandomRange => useRandomRange;
    public float FixedValue => fixedValue;
    public float MinValue => minValue;
    public float MaxValue => maxValue;
    
    /// <summary>
    /// コンストラクタ（固定値）
    /// </summary>
    public RandomRangeFloat(float fixedValue)
    {
        this.useRandomRange = false;
        this.fixedValue = fixedValue;
    }
    
    /// <summary>
    /// コンストラクタ（ランダム範囲）
    /// </summary>
    public RandomRangeFloat(float minValue, float maxValue)
    {
        this.useRandomRange = true;
        this.minValue = minValue;
        this.maxValue = maxValue;
    }
    
    /// <summary>
    /// デフォルトコンストラクタ（Unity用）
    /// </summary>
    public RandomRangeFloat()
    {
        useRandomRange = false;
        fixedValue = 60f;
        minValue = 50f;
        maxValue = 80f;
    }
}