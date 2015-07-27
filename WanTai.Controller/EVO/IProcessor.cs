using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WanTai.Controller.EVO
{
    public abstract class IProcessor
    {
        public delegate void OnEvoError();
        public virtual event OnEvoError onReceiveError;
        public virtual event OnEvoError onSendErrorResponse;
        public virtual event OnEvoError onOfflineStatus;

        public abstract bool StartScript(string sScriptName);
        public abstract bool StartScript(string sScriptName, Dictionary<string, string> lVariables);
        public abstract void StopScript();
        public abstract bool PauseScript();
        public abstract bool ResumeScript();
        public abstract EVO_ScriptStatus GetScriptStatus();
        public abstract void Close();
        public abstract void CloseLamp();
        public abstract bool RecoverScript(string sScriptName, short startLineNumber);
        public abstract bool RecoverScript(string sScriptName, Dictionary<string, string> lVariables, short startLineNumber);
        public abstract bool CheckCanRecover(string sScriptName, out short last_error_line);
        public abstract EVO_DoorLockStatus CheckDoorLockStatus();
        public abstract bool AboutKingFisher();
        public abstract bool isEVOOffline();        
        public abstract void SetLampStatus(int lampStatus);

        #region 2012-1-5 perry 提取运行时开始下一轮次扫描
        public delegate void OnNextTurnStep(string ScriptName);
        public virtual event OnNextTurnStep onOnNextTurnStepHandler;
        public abstract bool isOnNextTurnStep();
        public abstract void OnNextTurnStepDispse();
        #endregion
    }
}
