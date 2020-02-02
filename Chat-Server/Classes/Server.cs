using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace Chat_Server.Classes
{
    class Server
    {
        private LinkedList<TcpClient> TcpClients;
        private int CurrentConnection;
        private LinkedList<String> Names;
        private TcpListener TcpServer;
        private LinkedList<string> PublicKeys;
        private Dictionary<int, String> Messages;

        public Server(string address, int port)
        {
            TcpClients = new LinkedList<TcpClient>();
            CurrentConnection = 0;
            Names = new LinkedList<String>();
            PublicKeys = new LinkedList<string>();
            try
            {
                TcpServer = new TcpListener(IPAddress.Any, port);
                TcpServer.Start();
                Console.WriteLine("Server started.");
                for(; ; )
                {
                    Byte[] data = new Byte[2048];
                    String name = String.Empty;
                    Int32 bytes;
                    TcpClient current = TcpServer.AcceptTcpClient();
                    Console.WriteLine("Accepted client!");
                    NetworkStream stream = current.GetStream();
                    bytes = stream.Read(data, 0, data.Length);
                    PublicKeys.AddLast(Encoding.ASCII.GetString(data, 0, bytes));
                    Console.WriteLine(PublicKeys.ElementAt(CurrentConnection));
                    TcpClients.AddLast(current);
                    byte[] key;
                    byte[] index;
                    for (int i = 0; i < PublicKeys.Count; i++)
                    {
                        if(i != CurrentConnection)
                        {
                            index = BitConverter.GetBytes(i);
                            key = Encoding.ASCII.GetBytes(PublicKeys.ElementAt(i));
                            stream.Write(index, 0, index.Length);
                            stream.Write(key, 0, key.Length);
                        }
                    }
                    key = BitConverter.GetBytes(CurrentConnection + 1);
                    stream.Write(key, 0, key.Length);
                    string blankString = "";
                    for(int i = 0; i < 617; i++)
                    {
                        blankString = blankString + "0";
                    }
                    key = Encoding.ASCII.GetBytes(blankString);
                    stream.Write(key, 0, key.Length);
                    byte[] blankChar = Encoding.ASCII.GetBytes(" ");
                    key = BitConverter.GetBytes(CurrentConnection + 1);
                    for (int i = 0; i < PublicKeys.Count - 1; i++)
                    {
                        NetworkStream receiver = TcpClients.ElementAt(i).GetStream();
                        receiver.Write(blankChar, 0, blankChar.Length);
                        receiver.Write(key, 0, key.Length);
                        receiver.Write(data, 0, data.Length);
                    }
                    
                    Thread thread = new Thread(new ThreadStart(ClientThread));
                    thread.IsBackground = true;
                    thread.Start();
                    Thread.Sleep(1000);
                    CurrentConnection++;
                }
            }
            catch(ArgumentException e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private void ClientThread()
        {
            int Id = CurrentConnection;
            NetworkStream stream = TcpClients.ElementAt(Id).GetStream();
            for (; ; )
            {
                Byte[] data = new Byte[256];
                Byte[] client = new Byte[32];
                Int32 clientBytes = stream.Read(client, 0, client.Length);
                Int32 bytes = stream.Read(data, 0, data.Length);
                int userId = BitConverter.ToInt32(client, 0);
                Console.WriteLine("Client number " + Id.ToString() + " sent message!");
                NetworkStream sendStream = TcpClients.ElementAt(userId).GetStream();
                sendStream.Write(data, 0, data.Length);
            }
        }
    }
}
