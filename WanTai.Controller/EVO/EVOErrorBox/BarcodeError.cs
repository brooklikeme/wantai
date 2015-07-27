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
    public partial class BarcodeError : Form
    {
        private XmlMsgBarcodeError _MsgBarcodeError;
        private ErrorResponseOption result;

        public ErrorResponseOption ErrorResponse
        {
            get { return result; }
        }

        public BarcodeError(XmlMsgBarcodeError MsgBarcodeError)
        {            
            InitializeComponent();
            _MsgBarcodeError = MsgBarcodeError;
            this.ErrPrompt.Text = _MsgBarcodeError.errorPrompt;
            if (string.IsNullOrEmpty(_MsgBarcodeError.btnRetryCaption))
            {
                this.BtnRetry.Visible = false;
            }
            if (string.IsNullOrEmpty(_MsgBarcodeError.btnEnterCaption))
            {
                this.BtnEnter.Visible = false;
            }
            if (string.IsNullOrEmpty(_MsgBarcodeError.btnHelpCaption))
            {
                this.BtnHelp.Visible = false;
            }
            if (string.IsNullOrEmpty(_MsgBarcodeError.btnOverwriteCaption))
            {
                this.BtnOverwrite.Visible = false;
            }
            if (string.IsNullOrEmpty(_MsgBarcodeError.btnCancelCaption))
            {
                this.BtnCancel.Visible = false;
            }
        }

        private void BtnRetry_Click(object sender, EventArgs e)
        {
            result = ErrorResponseOption.DLG_RETRY;
            this.Close();
        }

        private void BtnEnter_Click(object sender, EventArgs e)
        {
            result = ErrorResponseOption.DLG_ENTER;
            this.Close();
        }

        private void BtnHelp_Click(object sender, EventArgs e)
        {
            result = ErrorResponseOption.DLG_HELP;
            this.Close();
        }

        private void BtnOverwrite_Click(object sender, EventArgs e)
        {
            result = ErrorResponseOption.DLG_OVERWRITE;
            this.Close();
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            result = ErrorResponseOption.DLG_CANCEL;
            this.Close();
        }  


    }
}
