using MaterialSkin;
using MaterialSkin.Controls;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Management;
using System.Windows.Forms;

namespace AMDGPUFIX
{
    public partial class bsodfix : MaterialForm
    {
        // globals
        public readonly MaterialSkinManager materialSkinManager;
        private RegistryKey RegistryKeyMaster = Registry.LocalMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Enum\\PCI\\", true);
        private bool ready = false;
        private Dictionary<string, string> keyValuePairs = new Dictionary<string, string>();
        internal class PCIInfo
        {
            public string Status;
            public string FriendlyName;
            public string Class;
            public string InstanceID;
            public string Service = null;
        }
        // end globals

        public bsodfix()
        {
            InitializeComponent();
            // Set material colors
            materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.EnforceBackcolorOnAllComponents = true;
            materialSkinManager.AddFormToManage(this);
            materialSkinManager.Theme = MaterialSkinManager.Themes.DARK;
            // Generetae combobox (device list)
            GenerateDropDownDevices();
            // Deselect DropDown
            materialComboBox1.SelectedIndex = -1;
            // Ready State
            ready = true;
        }

        // Get PCI Device Info by Partial ID'ing
        private List<PCIInfo> ShowAssociatedPCIDeviceInfo(string RelatedPCI)
        {
            // Get Ven
            RelatedPCI = RelatedPCI.Substring(0, RelatedPCI.IndexOf("&"));
            // Search PNP Devices
            var query = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity");
            ManagementObjectCollection moc = query.Get();
            // Enumerate
            List<PCIInfo> list = new List<PCIInfo>();
            foreach (ManagementObject mo in moc)
            {
                if (mo != null && mo.GetPropertyValue("PNPDeviceID") != null && mo.GetPropertyValue("Status") != null && mo.GetPropertyValue("Caption") != null && mo.GetPropertyValue("PNPClass") != null)
                {
                    // Create Obj
                    PCIInfo pci = new PCIInfo();
                    pci.InstanceID = mo.GetPropertyValue("PNPDeviceID").ToString();
                    pci.Status = mo.GetPropertyValue("Status").ToString();
                    pci.FriendlyName = mo.GetPropertyValue("Caption").ToString();
                    pci.Class = mo.GetPropertyValue("PNPClass").ToString();
                    if (mo.GetPropertyValue("Service") != null)
                        pci.Service = mo.GetPropertyValue("Service").ToString();
                    // Add Obj to list if validated
                    if (pci.InstanceID.Contains($"PCI\\{RelatedPCI}"))
                    {
                        list.Add(pci);
                    }
                }
            }
            return list;
        }


        // Adjust Buttons According to Status
        private void StatusSwitcher(bool MSIOnlineStatus, string PCIDevice, bool bypass = false)
        {
            if (MSIOnlineStatus)
            {
                // GUI
                materialButton1.Text = "MSISupport >ON<";
                materialButton1.Enabled = false;
                materialButton2.Text = "MSISupport OFF";
                materialButton2.Enabled = true;
                // REG
                if (!bypass)
                {
                    RegistryKey pcidevice_reg = RegistryKeyMaster.OpenSubKey($"{PCIDevice}\\Device Parameters\\Interrupt Management\\MessageSignaledInterruptProperties", true);
                    pcidevice_reg.SetValue("MSISupported", 0x01);
                }
            }
            else
            {
                // GUI
                materialButton1.Text = "MSISupport ON";
                materialButton1.Enabled = true;
                materialButton2.Text = "MSISupport >OFF<";
                materialButton2.Enabled = false;
                // REG
                if (!bypass)
                {
                    RegistryKey pcidevice_reg = RegistryKeyMaster.OpenSubKey($"{PCIDevice}\\Device Parameters\\Interrupt Management\\MessageSignaledInterruptProperties", true);
                    pcidevice_reg.SetValue("MSISupported", 0x00);
                }
            }
        }

