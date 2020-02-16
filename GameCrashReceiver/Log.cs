using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameCrashReceiver
{
    // Logging class for easy logging
    class Log
    {
        public static void LogStatus(string log)
        {
            Console.WriteLine(DateTime.Now.ToString("yyyy/MM/dd hh:mm:ss") + " : [STATUS] " + log);
        }

        public static void LogError(string log)
        {
            Console.WriteLine(DateTime.Now.ToString("yyyy/MM/dd hh:mm:ss") + " : [ERROR] " + log);
        }
    }
}
