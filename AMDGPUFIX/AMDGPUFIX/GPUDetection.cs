using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace AMDGPUFIX
{
    /// <summary>PCI / software GPU vendor classification.</summary>
    public enum GpuVendor
    {
        Unknown = 0,
        AMD,
        Nvidia,
        Intel,
        Microsoft,
        Virtual,
        Other
    }

    /// <summary>One display adapter discovered via WMI.</summary>
    public sealed class GpuAdapter
    {
        public string Name { get; init; } = "Unknown";
        public string DriverVersion { get; init; } = "???";
        public string PnpDeviceId { get; init; } = "";
        public string VendorId { get; init; } = "";
        public string DeviceId { get; init; } = "";
        public GpuVendor Vendor { get; init; } = GpuVendor.Unknown;
        public bool IsIntegrated { get; init; }
        public bool IsVirtual { get; init; }
        public bool IsPci { get; init; }
        public bool IsEnabled { get; init; } = true;
        public ulong AdapterRamBytes { get; init; }
        public string DriverServiceName { get; init; } = "";
        public int Score { get; set; }

        /// <summary>Real, enabled hardware GPU worth targeting for fixes.</summary>
        public bool IsUsable =>
            IsEnabled && IsPci && !IsVirtual
            && Vendor is not (GpuVendor.Microsoft or GpuVendor.Virtual or GpuVendor.Unknown);
    }

    /// <summary>
    /// Discovers GPUs via WMI, classifies brand from PCI VEN_ IDs (with driver/name fallbacks),
    /// separates iGPU vs dGPU, and picks a primary adapter for the UI.
    /// </summary>
    internal class GPUDetection
    {
        // ── Public surface (kept compatible with NewForm / UIHandles) ──────────
        public string GPUName = "Unknown";
        public string GPUVersion = "???";
        public bool isAMDGpu;
        public string url = "https://www.amd.com/en/support";
        public bool enableDXMOD;
        public bool disableDlDrivers;
        public GpuVendor Vendor = GpuVendor.Unknown;

        public IReadOnlyList<GpuAdapter> Adapters => _adapters;
        public GpuAdapter SelectedAdapter { get; private set; }

        private readonly List<GpuAdapter> _adapters = new();

        // PCI vendor IDs (lowercase hex, no 0x)
        private const string VenAmd = "1002";
        private const string VenAmdSys = "1022"; // AMD host/system devices (rare as VideoController)
        private const string VenNvidia = "10de";
        private const string VenIntel = "8086";
        private const string VenMicrosoft = "1414";
        private const string VenVmware = "15ad";
        private const string VenVirtualBox = "80ee";
        private const string VenRedHatVirtio = "1af4";
        private const string VenParallels = "1ab8";
        private const string VenQemu = "1234";
        private const string VenCitrix = "5853";

        private static readonly Regex PciVenDev = new(
            @"VEN_([0-9A-Fa-f]{4}).*DEV_([0-9A-Fa-f]{4})",
            RegexOptions.CultureInvariant | RegexOptions.Compiled);

        // Explicit discrete product tokens — if present, treat as dGPU even when "Graphics" appears
        private static readonly Regex DiscreteNameMarker = new(
            @"\b(" +
            @"RX\s*\d|Radeon\s+RX|Radeon\s+Pro|Radeon\s+VII|" +
            @"FirePro|FireGL|Instinct|Radeon\s+HD\s*\d{3,4}|" +
            @"RTX|GTX|GT\s*\d{3,4}|GeForce|Quadro|Tesla|Titan|NVS|" +
            @"Arc\s*(A|B)?\d{3,4}|Data\s*Center\s*GPU" +
            @")\b",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

        // AMD iGPU / APU style marketing names (no discrete product line)
        private static readonly Regex AmdIntegratedName = new(
            @"(" +
            @"Radeon\s*\(TM\)\s*Graphics|" +
            @"Radeon™\s*Graphics|" +
            @"AMD\s+Radeon(\s*\(TM\))?(\s*™)?\s+Graphics|" +
            @"Radeon\s+Graphics|" +
            @"RX\s*Vega\s*Graphics|" +                 // Raven Ridge etc. (not RX Vega 56/64)
            @"Radeon\s+Vega\s+\d+\s+Graphics|" +       // Vega 8/11 APU
            @"Radeon\s+\d{3,4}M\s+Graphics|" +         // 780M / 890M style
            @"Graphics\s*$" +                          // "... 780M Graphics" already covered; fallback
            @")",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

        private static readonly string[] VirtualNameHints =
        {
            "microsoft basic display",
            "microsoft remote display",
            "microsoft hyper-v",
            "hyper-v video",
            "vmware",
            "virtualbox",
            "vbox",
            "parallels",
            "citrix",
            "remote desktop",
            "indirect display",
            "virtual gpu",
            "virtio",
            "qxl",
            "bochs",
            "google virtual",
            "virtual adapter"
        };

        private static readonly string[] NonHardwarePnpPrefixes =
        {
            "ROOT\\",
            "SWD\\",
            "USB\\",
            "HTREE\\",
            "UMB\\"
        };

        // Valid Win32_VideoController properties only.
        // NOTE: "DriverName" is NOT a Win32_VideoController field — selecting it throws
        // ManagementException (Invalid query) and used to surface a false WMI-repair dialog.
        private const string VideoControllerQuery =
            "SELECT Name, DriverVersion, PNPDeviceID, AdapterRAM, " +
            "ConfigManagerErrorCode, Availability, Status, VideoProcessor, InstalledDisplayDrivers " +
            "FROM Win32_VideoController";

        /// <summary>
        /// Enumerate adapters, pick a primary GPU, set <see cref="GPUName"/> / <see cref="GPUVersion"/>,
        /// and stash PNP id for DXHandler.
        /// </summary>
        public string[] LoadGPUDriverVer()
        {
            _adapters.Clear();
            SelectedAdapter = null;
            GPUName = "Unknown";
            GPUVersion = "???";
            DXHandler.PNPDeviceID = "";

            if (!TryQueryVideoControllers(out string errorDetail))
            {
                // Only prompt for WMI repair when the repository/query actually failed —
                // not when zero adapters parse (unlikely) or a soft empty result.
                HandleWmiFailure(errorDetail);
                return new[] { "", "" };
            }

            // Score & choose
            foreach (GpuAdapter adapter in _adapters)
                adapter.Score = ScoreAdapter(adapter);

            SelectedAdapter = SelectPrimaryAdapter(_adapters);

            if (SelectedAdapter != null)
            {
                GPUName = SelectedAdapter.Name;
                GPUVersion = string.IsNullOrWhiteSpace(SelectedAdapter.DriverVersion)
                    ? "???"
                    : SelectedAdapter.DriverVersion;
                DXHandler.PNPDeviceID = SelectedAdapter.PnpDeviceId ?? "";
            }
            else if (_adapters.Count > 0)
            {
                // Fallback: anything we saw (e.g. only Basic Display)
                GpuAdapter fallback = _adapters.OrderByDescending(a => a.Score).First();
                GPUName = fallback.Name;
                GPUVersion = fallback.DriverVersion;
                DXHandler.PNPDeviceID = fallback.PnpDeviceId ?? "";
                SelectedAdapter = fallback;
            }

            return new[] { GPUName, GPUVersion };
        }

        /// <summary>
        /// Runs WMI enumeration. Returns false only when WMI itself fails (not when GPU list is empty).
        /// </summary>
        private bool TryQueryVideoControllers(out string errorDetail)
        {
            errorDetail = null;

            // Primary: explicit property list (faster, clear schema)
            if (TryFillAdapters(VideoControllerQuery, out errorDetail))
                return true;

            // Fallback: SELECT * (matches the old app behaviour; survives odd schema quirks)
            if (TryFillAdapters("SELECT * FROM Win32_VideoController", out errorDetail))
                return true;

            return false;
        }

        private bool TryFillAdapters(string wql, out string errorDetail)
        {
            errorDetail = null;
            try
            {
                using var searcher = new ManagementObjectSearcher(wql);
                using ManagementObjectCollection items = searcher.Get();

                // Get() does not return null on a healthy stack; empty collection is valid WMI.
                if (items == null)
                {
                    errorDetail = "WMI returned a null collection for Win32_VideoController.";
                    return false;
                }

                foreach (ManagementBaseObject raw in items)
                {
                    if (raw is not ManagementObject mo)
                        continue;

                    using (mo)
                    {
                        GpuAdapter adapter = ParseAdapter(mo);
                        if (adapter != null)
                            _adapters.Add(adapter);
                    }
                }

                return true;
            }
            catch (ManagementException ex)
            {
                _adapters.Clear();
                errorDetail = ex.Message;
                return false;
            }
            catch (Exception ex) when (ex is UnauthorizedAccessException
                                       or System.Runtime.InteropServices.COMException)
            {
                _adapters.Clear();
                errorDetail = ex.Message;
                return false;
            }
        }

        /// <summary>
        /// Resolve brand flags, driver download URL, and AMD-feature enablement.
        /// Call after <see cref="LoadGPUDriverVer"/>.
        /// </summary>
        public void BrandCompare()
        {
            bool anyAmd = _adapters.Any(a =>
                a.Vendor == GpuVendor.AMD && a.IsEnabled && !a.IsVirtual);

            GpuVendor primaryVendor = SelectedAdapter?.Vendor ?? GpuVendor.Unknown;
            Vendor = primaryVendor;

            // AMD-only tools (ULPS, Shader Cache, DXMOD) apply if *any* real AMD GPU is present
            // (covers hybrid: NVIDIA dGPU primary + AMD APU still in the box).
            isAMDGpu = anyAmd;
            enableDXMOD = anyAmd;
            disableDlDrivers = false;

            GpuVendor urlVendor = primaryVendor;
            if (urlVendor is GpuVendor.Unknown or GpuVendor.Microsoft or GpuVendor.Virtual or GpuVendor.Other)
            {
                // Prefer a usable adapter's vendor for the download button
                GpuAdapter usable = _adapters
                    .Where(a => a.IsUsable)
                    .OrderByDescending(a => a.Score)
                    .FirstOrDefault();
                if (usable != null)
                    urlVendor = usable.Vendor;
                else if (anyAmd)
                    urlVendor = GpuVendor.AMD;
            }

            switch (urlVendor)
            {
                case GpuVendor.AMD:
                    url = "https://www.amd.com/en/support";
                    break;
                case GpuVendor.Nvidia:
                    url = "https://www.nvidia.com/Download/index.aspx";
                    break;
                case GpuVendor.Intel:
                    url = "https://www.intel.com/content/www/us/en/download-center/home.html";
                    break;
                default:
                    url = string.Empty;
                    disableDlDrivers = true;
                    break;
            }
        }

        // ── Parsing ────────────────────────────────────────────────────────────

        private static GpuAdapter ParseAdapter(ManagementObject mo)
        {
            string name = GetString(mo, "Name");
            if (string.IsNullOrWhiteSpace(name))
                return null;

            string pnp = GetString(mo, "PNPDeviceID");
            string driverVersion = GetString(mo, "DriverVersion");
            // InstalledDisplayDrivers is comma-separated .dll/.sys paths — useful brand hint.
            // DriverName is NOT on Win32_VideoController.
            string installedDrivers = GetString(mo, "InstalledDisplayDrivers");
            string videoProcessor = GetString(mo, "VideoProcessor");

            ParsePciIds(pnp, out string ven, out string dev);

            bool isPci = !string.IsNullOrEmpty(pnp)
                         && pnp.StartsWith("PCI\\", StringComparison.OrdinalIgnoreCase);

            ushort errorCode = GetUInt16(mo, "ConfigManagerErrorCode");
            ushort availability = GetUInt16(mo, "Availability");
            // 0 = OK; 22 = device disabled. Availability 8 = offline.
            // If Availability is missing/0, don't treat as offline (GetUInt16 returns 0 on failure).
            bool enabled = errorCode == 0 && availability != 8;

            ulong ram = GetUInt64(mo, "AdapterRAM");

            GpuVendor vendor = ResolveVendor(ven, installedDrivers, name, videoProcessor);
            bool isVirtual = vendor == GpuVendor.Virtual
                             || vendor == GpuVendor.Microsoft
                             || IsVirtualName(name)
                             || IsVirtualName(videoProcessor);

            bool isIntegrated = !isVirtual && DetectIntegrated(vendor, name, dev);

            return new GpuAdapter
            {
                Name = name.Trim(),
                DriverVersion = string.IsNullOrWhiteSpace(driverVersion) ? "???" : driverVersion.Trim(),
                PnpDeviceId = pnp ?? "",
                VendorId = ven ?? "",
                DeviceId = dev ?? "",
                Vendor = vendor,
                IsIntegrated = isIntegrated,
                IsVirtual = isVirtual,
                IsPci = isPci,
                IsEnabled = enabled,
                AdapterRamBytes = ram,
                DriverServiceName = installedDrivers ?? ""
            };
        }

        private static void ParsePciIds(string pnpDeviceId, out string vendorId, out string deviceId)
        {
            vendorId = "";
            deviceId = "";
            if (string.IsNullOrEmpty(pnpDeviceId))
                return;

            Match m = PciVenDev.Match(pnpDeviceId);
            if (!m.Success)
                return;

            vendorId = m.Groups[1].Value.ToLowerInvariant();
            deviceId = m.Groups[2].Value.ToLowerInvariant();
        }

        /// <param name="driverHint">
        /// InstalledDisplayDrivers paths or similar (atikmpag, nvldumd, igdumdim64, …).
        /// </param>
        private static GpuVendor ResolveVendor(string ven, string driverHint, string name, string videoProcessor)
        {
            // 1) PCI vendor id — most reliable
            if (!string.IsNullOrEmpty(ven))
            {
                switch (ven)
                {
                    case VenAmd:
                    case VenAmdSys:
                        return GpuVendor.AMD;
                    case VenNvidia:
                        return GpuVendor.Nvidia;
                    case VenIntel:
                        return GpuVendor.Intel;
                    case VenMicrosoft:
                        return GpuVendor.Microsoft;
                    case VenVmware:
                    case VenVirtualBox:
                    case VenRedHatVirtio:
                    case VenParallels:
                    case VenQemu:
                    case VenCitrix:
                        return GpuVendor.Virtual;
                }
            }

            // 2) Installed display driver file paths (not kernel service names)
            if (!string.IsNullOrEmpty(driverHint))
            {
                string d = driverHint.ToLowerInvariant();
                if (d.Contains("ati") || d.Contains("amd") || d.Contains("amdxc") || d.Contains("amdxn"))
                    return GpuVendor.AMD;
                if (d.Contains("nvld") || d.Contains("nvd3d") || d.Contains("nvwgf") || d.Contains("nvgpu"))
                    return GpuVendor.Nvidia;
                if (d.Contains("igdu") || d.Contains("iglk") || d.Contains("intel") || d.Contains("ixe"))
                    return GpuVendor.Intel;
                if (d.Contains("basicdisplay") || d.Contains("framebuf"))
                    return GpuVendor.Microsoft;
            }

            // 3) Marketing name / video processor string
            string blob = $"{name} {videoProcessor}";
            if (IsVirtualName(blob))
                return GpuVendor.Virtual;

            if (ContainsAny(blob, "AMD", "Radeon", "Advanced Micro Devices", "ATI "))
                return GpuVendor.AMD;
            if (ContainsAny(blob, "NVIDIA", "GeForce", "Quadro", "Tesla", "Titan"))
                return GpuVendor.Nvidia;
            // GTX/RTX alone — only if not already classified (avoid random false positives)
            if (Regex.IsMatch(blob, @"\b(GTX|RTX)\s*\d", RegexOptions.IgnoreCase))
                return GpuVendor.Nvidia;
            if (ContainsAny(blob, "Intel", "Iris", "UHD Graphics", "HD Graphics")
                || Regex.IsMatch(blob, @"\bArc\s*(A|B)?\d{3,4}\b", RegexOptions.IgnoreCase))
                return GpuVendor.Intel;
            if (ContainsAny(blob, "Microsoft Basic", "Microsoft Remote"))
                return GpuVendor.Microsoft;

            return GpuVendor.Unknown;
        }

        private static bool DetectIntegrated(GpuVendor vendor, string name, string deviceId)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            // Strong discrete product markers win
            if (DiscreteNameMarker.IsMatch(name))
                return false;

            switch (vendor)
            {
                case GpuVendor.Intel:
                    // Discrete Intel Arc (Axxx / Bxxx). Other Intel graphics are almost always iGPU.
                    if (Regex.IsMatch(name, @"\bArc\b", RegexOptions.IgnoreCase)
                        && Regex.IsMatch(name, @"\b(A|B)?\d{3,4}\b", RegexOptions.IgnoreCase))
                        return false;
                    if (ContainsAny(name, "HD Graphics", "UHD Graphics", "Iris", "Graphics"))
                        return true;
                    // Unnamed Intel adapters default to integrated (safer for hybrid laptops)
                    return true;

                case GpuVendor.AMD:
                    // Discrete Vega 56/64 use "RX Vega 56" / "RX Vega 64" — caught by DiscreteNameMarker.
                    // Integrated Vega / GCN APUs use "RX Vega Graphics" or "Radeon Graphics".
                    if (AmdIntegratedName.IsMatch(name))
                    {
                        // Guard: RX 560 / 640 / 6500 etc. must not look like iGPU
                        if (Regex.IsMatch(name, @"\bRX\s*\d{3,4}\b", RegexOptions.IgnoreCase))
                            return false;
                        return true;
                    }
                    // "Radeon 780M" without the word Graphics
                    if (Regex.IsMatch(name, @"Radeon\s+\d{3,4}M\b", RegexOptions.IgnoreCase))
                        return true;
                    return false;

                case GpuVendor.Nvidia:
                    // Consumer NVIDIA is discrete; Max-Q etc. still dGPU for our purposes
                    return false;

                default:
                    return false;
            }
        }

        private static bool IsVirtualName(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return false;
            string lower = text.ToLowerInvariant();
            foreach (string hint in VirtualNameHints)
            {
                if (lower.Contains(hint))
                    return true;
            }
            return false;
        }

        private static bool IsNonHardwarePnp(string pnp)
        {
            if (string.IsNullOrEmpty(pnp))
                return true;
            foreach (string prefix in NonHardwarePnpPrefixes)
            {
                if (pnp.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        // ── Selection ──────────────────────────────────────────────────────────

        private static int ScoreAdapter(GpuAdapter a)
        {
            int score = 0;

            if (!a.IsEnabled) score -= 1000;
            if (a.IsVirtual) score -= 800;
            if (a.Vendor == GpuVendor.Microsoft) score -= 700;
            if (a.Vendor == GpuVendor.Unknown) score -= 100;
            if (!a.IsPci) score -= 400;

            // Prefer discrete over integrated (MPO/TDR users usually care about the dGPU)
            if (a.IsUsable && !a.IsIntegrated) score += 300;
            else if (a.IsUsable && a.IsIntegrated) score += 120;

            // Mild brand boost so AMD dGPU beats equal-score unknown edge cases in this app
            if (a.Vendor == GpuVendor.AMD && !a.IsIntegrated) score += 20;
            else if (a.Vendor == GpuVendor.Nvidia) score += 15;
            else if (a.Vendor == GpuVendor.Intel && !a.IsIntegrated) score += 10;

            // AdapterRAM is often wrong on modern WDDM, but a non-zero value still helps ranking
            if (a.AdapterRamBytes > 0)
            {
                // Cap bonus (~512 MB units, max +64)
                ulong mb = a.AdapterRamBytes / (1024UL * 1024UL);
                score += (int)Math.Min(mb / 512UL, 64UL);
            }

            // Prefer longer/more specific marketing names over generic stubs
            if (a.Name.Length > 12) score += 5;

            return score;
        }

        private static GpuAdapter SelectPrimaryAdapter(List<GpuAdapter> adapters)
        {
            if (adapters == null || adapters.Count == 0)
                return null;

            // Prefer usable hardware; if none, highest score overall
            IEnumerable<GpuAdapter> usable = adapters.Where(a => a.IsUsable);
            IEnumerable<GpuAdapter> pool = usable.Any() ? usable : adapters;

            return pool
                .OrderByDescending(a => a.Score)
                .ThenBy(a => a.IsIntegrated) // discrete first when scores tie
                .ThenByDescending(a => a.Name?.Length ?? 0)
                .FirstOrDefault();
        }

        // ── WMI helpers ────────────────────────────────────────────────────────

        private static string GetString(ManagementObject mo, string property)
        {
            try
            {
                object value = mo[property];
                return value?.ToString() ?? "";
            }
            catch
            {
                return "";
            }
        }

        private static ushort GetUInt16(ManagementObject mo, string property)
        {
            try
            {
                object value = mo[property];
                if (value == null) return 0;
                return Convert.ToUInt16(value);
            }
            catch
            {
                return 0;
            }
        }

        private static ulong GetUInt64(ManagementObject mo, string property)
        {
            try
            {
                object value = mo[property];
                if (value == null) return 0;
                // AdapterRAM is often reported as uint / int
                return Convert.ToUInt64(value);
            }
            catch
            {
                return 0;
            }
        }

        private static bool ContainsAny(string haystack, params string[] needles)
        {
            if (string.IsNullOrEmpty(haystack))
                return false;
            foreach (string needle in needles)
            {
                if (haystack.IndexOf(needle, StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
            }
            return false;
        }

        private static void HandleWmiFailure(string detail = null)
        {
            // Keep the repair prompt, but include the real exception so this is debuggable
            // (previously an invalid WQL property looked like a broken WMI repository).
            if (!string.IsNullOrWhiteSpace(detail))
            {
                MessageBox.Show(
                    "GPU detection could not query Win32_VideoController.\r\n\r\n" + detail,
                    "WMI Query Failed",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }

            if (WMIFix.Notice())
            {
                MessageBox.Show(
                    "Don't forget to reboot to apply changes after fixing your WMI Repository!",
                    "Info",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                Application.Exit();
            }
            else
            {
                MessageBox.Show(
                    "This app requires WMI Repository to work. Cancelled, closing...",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                Application.Exit();
            }
        }
    }
}
