using System.Diagnostics;
using System.IO;

namespace AMDGPUFIX
{
    public static class RadeonConflictHelper
    {
        /// <summary>
        /// Detects leftover pre-Adrenalin RadeonSettings.exe that fights with modern Adrenalin over registry values.
        /// If both the old and new executables are present, renames the old one to .bak so it can no longer interfere.
        /// Returns true if a rename was performed.
        /// </summary>
        public static bool NeutralizeOldRadeonSettings()
        {
            const string modernExe = @"C:\Program Files\AMD\CNext\CNext\RadeonSoftware.exe";
            const string oldExe = @"C:\Program Files\AMD\CNext\CNext\RadeonSettings.exe";
            const string backup = @"C:\Program Files\AMD\CNext\CNext\RadeonSettings.exe.bak";
            try
            {
                // Only act when both exist (classic leftover situation)
                if (!File.Exists(modernExe) || !File.Exists(oldExe))
                    return false;
                // Kill any running instance of the old panel first
                foreach (var proc in Process.GetProcessesByName("RadeonSettings"))
                {
                    try
                    {
                        proc.Kill();
                        proc.WaitForExit(4000);
                    }
                    catch { /* already exiting or access denied – ignore */ }
                }
                // Rename only if the backup doesn't already exist
                if (!File.Exists(backup))
                {
                    File.Move(oldExe, backup);
                    return true; // successfully neutralized
                }
            }
            catch { /* Ignore exceptions */ }
            return false;
        }
    }
}
