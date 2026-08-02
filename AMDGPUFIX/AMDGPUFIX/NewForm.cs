using ReaLTaiizor.Forms;
using ReaLTaiizor.Manager;
using System;
using System.Diagnostics;

namespace AMDGPUFIX
{
    public partial class NewForm : MaterialForm
    {
        // Globals
        public readonly MaterialSkinManager materialSkinManager;
        private static GPUDetection GPUDetection = new GPUDetection();
        UIHandles UIHandles = new UIHandles();
        bool AppStarted = false;
        // End Globals

        public NewForm()
        {
            InitializeComponent();
            // Apply ReaLTaiizor theming
            materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.EnforceBackcolorOnAllComponents = true;
            materialSkinManager.AddFormToManage(this);
            materialSkinManager.Theme = MaterialSkinManager.Themes.DARK;
            // Admin Check
            AdminHandles.CheckIsAdminRights();
            // GPU Handling
            GPUDetect();
            // DWM Handling
            DWMHandling();
            // GPU Driver Handling
            GPUDriverHandling();
            // Force Layout Update
            flowLayoutPanel1.PerformLayout();
            this.PerformLayout();
            // App Started Flag
            AppStarted = true;
        }

        public static string GetGPUName()
        {
            return GPUDetection.GPUName;
        }

        // GPU Driver Tweaks Handling
        private void GPUDriverHandling()
        {
            // Detect TDR Fix
            tdrFixSwitch.Checked = UIHandles.DetectTDRFix();
            // Detect HAGS Fix
            hagsFixSwitch.Checked = UIHandles.DetectHAGSFix();
            // Disable Overlays
            disableOverlaysSwitch.Checked = UIHandles.DetectDisableOverlays();
            // Detect TDR Level
            tdrLevelDropDown.SelectedIndex = UIHandles.DetectTDRLevel();
        }

        // Desktop Window Manager Handling
        private void DWMHandling()
        {
            // Detect MPO Fix
            mpoFixSwitch.Checked = UIHandles.DetectMPOFix();
            // Detect OverlayMinFPS Fix
            overlayMinFPSFixSwitch.Checked = UIHandles.DetectOverlayMinFPSFix();
        }

        private void GPUDetect()
        {
            string[] gpuInfo = GPUDetection.LoadGPUDriverVer();
            gpuName.Text += " " + gpuInfo[0];
            gpuVersion.Text += " " + gpuInfo[1];
            GPUDetection.BrandCompare();
            // UNK Flag
            driverDlButton.Enabled = !GPUDetection.disableDlDrivers;
            // AMD FEATURES
            if (GPUDetection.isAMDGpu)
            {
                // DXMOD Button
                dxModButton.Enabled = true;
                // AMD CARD
                amdCard.Visible = true;
                // Ini ShaderCache
                shaderCacheDropdown.Enabled = UIHandles.DetectShaderCache(out int index);
                shaderCacheDropdown.SelectedIndex = index;
                // Ini ULPS
                ulpsSwitch.Checked = UIHandles.DetectULPS();
            }
        }

        // Reboot Button
        private void materialFloatingActionButton2_Click(object sender, EventArgs e) =>
            Process.Start("ShutDown", "/r");

        // Open Driver Download Page Button
        private void driverDlButton_Click(object sender, EventArgs e) =>
            UIHandles.OpenURL(GPUDetection.url);

        // Donate Button
        private void materialButton6_Click(object sender, EventArgs e) =>
            UIHandles.OpenURL("https://www.paypal.com/donate/?hosted_button_id=ZURUG4V6F6LRN");

        // DX MOD Button
        private void dxModButton_Click(object sender, EventArgs e) =>
            UIHandles.OpenDXMOD();

        // BSOD Fix Button
        private void materialButton9_Click(object sender, EventArgs e) =>
            UIHandles.OpenBSODFix();

