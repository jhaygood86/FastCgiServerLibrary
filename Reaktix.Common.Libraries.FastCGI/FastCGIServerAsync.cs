using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Reaktix.Common.Libraries.FastCGI
{
    public abstract class FastCGIServerAsync
    {
        protected TcpListener TcpListener;
        protected CancellationToken CancellationToken;
        public bool Debug = false;

        public abstract Task HandleRequestAsync(FastCGIRequestAsync Request, FastCGIResponseAsync Response);

        public void ListenAsyncAndWait(ushort Port, string Address = "0.0.0.0")
        {
            ListenAsync(Port, Address).Wait();
        }

        public async Task ListenAsync(ushort Port, string Address = "0.0.0.0")
        {
            TcpListener = new TcpListener(IPAddress.Parse(Address), Port);
            TcpListener.Start();
            if (Debug)
            {
                await Console.Out.WriteLineAsync(String.Format("Listening {0}:{1}", Address, Port));
            }

            while (true)
            {
                var FastcgiServerClientHandlerAsync = new FastCGIServerClientHandlerAsync(this, await TcpListener.AcceptTcpClientAsync());
                await FastcgiServerClientHandlerAsync.Handle();
            }
        }

        public void Listen(ushort Port, string Address = "0.0.0.0")
        {
            ListenAsync(Port, Address);

            while (true)
            {
                Thread.Sleep(int.MaxValue);
            }
        }
    }
}
