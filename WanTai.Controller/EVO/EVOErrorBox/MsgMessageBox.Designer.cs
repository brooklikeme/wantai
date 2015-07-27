namespace WanTai.Controller.EVO.EVOErrorBox
{
    partial class MsgMessageBox
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
            this.btnOne = new System.Windows.Forms.Button();
            this.btnTwo = new System.Windows.Forms.Button();
            this.btnThree = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // errorPrompt
            // 
            this.errorPrompt.Location = new System.Drawing.Point(15, 19);
            this.errorPrompt.Name = "errorPrompt";
            this.errorPrompt.Size = new System.Drawing.Size(302, 109);
            this.errorPrompt.TabIndex = 2;
            this.errorPrompt.Text = "error";
            // 
            // btnOne
            // 
            this.btnOne.Location = new System.Drawing.Point(65, 149);
            this.btnOne.Name = "btnOne";
            this.btnOne.Size = new System.Drawing.Size(64, 20);
            this.btnOne.TabIndex = 7;
            this.btnOne.Text = "btnOne";
            this.btnOne.Click += new System.EventHandler(this.btnOne_Click);
            // 
            // btnTwo
            // 
            this.btnTwo.Location = new System.Drawing.Point(135, 149);
            this.btnTwo.Name = "btnTwo";
            this.btnTwo.Size = new System.Drawing.Size(64, 20);
            this.btnTwo.TabIndex = 8;
            this.btnTwo.Text = "btnTwo";
            this.btnTwo.Click += new System.EventHandler(this.btnTwo_Click);
            // 
            // btnThree
            // 
            this.btnThree.Location = new System.Drawing.Point(205, 149);
            this.btnThree.Name = "btnThree";
            this.btnThree.Size = new System.Drawing.Size(64, 20);
            this.btnThree.TabIndex = 9;
            this.btnThree.Text = "btnThree";
            this.btnThree.Click += new System.EventHandler(this.btnThree_Click);
            // 
            // MsgMessageBox
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(329, 215);
            this.ControlBox = false;
            this.Controls.Add(this.btnThree);
            this.Controls.Add(this.btnTwo);
            this.Controls.Add(this.btnOne);
            this.Controls.Add(this.errorPrompt);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "MsgMessageBox";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "错误";
            this.TopMost = true;
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label errorPrompt;
        private System.Windows.Forms.Button btnOne;
        private System.Windows.Forms.Button btnTwo;
        private System.Windows.Forms.Button btnThree;

    }
}