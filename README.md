## Having issues after installing a (new) gpu in your system?

Then this is the tool for you!

Here's a list of symptoms caused by MPO (a feature added by Microsoft a few years back):
- Stutters when tabbing out of games (multitasking)
- Driver crashes/System hangs when using hardware accelerated browsers
- Driver crashes/System hangs when trying to stream
- Anything fullscreen would cause huge stutters and can lead to crashes and hangs
- Screen flickering nonstop

List of GPU affected by MPO as reported by users:
- NVIDIA RTX 30 Series
- NVIDIA RTX 20 Series
- NVIDIA GTX 1600 Series
- AMD RX 5000 Series
- AMD RX 6000 Series
- AMD RX 7000 Series
- And maybe more!

Why did I make this tool?

Because I was also plagued by these issues and found out about MPO 2 weeks after enduring pure torture of having
a decent pc not being able to do jacksh**t. So I decided to help everyone out by making the process a lot easier.
I know that NVIDIA released Reg files to disable/restore MPO, but I wanted to make a user friendly experience.
Also because I got time on my hand during winter x')

# MPO-GPU-FIX

This small and compact tool disables MPO on windows that causes issues with GPU Drivers.
Check out the WIKI for more info: https://github.com/RedDot-3ND7355/MPO-GPU-FIX/wiki

Already Disabled MPO and still getting performance issues and such?
Head here: https://github.com/RedDot-3ND7355/MPO-GPU-FIX/wiki/Still-getting-issues%3F

What this tool does:
- Checks your GPU Name in use
- Checks your GPU Driver version
- Toggles ULPS/MPO/TDR/HAGS/SHADER CACHE via the Registry
- Gracefully Reboots your pc
- Download redirects to your brand's support page for latest drivers

1. Run the tool.

2. Toggle ON the fix.

(Optional) ULPS Toggle will be available for AMD GPU users. (also requires restart for changes to apply)

(Optional) TDR Fix Toggle will be available for those having TDR related errors when playing games. (also requires restart for changes to apply)

(Optional) HAGS Fix Toggle will be available for those having HAGS related performance drops and issues. (also requires restart for changes to apply)

(Optional) SHADER CACHE Dropdown will be available for those having FRAME TIME related performance drops and issues. (also requires restart for changes to apply)

(Optional) TDRLevel Dropdown will be available for those having TDR related errors when playing games. (also requires restart for changes to apply)

3. Hit the reboot button.


That's it!

Note: To restore MPO, just open the tool and Toggle off the fix then reboot once again.

## Donations

Feel free to donate! Any amount will help <3
[The link is right here!](https://www.paypal.com/donate/?hosted_button_id=ZURUG4V6F6LRN)

## Changelog
v6.5(hotfix)
- (Bugfix) A uneeded check was being done for non-existant ShaderCache value in registry preventing user to use the toggle

v6.4(hotfix)
- (Bugfix) Added an additional check for profile count even if ShaderCache entry does not exist in registry

v6.3(hotfix)
- Reworked GPU Profile counter (PS: Sorry for breaking ShaderCache dropdown)

v6.2(hotfix):
- TDRLevel Caption added
- Improved detection of Offline/Deactivated PCI Cards
- Permission Check for Driver-level registry path and key added

v6.1(hotfix):
- (Bugfix) Forgot to update TDRLevel info button to the new repos wiki page

v6.0(sixth release):
- TDRLevel Dropdown added
- Improved detection of APU/IGPU
- WMI Repository error detection added

v5.2(nothing note worthy):
- DONATE Button added

v5.1(hotfix):
- (Bugfix) Forgot to update SHADER CACHE info button to the new repos wiki page

v5.0(fifth release):
- SHADER CACHE Dropdown added

v4.0(fourth release):
- (Bugfix) GPU Brand detection has been tweaked and now detects properly
- HAGS Fix Toggle added

v3.2(hotfix):
- (Bugfix) GPU Label for non AMD was out of bounds
- Reworked GPU Get Name & Version

v3.1(hotfix):
- (Bugfix) TDR Fix Toggle was broken
- Added INTEL ARC GPU detection & Unknown brand

v3(third release):
- TDR Fix toggle added

v2(second release):
- ULPS Toggle added
- Run as admin only
- All-in-one executeable
- Proper GPU Brand detection

v1(Initial release):
- MPO Toggle
