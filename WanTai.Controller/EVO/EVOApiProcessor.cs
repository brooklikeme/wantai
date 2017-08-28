using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using EVOAPILib;
using EVOApiErrorMsg;
using WanTai.Common;
using WanTai.DataModel;
using System.Runtime.InteropServices;
namespace WanTai.Controller.EVO
{
    class EVOApiProcessor : IProcessor, MCSLib.IReceiveMsg
    {
        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("user32.dll", EntryPoint = "SetWindowText")]
        public static extern int SetWindowText(IntPtr hwnd, string lpString);
        private void SetFreedomEVOwareWindowText()
        {
            IntPtr hwnd_win;
            hwnd_win = FindWindow(null, "Freedom EVOware");
            SetWindowText(hwnd_win, "WanTag");
        }

        private static EVOApiProcessor instance;
        string m_mcsReceiver;
        string m_clientName;
        int m_curMsgID;
        EVOAPILib.SystemClass m_evoSys;
        EVOAPILib.ScriptClass m_evoScript;
        int iScriptID = -1;

        //EVOApi Error Message Section
        EVOApiErrorMsg.CMcsSvr m_mcsErrorWrapper;
        MCSLib.LogOnCookie m_clientCookie;
        int m_clientID;
        CEDHandler cedHandler;
    
        EVOAPILib.SC_Status m_currentStatus; //current status of evosys component  
        private System.Collections.Generic.List<string> errorList;

        public override event OnEvoError onReceiveError;
        public override event OnEvoError onSendErrorResponse;
        public override event OnEvoError onOfflineStatus;
        public override event OnNextTurnStep onOnNextTurnStepHandler; 
        public static bool EVOOffline = false;
        //No connection to the instrument
        private bool isNoConnection = false;

        #region 2012-1-5 perry修改运行
        public override bool isOnNextTurnStep()
        {
            if (onOnNextTurnStepHandler == null)
                return false;
            else
                return true;
        }
        public override void OnNextTurnStepDispse()
        {
            onOnNextTurnStepHandler = null;
        }
        #endregion
        protected EVOApiProcessor()
        {            
            Application.DoEvents();
            InitializeCOMComponents();
            SettingEVOApiTestInitialState();
            cedHandler = new CEDHandler();
            HideEVOGui(); 
            StartEVOWare();                      
        }

        public static EVOApiProcessor Instance()
        {
            if (instance == null)
                instance = new EVOApiProcessor();

            return instance;
        }        

        /// <summary>
        /// Instantiate MCS, EVOApiErrorMsg, EVOApi objects here
        /// </summary>
        private void InitializeCOMComponents()
        {
            this.m_mcsReceiver = "Evoware";
            this.m_clientName = "NET_Client";
            this.m_curMsgID = 0;
            this.m_evoSys = new EVOAPILib.SystemClass();
            this.m_evoScript = new EVOAPILib.ScriptClass();
            this.m_mcsErrorWrapper = new EVOApiErrorMsg.CMcsSvr(m_clientName);

            //Delegate all COM events handler implementation here
            this.m_evoSys.StatusChanged += new _ISystemEvents_StatusChangedEventHandler(this.StatusChanged);
            this.m_evoSys.ErrorEvent += new _ISystemEvents_ErrorEventEventHandler(this.ErrorEvent);
            //this.m_evoSys.LogonTimeoutEvent += new _ISystemEvents_LogonTimeoutEventEventHandler(this.LogonTimeoutEvent);
            this.m_evoSys.UserPromptEvent += new _ISystemEvents_UserPromptEventEventHandler(this.UserPromptEvent);
        }

        private void SettingEVOApiTestInitialState()
        {
            m_clientCookie = m_mcsErrorWrapper.Connect(m_clientName, (MCSLib.IReceiveMsg)this);

            if (m_clientCookie == null)
            {
                LogInfoController.AddLogInfo(LogInfoLevelEnum.Error, "m_clientCookie is null, can not connect to MCS", SessionInfo.LoginName, this.GetType().ToString() + "->" + "SettingEVOApiTestInitialState()", SessionInfo.ExperimentID);
                return;
            }

            m_clientID = m_clientCookie.PartnerID;
            m_mcsErrorWrapper.Subscribe(m_clientID, MSGChannels.MsgErrorsChannel);
            LogInfoController.AddLogInfo(LogInfoLevelEnum.Info, "Successful connect to MCS", SessionInfo.LoginName, this.GetType().ToString() + "->" + "SettingEVOApiTestInitialState()", SessionInfo.ExperimentID);                
        }


