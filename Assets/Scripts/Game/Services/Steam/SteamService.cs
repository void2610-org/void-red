#if !(UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX || UNITY_STANDALONE_OSX || STEAMWORKS_WIN || STEAMWORKS_LIN_OSX) && !UNITY_EDITOR
#define DISABLESTEAMWORKS
#endif

using System;
using UnityEngine;
using VContainer.Unity;
#if !DISABLESTEAMWORKS
using Steamworks;
#endif

public class SteamService : IDisposable, ITickable
{
#if !DISABLESTEAMWORKS
	private const int APP_ID = 3997140;
	
	private static bool _everInitialized;
	private bool _initialized;
	private SteamAPIWarningMessageHook_t _steamAPIWarningMessageHook;

	public static bool Initialized => _everInitialized;
	
	[AOT.MonoPInvokeCallback(typeof(SteamAPIWarningMessageHook_t))]
	protected static void SteamAPIDebugTextHook(int nSeverity, System.Text.StringBuilder pchDebugText) {
		Debug.LogWarning(pchDebugText);
	}

	public SteamService()
	{
		Init();
	}
	
	public bool UnlockAchievement(SteamAchieveType achieveType)
	{
		if (!_initialized) return false;
		if (!SteamUserStats.SetAchievement(achieveType.ToString())) return false;
		if (!SteamUserStats.StoreStats()) return false;
		return true;
	}
	
	public bool SetStat(SteamStatType statType, int value)
	{
		if (!_initialized) return false;
		if (!SteamUserStats.SetStat(statType.ToString(), value)) return false;
		if (!SteamUserStats.StoreStats()) return false;
		return true;
	}
	
	public bool SetStat(SteamStatType statType, float value)
	{
		if (!_initialized) return false;
		if (!SteamUserStats.SetStat(statType.ToString(), value)) return false;
		if (!SteamUserStats.StoreStats()) return false;
		return true;
	}
	
	public bool AddStat(SteamStatType statType, int value)
	{
		if (!_initialized) return false;
		if (!SteamUserStats.GetStat(statType.ToString(), out int currentValue)) return false;
		currentValue += value;
		if (!SteamUserStats.SetStat(statType.ToString(), currentValue)) return false;
		if (!SteamUserStats.StoreStats()) return false;
		return true;
	}
	
	public bool AddStat(SteamStatType statType, float value)
	{
		if (!_initialized) return false;
		if (!SteamUserStats.GetStat(statType.ToString(), out float currentValue)) return false;
		currentValue += value;
		if (!SteamUserStats.SetStat(statType.ToString(), currentValue)) return false;
		if (!SteamUserStats.StoreStats()) return false;
		return true;
	}
	
	public bool GetStat(SteamStatType statType, out int value)
	{
		value = 0;
		if (!_initialized) return false;
		return SteamUserStats.GetStat(statType.ToString(), out value);
	}
	
	public bool GetStat(SteamStatType statType, out float value)
	{
		value = 0f;
		if (!_initialized) return false;
		return SteamUserStats.GetStat(statType.ToString(), out value);
	}

	private void Init()
	{
		if (_everInitialized) throw new Exception("Tried to Initialize the SteamAPI twice in one session!");
		if (!Packsize.Test()) Debug.LogError("[Steamworks.NET] Packsize Test returned false, the wrong version of Steamworks.NET is being run in this platform.");
		if (!DllCheck.Test()) Debug.LogError("[Steamworks.NET] DllCheck Test returned false, One or more of the Steamworks binaries seems to be the wrong version.");

		try
		{
			// Steam以外で起動された場合、Steamクライアントを起動しゲームを再度Steam経由で起動する
			if (SteamAPI.RestartAppIfNecessary((AppId_t)APP_ID)) {
				Debug.Log("[Steamworks.NET] Shutting down because RestartAppIfNecessary returned true. Steam will restart the application.");
				Application.Quit();
				return;
			}
		}
		catch (DllNotFoundException e) 
		{
			Debug.LogError("[Steamworks.NET] Could not load [lib]steam_api.dll/so/dylib. It's likely not in the correct location. Refer to the README for more details.\n" + e);
			Application.Quit();
			return;
		}

		// SteamworksAPIを初期化
		_initialized = SteamAPI.Init();
		if (!_initialized)
		{
			Debug.LogError("[Steamworks.NET] SteamAPI_Init() failed. Refer to Valve's documentation or the comment above this line for more information.");
			return;
		}

		_everInitialized = true;
	}

	public void Tick()
	{
		if (!_initialized) return;
		SteamAPI.RunCallbacks();
	}
	
	public void Dispose()
	{
		if (!_initialized) return;
		SteamAPI.Shutdown();
		_initialized = false;
		_everInitialized = false;
	}
	
	/// <summary>
	/// [デバッグ用] 全ての実績と統計情報をリセットする
	/// </summary>
	public bool ResetAllStats(bool achievementsToo = true)
	{
		if (!_initialized) return false;
		return SteamUserStats.ResetAllStats(achievementsToo);
	}

#else
	// Steamが利用できない場合のダミー実装
	public static bool Initialized => false;
	public SteamService() { }
	public bool UnlockAchievement(SteamAchieveType achieveType) => false;
	public bool SetStat(SteamStatType statType, int value) => false;
	public bool SetStat(SteamStatType statType, float value) => false;
	public bool AddStat(SteamStatType statType, int value) => false;
	public bool AddStat(SteamStatType statType, float value) => false;
	public bool GetStat(SteamStatType statType, out int value) { value = 0; return false; }
	public bool GetStat(SteamStatType statType, out float value) { value = 0f; return false; }
	public void Tick() { }
	public void Dispose() { }
#endif
}