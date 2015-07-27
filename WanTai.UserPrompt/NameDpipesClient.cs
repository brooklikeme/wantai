using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using System.IO;
using System.Net;
namespace WanTai.UserPrompt
{
    class NameDpipesClient
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern SafeFileHandle CreateFile(String pipeName, uint dwDesiredAccess, uint dwShareMode, IntPtr lpSecurityAttributes, uint dwCreationDisposition, uint dwFlagsAndAttributes, IntPtr hTemplate);
        private const uint GENERIC_READ = (0x80000000);
        private const uint GENERIC_WRITE = (0x40000000);
        private const uint OPEN_EXISTING = 3;
        private const uint FILE_FLAG_OVERLAPPED = (0x40000000);
        public delegate void MessageReceivedHandler(string message);
        public event MessageReceivedHandler MessageReceived;
        private const int BUFFER_SIZE = 4096;
        private string pipeName { get { return "\\\\.\\pipe\\"+  Dns.GetHostName();} }
        private FileStream stream;
        private SafeFileHandle handle;
        public bool Connected
        {
            get;
            set;
        }
        public void Connect()
        {
            Connected = false;
            DateTime dtime = DateTime.Now;
        connect:
            if (DateTime.Now.Subtract(dtime).TotalMinutes > 2) return;
        CommonFunction.WriteLog("CreateFile=>" + this.pipeName);
            this.handle = CreateFile(this.pipeName, GENERIC_READ | GENERIC_WRITE, 0, IntPtr.Zero, OPEN_EXISTING, FILE_FLAG_OVERLAPPED, IntPtr.Zero);
            if (this.handle.IsInvalid)
                goto connect;
            CommonFunction.WriteLog("CreateFile=>success");
            this.stream = new FileStream(this.handle, FileAccess.ReadWrite, BUFFER_SIZE, true);
            Thread thred = new Thread(new ThreadStart(new ThreadStart(delegate
            {
                Connected = true;
                ASCIIEncoding encoder = new ASCIIEncoding();
                string ReadMessage = string.Empty;
                while (Connected)
                {
                    Thread.Sleep(100);
                    int bytesRead = 0;
                    byte[] readBuffer = new byte[BUFFER_SIZE];
                    try
                    {
                        if (!Connected || stream == null || handle == null || handle.IsClosed || handle.IsInvalid)
                        {
                            CommonFunction.WriteLog("!Connected || stream == null || handle == null || handle.IsClosed || handle.IsInvalid");
                            break;
                        }
                        bytesRead = this.stream.Read(readBuffer, 0, BUFFER_SIZE);
                      //  this.stream.Flush();
                    }
                    catch(Exception ex)
                    {
                        CommonFunction.WriteLog("Exception=>" + ex.ToString());
                        break;
                    }
                    if (bytesRead == 0)
                        continue;
                    CommonFunction.WriteLog("bytesRead=>" + bytesRead.ToString());
                    CommonFunction.WriteLog("Read -- Client----  while (Connected)");
                    string Message = encoder.GetString(readBuffer, 0, bytesRead);
                    if (ReadMessage == Message) continue;
                    ReadMessage = Message;
                    CommonFunction.WriteLog("Message=>" + Message.ToString());
                    if (Message.IndexOf("exit")>=0)
                        break;


                    if (this.MessageReceived != null)
                        this.MessageReceived(Message);
                }
                Dispose();
            })));
            thred.Start();
        }
        public void SendMessage(string Message)
        {
            if (Connected && stream != null && handle != null)
            {
                UTF8Encoding encoder = new UTF8Encoding();//可以传汉字
                byte[] messageBuffer = encoder.GetBytes(Message);
                this.stream.Write(messageBuffer, 0, messageBuffer.Length);
                this.stream.Flush();
            }
        }
        public void Dispose()
        {
            SendMessage("exit");
            Thread.Sleep(100);
            Connected = false;
            if (this.stream != null)
            {
                this.stream.Close();
                this.stream.Dispose();
            }
            if (this.handle != null)
            {
                this.handle.Close();
                this.handle.Dispose();
            }
        }      
    }
}
