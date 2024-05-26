using MaterialSkin.Controls;
using System.Diagnostics;
using System.Management;
using System.Windows.Forms;

namespace AMDGPUFIX
{
    public static class WMIFix
    {
        public static bool Notice()
        {
            if (DialogResult.OK == MaterialMessageBox.Show("WMI Has caused an error! Would you like to attempt to fix it?", "Error Detected :(", false, FlexibleMaterialForm.ButtonsPosition.Right))
            {
                Process.Start("https://www.thewindowsclub.com/how-to-repair-or-rebuild-the-wmi-repository-on-windows-10");
                return true;
            }
            else
                return false;
        }
    }
}
