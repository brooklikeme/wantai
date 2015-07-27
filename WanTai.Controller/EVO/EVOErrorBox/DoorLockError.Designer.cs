namespace WanTai.Controller.EVO.EVOErrorBox
{
    partial class DoorLockError
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
            this.BtnSwitchLock = new System.Windows.Forms.Button();
            this.BtnCancel = new System.Windows.Forms.Button();
            this.UserPrompt = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // BtnSwitchLock
            // 
            this.BtnSwitchLock.Location = new System.Drawing.Point(24, 84);
            this.BtnSwitchLock.Name = "BtnSwitchLock";
            this.BtnSwitchLock.Size = new System.Drawing.Size(117, 20);
            this.BtnSwitchLock.TabIndex = 7;
            this.BtnSwitchLock.Text = "Switch Lock Again";
            this.BtnSwitchLock.Click += new System.EventHandler(this.BtnSwitchLock_Click);
            // 
            // BtnCancel
            // 
            this.BtnCancel.Location = new System.Drawing.Point(213, 84);
            this.BtnCancel.Name = "BtnCancel";
            this.BtnCancel.Size = new System.Drawing.Size(64, 20);
            this.BtnCancel.TabIndex = 8;
            this.BtnCancel.Text = "Cancel";
            this.BtnCancel.Click += new System.EventHandler(this.BtnCancel_Click);
            // 
            // UserPrompt
            // 
            this.UserPrompt.Location = new System.Drawing.Point(15, 21);
            this.UserPrompt.Name = "UserPrompt";
            this.UserPrompt.Size = new System.Drawing.Size(302, 29);
            this.UserPrompt.TabIndex = 10;
            this.UserPrompt.Text = "error";
            // 
            // DoorLockError
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(329, 137);
            this.ControlBox = false;
            this.Controls.Add(this.UserPrompt);
            this.Controls.Add(this.BtnCancel);
            this.Controls.Add(this.BtnSwitchLock);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "DoorLockError";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Doorlock Error";
            this.TopMost = true;
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button BtnSwitchLock;
        private System.Windows.Forms.Button BtnCancel;
        private System.Windows.Forms.Label UserPrompt;

    }
}