using System.Diagnostics;
using System.Security.Principal;
using System.Windows.Forms;

namespace AMDGPUFIX
{
    public static class AdminHandles
    {
        public static void CheckIsAdminRights()
        {
            bool isElevated;
            using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
            {
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                isElevated = principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            if (!isElevated)
            {
                MessageBox.Show("Please run as admin!");
                Process.GetCurrentProcess().Kill();
            }
        }
    }
}
