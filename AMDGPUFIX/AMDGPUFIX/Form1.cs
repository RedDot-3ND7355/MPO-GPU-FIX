using MaterialSkin.Controls;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Management;

namespace AMDGPUFIX
{
    public partial class Form1 : MaterialForm
    {
        // Globals
        string GPUName = "";
        string GPUVersion = "";
        static RegistryKey defaultKey = null;
        bool Ready = false;
        // End

        public Form1()
        {
            InitializeComponent();
            LoadGPUDriverVer();
            DriverCompare();
            DetectMPO();
            Ready= true;
        }

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
        // Driver Compare
        //
        private void DriverCompare()
        {
            string current = GPUVersion.Replace(".", "");
            string expected = "3101202910015";
            materialLabel2.HighEmphasis = (current != expected);
            materialFloatingActionButton1.Enabled = (current != expected);
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
        // Download Latest Driver with Black screen fix
        //
        private void materialFloatingActionButton1_Click(object sender, EventArgs e) =>
            Process.Start("https://drivers.amd.com/drivers/whql-amd-software-adrenalin-edition-22.11.2-win10-win11-dec8.exe");
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
    }
}
