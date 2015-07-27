namespace WanTai.Controller.EVO.EVOErrorBox
{
    partial class DiTiNotMountedError
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
            this.btnRetry = new System.Windows.Forms.Button();
            this.btnContinuePipette = new System.Windows.Forms.Button();
            this.btnAbort = new System.Windows.Forms.Button();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.Tip = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Status = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Status1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.btnContinueNothing = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.SuspendLayout();
            // 
            // btnRetry
            // 
            this.btnRetry.Location = new System.Drawing.Point(11, 274);
            this.btnRetry.Name = "btnRetry";
            this.btnRetry.Size = new System.Drawing.Size(303, 20);
            this.btnRetry.TabIndex = 7;
            this.btnRetry.Text = "Retry at same position";
            this.btnRetry.Click += new System.EventHandler(this.btnRetry_Click);
            // 
            // btnContinuePipette
            // 
            this.btnContinuePipette.Location = new System.Drawing.Point(11, 299);
            this.btnContinuePipette.Name = "btnContinuePipette";
            this.btnContinuePipette.Size = new System.Drawing.Size(303, 20);
            this.btnContinuePipette.TabIndex = 8;
            this.btnContinuePipette.Text = "Contonue and pipette anyway";
            this.btnContinuePipette.Click += new System.EventHandler(this.btnContinuePipette_Click);
            // 
            // btnAbort
            // 
            this.btnAbort.Location = new System.Drawing.Point(11, 350);
            this.btnAbort.Name = "btnAbort";
            this.btnAbort.Size = new System.Drawing.Size(303, 20);
            this.btnAbort.TabIndex = 9;
            this.btnAbort.Text = "Abort";
            this.btnAbort.Click += new System.EventHandler(this.btnAbort_Click);
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AllowUserToDeleteRows = false;
            this.dataGridView1.AllowUserToResizeColumns = false;
            this.dataGridView1.AllowUserToResizeRows = false;
            this.dataGridView1.BackgroundColor = System.Drawing.SystemColors.Control;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Tip,
            this.Status,
            this.Status1});
            this.dataGridView1.Location = new System.Drawing.Point(12, 12);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.RowHeadersVisible = false;
            this.dataGridView1.Size = new System.Drawing.Size(303, 196);
            this.dataGridView1.TabIndex = 16;
            this.dataGridView1.DataBindingComplete += new System.Windows.Forms.DataGridViewBindingCompleteEventHandler(this.dataGridView1_DataBindingComplete);
            // 
            // Tip
            // 
            this.Tip.DataPropertyName = "tip";
            this.Tip.HeaderText = "通道";
            this.Tip.Name = "Tip";
            this.Tip.ReadOnly = true;
            this.Tip.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // Status
            // 
            this.Status.DataPropertyName = "statusChn";
            this.Status.HeaderText = "状态";
            this.Status.Name = "Status";
            this.Status.ReadOnly = true;
            this.Status.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.Status.Width = 200;
            // 
            // Status1
            // 
            this.Status1.DataPropertyName = "status";
            this.Status1.HeaderText = "Status";
            this.Status1.Name = "Status1";
            this.Status1.Visible = false;
            // 
            // btnContinueNothing
            // 
            this.btnContinueNothing.Location = new System.Drawing.Point(12, 325);
            this.btnContinueNothing.Name = "btnContinueNothing";
            this.btnContinueNothing.Size = new System.Drawing.Size(303, 20);
            this.btnContinueNothing.TabIndex = 17;
            this.btnContinueNothing.Text = "Continue, but pipette nothing with DiTis concerned";
            this.btnContinueNothing.Click += new System.EventHandler(this.btnContinueNothing_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(11, 211);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(139, 13);
            this.label1.TabIndex = 18;
            this.label1.Text = "有吸头未能很好的装配。";
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(11, 236);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(306, 35);
            this.label2.TabIndex = 19;
            this.label2.Text = "请检查吸头所对应的Labware设置，确保该Labware的位置设置正确。";
            this.label2.UseCompatibleTextRendering = true;
            // 
            // DiTiNotMountedError
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(329, 385);
            this.ControlBox = false;
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnContinueNothing);
            this.Controls.Add(this.dataGridView1);
            this.Controls.Add(this.btnAbort);
            this.Controls.Add(this.btnContinuePipette);
            this.Controls.Add(this.btnRetry);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "DiTiNotMountedError";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Error Mounting DiTis";
            this.TopMost = true;
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnRetry;
        private System.Windows.Forms.Button btnContinuePipette;
        private System.Windows.Forms.Button btnAbort;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.Button btnContinueNothing;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.DataGridViewTextBoxColumn Tip;
        private System.Windows.Forms.DataGridViewTextBoxColumn Status;
        private System.Windows.Forms.DataGridViewTextBoxColumn Status1;

    }
}