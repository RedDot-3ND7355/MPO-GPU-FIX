using Microsoft.Win32;
using ReaLTaiizor.Forms;
using ReaLTaiizor.Manager;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Management;
using System.Windows.Forms;

namespace AMDGPUFIX
{
    public partial class Bsodfix : MaterialForm
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

        public Bsodfix()
        {
            InitializeComponent();
            // Apply ReaLTaiizor theming
            materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.EnforceBackcolorOnAllComponents = true;
            materialSkinManager.AddFormToManage(this);
            materialSkinManager.Theme = MaterialSkinManager.Themes.DARK;
            // Generetae combobox (device list)
            GenerateDropDownDevices();
            // Deselect DropDown & Disable Card
            materialComboBox2.SelectedIndex = -1;
            materialCard1.Enabled = false;
            // Ready State
            ready = true;
        }

        // Get PCI Device Info by Partial ID'ing
        private List<PCIInfo> ShowAssociatedPCIDeviceInfo(string RelatedPCI)
        {
            // Get Ven
            int amp = RelatedPCI.IndexOf('&');
            if (amp > 0)
                RelatedPCI = RelatedPCI.Substring(0, amp);

            // Search PNP Devices
            var query = new ManagementObjectSearcher(
                "SELECT PNPDeviceID, Status, Caption, PNPClass, Service FROM Win32_PnPEntity");
            ManagementObjectCollection moc = query.Get();
            // Enumerate
            List<PCIInfo> list = new List<PCIInfo>();
            foreach (ManagementObject mo in moc)
            {
                using (mo)
                {
                    if (mo == null) continue;
                    object pnpObj = mo.GetPropertyValue("PNPDeviceID");
                    object statusObj = mo.GetPropertyValue("Status");
                    object captionObj = mo.GetPropertyValue("Caption");
                    object classObj = mo.GetPropertyValue("PNPClass");
                    if (pnpObj == null || statusObj == null || captionObj == null || classObj == null)
                        continue;

                    PCIInfo pci = new PCIInfo
                    {
                        InstanceID = pnpObj.ToString(),
                        Status = statusObj.ToString(),
                        FriendlyName = captionObj.ToString(),
                        Class = classObj.ToString()
                    };
                    object serviceObj = mo.GetPropertyValue("Service");
                    if (serviceObj != null)
                        pci.Service = serviceObj.ToString();

                    if (pci.InstanceID.IndexOf($"PCI\\{RelatedPCI}", StringComparison.OrdinalIgnoreCase) >= 0)
                        list.Add(pci);
                }
            }
            return list;
        }

        // Adjust Buttons According to Status
        private void StatusSwitcher(bool MSIOnlineStatus, string PCIDevice, bool bypass = false)
        {
            if (bypass) return;

            RegistryKey pcidevice_reg = RegistryKeyMaster?.OpenSubKey(
                $"{PCIDevice}\\Device Parameters\\Interrupt Management\\MessageSignaledInterruptProperties", true);
            if (pcidevice_reg == null) return;

            using (pcidevice_reg)
            {
                pcidevice_reg.SetValue("MSISupported", MSIOnlineStatus ? 0x01 : 0x00, RegistryValueKind.DWord);
            }
        }

        // Get Status of MSISupported
        private bool GetStatus(string PCIDevice)
        {
            RegistryKey pcidevice_reg = RegistryKeyMaster?.OpenSubKey(
                $"{PCIDevice}\\Device Parameters\\Interrupt Management\\MessageSignaledInterruptProperties", true);
            if (pcidevice_reg != null)
            {
                using (pcidevice_reg)
                {
                    object val = pcidevice_reg.GetValue("MSISupported");
                    if (val != null && val.ToString() == "1")
                        return true;
                }
            }
            return false;
        }

