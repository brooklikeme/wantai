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
    public partial class FrmNextTurnStep : Form
    {
        public FrmNextTurnStep(string toMessage)
        {
            InitializeComponent();
            CommonFunction.WriteLog("FrmNextTurnStep_Shown---InitializeComponent");
            this.toMessage = toMessage;
        }
        private string toMessage { get; set; }
        private NameDpipesClient client;
        
        private delegate void DisplayMessageDelegate(string message);
        private void DisplayMessage(string message)
        {
            if (!this.InvokeRequired)
            {
                this.rboxRecivedMessage.Text += message;
                if (message.IndexOf("NextStepRun") >= 0)
                    client.SendMessage("ServerClose");
                if (message.IndexOf("WaitForSecondMix") >= 0)
                    client.SendMessage("NotifyForSecondMix");
                if (message.IndexOf("NextStepScan") >= 0 || message.IndexOf("NextStepRun") >= 0 || message.IndexOf("WaitForSecondMix") >= 0)
                {
                    System.Threading.Thread.Sleep(100 );
                    this.Close();
                }
            }
            else
                this.Invoke(new DisplayMessageDelegate(DisplayMessage), message);
               
        }
        private void button1_Click(object sender, EventArgs e)
        {
            client.SendMessage(textBox1.Text);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            client.Dispose();
        }

        private void FrmNextTurnStep_Shown(object sender, EventArgs e)
        {
            CommonFunction.WriteLog("FrmNextTurnStep_Shown");
            this.Visible = false;
            client = new NameDpipesClient();
            client.MessageReceived += new NameDpipesClient.MessageReceivedHandler(DisplayMessage);
            client.Connect();
            
            System.Threading.Thread.Sleep(100);
            if (client.Connected)
            {
                CommonFunction.WriteLog("FrmNextTurnStep_Shown   client.Connected    toMessage====>" + toMessage);
                client.SendMessage(toMessage);
            }
        }
    }
}