        // Get Status of HDAudBus->MSISupport
        private bool GetStatus(string PCIDevice)
        {
            RegistryKey pcidevice_reg = RegistryKeyMaster.OpenSubKey($"{PCIDevice}\\Device Parameters\\Interrupt Management\\MessageSignaledInterruptProperties", true);
            if (pcidevice_reg != null && pcidevice_reg.GetValue("MSISupported") != null && pcidevice_reg.GetValue("MSISupported").ToString() == "1")
            {
                StatusSwitcher(true, materialComboBox1.SelectedItem.ToString() + "\\" + keyValuePairs[materialComboBox1.SelectedItem.ToString()], true);
                return true;
            }
            StatusSwitcher(false, materialComboBox1.SelectedItem.ToString() + "\\" + keyValuePairs[materialComboBox1.SelectedItem.ToString()], true);
            return false;
        }

        // Generetae combobox (device list)
        private void GenerateDropDownDevices()
        {
            foreach (string _key in RegistryKeyMaster.GetSubKeyNames())
            {
                foreach (string _subkey in RegistryKeyMaster.OpenSubKey($"{_key}", true).GetSubKeyNames())
                {
                    RegistryKey fullpath = RegistryKeyMaster.OpenSubKey($"{_key}\\{_subkey}");
                    if (fullpath.GetValue("Service") != null && fullpath.GetValue("Service").ToString() == "HDAudBus" && !materialComboBox1.Items.Contains(_key))
                    {
                        materialComboBox1.Items.Add(_key);
                        keyValuePairs.Add(_key, _subkey);
                    }
                }
            }
        }

        // MSISupport OFF
        private void materialButton2_Click(object sender, EventArgs e) =>
            StatusSwitcher(false, materialComboBox1.SelectedItem.ToString() + "\\" + keyValuePairs[materialComboBox1.SelectedItem.ToString()]);

        // MSISupport ON
        private void materialButton1_Click(object sender, EventArgs e) =>
            StatusSwitcher(true, materialComboBox1.SelectedItem.ToString() + "\\" + keyValuePairs[materialComboBox1.SelectedItem.ToString()]);

        // Change Selected Device
        private void materialComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!ready) return;
            materialButton4.Enabled = true;
            GetStatus(materialComboBox1.SelectedItem.ToString() + "\\" + keyValuePairs[materialComboBox1.SelectedItem.ToString()]);
        }

        // Get PCI Info Displayed!
        private void GetMFGInfo(string PCIDevice, string PCIVen)
        {
            List<PCIInfo> AssDevs = ShowAssociatedPCIDeviceInfo(PCIVen);
            string Devices_n_Info = "";
            int pos = 0;
            foreach (PCIInfo device in AssDevs)
            {
                Devices_n_Info += $"Device ({pos}){(device.InstanceID.ToLower().Contains(PCIDevice.ToLower()) ? " [Currently Selected Device]" : "")}\r\nInstanceID: {device.InstanceID} \r\nFriendlyName: {device.FriendlyName} {(device.Service != null ? $"\r\nService: {device.Service}" : "")}\r\nClass: {device.Class} \r\nStatus: {device.Status} \r\n\r\n";
                pos++;
            }
            MaterialMessageBox.Show($"Selected PCIDevice -> {PCIDevice} \r\n\r\nAssociated Devices: \r\n{Devices_n_Info}");
        }

        // Github info url
        private void materialButton3_Click(object sender, EventArgs e) =>
            Process.Start("https://github.com/RedDot-3ND7355/MPO-GPU-FIX/wiki/HDAUDBUS.SYS-MSI-Support");

        // Get MFG Button
        private void materialButton4_Click(object sender, EventArgs e) =>
            GetMFGInfo(materialComboBox1.SelectedItem.ToString() + "\\" + keyValuePairs[materialComboBox1.SelectedItem.ToString()], materialComboBox1.SelectedItem.ToString());
    }
}
