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
    public partial class MsgUserPrompt : Form
    {
        private XmlMsgUserPrompt _MsgUserPrompt;
        private ErrorResponseOption result;

        public ErrorResponseOption ErrorResponse
        {
            get { return result; }
        }

        public MsgUserPrompt(XmlMsgUserPrompt MsgUserPrompt)
        {            
            InitializeComponent();
            _MsgUserPrompt = MsgUserPrompt;
            this.MessageText.Text = _MsgUserPrompt.MessageText;
            if (string.IsNullOrEmpty(_MsgUserPrompt.btnOKCaption))
            {
                this.btnOK.Visible = false;
            }
            if (string.IsNullOrEmpty(_MsgUserPrompt.btnCancelCaption))
            {
                this.btnCancel.Visible = false;
            }            
        }       

        private void btnOK_Click(object sender, EventArgs e)
        {
            result = ErrorResponseOption.DLG_OK;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            result = ErrorResponseOption.DLG_CANCEL;
            this.Close();
        }


    }
}
