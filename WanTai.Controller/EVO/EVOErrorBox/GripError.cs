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
    public partial class GripError : Form
    {
        private XmlMsgGripError _Msg;
        private ErrorResponseOption result;

        public ErrorResponseOption ErrorResponse
        {
            get { return result; }
        }

        public GripError(XmlMsgGripError Msg)
        {            
            InitializeComponent();
            _Msg = Msg;
            string text = _Msg.errorPrompt;
            if (text.Contains("Rack not fetched"))
            {                
                this.ErrorPrompt.Text = WanTai.Common.Resources.WanTaiResource.Racknotfetched;
            }
            else
            {
                this.ErrorPrompt.Text = _Msg.errorPrompt;
            }

            BtnRetry.Text = WanTai.Common.Resources.WanTaiResource.BtnRetry;
            BtnIgnore.Text = WanTai.Common.Resources.WanTaiResource.Ignore;
            BtnAbort.Text = WanTai.Common.Resources.WanTaiResource.btnAbort;
        }

        private void BtnRetry_Click(object sender, EventArgs e)
        {
            result = ErrorResponseOption.DLG_RETRY;
            this.Close();
        }

        private void BtnIgnore_Click(object sender, EventArgs e)
        {
            result = ErrorResponseOption.DLG_IGNORE;
            this.Close();
        }

        private void BtnAbort_Click(object sender, EventArgs e)
        {
            result = ErrorResponseOption.DLG_ABORT;
            this.Close();
        }
        
    }
}
