using MaterialSkin;
using MaterialSkin.Controls;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Management;
using System.Windows.Forms;

namespace AMDGPUFIX
{
    public partial class Form1 : MaterialForm
    {
        // Globals
        string GPUName = "";
        string GPUVersion = "";
        string url = string.Empty;
        static RegistryKey defaultKey = null;
        bool Ready = false;
        ULPS ULPS = new ULPS();
        public readonly MaterialSkinManager materialSkinManager;
        // End

        public Form1()
        {
            InitializeComponent();
            // Set material colors
            materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.EnforceBackcolorOnAllComponents = true;
            materialSkinManager.AddFormToManage(this);
            materialSkinManager.Theme = MaterialSkinManager.Themes.DARK;
            // continue...
            LoadGPUDriverVer();
            BrandCompare();
            DetectULPS();
            DetectMPO();
            Ready = true;
        }

        //
        // Detect Ultra-Low Power State
        // 
        private void DetectULPS()
        {
            if (!materialLabel3.Visible)
                materialSwitch2.Checked = ULPS.CheckULPS();
        }
        //
        // End
        //

        //
        // Detect MultiPlane-Overlay
        //
        private void DetectMPO()
        {
            RegistryKey localMachine = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
            defaultKey = localMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\DWM\\", writable: true);
            if (defaultKey.GetValue("OverlayTestMode") != null)
            {
                string val = defaultKey.GetValue("OverlayTestMode").ToString();
                if (int.TryParse(val, out int result))
                    if (result == 5)
                        materialSwitch1.Checked = true;
            }
        }
        //
        // End
        //

        //
        // GPU Brand Compare
        //
        private void BrandCompare()
        {
            // AMD Brand
            if (GPUName.Contains("AMD"))
            {
                url = "https://www.amd.com/en/support";
                materialLabel3.Visible = false;
            }
            // NVIDIA Brand
            else
                url = "https://www.nvidia.com/download/index.aspx";
        }
        //
        // End
        //

        //
        // Load GPU Driver Version
        //
        ManagementObjectSearcher gpusearcher = new ManagementObjectSearcher("SELECT * FROM Win32_DisplayConfiguration");
        ManagementObjectSearcher drvsearcher = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController");
        private void LoadGPUDriverVer()
        {
            // GPU Name
            foreach (ManagementObject mo in gpusearcher.Get())
                foreach (PropertyData property in mo.Properties)
                    if (property.Name == "DeviceName")
                        GPUName = property.Value.ToString();
            // End
            // GPU Driver
            foreach (ManagementObject mo in drvsearcher.Get())
                foreach (PropertyData property in mo.Properties)
                    if (property.Name == "DriverVersion")
                        GPUVersion = property.Value.ToString();
            // End
            materialLabel1.Text += GPUName;
            materialLabel2.Text += GPUVersion;
        }
        //
        // End
        //

        //
        // Download Latest Driver
        //
        private void materialFloatingActionButton1_Click(object sender, EventArgs e) =>
            Process.Start(url);
        //
        // End
        //

        //
        // ULPS Switch
        //
        private void materialSwitch2_CheckedChanged(object sender, EventArgs e)
        {
            if (!Ready) return;
            ULPS.ULPSHandler(materialSwitch2.Checked);
            materialFloatingActionButton2.Visible = true;
        }
        //
        // End
        //

        //
        // MPO Switch
        //
        private void materialSwitch1_CheckedChanged(object sender, EventArgs e)
        {
            if (!Ready) return;
            if (materialSwitch1.Checked)
                defaultKey.SetValue("OverlayTestMode", 0x00000005, RegistryValueKind.DWord);
            else
                defaultKey.DeleteValue("OverlayTestMode");
            materialFloatingActionButton2.Visible = true;
        }
        //
        // End
        //

        //
        // Reboot PC Button
        //
        private void materialFloatingActionButton2_Click(object sender, EventArgs e) =>
            Process.Start("ShutDown", "/r");
        //
        // End
        //

        //
        // ULPS Info Button
        //
        private void materialButton1_Click(object sender, EventArgs e) =>
            Process.Start("https://github.com/RedDot-3ND7355/AMD-GPU-FIX/wiki/ULPS");
        //
        // End
        //

        //
        // MPO Info Button
        //
        private void materialButton2_Click(object sender, EventArgs e) =>
            Process.Start("https://github.com/RedDot-3ND7355/AMD-GPU-FIX/wiki/MPO");
        //
        // End
        //

        //
        // Kill Process when closed
        //
        private void Form1_FormClosed(object sender, FormClosedEventArgs e) =>
            Process.GetCurrentProcess().Kill();
        //
        // End
        //
    }
}
