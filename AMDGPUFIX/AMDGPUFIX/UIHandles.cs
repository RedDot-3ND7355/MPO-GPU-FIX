using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace AMDGPUFIX
{
    internal class UIHandles
    {
        // Globals
        SHADERCACHE shaderCache;
        ULPS ulps;
        Dxmod dxmod;
        Bsodfix _bsodfix;
        RegistryKey defaultKey = null;
        RegistryKey minfpsKey = null;
        RegistryKey tdrKey = null;
        RegistryKey overlayKey = null;
        RegistryKey hagsKey = null;
        RegistryKey tdrLevel = null;
        RegistryKey directFlipKey = null;
        // End Globals

        // System reboot confirmation dialog
        public void ShowRebootConfirmationDialog()
        {
            DialogResult result = MessageBox.Show("Do you want to reboot now?", "Reboot Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
                Process.Start(new ProcessStartInfo("shutdown", "/r") { CreateNoWindow = true, UseShellExecute = false });
        }

        // Open URL in default browser
        public void OpenURL(string url)
        {
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }

        // Detect AMD Shader Cache
        public bool DetectShaderCache(out int selectedIndex)
        {
            shaderCache = new SHADERCACHE();
            selectedIndex = shaderCache.CheckShaderCache();
            if (shaderCache.GpuProfilesCount() > 0)
                return true;
            return false;
        }

        // Detect AMD ULPS
        public bool DetectULPS()
        {
            ulps = new ULPS();
            return ulps.CheckULPS();
        }

        // TDR Level Handler
        public void TDRLevelHandler(int selectedIndex)
        {
            switch (selectedIndex)
            {
                case 1:
                    tdrLevel.SetValue("TdrLevel", 0x00000000, RegistryValueKind.DWord);
                    break;
                case 2:
                    tdrLevel.SetValue("TdrLevel", 0x00000001, RegistryValueKind.DWord);
                    break;
                case 3:
                    tdrLevel.SetValue("TdrLevel", 0x00000002, RegistryValueKind.DWord);
                    break;
                case 4:
                    tdrLevel.SetValue("TdrLevel", 0x00000003, RegistryValueKind.DWord);
                    break;
                default:
                    if (tdrLevel.GetValue("TdrLevel") != null)
                        tdrLevel.DeleteValue("TdrLevel");
                    break;
            }
        }

        // Disable Overlays Fix Handler
        public void DisableOverlaysFixHandler(bool enable)
        {
            if (enable)
                overlayKey?.SetValue("DisableOverlays", 0x00000001, RegistryValueKind.DWord);
            else
                if (overlayKey?.GetValue("DisableOverlays") != null)
                    overlayKey?.DeleteValue("DisableOverlays");
        }

        // Force Direct Flip Handler
        public void ForceDirectFlipHandler(bool enable)
        {
            if (enable)
                directFlipKey?.SetValue("GameDVR_FSEBehaviorMode", 0x00000002, RegistryValueKind.DWord);
            else
                directFlipKey?.SetValue("GameDVR_FSEBehaviorMode", 0x00000000, RegistryValueKind.DWord);
        }


        // HAGS Fix Handler
        public void HAGSFixHandler(bool enable)
        {
            if (enable)
                hagsKey?.SetValue("HwSchMode", 0x00000001, RegistryValueKind.DWord);
            else
                hagsKey?.SetValue("HwSchMode", 0x00000002, RegistryValueKind.DWord);
        }

        // TDR Fix Handler
        public void TDRFixHandler(bool enable)
        {
            if (enable)
                tdrKey?.SetValue("TdrDelay", 0x0000000A, RegistryValueKind.DWord);
            else
                if (tdrKey?.GetValue("TdrDelay") != null)
                    tdrKey?.DeleteValue("TdrDelay");
        }

        // OverlayMinFPS Fix Handler
        public void OverlayMinFPSFixHandler(bool enable)
        {
            if (enable)
                minfpsKey?.SetValue("OverlayMinFPS", 0x00000000, RegistryValueKind.DWord);
            else
                if (minfpsKey?.GetValue("OverlayMinFPS") != null)
                    minfpsKey?.DeleteValue("OverlayMinFPS");
        }

        // MPO Fix Handler
        public void MPOFixHandler(bool enable)
        {
            if (enable)
                defaultKey?.SetValue("OverlayTestMode", 0x00000005, RegistryValueKind.DWord);
            else
                if (defaultKey?.GetValue("OverlayTestMode") != null)
                    defaultKey?.DeleteValue("OverlayTestMode");
        }

        // Shader Cache Handler
        public void ShaderCacheHandler(string selectedItem)
        {
            switch (selectedItem)
            {
                case "AMD Optimized":
                    shaderCache.ShaderCacheHandler(1);
                    break;
                case "ON":
                    shaderCache.ShaderCacheHandler(0);
                    break;
                case "OFF":
                    shaderCache.ShaderCacheHandler(2);
                    break;
            }
        }

        // ULPS Switcher
        public void ULPSHandler(bool enable)
        {
            string GPUName = NewForm.GetGPUName();
            // Parentheses matter: only block *disabling* ULPS on 9000-series, not every toggle
            if (!enable && IsAmd9000Series(GPUName))
            {
                MessageBox.Show("Disabling ULPS may cause system instabilities on AMD 9000 Series GPU. Monitor your system after enabling and disable if you encounter any instability.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            ulps.ULPSHandler(enable);
        }

        // Detect OverlayMinFPS Fix
        public bool DetectOverlayMinFPSFix()
        {
            RegistryKey localMachine = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64) // 64
                     ?? RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32); // 32 fallback
            minfpsKey = localMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\DWM\\", writable: true);
            if (minfpsKey?.GetValue("OverlayMinFPS") != null)
            {
                string val = minfpsKey.GetValue("OverlayMinFPS").ToString();
                if (int.TryParse(val, out int result))
                    if (result == 0)
                        return true;
            }
            return false;
        }

        // Detect Force Direct Flip
        public bool DetectForceDirectFlip()
        {
            RegistryKey currentUser = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry64) // 64
                     ?? RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry32); // 32 fallback
            directFlipKey = currentUser.OpenSubKey("System\\GameConfigStore", writable: true);
            if (directFlipKey?.GetValue("GameDVR_FSEBehaviorMode") != null)
            {
                string val = directFlipKey.GetValue("GameDVR_FSEBehaviorMode").ToString();
                if (int.TryParse(val, out int result))
                    if (result == 2)
                        return true;
            }
            return false;
        }

        // Detect MPO Fix
        public bool DetectMPOFix()
        {
            RegistryKey localMachine = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64) // 64
                     ?? RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32); // 32 fallback
            defaultKey = localMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\DWM\\", writable: true);
            if (defaultKey?.GetValue("OverlayTestMode") != null)
            {
                string val = defaultKey.GetValue("OverlayTestMode").ToString();
                if (int.TryParse(val, out int result))
                    if (result == 5)
                        return true;
            }
            return false;
        }

        // Detect Disable Overlays
        public bool DetectDisableOverlays()
        {
            RegistryKey localMachine = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64) // 64
                     ?? RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32); // 32 fallback
            overlayKey = localMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Control\\GraphicsDrivers", writable: true);
            if (overlayKey?.GetValue("DisableOverlays") != null)
            {
                string val = overlayKey.GetValue("DisableOverlays").ToString();
                if (int.TryParse(val, out int result))
                    if (result == 1)
                        return true;
            }
            return false;
        }

        // Detect HAGS Fix
        public bool DetectHAGSFix()
        {
            RegistryKey localMachine = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64) // 64
                     ?? RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32); // 32 fallback
            hagsKey = localMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Control\\GraphicsDrivers", writable: true);
            if (hagsKey?.GetValue("HwSchMode") != null)
            {
                string val = hagsKey.GetValue("HwSchMode").ToString();
                if (int.TryParse(val, out int result))
                    if (result == 1)
                        return true;
            }
            return false;
        }

        // Detect TDR Fix
        public bool DetectTDRFix()
        {
            RegistryKey localMachine = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64) // 64
                     ?? RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32); // 32 fallback
            tdrKey = localMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Control\\GraphicsDrivers", writable: true);
            if (tdrKey?.GetValue("TdrDelay") != null)
            {
                string val = tdrKey.GetValue("TdrDelay").ToString();
                if (int.TryParse(val, out int result))
                    if (result == 10)
                        return true;
            }
            return false;
        }

        // Detect TDL Level
        public int DetectTDRLevel()
        {
            RegistryKey localMachine = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64) // 64
                     ?? RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32); // 32 fallback 
            tdrLevel = localMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Control\\GraphicsDrivers", writable: true);
            if (tdrLevel?.GetValue("TdrLevel") != null)
            {
                string val = tdrLevel?.GetValue("TdrLevel").ToString();
                if (int.TryParse(val, out int result))
                    switch (result)
                    {
                        case 0:
                            return 1;
                        case 1:
                            return 2;
                        case 2:
                            return 3;
                        case 3:
                            return 4;
                    }
            }
            return 0;
        }

        // Open DXMOD Window
        public void OpenDXMOD()
        {
            if (dxmod != null)
                dxmod.Close();
            dxmod = new Dxmod();
            if (dxmod.disabled)
            {
                MessageBox.Show("DXMOD is disabled due to no compatible DX versions found.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            dxmod.Show();
            dxmod.TopMost = true;
            dxmod.TopMost = false;
        }

        // Open BSODFIX Window
        public void OpenBSODFix()
        {
            if (_bsodfix != null)
                _bsodfix.Close();
            _bsodfix = new Bsodfix();
            _bsodfix.Show();
            _bsodfix.TopMost = true;
            _bsodfix.TopMost = false;
        }

        // RX 9000 series (9050 / 9060 / 9070, …) — do not allow disabling ULPS
        private static bool IsAmd9000Series(string gpuName)
        {
            if (string.IsNullOrEmpty(gpuName))
                return false;

            string[] markers = { "9050", "9060", "9070" };
            foreach (string marker in markers)
            {
                if (gpuName.Contains(marker, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }
    }
}
