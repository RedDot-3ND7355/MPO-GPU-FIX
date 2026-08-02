namespace AMDGPUFIX
{
    partial class Bsodfix
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Bsodfix));
            materialComboBox2 = new ReaLTaiizor.Controls.MaterialComboBox();
            materialButton5 = new ReaLTaiizor.Controls.MaterialButton();
            flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            materialCard1 = new ReaLTaiizor.Controls.MaterialCard();
            materialLabel1 = new ReaLTaiizor.Controls.MaterialLabel();
            materialLabel9 = new ReaLTaiizor.Controls.MaterialLabel();
            materialLabel5 = new ReaLTaiizor.Controls.MaterialLabel();
            materialButton6 = new ReaLTaiizor.Controls.MaterialButton();
            materialSwitch1 = new ReaLTaiizor.Controls.MaterialSwitch();
            flowLayoutPanel1.SuspendLayout();
            materialCard1.SuspendLayout();
            SuspendLayout();
            // 
            // materialComboBox2
            // 
            materialComboBox2.AutoResize = false;
            materialComboBox2.BackColor = System.Drawing.Color.FromArgb(255, 255, 255);
            materialComboBox2.Depth = 0;
            materialComboBox2.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
            materialComboBox2.DropDownHeight = 174;
            materialComboBox2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            materialComboBox2.DropDownWidth = 121;
            materialComboBox2.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel);
            materialComboBox2.ForeColor = System.Drawing.Color.FromArgb(222, 0, 0, 0);
            materialComboBox2.FormattingEnabled = true;
            materialComboBox2.Hint = "PCI Device";
            materialComboBox2.IntegralHeight = false;
            materialComboBox2.ItemHeight = 43;
            materialComboBox2.Location = new System.Drawing.Point(5, 7);
            materialComboBox2.Margin = new System.Windows.Forms.Padding(5, 7, 5, 2);
            materialComboBox2.MaxDropDownItems = 4;
            materialComboBox2.MouseState = ReaLTaiizor.Helper.MaterialDrawHelper.MaterialMouseState.OUT;
            materialComboBox2.Name = "materialComboBox2";
            materialComboBox2.Size = new System.Drawing.Size(409, 49);
            materialComboBox2.StartIndex = 0;
            materialComboBox2.TabIndex = 5;
            materialComboBox2.UseAccent = false;
            materialComboBox2.SelectedIndexChanged += materialComboBox1_SelectedIndexChanged;
            // 
            // materialButton5
            // 
            materialButton5.AutoSize = false;
            materialButton5.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            materialButton5.Density = ReaLTaiizor.Controls.MaterialButton.MaterialButtonDensity.Default;
            materialButton5.Depth = 0;
            materialButton5.Font = new System.Drawing.Font("Segoe UI Emoji", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            materialButton5.HighEmphasis = true;
            materialButton5.Icon = null;
            materialButton5.IconType = ReaLTaiizor.Controls.MaterialButton.MaterialIconType.Rebase;
            materialButton5.Location = new System.Drawing.Point(18, 97);
            materialButton5.Margin = new System.Windows.Forms.Padding(4, 6, 4, 0);
            materialButton5.MouseState = ReaLTaiizor.Helper.MaterialDrawHelper.MaterialMouseState.HOVER;
            materialButton5.Name = "materialButton5";
            materialButton5.NoAccentTextColor = System.Drawing.Color.Empty;
            materialButton5.Size = new System.Drawing.Size(162, 30);
            materialButton5.TabIndex = 6;
            materialButton5.Text = "PCI DEVICES INFO ℹ️";
            materialButton5.Type = ReaLTaiizor.Controls.MaterialButton.MaterialButtonType.Contained;
            materialButton5.UseAccentColor = false;
            materialButton5.UseVisualStyleBackColor = true;
            materialButton5.Click += materialButton4_Click;
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.AutoSize = true;
            flowLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            flowLayoutPanel1.Controls.Add(materialComboBox2);
            flowLayoutPanel1.Controls.Add(materialCard1);
            flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Top;
            flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            flowLayoutPanel1.Location = new System.Drawing.Point(3, 24);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new System.Drawing.Size(419, 208);
            flowLayoutPanel1.TabIndex = 7;
            flowLayoutPanel1.WrapContents = false;
            // 
            // materialCard1
            // 
            materialCard1.AutoSize = true;
            materialCard1.BackColor = System.Drawing.Color.FromArgb(255, 255, 255);
            materialCard1.Controls.Add(materialLabel1);
            materialCard1.Controls.Add(materialLabel9);
            materialCard1.Controls.Add(materialLabel5);
            materialCard1.Controls.Add(materialButton6);
            materialCard1.Controls.Add(materialSwitch1);
            materialCard1.Controls.Add(materialButton5);
            materialCard1.Depth = 0;
            materialCard1.Enabled = false;
            materialCard1.ForeColor = System.Drawing.Color.FromArgb(222, 0, 0, 0);
            materialCard1.Location = new System.Drawing.Point(5, 60);
            materialCard1.Margin = new System.Windows.Forms.Padding(5, 2, 5, 7);
            materialCard1.MouseState = ReaLTaiizor.Helper.MaterialDrawHelper.MaterialMouseState.HOVER;
            materialCard1.Name = "materialCard1";
            materialCard1.Padding = new System.Windows.Forms.Padding(14, 14, 14, 11);
            materialCard1.Size = new System.Drawing.Size(409, 141);
            materialCard1.TabIndex = 6;
            // 
            // materialLabel1
            // 
            materialLabel1.AutoSize = true;
            materialLabel1.Depth = 0;
            materialLabel1.Font = new System.Drawing.Font("Roboto", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            materialLabel1.FontType = ReaLTaiizor.Manager.MaterialSkinManager.FontType.Body2;
            materialLabel1.Location = new System.Drawing.Point(18, 44);
            materialLabel1.MouseState = ReaLTaiizor.Helper.MaterialDrawHelper.MaterialMouseState.HOVER;
            materialLabel1.Name = "materialLabel1";
            materialLabel1.Size = new System.Drawing.Size(374, 17);
            materialLabel1.TabIndex = 22;
            materialLabel1.Text = "Fixes system latency and hardware level BSOD loop crashes.";
            // 
            // materialLabel9
            // 
            materialLabel9.AutoSize = true;
            materialLabel9.Depth = 0;
            materialLabel9.Font = new System.Drawing.Font("Roboto", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            materialLabel9.FontType = ReaLTaiizor.Manager.MaterialSkinManager.FontType.Body2;
            materialLabel9.Location = new System.Drawing.Point(16, 69);
            materialLabel9.MouseState = ReaLTaiizor.Helper.MaterialDrawHelper.MaterialMouseState.HOVER;
            materialLabel9.Name = "materialLabel9";
            materialLabel9.Size = new System.Drawing.Size(135, 17);
            materialLabel9.TabIndex = 21;
            materialLabel9.Text = "Selected Device: None";
            // 
            // materialLabel5
            // 
            materialLabel5.AutoSize = true;
            materialLabel5.Depth = 0;
            materialLabel5.Font = new System.Drawing.Font("Roboto Medium", 20F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel);
            materialLabel5.FontType = ReaLTaiizor.Manager.MaterialSkinManager.FontType.H6;
            materialLabel5.Location = new System.Drawing.Point(16, 16);
            materialLabel5.Margin = new System.Windows.Forms.Padding(2, 0, 0, 2);
            materialLabel5.MouseState = ReaLTaiizor.Helper.MaterialDrawHelper.MaterialMouseState.HOVER;
            materialLabel5.Name = "materialLabel5";
            materialLabel5.Size = new System.Drawing.Size(379, 24);
            materialLabel5.TabIndex = 10;
            materialLabel5.Text = "Toggle Message Signaled Interrupts (MSI)";
            // 
            // materialButton6
            // 
            materialButton6.AutoSize = false;
            materialButton6.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            materialButton6.Density = ReaLTaiizor.Controls.MaterialButton.MaterialButtonDensity.Default;
            materialButton6.Depth = 0;
            materialButton6.HighEmphasis = true;
            materialButton6.Icon = null;
            materialButton6.IconType = ReaLTaiizor.Controls.MaterialButton.MaterialIconType.Rebase;
            materialButton6.Location = new System.Drawing.Point(188, 97);
            materialButton6.Margin = new System.Windows.Forms.Padding(4, 6, 4, 0);
            materialButton6.MouseState = ReaLTaiizor.Helper.MaterialDrawHelper.MaterialMouseState.HOVER;
            materialButton6.Name = "materialButton6";
            materialButton6.NoAccentTextColor = System.Drawing.Color.Empty;
            materialButton6.Size = new System.Drawing.Size(30, 30);
            materialButton6.TabIndex = 9;
            materialButton6.Text = "?";
            materialButton6.Type = ReaLTaiizor.Controls.MaterialButton.MaterialButtonType.Contained;
            materialButton6.UseAccentColor = false;
            materialButton6.UseVisualStyleBackColor = true;
            materialButton6.Click += materialButton6_Click;
            // 
            // materialSwitch1
            // 
            materialSwitch1.AutoSize = true;
            materialSwitch1.Depth = 0;
            materialSwitch1.Location = new System.Drawing.Point(235, 93);
            materialSwitch1.Margin = new System.Windows.Forms.Padding(0);
            materialSwitch1.MouseLocation = new System.Drawing.Point(-1, -1);
            materialSwitch1.MouseState = ReaLTaiizor.Helper.MaterialDrawHelper.MaterialMouseState.HOVER;
            materialSwitch1.Name = "materialSwitch1";
            materialSwitch1.Ripple = true;
            materialSwitch1.Size = new System.Drawing.Size(157, 37);
            materialSwitch1.TabIndex = 8;
            materialSwitch1.Text = "MSISUPPORT";
            materialSwitch1.UseAccentColor = false;
            materialSwitch1.UseVisualStyleBackColor = true;
            materialSwitch1.CheckedChanged += materialSwitch1_CheckedChanged;
            // 
            // Bsodfix
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            AutoSize = true;
            ClientSize = new System.Drawing.Size(425, 237);
            Controls.Add(flowLayoutPanel1);
            FormStyle = ReaLTaiizor.Enum.Material.FormStyles.ActionBar_None;
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "Bsodfix";
            Padding = new System.Windows.Forms.Padding(3, 24, 3, 3);
            Sizable = false;
            SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "BSOD FIX (HDAUDBUS.SYS/PCI.SYS)";
            flowLayoutPanel1.ResumeLayout(false);
            flowLayoutPanel1.PerformLayout();
            materialCard1.ResumeLayout(false);
            materialCard1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion
        private ReaLTaiizor.Controls.MaterialComboBox materialComboBox2;
        private ReaLTaiizor.Controls.MaterialButton materialButton5;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private ReaLTaiizor.Controls.MaterialCard materialCard1;
        private ReaLTaiizor.Controls.MaterialSwitch materialSwitch1;
        private ReaLTaiizor.Controls.MaterialButton materialButton6;
        private ReaLTaiizor.Controls.MaterialLabel materialLabel5;
        private ReaLTaiizor.Controls.MaterialLabel materialLabel1;
        private ReaLTaiizor.Controls.MaterialLabel materialLabel9;
    }
}