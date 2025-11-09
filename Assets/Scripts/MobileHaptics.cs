using UnityEngine;
using System;

public static class MobileHaptics
{
    // Global toggle for haptics
    private static bool isEnabled = true;
    
    /// <summary>
    /// Enable or disable all haptic feedback
    /// </summary>
    public static void SetEnabled(bool enabled)
    {
        isEnabled = enabled;
    }
    
    /// <summary>
    /// Check if haptics are currently enabled
    /// </summary>
    public static bool IsEnabled()
    {
        return isEnabled;
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    static AndroidJavaObject vibrator;
    static bool hasAmplitude;
    static int sdkInt;
    static long lastTickMs;
    const int MinIntervalMs = 25; // throttle to avoid spam

    static void EnsureInit()
    {
        if (vibrator != null) return;

        using (var version = new AndroidJavaClass("android.os.Build$VERSION"))
            sdkInt = version.GetStatic<int>("SDK_INT");

        using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        using (var context = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
        {
            vibrator = context.Call<AndroidJavaObject>("getSystemService", "vibrator");
        }

        if (sdkInt >= 26 && vibrator != null)
            hasAmplitude = vibrator.Call<bool>("hasAmplitudeControl");
    }

    static bool Throttle()
    {
        long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        if (now - lastTickMs < MinIntervalMs) return true;
        lastTickMs = now;
        return false;
    }

    static void VibrateOneShot(int durationMs, int amplitude /*1-255*/)
    {
        EnsureInit();
        if (vibrator == null || Throttle() || !isEnabled) return;

        if (sdkInt >= 26)
        {
            using (var vfxClass = new AndroidJavaClass("android.os.VibrationEffect"))
            {
                AndroidJavaObject effect;
                if (hasAmplitude)
                    effect = vfxClass.CallStatic<AndroidJavaObject>("createOneShot", (long)durationMs, amplitude);
                else
                    effect = vfxClass.CallStatic<AndroidJavaObject>("createOneShot", (long)durationMs, /*DEFAULT_AMPLITUDE*/-1);

                vibrator.Call("vibrate", effect);
            }
        }
        else
        {
            vibrator.Call("vibrate", (long)durationMs);
        }
    }

    static void VibratePattern(long[] patternMs, int[] amplitudes)
    {
        EnsureInit();
        if (vibrator == null || Throttle() || !isEnabled) return;

        if (sdkInt >= 26)
        {
            using (var vfxClass = new AndroidJavaClass("android.os.VibrationEffect"))
            {
                var effect = vfxClass.CallStatic<AndroidJavaObject>(
                    "createWaveform", patternMs, amplitudes, -1 /*no repeat*/);
                vibrator.Call("vibrate", effect);
            }
        }
        else
        {
            // Pre-26 can't set amplitudes; still do a simple vibrate
            long total = 0; foreach (var ms in patternMs) total += ms;
            vibrator.Call("vibrate", total);
        }
    }
#endif

    // Public API (safe no-ops in Editor / non-Android)
    public static void Tap()            { OneShot(12, 32); }
    public static void ImpactLight()    { OneShot(18, 48); }
    public static void ImpactMedium()   { OneShot(28, 120); }
    public static void ImpactHeavy()    { OneShot(40, 255); }

    public static void Success()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        VibratePattern(
            new long[] { 0, 12, 30, 22 },          // wait, buzz, gap, buzz
            new int[]  { 0, 64, 0, 160 });
#endif
    }

    public static void Warning()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        VibratePattern(
            new long[] { 0, 16, 18, 16 },
            new int[]  { 0, 180, 0, 180 });
#endif
    }

    public static void Error()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        VibratePattern(
            new long[] { 0, 30, 30, 30 },
            new int[]  { 0, 255, 0, 200 });
#endif
    }

    static void OneShot(int ms, int amp)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        VibrateOneShot(ms, amp);
#endif
    }
}
