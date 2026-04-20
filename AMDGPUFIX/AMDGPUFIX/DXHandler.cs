using MaterialSkin.Controls;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace AMDGPUFIX
{
    public static class DXHandler
    {
        //
        // reference table for values
        //
        /* 
         * 64bit
        0
         Regular DX:
            atiumd64.dll
            atiumd64.dll
            atidxx64.dll
            atidxx64.dll
        1
        Regular DX9 with DX11 NAVI:
            atiumd64.dll
            atiumd64.dll
            amdxx64.dll
            amdxx64.dll
        2
        DX9 NAVI with Regular DX11:
            amdxn64.dll
            amdxn64.dll
            atidxx64.dll
            atidxx64.dll
        3
        Full DXNAVI:
            amdxn64.dll
            amdxn64.dll
            amdxx64.dll
            amdxx64.dll
        * 32bit
         0
         Regular DX:
            atiumdag.dll
            atiumdag.dll
            atidxx32.dll
            atidxx32.dll
        1
        Regular DX9 with DX11 NAVI:
            atiumdag.dll
            atiumdag.dll
            amdxx32.dll
            amdxx32.dll
        2
        DX9 NAVI with Regular DX11:
            amdxn32.dll
            amdxn32.dll
            atidxx32.dll
            atidxx32.dll
        3
        Full DXNAVI:
            amdxn32.dll
            amdxn32.dll
            amdxx32.dll
            amdxx32.dll
        */
        //
        // End reference table
        //

        // Globals
        private static List<string> gpu_profiles = new List<string>();
        private static RegistryKey localMachine;
        private static RegistryKey dxnaviKey;
        private static string[] D3DVendorName;
        private static string[] D3DVendorNameWow;
        public static string PNPDeviceID = "";
        public static int CurrentDX = -1;
        private static Dictionary<int, bool> DXVerif = new Dictionary<int, bool>()
        {
            {0 , true },
            {1 , true },
            {2 , true },
            {3 , true }
        };
        private static int LastDX = -1;
        // End

        // Ini DX verifications for profiles
        public static void IniDXHandler()
        {
            // Set Base Reg Path
            localMachine = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64); // Use 64 first
            if (localMachine == null)
                localMachine = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32); // Use 32 if 64 failed
            if (localMachine == null)
            {
                MaterialMessageBox.Show("Error! DX Navi Switches Could not set registry base path due to lack of permission.");
                return;
            }
            // Count Profiles
            try
            {
                dxnaviKey = localMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Control\\Class\\{4d36e968-e325-11ce-bfc1-08002be10318}", writable: true);
                var profiles = dxnaviKey.GetSubKeyNames();
                foreach (string profile in profiles)
                {
                    if (profile.Length == 4 && profile.All(Char.IsDigit))
                    {
                        D3DVendorName = (string[])localMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Control\\Class\\{4d36e968-e325-11ce-bfc1-08002be10318}\\" + profile).GetValue("D3DVendorName");
                        D3DVendorNameWow = (string[])localMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Control\\Class\\{4d36e968-e325-11ce-bfc1-08002be10318}\\" + profile).GetValue("D3DVendorNameWow");
                        if (D3DVendorName != null && D3DVendorNameWow != null)
                        {
                            gpu_profiles.Add("SYSTEM\\CurrentControlSet\\Control\\Class\\{4d36e968-e325-11ce-bfc1-08002be10318}\\" + profile);
                        }
                    }
                }
                DetectAvailable();
                DetectCurrent();
            }
            catch
            {
                MaterialMessageBox.Show(" Permission Denied!\r\n You are probably affected by a rootkit (virus)\r\n or User account that lacks permissions due to being managed by organisation.\r\n or Anti-Ransomware protection preventing registry access(such as Acronis True Image).\r\n Shader Cache Dropdown will be disabled to prevent any issues.");
                return;
            }
        }

        public static bool IsAvailable(int DesiredFeature = 0)
        {
            if (DXVerif[DesiredFeature])
                return true;
            else 
                return false;
        }

        private static void DetectAvailable()
        {
            // Check Regular DX9
            if (!File.Exists(Path.GetDirectoryName(D3DVendorName[0]) + "\\atiumd64.dll") || !File.Exists(Path.GetDirectoryName(D3DVendorName[3]) + "\\atidxx64.dll") || !File.Exists(Path.GetDirectoryName(D3DVendorNameWow[0]) + "\\atiumdag.dll") || !File.Exists(Path.GetDirectoryName(D3DVendorNameWow[3]) + "\\atidxx32.dll"))
            {
                DXVerif[0] = false;
            }
            // Check Regular DX9 with Navi 11
            if (!File.Exists(Path.GetDirectoryName(D3DVendorName[0]) + "\\atiumd64.dll") || !File.Exists(Path.GetDirectoryName(D3DVendorName[3]) + "\\amdxx64.dll") || !File.Exists(Path.GetDirectoryName(D3DVendorNameWow[0]) + "\\atiumdag.dll") || !File.Exists(Path.GetDirectoryName(D3DVendorNameWow[3]) + "\\amdxx32.dll"))
            {
                DXVerif[1] = false;
            }
            // Check Navi DX9 with Regular 11
            if (!File.Exists(Path.GetDirectoryName(D3DVendorName[0]) + "\\amdxn64.dll") || !File.Exists(Path.GetDirectoryName(D3DVendorName[3]) + "\\atidxx64.dll") || !File.Exists(Path.GetDirectoryName(D3DVendorNameWow[0]) + "\\amdxn32.dll") || !File.Exists(Path.GetDirectoryName(D3DVendorNameWow[3]) + "\\atidxx32.dll"))
            {
                DXVerif[2] = false;
            }
            // Check Full Navi
            if (!File.Exists(Path.GetDirectoryName(D3DVendorName[0]) + "\\amdxn64.dll") || !File.Exists(Path.GetDirectoryName(D3DVendorName[3]) + "\\amdxx64.dll") || !File.Exists(Path.GetDirectoryName(D3DVendorNameWow[0]) + "\\amdxn32.dll") || !File.Exists(Path.GetDirectoryName(D3DVendorNameWow[3]) + "\\amdxx32.dll"))
            {
                DXVerif[3] = false;
            }
        }

        private static void DetectCurrent()
        {
            CurrentDX = -1;
            // Check Regular DX9
            if (D3DVendorName[0].Contains("atiumd64.dll") && D3DVendorName[3].Contains("atidxx64.dll") && D3DVendorNameWow[0].Contains("atiumdag.dll") && D3DVendorNameWow[3].Contains("atidxx32.dll"))
            {
                CurrentDX = 0;
            }
            // Check Regular DX9 with Navi 11
            if (D3DVendorName[0].Contains("atiumd64.dll") && D3DVendorName[3].Contains("amdxx64.dll") && D3DVendorNameWow[0].Contains("atiumdag.dll") && D3DVendorNameWow[3].Contains("amdxx32.dll"))
            {
                CurrentDX = 1;
            }
            // Check Navi DX9 with Regular 11
            if (D3DVendorName[0].Contains("amdxn64.dll") && D3DVendorName[3].Contains("atidxx64.dll") && D3DVendorNameWow[0].Contains("amdxn32.dll") && D3DVendorNameWow[3].Contains("atidxx32.dll"))
            {
                CurrentDX = 2;
            }
            // Check Full Navi
            if (D3DVendorName[0].Contains("amdxn64.dll") && D3DVendorName[3].Contains("amdxx64.dll") && D3DVendorNameWow[0].Contains("amdxn32.dll") && D3DVendorNameWow[3].Contains("amdxx32.dll"))
            {
                CurrentDX = 3;
            }
        }

        // Restart GPU Adapter
        private static void RestartPNPDeviceByID()
        {
            if (LastDX != CurrentDX)
            {
                MaterialMessageBox.Show("GPU Adapter will be restarted! Save any unsaved projects to prevent any loss of data.");
                System.Diagnostics.Process process = new System.Diagnostics.Process();
                System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                startInfo.WorkingDirectory = Environment.SystemDirectory;
                startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                startInfo.FileName = "cmd.exe";
                startInfo.Arguments = "/C C:\\Windows\\SysNative\\PNPUTIL /restart-device \"" + PNPDeviceID + "\"";
                process.StartInfo = startInfo;
                if (process.Start())
                    MaterialMessageBox.Show("Changes applied successfully!");
                else
                    MaterialMessageBox.Show("Failed to restart adapter, please reboot to apply changes!");
            }
            else
            {
                MaterialMessageBox.Show("Changes reverted to prevent any issues due to missing driver files for selected profile OR selected profile is already in use.");
            }
        }

        private static string[] Switcher(string[] value, int CurrDx, int DesiredDx, string D3DObj)
        {
            LastDX = CurrDx;
            string[] desired = new string[] { "", "", "", "" };
            string[] original = value;
            // Set 32Bit
            if (D3DObj == "D3DVendorNameWow")
            {
                if (CurrDx == 0)
                {
                    if (DesiredDx == 3)
                    {
                        desired[0] = original[0].Replace("atiumdag.dll", "amdxn32.dll");
                        desired[1] = original[1].Replace("atiumdag.dll", "amdxn32.dll");
                        desired[2] = original[2].Replace("atidxx32.dll", "amdxx32.dll");
                        desired[3] = original[3].Replace("atidxx32.dll", "amdxx32.dll");
                    }
                    else if (DesiredDx == 2)
                    {
                        desired[0] = original[0].Replace("atiumdag.dll", "amdxn32.dll");
                        desired[1] = original[1].Replace("atiumdag.dll", "amdxn32.dll");
                        desired[2] = original[2];
                        desired[3] = original[3];
                    }
                    else if (DesiredDx == 1)
                    {
                        desired[0] = original[0];
                        desired[1] = original[1];
                        desired[2] = original[2].Replace("atidxx32.dll", "amdxx32.dll");
                        desired[3] = original[3].Replace("atidxx32.dll", "amdxx32.dll");
                    }
                    else
                    {
                        return original;
                    }
                }
                else if (CurrDx == 1)
                {
                    if (DesiredDx == 3)
                    {
                        desired[0] = original[0].Replace("atiumdag.dll", "amdxn32.dll");
                        desired[1] = original[1].Replace("atiumdag.dll", "amdxn32.dll");
                        desired[2] = original[2];
                        desired[3] = original[3];
                    }
                    else if (DesiredDx == 2)
                    {
                        desired[0] = original[0].Replace("atiumdag.dll", "amdxn32.dll");
                        desired[1] = original[1].Replace("atiumdag.dll", "amdxn32.dll");
                        desired[2] = original[2].Replace("amdxx32.dll", "atidxx32.dll");
                        desired[3] = original[3].Replace("amdxx32.dll", "atidxx32.dll");
                    }
                    else if (DesiredDx == 0)
                    {
                        desired[0] = original[0];
                        desired[1] = original[1];
                        desired[2] = original[2].Replace("amdxx32.dll", "atidxx32.dll");
                        desired[3] = original[3].Replace("amdxx32.dll", "atidxx32.dll");
                    }
                    else
                    {
                        return original;
                    }
                }
                else if (CurrDx == 2)
                {
                    if (DesiredDx == 3)
                    {
                        desired[0] = original[0];
                        desired[1] = original[1];
                        desired[2] = original[2].Replace("atidxx32.dll", "amdxx32.dll");
                        desired[3] = original[3].Replace("atidxx32.dll", "amdxx32.dll");
                    }
                    else if (DesiredDx == 1)
                    {
                        desired[0] = original[0].Replace("amdxn32.dll", "atiumdag.dll");
                        desired[1] = original[1].Replace("amdxn32.dll", "atiumdag.dll");
                        desired[2] = original[2].Replace("atidxx32.dll", "amdxx32.dll");
                        desired[3] = original[3].Replace("atidxx32.dll", "amdxx32.dll");
                    }
                    else if (DesiredDx == 0)
                    {
                        desired[0] = original[0].Replace("amdxn32.dll", "atiumdag.dll");
                        desired[1] = original[1].Replace("amdxn32.dll", "atiumdag.dll");
                        desired[2] = original[2];
                        desired[3] = original[3];
                    }
                    else
                    {
                        return original;
                    }
                }
                else if (CurrDx == 3)
                {
                    if (DesiredDx == 2)
                    {
                        desired[0] = original[0];
                        desired[1] = original[1];
                        desired[2] = original[2].Replace("amdxx32.dll", "atidxx32.dll");
                        desired[3] = original[3].Replace("amdxx32.dll", "atidxx32.dll");
                    }
                    else if (DesiredDx == 1)
                    {
                        desired[0] = original[0].Replace("amdxn32.dll", "atiumdag.dll");
                        desired[1] = original[1].Replace("amdxn32.dll", "atiumdag.dll");
                        desired[2] = original[2];
                        desired[3] = original[3];
                    }
                    else if (DesiredDx == 0)
                    {
                        desired[0] = original[0].Replace("amdxn32.dll", "atiumdag.dll");
                        desired[1] = original[1].Replace("amdxn32.dll", "atiumdag.dll");
                        desired[2] = original[2].Replace("amdxx32.dll", "atidxx32.dll");
                        desired[3] = original[3].Replace("amdxx32.dll", "atidxx32.dll");
                    }
                    else
                    {
                        return original;
                    }
                }
            }
            // Set 64Bit
            else if (D3DObj == "D3DVendorName")
            {
                if (CurrDx == 0)
                {
                    if (DesiredDx == 3)
                    {
                        desired[0] = original[0].Replace("atiumd64.dll", "amdxn64.dll");
                        desired[1] = original[1].Replace("atiumd64.dll", "amdxn64.dll");
                        desired[2] = original[2].Replace("atidxx64.dll", "amdxx64.dll");
                        desired[3] = original[3].Replace("atidxx64.dll", "amdxx64.dll");
                    }
                    else if (DesiredDx == 2)
                    {
                        desired[0] = original[0].Replace("atiumd64.dll", "amdxn64.dll");
                        desired[1] = original[1].Replace("atiumd64.dll", "amdxn64.dll");
                        desired[2] = original[2];
                        desired[3] = original[3];
                    }
                    else if (DesiredDx == 1)
                    {
                        desired[0] = original[0];
                        desired[1] = original[1];
                        desired[2] = original[2].Replace("atidxx64.dll", "amdxx64.dll");
                        desired[3] = original[3].Replace("atidxx64.dll", "amdxx64.dll");
                    }
                    else
                    {
                        return original;
                    }
                }
                else if (CurrDx == 1)
                {
                    if (DesiredDx == 3)
                    {
                        desired[0] = original[0].Replace("atiumd64.dll", "amdxn64.dll");
                        desired[1] = original[1].Replace("atiumd64.dll", "amdxn64.dll");
                        desired[2] = original[2];
                        desired[3] = original[3];
                    }
                    else if (DesiredDx == 2)
                    {
                        desired[0] = original[0].Replace("atiumd64.dll", "amdxn64.dll");
                        desired[1] = original[1].Replace("atiumd64.dll", "amdxn64.dll");
                        desired[2] = original[2].Replace("amdxx64.dll", "atidxx64.dll");
                        desired[3] = original[3].Replace("amdxx64.dll", "atidxx64.dll");
                    }
                    else if (DesiredDx == 0)
                    {
                        desired[0] = original[0];
                        desired[1] = original[1];
                        desired[2] = original[2].Replace("amdxx64.dll", "atidxx64.dll");
                        desired[3] = original[3].Replace("amdxx64.dll", "atidxx64.dll");
                    }
                    else
                    {
                        return original;
                    }
                }
                else if (CurrDx == 2)
                {
                    if (DesiredDx == 3)
                    {
                        desired[0] = original[0];
                        desired[1] = original[1];
                        desired[2] = original[2].Replace("atidxx64.dll", "amdxx64.dll");
                        desired[3] = original[3].Replace("atidxx64.dll", "amdxx64.dll");
                    }
                    else if (DesiredDx == 1)
                    {
                        desired[0] = original[0].Replace("amdxn64.dll", "atiumd64.dll");
                        desired[1] = original[1].Replace("amdxn64.dll", "atiumd64.dll");
                        desired[2] = original[2].Replace("atidxx64.dll", "amdxx64.dll");
                        desired[3] = original[3].Replace("atidxx64.dll", "amdxx64.dll");
                    }
                    else if (DesiredDx == 0)
                    {
                        desired[0] = original[0].Replace("amdxn64.dll", "atiumd64.dll");
                        desired[1] = original[1].Replace("amdxn64.dll", "atiumd64.dll");
                        desired[2] = original[2];
                        desired[3] = original[3];
                    }
                    else
                    {
                        return original;
                    }
                }
                else if (CurrDx == 3)
                {
                    if (DesiredDx == 2)
                    {
                        desired[0] = original[0];
                        desired[1] = original[1];
                        desired[2] = original[2].Replace("amdxx64.dll", "atidxx64.dll");
                        desired[3] = original[3].Replace("amdxx64.dll", "atidxx64.dll");
                    }
                    else if (DesiredDx == 1)
                    {
                        desired[0] = original[0].Replace("amdxn64.dll", "atiumd64.dll");
                        desired[1] = original[1].Replace("amdxn64.dll", "atiumd64.dll");
                        desired[2] = original[2];
                        desired[3] = original[3];
                    }
                    else if (DesiredDx == 0)
                    {
                        desired[0] = original[0].Replace("amdxn64.dll", "atiumd64.dll");
                        desired[1] = original[1].Replace("amdxn64.dll", "atiumd64.dll");
                        desired[2] = original[2].Replace("amdxx64.dll", "atidxx64.dll");
                        desired[3] = original[3].Replace("amdxx64.dll", "atidxx64.dll");
                    }
                    else
                    {
                        return original;
                    }
                }
            }
            // Verify to make sure the files are present to prevent any issues.
            int set = 0;
            foreach (string filepath in desired)
            {
                set++;
                if (!File.Exists(filepath))
                {
                    desired = original;
                    break;
                }
                else
                    MessageBox.Show($"{filepath} exists");
            }
            return desired;
        }

        // Full Navi
        public static void SetFullNavi()
        {
            foreach (string profile in gpu_profiles)
            {
                dxnaviKey = localMachine.OpenSubKey(profile, writable: true);
                // Set 32bit (get, mod, apply)
                D3DVendorNameWow = (string[])dxnaviKey.GetValue("D3DVendorNameWow");
                D3DVendorNameWow = Switcher(D3DVendorNameWow, CurrentDX, 3, "D3DVendorNameWow");
                dxnaviKey.SetValue("D3DVendorNameWow", D3DVendorNameWow, RegistryValueKind.MultiString);
                // Set 64bit
                D3DVendorName = (string[])dxnaviKey.GetValue("D3DVendorName");
                D3DVendorName = Switcher(D3DVendorName, CurrentDX, 3, "D3DVendorName");
                dxnaviKey.SetValue("D3DVendorName", D3DVendorName, RegistryValueKind.MultiString);
            }
            DetectCurrent();
            // Restart Adapter
            RestartPNPDeviceByID();
        }

        // Regular DX9 with DX11 Navi
        public static void SetRDX9WDX11Navi()
        {
            foreach (string profile in gpu_profiles)
            {
                dxnaviKey = localMachine.OpenSubKey(profile, writable: true);
                // Set 32bit (get, mod, apply)
                D3DVendorNameWow = (string[])dxnaviKey.GetValue("D3DVendorNameWow");
                D3DVendorNameWow = Switcher(D3DVendorNameWow, CurrentDX, 1, "D3DVendorNameWow");
                dxnaviKey.SetValue("D3DVendorNameWow", D3DVendorNameWow, RegistryValueKind.MultiString);
                // Set 64bit
                D3DVendorName = (string[])dxnaviKey.GetValue("D3DVendorName");
                D3DVendorName = Switcher(D3DVendorName, CurrentDX, 1, "D3DVendorName");
                dxnaviKey.SetValue("D3DVendorName", D3DVendorName, RegistryValueKind.MultiString);
            }
            DetectCurrent();
            // Restart Adapter
            RestartPNPDeviceByID();
        }

        // Navi DX9 with regular DX11
        public static void SetDX9NaviWRDX11()
        {
            foreach (string profile in gpu_profiles)
            {
                dxnaviKey = localMachine.OpenSubKey(profile, writable: true);
                // Set 32bit (get, mod, apply)
                D3DVendorNameWow = (string[])dxnaviKey.GetValue("D3DVendorNameWow");
                D3DVendorNameWow = Switcher(D3DVendorNameWow, CurrentDX, 2, "D3DVendorNameWow");
                dxnaviKey.SetValue("D3DVendorNameWow", D3DVendorNameWow, RegistryValueKind.MultiString);
                // Set 64bit
                D3DVendorName = (string[])dxnaviKey.GetValue("D3DVendorName");
                D3DVendorName = Switcher(D3DVendorName, CurrentDX, 2, "D3DVendorName");
                dxnaviKey.SetValue("D3DVendorName", D3DVendorName, RegistryValueKind.MultiString);
            }
            DetectCurrent();
            // Restart Adapter
            RestartPNPDeviceByID();
        }

        // Regular DX9
        public static void RegularDX9()
        {
            foreach (string profile in gpu_profiles)
            {
                dxnaviKey = localMachine.OpenSubKey(profile, writable: true);
                // Set 32bit (get, mod, apply)
                D3DVendorNameWow = (string[])dxnaviKey.GetValue("D3DVendorNameWow");
                D3DVendorNameWow = Switcher(D3DVendorNameWow, CurrentDX, 0, "D3DVendorNameWow");
                dxnaviKey.SetValue("D3DVendorNameWow", D3DVendorNameWow, RegistryValueKind.MultiString);
                // Set 64bit
                D3DVendorName = (string[])dxnaviKey.GetValue("D3DVendorName");
                D3DVendorName = Switcher(D3DVendorName, CurrentDX, 0, "D3DVendorName");
                dxnaviKey.SetValue("D3DVendorName", D3DVendorName, RegistryValueKind.MultiString);
            }
            DetectCurrent();
            // Restart Adapter
            RestartPNPDeviceByID();
        }
    }
}
