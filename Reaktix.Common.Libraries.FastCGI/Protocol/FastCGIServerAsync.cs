using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Reaktix.Common.Libraries.FastCGI.Protocol
{
    abstract class FastCGIServerAsync
    {
        protected TcpListener TcpListener;
        public bool Debug = false;

        public abstract Task HandleRequestAsync(FastCGIRequestAsync Request, FastCGIResponseAsync Response);

        public async Task ListenAsync(int Port, IPAddress Address)
        {
            TcpListener = new TcpListener(Address, Port);
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
    }
}
