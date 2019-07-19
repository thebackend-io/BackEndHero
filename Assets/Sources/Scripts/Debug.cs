using UnityEngine;

public static class Debug{

	[System.Diagnostics.Conditional("ENABLE_LOG")]
	public static void Log(object msg)
	{
		UnityEngine.Debug.Log("[BackEndLog] : " + msg);
	}

	[System.Diagnostics.Conditional("ENABLE_LOG")]
	public static void LogWarning(object msg)
	{
		UnityEngine.Debug.LogWarning("[BackEndLog] : " + msg);
	}

	[System.Diagnostics.Conditional("ENABLE_LOG")]
	public static void LogError(object msg)
	{
		UnityEngine.Debug.LogError("[BackEndLog] : " + msg);
	}
}
