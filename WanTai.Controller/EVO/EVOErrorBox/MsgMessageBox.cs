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
    public partial class MsgMessageBox : Form
    {
        private XmlMsgMessageBox _MsgMessageBox;
        private ErrorResponseOption result;

        public ErrorResponseOption ErrorResponse
        {
            get { return result; }
        }

        public MsgMessageBox(XmlMsgMessageBox MsgMessageBox)
        {            
            InitializeComponent();
            _MsgMessageBox = MsgMessageBox;
            if (_MsgMessageBox.errorPrompt.Contains("Evoware switches to offline mode and aborts script"))
            {
                this.errorPrompt.Text = WanTai.Common.Resources.WanTaiResource.OffLineModeMessage;
            }
            else if (_MsgMessageBox.errorPrompt.Contains("No connection to the instrument"))
            {
                this.errorPrompt.Text = WanTai.Common.Resources.WanTaiResource.OffLineMessage;
            }
            else
            {
                this.errorPrompt.Text = _MsgMessageBox.errorPrompt;
            }
            this.Text = _MsgMessageBox.hdrCaption;
            if (string.IsNullOrEmpty(_MsgMessageBox.btnOneCaption))
            {
                this.btnOne.Visible = false;
            }
            else
            {
                string text = WanTai.Common.Resources.WanTaiResource.ResourceManager.GetString(_MsgMessageBox.btnOneCaption, WanTai.Common.Resources.WanTaiResource.Culture);
                if (!string.IsNullOrEmpty(text))
                {
                    this.btnOne.Text = text;
                }
                else
                {
                    this.btnOne.Text = _MsgMessageBox.btnOneCaption;
                }                
            }

            if (string.IsNullOrEmpty(_MsgMessageBox.btnTwoCaption))
            {
                this.btnTwo.Visible = false;
            }
            else
            {
                string text = WanTai.Common.Resources.WanTaiResource.ResourceManager.GetString(_MsgMessageBox.btnTwoCaption, WanTai.Common.Resources.WanTaiResource.Culture);
                if (!string.IsNullOrEmpty(text))
                {
                    this.btnTwo.Text = text;
                }
                else
                {
                    this.btnTwo.Text = _MsgMessageBox.btnTwoCaption;
                }  
            }

            if (string.IsNullOrEmpty(_MsgMessageBox.btnThreeCaption))
            {
                this.btnThree.Visible = false;
            }
            else
            {
                string text = WanTai.Common.Resources.WanTaiResource.ResourceManager.GetString(_MsgMessageBox.btnThreeCaption, WanTai.Common.Resources.WanTaiResource.Culture);
                if (!string.IsNullOrEmpty(text))
                {
                    this.btnThree.Text = text;
                }
                else
                {
                    this.btnThree.Text = _MsgMessageBox.btnThreeCaption;
                }  
            }
        }
        
        private void btnOne_Click(object sender, EventArgs e)
        {
            result = (ErrorResponseOption)Enum.Parse(typeof(ErrorResponseOption), "DLG_" + _MsgMessageBox.btnOneCaption);
            this.Close();
        }

        private void btnTwo_Click(object sender, EventArgs e)
        {
            result = (ErrorResponseOption)Enum.Parse(typeof(ErrorResponseOption), "DLG_" + _MsgMessageBox.btnTwoCaption);
            this.Close();
        }

        private void btnThree_Click(object sender, EventArgs e)
        {
            result = (ErrorResponseOption)Enum.Parse(typeof(ErrorResponseOption), "DLG_" + _MsgMessageBox.btnThreeCaption);
            this.Close();
        }
    }
}
