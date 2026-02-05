using R3;
using UnityEngine.InputSystem;

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

    /// <summary>
    /// InputAction.startedイベントをR3のObservableに変換
    /// </summary>
    /// <param name="action">変換するInputAction</param>
    /// <returns>performedイベントのObservable</returns>
    public static Observable<Unit> OnStartedAsObservable(this InputAction action)
    {
        return Observable.FromEvent<InputAction.CallbackContext>(
            h => action.started += h,
            h => action.started -= h
        ).Select(_ => Unit.Default);
    }

    /// <summary>
    /// InputAction.canceledイベントをR3のObservableに変換
    /// </summary>
    /// <param name="action">変換するInputAction</param>
    /// <returns>performedイベントのObservable</returns>
    public static Observable<Unit> OnCanceledAsObservable(this InputAction action)
    {
        return Observable.FromEvent<InputAction.CallbackContext>(
            h => action.canceled += h,
            h => action.canceled -= h
        ).Select(_ => Unit.Default);
    }
}
