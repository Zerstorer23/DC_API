using DCAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DCAPI
{
   public class TCPServer
    {
        public Driver driver;
        int portNumber = 9917;
        static int BUFFER = 32 * 1024;
        public readonly static string NET_DELIM = "#";
        public readonly static string NET_SIG = "LEX";
        Thread listenThread;
        TcpListener listener;
        TcpClient client;

        public TCPServer(Driver chatDriver)
        {
            this.driver = chatDriver;
        }


        // Start is called before the first frame update

        public bool OpenServer()
        {
            Console.WriteLine("하망호 클라이언트 연결 대기");
            listener = new TcpListener(IPAddress.Any, portNumber);
            listener.Start();
            client = listener.AcceptTcpClient();
            Console.WriteLine("연결됨 IP {0}", client.Client.RemoteEndPoint.AddressFamily.ToString());
            listenThread = new Thread(new ThreadStart(ListenMessage));
            listenThread.IsBackground = true;
            listenThread.Start();
            return true;
        }
        private void ListenMessage()
        {
            byte[] packet = new byte[BUFFER];
            int receivedBytes;
            NetworkStream stream = client.GetStream();
            while ((receivedBytes = stream.Read(packet, 0, packet.Length)) != 0)
            {
                string str = Encoding.UTF8.GetString(packet, 0, receivedBytes);
                str = str.Replace("\0", string.Empty);
            //    Console.WriteLine("받음 {0} : {1} : {2}", str,str.Length,receivedBytes);
                string[] tokens = str.Split(NET_DELIM);
                if (tokens.Length < 2) continue;
                if (tokens[0].Length == NET_SIG.Length + 1) {
                    tokens[0] = tokens[0].Substring(1);
                }
                if (tokens[0] != NET_SIG) continue;
                str = tokens[1];
                driver.EnqueueMessage(str);
            }
         //   Console.WriteLine("연결해제 " + receivedBytes);
            stream.Close();
            client.Close();
            listener.Stop();
            
        }
        public void Join() {
            listenThread.Join();
        }
        public void ForceDisconnect() {
            if (client != null) {
                client.GetStream().Close();
                client.Close();
                listener.Stop();
            }

        }
    }



}
