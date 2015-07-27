using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
namespace WanTai.Controller.EVO
{
    public class MockProcessor : IProcessor
    {
        private static MockProcessor instance;

        public override event OnEvoError onReceiveError;
        public override event OnEvoError onSendErrorResponse;
        public override event OnNextTurnStep onOnNextTurnStepHandler;
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
            onOnNextTurnStepHandler =null;
        }
        #endregion
        protected MockProcessor()
        {                       
        }
      
        private static int State = 0;
        private static int SleepTime = 10;
        private static bool ReturnFalg = false;
        public static MockProcessor Instance()
        {
            if (instance == null)
                instance = new MockProcessor();

            return instance;
        }

        public override bool StartScript(string sScriptName)
        {
            if (onOnNextTurnStepHandler != null)
                onOnNextTurnStepHandler(sScriptName);
            ReturnFalg = false;
            System.Windows.Forms.Application.DoEvents();
            SleepTime = 10;
            State = 0;
            string pcrStartFilePath = WanTai.Common.Configuration.GetEvoOutputPath() + "PCRStart.csv";
            string pcrFinishedFilePath = WanTai.Common.Configuration.GetEvoOutputPath() + "PCRFinished.csv";
            string HunYangStartFilePath = WanTai.Common.Configuration.GetEvoOutputPath() + "HunYangStart.csv";
            string hunYangFinishedFilePath = WanTai.Common.Configuration.GetEvoOutputPath() + "HunYangFinished.csv";
            if (File.Exists(pcrStartFilePath))
                File.Delete(pcrStartFilePath);
            if (File.Exists(pcrFinishedFilePath))
                File.Delete(pcrFinishedFilePath);

            if (File.Exists(HunYangStartFilePath))
                File.Delete(HunYangStartFilePath);
            if (File.Exists(hunYangFinishedFilePath))
                File.Delete(hunYangFinishedFilePath);
            if (sScriptName != "Test_1_AddBuffer_Extraction_PCRMix.esc" && sScriptName != "TEST_2_AddLiquidToMIX.esc")
            {
                while (SleepTime > 0)
                {
                    if (ReturnFalg) return false;
                    SleepTime--;
                    Thread.Sleep(1000);
                    System.Windows.Forms.Application.DoEvents();
                }
                if (sScriptName == "TEST_3_TIQUANDMIX.esc" || sScriptName == "TEST_3_TIQUANDMIX_NoPCRLiquid.esc")
                {
                    Thread.Sleep(1000*60);
                    string EvoVariableOutputPath = WanTai.Common.Configuration.GetEvoVariableOutputPath();
                    bool hasNext = false;
                    using (FileStream DWFile = new FileStream(EvoVariableOutputPath + "Times.CSV", FileMode.Open, FileAccess.Read))
                    {
                        using (StreamReader DWStreamReader = new StreamReader(DWFile, Encoding.Default))
                        {
                            string value = string.Empty;
                            while (!DWStreamReader.EndOfStream)
                            {
                                value = DWStreamReader.ReadLine();
                            }

                            if (int.Parse(value) == 1)
                            {
                                hasNext = true;
                            }
                        }
                    }
                    if (hasNext)
                    {
                        WanTai.UserPrompt.Program.Main(new string[] { "GoToScanPage" });
                        //System.Diagnostics.Process.Start("WanTai.UserPrompt.exe", "GoToScanPage");
                        Thread.Sleep(1000 * 60 * 1);
                        WanTai.UserPrompt.Program.Main(new string[] { "ScanFinished" });
                        //System.Diagnostics.Process.Start("WanTai.UserPrompt.exe", "ScanFinished");
                    }
                    //Thread.Sleep(1000 * 30 * 1);
                    SleepTime = 10;
                    State = 2;
                    while (SleepTime > 0)
                    {
                        if (ReturnFalg) return false;
                        SleepTime--;
                        Thread.Sleep(1000);
                        System.Windows.Forms.Application.DoEvents();
                    }
                    if (ReturnFalg) return false;
                    using (FileStream DWFile = new FileStream(HunYangStartFilePath, FileMode.Create, FileAccess.Write))
                    {
                        using (StreamWriter DWStreamWriter = new StreamWriter(DWFile, Encoding.Default))
                        {
                            DWStreamWriter.WriteLine("混样");
                        }
                    }

                    SleepTime = 30;
                    State = 2;
                    while (SleepTime > 0)
                    {
                        if (ReturnFalg) return false;
                        SleepTime--;
                        Thread.Sleep(1000);
                        System.Windows.Forms.Application.DoEvents();
                    }

                    using (FileStream DWFile = new FileStream(hunYangFinishedFilePath, FileMode.Create, FileAccess.Write))
                    {
                        using (StreamWriter DWStreamWriter = new StreamWriter(DWFile, Encoding.Default))
                        {
                            DWStreamWriter.WriteLine("混样");
                        }
                    }

                    using (FileStream DWFile = new FileStream(pcrStartFilePath, FileMode.Create, FileAccess.Write))
                    {
                        using (StreamWriter DWStreamWriter = new StreamWriter(DWFile, Encoding.Default))
                        {
                            DWStreamWriter.WriteLine("PCR配液");
                        }
                    }

                    SleepTime = 20;
                    State = 1;
                    while (SleepTime > 0)
                    {
                        if (ReturnFalg) return false;
                        SleepTime--;
                        Thread.Sleep(1000);
                        System.Windows.Forms.Application.DoEvents();
                    }
                    if (ReturnFalg) return false;
                    using (FileStream DWFile = new FileStream(pcrFinishedFilePath, FileMode.Create, FileAccess.Write))
                    {
                        using (StreamWriter DWStreamWriter = new StreamWriter(DWFile, Encoding.Default))
                        {
                            DWStreamWriter.WriteLine("PCR配液");
                        }
                    }
                    
                    Thread.Sleep(1000*30*1);
                }
                return true;
            }

            
            
            SleepTime = 10;
            State = 3;
            while (SleepTime > 0)
            {
                if (ReturnFalg) return false;
                SleepTime--;
                Thread.Sleep(1000);
                System.Windows.Forms.Application.DoEvents();
            }
            if (ReturnFalg) return false;
            //using (FileStream DWFile = new FileStream("output//TiQu.csv", FileMode.OpenOrCreate, FileAccess.Write))
            //{
            //    using (StreamWriter DWStreamWriter = new StreamWriter(DWFile, Encoding.Default))
            //    {
            //        DWStreamWriter.WriteLine("提取");
            //        DWStreamWriter.Close();
            //    }
            //    DWFile.Close();
            //}
            
            ReturnFalg = false;
            return true;
        }

