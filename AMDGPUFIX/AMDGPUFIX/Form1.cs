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
        string GPUName = "Unknown";
        string GPUVersion = "???";
        string url = string.Empty;
        static RegistryKey defaultKey = null;
        static RegistryKey tdrKey = null;
        static RegistryKey hagsKey = null;
        static RegistryKey tdrLevel = null;
        bool Ready = false;
        ULPS ULPS = new ULPS();
        SHADERCACHE shdrch = new SHADERCACHE();
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
            DetectSHADERCACHE();
            DetectMPO();
            DetectHAGS();
            DetectTDRLevel();
            Ready = true;
        }

        private void DetectTDRLevel()
        {
            RegistryKey localMachine = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
            tdrLevel = localMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Control\\GraphicsDrivers", writable: true);
            if (tdrLevel.GetValue("TdrLevel") != null)
            {
                string val = tdrLevel.GetValue("TdrLevel").ToString();
                if (int.TryParse(val, out int result))
                    switch (result)
                    {
                        case 0:
                            materialComboBox2.SelectedIndex = 1;
                            break;
                        case 1:
                            materialComboBox2.SelectedIndex = 2;
                            break;
                        case 2:
                            materialComboBox2.SelectedIndex = 3;
                            break;
                        case 3:
                            materialComboBox2.SelectedIndex = 4;
                            break;
                    }

            }
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
        // Detect Ultra-Low Power State (AMD)
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
        // Detect Shader Cache Setting (AMD)
        // 
        private void DetectSHADERCACHE()
        {
            if (!materialLabel3.Visible)
            {
                materialComboBox1.SelectedIndex = shdrch.CheckShaderCache();
                if (materialComboBox1.SelectedIndex < 0)
                    materialComboBox1.Enabled = false;
            }
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
            if (GPUName.Contains("AMD") || GPUName.Contains("Radeon") || GPUName.Contains("Vega") || GPUName.Contains("Advanced Micro Devices"))
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

        // Debug function - requires Newtonsoft.Json Import
        //private string Serialize(Object o)
        //{
        //    string json = JsonConvert.SerializeObject(o, Formatting.Indented);
        //    return json;
        //}

        //
        // Load GPU Driver Version
        //
        ManagementObjectSearcher drvsearcher = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController");
        private void LoadGPUDriverVer()
        {
            ManagementObjectCollection items = drvsearcher.Get();
            // GPU Name & Driver
            if (items != null)
            {
                // debug int
                // int ioii = 0;
                foreach (ManagementObject mo in items)
                {
                    if (mo != null)
                    {
                        // Debug dumper
                        // File.WriteAllText(ioii + ".dump", Serialize(mo.Properties));
                        foreach (PropertyData property in mo.Properties)
                        {
                            if (property != null)
                            {
                                // Get GPUName 
                                if (property.Name == "Name")
                                    GPUName = property.Value.ToString();
                                // Get GPUVersion
                                if (property.Name == "DriverVersion")
                                    GPUVersion = property.Value.ToString();
                                // Validate APU, Status and GPU
                                if (VerifyifAPU(GPUName) || (property.Name == "ConfigManagerErrorCode" && property.Value.ToString() == "22") || (property.Name == "Availability" && property.Value.ToString() == "8") || (property.Name == "PNPDeviceID" && !property.Value.ToString().Contains("PCI")))
                                {
                                    GPUVersion = "";
                                    GPUName = "";
                                    break;
                                }
                            }
                        }
                        if (GPUVersion.Length > 0 && GPUName.Length > 0)
                            break;
                    }
                }
                // End
                materialLabel1.Text += GPUName;
                materialLabel2.Text += GPUVersion;
            }
            else // RESTORE WMI
            {
                if (WMIFix.Notice())
                {
                    MaterialMessageBox.Show("Don't forget to reboot to apply changes after fixing your WMI Repository!");
                    Application.Exit();
                }
                else
                {
                    MaterialMessageBox.Show("This app requires WMI Repository to work. Cancelled, closing...");
                    Application.Exit();
                }
            }
        }
        //
        // End
        //

        //
        // Begin Verification of APU
        //
        string[] blacklistedgpunames = { "HD Graphics", "UHD Graphics", "RX Vega Graphics" };
        string[] whitelist = { "56", "64" };
        private bool VerifyifAPU(string gpuname) // true IF apu
        {
            foreach (string name in blacklistedgpunames)
            {
                if (!CheckWhiteList(gpuname) && gpuname.Contains(name))
                    return true;
            }
            return false;
        }

        private bool CheckWhiteList(string wlitem) // false IF not GPU whitelist
        {
            bool triggered = false;
            foreach (string name in whitelist)
                if (wlitem.Contains(name))
                    triggered = true;
            if (triggered)
                return true;
            return false;
        }
        //
        // End Verification of APU
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
        // ULPS Switch (AMD)
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
        // Shade Cache ComboBox (AMD)
        // 
        private void materialComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!Ready) return;
            switch (materialComboBox1.SelectedItem.ToString())
            {
                case "AMD Optimized":
                    shdrch.ShaderCacheHandler(1);
                    break;
                case "ON":
                    shdrch.ShaderCacheHandler(0);
                    break;
                case "OFF":
                    shdrch.ShaderCacheHandler(2);
                    break;
            }
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
        // TDR Level Dropdown
        //
        private void materialComboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!Ready) return;
            if (materialComboBox2.SelectedIndex > 0) // -1
            {
                switch (materialComboBox2.SelectedIndex)
                {
                    case 1:
                        tdrLevel.SetValue("TdrLevel", 0, RegistryValueKind.DWord); // 0
                        materialFloatingActionButton2.Visible = true;
                        break;
                    case 2:
                        tdrLevel.SetValue("TdrLevel", 1, RegistryValueKind.DWord); // 1
                        materialFloatingActionButton2.Visible = true;
                        break;
                    case 3:
                        tdrLevel.SetValue("TdrLevel", 2, RegistryValueKind.DWord); // 2
                        materialFloatingActionButton2.Visible = true;
                        break;
                    case 4:
                        tdrLevel.SetValue("TdrLevel", 3, RegistryValueKind.DWord); // 3
                        materialFloatingActionButton2.Visible = true;
                        break;
                }
            }
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

        //
        // SHADER CACHE Info Button
        //
        private void materialButton5_Click(object sender, EventArgs e) =>
            Process.Start("https://github.com/RedDot-3ND7355/MPO-GPU-FIX/wiki/SHADER-CACHE-(AMD)");
        //
        // End
        //

        //
        // DONATE Button
        //
        private void materialButton6_Click(object sender, EventArgs e) =>
            Process.Start("https://www.paypal.com/donate/?hosted_button_id=ZURUG4V6F6LRN");
        //
        // End
        //

        //
        // TDR Level Info Button
        //
        private void materialButton7_Click(object sender, EventArgs e) =>
            Process.Start("https://github.com/RedDot-3ND7355/MPO-GPU-FIX/wiki/TDRLevel");
        //
        // End
        //
    }
}
