// MaterialSkin usage removed from this file; using System.Windows.Forms.MessageBox instead
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
        // PNP ids of every AMD adapter (dual GPU: only these get restarted after a switch)
        public static List<string> PNPDeviceIDs = new List<string>();
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

        // Get Profile Count to verify if compatible GPU is present
        public static int ProfileCount() => gpu_profiles.Count;

        // Ini DX verifications for profiles
        public static void IniDXHandler()
        {
            // Set Base Reg Path
            localMachine = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64); // Use 64 first
            if (localMachine == null)
                localMachine = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32); // Use 32 if 64 failed
            if (localMachine == null)
            {
                MessageBox.Show("Error! DX Navi Switches Could not set registry base path due to lack of permission.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            // Reset state (the window can be reopened; statics would otherwise pile up)
            gpu_profiles.Clear();
            CurrentDX = -1;
            foreach (int mode in DXVerif.Keys.ToList())
                DXVerif[mode] = true;
            // Count Profiles
            try
            {
                dxnaviKey = localMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Control\\Class\\{4d36e968-e325-11ce-bfc1-08002be10318}");
                if (dxnaviKey == null) return;
                var profiles = dxnaviKey.GetSubKeyNames();
                foreach (string profile in profiles)
                {
                    if (profile.Length != 4 || !profile.All(char.IsDigit)) continue;
                    using (var key = localMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Control\\Class\\{4d36e968-e325-11ce-bfc1-08002be10318}\\" + profile))
                    {
                        if (key == null) continue;
                        // Dual GPU: never touch a profile that belongs to a non-AMD adapter
                        if (!GpuRegistry.IsAmdProfile(key)) continue;
                        if (IsValidProfile(key.GetValue("D3DVendorName") as string[], key.GetValue("D3DVendorNameWow") as string[]))
                            gpu_profiles.Add("SYSTEM\\CurrentControlSet\\Control\\Class\\{4d36e968-e325-11ce-bfc1-08002be10318}\\" + profile);
                    }
                }
                if (gpu_profiles.Count == 0) return;
                DetectAvailable();
                DetectCurrent();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message + Environment.NewLine + ex.InnerException}\r\n Permission Denied!\r\n You are probably affected by a rootkit (virus)\r\n or User account that lacks permissions due to being managed by organisation.\r\n or Anti-Ransomware protection preventing registry access(such as Acronis True Image).\r\n DXMOD will be disabled to prevent any issues.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        // Both value lists must hold the 4 expected driver paths
        private static bool IsValidProfile(string[] vendorName, string[] vendorNameWow) =>
            vendorName != null && vendorNameWow != null
            && vendorName.Length >= 4 && vendorNameWow.Length >= 4
            && vendorName.Take(4).All(x => !string.IsNullOrEmpty(x))
            && vendorNameWow.Take(4).All(x => !string.IsNullOrEmpty(x));

        // Read both value lists of a profile, false if the profile is unusable
        private static bool ReadProfile(string profile, out string[] vendorName, out string[] vendorNameWow)
        {
            vendorName = null;
            vendorNameWow = null;
            using (var key = localMachine.OpenSubKey(profile))
            {
                if (key == null) return false;
                vendorName = key.GetValue("D3DVendorName") as string[];
                vendorNameWow = key.GetValue("D3DVendorNameWow") as string[];
            }
            return IsValidProfile(vendorName, vendorNameWow);
        }

        // Driver file names used by each mode: { DX9 64bit, DX11 64bit, DX9 32bit, DX11 32bit }
        private static readonly string[][] ModeFiles =
        {
            new[] { "atiumd64.dll", "atidxx64.dll", "atiumdag.dll", "atidxx32.dll" }, // 0 Regular DX
            new[] { "atiumd64.dll", "amdxx64.dll", "atiumdag.dll", "amdxx32.dll" },   // 1 Regular DX9 with DX11 NAVI
            new[] { "amdxn64.dll", "atidxx64.dll", "amdxn32.dll", "atidxx32.dll" },   // 2 DX9 NAVI with Regular DX11
            new[] { "amdxn64.dll", "amdxx64.dll", "amdxn32.dll", "amdxx32.dll" }      // 3 Full DXNAVI
        };

        // Are the driver files of a mode installed for this profile?
        private static bool ModeAvailable(int mode, string[] vendorName, string[] vendorNameWow)
        {
            string[] files = ModeFiles[mode];
            return File.Exists(Path.GetDirectoryName(vendorName[0]) + "\\" + files[0])
                && File.Exists(Path.GetDirectoryName(vendorName[3]) + "\\" + files[1])
                && File.Exists(Path.GetDirectoryName(vendorNameWow[0]) + "\\" + files[2])
                && File.Exists(Path.GetDirectoryName(vendorNameWow[3]) + "\\" + files[3]);
        }

        // Mode a profile is currently set to, -1 if unknown
        private static int ModeOf(string[] vendorName, string[] vendorNameWow)
        {
            int found = -1;
            for (int mode = 0; mode < ModeFiles.Length; mode++)
            {
                string[] files = ModeFiles[mode];
                if (vendorName[0].Contains(files[0]) && vendorName[3].Contains(files[1])
                    && vendorNameWow[0].Contains(files[2]) && vendorNameWow[3].Contains(files[3]))
                    found = mode;
            }
            return found;
        }

        // A mode is offered when at least one AMD adapter has its files
        // (dual AMD: adapters on different driver packages are verified one by one when switching)
        private static void DetectAvailable()
        {
            foreach (int mode in DXVerif.Keys.ToList())
                DXVerif[mode] = false;
            foreach (string profile in gpu_profiles)
            {
                if (!ReadProfile(profile, out string[] vendorName, out string[] vendorNameWow)) continue;
                for (int mode = 0; mode < ModeFiles.Length; mode++)
                    if (ModeAvailable(mode, vendorName, vendorNameWow))
                        DXVerif[mode] = true;
            }
        }

        // Current mode = mode of the first AMD profile that reports a known one
        private static void DetectCurrent()
        {
            CurrentDX = -1;
            foreach (string profile in gpu_profiles)
            {
                if (!ReadProfile(profile, out string[] vendorName, out string[] vendorNameWow)) continue;
                D3DVendorName = vendorName;
                D3DVendorNameWow = vendorNameWow;
                int mode = ModeOf(vendorName, vendorNameWow);
                if (mode == -1) continue;
                CurrentDX = mode;
                return;
            }
        }

        // Restart the AMD adapter(s) whose profile was changed
        private static void RestartPNPDeviceByID(List<string> changedProfiles)
        {
            if (changedProfiles.Count > 0)
            {
                // Dual GPU: map each AMD adapter to its class profile and only restart the changed ones.
                // If the mapping can't be resolved, restart every AMD adapter.
                List<string> targets = new List<string>();
                foreach (string id in PNPDeviceIDs)
                {
                    string deviceProfile = GpuRegistry.GetProfileForDevice(localMachine, id);
                    if (deviceProfile != "" && changedProfiles.Any(p => p.EndsWith("\\" + deviceProfile, StringComparison.OrdinalIgnoreCase)))
                        targets.Add(id);
                }
                if (targets.Count == 0)
                    targets = PNPDeviceIDs.ToList();
                if (targets.Count == 0)
                {
                    MessageBox.Show("Failed to restart adapter, please reboot to apply changes!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                MessageBox.Show("GPU Adapter will be restarted! Save any unsaved projects to prevent any loss of data.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                // SysNative only exists for 32bit processes on 64bit Windows
                string windows = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
                string pnputil = Path.Combine(windows, "SysNative", "pnputil.exe");
                if (!File.Exists(pnputil))
                    pnputil = Path.Combine(windows, "System32", "pnputil.exe");
                bool started = true;
                foreach (string id in targets)
                {
                    try
                    {
                        System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                        startInfo.WorkingDirectory = Environment.SystemDirectory;
                        startInfo.UseShellExecute = false;
                        startInfo.CreateNoWindow = true;
                        startInfo.FileName = pnputil;
                        startInfo.Arguments = "/restart-device \"" + id + "\"";
                        using (System.Diagnostics.Process process = System.Diagnostics.Process.Start(startInfo))
                        {
                            if (process == null)
                                started = false;
                            else
                                process.WaitForExit(15000); // one adapter at a time
                        }
                    }
                    catch
                    {
                        started = false;
                    }
                }
                if (started)
                    MessageBox.Show("Changes applied successfully!", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                else
                    MessageBox.Show("Failed to restart adapter, please reboot to apply changes!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                MessageBox.Show("Changes reverted to prevent any issues due to missing driver files for selected profile OR selected profile is already in use.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        // Switch every AMD profile to the desired mode (each one from its own current mode)
        private static void ApplyMode(int desiredDx)
        {
            List<string> changedProfiles = new List<string>();
            foreach (string profile in gpu_profiles)
            {
                if (!ReadProfile(profile, out string[] vendorName, out string[] vendorNameWow)) continue;
                int profileDx = ModeOf(vendorName, vendorNameWow);
                if (profileDx == -1 || profileDx == desiredDx) continue;
                // Mod both first, so 32bit and 64bit never end up on different modes
                string[] newWow = Switcher(vendorNameWow, profileDx, desiredDx, "D3DVendorNameWow");
                string[] newName = Switcher(vendorName, profileDx, desiredDx, "D3DVendorName");
                if (newWow.SequenceEqual(vendorNameWow) || newName.SequenceEqual(vendorName)) continue; // missing files
                using (RegistryKey key = localMachine.OpenSubKey(profile, writable: true))
                {
                    if (key == null) continue;
                    key.SetValue("D3DVendorNameWow", newWow, RegistryValueKind.MultiString);
                    key.SetValue("D3DVendorName", newName, RegistryValueKind.MultiString);
                }
                changedProfiles.Add(profile);
            }
            DetectCurrent();
            // Restart Adapter
            RestartPNPDeviceByID(changedProfiles);
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
                //else
                //    MessageBox.Show($"{filepath} exists");
            }
            return desired;
        }

        // Full Navi
        public static void SetFullNavi() => ApplyMode(3);

        // Regular DX9 with DX11 Navi
        public static void SetRDX9WDX11Navi() => ApplyMode(1);

        // Navi DX9 with regular DX11
        public static void SetDX9NaviWRDX11() => ApplyMode(2);

        // Regular DX9
        public static void RegularDX9() => ApplyMode(0);
    }
}
