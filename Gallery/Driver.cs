using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DCAPI
{
    public class Driver
    {

        private Mutex mutex = new Mutex();
        Thread writeThread;

        Random rand;
        string userId;
        string password;
        bool isYudong = false;
        long startTime = 0;
        long tokenTime = 0;
        public static int base_post_delay = 30;
        public long postDelay;

        Queue<string> chatQueue = new Queue<string>();
        DCAPI myAPI;
        IUser myUser;
        Gallery.Gallery myGallery;
        bool tokenAvailalbe = false;
        public bool stopThread = false;
        public void EnqueueMessage(string msg) //mutex
        {
          //  Console.WriteLine("잠금");
            mutex.WaitOne();
            chatQueue.Enqueue(msg);
            Console.WriteLine("받음 " + msg+" 대기중인 메세지 :"+chatQueue.Count);
            mutex.ReleaseMutex();
        }

        public DCAPI InitDriver()
        {
            rand = new Random();
            myAPI = new DCAPI();
            myGallery = new Gallery.Gallery(myAPI.REST, myAPI.Token, "haruhiism");
            postDelay = (base_post_delay - 5) * 1000;
            Console.WriteLine("도배방지 시간: " + base_post_delay+"초");
            return myAPI;
        }
        public void SetCredential(bool yudong, string id, string pw)
        {

            this.userId = id;
            this.password = pw;
            isYudong = yudong;
            if (isYudong)
            {
                myUser = new Guest(id,pw);
            }
            else {
                myUser = new Member(id, pw);
            }
            tokenTime = CurrentTimeInMills();
        }

        public void WritePost(string title, string content)
        {
            if (!tokenAvailalbe) {
                long elapsedInMills = (CurrentTimeInMills() - tokenTime);
              //  Console.WriteLine(elapsedInMills + "초 지남");
                int remainTime = (int)(10 * 1000 - elapsedInMills);
                if (remainTime > 0 ) {
                    Console.WriteLine((remainTime / 1000) + "초 대기");
                    Thread.Sleep(remainTime);
                }
                tokenAvailalbe = true;
            }
            Console.WriteLine("     작성중 : " + title);
       
            var writeTask = myGallery.Write(myUser, title, content);
            if (writeTask.Result.result)
            {
                Console.WriteLine("     작성성공 " + title);
            }
            else {

                Console.WriteLine("     작성실패 " + writeTask.Result.cause);
            }
        }
        public long CurrentTimeInMills() {
            return DateTime.Now.Ticks / 10000;
        }
        public void DoSleep()
        {
            try
            {
               long delay = postDelay + (long)(rand.NextDouble() * 5000d);
                Console.WriteLine("도배 회피를 위해 " + ((double)delay / 1000d).ToString("0.0") + " 초 대기 ..."); 
                Thread.Sleep((int)delay);
                Console.WriteLine("대기 끝");
                startTime = CurrentTimeInMills();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }
        }
        public void Start()
        {
            stopThread = false;
            writeThread = new Thread(new ThreadStart(WriteMessages));
            writeThread.IsBackground = true;
            writeThread.Start();
        }
        public void Join() {
            writeThread.Join();
        }
        public void Stop() {
            stopThread = true;
        }
        public void WriteMessages() { 
            try
            {
                while (!stopThread)
                {
                    Console.WriteLine("남은 메세지 " + chatQueue.Count + " 개");
                    mutex.WaitOne();
                    if (chatQueue.Count > 0)
                    {
                        string msg = MergeMessages();
                        string title = msg;
                        if (title.Length > 30)
                        {
                            title = title.Substring(0, 30);
                        }
                        mutex.ReleaseMutex();
                        WritePost(title, msg);
                        DoSleep();
                    }
                    else
                    {
                        mutex.ReleaseMutex();
                        Thread.Sleep((int)3000);
                    }

                }
                Console.WriteLine("작성쓰레드 정지");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }
        }

        public string MergeMessages() //mutex
        {
            string msg = chatQueue.Dequeue(); 
            StringBuilder message = new(msg);
            while (chatQueue.Count > 0)
            {
                string nextMessage = chatQueue.Peek();
                int newLength = message.Length + nextMessage.Length + 1;
                if (newLength >= 30)
                {
                    break;
                }
                else {
                    message.Append(' ').Append(nextMessage);
                    chatQueue.Dequeue();
                }
            }
            return message.ToString();
        }

    }
}
