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
    public partial class ChangePCRPlate : Form
    {
        public ChangePCRPlate(string testingItem)
        {            
            InitializeComponent();
            CommonFunction.WriteLog("ChangePCRPlate---InitializeComponent");

            string workDeskType = ConfigurationManager.AppSettings["WorkDeskType"];

            this.textPrompt.Text = string.Format(this.textPrompt.Text, testingItem);

           // ChangePCRPlate
            if (workDeskType == "100")
            {
                this.pictureBox1.Image = Properties.Resources.PCRPlate_100;
                this.pictureBox1.Left = 200;
            }
            else if (workDeskType == "150")
            {
                this.pictureBox1.Image = Properties.Resources.PCRPlate_150;
                this.pictureBox1.Left = 100;
            }
            else if (workDeskType == "200")
            {
                this.pictureBox1.Image = Properties.Resources.PCRPlate_200;
                
            }

        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            this.Close();
        }     
        
    }
}