        private void StatusChanged(EVOAPILib.SC_Status Status)
        {            
            m_currentStatus = Status;

            string statusText = "";

            if (Status == 0)
                statusText += "UNKNOWN ";

            // System init / shutdown
            if ((Status & EVOAPILib.SC_Status.STATUS_INITIALIZED) == EVOAPILib.SC_Status.STATUS_INITIALIZED)
                statusText += "INITIALIZED ";
            if ((Status & EVOAPILib.SC_Status.STATUS_INITIALIZING) == EVOAPILib.SC_Status.STATUS_INITIALIZING)
                statusText += "INITIALIZING ";
            if ((Status & EVOAPILib.SC_Status.STATUS_LOADING) == EVOAPILib.SC_Status.STATUS_LOADING)
                statusText += "LOADING ";
            if ((Status & EVOAPILib.SC_Status.STATUS_LOGON_ERROR) == EVOAPILib.SC_Status.STATUS_LOGON_ERROR)
                statusText += "LOGON_ERROR ";
            if ((Status & EVOAPILib.SC_Status.STATUS_NOINTRUMENTS) == EVOAPILib.SC_Status.STATUS_NOINTRUMENTS)
                statusText += "NOINTRUMENTS ";
            if ((Status & EVOAPILib.SC_Status.STATUS_NOTINITIALIZED) == EVOAPILib.SC_Status.STATUS_NOTINITIALIZED)
                statusText += "NOTINITIALIZED ";
            if ((Status & EVOAPILib.SC_Status.STATUS_SHUTTINGDOWN) == EVOAPILib.SC_Status.STATUS_SHUTTINGDOWN)
                statusText += "SHUTTINGDOWN ";
            if ((Status & EVOAPILib.SC_Status.STATUS_SHUTDOWN) == EVOAPILib.SC_Status.STATUS_SHUTDOWN)
                statusText += "SHUTDOWN ";
            if ((Status & EVOAPILib.SC_Status.STATUS_UNLOADING) == EVOAPILib.SC_Status.STATUS_UNLOADING)
                statusText += "UNLOADING ";

            // Script status (Standard)
            if ((Status & EVOAPILib.SC_Status.STATUS_IDLE) == EVOAPILib.SC_Status.STATUS_IDLE)
                statusText += "IDLE ";
            if ((Status & EVOAPILib.SC_Status.STATUS_BUSY) == EVOAPILib.SC_Status.STATUS_BUSY)
                statusText += "BUSY ";
            if ((Status & EVOAPILib.SC_Status.STATUS_PIPETTING) == EVOAPILib.SC_Status.STATUS_PIPETTING)
                statusText += "PIPETTING ";
            if ((Status & EVOAPILib.SC_Status.STATUS_ABORTED) == EVOAPILib.SC_Status.STATUS_ABORTED)
                statusText += "ABORTED ";
            if ((Status & EVOAPILib.SC_Status.STATUS_STOPPED) == EVOAPILib.SC_Status.STATUS_STOPPED)
                statusText += "STOPPED ";
            if ((Status & EVOAPILib.SC_Status.STATUS_SIMULATION) == EVOAPILib.SC_Status.STATUS_SIMULATION)
                statusText += "SIMULATION ";

            // Process status (Plus)
            if ((Status & EVOAPILib.SC_Status.STATUS_RUNNING) == EVOAPILib.SC_Status.STATUS_RUNNING)
                statusText += "RUNNING ";
            if ((Status & EVOAPILib.SC_Status.STATUS_PAUSEREQUESTED) == EVOAPILib.SC_Status.STATUS_PAUSEREQUESTED)
                statusText += "PAUSEREQUESTED ";
            if ((Status & EVOAPILib.SC_Status.STATUS_PAUSED) == EVOAPILib.SC_Status.STATUS_PAUSED)
                statusText += "PAUSED ";
            if ((Status & EVOAPILib.SC_Status.STATUS_ERROR) == EVOAPILib.SC_Status.STATUS_ERROR)
                statusText += "ERROR ";
            if ((Status & EVOAPILib.SC_Status.STATUS_EXECUTIONERROR) == EVOAPILib.SC_Status.STATUS_EXECUTIONERROR)
                statusText += "EXECUTIONERROR ";
            if ((Status & EVOAPILib.SC_Status.STATUS_RESOURCEMISSING) == EVOAPILib.SC_Status.STATUS_RESOURCEMISSING)
                statusText += "RESOURCEMISSING ";
            if ((Status & EVOAPILib.SC_Status.STATUS_DEADLOCK) == EVOAPILib.SC_Status.STATUS_DEADLOCK)
                statusText += "DEADLOCK";
            if ((Status & EVOAPILib.SC_Status.STATUS_TIMEVIOLATION) == EVOAPILib.SC_Status.STATUS_TIMEVIOLATION)
                statusText += "TIMEVIOLATION ";

            if (isNoConnection && statusText.Equals("loading error",StringComparison.CurrentCultureIgnoreCase))
            {
                EVOOffline = true;
                if (onOfflineStatus != null)
                {
                    onOfflineStatus();
                }

                isNoConnection = false;
            }

            LogInfoController.AddLogInfo(LogInfoLevelEnum.Debug, "m_currentStatus:" + m_currentStatus.ToString("x") + ",statusText" + statusText, SessionInfo.LoginName, this.GetType().ToString() + "->" + "StatusChanged()", SessionInfo.ExperimentID);
        }

        private void ErrorEvent(DateTime StartTime, DateTime EndTime, string Device, string Macro, string Object, string Message, short Status, string ProcessName, int ProcessID, string MacroID)
        {
            string msgError;

            msgError = string.Format(WanTai.Common.Resources.WanTaiResource.EVOAPI_EVENTLOG,
                StartTime, EndTime, Device, Macro, Object, Message, Status, ProcessName, ProcessID, MacroID);

            LogInfoController.AddLogInfo(LogInfoLevelEnum.EVORunTime, "msg:" + msgError, SessionInfo.LoginName, this.GetType().ToString() + "->" + "ErrorEvent()", SessionInfo.ExperimentID);
        
            if (errorList != null)
            {
                errorList.Add(Message);
            }
        }

        public void UserPromptEvent(int ID, string Text, string Caption, int Choices, out int Answer)
        {
            // TODO:  Add EVOApiDlg.UserPromptEvent implementation
            Answer = 0;
        }

        private void CheckEvoSystemClass()
        {
            if (m_evoSys == null)
                throw new Exception("EVO System Class is Null");
        }

