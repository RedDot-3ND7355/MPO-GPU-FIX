using Microsoft.Win32;
using System;
using System.Text.RegularExpressions;

namespace AMDGPUFIX
{
    /// <summary>
    /// Helpers for the display-adapter class key
    /// (HKLM\SYSTEM\CurrentControlSet\Control\Class\{4d36e968-...}\000X).
    /// On dual / hybrid GPU systems that key holds one profile per adapter, of any brand,
    /// so AMD-only tweaks must check which adapter a profile belongs to before writing to it.
    /// </summary>
    internal static class GpuRegistry
    {
        public const string ClassGuid = "{4d36e968-e325-11ce-bfc1-08002be10318}";

        private static readonly Regex VenId = new(
            @"VEN_([0-9A-Fa-f]{4})",
            RegexOptions.CultureInvariant | RegexOptions.Compiled);

        /// <summary>
        /// False only when the profile clearly belongs to another vendor (NVIDIA, Intel, …).
        /// Profiles that can't be identified are accepted, which keeps single-GPU behaviour unchanged.
        /// </summary>
        public static bool IsAmdProfile(RegistryKey profileKey)
        {
            if (profileKey == null)
                return false;

            try
            {
                // e.g. "PCI\VEN_1002&DEV_73BF&..."
                string matchingId = profileKey.GetValue("MatchingDeviceId")?.ToString() ?? "";
                Match m = VenId.Match(matchingId);
                if (m.Success)
                    return m.Groups[1].Value.Equals("1002", StringComparison.OrdinalIgnoreCase);

                string provider = profileKey.GetValue("ProviderName")?.ToString() ?? "";
                if (provider.Contains("NVIDIA", StringComparison.OrdinalIgnoreCase)
                    || provider.Contains("Intel", StringComparison.OrdinalIgnoreCase)
                    || provider.Contains("Microsoft", StringComparison.OrdinalIgnoreCase))
                    return false;
            }
            catch
            {
                // Unreadable values: fall through and accept, like the pre-dual-GPU behaviour
            }

            return true;
        }

        /// <summary>
        /// Class profile ("0000", "0001", …) the given PNP device is bound to, or "" when unknown.
        /// </summary>
        public static string GetProfileForDevice(RegistryKey localMachine, string pnpDeviceId)
        {
            if (localMachine == null || string.IsNullOrEmpty(pnpDeviceId))
                return "";

            try
            {
                using RegistryKey key = localMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Enum\\" + pnpDeviceId);
                // e.g. "{4d36e968-e325-11ce-bfc1-08002be10318}\0001"
                string driver = key?.GetValue("Driver")?.ToString() ?? "";
                int slash = driver.LastIndexOf('\\');
                if (slash < 0 || !driver.StartsWith(ClassGuid, StringComparison.OrdinalIgnoreCase))
                    return "";
                return driver.Substring(slash + 1);
            }
            catch
            {
                return "";
            }
        }
    }
}
