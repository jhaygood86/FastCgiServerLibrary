using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reaktix.Common.Libraries.FastCGI
{
    public class FastCGIRequestAsync
    {
        public Dictionary<String, String> Params;
        public FastCGIInputStream StdInStream;
    }
}
