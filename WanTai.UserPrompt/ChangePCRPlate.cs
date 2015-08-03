using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WanTai.UserPrompt
{
    public partial class ChangePCRPlate : Form
    {
        public ChangePCRPlate(string testingItem)
        {            
            InitializeComponent();
            CommonFunction.WriteLog("ChangePCRPlate---InitializeComponent");
            this.textPrompt.Text = string.Format(this.textPrompt.Text, testingItem);

           // ChangePCRPlate
           // pictureBox1.Image = "";
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            this.Close();
        }       
        
    }
}
