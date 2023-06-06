using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace dmServer
{
    public partial class Server_again : Form
    {
        public Server_again()
        {
            InitializeComponent();
        }
        IPEndPoint ipe = new IPEndPoint(IPAddress.Any, 18000);
        Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        List<Socket> listClient = new List<Socket>();

        private void button1_Click(object sender, EventArgs e)
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
                        listClient.Add(client);
                        string str = "Client mới kết nối từ: " + client.RemoteEndPoint.ToString() + "\n";
                        listView1.Items.Add(new ListViewItem(str));
                        Thread recieve_thr = new Thread(receive);
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
        void receive(object obj) // hàm nhận message cùng với đó là gửi message đó cho các client còn lại.
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
                    foreach (Socket item in listClient)
                    {
                        if (item != null && item != cli) item.Send(data);
                    }
                }
            }
            catch
            {
                listClient.Remove(cli);
                cli.Close();
            }
        }
        private void button2_Click(object sender, EventArgs e) //button close
        {
            this.Close();
        }
        public static void doChat(Socket clientSocket) //nhan file va xu ly

        {
            Console.WriteLine("getting file....");
            byte[] clientData = new byte[1024 * 5000];
            int receivedBytesLen = clientSocket.Receive(clientData);
            int fileNameLen = BitConverter.ToInt32(clientData, 0);
            string fileName = Encoding.ASCII.GetString(clientData, 4, fileNameLen);
            BinaryWriter bWrite = new BinaryWriter(File.Open(fileName, FileMode.Create));
            bWrite.Write(clientData, 4 + fileNameLen, receivedBytesLen - 4 - fileNameLen);
            bWrite.Close();
            clientSocket.Close();

            //[0]filenamelen[4]filenamebyte[*]filedata   

        }
    }
}
