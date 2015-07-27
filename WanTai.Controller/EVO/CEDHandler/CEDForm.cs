using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Xml;
using System.Windows.Forms;
using MCSLib;
using Tecan.MCS.Messages.CommonErrorDialog;

namespace WanTai.Controller.EVO
{
    public partial class CEDForm : Form, IReceiveMsg
    {
        LogOnCookie m_clientCookie;
        MsgSrv m_MCS;
        int m_clientID;
        int m_subscription;

        public CEDForm()
        {
            InitializeComponent();
            cedDescription.Visible = false;
            cedButton1.Visible = false;
            cedButton2.Visible = false;
            cedButton3.Visible = false;
            cedButton4.Visible = false;
            cedButton5.Visible = false;

            m_MCS = new MsgSrv();
            m_clientCookie = m_MCS.LogOn("CEDForm-" + System.Guid.NewGuid().ToString(), this);

            if (m_clientCookie == null) return;

            m_clientID = m_clientCookie.PartnerID;
            m_subscription = m_MCS.Subscribe(m_clientID, "CedResponse", "", "", "", "", "", 0);
        }

        uint selectedButton = 0;
        private string _syncID = "";

        public string SyncID
        {
            set { 
                _syncID = value; 
            }
        }

        ~CEDForm()
        {
            try
            {
                m_MCS.Unsubscribe(m_clientID, m_subscription);
                m_MCS.LogOff(ref m_clientCookie);
            }
            catch (Exception) { }
        }

        private void CEDForm_Load(object sender, EventArgs e)
        {
        }

        public string Description
        {
            set
            {
                cedDescription.Text = value;
                cedDescription.Visible = true;
            }
        }

        public string Button1
        {
            set
            {
                cedButton1.Text = value;
                cedButton1.Visible = true;
            }
        }
        public string Button2
        {
            set
            {
                cedButton2.Text = value;
                cedButton2.Visible = true;
            }
        }
        public string Button3
        {
            set
            {
                cedButton3.Text = value;
                cedButton3.Visible = true;
            }
        }
        public string Button4
        {
            set
            {
                cedButton4.Text = value;
                cedButton4.Visible = true;
            }
        }
        public string Button5
        {
            set
            {
                cedButton5.Text = value;
                cedButton5.Visible = true;
            }
        }
        public int DefaultButton
        {
            set
            {
                switch (value)
                {
                    case 1:
                        this.AcceptButton = cedButton1;
                        break;
                    case 2:
                        this.AcceptButton = cedButton2;
                        break;
                    case 3:
                        this.AcceptButton = cedButton3;
                        break;
                    case 4:
                        this.AcceptButton = cedButton4;
                        break;
                    case 5:
                        this.AcceptButton = cedButton5;
                        break;
                }
            }
        }
        public uint SelectedButton
        {
            get
            {
                return selectedButton;
            }
        }

        private void cedButton_Click(object sender, EventArgs e)
        {
            string name = ((Button)sender).Name;
            name = name.Substring(name.Length - 1);
            selectedButton = uint.Parse(name);
            this.Close();
            //Hide();
        }
        public void OnMsg(int MsgID, string MsgType, string MsgValue, string MsgGroup, string Sender, string SenderGroup, string Receiver, DateTime Timestamp)
        {
            try
            {
                XmlDocument wholeMessage = new XmlDocument();
                wholeMessage.LoadXml(MsgValue);
                XmlNode payloadNode = wholeMessage.SelectSingleNode(MsgType + "/Payload");
                CedResponse.PayloadData payload = new CedResponse.PayloadData();
                payload.ReadXml(payloadNode);
                if (payload.SyncId == _syncID)
                    Hide();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
    }
}
