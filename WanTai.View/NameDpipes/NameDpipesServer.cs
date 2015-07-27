using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;
using System.IO;
using Microsoft.Win32.SafeHandles;
using System.Net;
namespace WanTai.View
{
    public class NameDpipesServer
    {
        public NameDpipesServer() { }
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern SafeFileHandle CreateNamedPipe(String pipeName, uint dwOpenMode, uint dwPipeMode, uint nMaxInstances, uint nOutBufferSize, uint nInBufferSize, uint nDefaultTimeOut, IntPtr lpSecurityAttributes);
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern int ConnectNamedPipe(SafeFileHandle hNamedPipe, IntPtr lpOverlapped);
        private const uint DUPLEX = (0x00000003);
        private const uint FILE_FLAG_OVERLAPPED = (0x40000000);
        public delegate void MessageReceivedHandler(string message);
        public event MessageReceivedHandler MessageReceived;
        private const int BUFFER_SIZE = 4096;
        private string pipeName { get { return "\\\\.\\pipe\\"+  Dns.GetHostName();} }
        public struct Client
        {
            public SafeFileHandle clientHandle;
            public FileStream stream;
            public bool Connected;
        }
        public  Boolean Runing123 { get; set; }
        private List<Client> clients;
        private Thread thread;
        public void start()
        {
            clients = new List<Client>();
            Runing123 = true;
            thread = new Thread(new ThreadStart(delegate {
                //while (Runing123)
                //{
                    DateTime dtime = DateTime.Now;
                strat:
                    if(DateTime.Now.Subtract(dtime).TotalMinutes < 5)
                    {
                        Thread.Sleep(1000);
                        Client client = new Client();
                        WanTai.Common.CommonFunction.WriteLog("CreateNamedPipe=====>" + this.pipeName);
                        client.clientHandle = CreateNamedPipe(this.pipeName, DUPLEX | FILE_FLAG_OVERLAPPED, 0, 255, BUFFER_SIZE, BUFFER_SIZE, 0, IntPtr.Zero);
                        if ((client.clientHandle == null || client.clientHandle.IsClosed || client.clientHandle.IsInvalid) && Runing123)
                            goto strat;
                        int success = ConnectNamedPipe(client.clientHandle, IntPtr.Zero);
                        if (success == 0 && Runing123)
                            goto strat;
                        WanTai.Common.CommonFunction.WriteLog("CreateNamedPipe=====>success");
                        if (!Runing123)
                        {
                            if (client.clientHandle != null)
                            {
                                client.clientHandle.SetHandleAsInvalid();
                                client.clientHandle.Close();
                                client.clientHandle.Dispose();
                                client.clientHandle = null;
                            }
                           // break;
                        }
                        client.Connected = true;
                        client.stream = new FileStream(client.clientHandle, FileAccess.ReadWrite, BUFFER_SIZE, true);
                        lock (this.clients)
                            clients.Add(client);
                        Thread readThread = new Thread(new ParameterizedThreadStart(Read));
                        readThread.IsBackground = true;
                        readThread.Start(client);
                    }
                //}
            }));
            thread.IsBackground = true;
            thread.Start();
        }
        public void Dispose()
        {
            Runing123 = false;
        
         //   thread.Abort();
            if (clients != null)
            {
                lock (this.clients)
                {
                    for (int i = 0; i < clients.Count; i++)
                    {
                        Client client = clients[i];
                        client.Connected = false;
                        SendMessage("exit");
                        Thread.Sleep(100);
                        if (client.stream != null)
                        {
                            client.stream.Close();
                            client.stream.Dispose();
                            client.stream = null;
                        }
                        if (client.clientHandle != null)
                        {
                        
                            client.clientHandle.SetHandleAsInvalid();
                            client.clientHandle.Close();
                            client.clientHandle.Dispose();
                            client.clientHandle = null;
                        }
                    }
                    clients = new List<Client>();
                }
            }
        }
        private void Read(object clientObj)
        {
            Client client = (Client)clientObj;
            //ASCIIEncoding encoder = new ASCIIEncoding();
            UTF8Encoding encoder = new UTF8Encoding();//可以传汉字
            string ReadMessage = string.Empty;
            while (client.Connected)
            {
                Thread.Sleep(100);
                int bytesRead = 0;
                byte[] buffer = new byte[BUFFER_SIZE];
                try
                {
                    if (!client.Connected || client.stream == null || client.clientHandle == null || client.clientHandle.IsClosed || client.clientHandle.IsInvalid)
                    {
                        WanTai.Common.CommonFunction.WriteLog("!client.Connected || client.stream == null || client.clientHandle == null || client.clientHandle.IsClosed || client.clientHandle.IsInvalid");
                        break;
                    }
                    bytesRead = client.stream.Read(buffer, 0, BUFFER_SIZE);
                   // client.stream.Flush();
                }
                catch(Exception ex)
                {
                    WanTai.Common.CommonFunction.WriteLog("Exception ----> " + ex.ToString());
                    break;
                }

                if (bytesRead == 0)
                    continue;
                string Message = encoder.GetString(buffer, 0, bytesRead);
                WanTai.Common.CommonFunction.WriteLog("Read---- while (client.Connected)");
                WanTai.Common.CommonFunction.WriteLog("bytesRead ----> " + bytesRead.ToString());
                WanTai.Common.CommonFunction.WriteLog("Message ----> " + Message);
                if (Message.IndexOf("exit")>=0)
                    break;
                if (ReadMessage == Message) continue;
                ReadMessage = Message;
                if (this.MessageReceived != null)
                    this.MessageReceived( Message);                
            }
            lock (this.clients)
                this.clients.Remove(client);
            client.Connected = false;
            if (client.stream != null)
            {                
                client.stream.Close();
                client.stream.Dispose();
                client.stream = null;
            }
            if (client.clientHandle != null)
            {
                client.clientHandle.SetHandleAsInvalid();
                client.clientHandle.Close();
                client.clientHandle.Dispose();
                client.clientHandle = null;
            }
            
        }

        public void SendMessage(string message)
        {
            //ASCIIEncoding encoder = new ASCIIEncoding();
            UTF8Encoding encoder = new UTF8Encoding();
            lock (this.clients)
            {
                for (int i = 0; i < clients.Count; i++)
                {
                    Client client = clients[i];
                    byte[] messageBuffer = encoder.GetBytes(message);
                    if (!client.Connected || client.stream == null || client.clientHandle == null || client.clientHandle.IsClosed)
                    {
                        client.Connected = false;
                        if (client.stream != null)
                        {
                            client.stream.Close();
                            client.stream.Dispose();
                            client.stream = null;
                        }
                        if (client.clientHandle != null)
                        {
                            client.clientHandle.SetHandleAsInvalid();
                            client.clientHandle.Close();
                            client.clientHandle.Dispose();
                            client.clientHandle = null;
                        }
                        WanTai.Common.CommonFunction.WriteLog(" client.stream.Write====>" + i.ToString());
                        continue;
                    }
                    client.stream.Write(messageBuffer, 0, messageBuffer.Length);
                    client.stream.Flush();
                    WanTai.Common.CommonFunction.WriteLog(" client.stream.Write====>" + message);
                }
            }
        }
    }
}
