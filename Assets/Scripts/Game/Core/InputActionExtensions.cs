using UnityEngine.InputSystem;
using R3;

/// <summary>
/// InputActionの拡張メソッド
/// InputActionをR3のObservableに変換する機能を提供
/// </summary>
public static class InputActionExtensions
{
    /// <summary>
    /// InputAction.performedイベントをR3のObservableに変換
    /// </summary>
    /// <param name="action">変換するInputAction</param>
    /// <returns>performedイベントのObservable</returns>
    public static Observable<Unit> OnPerformedAsObservable(this InputAction action)
    {
        return Observable.FromEvent<InputAction.CallbackContext>(
            h => action.performed += h,
            h => action.performed -= h
        ).Select(_ => Unit.Default);
    }
}