        // Generetae combobox (HDAudBus devices only — MSI BSOD/latency fix target)
        private void GenerateDropDownDevices()
        {
            if (RegistryKeyMaster == null) return;

            foreach (string _key in RegistryKeyMaster.GetSubKeyNames())
            {
                using (RegistryKey deviceKey = RegistryKeyMaster.OpenSubKey(_key, false))
                {
                    if (deviceKey == null) continue;

                    foreach (string _subkey in deviceKey.GetSubKeyNames())
                    {
                        using (RegistryKey fullpath = deviceKey.OpenSubKey(_subkey, false))
                        {
                            if (fullpath == null) continue;
                            object serviceObj = fullpath.GetValue("Service");
                            if (serviceObj == null) continue;
                            if (!serviceObj.ToString().Equals("HDAudBus", StringComparison.OrdinalIgnoreCase))
                                continue;
                            if (materialComboBox2.Items.Contains(_key)) continue;

                            materialComboBox2.Items.Add(_key);
                            keyValuePairs[_key] = _subkey;
                        }
                    }
                }
            }
        }

        // Change Selected Device
        private void materialComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!ready) return;
            if (materialComboBox2.SelectedItem == null) return;

            string hw = materialComboBox2.SelectedItem.ToString();
            if (!keyValuePairs.TryGetValue(hw, out string instance)) return;

            string path = hw + "\\" + instance;
            materialCard1.Enabled = true;
            materialSwitch1.Checked = GetStatus(path);
            materialLabel9.Text = $"Selected Device: {GetDeviceFriendlyName(path)}";
        }

        private string GetDeviceFriendlyName(string hardwareId)
        {
            if (string.IsNullOrWhiteSpace(hardwareId))
                return "Unknown Device";
            try
            {
                using (RegistryKey key = RegistryKeyMaster?.OpenSubKey(hardwareId))
                {
                    if (key != null)
                    {
                        object friendlyName = key.GetValue("FriendlyName");
                        if (friendlyName != null)
                            return friendlyName.ToString();
                        object deviceDesc = key.GetValue("DeviceDesc");
                        if (deviceDesc != null)
                        {
                            string desc = deviceDesc.ToString();
                            if (desc.Contains(";"))
                            {
                                string[] parts = desc.Split(';');
                                return parts[parts.Length - 1];
                            }
                            return desc;
                        }
                    }
                }
            }
            catch { }
            return "Generic Hardware Device";
        }

        // Get PCI Info Displayed!
        private void GetMFGInfo(string PCIDevice, string PCIVen)
        {
            List<PCIInfo> AssDevs = ShowAssociatedPCIDeviceInfo(PCIVen);
            string Devices_n_Info = "";
            int pos = 0;
            foreach (PCIInfo device in AssDevs)
            {
                Devices_n_Info +=
                    $"Device ({pos}){(device.InstanceID.IndexOf(PCIDevice, StringComparison.OrdinalIgnoreCase) >= 0 ? " [Currently Selected Device]" : "")}\r\n" +
                    $"InstanceID: {device.InstanceID}\r\n" +
                    $"FriendlyName: {device.FriendlyName}" +
                    (device.Service != null ? $"\r\nService: {device.Service}" : "") +
                    $"\r\nClass: {device.Class}\r\nStatus: {device.Status}\r\n\r\n";
                pos++;
            }
            MessageBox.Show(
                $"Selected PCIDevice -> {PCIDevice}\r\n\r\nAssociated Devices:\r\n{Devices_n_Info}",
                "Info",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        // Get MFG Button
        private void materialButton4_Click(object sender, EventArgs e)
        {
            if (materialComboBox2.SelectedItem == null) return;
            string hw = materialComboBox2.SelectedItem.ToString();
            if (!keyValuePairs.TryGetValue(hw, out string instance)) return;
            GetMFGInfo(hw + "\\" + instance, hw);
        }

        // Open MSI Support WIKI Button
        private void materialButton6_Click(object sender, EventArgs e) =>
            Process.Start(new ProcessStartInfo("https://github.com/RedDot-3ND7355/MPO-GPU-FIX/wiki/HDAUDBUS.SYS-MSI-Support") { UseShellExecute = true });

        // MSI Switch
        private void materialSwitch1_CheckedChanged(object sender, EventArgs e)
        {
            if (!ready) return;
            if (materialComboBox2.SelectedItem == null) return;
            string hw = materialComboBox2.SelectedItem.ToString();
            if (!keyValuePairs.TryGetValue(hw, out string instance)) return;
            StatusSwitcher(materialSwitch1.Checked, hw + "\\" + instance);
        }
    }
}
