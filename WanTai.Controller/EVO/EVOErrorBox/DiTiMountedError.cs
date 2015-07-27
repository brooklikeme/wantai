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
    public partial class DiTiMountedError : Form
    {
        private XmlMsgDiTiMountedError _msg;
        private ErrorResponseOption result;

        public ErrorResponseOption ErrorResponse
        {
            get { return result; }
        }

        public DiTiMountedError(XmlMsgDiTiMountedError msg)
        {            
            InitializeComponent();
            _msg = msg;

            string ditiStatus = _msg.DiTiStatus;
            DataTable datatable = new DataTable();
            datatable.Columns.Add("tip", typeof(int));
            datatable.Columns.Add("status", typeof(string));
            datatable.Columns.Add("statusChn", typeof(string));

            string[] ditiStatus_s = ditiStatus.Split(';');
            int maxTip = 0;
            if (ditiStatus_s.Count() > 0)
            {
                foreach (string _ditiStatus in ditiStatus_s)
                {
                    string[] oneditiStatus_s = _ditiStatus.Split(',');
                    if (oneditiStatus_s.Count() != 2)
                        continue;

                    System.Data.DataRow dRow = datatable.NewRow();
                    dRow["tip"] = int.Parse(oneditiStatus_s[0]);
                    maxTip = int.Parse(oneditiStatus_s[0]);
                    dRow["status"] = oneditiStatus_s[1];
                    string status = oneditiStatus_s[1];
                    string statusChn = string.Empty;
                    if (!string.IsNullOrEmpty(status) && status.Contains("DiTi already mounted"))
                    {
                        statusChn = WanTai.Common.Resources.WanTaiResource.DiTialreadymounted;
                    }
                    else if (!string.IsNullOrEmpty(status) && status.Contains("OK / automatic handling"))
                    {
                        statusChn = WanTai.Common.Resources.WanTaiResource.OKautomatichandling;
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

            btnDiTiContPipetNone.Text = WanTai.Common.Resources.WanTaiResource.btnDiTiContPipetNone;
            btnRetrySameSpot.Text = WanTai.Common.Resources.WanTaiResource.RetryAtSamePosition;
            btnEjectThenMountNewDiTi.Text = WanTai.Common.Resources.WanTaiResource.btnEjectThenMountNewDiTi;            
            btnAbort.Text = WanTai.Common.Resources.WanTaiResource.btnAbort;
        }       

        private void dataGridView1_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            int index = 0;
            foreach (DataRow row in ((DataTable)dataGridView1.DataSource).Rows)
            {
                string status = row["status"].ToString();
                if (status.Contains("DiTi already mounted"))
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
        
        private void btnAbort_Click(object sender, EventArgs e)
        {
            result = ErrorResponseOption.DLG_ABORT;
            this.Close();
        }

        private void btnDiTiContPipetNone_Click(object sender, EventArgs e)
        {
            result = ErrorResponseOption.DLG_DITI_CONT_PIPETNONE;
            this.Close();
        }

        private void btnRetrySameSpot_Click(object sender, EventArgs e)
        {
            result = ErrorResponseOption.DLG_RETRY_SAMESPOT;
            this.Close();
        }

        private void btnEjectThenMountNewDiTi_Click(object sender, EventArgs e)
        {
            result = ErrorResponseOption.DLG_EJECT_MOUNT_NEWDITI;
            this.Close();
        }        
    }
}
