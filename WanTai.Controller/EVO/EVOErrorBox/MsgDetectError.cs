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
    public partial class MsgDetectError : Form
    {
        private XmlMsgDetectError _detectError;
        private ErrorResponseOption result;

        public ErrorResponseOption ErrorResponse
        {
            get { return result; }
        }

        public MsgDetectError(XmlMsgDetectError detectError)
        {            
            InitializeComponent();
            _detectError = detectError;

            string liquidStatus = _detectError.liquidStatus;
            DataTable datatable = new DataTable();
            datatable.Columns.Add("tip", typeof(int));
            datatable.Columns.Add("status", typeof(string));
            datatable.Columns.Add("statusChn", typeof(string));
            datatable.Columns.Add("volume", typeof(int));

            string[] liquidStatus_s = liquidStatus.Split(';');
            if (liquidStatus_s.Count() > 0)
            {
                foreach (string _liquidStatus in liquidStatus_s)
                {
                    string[] oneliquidStatus_s = _liquidStatus.Split(',');
                    if (oneliquidStatus_s.Count() != 3)
                        continue;

                    System.Data.DataRow dRow = datatable.NewRow();
                    dRow["tip"] = int.Parse(oneliquidStatus_s[0]);
                    dRow["status"] = oneliquidStatus_s[1];
                    string status = oneliquidStatus_s[1];
                    string statusChn = string.Empty;
                    if(!string.IsNullOrEmpty(status) && status.Contains("OK / automatic handling"))
                    {
                        statusChn = WanTai.Common.Resources.WanTaiResource.OKautomatichandling;
                    }
                    else if (!string.IsNullOrEmpty(status) && status.Contains("not enough liquid"))
                    {
                        statusChn = WanTai.Common.Resources.WanTaiResource.NotEnoughLiquid;
                    }
                    else if (!string.IsNullOrEmpty(status) && status.Contains("no liquid"))
                    {
                        statusChn = WanTai.Common.Resources.WanTaiResource.NoLiquid;
                    }

                    dRow["statusChn"] = statusChn;
                    dRow["volume"] = oneliquidStatus_s[2];
                    datatable.Rows.Add(dRow);
                }
            }

            this.dataGridView1.DataSource = datatable;

            btnRetryDetect.Text = WanTai.Common.Resources.WanTaiResource.btnRetryDetect;
            btnMoveZMax.Text = WanTai.Common.Resources.WanTaiResource.btnMoveZMax;
            btnPipetNothing.Text = WanTai.Common.Resources.WanTaiResource.btnPipetNothing;
            btnAspAir.Text = WanTai.Common.Resources.WanTaiResource.btnAspAir;
            btnAbort.Text = WanTai.Common.Resources.WanTaiResource.btnAbort;
            btnMoveZTravel.Text = WanTai.Common.Resources.WanTaiResource.btnMoveZTravel;
        }

        private void btnRetryDetect_Click(object sender, EventArgs e)
        {
            result = ErrorResponseOption.DLG_RETRYDETECT;
            this.Close();
        }

        private void btnMoveZMax_Click(object sender, EventArgs e)
        {
            result = ErrorResponseOption.DLG_MOVE_ZMAX;
            this.Close();
        }

        private void btnPipetNothing_Click(object sender, EventArgs e)
        {
            result = ErrorResponseOption.DLG_PIPET_NONE;
            this.Close();
        }

        private void btnAspAir_Click(object sender, EventArgs e)
        {
            result = ErrorResponseOption.DLG_ASP_AIR;
            this.Close();
        }

        private void btnAbort_Click(object sender, EventArgs e)
        {
            result = ErrorResponseOption.DLG_ABORT;
            this.Close();
        }

        private void btnMoveZTravel_Click(object sender, EventArgs e)
        {
            result = ErrorResponseOption.DLG_MOVE_ZTRAVEL;
            this.Close();
        }        

        private void dataGridView1_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            int index = 0;
            foreach (DataRow row in ((DataTable)dataGridView1.DataSource).Rows)
            {
                string status = row["status"].ToString();
                if (status.Contains("not enough liquid") || status.Contains("no liquid"))
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
    }
}
