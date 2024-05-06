
namespace takecare
{
    partial class FrmMain
    {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.panel_main = new System.Windows.Forms.Panel();
            this.lblTitle = new System.Windows.Forms.Label();
            this.textBoxKey = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.txtLog = new System.Windows.Forms.TextBox();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.panel_theme_flash = new System.Windows.Forms.Panel();
            this.label_theme_flash = new System.Windows.Forms.Label();
            this.timer_theme_lash = new System.Windows.Forms.Timer(this.components);
            this.panel_main.SuspendLayout();
            this.panel_theme_flash.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel_main
            // 
            this.panel_main.BackColor = System.Drawing.Color.DarkRed;
            this.panel_main.Controls.Add(this.lblTitle);
            this.panel_main.Controls.Add(this.textBoxKey);
            this.panel_main.Controls.Add(this.button1);
            this.panel_main.Controls.Add(this.txtLog);
            this.panel_main.Location = new System.Drawing.Point(56, 59);
            this.panel_main.Name = "panel_main";
            this.panel_main.Size = new System.Drawing.Size(1228, 750);
            this.panel_main.TabIndex = 1;
            // 
            // label1
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Exo 2", 24F);
            this.lblTitle.Location = new System.Drawing.Point(448, 0);
            this.lblTitle.Name = "label1";
            this.lblTitle.Size = new System.Drawing.Size(338, 58);
            this.lblTitle.TabIndex = 3;
            this.lblTitle.Text = "YOUR TITLE HERE";
            // 
            // textBox3
            // 
            this.textBoxKey.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxKey.BackColor = System.Drawing.Color.DarkRed;
            this.textBoxKey.ForeColor = System.Drawing.Color.White;
            this.textBoxKey.Location = new System.Drawing.Point(19, 700);
            this.textBoxKey.Name = "textBox3";
            this.textBoxKey.Size = new System.Drawing.Size(899, 31);
            this.textBoxKey.TabIndex = 2;
            this.textBoxKey.Text = "Enter Decryption Key here";
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button1.Location = new System.Drawing.Point(934, 694);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(274, 42);
            this.button1.TabIndex = 1;
            this.button1.Text = "Decrypt";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.BtnDecrypt_Click);
            // 
            // textBox1
            // 
            this.txtLog.BackColor = System.Drawing.Color.DarkRed;
            this.txtLog.ForeColor = System.Drawing.Color.White;
            this.txtLog.Location = new System.Drawing.Point(19, 102);
            this.txtLog.Multiline = true;
            this.txtLog.Name = "textBox1";
            this.txtLog.ReadOnly = true;
            this.txtLog.Size = new System.Drawing.Size(1189, 573);
            this.txtLog.TabIndex = 0;
            // 
            // timer1
            // 
            this.timer1.Interval = 50;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // panel_theme_flash
            // 
            this.panel_theme_flash.BackColor = System.Drawing.Color.White;
            this.panel_theme_flash.Controls.Add(this.label_theme_flash);
            this.panel_theme_flash.Enabled = false;
            this.panel_theme_flash.Location = new System.Drawing.Point(24, 13);
            this.panel_theme_flash.Name = "panel_theme_flash";
            this.panel_theme_flash.Size = new System.Drawing.Size(604, 443);
            this.panel_theme_flash.TabIndex = 2;
            this.panel_theme_flash.Visible = false;
            // 
            // label_theme_flash
            // 
            this.label_theme_flash.AutoSize = true;
            this.label_theme_flash.ForeColor = System.Drawing.Color.Black;
            this.label_theme_flash.Location = new System.Drawing.Point(193, 180);
            this.label_theme_flash.Name = "label_theme_flash";
            this.label_theme_flash.Size = new System.Drawing.Size(69, 29);
            this.label_theme_flash.TabIndex = 0;
            this.label_theme_flash.Text = "label2";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 29F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.DarkRed;
            this.ClientSize = new System.Drawing.Size(1332, 865);
            this.ControlBox = false;
            this.Controls.Add(this.panel_theme_flash);
            this.Controls.Add(this.panel_main);
            this.Font = new System.Drawing.Font("Exo 2", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForeColor = System.Drawing.Color.White;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FrmMain_FormClosing);
            this.Load += new System.EventHandler(this.FrmMain_Load);
            this.panel_main.ResumeLayout(false);
            this.panel_main.PerformLayout();
            this.panel_theme_flash.ResumeLayout(false);
            this.panel_theme_flash.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Panel panel_main;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.TextBox textBoxKey;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TextBox txtLog;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Panel panel_theme_flash;
        private System.Windows.Forms.Label label_theme_flash;
        private System.Windows.Forms.Timer timer_theme_lash;
    }
}

