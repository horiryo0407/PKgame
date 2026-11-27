using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class KeeperCursorLock : MonoBehaviour
{
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int left, top, right, bottom;
    }

    private delegate bool MonitorEnumProc(
        IntPtr hMonitor, IntPtr hdc, ref RECT rect, IntPtr data);

    [DllImport("user32.dll")]
    private static extern bool EnumDisplayMonitors(
        IntPtr hdc, IntPtr clip, MonitorEnumProc proc, IntPtr data);

    [DllImport("user32.dll")]
    private static extern bool ClipCursor(ref RECT rect);

    [DllImport("user32.dll")]
    private static extern bool ClipCursor(IntPtr rect);

    private int lockedDisplay = -1;

    public void Lock(int unityDisplayIndex)
    {
        lockedDisplay = unityDisplayIndex;

        List<RECT> monitors = new List<RECT>();
        EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero,
            (IntPtr h, IntPtr dc, ref RECT r, IntPtr d) =>
            {
                monitors.Add(r);
                return true;
            }, IntPtr.Zero);

        if (monitors.Count < 2)
            return;

        RECT primary = monitors[0];
        RECT other = monitors[1];

        foreach (RECT m in monitors)
        {
            if (m.left <= 0 && 0 < m.right && m.top <= 0 && 0 < m.bottom)
            {
                primary = m;
                break;
            }
        }

        foreach (RECT m in monitors)
        {
            if (m.left != primary.left || m.top != primary.top)
            {
                other = m;
                break;
            }
        }

        RECT target = unityDisplayIndex == 0 ? primary : other;
        ClipCursor(ref target);
    }

    public void Unlock()
    {
        lockedDisplay = -1;
        ClipCursor(IntPtr.Zero);
    }

    private void OnApplicationFocus(bool focus)
    {
        if (focus && lockedDisplay >= 0)
            Lock(lockedDisplay);
    }

    private void OnDestroy()
    {
        Unlock();
    }
#else
    public void Lock(int unityDisplayIndex) { }
    public void Unlock() { }
#endif
}