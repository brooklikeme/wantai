namespace WanTai.Controller.EVO.EVOErrorBox
{
    partial class GripError
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
            this.BtnRetry = new System.Windows.Forms.Button();
            this.BtnAbort = new System.Windows.Forms.Button();
            this.ErrorPrompt = new System.Windows.Forms.Label();
            this.BtnIgnore = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // BtnRetry
            // 
            this.BtnRetry.Location = new System.Drawing.Point(24, 84);
            this.BtnRetry.Name = "BtnRetry";
            this.BtnRetry.Size = new System.Drawing.Size(71, 20);
            this.BtnRetry.TabIndex = 7;
            this.BtnRetry.Text = "Retry";
            this.BtnRetry.Click += new System.EventHandler(this.BtnRetry_Click);
            // 
            // BtnAbort
            // 
            this.BtnAbort.Location = new System.Drawing.Point(213, 84);
            this.BtnAbort.Name = "BtnAbort";
            this.BtnAbort.Size = new System.Drawing.Size(64, 20);
            this.BtnAbort.TabIndex = 8;
            this.BtnAbort.Text = "About";
            this.BtnAbort.Click += new System.EventHandler(this.BtnAbort_Click);
            // 
            // ErrorPrompt
            // 
            this.ErrorPrompt.Location = new System.Drawing.Point(15, 21);
            this.ErrorPrompt.Name = "ErrorPrompt";
            this.ErrorPrompt.Size = new System.Drawing.Size(302, 29);
            this.ErrorPrompt.TabIndex = 10;
            this.ErrorPrompt.Text = "error";
            // 
            // BtnIgnore
            // 
            this.BtnIgnore.Location = new System.Drawing.Point(123, 84);
            this.BtnIgnore.Name = "BtnIgnore";
            this.BtnIgnore.Size = new System.Drawing.Size(64, 20);
            this.BtnIgnore.TabIndex = 11;
            this.BtnIgnore.Text = "Ignore";
            this.BtnIgnore.Click += new System.EventHandler(this.BtnIgnore_Click);
            // 
            // GripError
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(329, 137);
            this.ControlBox = false;
            this.Controls.Add(this.BtnIgnore);
            this.Controls.Add(this.ErrorPrompt);
            this.Controls.Add(this.BtnAbort);
            this.Controls.Add(this.BtnRetry);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "GripError";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Grip Error";
            this.TopMost = true;
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button BtnRetry;
        private System.Windows.Forms.Button BtnAbort;
        private System.Windows.Forms.Label ErrorPrompt;
        private System.Windows.Forms.Button BtnIgnore;

    }
}