namespace WanTai.Controller.EVO
{
    partial class CEDForm
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
            this.cedDescription = new System.Windows.Forms.Label();
            this.cedButton1 = new System.Windows.Forms.Button();
            this.cedButton2 = new System.Windows.Forms.Button();
            this.cedButton3 = new System.Windows.Forms.Button();
            this.cedButton4 = new System.Windows.Forms.Button();
            this.cedButton5 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // cedDescription
            // 
            this.cedDescription.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.cedDescription.Location = new System.Drawing.Point(14, 16);
            this.cedDescription.Name = "cedDescription";
            this.cedDescription.Size = new System.Drawing.Size(506, 55);
            this.cedDescription.TabIndex = 0;
            this.cedDescription.Text = "label1";
            // 
            // cedButton1
            // 
            this.cedButton1.Location = new System.Drawing.Point(18, 74);
            this.cedButton1.Name = "cedButton1";
            this.cedButton1.Size = new System.Drawing.Size(94, 23);
            this.cedButton1.TabIndex = 1;
            this.cedButton1.Text = "button1";
            this.cedButton1.UseVisualStyleBackColor = true;
            this.cedButton1.Click += new System.EventHandler(this.cedButton_Click);
            // 
            // cedButton2
            // 
            this.cedButton2.Location = new System.Drawing.Point(118, 74);
            this.cedButton2.Name = "cedButton2";
            this.cedButton2.Size = new System.Drawing.Size(94, 23);
            this.cedButton2.TabIndex = 2;
            this.cedButton2.Text = "button2";
            this.cedButton2.UseVisualStyleBackColor = true;
            this.cedButton2.Click += new System.EventHandler(this.cedButton_Click);
            // 
            // cedButton3
            // 
            this.cedButton3.Location = new System.Drawing.Point(218, 74);
            this.cedButton3.Name = "cedButton3";
            this.cedButton3.Size = new System.Drawing.Size(94, 23);
            this.cedButton3.TabIndex = 3;
            this.cedButton3.Text = "button3";
            this.cedButton3.UseVisualStyleBackColor = true;
            this.cedButton3.Click += new System.EventHandler(this.cedButton_Click);
            // 
            // cedButton4
            // 
            this.cedButton4.Location = new System.Drawing.Point(318, 74);
            this.cedButton4.Name = "cedButton4";
            this.cedButton4.Size = new System.Drawing.Size(94, 23);
            this.cedButton4.TabIndex = 4;
            this.cedButton4.Text = "button4";
            this.cedButton4.UseVisualStyleBackColor = true;
            this.cedButton4.Click += new System.EventHandler(this.cedButton_Click);
            // 
            // cedButton5
            // 
            this.cedButton5.Location = new System.Drawing.Point(418, 74);
            this.cedButton5.Name = "cedButton5";
            this.cedButton5.Size = new System.Drawing.Size(94, 23);
            this.cedButton5.TabIndex = 5;
            this.cedButton5.Text = "button5";
            this.cedButton5.UseVisualStyleBackColor = true;
            this.cedButton5.Click += new System.EventHandler(this.cedButton_Click);
            // 
            // CEDForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(532, 111);
            this.Controls.Add(this.cedButton5);
            this.Controls.Add(this.cedButton4);
            this.Controls.Add(this.cedButton3);
            this.Controls.Add(this.cedButton2);
            this.Controls.Add(this.cedButton1);
            this.Controls.Add(this.cedDescription);
            this.Name = "CEDForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Barcode Error";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.CEDForm_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label cedDescription;
        private System.Windows.Forms.Button cedButton1;
        private System.Windows.Forms.Button cedButton2;
        private System.Windows.Forms.Button cedButton3;
        private System.Windows.Forms.Button cedButton4;
        private System.Windows.Forms.Button cedButton5;
    }
}