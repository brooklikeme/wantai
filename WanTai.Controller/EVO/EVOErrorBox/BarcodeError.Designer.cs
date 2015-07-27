namespace WanTai.Controller.EVO.EVOErrorBox
{
    partial class BarcodeError
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
            this.ErrPrompt = new System.Windows.Forms.Label();
            this.BtnRetry = new System.Windows.Forms.Button();
            this.BtnEnter = new System.Windows.Forms.Button();
            this.BtnHelp = new System.Windows.Forms.Button();
            this.BtnOverwrite = new System.Windows.Forms.Button();
            this.BtnCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // ErrPrompt
            // 
            this.ErrPrompt.Location = new System.Drawing.Point(12, 40);
            this.ErrPrompt.Name = "ErrPrompt";
            this.ErrPrompt.Size = new System.Drawing.Size(100, 15);
            this.ErrPrompt.TabIndex = 2;
            this.ErrPrompt.Text = "error";
            // 
            // BtnRetry
            // 
            this.BtnRetry.Location = new System.Drawing.Point(12, 148);
            this.BtnRetry.Name = "BtnRetry";
            this.BtnRetry.Size = new System.Drawing.Size(64, 20);
            this.BtnRetry.TabIndex = 7;
            this.BtnRetry.Text = "重试";
            this.BtnRetry.Click += new System.EventHandler(this.BtnRetry_Click);
            // 
            // BtnEnter
            // 
            this.BtnEnter.Location = new System.Drawing.Point(109, 148);
            this.BtnEnter.Name = "BtnEnter";
            this.BtnEnter.Size = new System.Drawing.Size(64, 20);
            this.BtnEnter.TabIndex = 8;
            this.BtnEnter.Text = "确认";
            this.BtnEnter.Click += new System.EventHandler(this.BtnEnter_Click);
            // 
            // BtnHelp
            // 
            this.BtnHelp.Location = new System.Drawing.Point(204, 148);
            this.BtnHelp.Name = "BtnHelp";
            this.BtnHelp.Size = new System.Drawing.Size(64, 20);
            this.BtnHelp.TabIndex = 9;
            this.BtnHelp.Text = "帮助";
            this.BtnHelp.Click += new System.EventHandler(this.BtnHelp_Click);
            // 
            // BtnOverwrite
            // 
            this.BtnOverwrite.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.BtnOverwrite.Location = new System.Drawing.Point(48, 187);
            this.BtnOverwrite.Name = "BtnOverwrite";
            this.BtnOverwrite.Size = new System.Drawing.Size(64, 20);
            this.BtnOverwrite.TabIndex = 10;
            this.BtnOverwrite.Text = "覆盖";
            this.BtnOverwrite.Click += new System.EventHandler(this.BtnOverwrite_Click);
            // 
            // BtnCancel
            // 
            this.BtnCancel.Location = new System.Drawing.Point(141, 187);
            this.BtnCancel.Name = "BtnCancel";
            this.BtnCancel.Size = new System.Drawing.Size(64, 20);
            this.BtnCancel.TabIndex = 11;
            this.BtnCancel.Text = "取消";
            this.BtnCancel.Click += new System.EventHandler(this.BtnCancel_Click);
            // 
            // BarcodeError
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(292, 266);
            this.Controls.Add(this.BtnCancel);
            this.Controls.Add(this.BtnOverwrite);
            this.Controls.Add(this.BtnHelp);
            this.Controls.Add(this.BtnEnter);
            this.Controls.Add(this.BtnRetry);
            this.Controls.Add(this.ErrPrompt);
            this.Name = "BarcodeError";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "错误";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label ErrPrompt;
        private System.Windows.Forms.Button BtnRetry;
        private System.Windows.Forms.Button BtnEnter;
        private System.Windows.Forms.Button BtnHelp;
        private System.Windows.Forms.Button BtnOverwrite;
        private System.Windows.Forms.Button BtnCancel;

    }
}