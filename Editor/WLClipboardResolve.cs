#nullable enable
using System;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;

namespace net.rs64.PAngelsStealersUtility
{
    public static class WLClipboardResolve
    {
        [MenuItem("Clipboard/WaylandToX11")]
        public static void WaylandToX11()
        {
            using var pasteProcess = Process.Start(new ProcessStartInfo("wl-paste") { UseShellExecute = false, RedirectStandardOutput = true });
            pasteProcess.WaitForExit(500);
            GUIUtility.systemCopyBuffer = pasteProcess.StandardOutput.ReadToEnd().Trim();
        }
        [MenuItem("Clipboard/X11ToWayland")]
        public static void X11ToWayland()
        {
            using var pasteProcess = Process.Start(new ProcessStartInfo("wl-copy", GUIUtility.systemCopyBuffer)
            { UseShellExecute = false, RedirectStandardOutput = true });
            pasteProcess.WaitForExit(500);
        }
    }
}
