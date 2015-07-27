namespace WanTai.UserPrompt
{
    partial class ChangeWash
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
            this.textPrompt = new System.Windows.Forms.Label();
            this.btnOK = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // textPrompt
            // 
            this.textPrompt.Location = new System.Drawing.Point(15, 19);
            this.textPrompt.Name = "textPrompt";
            this.textPrompt.Size = new System.Drawing.Size(302, 109);
            this.textPrompt.TabIndex = 2;
            this.textPrompt.Text = "请放入洗液：";
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(121, 148);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(64, 20);
            this.btnOK.TabIndex = 7;
            this.btnOK.Text = "确认";
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // ChangeWash
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(329, 215);
            this.ControlBox = false;
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.textPrompt);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "ChangeWash";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "提示";
            this.TopMost = true;
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label textPrompt;
        private System.Windows.Forms.Button btnOK;

    }
}