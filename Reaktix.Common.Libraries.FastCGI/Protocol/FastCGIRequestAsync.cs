using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reaktix.Common.Libraries.FastCGI.Protocol
{
    class FastCGIRequestAsync
    {
        public Dictionary<String, String> Params;
        public FastCGIInputStream StdInStream;
    }
}
