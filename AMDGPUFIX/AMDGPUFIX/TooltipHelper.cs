using System;
using System.Drawing;
using System.Windows.Forms;

namespace AMDGPUFIX
{
    public class TooltipHelper
    {
        // All
        ToolTip Tooltips;
        // Donate
        ToolTip TooltipDonate;
        // Reboot
        ToolTip TooltipReboot;
        // Download
        ToolTip TooltipDownload;

        private void Initialize()
        {
            // Download
            TooltipDownload = new ToolTip();
            // Reboot
            TooltipReboot = new ToolTip();
            // Donate
            TooltipDonate = new ToolTip();
            // All
            Tooltips = new ToolTip();
        }

        private void ConfigureTooltips()
        {
            System.Action<ToolTip, string, ToolTipIcon> setupTip = (tt, title, icon) =>
            {
                tt.IsBalloon = false;
                tt.OwnerDraw = true;
                tt.ToolTipIcon = icon;
                tt.InitialDelay = 150;
                tt.ReshowDelay = 100;
                tt.BackColor = Color.FromArgb(80, 80, 80);
                tt.ForeColor = Color.FromArgb(232, 232, 232);
                tt.ToolTipTitle = title;
                tt.UseAnimation = true; 
                tt.UseFading = true;

                // Attach the rendering logic
                tt.Popup += ToolTip_Popup;
                tt.Draw += ToolTip_Draw;
            };
            // All
            setupTip(Tooltips, "Wiki Section", ToolTipIcon.Info);
            // Download
            setupTip(TooltipDownload, "Download Drivers", ToolTipIcon.Info);
            // Reboot
            setupTip(TooltipReboot, "Reboot Confirmation", ToolTipIcon.Warning);
            // Donate
            setupTip(TooltipDonate, "Donate", ToolTipIcon.Info);
        }

        private void ClearAllActivePopups(object sender, System.EventArgs e)
        {
            // Forcing an internal programmatic text assignment loop clear command 
            // commands the native win32 environment to dismiss active tip windows immediately.
            TooltipReboot.Hide((Control)sender);
            TooltipDonate.Hide((Control)sender);
            TooltipDownload.Hide((Control)sender);
            Tooltips.Hide((Control)sender);
        }

        private void ToolTip_Popup(object sender, PopupEventArgs e)
        {
            ToolTip tt = (ToolTip)sender;
            string mainText = tt.GetToolTip(e.AssociatedControl);

            // Balanced visual padding
            int leftOffset = (tt.ToolTipIcon != ToolTipIcon.None) ? 32 : 12;
            int rightPadding = 0;
            int topPadding = 10;
            int elementSpacing = 4;
            int bottomPadding = 2;

            int calculatedHeight = topPadding;
            int calculatedWidth = 60;

            Font baseFont = e.AssociatedControl?.Font ?? SystemFonts.DefaultFont;

            using (Graphics g = e.AssociatedControl.CreateGraphics())
            {
                // Use uniform rendering properties to keep metrics completely identical
                using (StringFormat sf = new StringFormat(StringFormat.GenericTypographic))
                {
                    sf.FormatFlags = StringFormatFlags.LineLimit;

                    // 1. Measure Title Dimensions
                    Size titleSize = Size.Empty;
                    if (!string.IsNullOrEmpty(tt.ToolTipTitle))
                    {
                        using (Font titleFont = new Font(baseFont, FontStyle.Bold))
                        {
                            SizeF tSize = g.MeasureString(tt.ToolTipTitle, titleFont, new SizeF(260, 0), sf);
                            titleSize = new Size((int)Math.Ceiling(tSize.Width), (int)Math.Ceiling(tSize.Height));
                            calculatedHeight += titleSize.Height + elementSpacing;
                        }
                    }

                    // 2. Measure Body Dimensions
                    int maxTextWidth = 280 - leftOffset - rightPadding;
                    SizeF bSize = g.MeasureString(mainText, baseFont, new SizeF(maxTextWidth, 0), sf);
                    Size bodyTextSize = new Size((int)Math.Ceiling(bSize.Width), (int)Math.Ceiling(bSize.Height));

                    calculatedHeight += bodyTextSize.Height + bottomPadding;

                    // 3. Compact Horizontal Fitting
                    int longestTextWidth = Math.Max(titleSize.Width, bodyTextSize.Width);
                    calculatedWidth = leftOffset + longestTextWidth + rightPadding;
                }
            }

            if (tt.ToolTipIcon != ToolTipIcon.None && calculatedHeight < 34)
            {
                calculatedHeight = 34;
            }

            e.ToolTipSize = new Size(calculatedWidth, calculatedHeight);
        }

