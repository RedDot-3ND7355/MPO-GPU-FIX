# MPO-GPU-FIX

This small and compact tool disables MPO on windows that causes issues with GPU Drivers.
Check out the WIKI for more info: https://github.com/RedDot-3ND7355/MPO-GPU-FIX/wiki

Already Disabled MPO and still getting performance issues and such?
Head here: https://github.com/RedDot-3ND7355/MPO-GPU-FIX/wiki/Still-getting-issues%3F

What this tool does:
- Checks your GPU Name in use
- Checks your GPU Driver version
- Toggles ULPS/MPO/TDR via the Registry
- Gracefully Reboots your pc
- Download redirects to your brand's support page for latest drivers

1. Run the tool.

2. Toggle ON the fix.

(Optional) ULPS Toggle will be available for AMD GPU users. (also requires restart for changes to apply)

(Optional) TDR Fix Toggle will be available for those having TDR related errors when playing games. (also requires restart for changes to apply)

3. Hit the reboot button.


That's it!

Note: To restore MPO, just open the tool and Toggle off the fix then reboot once again.


Changelog

v3(latest release):
- TDR Fix toggle added

v2(second release):
- ULPS Toggle added
- Run as admin only
- All-in-one executeable
- Proper GPU Brand detection

v1(Initial release):
- MPO Toggle
