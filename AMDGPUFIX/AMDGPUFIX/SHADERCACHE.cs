using MaterialSkin.Controls;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AMDGPUFIX
{
    public class SHADERCACHE
    {
        private List<string> gpu_profiles = new List<string>();
        private RegistryKey shadercacheKey = null;

        // Value Table
        // 1 = AMD Optimized | 31-00
        // 0 = ON            | 32-00
        // 2 = OFF           | 30-00

        // Check profile count externally
        public int GpuProfilesCount()
        {
            return gpu_profiles.Count;
        }

        // Check last profile's value to return
        public int CheckShaderCache()
        {
            // Registry Check
            RegistryKey localMachine = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64); // Use 64 first
            if (localMachine == null)
                localMachine = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32); // Use 32 if 64 failed
            if (localMachine == null)
            {
                MaterialMessageBox.Show("Error! Could not set registry base path due to lack of permission.");
                return -1;
            }
            // Count Profiles
            try
            {
                shadercacheKey = localMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Control\\Class\\{4d36e968-e325-11ce-bfc1-08002be10318}");
                if (shadercacheKey == null) { return -1; }
                var profiles = shadercacheKey.GetSubKeyNames();
                foreach (string profile in profiles)
                {
                    if (profile.Length == 4 && profile.All(Char.IsDigit))
                    {
                        shadercacheKey = localMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Control\\Class\\{4d36e968-e325-11ce-bfc1-08002be10318}\\" + profile + "\\UMD");
                        if (shadercacheKey != null)
                        {
                            gpu_profiles.Add("SYSTEM\\CurrentControlSet\\Control\\Class\\{4d36e968-e325-11ce-bfc1-08002be10318}\\" + profile + "\\UMD");
                        }
                    }
                }
            } 
            catch (Exception ex)
            {
                MaterialMessageBox.Show($"{ex.Message + Environment.NewLine + ex.Source}\r\n Permission Denied!\r\n You are probably affected by a rootkit (virus)\r\n or User account that lacks permissions due to being managed by organisation.\r\n or Anti-Ransomware protection preventing registry access(such as Acronis True Image).\r\n Shader Cache Dropdown will be disabled to prevent any issues.");
                return -1;
            }
            // Check Values
            foreach (string profile in gpu_profiles)
            {
                shadercacheKey = localMachine.OpenSubKey(profile, writable: true);
                if (shadercacheKey != null && shadercacheKey.GetValue("ShaderCache") != null)
                {
                    byte[] result = new byte[0];
                    var valueKind = shadercacheKey.GetValueKind("ShaderCache");
                    if (valueKind == RegistryValueKind.Binary)
                    {
                        var value = (byte[])shadercacheKey.GetValue("ShaderCache");
                        result = value;
                    }
                    /* New detection */
                    if (result != null && result.Length >= 2)
                    {
                        // Primary Check
                        if (result[1] == 0x00)
                        {
                            if (result[0] == 0x32)      // ON (32 00)
                                return 0; // Enabled
                            else if (result[0] == 0x31) // Optimized (31 00)
                                return 1; // Default
                            else if (result[0] == 0x30) // OFF (30 00)
                                return 2; // Disabled
                        }
                        MaterialMessageBox.Show("Unknown ShaderCache value type detected in registry: " + BitConverter.ToString(result) + "\r\n could be future update changing the value. Please report this to RedDot3ND on github.");
                    }
                    else if (result == null || result == new byte[0])
                    {
                        MaterialMessageBox.Show("ShaderCache value is null, using AMD Optimized as default value.\r\nDriver update removed this value.");
                        return 1;
                    }
                }
                else
                {
                    MaterialMessageBox.Show("No ShaderCache profile has been set, using AMD Optimized as default value.\r\nDriver update removed this value.");
                    return 1;
                }
            }
            return -1;
        }

        // Set value to all profiles
        public void ShaderCacheHandler(int value)
        {
            byte[] byteval = null;
            RegistryKey localMachine = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
            // Table out values!
            switch (value)
            {
                case 0:
                    byteval = GetBytes("32-00");
                    break;
                case 1:
                    byteval = GetBytes("31-00");
                    break; 
                case 2:
                    byteval = GetBytes("30-00");
                    break;
            }

            // Set it!
            foreach (string profile in gpu_profiles)
            {
                shadercacheKey = localMachine.OpenSubKey(profile, writable: true);
                if (shadercacheKey != null /*&& shadercacheKey.GetValue("ShaderCache") != null*/)
                    shadercacheKey.SetValue("ShaderCache", byteval, RegistryValueKind.Binary);
            }
        }

        // Convert Data to Byte[]
        private byte[] GetBytes(string value)
        {
            var data = value.Split('-').Select(x => Convert.ToByte(x, 16)).ToArray();
            return data;
        }
    }
}
