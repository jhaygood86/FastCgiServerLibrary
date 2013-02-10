using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Reaktix.Common.Libraries.FastCGI.TestApplication
{
    class Program
    {
        private static FastCGIServer server;

        static void Main(string[] args)
        {
            server = new Server(4242);
            server.Start();

            while (true)
            {
                Thread.Sleep(int.MaxValue);
            }
        }


    }
}
