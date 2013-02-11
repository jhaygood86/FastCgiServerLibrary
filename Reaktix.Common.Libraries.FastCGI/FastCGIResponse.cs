using Reaktix.Common.Libraries.FastCGI.Protocol;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Reaktix.Common.Libraries.FastCGI
{
    public class FastCGIResponse
    {
        public NameValueCollection Headers { get; private set; }
        public Stream OutputStream { get; private set; }

        public HttpStatusCode StatusCode { get; set; }

        public FastCGIResponse()
        {
            Headers = new NameValueCollection();
            StatusCode = HttpStatusCode.OK;
            OutputStream = new MemoryStream();
        }

        internal void WriteToResponse(FastCGIResponseAsync asyncResponse)
        {
            asyncResponse.Headers.Add("Status", ((int)StatusCode).ToString());

            foreach (var header in Headers.AllKeys)
            {
                asyncResponse.Headers.Add(header, Headers.Get(header));
            }

            OutputStream.CopyTo(asyncResponse.StdOutStream);
        }
    }
}
