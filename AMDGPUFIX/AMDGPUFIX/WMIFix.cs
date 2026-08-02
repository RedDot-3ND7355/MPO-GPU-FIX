using System.Diagnostics;
using System.Windows.Forms;

namespace AMDGPUFIX
{
    public static class WMIFix
    {
        public static bool Notice()
        {
            if (DialogResult.OK == MessageBox.Show("WMI Has caused an error! Would you like to attempt to fix it?", "Error Detected :(", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning))
            {
                Process.Start(new ProcessStartInfo("https://www.thewindowsclub.com/how-to-repair-or-rebuild-the-wmi-repository-on-windows-10") { UseShellExecute = true });
                return true;
            }
            else
                return false;
        }
    }
}
