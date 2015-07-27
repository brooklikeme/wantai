namespace WanTai.Controller.EVO.EVOErrorBox
{
    partial class MsgDetectError
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
            this.btnRetryDetect = new System.Windows.Forms.Button();
            this.btnMoveZMax = new System.Windows.Forms.Button();
            this.btnPipetNothing = new System.Windows.Forms.Button();
            this.btnAspAir = new System.Windows.Forms.Button();
            this.btnAbort = new System.Windows.Forms.Button();
            this.btnMoveZTravel = new System.Windows.Forms.Button();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.Tip = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Status = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Volume = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Status1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.SuspendLayout();
            // 
            // btnRetryDetect
            // 
            this.btnRetryDetect.Location = new System.Drawing.Point(11, 216);
            this.btnRetryDetect.Name = "btnRetryDetect";
            this.btnRetryDetect.Size = new System.Drawing.Size(303, 20);
            this.btnRetryDetect.TabIndex = 7;
            this.btnRetryDetect.Text = "Retry detection";
            this.btnRetryDetect.Click += new System.EventHandler(this.btnRetryDetect_Click);
            // 
            // btnMoveZMax
            // 
            this.btnMoveZMax.Location = new System.Drawing.Point(11, 241);
            this.btnMoveZMax.Name = "btnMoveZMax";
            this.btnMoveZMax.Size = new System.Drawing.Size(303, 20);
            this.btnMoveZMax.TabIndex = 8;
            this.btnMoveZMax.Text = "Move tips to Z (Max)";
            this.btnMoveZMax.Click += new System.EventHandler(this.btnMoveZMax_Click);
            // 
            // btnPipetNothing
            // 
            this.btnPipetNothing.Location = new System.Drawing.Point(11, 267);
            this.btnPipetNothing.Name = "btnPipetNothing";
            this.btnPipetNothing.Size = new System.Drawing.Size(303, 20);
            this.btnPipetNothing.TabIndex = 9;
            this.btnPipetNothing.Text = "Pipette nothing";
            this.btnPipetNothing.Click += new System.EventHandler(this.btnPipetNothing_Click);
            // 
            // btnAspAir
            // 
            this.btnAspAir.Location = new System.Drawing.Point(11, 293);
            this.btnAspAir.Name = "btnAspAir";
            this.btnAspAir.Size = new System.Drawing.Size(303, 23);
            this.btnAspAir.TabIndex = 10;
            this.btnAspAir.Text = "Aspirate air instead of the liquid";
            this.btnAspAir.UseVisualStyleBackColor = true;
            this.btnAspAir.Click += new System.EventHandler(this.btnAspAir_Click);
            // 
            // btnAbort
            // 
            this.btnAbort.Location = new System.Drawing.Point(91, 322);
            this.btnAbort.Name = "btnAbort";
            this.btnAbort.Size = new System.Drawing.Size(138, 20);
            this.btnAbort.TabIndex = 11;
            this.btnAbort.Text = "Abort";
            this.btnAbort.Click += new System.EventHandler(this.btnAbort_Click);
            // 
            // btnMoveZTravel
            // 
            this.btnMoveZTravel.Location = new System.Drawing.Point(11, 349);
            this.btnMoveZTravel.Name = "btnMoveZTravel";
            this.btnMoveZTravel.Size = new System.Drawing.Size(303, 20);
            this.btnMoveZTravel.TabIndex = 12;
            this.btnMoveZTravel.Text = "Retract all tips to global Z (Travel)";
            this.btnMoveZTravel.Click += new System.EventHandler(this.btnMoveZTravel_Click);
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
            this.Volume,
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
            this.Tip.Width = 60;
            // 
            // Status
            // 
            this.Status.DataPropertyName = "statusChn";
            this.Status.HeaderText = "状态";
            this.Status.Name = "Status";
            this.Status.ReadOnly = true;
            this.Status.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.Status.Width = 160;
            // 
            // Volume
            // 
            this.Volume.DataPropertyName = "volume";
            this.Volume.HeaderText = "体积[ul]";
            this.Volume.Name = "Volume";
            this.Volume.ReadOnly = true;
            this.Volume.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.Volume.Width = 80;
            // 
            // Status1
            // 
            this.Status1.DataPropertyName = "status";
            this.Status1.HeaderText = "Status";
            this.Status1.Name = "Status1";
            this.Status1.Visible = false;
            // 
            // MsgDetectError
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(329, 385);
            this.ControlBox = false;
            this.Controls.Add(this.dataGridView1);
            this.Controls.Add(this.btnMoveZTravel);
            this.Controls.Add(this.btnAbort);
            this.Controls.Add(this.btnAspAir);
            this.Controls.Add(this.btnPipetNothing);
            this.Controls.Add(this.btnMoveZMax);
            this.Controls.Add(this.btnRetryDetect);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "MsgDetectError";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Detection Error";
            this.TopMost = true;
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnRetryDetect;
        private System.Windows.Forms.Button btnMoveZMax;
        private System.Windows.Forms.Button btnPipetNothing;
        private System.Windows.Forms.Button btnAspAir;
        private System.Windows.Forms.Button btnAbort;
        private System.Windows.Forms.Button btnMoveZTravel;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.DataGridViewTextBoxColumn Tip;
        private System.Windows.Forms.DataGridViewTextBoxColumn Status;
        private System.Windows.Forms.DataGridViewTextBoxColumn Volume;
        private System.Windows.Forms.DataGridViewTextBoxColumn Status1;

    }
}