        public override bool StartScript(string sScriptName, Dictionary<string, string> lVariables)
        {
            return StartScript(sScriptName);
        }

        public override void StopScript()
        {
            ReturnFalg = true;
            Thread.Sleep(1000 * 10 * 1);
        }

        public override bool PauseScript()
        {
            ReturnFalg = true;
            Thread.Sleep(1000 * 2 * 1);
            return true;
        }

        public override bool ResumeScript()
        {
            ReturnFalg = false;

            if (File.Exists("output//PCR.csv"))
                File.Delete("output//PCR.csv");
            if (File.Exists("output//HunYang.csv"))
                File.Delete("output//HunYang.csv");
            if (File.Exists("output//TiQu.csv"))
                File.Delete("output//TiQu.csv");
            while (SleepTime > 0)
            {
               if (ReturnFalg) return false;
                SleepTime--;
                Thread.Sleep(1000);
            }
            if (ReturnFalg) return false;
            using (FileStream DWFile = new FileStream("output//PCR.csv", FileMode.OpenOrCreate, FileAccess.Write))
            {
                using (StreamWriter DWStreamWriter = new StreamWriter(DWFile, Encoding.Default))
                {
                    DWStreamWriter.WriteLine("PCR配液");
                    DWStreamWriter.Close();
                }
                DWFile.Close();
            }

            
            SleepTime = 10;
            State = 2;
            while (SleepTime > 0)
            {
                if (ReturnFalg) return false;
                SleepTime--;
                Thread.Sleep(1000);
            }
            if (ReturnFalg) return false;
            using (FileStream DWFile = new FileStream("output//HunYang.csv", FileMode.OpenOrCreate, FileAccess.Write))
            {
                using (StreamWriter DWStreamWriter = new StreamWriter(DWFile, Encoding.Default))
                {
                    DWStreamWriter.WriteLine("混样");
                    DWStreamWriter.Close();
                }
                DWFile.Close();
            }


            SleepTime = 10;
            State = 3;
            while (SleepTime > 0)
            {
                if (ReturnFalg) return false;
                SleepTime--;
                Thread.Sleep(1000);
            }
            if (ReturnFalg) return false;
            using (FileStream DWFile = new FileStream("output//TiQu.csv", FileMode.OpenOrCreate, FileAccess.Write))
            {
                using (StreamWriter DWStreamWriter = new StreamWriter(DWFile, Encoding.Default))
                {
                    DWStreamWriter.WriteLine("提取");
                    DWStreamWriter.Close();
                }
                DWFile.Close();
            }
            ReturnFalg = false;
            return true;
        }

        public override EVO_ScriptStatus GetScriptStatus()
        {
            return EVO_ScriptStatus.IDLE;
        }

        public override void Close()
        {
        }

        public override void CloseLamp()
        {
        }

        public override bool RecoverScript(string sScriptName, short startLineNumber)
        {
            return true;
        }

        public override bool RecoverScript(string sScriptName, Dictionary<string, string> lVariables, short startLineNumber)
        {
            return true;
        }

        public override bool CheckCanRecover(string sScriptName, out short last_error_line)
        {
            last_error_line = 0;
            return false;
        }

        public override EVO_DoorLockStatus CheckDoorLockStatus()
        {
            //Random random = new Random();
            //int number = random.Next(0, 10);
            //if (number % 2 == 0)
            //{
            //    return EVO_DoorLockStatus.OFF;
            //}
            //else
            //{
            //    return EVO_DoorLockStatus.ON;
            //}

            return EVO_DoorLockStatus.OFF;
        }

        public override bool AboutKingFisher()
        {
            Thread.Sleep(2000);
            return true;
        }

        public override bool isEVOOffline()
        {
            return false;
        }

        public override void SetLampStatus(int lampStatus)
        {
            
        }
    }
}
