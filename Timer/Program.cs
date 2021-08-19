using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Timer
{
    class Program
    {
        static int RunNum = 10;
        static AutoResetEvent myEvent = new AutoResetEvent(false);
        static void Main(string[] args)
        {
            Console.WriteLine("请输入半径");
            double r = Convert.ToDouble(Console.ReadLine());//半径
            double Π= 3.14159265358979323846;
            Console.WriteLine("请输入高");
            double h = Convert.ToDouble(Console.ReadLine());//高

            Console.WriteLine(string.Format("体积：{0:N2}",Convert.ToDouble(r*r*Π*4/3)));









            Dictionary<string, string> keyValuePairs = new Dictionary<string, string>();
            keyValuePairs.Remove("11");
            keyValuePairs.Add("11","22");
            if (keyValuePairs.Keys.Where(x=> x=="11").Count() > 0)
            {

            }
            
            Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff"));
            for (int i = 0; i < RunNum; i++)
            {
                Thread thread = new Thread(() => { ThreadTestWrite(i + 1); });
                thread.IsBackground = true;
                thread.Start();
            }

            Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff"));

            myEvent.WaitOne();
            Console.WriteLine("线程创建结束！");
            myEvent.Reset();

            Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff"));


            for (int i = 0; i < RunNum; i++)
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback(ThreadPoolTestWrite),i + 1);
            }

            Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff"));

            myEvent.WaitOne();
            Console.WriteLine("线程池结束！");
            
            Console.ReadKey();
        }

        public static void ThreadTestWrite(object Value)
        {
            Console.WriteLine(string.Format("{0}:创建了{1}号线程！",DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff"),(int)Value));
            if ((int)Value == 10)
            {
                myEvent.Set();
            }
        }

        public static void ThreadPoolTestWrite(object Value)
        {
            Console.WriteLine(string.Format("{0}:线程池运行了{1}号线程！", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff"), (int)Value));
            if ((int)Value == 10)
            {
                myEvent.Set();
            }
        }
    }
}