        /// <summary>
        /// Start Evoware in standard mode with login/password
        /// </summary>
        private void StartEVOWare()
        {
            CheckEvoSystemClass();

            string userName;
            string password;
            bool bSimulation;

            userName = WanTai.Common.Configuration.GetLogonUserName();
            password = WanTai.Common.Configuration.GetLogonPassword();
            bSimulation = bool.Parse(WanTai.Common.Configuration.GetLogonSimulation());

            //Log on, this takes a few seconds has to wait til status = STATUS_NOTINITIALIZED or STATUS_INITIALIZED 
            m_evoSys.Logon(userName, password, 0, bSimulation ? 1 : 0);
            StatusChanged(m_evoSys.GetStatus()); 

            do
            {
                Application.DoEvents();
                System.Threading.Thread.Sleep(250);
            } while (((m_currentStatus & EVOAPILib.SC_Status.STATUS_LOADING) == EVOAPILib.SC_Status.STATUS_LOADING)
                && ((m_currentStatus & EVOAPILib.SC_Status.STATUS_ERROR) != EVOAPILib.SC_Status.STATUS_ERROR));
            SetFreedomEVOwareWindowText();

            //allow some extra time till evoware is completly loaded
            int index = 0;
            do
            {
                Application.DoEvents();
                System.Threading.Thread.Sleep(1000);
                index++;
            } while (index < 5);            

            if (!EVOOffline)
            {
                try
                {
                    System.Threading.Thread.Sleep(5000);
                    string scanTubeScriptName = WanTai.Common.Configuration.GetTecanRestorationScriptName();
                    m_evoSys.PrepareScript(scanTubeScriptName);
                    m_evoSys.Initialize();
                    do
                    {
                        Application.DoEvents();
                        System.Threading.Thread.Sleep(1000);
                    } while ((m_currentStatus != EVOAPILib.SC_Status.STATUS_INITIALIZED) && (m_currentStatus != (EVOAPILib.SC_Status.STATUS_INITIALIZED | EVOAPILib.SC_Status.STATUS_IDLE)));
                }
                catch
                {
                    try
                    {
                        System.Threading.Thread.Sleep(10000);
                        string scanTubeScriptName = WanTai.Common.Configuration.GetTecanRestorationScriptName();
                        m_evoSys.PrepareScript(scanTubeScriptName);
                        m_evoSys.Initialize();
                        do
                        {
                            Application.DoEvents();
                            System.Threading.Thread.Sleep(1000);
                        } while ((m_currentStatus != EVOAPILib.SC_Status.STATUS_INITIALIZED) && (m_currentStatus != (EVOAPILib.SC_Status.STATUS_INITIALIZED | EVOAPILib.SC_Status.STATUS_IDLE)));
                    }
                    catch
                    {

                    }
                }
            }
        }

        private void HideEVOGui()
        {
            CheckEvoSystemClass();
            m_evoSys.HideGUI(1);
        }        

        private void LogOff()
        {
            Application.DoEvents();
            CheckEvoSystemClass();

            m_evoSys.Logoff();
            System.Threading.Thread.Sleep(3000);
        }

        private void ShutDown()
        {
            Application.DoEvents();
            CheckEvoSystemClass();

            m_evoSys.Shutdown();
            System.Threading.Thread.Sleep(15000);
        }

        //Handling event capture from MCS component
        #region IReceiveMsg Members

        public void OnMsg(int MsgID, string MsgType, string MsgValue, string MsgGroup, string Sender, string SenderGroup, string Receiver, DateTime Timestamp)
        {
            LogInfoController.AddLogInfo(LogInfoLevelEnum.Debug, "Into OnMsg, MsgValue = " + MsgValue, SessionInfo.LoginName, this.GetType().ToString() + "->" + "OnMsg()", SessionInfo.ExperimentID);
        
            if (onReceiveError != null)
            {
                onReceiveError();
            }

            string m_OriginMsgGUID = "";
            // create instance of XmlMsgBase and parse MsgValue content
            EVOApiErrorMsg.XmlMsgBase msgBase = new EVOApiErrorMsg.XmlMsgBase("", MsgValue);
            // save MsgGUID for response message (required since EVOApiErrorMessage V2.2.0.2)
            //m_OriginMsgGUID = msgBase.MsgGUID;
            System.Xml.XmlDocument xmlDoc = new System.Xml.XmlDocument();
            xmlDoc.LoadXml(MsgValue);
            System.Xml.XmlNode node = xmlDoc.SelectSingleNode("/*/Header/MsgID");
            if (null != node)
            {
                m_OriginMsgGUID = node.InnerText;
            }

            //Get Message type in order to create correct message object to access to the error member
            XmlMsgType msgType;
            msgType = (XmlMsgType)Enum.Parse(typeof(XmlMsgType), MsgType);
            switch (msgType)
            {
                case (XmlMsgType.MST_DetectError):
                    XmlMsgDetectError msgDetect = new XmlMsgDetectError(MsgValue);
                    msgDetect.ParsingXmlContent();
                    EVOErrorBox.MsgDetectError detectError = new EVOErrorBox.MsgDetectError(msgDetect);
                    detectError.ShowDialog();
                    SendRespons(m_OriginMsgGUID, detectError.ErrorResponse);
                    break;
                //the clot error message has no content
                case (XmlMsgType.MST_ClotError):
                    XmlMsgClotError msgClotError = new XmlMsgClotError(MsgValue);
                    msgClotError.ParsingXmlContent();
                    EVOErrorBox.ClotError clotError = new EVOErrorBox.ClotError(msgClotError);
                    clotError.ShowDialog();
                    SendRespons(m_OriginMsgGUID, clotError.ErrorResponse);
                    break;
                case(XmlMsgType.MST_DiluterCheckError):
                    XmlMsgDiluterCheckError msg = new XmlMsgDiluterCheckError(MsgValue);
                    msg.ParsingXmlContent();
                    EVOErrorBox.DiluterCheckError dulterCheckError = new EVOErrorBox.DiluterCheckError(msg);
                    dulterCheckError.ShowDialog();
                    SendRespons(m_OriginMsgGUID, dulterCheckError.ErrorResponse);
                    break;
                case (XmlMsgType.MST_DiTiFetchError):
                    XmlMsgDiTiFetchError msgDiTiFetch = new XmlMsgDiTiFetchError(MsgValue);
                    msgDiTiFetch.ParsingXmlContent();
                    EVOErrorBox.DiTiFetchError diTiFetchError = new EVOErrorBox.DiTiFetchError(msgDiTiFetch);
                    diTiFetchError.ShowDialog();
                    SendRespons(m_OriginMsgGUID, diTiFetchError.ErrorResponse);
                    break;
                case (XmlMsgType.MST_DiTiNotMountedError):
                    XmlMsgDiTiNotMountedError msgditiNotMount = new XmlMsgDiTiNotMountedError(MsgValue);
                    msgditiNotMount.ParsingXmlContent();
                    EVOErrorBox.DiTiNotMountedError ditiNotMount = new EVOErrorBox.DiTiNotMountedError(msgditiNotMount);
                    ditiNotMount.ShowDialog();
                    SendRespons(m_OriginMsgGUID, ditiNotMount.ErrorResponse);
                    break;
                case (XmlMsgType.MST_DiTiLostError):
                    XmlMsgDiTiLostError msgditiLost = new XmlMsgDiTiLostError(MsgValue);
                    msgditiLost.ParsingXmlContent();
                    EVOErrorBox.DiTiLostError ditiLost = new EVOErrorBox.DiTiLostError(msgditiLost);
                    ditiLost.ShowDialog();
                    SendRespons(m_OriginMsgGUID, ditiLost.ErrorResponse);
                    break;
                case (XmlMsgType.MST_DiTiMountedError):
                    XmlMsgDiTiMountedError msgDitiMount = new XmlMsgDiTiMountedError(MsgValue);
                    msgDitiMount.ParsingXmlContent();
                    EVOErrorBox.DiTiMountedError DitiMount = new EVOErrorBox.DiTiMountedError(msgDitiMount);
                    DitiMount.ShowDialog();
                    SendRespons(m_OriginMsgGUID, DitiMount.ErrorResponse);
                    break;                
                case (XmlMsgType.MST_MessageBox):
                    XmlMsgMessageBox msgMB = new XmlMsgMessageBox(MsgValue);
                    msgMB.ParsingXmlContent();
                    if (msgMB.errorPrompt.Contains("Evoware switches to offline mode and aborts script"))
                    {
                        EVOOffline = true;
                        if (onOfflineStatus != null)
                        {
                            onOfflineStatus();
                        }
                    }

                    EVOErrorBox.MsgMessageBox messageBox = new EVOErrorBox.MsgMessageBox(msgMB);
                    messageBox.ShowDialog();
                    if (msgMB.errorPrompt.Contains("No connection to the instrument"))
                    {                        
                        if (messageBox.ErrorResponse == ErrorResponseOption.DLG_NO)
                        {
                            EVOOffline = true;
                            if (onOfflineStatus != null)
                            {
                                onOfflineStatus();
                            }
                        }
                        else
                        {
                            isNoConnection = true;
                        }
                    }
                    SendRespons(m_OriginMsgGUID, messageBox.ErrorResponse);
                    break;
                case (XmlMsgType.MST_DoorLockError):
                    XmlMsgDoorLockError msgDoorLockError = new XmlMsgDoorLockError(MsgValue);
                    msgDoorLockError.ParsingXmlContent();
                    EVOErrorBox.DoorLockError doorLockError = new EVOErrorBox.DoorLockError(msgDoorLockError);
                    doorLockError.ShowDialog();
                    SendRespons(m_OriginMsgGUID, doorLockError.ErrorResponse);
                    break;
                case (XmlMsgType.MST_GripError):
                    XmlMsgGripError msgGripError = new XmlMsgGripError(MsgValue);
                    msgGripError.ParsingXmlContent();
                    EVOErrorBox.GripError gripError = new EVOErrorBox.GripError(msgGripError);
                    gripError.ShowDialog();
                    SendRespons(m_OriginMsgGUID, gripError.ErrorResponse);
                    break;    
                default:
                    SendRespons(m_OriginMsgGUID, ErrorResponseOption.NONE);
                    break;
                //All other message type here
            }
        }

