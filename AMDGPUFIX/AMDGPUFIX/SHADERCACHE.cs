using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace AMDGPUFIX
{
    public class SHADERCACHE
    {
        private List<string> gpu_profiles = new List<string>();
        private const string ClassGuid = "{4d36e968-e325-11ce-bfc1-08002be10318}";
        string[] wheretocheck = { "SYSTEM\\CurrentControlSet", "SYSTEM\\ControlSet001" };

        // Value Table
        // 1 = AMD Optimized | 31-00
        // 0 = ON            | 32-00
        // 2 = OFF           | 30-00

        // Check profile count externally
        public int GpuProfilesCount()
        {
            return gpu_profiles.Count;
        }

        private void LocateShaderCacheProfiles()
        {
            // Set Profiles
            RegistryKey localMachine = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64) // 64
                     ?? RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32); // 32 fallback
            if (localMachine == null)
            {
                MessageBox.Show("Error! ShaderCache Could not set registry base path due to lack of permission.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            try
            {
                foreach (var reg_path_to in wheretocheck)
                {
                    using (var key = localMachine.OpenSubKey($"{reg_path_to}\\Control\\Class\\{ClassGuid}"))
                    {
                        if (key == null) continue;
                        foreach (var profile in key.GetSubKeyNames().Where(x => x.Length == 4 && x.All(char.IsDigit)))
                        {
                            string umdPath = $"{reg_path_to}\\Control\\Class\\{ClassGuid}\\{profile}\\UMD";
                            using (var checkKey = localMachine.OpenSubKey(umdPath))
                                if (checkKey != null) gpu_profiles.Add(umdPath);
                        }
                    }
                }
            }
            catch
            {
                MessageBox.Show("Permission Denied!\r\n You are probably affected by a rootkit (virus)\r\n or User account that lacks permissions due to being managed by organisation.\r\n or Anti-Ransomware protection preventing registry access(such as Acronis True Image).\r\n Shader Cache Dropdown will be disabled to prevent any issues.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Check last profile's value to return
        public int CheckShaderCache()
        {
            // Locate Profiles
            LocateShaderCacheProfiles();
            if (gpu_profiles.Count == 0) return -1; // No profiles found

            RegistryKey localMachine = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64)
                                     ?? RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);

            foreach (string profile in gpu_profiles)
            {
                using (RegistryKey shadercacheKey = localMachine.OpenSubKey(profile, writable: true))
                {
                    if (shadercacheKey == null) continue;

                    object rawValue = shadercacheKey.GetValue("ShaderCache");
                    if (rawValue == null)
                    {
                        MessageBox.Show("No ShaderCache profile has been set, using AMD Optimized as default value.\r\nDriver update removed this value.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return 1; // Default
                    }

                    var valueKind = shadercacheKey.GetValueKind("ShaderCache");

                    // === Handle REG_BINARY (classic driver format) ===
                    if (valueKind == RegistryValueKind.Binary)
                    {
                        byte[] result = (byte[])rawValue;
                        if (result == null || result.Length == 0)
                        {
                            MessageBox.Show("ShaderCache value is null, using AMD Optimized as default value.\r\nDriver update removed this value.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return 1;
                        }

                        if (result.Length >= 2 && result[1] == 0x00)
                        {
                            switch (result[0])
                            {
                                case 0x32: return 0; // ON
                                case 0x31: return 1; // AMD Optimized
                                case 0x30: return 2; // OFF
                            }
                        }

                        // Unknown binary
                        string hexString = BitConverter.ToString(result);
                        MessageBox.Show($"Unknown ShaderCache binary value detected: {hexString}\r\nPlease report this to RedDot3ND on GitHub.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return -1;
                    }

                    // === Handle REG_SZ (Radeon Software rewritten format) ===
                    if (valueKind == RegistryValueKind.String)
                    {
                        string strVal = rawValue.ToString().Trim();

                        switch (strVal)
                        {
                            case "2": return 0; // ON
                            case "1": return 1; // AMD Optimized
                            case "0": return 2; // OFF
                            default:
                                MessageBox.Show($"Unknown ShaderCache string value detected: \"{strVal}\"\r\nPlease report this to RedDot3ND on GitHub.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                return -1;
                        }
                    }

                    // Unsupported type
                    MessageBox.Show($"Unsupported ShaderCache value type: {valueKind}\r\nPlease report this to RedDot3ND on GitHub.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return -1;
                }
            }

            return -1; // No valid values detected
        }

        // Set value to all profiles
        public void ShaderCacheHandler(int value)
        {
            // Table out byte vals and set it
            byte[] byteval = value == 0 ? new byte[] { 0x32, 0x00 } : value == 1 ? new byte[] { 0x31, 0x00 } : new byte[] { 0x30, 0x00 };
            string strVal = value == 0 ? "2" : value == 1 ? "1" : "0";
            var localMachine = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64) // 64
                            ?? RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32); // 32 fallback
            foreach (string profile in gpu_profiles)
                try
                {
                    using (var key = localMachine.OpenSubKey(profile, true))
                    {
                        if (key?.GetValue("ShaderCache") != null && key?.GetValueKind("ShaderCache") == RegistryValueKind.String)
                            key.SetValue("ShaderCache", strVal, RegistryValueKind.String);
                        else
                            key?.SetValue("ShaderCache", byteval, RegistryValueKind.Binary);
                    }
                }
                catch { }
        }
    }
}
