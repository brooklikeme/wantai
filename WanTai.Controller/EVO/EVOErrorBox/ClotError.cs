using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using EVOApiErrorMsg;

namespace WanTai.Controller.EVO.EVOErrorBox
{
    public partial class ClotError : Form
    {
        private XmlMsgClotError _clotError;
        private ErrorResponseOption result;

        public ErrorResponseOption ErrorResponse
        {
            get { return result; }
        }

        public ClotError(XmlMsgClotError clotError)
        {            
            InitializeComponent();
            _clotError = clotError;

            string tipStatus = _clotError.tipStatus;
            DataTable datatable = new DataTable();
            datatable.Columns.Add("tip", typeof(int));
            datatable.Columns.Add("status", typeof(string));
            datatable.Columns.Add("statusChn", typeof(string));
            int maxTip = 0;
            string[] tipStatus_s = tipStatus.Split(';');
            if (tipStatus_s.Count() > 0)
            {
                foreach (string _tipStatus in tipStatus_s)
                {
                    string[] oneTipStatus_s = _tipStatus.Split(',');
                    if (oneTipStatus_s.Count() != 2)
                        continue;

                    System.Data.DataRow dRow = datatable.NewRow();
                    dRow["tip"] = int.Parse(oneTipStatus_s[0]);
                    maxTip = int.Parse(oneTipStatus_s[0]);
                    dRow["status"] = oneTipStatus_s[1];
                    string status = oneTipStatus_s[1];
                    string statusChn = string.Empty;
                    if(!string.IsNullOrEmpty(status) && status.Contains("OK / automatic handling"))
                    {
                        statusChn = WanTai.Common.Resources.WanTaiResource.OKautomatichandling;
                    }
                    else if (!string.IsNullOrEmpty(status) && status.Contains("clot detected"))
                    {
                        statusChn = WanTai.Common.Resources.WanTaiResource.clotdetected;
                    }

                    dRow["statusChn"] = statusChn;
                    datatable.Rows.Add(dRow);
                }
            }

            if (maxTip < 8)
            {
                for (int i = maxTip + 1; i < 9; i++)
                {
                    System.Data.DataRow dRow = datatable.NewRow();
                    dRow["tip"] = i;
                    dRow["statusChn"] = string.Empty;
                    dRow["status"] = string.Empty;
                    datatable.Rows.Add(dRow);
                }
            }

            this.dataGridView1.DataSource = datatable;

            btnDispenseRetry.Text = WanTai.Common.Resources.WanTaiResource.btnDispenseRetry;
            btnDispensePipetNone.Text = WanTai.Common.Resources.WanTaiResource.btnDispensePipetNone;
            btnGoWithClot.Text = WanTai.Common.Resources.WanTaiResource.btnGoWithClot;
            btnContinueIgnore.Text = WanTai.Common.Resources.WanTaiResource.btnContinueIgnore;
        }

            

        private void dataGridView1_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            int index = 0;
            foreach (DataRow row in ((DataTable)dataGridView1.DataSource).Rows)
            {
                string status = row["status"].ToString();
                if (status.Contains("clot detected"))
                {
                    //dataGridView1.Rows[index].DefaultCellStyle.ForeColor = Color.Black;
                }
                else
                {
                    dataGridView1.Rows[index].DefaultCellStyle.ForeColor = Color.Gray;
                }

                index++;
            }
        }

        private void btnDispenseRetry_Click(object sender, EventArgs e)
        {
            result = ErrorResponseOption.DLG_DISP_BACK_RETRY;
            this.Close();
        }

        private void btnDispensePipetNone_Click(object sender, EventArgs e)
        {
            result = ErrorResponseOption.DLG_DISP_BACK_PIPETNONE;
            this.Close();
        }

        private void btnGoWithClot_Click(object sender, EventArgs e)
        {
            result = ErrorResponseOption.DLG_CONT_WITHCLOT;
            this.Close();
        }

        private void btnContinueIgnore_Click(object sender, EventArgs e)
        {
            result = ErrorResponseOption.DLG_CONT_IGNORECLOT;
            this.Close();
        }  
    }
}
