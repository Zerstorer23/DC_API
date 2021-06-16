using System;
using System.Threading;
namespace DCAPI
{
    public class Program
    {
        static Driver chatDriver;
        static TCPServer tcpServer;
        static string id, pw;
        static bool yudong = true;

        static void Main(string[] args)
        {
            ReadCredentials("setting.txt");
            StartServer();
        }
        public static void StartServer() {
            chatDriver = new Driver();
            try
            {
                chatDriver.SetCredential(yudong, id, pw);
                chatDriver.InitDriver();
                while (true) { 
                
                
                    tcpServer = new TCPServer(chatDriver);
                    bool connected = tcpServer.OpenServer(); //소켓 연결후 스레드 시작
                    if (connected) chatDriver.Start();

                    tcpServer.Join();
                    Console.WriteLine("하망호 클라이언트가 종료됨.");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                CloseServer();
            }
        }
        public static void CloseServer() {
            if (chatDriver != null)
            {
                chatDriver.Stop();
            }
            if (tcpServer != null)
            {
                tcpServer.ForceDisconnect();
            }
        }


        public static void Test()
        {
            DCAPI api = new DCAPI();
            Guest guest = new Guest("ㅇㅇ", "1234");
            Member member = new Member("1", "1");
            /* Gallery.Article article = api.GetArticle("programming", 77387);
             var Task1 = article.WriteComment(guest, "안녕하세요");
             var Task2 = article.WriteComment(member, "안녕하세요");*/
            Console.WriteLine("대기 10");
            Thread.Sleep(10 * 1000);

          /*  for (int i = 0; i < 5; i++)
            {

                var Task3 = REST.Upload.GalleryWrite(api.REST, "haruhiism", api.Token.AppId, "write", api.Token.ClientToken,
                    "스즈미야하루히안녕하세요" + i, null, null, ((IUser)member).UserId, new string[] { "안녕하세요" + i }, null, null);
                var result = Task3.Result;
            }*/

        }

        public static void ReadCredentials(string s)
        {
            try
            {
                System.IO.StreamReader reader = new System.IO.StreamReader(s);

                reader.ReadLine();
                string temp = reader.ReadLine();
                string yudongStr = temp.Split("=")[1];

                yudong = Int32.Parse(yudongStr) != 0;

                temp = reader.ReadLine();
                id = temp.Split("=")[1];
                temp = reader.ReadLine();
                pw = temp.Split("=")[1];
                temp = reader.ReadLine();
                if (temp != null) {
                    Driver.base_post_delay = Int32.Parse(temp.Split("=")[1]);
                }

            }
            catch (Exception)
            {

                Console.WriteLine("Settings 파일이 없습니다. 직접 정보를 입력하세요");
                Console.WriteLine("유동로그인 = 1, 고닉로그인 = 0 입력");
                string y = Console.ReadLine();
                yudong = !y.Contains('0');
                Console.Clear();
                Console.WriteLine("아이디 입력:");
                id = Console.ReadLine();

                Console.WriteLine("비밀번호 입력:");
                pw = Console.ReadLine();
                Console.Clear();
            }
                Console.WriteLine(yudong ? "유동으로 접속 " + id : "고닉으로 접속");

        }
    }
}
