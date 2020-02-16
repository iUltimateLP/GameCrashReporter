using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace GameCrashReceiver
{
    class Program
    {
        // Main execution
        static void Main(string[] args)
        {
            new CrashReceiver().Run();
        }
    }
}
