using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Globalization;
using MibAi.Plugins;
using MibAi;

namespace WanTai.Controller.Thermo
{
    public class ThermoProcessor
    { 
        public bool AboutKingFisher()
        {            
            Assembly assembly = Assembly.LoadFile(WanTai.Common.Configuration.GetThermoPluginPath() + @"Plugins\MibAiPlugin.dll");
            PluginInterface AI = (PluginInterface)assembly.CreateInstance("MibAi.Plugins.BindItPlugin", true, BindingFlags.CreateInstance, null, null, CultureInfo.CurrentCulture, null);
            string userName = WanTai.Common.Configuration.GetThermoUsername();
            string password = WanTai.Common.Configuration.GetThermoPassword();
            bool result = AI.Login(userName, password);
            if (!result)
            {
                throw new Exception("Login KingFisher failed");
            }

            result = AI.Connect(WanTai.Common.Configuration.GetThermoInstrumentName());
            //if (!result)
            //{
            //    throw new Exception("Connect to KingFisher failed");
            //}

            int state = 0;
            int error = 0;
            int currentStep = 0;
            string currentSessionName = "";
            result = AI.GetAIServerStatus(ref state, ref error, ref currentSessionName, ref currentStep);
            if ((EngineState)state == EngineState.Executing)
            {
                result = AI.Abort();
                if (!result)
                {
                    return false;
                }
            }

            result = AI.Disconnect();
            if (!result)
            {
                throw new Exception("Disconnect from KingFisher failed");
            }

            return true;
        }
    }
}
