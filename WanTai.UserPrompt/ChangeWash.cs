using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Configuration;

namespace WanTai.UserPrompt
{
    public partial class ChangeWash : Form
    {
        public ChangeWash(string wash)
        {            
            InitializeComponent();
            CommonFunction.WriteLog("ChangeWash---InitializeComponent");
            string _washName = ConfigurationManager.AppSettings[wash];

            this.textPrompt.Text = this.textPrompt.Text + _washName;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            this.Close();
        }       
        
    }
}
