using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Net.Sockets;
using System.Threading;
using System.Net;

namespace LiveChat___Server
{
    class Program
    {
        public static Hashtable clientsList = new Hashtable();

        static void Main(string[] args)
        {
            IPAddress ip = IPAddress.Any;
            int port = 8888;
            TcpListener serverSocket = new TcpListener(IPAddress.Any, port);
            TcpClient clientSocket = default(TcpClient);
            int counter = 0;

            serverSocket.Start();
            Console.WriteLine("Chat Server Started ....");
            Console.WriteLine("IP address: " + ip);
            Console.WriteLine("Port: " + port);

            counter = 0;
            while (true)
            {
                counter++;
                clientSocket = serverSocket.AcceptTcpClient();

                byte[] bytesFrom = new byte[clientSocket.ReceiveBufferSize];
                string dataFromClient = null;

                NetworkStream networkStream = clientSocket.GetStream();
                networkStream.Read(bytesFrom, 0, clientSocket.ReceiveBufferSize);
                dataFromClient = System.Text.Encoding.ASCII.GetString(bytesFrom);
                dataFromClient = dataFromClient.Substring(0, dataFromClient.IndexOf("$"));

                if (clientsList.ContainsKey(dataFromClient))
                {
                    Whisper("Nickname '" + dataFromClient + "' was already taken, please try another.", clientSocket);
                    clientSocket.Client.Shutdown(SocketShutdown.Both);
                    clientSocket.Close();
                }
                else
                {
                    clientsList.Add(dataFromClient, clientSocket);
                    Broadcast(dataFromClient + " Joined ", dataFromClient, false);
                    Console.WriteLine(dataFromClient + " Joined chat room ");
                    HandleClient client = new HandleClient();
                    client.StartClient(clientSocket, dataFromClient, clientsList);
                }
            }

            clientSocket.Close();
            serverSocket.Stop();
            Console.WriteLine("exit");
            Console.ReadLine();
        }

        public static void Whisper(string message, TcpClient client)
        {
            TcpClient socket = client;
            NetworkStream whisperStream = socket.GetStream();
            Byte[] whisperBytes = Encoding.ASCII.GetBytes(message);

            whisperStream.Write(whisperBytes, 0, whisperBytes.Length);
            whisperStream.Flush();
        }

        public static void Broadcast(string message, string uName, bool flag)
        {
            foreach (DictionaryEntry Item in clientsList)
            {
                TcpClient broadcastSocket;
                broadcastSocket = (TcpClient)Item.Value;
                NetworkStream broadcastStream = broadcastSocket.GetStream();
                Byte[] broadcastBytes = null;

                if (flag == true)
                {
                    broadcastBytes = Encoding.ASCII.GetBytes(uName + " says : " + message);
                }
                else
                {
                    broadcastBytes = Encoding.ASCII.GetBytes(message);
                }

                broadcastStream.Write(broadcastBytes, 0, broadcastBytes.Length);
                broadcastStream.Flush();
            }
        }
    }

    public class HandleClient
    {
        TcpClient clientSocket;
        string clNo;
        Hashtable clientsList;

        public void StartClient(TcpClient inClientSocket, string clineNo, Hashtable cList)
        {
            this.clientSocket = inClientSocket;
            this.clNo = clineNo;
            this.clientsList = cList;
            Thread ctThread = new Thread(doChat);
            ctThread.Start();
        }

        private void doChat()
        {
            int requestCount = 0;
            byte[] bytesFrom = new byte[clientSocket.ReceiveBufferSize];
            string dataFromClient = null;
            Byte[] sendBytes = null;
            string serverResponse = null;
            string rCount = null;
            requestCount = 0;

            while ((true))
            {
                try
                {
                    requestCount = requestCount + 1;
                    NetworkStream networkStream = clientSocket.GetStream();
                    networkStream.Read(bytesFrom, 0, clientSocket.ReceiveBufferSize);

                    dataFromClient = System.Text.Encoding.ASCII.GetString(bytesFrom);
                    dataFromClient = dataFromClient.Substring(0, dataFromClient.IndexOf("$"));
                    Console.WriteLine("From client - " + clNo + " : " + dataFromClient);
                    rCount = Convert.ToString(requestCount);

                    Program.Broadcast(dataFromClient, clNo, true);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    Console.WriteLine("Client : " + clNo + " disconnected.");

                    Program.clientsList.Remove(clNo);
                    break;
                }
            }
        }
    }
}
