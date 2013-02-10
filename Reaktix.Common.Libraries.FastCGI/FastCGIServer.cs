using Reaktix.Common.Libraries.FastCGI.Handler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Reaktix.Common.Libraries.FastCGI
{
    public abstract class FastCGIServer
    {
        private int port;
        private IPAddress address;

        private FastCGIServerHandler handler;

        private Task task;

        public FastCGIServer(int port, IPAddress address)
        {
            handler = new FastCGIServerHandler();

            this.port = port;
            this.address = address;
        }

        public FastCGIServer(int port, string address = "0.0.0.0")
        {
            IPAddress ipAddress = IPAddress.Parse(address);

            this.port = port;
            this.address = ipAddress;

            handler = new FastCGIServerHandler();
        }

        protected void AddHandler(string path, Expression<Func<FastCGIRequest, FastCGIResponse>> requestHandler)
        {
            var requestDelegate = requestHandler.Compile();
            handler.AddHandlerDelegate(path, requestDelegate);
        }

        public void Start()
        {
            task = Task.Factory.StartNew(() => handler.ListenAsync(port, address));
        }
    }
}