        #endregion

        private void SendRespons(string m_OriginMsgGUID, EVOApiErrorMsg.ErrorResponseOption Response)
        {
            //response to an error, depends on button options for each EVOware error message
            EVOApiErrorMsg.ErrorResponseOption iResponse;
            //Indicate the error display will be handled by the client application or EVOware
            EVOApiErrorMsg.ErrorControlModes iMode;
            //response message object
            EVOApiErrorMsg.XmlMsgResponse xmlMsg;
            iResponse = Response;
            //Error Control Mode whether let’s EVOware handling display the error, in this sample EVOware does
            //not need to display the error window
            if (Response == ErrorResponseOption.NONE)
            {
                iMode = EVOApiErrorMsg.ErrorControlModes.EvowareControled;
            }
            else
            {
                iMode = EVOApiErrorMsg.ErrorControlModes.RemotelyControled;
            }            
            //Instantiate new response message
            // the OriginMsgGUID is the MsgGUID of the message that we want to answer (required since
            // EVOApiErrorMessage V2.2.0.2, see example above)
            xmlMsg = new XmlMsgResponse(iMode, iResponse, m_OriginMsgGUID);
            //Sending response back to EVOware
            int MCSID = m_mcsErrorWrapper.SendingXmlMsg(m_clientID, xmlMsg, m_mcsReceiver,
            MSGChannels.MsgResponseChannel, 0);
            if (onSendErrorResponse != null)
            {
                onSendErrorResponse();
            }

            LogInfoController.AddLogInfo(LogInfoLevelEnum.Debug, "Send Reponse to EVO", SessionInfo.LoginName, this.GetType().ToString() + "->" + "SendRespons()", SessionInfo.ExperimentID);        
        }

