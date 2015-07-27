using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace WanTai.UserPrompt
{
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main(string[] arg)
        {
            Application.EnableVisualStyles();

       //     System.Diagnostics.Process.Start(Application.StartupPath + @"\aab.vbs","aa");
  
            //Application.SetCompatibleTextRenderingDefault(false);
            CommonFunction.WriteLog(arg[0]);
            if (arg.Length > 0 && (arg[0] == "GoToScanPage" || arg[0] == "ScanFinished"))
            {
                Application.Run(new FrmNextTurnStep(arg[0]));
            }
            else if(arg.Length > 0 && (arg[0] == "HBV" || arg[0] == "HCV" || arg[0] == "HIV"))
            {
                Application.Run(new ChangePCRPlate(arg[0]));
            }
            else if (arg.Length > 0 && (arg[0] == "1000DITI"))
            {
                Application.Run(new ChangeDITI1000());
            }
            else if (arg.Length > 0 && (arg[0] == "200DITI"))
            {
                Application.Run(new ChangeDITI200());
            }
            else if (arg.Length > 0 && arg[0].StartsWith("wash"))
            {
                Application.Run(new ChangeWash(arg[0]));
            }
        }
    }
}
