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
        static RegistryKey tdrKey = null;
        static RegistryKey hagsKey= null;
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
            DetectTDR();
            DetectULPS();
            DetectMPO();
            DetectHAGS();
            Ready = true;
        }

        //
        // Detect Hardware GPU Scheduler
        //
        private void DetectHAGS()
        {
            RegistryKey localMachine = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
            hagsKey = localMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Control\\GraphicsDrivers", writable: true);
            if (hagsKey.GetValue("HwSchMode") != null)
            {
                string val = hagsKey.GetValue("HwSchMode").ToString();
                if (int.TryParse(val, out int result))
                    if (result == 1)
                        materialSwitch4.Checked = true;
            }
        }
        //
        // End
        //

        //
        // Detect Timeout detection and recovery
        //
        private void DetectTDR()
        {
            RegistryKey localMachine = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
            tdrKey = localMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Control\\GraphicsDrivers", writable: true);
            if (tdrKey.GetValue("TdrDelay") != null)
            {
                string val = tdrKey.GetValue("TdrDelay").ToString();
                if (int.TryParse(val, out int result))
                    if (result == 10)
                        materialSwitch3.Checked = true;
            }
        }
        //
        // End
        //

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
            if (GPUName.Contains("AMD") || GPUName.Contains("Radeon"))
            {
                url = "https://www.amd.com/en/support";
                materialLabel3.Visible = false;
            }
            // NVIDIA Brand
            else if (GPUName.Contains("NVIDIA") || GPUName.Contains("RTX") || GPUName.Contains("GTX"))
            {
                url = "https://www.nvidia.com/download/index.aspx";
                materialLabel3.Text = "NVIDIA GPU";
                materialLabel3.Visible = true;
            }
            // INTEL Brand
            else if (GPUName.Contains("INTEL") || GPUName.Contains("ARC"))
            {
                url = "https://www.intel.ca/content/www/ca/en/download/726609/intel-arc-iris-xe-graphics-whql-windows.html?";
                materialLabel3.Text = "INTEL GPU";
                materialLabel3.Visible = true;
            }
            // Unknown Brand
            else
            {
                materialFloatingActionButton1.Enabled = false;
                materialLabel3.Text = "UNKWN GPU";
                materialLabel3.Visible = true;
            }
        }
        //
        // End
        //

        //
        // Load GPU Driver Version
        //
        ManagementObjectSearcher drvsearcher = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController");
        private void LoadGPUDriverVer()
        {
            // GPU Name & Driver
            foreach (ManagementObject mo in drvsearcher.Get())
            {
                foreach (PropertyData property in mo.Properties)
                {
                    // Get GPUName 
                    if (property.Name == "Name")
                        GPUName = property.Value.ToString();
                    // Get GPUVersion
                    if (property.Name == "DriverVersion")
                        GPUVersion = property.Value.ToString();
                    // Validate GPU
                    if (property.Name == "PNPDeviceID")
                        if (!property.Value.ToString().Contains("PCI"))
                        {
                            GPUVersion = "";
                            GPUName = "";
                        }
                        else break;
                }
                if (GPUVersion.Length > 0 && GPUName.Length > 0)
                    break;
            }
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
        // TDR Switch
        //
        private void materialSwitch3_CheckedChanged(object sender, EventArgs e)
        {
            if (!Ready) return;
            if (materialSwitch3.Checked)
                tdrKey.SetValue("TdrDelay", 0x0000000A, RegistryValueKind.DWord);
            else
                tdrKey.DeleteValue("TdrDelay");
            materialFloatingActionButton2.Visible = true;
        }
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
        // HAGS Switch
        //
        private void materialSwitch4_CheckedChanged(object sender, EventArgs e)
        {
            if (!Ready) return;
            if (materialSwitch4.Checked)
                hagsKey.SetValue("HwSchMode", 0x00000001, RegistryValueKind.DWord);
            else
                hagsKey.SetValue("HwSchMode", 0x00000002, RegistryValueKind.DWord);
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

        //
        // TDR Info Button
        //
        private void materialButton3_Click(object sender, EventArgs e) =>
            Process.Start("https://github.com/RedDot-3ND7355/MPO-GPU-FIX/wiki/TDR");
        //
        // End
        //

        //
        // HAGS Info Button
        //
        private void materialButton4_Click(object sender, EventArgs e) =>
            Process.Start("https://github.com/RedDot-3ND7355/MPO-GPU-FIX/wiki/HAGS");
        //
        // End
        //
    }
}