        /// <summary>
        /// Executing StartScript Api Command
        /// </summary>
        public override bool StartScript(string sScriptName)
        {
            if (onOnNextTurnStepHandler != null)
                onOnNextTurnStepHandler(sScriptName);
            CheckEvoSystemClass();
            
            bool bSuccessful = false;           
            
            iScriptID = m_evoSys.PrepareScript(sScriptName);
            System.Threading.Thread.Sleep(2000);
            StatusChanged(m_evoSys.GetStatus()); 

            m_evoSys.Initialize();
            do
            {
                Application.DoEvents();
                System.Threading.Thread.Sleep(1000);                
            } while ((m_currentStatus != EVOAPILib.SC_Status.STATUS_INITIALIZED) && (m_currentStatus != (EVOAPILib.SC_Status.STATUS_INITIALIZED | EVOAPILib.SC_Status.STATUS_IDLE)));
                       
            System.Threading.Thread.Sleep(2000);
            errorList = new System.Collections.Generic.List<string>();

            m_evoSys.StartScript(iScriptID, 0, 0);
            System.Threading.Thread.Sleep(5000);
            do
            {
                Application.DoEvents();
                System.Threading.Thread.Sleep(1000);
                if (m_evoSys.GetScriptStatus(iScriptID) == EVOAPILib.SC_ScriptStatus.SS_ERROR || m_evoSys.GetScriptStatus(iScriptID) == EVOAPILib.SC_ScriptStatus.SS_STATUS_ERROR)
                {
                    bSuccessful = false;
                    return bSuccessful;
                }
            } while (m_evoSys.GetScriptStatus(iScriptID) == EVOAPILib.SC_ScriptStatus.SS_BUSY);

            try
            {
                if (m_evoSys.GetScriptStatus(iScriptID) == EVOAPILib.SC_ScriptStatus.SS_STOPPED)
                {
                    System.Threading.Thread.Sleep(5000);
                    if (errorList != null && errorList.Count > 0 &&
                        (errorList[errorList.Count - 1].StartsWith("Run finished with errors") || ((errorList.Count - 2) >= 0
                        && errorList[errorList.Count - 2].StartsWith("Run finished with errors"))))
                    {
                        bSuccessful = false;
                    }
                }
                else
                {
                    bSuccessful = true;
                }

                if (errorList != null)
                {
                    errorList.Clear();
                    errorList = null;
                }
            }
            catch (Exception e)
            {
                WanTai.Common.CommonFunction.WriteLog(e.Message);
                WanTai.Common.CommonFunction.WriteLog(e.StackTrace);
            }
            return bSuccessful;
        }

        /// <summary>
        /// Executing StartScript Api Command
        /// </summary>
        public override bool StartScript(string sScriptName, Dictionary<string, string> lVariables)
        {
            if (onOnNextTurnStepHandler != null)
                onOnNextTurnStepHandler(sScriptName);
            CheckEvoSystemClass();

            bool bSuccessful = false;

            this.iScriptID = m_evoSys.PrepareScript(sScriptName);
            System.Threading.Thread.Sleep(2000);

            m_evoSys.Initialize();
            do
            {
                Application.DoEvents();
                System.Threading.Thread.Sleep(1000);
            } while ((m_currentStatus != EVOAPILib.SC_Status.STATUS_INITIALIZED) && (m_currentStatus != (EVOAPILib.SC_Status.STATUS_INITIALIZED | EVOAPILib.SC_Status.STATUS_IDLE)));
            System.Threading.Thread.Sleep(2000);            

            if (lVariables != null && lVariables.Count > 0)
            {
                foreach (string key in lVariables.Keys)
                {
                    m_evoSys.SetScriptVariable(iScriptID, key, lVariables[key]);
                }
            }

            errorList = new System.Collections.Generic.List<string>();
            m_evoSys.StartScript(iScriptID, 0, 0);
            System.Threading.Thread.Sleep(5000);
            do
            {
                Application.DoEvents();
                System.Threading.Thread.Sleep(1000);
                if (m_evoSys.GetScriptStatus(iScriptID) == EVOAPILib.SC_ScriptStatus.SS_ERROR || m_evoSys.GetScriptStatus(iScriptID) == EVOAPILib.SC_ScriptStatus.SS_STATUS_ERROR)
                {
                    bSuccessful = false;
                    return bSuccessful;
                }
            } while (m_evoSys.GetScriptStatus(iScriptID) == EVOAPILib.SC_ScriptStatus.SS_BUSY);


            try
            {
                if (m_evoSys.GetScriptStatus(iScriptID) == EVOAPILib.SC_ScriptStatus.SS_STOPPED)
                {
                    System.Threading.Thread.Sleep(5000);
                    if (errorList != null && errorList.Count > 0 &&
                        (errorList[errorList.Count - 1].StartsWith("Run finished with errors") || ((errorList.Count - 2) >= 0
                        && errorList[errorList.Count - 2].StartsWith("Run finished with errors"))))
                    {
                        bSuccessful = false;
                    }
                }
                else
                {
                    bSuccessful = true;
                }

                if (errorList != null)
                {
                    errorList.Clear();
                    errorList = null;
                }
            }
            catch (Exception e)
            {
                WanTai.Common.CommonFunction.WriteLog(e.Message);
                WanTai.Common.CommonFunction.WriteLog(e.StackTrace);
            }
            return bSuccessful;
        }

        /// <summary>
        /// Executing StartStopScript Api Command
        /// </summary>
        public override void StopScript()
        {
            CheckEvoSystemClass();

            //wait a short time, then stop the script
            System.Threading.Thread.Sleep(2000);

            if (iScriptID!=-1 && m_evoSys.GetScriptStatus(iScriptID) == EVOAPILib.SC_ScriptStatus.SS_BUSY)
            {
                m_evoSys.Stop();
            }
        }

        public override bool PauseScript()
        {
            CheckEvoSystemClass();
            if (iScriptID != -1 && ((m_evoSys.GetStatus() & EVOAPILib.SC_Status.STATUS_BUSY) == EVOAPILib.SC_Status.STATUS_BUSY))
            {
                if (m_evoSys.GetScriptStatus(iScriptID) == EVOAPILib.SC_ScriptStatus.SS_BUSY)
                {
                    m_evoSys.Pause();
                    return true;
                }
            }

            return false;                   
        }

        public override bool ResumeScript()
        {
            CheckEvoSystemClass();

            bool bSuccessful = false;

            if (iScriptID != -1 && ((m_evoSys.GetStatus() & EVOAPILib.SC_Status.STATUS_PAUSED) == EVOAPILib.SC_Status.STATUS_PAUSED) && m_evoSys.GetScriptStatus(iScriptID) == EVOAPILib.SC_ScriptStatus.SS_PAUSED)
            {
                errorList = new System.Collections.Generic.List<string>();
                m_evoSys.Resume();

                System.Threading.Thread.Sleep(5000);

                do
                {
                    Application.DoEvents();
                    System.Threading.Thread.Sleep(1000);
                    if (m_evoSys.GetScriptStatus(iScriptID) == EVOAPILib.SC_ScriptStatus.SS_ERROR)
                    {
                        bSuccessful = false;
                        return bSuccessful;
                    }

                } while (m_evoSys.GetScriptStatus(iScriptID) == EVOAPILib.SC_ScriptStatus.SS_BUSY);

                if (m_evoSys.GetScriptStatus(iScriptID) == EVOAPILib.SC_ScriptStatus.SS_STOPPED)
                {
                    System.Threading.Thread.Sleep(5000);
                    if (errorList != null && errorList.Count > 0 &&
                    (errorList[errorList.Count - 1].StartsWith("Run finished with errors") || ((errorList.Count - 2) >= 0
                    && errorList[errorList.Count - 2].StartsWith("Run finished with errors"))))
                    {
                        bSuccessful = false;
                    }
                }
                else
                {
                    bSuccessful = true;
                }
            }
            else
            {
                bSuccessful = true;
            }

            errorList.Clear();
            errorList = null;
            return bSuccessful;
        }

