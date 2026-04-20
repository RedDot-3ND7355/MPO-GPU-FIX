using MaterialSkin;
using MaterialSkin.Controls;
using System;

namespace AMDGPUFIX
{
    public partial class dxmod : MaterialForm
    {
        // Globals
        public readonly MaterialSkinManager materialSkinManager;
        // End

        public dxmod()
        {
            InitializeComponent();
            // Set material colors
            materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.EnforceBackcolorOnAllComponents = true;
            materialSkinManager.AddFormToManage(this);
            materialSkinManager.Theme = MaterialSkinManager.Themes.DARK;
            // Detect and INI
            DXHandler.IniDXHandler();
            ApplyDetected();
            CheckAvailability();
        }

        private void CheckAvailability()
        {
            // Button 0
            if (!DXHandler.IsAvailable(0))
                materialButton1.Enabled = false;
            // Button 1
            if (!DXHandler.IsAvailable(1))
                materialButton2.Enabled = false;
            // Button 2
            if (!DXHandler.IsAvailable(2))
                materialButton3.Enabled = false;
            // Button 3
            if (!DXHandler.IsAvailable(3))
                materialButton4.Enabled = false;
        }

        private void ApplyDetected()
        {
            if (DXHandler.CurrentDX == -1)
                return;
            if (DXHandler.CurrentDX == 0)
            {
                materialButton1.UseAccentColor = true;
                materialButton2.UseAccentColor = false;
                materialButton3.UseAccentColor = false;
                materialButton4.UseAccentColor = false;
            }
            if (DXHandler.CurrentDX == 1)
            {
                materialButton1.UseAccentColor = false;
                materialButton2.UseAccentColor = true;
                materialButton3.UseAccentColor = false;
                materialButton4.UseAccentColor = false;
            }
            if (DXHandler.CurrentDX == 2)
            {
                materialButton1.UseAccentColor = false;
                materialButton2.UseAccentColor = false;
                materialButton3.UseAccentColor = true;
                materialButton4.UseAccentColor = false;
            }
            if (DXHandler.CurrentDX == 3)
            {
                materialButton1.UseAccentColor = false;
                materialButton2.UseAccentColor = false;
                materialButton3.UseAccentColor = false;
                materialButton4.UseAccentColor = true;
            }
        }

        // Full DX Navi Switch
        private void materialButton4_Click(object sender, EventArgs e)
        {
            DXHandler.SetFullNavi();
            ApplyDetected();
        }

        // DX 9 Navi with regular DX 11
        private void materialButton3_Click(object sender, EventArgs e)
        {
            DXHandler.SetDX9NaviWRDX11();
            ApplyDetected();
        }

        // Regular DX 9 with navi DX 11
        private void materialButton2_Click(object sender, EventArgs e)
        {
            DXHandler.SetRDX9WDX11Navi();
            ApplyDetected();
        }

        // Regular DX 9 
        private void materialButton1_Click(object sender, EventArgs e)
        {
            DXHandler.RegularDX9();
            ApplyDetected();
        }

        // Help Button
        private void materialButton5_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/RedDot-3ND7355/MPO-GPU-FIX/wiki/DX---Navi-Switches-(AMD)");
        }
    }
}
