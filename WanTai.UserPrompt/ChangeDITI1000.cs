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
    public partial class ChangeDITI1000 : Form
    {
        public ChangeDITI1000()
        {            
            InitializeComponent();
            CommonFunction.WriteLog("ChangeDITI1000---InitializeComponent");
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            string diti1000file = ConfigurationManager.AppSettings["EvoVariableOutputPath"] + ConfigurationManager.AppSettings["Diti1000File"];
            if (!string.IsNullOrEmpty(diti1000file))
            {
                using (FileStream fileStream = new System.IO.FileStream(diti1000file, System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.ReadWrite, System.IO.FileShare.ReadWrite))
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
