namespace teardrop
{
    partial class FrmMain
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            txtBox = new RichTextBox();
            btnDecode = new Button();
            txtKey = new TextBox();
            SuspendLayout();
            // 
            // txtBox
            // 
            txtBox.BackColor = SystemColors.ScrollBar;
            txtBox.Location = new Point(12, 12);
            txtBox.Name = "txtBox";
            txtBox.Size = new Size(776, 388);
            txtBox.TabIndex = 0;
            txtBox.Text = "";
            // 
            // btnDecode
            // 
            btnDecode.BackColor = Color.LightCoral;
            btnDecode.Font = new Font("Segoe UI", 10F, FontStyle.Bold, GraphicsUnit.Point);
            btnDecode.Location = new Point(652, 406);
            btnDecode.Name = "btnDecode";
            btnDecode.Size = new Size(136, 32);
            btnDecode.TabIndex = 1;
            btnDecode.Text = "DECODE";
            btnDecode.UseVisualStyleBackColor = false;
            btnDecode.Click += BtnDecode_Click;
            // 
            // txtKey
            // 
            txtKey.Location = new Point(12, 411);
            txtKey.Name = "txtKey";
            txtKey.Size = new Size(634, 23);
            txtKey.TabIndex = 2;
            // 
            // FrmMain
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.Brown;
            ClientSize = new Size(800, 450);
            Controls.Add(txtKey);
            Controls.Add(btnDecode);
            Controls.Add(txtBox);
            FormBorderStyle = FormBorderStyle.None;
            MinimizeBox = false;
            MinimumSize = new Size(800, 450);
            Name = "FrmMain";
            ShowIcon = false;
            ShowInTaskbar = false;
            SizeGripStyle = SizeGripStyle.Hide;
            StartPosition = FormStartPosition.CenterScreen;
            TopMost = true;
            FormClosing += FrmMain_FormClosing;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private RichTextBox txtBox;
        private Button btnDecode;
        private TextBox txtKey;
    }
}