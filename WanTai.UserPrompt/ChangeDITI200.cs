using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Configuration;
using System.IO;

namespace WanTai.UserPrompt
{
    public partial class ChangeDITI200 : Form
    {
        public ChangeDITI200()
        {            
            InitializeComponent();
            CommonFunction.WriteLog("ChangeDITI1000---InitializeComponent");
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            string diti200file = ConfigurationManager.AppSettings["EvoVariableOutputPath"] + ConfigurationManager.AppSettings["Diti200File"];
            if (!string.IsNullOrEmpty(diti200file))
            {
                using (FileStream fileStream = new System.IO.FileStream(diti200file, System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.ReadWrite, System.IO.FileShare.ReadWrite))
                {
                    using (System.IO.StreamWriter mysr = new System.IO.StreamWriter(fileStream))
                    {
                        mysr.WriteLine("1");
                    }
                }
            }

            this.Close();
        }        
    }
}
