using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reaktix.Common.Libraries.FastCGI.TestApplication
{
    public class Server : FastCGIServer
    {
        public Server(int port)
            : base(port)
        {
            AddHandler("/serve.coil",(request) => HandleRequest(request));
        }

        FastCGIResponse HandleRequest(FastCGIRequest request)
        {
            return new FastCGIResponse();
        }
    }
}
