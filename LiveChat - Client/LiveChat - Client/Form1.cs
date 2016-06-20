using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace LiveChat___Client
{
    public partial class Form1 : Form
    {

        TcpClient clientSocket;
        NetworkStream serverStream = default(NetworkStream);
        string readData = null;

        string nickname;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            textBoxMessage.Focus();
        }

        private void buttonConnect_Click(object sender, EventArgs e)
        {
            readData = "Trying to connect...";
            WriteMessage();

            if (Connect(textBoxIP.Text, int.Parse(textBoxPort.Text)))
            {
                readData = "Connection successful, now chatting.";
                WriteMessage();           
            }
            else
            {
                readData = "Connection failed.";
                WriteMessage();
            }
        }

        private void buttonSend_Click(object sender, EventArgs e)
        {
            if (IsConnected())
            {
                SendMessage(textBoxMessage.Text);
                textBoxMessage.Text = "";
            }
        }

        private void buttonDisconnect_Click(object sender, EventArgs e)
        {
            if (IsConnected())
                Disconnect();
        }


        private void SendMessage(string message)
        {
            if (IsConnected())
            {
                byte[] outStream = System.Text.Encoding.UTF8.GetBytes(message + "$");
                serverStream.Write(outStream, 0, outStream.Length);
                serverStream.Flush();
            }
        }

        private void GetMessage()
        {
            while (IsConnected())
            {
                try
                {
                    serverStream = clientSocket.GetStream();
                    byte[] inStream = new byte[clientSocket.ReceiveBufferSize];
                    serverStream.Read(inStream, 0, inStream.Length);
                    string returndata = System.Text.Encoding.UTF8.GetString(inStream);
                    readData = returndata;
                    WriteMessage();
                }
                catch (Exception ex)
                {
                    //readData = ex.ToString();
                    //WriteMessage();
                    break;
                }
            }

            Disconnect();
        }

        private void Disconnect()
        {
            serverStream.Close();
            clientSocket.Close();

            readData = "Disconnected from server.";
            WriteMessage();
        }

        private void WriteMessage()
        {
            if (this.InvokeRequired)
                this.Invoke(new MethodInvoker(WriteMessage));
            else
                textBoxChat.Text += Environment.NewLine + " >> " + readData;
        }

        private bool Connect(string ip, int port)
        {
            try
            {
                nickname = textBoxNick.Text;

                clientSocket = new TcpClient();
                clientSocket.Connect(ip, port);
                serverStream = clientSocket.GetStream();

                byte[] outStream = System.Text.Encoding.ASCII.GetBytes(textBoxNick.Text + "$");
                serverStream.Write(outStream, 0, outStream.Length);
                serverStream.Flush();

                //if connection was denied by server.
                byte[] inStream = new byte[clientSocket.ReceiveBufferSize];
                serverStream.Read(inStream, 0, inStream.Length);
                string s = Encoding.ASCII.GetString(inStream);

                readData = s;
                WriteMessage();

                if (s.Substring(0, 7) == "Denied;")
                {
                    return false;
                }

                Thread ctThread = new Thread(GetMessage);
                ctThread.Start();
                return true;
            }
            catch (Exception ex)
            {
                readData = ex.ToString();
                WriteMessage();
                return false;
            }
        }

        private bool IsConnected()
        {
            try
            {
                if (clientSocket != null && clientSocket.Client != null && clientSocket.Client.Connected)
                {
                    if (clientSocket.Client.Poll(0, SelectMode.SelectRead))
                    {
                        byte[] buff = new byte[1];
                        if (clientSocket.Client.Receive(buff, SocketFlags.Peek) == 0)
                        {
                            return false;
                        }
                        else
                        {
                            return true;
                        }

                    }
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