        public override EVO_ScriptStatus GetScriptStatus()
        {
            if (iScriptID != -1)
            {
                SC_ScriptStatus status = m_evoSys.GetScriptStatus(iScriptID);
                int status_num = (int)status;

                return (EVO_ScriptStatus)status_num;
            }
            else
            {
                return EVO_ScriptStatus.NONE;
            }
        }

        /// <summary>
        /// Executing RecoverScript Api Command
        /// </summary>
        public override bool RecoverScript(string sScriptName, short startLineNumber)
        {
            //CheckEvoSystemClass();

            //bool bSuccessful = false;
            //string newScriptName = GenerateRecoverScriptFile(sScriptName);
            //if (string.IsNullOrEmpty(newScriptName))
            //{                
            //    return bSuccessful;
            //}            

            //bSuccessful = this.StartScript(newScriptName);
            //DeleteTempRecoverScriptFile(newScriptName);          

            //return bSuccessful;  
            if (onOnNextTurnStepHandler != null)
                onOnNextTurnStepHandler(sScriptName);
            CheckEvoSystemClass();

            bool bSuccessful = false;

            iScriptID = m_evoSys.PrepareScript(sScriptName);
            System.Threading.Thread.Sleep(2000);
            StatusChanged(m_evoSys.GetStatus()); 

            m_evoSys.Initialize();
            do
            {
                Application.DoEvents();
                System.Threading.Thread.Sleep(1000);
            } while ((m_currentStatus != EVOAPILib.SC_Status.STATUS_INITIALIZED) && (m_currentStatus != (EVOAPILib.SC_Status.STATUS_INITIALIZED | EVOAPILib.SC_Status.STATUS_IDLE)));

            System.Threading.Thread.Sleep(2000);
            errorList = new System.Collections.Generic.List<string>();

            m_evoSys.StartScript(iScriptID, startLineNumber, 0);
            System.Threading.Thread.Sleep(5000);
            do
            {
                Application.DoEvents();
                System.Threading.Thread.Sleep(1000);
                if (m_evoSys.GetScriptStatus(iScriptID) == EVOAPILib.SC_ScriptStatus.SS_ERROR || m_evoSys.GetScriptStatus(iScriptID) == EVOAPILib.SC_ScriptStatus.SS_STATUS_ERROR)
                {
                    bSuccessful = false;
                    return bSuccessful;
                }
            } while (m_evoSys.GetScriptStatus(iScriptID) == EVOAPILib.SC_ScriptStatus.SS_BUSY);

            if (m_evoSys.GetScriptStatus(iScriptID) == EVOAPILib.SC_ScriptStatus.SS_STOPPED)
            {
                System.Threading.Thread.Sleep(5000);
                if (errorList != null && errorList.Count > 0 &&
                    (errorList[errorList.Count - 1].StartsWith("Run finished with errors") || ((errorList.Count - 2) >= 0
                    && errorList[errorList.Count - 2].StartsWith("Run finished with errors"))))
                {
                    bSuccessful = false;
                }
            }
            else
            {
                bSuccessful = true;
            }

            errorList.Clear();
            errorList = null;
            return bSuccessful;
        }

        /// <summary>
        /// Executing RecoverScript Api Command
        /// </summary>
        public override bool RecoverScript(string sScriptName, Dictionary<string, string> lVariables, short startLineNumber)
        {
            if (onOnNextTurnStepHandler != null)
                onOnNextTurnStepHandler(sScriptName);
            CheckEvoSystemClass();

            bool bSuccessful = false;

            this.iScriptID = m_evoSys.PrepareScript(sScriptName);
            System.Threading.Thread.Sleep(2000);

            m_evoSys.Initialize();
            do
            {
                Application.DoEvents();
                System.Threading.Thread.Sleep(1000);
            } while ((m_currentStatus != EVOAPILib.SC_Status.STATUS_INITIALIZED) && (m_currentStatus != (EVOAPILib.SC_Status.STATUS_INITIALIZED | EVOAPILib.SC_Status.STATUS_IDLE)));
            System.Threading.Thread.Sleep(2000);

            if (lVariables != null && lVariables.Count > 0)
            {
                foreach (string key in lVariables.Keys)
                {
                    m_evoSys.SetScriptVariable(iScriptID, key, lVariables[key]);
                }
            }

            errorList = new System.Collections.Generic.List<string>();
            m_evoSys.StartScript(iScriptID, startLineNumber, 0);
            System.Threading.Thread.Sleep(5000);
            do
            {
                Application.DoEvents();
                System.Threading.Thread.Sleep(1000);
                if (m_evoSys.GetScriptStatus(iScriptID) == EVOAPILib.SC_ScriptStatus.SS_ERROR || m_evoSys.GetScriptStatus(iScriptID) == EVOAPILib.SC_ScriptStatus.SS_STATUS_ERROR)
                {
                    bSuccessful = false;
                    return bSuccessful;
                }
            } while (m_evoSys.GetScriptStatus(iScriptID) == EVOAPILib.SC_ScriptStatus.SS_BUSY);


            if (m_evoSys.GetScriptStatus(iScriptID) == EVOAPILib.SC_ScriptStatus.SS_STOPPED)
            {
                System.Threading.Thread.Sleep(5000);
                if (errorList != null && errorList.Count > 0 &&
                    (errorList[errorList.Count - 1].StartsWith("Run finished with errors") || ((errorList.Count - 2) >= 0
                    && errorList[errorList.Count - 2].StartsWith("Run finished with errors"))))
                {
                    bSuccessful = false;
                }
            }
            else
            {
                bSuccessful = true;
            }

            errorList.Clear();
            errorList = null;
            return bSuccessful;
        }

