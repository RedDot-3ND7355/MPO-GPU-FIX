using ReaLTaiizor.Controls;
using ReaLTaiizor.Forms;
using ReaLTaiizor.Manager;
using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace AMDGPUFIX
{
    public partial class Dxmod : MaterialForm
    {
        // Globals
        public readonly MaterialSkinManager materialSkinManager;
        public bool disabled = false;
        // End


        public Dxmod()
        {
            InitializeComponent();
            // Apply ReaLTaiizor theming
            materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.EnforceBackcolorOnAllComponents = true;
            materialSkinManager.AddFormToManage(this);
            materialSkinManager.Theme = MaterialSkinManager.Themes.DARK;
            // Detect and INI
            DXHandler.IniDXHandler();
            if (DXHandler.ProfileCount() == 0)
            {
                disabled = true;
                this.Close();
                return;
            }
            ApplyDetected();
            CheckAvailability();
            SetupCardHoverEffects();
        }

        private void SetupCardHoverEffects()
        {
            MaterialCard[] dxCards = { materialCard1, materialCard2, materialCard3, materialCard5 };
            foreach (var card in dxCards)
            {
                if (card.Enabled)
                    card.Cursor = Cursors.Hand;
                else
                {
                    card.Cursor = Cursors.Default;
                    foreach (Control child in card.Controls)
                        child.Cursor = Cursors.Default;
                }
            }
        }

        private void CheckAvailability()
        {
            // Button 0
            if (!DXHandler.IsAvailable(0))
            {
                materialCard5.Enabled = false;
                materialLabel10.HighEmphasis = false;
            }
            // Button 1
            if (!DXHandler.IsAvailable(1))
            {
                materialCard3.Enabled = false;
                materialLabel11.HighEmphasis = false;
            }
            // Button 2
            if (!DXHandler.IsAvailable(2))
            {
                materialCard2.Enabled = false;
                materialLabel12.HighEmphasis = false;
            }
            // Button 3
            if (!DXHandler.IsAvailable(3))
            {
                materialCard1.Enabled = false;
                materialLabel8.HighEmphasis = false;
            }
        }

        private void ApplyDetected()
        {
            if (DXHandler.CurrentDX == -1)
                return;
            if (DXHandler.CurrentDX == 0)
            {
                // Material card 5
                materialLabel14.Visible = true;
                materialLabel13.Visible = false;
                materialLabel2.Visible = false;
                materialLabel1.Visible = false;
            }
            if (DXHandler.CurrentDX == 1)
            {
                // Material Card 3
                materialLabel14.Visible = false;
                materialLabel13.Visible = true;
                materialLabel2.Visible = false;
                materialLabel1.Visible = false;
            }
            if (DXHandler.CurrentDX == 2)
            {
                // Material Card 2
                materialLabel14.Visible = false;
                materialLabel13.Visible = false;
                materialLabel2.Visible = true;
                materialLabel1.Visible = false;
            }
            if (DXHandler.CurrentDX == 3)
            {
                // Material Card 1
                materialLabel14.Visible = false;
                materialLabel13.Visible = false;
                materialLabel2.Visible = false;
                materialLabel1.Visible = true;
            }
        }

        // Full DX Navi Switch
        private void materialButton4_Click(object sender, EventArgs e)
        {
            if (!materialCard1.Enabled || materialLabel1.Visible) return;
            DXHandler.SetFullNavi();
            ApplyDetected();
        }

        // DX 9 Navi with regular DX 11
        private void materialButton3_Click(object sender, EventArgs e)
        {
            if (!materialCard2.Enabled || materialLabel2.Visible) return;
            DXHandler.SetDX9NaviWRDX11();
            ApplyDetected();
        }

        // Regular DX 9 with navi DX 11
        private void materialButton2_Click(object sender, EventArgs e)
        {
            if (!materialCard3.Enabled || materialLabel13.Visible) return;
            DXHandler.SetRDX9WDX11Navi();
            ApplyDetected();
        }

        // Regular DX 9 
        private void materialButton1_Click(object sender, EventArgs e)
        {
            if (!materialCard5.Enabled || materialLabel14.Visible) return;
            DXHandler.RegularDX9();
            ApplyDetected();
        }

        // Help Button
        private void materialButton5_Click(object sender, EventArgs e) =>
            Process.Start(new ProcessStartInfo("https://github.com/RedDot-3ND7355/MPO-GPU-FIX/wiki/DX---Navi-Switches-(AMD)") { UseShellExecute = true });
    }
}