        // Open ULPS WIKI Button 
        private void materialButton1_Click(object sender, EventArgs e) =>
            UIHandles.OpenURL("https://github.com/RedDot-3ND7355/AMD-GPU-FIX/wiki/ULPS");

        // MPO Fix WIKI Button
        private void materialButton2_Click(object sender, EventArgs e) =>
            UIHandles.OpenURL("https://github.com/RedDot-3ND7355/AMD-GPU-FIX/wiki/MPO");

        // OverlayMinFPS Fix WIKI Button
        private void materialButton10_Click(object sender, EventArgs e) =>
            UIHandles.OpenURL("https://github.com/RedDot-3ND7355/MPO-GPU-FIX/wiki/OverlayMinFPS");

        // Open Shader Cache WIKI Button
        private void materialButton5_Click(object sender, EventArgs e) =>
            UIHandles.OpenURL("https://github.com/RedDot-3ND7355/MPO-GPU-FIX/wiki/SHADER-CACHE-(AMD)");

        // Open HAGS Fix WIKI Button
        private void materialButton4_Click(object sender, EventArgs e) =>
            UIHandles.OpenURL("https://github.com/RedDot-3ND7355/MPO-GPU-FIX/wiki/HAGS");

        // Open Disable Overlays WIKI Button
        private void materialButton7_Click(object sender, EventArgs e) =>
            UIHandles.OpenURL("https://github.com/RedDot-3ND7355/MPO-GPU-FIX/wiki/Disable-Overlays");

        // Open TDR Fix WIKI Button
        private void materialButton3_Click(object sender, EventArgs e) =>
            UIHandles.OpenURL("https://github.com/RedDot-3ND7355/MPO-GPU-FIX/wiki/TDR");

        // Open TDRLevel Wiki Button
        private void materialButton11_Click(object sender, EventArgs e) =>
            UIHandles.OpenURL("https://github.com/RedDot-3ND7355/MPO-GPU-FIX/wiki/TDRLevel");

        // AMD ULPS Switch Handler
        private void ulpsSwitch_CheckedChanged(object sender, EventArgs e)
        {
            if (!AppStarted) return;
            UIHandles.ULPSHandler(ulpsSwitch.Checked);
        }

        // Shader Cache Dropdown Handler
        private void shaderCacheDropdown_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!AppStarted) return;
            UIHandles.ShaderCacheHandler(shaderCacheDropdown.SelectedItem.ToString());
        }

        // MPO Fix Switch Handler
        private void mpoFixSwitch_CheckedChanged(object sender, EventArgs e)
        {
            if (!AppStarted) return;
            UIHandles.MPOFixHandler(mpoFixSwitch.Checked);
        }

        // OverlayMinFPS Fix Switch Handler
        private void overlayMinFPSFixSwitch_CheckedChanged(object sender, EventArgs e)
        {
            if (!AppStarted) return;
            UIHandles.OverlayMinFPSFixHandler(overlayMinFPSFixSwitch.Checked);
        }

        // TDR Fix Switch Handler
        private void tdrFixSwitch_CheckedChanged(object sender, EventArgs e)
        {
            if (!AppStarted) return;
            UIHandles.TDRFixHandler(tdrFixSwitch.Checked);
        }

        // HAGS Fix Switch Handler
        private void hagsFixSwitch_CheckedChanged(object sender, EventArgs e)
        {
            if (!AppStarted) return;
            UIHandles.HAGSFixHandler(hagsFixSwitch.Checked);
        }

        // Disable Overlays Switch Handler
        private void disableOverlaysSwitch_CheckedChanged(object sender, EventArgs e)
        {
            if (!AppStarted) return;
            UIHandles.DisableOverlaysFixHandler(disableOverlaysSwitch.Checked);
        }

        // TDR Level Dropdown Handler
        private void tdrLevelDropDown_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!AppStarted) return;
            UIHandles.TDRLevelHandler(tdrLevelDropDown.SelectedIndex);
        }
    }
}
