using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Coroutine helpers
/// </summary>
public static class MyCoroutine
{
	/// <summary>
	/// Waits for the specified amount of frames
	/// use : yield return MMCoroutine.WaitFor(1);
	/// </summary>
	/// <param name="frameCount"></param>
	/// <returns></returns>
	public static IEnumerator WaitForFrames(int frameCount)
	{
		while (frameCount > 0)
		{
			frameCount--;
			yield return null;
		}
	}
	/// <summary>
	/// Waits for the specified amount of seconds (using regular time)
	/// and optionally invokes a callback each frame with the progress (0 to 1).
	/// </summary>
	/// <remarks>
	/// Usage:
	/// yield return MyCoroutine.WaitFor(1f);                   // without callback  
	/// yield return MyCoroutine.WaitFor(1f, progress => { ... }); // with callback  
	/// </remarks>
	/// <param name="seconds">The duration to wait in seconds.</param>
	/// <param name="onChanged">
	/// Optional callback invoked every frame during the wait, with a float parameter
	/// indicating the normalized progress from 0 (start) to 1 (end).
	/// </param>
	/// <returns>IEnumerator for use in a coroutine.</returns>
	public static IEnumerator WaitFor(float seconds, Action<float> onChanged = null)
	{
		for (float timer = 0f; timer < seconds; timer += Time.deltaTime)
		{
			onChanged?.Invoke(timer / seconds);
			yield return null;
		}
		onChanged?.Invoke(1);
	}
	/// <summary>
	/// Waits for the specified amount of seconds using unscaled time (ignores Time.timeScale)
	/// and optionally invokes a callback each frame with the progress (0 to 1).
	/// </summary>
	/// <remarks>
	/// Usage:
	/// yield return MyCoroutine.WaitForUnscaled(1f);                   // without callback  
	/// yield return MyCoroutine.WaitForUnscaled(1f, progress => { ... }); // with callback  
	/// </remarks>
	/// <param name="seconds">The duration to wait in seconds (unscaled).</param>
	/// <param name="onChanged">
	/// Optional callback invoked every frame during the wait, with a float parameter
	/// indicating the normalized progress from 0 (start) to 1 (end).
	/// </param>
	/// <returns>IEnumerator for use in a coroutine.</returns>
	public static IEnumerator WaitForUnscaled(float seconds, Action<float> onChanged = null)
	{
		for (float timer = 0f; timer < seconds; timer += Time.unscaledDeltaTime)
		{
			onChanged?.Invoke(timer / seconds);
			yield return null;
		}
		onChanged?.Invoke(1);
	}
}