        public override bool CheckCanRecover(string sScriptName, out short last_error_line)
        {
            System.IO.FileInfo fileInfo = new System.IO.FileInfo(WanTai.Common.Configuration.GetEvoScriptFileLocation() + sScriptName);
            bool canRecover = false;
            last_error_line = 0;

            if (fileInfo.Exists)
            {
                string recFilePath = fileInfo.DirectoryName + "\\" + fileInfo.Name.Substring(0, fileInfo.Name.Length - fileInfo.Extension.Length) + ".rec";
                if (System.IO.File.Exists(recFilePath))
                {                   
                    using (System.IO.StreamReader reader = new System.IO.StreamReader(recFilePath))
                    {
                        while (!reader.EndOfStream)
                        {
                            string lineStr = reader.ReadLine();
                            if (!string.IsNullOrEmpty(lineStr) && lineStr.Contains("LAST_ERROR_LINE"))
                            {
                                string[] lines = lineStr.Split(';');
                                if (lines.Length == 2)
                                {
                                    last_error_line = short.Parse(lines[1]);
                                }
                            }
                        }
                    }

                    if (last_error_line > 0)
                    {
                        canRecover = true;
                    }
                }
            }

            return canRecover;
        }

        //private string GenerateRecoverScriptFile(string sScriptName)
        //{            
        //    string newScriptName = null;
        //    string destRecFilePath = null;
        //    string destFilePath = null;
        //    try
        //    {
        //        int last_error_line = 0;
        //        bool canRecover = CheckCanRecover(sScriptName, out last_error_line);
        //        if (canRecover && last_error_line > 0)
        //        {
        //            System.IO.FileInfo fileInfo = new System.IO.FileInfo(WanTai.Common.Configuration.GetEvoScriptFileLocation() + sScriptName);
        //            string recFilePath = fileInfo.DirectoryName + "\\" + System.IO.Path.GetFileNameWithoutExtension(fileInfo.Name) + ".rec";
        //            bool hasRPG = false;
        //            int currentLine = 0;
        //            m_evoScript.ReadScript(sScriptName);
        //            string destFileName = System.IO.Path.GetFileNameWithoutExtension(fileInfo.Name) + DateTime.Now.Ticks + fileInfo.Extension;
        //            m_evoScript.SaveScript(destFileName);
        //            destFilePath = fileInfo.DirectoryName + "\\" + destFileName;
        //            using (System.IO.StreamReader source = new System.IO.StreamReader(new System.IO.FileStream(fileInfo.FullName, System.IO.FileMode.Open)))
        //            {
        //                while (!source.EndOfStream)
        //                {
        //                    string lineStr = source.ReadLine();

        //                    if (lineStr.StartsWith("--{ RPG }--"))
        //                    {
        //                        hasRPG = true;
        //                    }

        //                    if (hasRPG)
        //                    {
        //                        if (currentLine >= last_error_line)
        //                        {
        //                            m_evoScript.AddScriptLine(lineStr);
        //                        }
        //                        else
        //                        {
        //                            if (lineStr.StartsWith("Variable("))
        //                            {
        //                                m_evoScript.AddScriptLine(lineStr);
        //                            }
        //                        }

        //                        currentLine++;
        //                    }
        //                }
                        
        //                destRecFilePath = fileInfo.DirectoryName + "\\" + System.IO.Path.GetFileNameWithoutExtension(destFileName) + ".rec";
        //                new System.IO.FileInfo(recFilePath).CopyTo(destRecFilePath);
        //                newScriptName = destFileName;
        //                m_evoScript.SaveScript(destFileName);
        //            }
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        if (!string.IsNullOrEmpty(destRecFilePath) && System.IO.File.Exists(destRecFilePath))
        //        {
        //            System.IO.File.Delete(destRecFilePath);
        //        }

        //        if (!string.IsNullOrEmpty(destFilePath) && System.IO.File.Exists(destFilePath))
        //        {
        //            System.IO.File.Delete(destFilePath);
        //        }

        //        throw;
        //    }

        //    return newScriptName;
        //}

        //private void DeleteTempRecoverScriptFile(string newScriptName)
        //{
        //    System.IO.FileInfo fileInfo = new System.IO.FileInfo(WanTai.Common.Configuration.GetEvoScriptFileLocation() + newScriptName);
        //    string destRecFilePath = WanTai.Common.Configuration.GetEvoScriptFileLocation() + fileInfo.Name.Substring(0, fileInfo.Name.Length - fileInfo.Extension.Length) + ".rec";
        //    if (System.IO.File.Exists(destRecFilePath))
        //    {
        //        System.IO.File.Delete(destRecFilePath);
        //    }
        //    System.IO.File.Delete(WanTai.Common.Configuration.GetEvoScriptFileLocation() + newScriptName);
        //}

        public override void Close()
        {
            LogOff();
            ShutDown();
            if (cedHandler != null)
            {
                cedHandler.Dispose();
                cedHandler = null;
            }
            if (m_evoSys != null)
                m_evoSys = null;
            if (instance != null)
                instance = null;
        }

        ~EVOApiProcessor()
        {
            try
            {
                if (cedHandler != null)
                {
                    cedHandler.Dispose();
                    cedHandler = null;
                }
            }
            catch(Exception) {}
        }

