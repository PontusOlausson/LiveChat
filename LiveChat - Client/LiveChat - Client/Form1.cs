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

        TcpClient clientSocket = new TcpClient();
        NetworkStream serverStream = default(NetworkStream);
        string readData = null;

        string nickname, ip;
        int port;

        public Form1()
        {
            InitializeComponent();
        }

        private void buttonConnect_Click(object sender, EventArgs e)
        {
            readData = "Trying to connect...";
            WriteMessage();

            if (Connect())
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
            SendMessage(textBoxMessage.Text);
        }

        private void buttonDisconnect_Click(object sender, EventArgs e)
        {

        }


        private void SendMessage(string message)
        {
            byte[] outStream = System.Text.Encoding.ASCII.GetBytes(message + "$");
            serverStream.Write(outStream, 0, outStream.Length);
            serverStream.Flush();
        }

        private void GetMessage()
        {
            while (true)
            {
                serverStream = clientSocket.GetStream();
                int buffSize = 0;
                byte[] inStream = new byte[10025];
                buffSize = clientSocket.ReceiveBufferSize;
                //serverStream.Read(inStream, 0, buffSize);
                serverStream.Read(inStream, 0, inStream.Length);
                string returndata = System.Text.Encoding.ASCII.GetString(inStream);
                readData = "" + returndata;
                WriteMessage();
            }
        }

        private void WriteMessage()
        {
            if (this.InvokeRequired)
                this.Invoke(new MethodInvoker(WriteMessage));
            else
                textBoxChat.Text += Environment.NewLine + " >> " + readData;
        }

        private bool Connect()
        {
            try
            {
                nickname = textBoxNick.Text;
                ip = textBoxIP.Text;
                port = int.Parse(textBoxPort.Text);

                clientSocket.Connect(ip, port);
                serverStream = clientSocket.GetStream();

                byte[] outStream = System.Text.Encoding.ASCII.GetBytes(textBoxNick.Text + "$");
                serverStream.Write(outStream, 0, outStream.Length);
                serverStream.Flush();

                Thread ctThread = new Thread(GetMessage);
                ctThread.Start();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
