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
    public partial class DiluterCheckError : Form
    {
        private XmlMsgDiluterCheckError _msg;
        private ErrorResponseOption result;

        public ErrorResponseOption ErrorResponse
        {
            get { return result; }
        }

        public DiluterCheckError(XmlMsgDiluterCheckError msg)
        {
            InitializeComponent();

            _msg = msg;
            string message = _msg.errorPrompt;
            if (!string.IsNullOrEmpty(message) && message.Contains("too much in diluter"))
            {
                message = message.Substring(0, message.Length - "too much in diluter".Length);
                this.errorPrompt.Text = message + WanTai.Common.Resources.WanTaiResource.DiluterOverflowError;
            }
            else
            {
                this.errorPrompt.Text = _msg.errorPrompt;
            }

            this.ignore.Text = WanTai.Common.Resources.WanTaiResource.Ignore;
            this.cancel.Text = WanTai.Common.Resources.WanTaiResource.BtnCancel;
        }

        private void ignore_Click(object sender, EventArgs e)
        {
            result = ErrorResponseOption.DLG_IGNORE;
            this.Close();
        }

        private void cancel_Click(object sender, EventArgs e)
        {
            result = ErrorResponseOption.DLG_CANCEL;
            this.Close();
        }
    }
}