        public override void CloseLamp()
        {
            CheckEvoSystemClass();
            Application.DoEvents();

            m_evoSys.Initialize();
            int index = 0;
            do
            {               
                Application.DoEvents();
                System.Threading.Thread.Sleep(1000);
                if (index > 5)
                    break;
                index++;
            } while ((m_currentStatus != EVOAPILib.SC_Status.STATUS_INITIALIZED) && (m_currentStatus != (EVOAPILib.SC_Status.STATUS_INITIALIZED | EVOAPILib.SC_Status.STATUS_IDLE)) && (m_currentStatus != (EVOAPILib.SC_Status.STATUS_INITIALIZED | EVOAPILib.SC_Status.STATUS_BUSY)));


            m_evoSys.SetRemoteMode(1);//1: on; 0:off
            //m_evoSys.SetDoorLocks(1);//1:on; 0:off

            m_evoSys.SetLamp(EVOAPILib.SC_LampStatus.LAMP_OFF);
            //m_evoSys.SetDoorLocks(0);
            m_evoSys.SetRemoteMode(0);
        }
        //0-green,1-REDFLASHING,2-greenFlashing
        public override void SetLampStatus(int lampStatus)
        {
            CheckEvoSystemClass();
            Application.DoEvents();

            m_evoSys.Initialize();
            int index = 0;
            do
            {
                Application.DoEvents();
                System.Threading.Thread.Sleep(1000);
                if (index > 5)
                    break;
                index++;
            } while ((m_currentStatus != EVOAPILib.SC_Status.STATUS_INITIALIZED) && (m_currentStatus != (EVOAPILib.SC_Status.STATUS_INITIALIZED | EVOAPILib.SC_Status.STATUS_IDLE)) && (m_currentStatus != (EVOAPILib.SC_Status.STATUS_INITIALIZED | EVOAPILib.SC_Status.STATUS_BUSY)));


            m_evoSys.SetRemoteMode(1);//1: on; 0:off, 

            if (lampStatus == 0)
            {
                m_evoSys.SetLamp(EVOAPILib.SC_LampStatus.LAMP_GREEN);
            }
            else if (lampStatus == 1)
            {
                m_evoSys.SetLamp(EVOAPILib.SC_LampStatus.LAMP_REDFLASHING);
            }
            else if (lampStatus == 2)
            {
                m_evoSys.SetLamp(EVOAPILib.SC_LampStatus.LAMP_GREENFLASHING);
            }

            m_evoSys.SetRemoteMode(0);
        }

        public override EVO_DoorLockStatus CheckDoorLockStatus()
        {
            //CheckEvoSystemClass();


            //DateTime startTime = DateTime.Now;

            //m_evoSys.Initialize();
            //do
            //{
            //    Application.DoEvents();
            //    System.Threading.Thread.Sleep(1000);
            //} while ((m_currentStatus != EVOAPILib.SC_Status.STATUS_INITIALIZED) && (m_currentStatus != (EVOAPILib.SC_Status.STATUS_INITIALIZED | EVOAPILib.SC_Status.STATUS_IDLE)));

            //m_evoSys.ExecuteScriptCommand(WanTai.Common.Configuration.GetCheckDoorLockStatusCommand());

            //do
            //{
            //    Application.DoEvents();
            //    System.Threading.Thread.Sleep(1000);
            //} while (m_evoSys.GetScriptStatus(iScriptID) == EVOAPILib.SC_ScriptStatus.SS_BUSY);

            ////wait a short time, then command result from log file
            //System.Threading.Thread.Sleep(2000);

            //int doorLockStatus = getDoorLockStatusFromLog(startTime);
            //return (EVO_DoorLockStatus)doorLockStatus;
            return EVO_DoorLockStatus.OFF;
        }

        private int getDoorLockStatusFromLog(DateTime startTime)
        {
            int doorLockStatus = 0;
            DateTime maxTime = startTime;
            string filePatten = "EVO_" + DateTime.Now.ToString("yyyyMMdd") + "_*.LOG";
            string[] filenames = System.IO.Directory.GetFiles(WanTai.Common.Configuration.GetEvoWareLogPath(), filePatten);
            string lastLogFileName = null;
            if (filenames.Length > 0)
            {
                foreach (string fileName in filenames)
                {
                    System.IO.FileInfo fileInfo = new System.IO.FileInfo(fileName);
                    if (fileInfo.LastWriteTime > maxTime)
                    {
                        lastLogFileName = fileName;
                        maxTime = fileInfo.LastWriteTime;
                    }
                }
            }

            if (lastLogFileName != null)
            {
                using (System.IO.FileStream stream = new System.IO.FileStream(lastLogFileName, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite))
                {
                    using (System.IO.StreamReader source = new System.IO.StreamReader(stream))
                    {
                        while (!source.EndOfStream)
                        {
                            string lineStr = source.ReadLine();

                            if (lineStr.Contains("ExecuteScriptCommand;" + WanTai.Common.Configuration.GetCheckDoorLockStatusCommand()))
                            {
                                int i = 0;
                                while (i < 2)
                                {
                                    if (!source.EndOfStream)
                                    {
                                        lineStr = source.ReadLine();
                                    }

                                    i++;
                                }

                                if (lineStr.Contains("- O2,0,"))
                                {
                                    string lastStr = lineStr.Substring(lineStr.Length - 1);
                                    int _doorLockStatus = 0;
                                    if (int.TryParse(lastStr, out _doorLockStatus))
                                    {
                                        doorLockStatus = int.Parse(lastStr);
                                    }
                                }
                            }
                        }

                        source.Close();
                    }

                    stream.Close();
                }
            }

            return doorLockStatus;
        }

        public override bool AboutKingFisher()
        {
            Thermo.ThermoProcessor thermoProcessor = new Thermo.ThermoProcessor();
            return thermoProcessor.AboutKingFisher();
        }

        public override bool isEVOOffline()
        {
            return EVOOffline;
        }
    }

    public enum EVO_ScriptStatus
    {
        UNKNOWN = 0,
        IDLE = 1,
        BUSY = 2,
        ABORTED = 3,
        STOPPED = 4,
        PIPETTING = 5,
        PAUSED = 6,
        ERROR = 7,
        SIMULATION = 8,
        STATUS_ERROR = 9,
        NONE = 10,
    }

    public enum EVO_DoorLockStatus
    {
        OFF = 0,
        ON = 1
    }
}
