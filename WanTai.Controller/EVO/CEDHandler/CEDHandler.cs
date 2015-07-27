using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Threading;
using MCSLib;
using Tecan.MCS.Messages.CommonErrorDialog;
using WanTai.Common.Resources;

namespace WanTai.Controller.EVO
{
    class CEDHandler : IReceiveMsg, IDisposable
    {
        LogOnCookie m_clientCookie;
        MsgSrv m_MCS;
        int m_clientID;
        int m_subscription;
        string syncID = "";

        public CEDHandler()
        {
            m_MCS = new MsgSrv();
            m_clientCookie = m_MCS.LogOn("CEDHandler-" + System.Guid.NewGuid().ToString(), this);

            if (m_clientCookie == null) return;

            m_clientID = m_clientCookie.PartnerID;
            m_subscription = m_MCS.Subscribe(m_clientID, "CedChallenge", "", "", "", "", "", 0);
        }

        //~CEDHandler()
        //{
        //    try
        //    {
        //        m_MCS.Unsubscribe(m_clientID, m_subscription);
        //        m_MCS.LogOff(ref m_clientCookie);
        //    }
        //    catch(Exception) {}
        //}

        public void Dispose()
        {
            try
            {
                if (m_clientCookie != null)
                {
                    m_MCS.Unsubscribe(m_clientID, m_subscription);
                    m_MCS.LogOff(ref m_clientCookie);
                }

                m_clientCookie = null;
            }
            catch (Exception) { }
        }

        public void OnMsg(int MsgID, string MsgType, string MsgValue, string MsgGroup, string Sender, string SenderGroup, string Receiver, DateTime Timestamp)
        {
            try
            {
                XmlDocument wholeMessage = new XmlDocument();
                wholeMessage.LoadXml(MsgValue);
                XmlNode payloadNode = wholeMessage.SelectSingleNode(MsgType + "/Payload");
                CedChallenge.PayloadData payload = new CedChallenge.PayloadData();
                payload.ReadXml(payloadNode);
                CEDForm cedForm = new CEDForm();
                string desc = payload.Description;
                if (!string.IsNullOrEmpty(desc) && desc.Contains("Evoware switches to offline mode and aborts script"))
                {
                    EVOApiProcessor.EVOOffline = true;
                    cedForm.Description = WanTaiResource.OffLineMessage;
                } 
                else if (!string.IsNullOrEmpty(desc) && desc.Contains("Error reading barcode of "))
                {
                    string errorMsg =  WanTaiResource.ErrorReadingBarcode;                    
                    string[] descStr = desc.Split('#');
                    if (descStr.Length > 1)
                    {
                        errorMsg = descStr[1] + " " + errorMsg;
                    }

                    if (desc.Contains("tube"))
                    {
                        errorMsg = "采血管" + errorMsg;
                    }
                    else if (desc.Contains("labware"))
                    {
                        errorMsg = "labware" + errorMsg;
                    }

                    cedForm.Description = errorMsg;
                }
                else
                {
                    cedForm.Description = payload.Description;
                }

                if (payload.Buttons.Count > 0)
                {
                    string buttonText = payload.Buttons[0].ButtonText;
                    if (buttonText.StartsWith("&") && buttonText.Length>1)
                        buttonText = buttonText.Substring(1);
                    string text = WanTaiResource.ResourceManager.GetString(buttonText, WanTaiResource.Culture);
                    if (!string.IsNullOrEmpty(text))
                    {
                        cedForm.Button1 = text;
                    }
                    else
                    {
                        cedForm.Button1 = payload.Buttons[0].ButtonText;
                    }
                }
                if (payload.Buttons.Count > 1)
                {
                    string buttonText = payload.Buttons[1].ButtonText;
                    if (buttonText.StartsWith("&") && buttonText.Length > 1)
                        buttonText = buttonText.Substring(1);
                    string text = WanTaiResource.ResourceManager.GetString(buttonText, WanTaiResource.Culture);
                    if (!string.IsNullOrEmpty(text))
                    {
                        cedForm.Button2 = text;
                    }
                    else
                    {
                        cedForm.Button2 = payload.Buttons[1].ButtonText;
                    }
                }
                if (payload.Buttons.Count > 2)
                {
                    string buttonText = payload.Buttons[2].ButtonText;
                    if (buttonText.StartsWith("&") && buttonText.Length > 1)
                        buttonText = buttonText.Substring(1);
                    string text = WanTaiResource.ResourceManager.GetString(buttonText, WanTaiResource.Culture);
                    if (!string.IsNullOrEmpty(text))
                    {
                        cedForm.Button3 = text;
                    }
                    else
                    {
                        cedForm.Button3 = payload.Buttons[2].ButtonText;
                    }
                }
                if (payload.Buttons.Count > 3)
                {
                    string buttonText = payload.Buttons[3].ButtonText;
                    if (buttonText.StartsWith("&") && buttonText.Length > 1)
                        buttonText = buttonText.Substring(1);
                    string text = WanTaiResource.ResourceManager.GetString(buttonText, WanTaiResource.Culture);
                    if (!string.IsNullOrEmpty(text))
                    {
                        cedForm.Button4 = text;
                    }
                    else
                    {
                        cedForm.Button4 = payload.Buttons[3].ButtonText;
                    }
                }
                if (payload.Buttons.Count > 4)
                {
                    string buttonText = payload.Buttons[4].ButtonText;
                    if (buttonText.StartsWith("&") && buttonText.Length > 1)
                        buttonText = buttonText.Substring(1);
                    string text = WanTaiResource.ResourceManager.GetString(buttonText, WanTaiResource.Culture);
                    if (!string.IsNullOrEmpty(text))
                    {
                        cedForm.Button5 = text;
                    }
                    else
                    {
                        cedForm.Button5 = payload.Buttons[4].ButtonText;
                    }
                }
                for (int i = 0; i < payload.Buttons.Count; i++)
                {
                    if (payload.Buttons[i].IsDefault)
                        cedForm.DefaultButton = i + 1;
                }

                cedForm.SyncID = payload.SyncId;
                cedForm.Show();
                while (cedForm.Visible)
                {
                    Thread.Sleep(200);
                    Application.DoEvents();
                }
                if (cedForm.SelectedButton > 0)
                {
                    CedResponse response = new CedResponse();
                    response.Payload = new CedResponse.PayloadData();
                    response.Payload.SyncId = payload.SyncId;
                    response.Payload.HandledByUser = true;
                    response.Payload.Username = "CEDHandler";
                    response.Payload.SelectedButton = cedForm.SelectedButton;
                    string message = response.ToString();
                    m_MCS.PostMessage(m_clientID, "", "CedResponse", message, "", "", null, 0, 0, 0);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
    }
}
