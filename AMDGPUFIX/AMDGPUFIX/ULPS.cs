using Microsoft.Win32;
using System.Collections.Generic;
using System.Linq;

namespace AMDGPUFIX
{
    public class ULPS
    {
        private List<string> ulps_profiles = new List<string>();
        private const string ClassGuid = "{4d36e968-e325-11ce-bfc1-08002be10318}";

        // Locate ULPS profiles
        private void LocateUlpsProfiles()
        {
            string[] wheretocheck = { "SYSTEM\\CurrentControlSet", "SYSTEM\\ControlSet001" };
            var localMachine = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64) // 64
                ?? RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32); // 32 fallback
            try
            {
                foreach (var reg_path_to in wheretocheck)
                {
                    string basePath = $"{reg_path_to}\\Control\\Class\\{ClassGuid}";
                    using var key = localMachine.OpenSubKey(basePath);
                    if (key == null) continue;
                    var profiles = key.GetSubKeyNames().Where(x => x.Length == 4 && x.All(char.IsDigit));
                    foreach (var profile in profiles)
                    {
                        string profilePath = $"{basePath}\\{profile}";
                        using var checkKey = localMachine.OpenSubKey($"{profilePath}\\UMD");
                        if (checkKey != null) ulps_profiles.Add(profilePath);
                    }
                }
            }
            catch { }
        }

        // Check if ULPS is on or not
        public bool CheckULPS()
        {
            LocateUlpsProfiles();
            if (ulps_profiles.Count == 0) return true; // No profiles found, assume ULPS is enabled by default

            var localMachine = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64) // 64
                ?? RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32); // 32 fallback
            foreach (string path in ulps_profiles)
            {
                using var key = localMachine.OpenSubKey(path);
                if (key?.GetValue("EnableUlps")?.ToString() == "0") return false;
            }
            return true;
        }

        // Enable & Disable Toggle Handler
        public void ULPSHandler(bool enable)
        {
            var localMachine = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64) // 64
                ?? RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32); // 32 fallback
            foreach (string profile in ulps_profiles)
            {
                try
                {
                    using var key = localMachine.OpenSubKey(profile, writable: true);
                    if (key?.GetValue("EnableUlps") != null) key.SetValue("EnableUlps", enable ? 1 : 0, RegistryValueKind.DWord);
                    // Make sure AMD doesn't override the setting by adding a backup value
                    if (key?.GetValue("EnableUlps_NA") != null) key?.SetValue("EnableUlps_NA", enable ? "1" : "0", RegistryValueKind.String);
                }
                catch { }
            }
        }
    }
}
