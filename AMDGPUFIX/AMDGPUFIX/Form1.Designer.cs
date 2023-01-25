namespace AMDGPUFIX
{
    partial class Form1
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.materialCard1 = new MaterialSkin.Controls.MaterialCard();
            this.materialLabel2 = new MaterialSkin.Controls.MaterialLabel();
            this.materialLabel1 = new MaterialSkin.Controls.MaterialLabel();
            this.materialSwitch1 = new MaterialSkin.Controls.MaterialSwitch();
            this.materialSwitch2 = new MaterialSkin.Controls.MaterialSwitch();
            this.materialButton1 = new MaterialSkin.Controls.MaterialButton();
            this.materialButton2 = new MaterialSkin.Controls.MaterialButton();
            this.materialLabel3 = new MaterialSkin.Controls.MaterialLabel();
            this.materialFloatingActionButton2 = new MaterialSkin.Controls.MaterialFloatingActionButton();
            this.materialFloatingActionButton1 = new MaterialSkin.Controls.MaterialFloatingActionButton();
            this.materialSwitch3 = new MaterialSkin.Controls.MaterialSwitch();
            this.materialButton3 = new MaterialSkin.Controls.MaterialButton();
            this.materialCard1.SuspendLayout();
            this.SuspendLayout();
            // 
            // materialCard1
            // 
            this.materialCard1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.materialCard1.Controls.Add(this.materialLabel2);
            this.materialCard1.Controls.Add(this.materialLabel1);
            this.materialCard1.Depth = 0;
            this.materialCard1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(222)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.materialCard1.Location = new System.Drawing.Point(17, 78);
            this.materialCard1.Margin = new System.Windows.Forms.Padding(14);
            this.materialCard1.MouseState = MaterialSkin.MouseState.HOVER;
            this.materialCard1.Name = "materialCard1";
            this.materialCard1.Padding = new System.Windows.Forms.Padding(14);
            this.materialCard1.Size = new System.Drawing.Size(307, 100);
            this.materialCard1.TabIndex = 0;
            // 
            // materialLabel2
            // 
            this.materialLabel2.AutoSize = true;
            this.materialLabel2.Depth = 0;
            this.materialLabel2.Font = new System.Drawing.Font("Roboto", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.materialLabel2.Location = new System.Drawing.Point(17, 57);
            this.materialLabel2.MouseState = MaterialSkin.MouseState.HOVER;
            this.materialLabel2.Name = "materialLabel2";
            this.materialLabel2.Size = new System.Drawing.Size(85, 19);
            this.materialLabel2.TabIndex = 4;
            this.materialLabel2.Text = "GPU Driver: ";
            this.materialLabel2.UseAccent = true;
            // 
            // materialLabel1
            // 
            this.materialLabel1.AutoSize = true;
            this.materialLabel1.Depth = 0;
            this.materialLabel1.Font = new System.Drawing.Font("Roboto", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.materialLabel1.Location = new System.Drawing.Point(17, 20);
            this.materialLabel1.MouseState = MaterialSkin.MouseState.HOVER;
            this.materialLabel1.Name = "materialLabel1";
            this.materialLabel1.Size = new System.Drawing.Size(40, 19);
            this.materialLabel1.TabIndex = 3;
            this.materialLabel1.Text = "GPU: ";
            this.materialLabel1.UseAccent = true;
            // 
            // materialSwitch1
            // 
            this.materialSwitch1.AutoSize = true;
            this.materialSwitch1.Depth = 0;
            this.materialSwitch1.Location = new System.Drawing.Point(165, 192);
            this.materialSwitch1.Margin = new System.Windows.Forms.Padding(0);
            this.materialSwitch1.MouseLocation = new System.Drawing.Point(-1, -1);
            this.materialSwitch1.MouseState = MaterialSkin.MouseState.HOVER;
            this.materialSwitch1.Name = "materialSwitch1";
            this.materialSwitch1.Ripple = true;
            this.materialSwitch1.Size = new System.Drawing.Size(118, 37);
            this.materialSwitch1.TabIndex = 2;
            this.materialSwitch1.Text = "MPO Fix";
            this.materialSwitch1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.materialSwitch1.UseVisualStyleBackColor = true;
            this.materialSwitch1.CheckedChanged += new System.EventHandler(this.materialSwitch1_CheckedChanged);
            // 
            // materialSwitch2
            // 
            this.materialSwitch2.AutoSize = true;
            this.materialSwitch2.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.materialSwitch2.Checked = true;
            this.materialSwitch2.CheckState = System.Windows.Forms.CheckState.Checked;
            this.materialSwitch2.Depth = 0;
            this.materialSwitch2.Location = new System.Drawing.Point(17, 192);
            this.materialSwitch2.Margin = new System.Windows.Forms.Padding(0);
            this.materialSwitch2.MouseLocation = new System.Drawing.Point(-1, -1);
            this.materialSwitch2.MouseState = MaterialSkin.MouseState.HOVER;
            this.materialSwitch2.Name = "materialSwitch2";
            this.materialSwitch2.Ripple = true;
            this.materialSwitch2.Size = new System.Drawing.Size(136, 37);
            this.materialSwitch2.TabIndex = 4;
            this.materialSwitch2.Text = "AMD ULPS";
            this.materialSwitch2.UseVisualStyleBackColor = true;
            this.materialSwitch2.CheckedChanged += new System.EventHandler(this.materialSwitch2_CheckedChanged);
            // 
            // materialButton1
            // 
            this.materialButton1.AutoSize = false;
            this.materialButton1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.materialButton1.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.materialButton1.Depth = 0;
            this.materialButton1.HighEmphasis = true;
            this.materialButton1.Icon = null;
            this.materialButton1.Location = new System.Drawing.Point(186, 231);
            this.materialButton1.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.materialButton1.MouseState = MaterialSkin.MouseState.HOVER;
            this.materialButton1.Name = "materialButton1";
            this.materialButton1.NoAccentTextColor = System.Drawing.Color.Empty;
            this.materialButton1.Size = new System.Drawing.Size(54, 36);
            this.materialButton1.TabIndex = 5;
            this.materialButton1.Text = "ULPS?";
            this.materialButton1.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.materialButton1.UseAccentColor = false;
            this.materialButton1.UseVisualStyleBackColor = true;
            this.materialButton1.Click += new System.EventHandler(this.materialButton1_Click);
            // 
            // materialButton2
            // 
            this.materialButton2.AutoSize = false;
            this.materialButton2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.materialButton2.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.materialButton2.Depth = 0;
            this.materialButton2.HighEmphasis = true;
            this.materialButton2.Icon = null;
            this.materialButton2.Location = new System.Drawing.Point(241, 231);
            this.materialButton2.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.materialButton2.MouseState = MaterialSkin.MouseState.HOVER;
            this.materialButton2.Name = "materialButton2";
            this.materialButton2.NoAccentTextColor = System.Drawing.Color.Empty;
            this.materialButton2.Size = new System.Drawing.Size(49, 36);
            this.materialButton2.TabIndex = 6;
            this.materialButton2.Text = "MPO?";
            this.materialButton2.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.materialButton2.UseAccentColor = false;
            this.materialButton2.UseVisualStyleBackColor = true;
            this.materialButton2.Click += new System.EventHandler(this.materialButton2_Click);
            // 
            // materialLabel3
            // 
            this.materialLabel3.Depth = 0;
            this.materialLabel3.Font = new System.Drawing.Font("Roboto", 24F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel);
            this.materialLabel3.FontType = MaterialSkin.MaterialSkinManager.fontType.H5;
            this.materialLabel3.Location = new System.Drawing.Point(19, 192);
            this.materialLabel3.MouseState = MaterialSkin.MouseState.HOVER;
            this.materialLabel3.Name = "materialLabel3";
            this.materialLabel3.Size = new System.Drawing.Size(143, 37);
            this.materialLabel3.TabIndex = 7;
            this.materialLabel3.Text = "NVIDIA GPU";
            this.materialLabel3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.materialLabel3.UseAccent = true;
            // 
            // materialFloatingActionButton2
            // 
            this.materialFloatingActionButton2.AutoSize = true;
            this.materialFloatingActionButton2.Depth = 0;
            this.materialFloatingActionButton2.DrawShadows = false;
            this.materialFloatingActionButton2.Icon = global::AMDGPUFIX.Properties.Resources.outline_restart_alt_black_48dp;
            this.materialFloatingActionButton2.Location = new System.Drawing.Point(295, 203);
            this.materialFloatingActionButton2.MouseState = MaterialSkin.MouseState.HOVER;
            this.materialFloatingActionButton2.Name = "materialFloatingActionButton2";
            this.materialFloatingActionButton2.Size = new System.Drawing.Size(56, 60);
            this.materialFloatingActionButton2.TabIndex = 3;
            this.materialFloatingActionButton2.UseVisualStyleBackColor = true;
            this.materialFloatingActionButton2.Visible = false;
            this.materialFloatingActionButton2.Click += new System.EventHandler(this.materialFloatingActionButton2_Click);
            // 
            // materialFloatingActionButton1
            // 
            this.materialFloatingActionButton1.Depth = 0;
            this.materialFloatingActionButton1.Icon = global::AMDGPUFIX.Properties.Resources.outline_file_download_black_48dp;
            this.materialFloatingActionButton1.Location = new System.Drawing.Point(295, 98);
            this.materialFloatingActionButton1.MouseState = MaterialSkin.MouseState.HOVER;
            this.materialFloatingActionButton1.Name = "materialFloatingActionButton1";
            this.materialFloatingActionButton1.Size = new System.Drawing.Size(56, 63);
            this.materialFloatingActionButton1.TabIndex = 1;
            this.materialFloatingActionButton1.UseVisualStyleBackColor = true;
            this.materialFloatingActionButton1.Click += new System.EventHandler(this.materialFloatingActionButton1_Click);
            // 
            // materialSwitch3
            // 
            this.materialSwitch3.AutoSize = true;
            this.materialSwitch3.Depth = 0;
            this.materialSwitch3.Location = new System.Drawing.Point(17, 230);
            this.materialSwitch3.Margin = new System.Windows.Forms.Padding(0);
            this.materialSwitch3.MouseLocation = new System.Drawing.Point(-1, -1);
            this.materialSwitch3.MouseState = MaterialSkin.MouseState.HOVER;
            this.materialSwitch3.Name = "materialSwitch3";
            this.materialSwitch3.Ripple = true;
            this.materialSwitch3.Size = new System.Drawing.Size(114, 37);
            this.materialSwitch3.TabIndex = 8;
            this.materialSwitch3.Text = "TDR Fix";
            this.materialSwitch3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.materialSwitch3.UseVisualStyleBackColor = true;
            this.materialSwitch3.CheckedChanged += new System.EventHandler(this.materialSwitch3_CheckedChanged);
            // 
            // materialButton3
            // 
            this.materialButton3.AutoSize = false;
            this.materialButton3.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.materialButton3.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.materialButton3.Depth = 0;
            this.materialButton3.HighEmphasis = true;
            this.materialButton3.Icon = null;
            this.materialButton3.Location = new System.Drawing.Point(142, 231);
            this.materialButton3.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.materialButton3.MouseState = MaterialSkin.MouseState.HOVER;
            this.materialButton3.Name = "materialButton3";
            this.materialButton3.NoAccentTextColor = System.Drawing.Color.Empty;
            this.materialButton3.Size = new System.Drawing.Size(43, 36);
            this.materialButton3.TabIndex = 9;
            this.materialButton3.Text = "TDR?";
            this.materialButton3.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.materialButton3.UseAccentColor = false;
            this.materialButton3.UseVisualStyleBackColor = true;
            this.materialButton3.Click += new System.EventHandler(this.materialButton3_Click);
            // 
            // Form1
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(366, 283);
            this.Controls.Add(this.materialLabel3);
            this.Controls.Add(this.materialButton3);
            this.Controls.Add(this.materialSwitch3);
            this.Controls.Add(this.materialButton2);
            this.Controls.Add(this.materialButton1);
            this.Controls.Add(this.materialSwitch2);
            this.Controls.Add(this.materialFloatingActionButton2);
            this.Controls.Add(this.materialSwitch1);
            this.Controls.Add(this.materialFloatingActionButton1);
            this.Controls.Add(this.materialCard1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.Sizable = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "MPO Fix for GPUs";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form1_FormClosed);
            this.materialCard1.ResumeLayout(false);
            this.materialCard1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private MaterialSkin.Controls.MaterialCard materialCard1;
        private MaterialSkin.Controls.MaterialFloatingActionButton materialFloatingActionButton1;
        private MaterialSkin.Controls.MaterialSwitch materialSwitch1;
        private MaterialSkin.Controls.MaterialLabel materialLabel2;
        private MaterialSkin.Controls.MaterialLabel materialLabel1;
        private MaterialSkin.Controls.MaterialFloatingActionButton materialFloatingActionButton2;
        private MaterialSkin.Controls.MaterialSwitch materialSwitch2;
        private MaterialSkin.Controls.MaterialButton materialButton1;
        private MaterialSkin.Controls.MaterialButton materialButton2;
        private MaterialSkin.Controls.MaterialLabel materialLabel3;
        private MaterialSkin.Controls.MaterialSwitch materialSwitch3;
        private MaterialSkin.Controls.MaterialButton materialButton3;
    }
}