        private void ToolTip_Draw(object sender, DrawToolTipEventArgs e)
        {
            ToolTip tt = (ToolTip)sender;

            // Fill background
            using (SolidBrush bgBrush = new SolidBrush(tt.BackColor))
            {
                e.Graphics.FillRectangle(bgBrush, e.Bounds);
            }

            // Border line
            using (Pen borderPen = new Pen(Color.FromArgb(100, 100, 100), 1))
            {
                Rectangle borderBounds = e.Bounds;
                borderBounds.Width -= 1;
                borderBounds.Height -= 1;
                e.Graphics.DrawRectangle(borderPen, borderBounds);
            }

            // Draw standard icon layout safely
            int textLeftOffset = 12;
            Icon iconToDraw = null;

            if (tt.ToolTipIcon == ToolTipIcon.Info) iconToDraw = SystemIcons.Information;
            else if (tt.ToolTipIcon == ToolTipIcon.Warning) iconToDraw = SystemIcons.Warning;
            else if (tt.ToolTipIcon == ToolTipIcon.Error) iconToDraw = SystemIcons.Error;

            if (iconToDraw != null)
            {
                e.Graphics.DrawIcon(iconToDraw, new Rectangle(8, 8, 16, 16));
                textLeftOffset = 32;
            }

            float currentY = 8;

            // Explicitly share GenericTypographic format settings to align matching widths exactly
            using (StringFormat sf = new StringFormat(StringFormat.GenericTypographic))
            {
                sf.FormatFlags = StringFormatFlags.LineLimit;
                sf.Trimming = StringTrimming.EllipsisWord;

                using (SolidBrush textBrush = new SolidBrush(tt.ForeColor))
                {
                    // Render Title layout
                    if (!string.IsNullOrEmpty(tt.ToolTipTitle))
                    {
                        using (Font titleFont = new Font(e.Font, FontStyle.Bold))
                        {
                            e.Graphics.DrawString(tt.ToolTipTitle, titleFont, textBrush, textLeftOffset, currentY, sf);
                            currentY += titleFont.Height + 4;
                        }
                    }

                    // Render Body text layout tightly inside the tracking dimensions
                    RectangleF textLayoutBounds = new RectangleF(
                        textLeftOffset,
                        currentY,
                        e.Bounds.Width - textLeftOffset - 12,
                        e.Bounds.Height - currentY - 4
                    );

                    e.Graphics.DrawString(e.ToolTipText, e.Font, textBrush, textLayoutBounds, sf);
                }
            }
        }

        private void SetToolTips(NewForm form)
        {
            Control[] trackedControls = new Control[] {
                form.materialFloatingActionButton2, form.materialButton6, form.driverDlButton,
                form.ulpsHelpButton, form.materialButton2, form.materialButton10,
                form.materialButton5, form.materialButton4, form.materialButton7,
                form.materialButton3, form.materialButton11
            };

            foreach (Control ctrl in trackedControls)
            {
                if (ctrl != null)
                {
                    ctrl.MouseEnter += ClearAllActivePopups;
                }
            }

            // Reboot
            TooltipReboot.SetToolTip(form.materialFloatingActionButton2, "Reboot your PC to apply changes");
            // Donate
            TooltipDonate.SetToolTip(form.materialButton6, "Donate to support development");
            // Download
            TooltipDownload.SetToolTip(form.driverDlButton, "Download the latest GPU drivers");
            // All
            Tooltips.SetToolTip(form.ulpsHelpButton, "Learn more about ULPS");
            Tooltips.SetToolTip(form.materialButton2, "Learn more about MPO");
            Tooltips.SetToolTip(form.materialButton10, "Learn more about OverlayMinFPS");
            Tooltips.SetToolTip(form.shaderCacheHelpButton, "Learn more about Shader Cache");
            Tooltips.SetToolTip(form.materialButton4, "Learn more about HAGS");
            Tooltips.SetToolTip(form.materialButton7, "Learn more about Disable Overlays");
            Tooltips.SetToolTip(form.materialButton3, "Learn more about TDR");
            Tooltips.SetToolTip(form.materialButton11, "Learn more about TDRLevel");
            Tooltips.SetToolTip(form.materialButton5, "Learn more about Force Direct Flip");
        }

        public void InitializeTooltips(NewForm form)
        {
            Initialize();
            ConfigureTooltips();
            SetToolTips(form);
        }
    }

}
