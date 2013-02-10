using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reaktix.Common.Libraries.FastCGI
{
    public class FastCGIResponseAsync
    {
        public FastCGIOutputStream StdOutStream;
        public FastCGIOutputStream StdErrStream;
        public FastCGIHeaders Headers;

        public StreamWriter StdOutWriter;
        public StreamWriter StdErrWriter;
    }
}
