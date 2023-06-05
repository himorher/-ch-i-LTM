﻿using System.Net.Sockets;
using System.Net;
using System.Text;

namespace dmServer
{
    public partial class Server : Form
    {
        public Server()
        {
            InitializeComponent();
        }

        IPEndPoint ipe = new IPEndPoint(IPAddress.Any, 18000);
        Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        List<Socket> clientlist = new List<Socket>();

        private void Server_Load(object sender, EventArgs e)
        {
            listView1.Items.Add("Server is ready...");
            connect();
        }

        void connect() // hàm dùng để kết nối với các client
        {
            server.Bind(ipe);
            Thread listen = new Thread(() =>
            {
                try
                {
                    while (true)
                    {
                        server.Listen(100);
                        Socket client = server.Accept();
                        //listView1.Items.Add("Client is connected");
                        clientlist.Add(client);
                        string str = "Client mới kết nối từ: " + client.RemoteEndPoint.ToString() + "\n";
                        listView1.Items.Add(new ListViewItem(str));
                        Thread recieve_thr = new Thread(recieve);
                        recieve_thr.Start(client);
                    }
                }
                catch
                {
                    IPEndPoint ipe = new IPEndPoint(IPAddress.Any, 18000);
                    Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                }

            });
            listen.Start();
        }
        void recieve(object obj) // hàm nhận message cùng với đó là gửi message đó cho các client còn lại.
        {
            // Socket cli = obj as Socket;
            Socket cli = (Socket)obj;
            try
            {
                while (true)
                {
                    byte[] data = new byte[1024 * 200];
                    cli.Receive(data);
                    string mess = Encoding.UTF8.GetString(data);
                    listView1.Items.Add(mess);
                    foreach (Socket item in clientlist)
                    {
                        if (item != null && item != cli) item.Send(data);
                    }
                }
            }
            catch
            {
                clientlist.Remove(cli);
                cli.Close();
            }
        }

        public static void doChat(Socket clientSocket, string n)

        {
            Console.WriteLine("getting file....");
            byte[] clientData = new byte[1024 * 5000];
            int receivedBytesLen = clientSocket.Receive(clientData);
            int fileNameLen = BitConverter.ToInt32(clientData, 0);
            string fileName = Encoding.ASCII.GetString(clientData, 4, fileNameLen);
            BinaryWriter bWrite = new BinaryWriter(File.Open(fileName + n, FileMode.Create));
            bWrite.Write(clientData, 4 + fileNameLen, receivedBytesLen - 4 - fileNameLen);
            bWrite.Close();
            clientSocket.Close();

            //[0]filenamelen[4]filenamebyte[*]filedata   

        }
    }
}