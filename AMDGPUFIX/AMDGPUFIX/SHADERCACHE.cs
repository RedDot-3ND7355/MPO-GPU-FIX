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
                shadercacheKey = localMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Control\\Class\\{4d36e968-e325-11ce-bfc1-08002be10318}" , writable: true);
                var profiles = shadercacheKey.GetSubKeyNames();
                foreach (string profile in profiles)
                {
                    if (profile.Length == 4 && profile.All(Char.IsDigit))
                    {
                        shadercacheKey = localMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Control\\Class\\{4d36e968-e325-11ce-bfc1-08002be10318}\\" + profile + "\\UMD", writable: true);
                        if (shadercacheKey != null)
                        {
                            gpu_profiles.Add("SYSTEM\\CurrentControlSet\\Control\\Class\\{4d36e968-e325-11ce-bfc1-08002be10318}\\" + profile + "\\UMD");
                        }
                    }
                }
            } 
            catch
            {
                MaterialMessageBox.Show(" Permission Denied!\r\n You are probably affected by a rootkit (virus)\r\n or User account that lacks permissions due to being managed by organisation.\r\n or Anti-Ransomware protection preventing registry access.\r\n Shader Cache Dropdown will be disabled to prevent any issues.");
                return -1;
            }
            // Check Values
            int intval = -1;
            foreach (string profile in gpu_profiles)
            {
                shadercacheKey = localMachine.OpenSubKey(profile, writable: true);
                if (shadercacheKey != null && shadercacheKey.GetValue("ShaderCache") != null)
                {
                    string result = "";
                    var valueKind = shadercacheKey.GetValueKind("ShaderCache");
                    if (valueKind == RegistryValueKind.Binary)
                    {
                        var value = (byte[])shadercacheKey.GetValue("ShaderCache");
                        result = BitConverter.ToString(value);
                    }
                    switch (result)
                    {
                        case "32-00":
                            intval = 0; // ON
                            break;
                        case "31-00":
                            intval = 1; // AMD Optimized
                            break;
                        case "30-00":
                            intval = 2; // OFF
                            break;
                        default:
                            intval = -1; // UNKN, bad profile?
                            break;
                    }
                }
            }
            return intval;
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
                if (shadercacheKey != null && shadercacheKey.GetValue("ShaderCache") != null)
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
