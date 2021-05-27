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
        long postDelay = 10 * 1000;

        Queue<string> chatQueue = new Queue<string>();
        DCAPI myAPI;
        IUser myUser;
        Gallery.Gallery myGallery;
        bool tokenAvailalbe = false;
        public bool stopThread = false;
        public void EnqueueMessage(string msg) //mutex
        {
          //  Console.WriteLine("잠금");
         //   mutex.WaitOne();
            chatQueue.Enqueue(msg);
       //     mutex.ReleaseMutex();
            //Console.WriteLine("열림");
        }

        public DCAPI InitDriver()
        {
            rand = new Random();
            myAPI = new DCAPI();
            myGallery = new Gallery.Gallery(myAPI.REST, myAPI.Token, "haruhiism");
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
            Console.WriteLine("작성 : " + title);
       
            var writeTask = myGallery.Write(myUser, title, content);
            if (writeTask.Result.result)
            {
                Console.WriteLine("작성성공 " + title);
            }
            else {

                Console.WriteLine("작성실패 " + writeTask.Result.cause);
            }
            
            /*else
            {
                Console.WriteLine("작성시도 {0} {1}", title, content);
                var writeTask = REST.Upload.GalleryWrite(myAPI.REST, gallID, myAPI.Token.AppId, "write", myAPI.Token.ClientToken,
               title, null, null, myUser.UserId, new string[] { content }, null, null);
                var res=  DCException.GetResult(writeTask.Result);
                Console.WriteLine("작성완료: " + res.cause);
            }*/
        }
        public long CurrentTimeInMills() {
            return DateTime.Now.Ticks / 10000;
        }
        public void DoSleep()
        {
            try
            {
                //  long nextDelay = postDelay - (CurrentTimeInMills() - startTime);
                //   if (nextDelay < 500) nextDelay = 500;
                //   Console.WriteLine("도배 회피를 위해 " + ((double)nextDelay / 1000d).ToString("0.0") + " 초 대기 ...");
                postDelay += (long)(rand.NextDouble() * 5000d);
                Console.WriteLine("도배 회피를 위해 " + ((double)postDelay / 1000d).ToString("0.0") + " 초 대기 ..."); 
                Thread.Sleep((int)postDelay);
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
                    if (chatQueue.Count > 0)
                    {
                    //    Console.WriteLine("잠금");
                     //   mutex.WaitOne();
                        string msg = MergeMessages();
                      //  mutex.ReleaseMutex();
                     //   Console.WriteLine("열림");
                        string title = msg;
                        if (title.Length > 30)
                        {
                            title = title.Substring(0, 30);
                        }
                        WritePost(title, msg);
                        DoSleep();
                    }

                }
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
                if ((message.Length) < 10) //10자 이하는 모두 합성
                {
                    message.Append(' ').Append(nextMessage);
                    chatQueue.Dequeue();
                }
            }
            return message.ToString();
        }

    }
}
