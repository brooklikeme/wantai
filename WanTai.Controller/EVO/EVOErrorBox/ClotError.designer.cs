namespace WanTai.Controller.EVO.EVOErrorBox
{
    partial class ClotError
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
            this.btnDispenseRetry = new System.Windows.Forms.Button();
            this.btnDispensePipetNone = new System.Windows.Forms.Button();
            this.btnGoWithClot = new System.Windows.Forms.Button();
            this.btnContinueIgnore = new System.Windows.Forms.Button();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.Tip = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Status = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Status1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.SuspendLayout();
            // 
            // btnDispenseRetry
            // 
            this.btnDispenseRetry.Location = new System.Drawing.Point(11, 216);
            this.btnDispenseRetry.Name = "btnDispenseRetry";
            this.btnDispenseRetry.Size = new System.Drawing.Size(303, 20);
            this.btnDispenseRetry.TabIndex = 7;
            this.btnDispenseRetry.Text = "Dispense back into vessel, then retry";
            this.btnDispenseRetry.Click += new System.EventHandler(this.btnDispenseRetry_Click);
            // 
            // btnDispensePipetNone
            // 
            this.btnDispensePipetNone.Location = new System.Drawing.Point(11, 241);
            this.btnDispensePipetNone.Name = "btnDispensePipetNone";
            this.btnDispensePipetNone.Size = new System.Drawing.Size(303, 20);
            this.btnDispensePipetNone.TabIndex = 8;
            this.btnDispensePipetNone.Text = "Dispense back into vessel, then pipette nothing";
            this.btnDispensePipetNone.Click += new System.EventHandler(this.btnDispensePipetNone_Click);
            // 
            // btnGoWithClot
            // 
            this.btnGoWithClot.Location = new System.Drawing.Point(11, 267);
            this.btnGoWithClot.Name = "btnGoWithClot";
            this.btnGoWithClot.Size = new System.Drawing.Size(303, 20);
            this.btnGoWithClot.TabIndex = 9;
            this.btnGoWithClot.Text = "Go on with clot";
            this.btnGoWithClot.Click += new System.EventHandler(this.btnGoWithClot_Click);
            // 
            // btnContinueIgnore
            // 
            this.btnContinueIgnore.Location = new System.Drawing.Point(11, 293);
            this.btnContinueIgnore.Name = "btnContinueIgnore";
            this.btnContinueIgnore.Size = new System.Drawing.Size(303, 23);
            this.btnContinueIgnore.TabIndex = 10;
            this.btnContinueIgnore.Text = "Continue, ignore clot error";
            this.btnContinueIgnore.UseVisualStyleBackColor = true;
            this.btnContinueIgnore.Click += new System.EventHandler(this.btnContinueIgnore_Click);
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
            this.dataGridView1.GridColor = System.Drawing.SystemColors.Control;
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
            // ClotError
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(329, 385);
            this.ControlBox = false;
            this.Controls.Add(this.dataGridView1);
            this.Controls.Add(this.btnContinueIgnore);
            this.Controls.Add(this.btnGoWithClot);
            this.Controls.Add(this.btnDispensePipetNone);
            this.Controls.Add(this.btnDispenseRetry);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "ClotError";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Clot Error";
            this.TopMost = true;
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnDispenseRetry;
        private System.Windows.Forms.Button btnDispensePipetNone;
        private System.Windows.Forms.Button btnGoWithClot;
        private System.Windows.Forms.Button btnContinueIgnore;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.DataGridViewTextBoxColumn Tip;
        private System.Windows.Forms.DataGridViewTextBoxColumn Status;
        private System.Windows.Forms.DataGridViewTextBoxColumn Status1;

    }
}