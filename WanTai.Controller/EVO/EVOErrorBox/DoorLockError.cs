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
    public partial class DoorLockError : Form
    {
        private XmlMsgDoorLockError _Msg;
        private ErrorResponseOption result;

        public ErrorResponseOption ErrorResponse
        {
            get { return result; }
        }

        public DoorLockError(XmlMsgDoorLockError Msg)
        {            
            InitializeComponent();
            _Msg = Msg;
            string text = _Msg.userPrompt;
            if (text.Contains("Doorlock") && text.Contains("not activated"))
            {
                string doorNumber = text.Substring("Doorlock".Length + 1, 1);
                this.UserPrompt.Text = string.Format(WanTai.Common.Resources.WanTaiResource.Doorlocknotactivated, doorNumber);
            }
            else
            {
                this.UserPrompt.Text = _Msg.userPrompt;
            }

            BtnSwitchLock.Text = WanTai.Common.Resources.WanTaiResource.BtnSwitchLock;
            BtnCancel.Text = WanTai.Common.Resources.WanTaiResource.BtnCancel;
        }

        private void BtnSwitchLock_Click(object sender, EventArgs e)
        {
            result = ErrorResponseOption.DLG_SWITCHLOCK;
            this.Close();
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            result = ErrorResponseOption.DLG_CANCEL;
            this.Close();
        }
    }
}
