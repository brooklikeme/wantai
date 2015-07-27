namespace WanTai.Controller.EVO.EVOErrorBox
{
    partial class DiluterCheckError
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
            this.errorPrompt = new System.Windows.Forms.Label();
            this.ignore = new System.Windows.Forms.Button();
            this.cancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // errorPrompt
            // 
            this.errorPrompt.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.errorPrompt.Location = new System.Drawing.Point(20, 38);
            this.errorPrompt.Name = "errorPrompt";
            this.errorPrompt.Size = new System.Drawing.Size(277, 55);
            this.errorPrompt.TabIndex = 1;
            this.errorPrompt.Text = "label1";
            // 
            // ignore
            // 
            this.ignore.Location = new System.Drawing.Point(92, 117);
            this.ignore.Name = "ignore";
            this.ignore.Size = new System.Drawing.Size(75, 23);
            this.ignore.TabIndex = 2;
            this.ignore.Text = "Ignore";
            this.ignore.UseVisualStyleBackColor = true;
            this.ignore.Click += new System.EventHandler(this.ignore_Click);
            // 
            // cancel
            // 
            this.cancel.Location = new System.Drawing.Point(222, 117);
            this.cancel.Name = "cancel";
            this.cancel.Size = new System.Drawing.Size(75, 23);
            this.cancel.TabIndex = 3;
            this.cancel.Text = "Cancel";
            this.cancel.UseVisualStyleBackColor = true;
            this.cancel.Click += new System.EventHandler(this.cancel_Click);
            // 
            // DiluterCheckError
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(329, 173);
            this.ControlBox = false;
            this.Controls.Add(this.cancel);
            this.Controls.Add(this.ignore);
            this.Controls.Add(this.errorPrompt);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "DiluterCheckError";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Volume Monitoring";
            this.TopMost = true;
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label errorPrompt;
        private System.Windows.Forms.Button ignore;
        private System.Windows.Forms.Button cancel;
    }
}