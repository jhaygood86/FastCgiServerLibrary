using Reaktix.Common.Libraries.FastCGI.Protocol;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Reaktix.Common.Libraries.FastCGI
{
    public class FastCGIRequest
    {
        public NameValueCollection QueryStringParams { get; private set; }


        internal FastCGIRequest(FastCGIRequestAsync request)
        {
            QueryStringParams = HttpUtility.ParseQueryString(request.Params["QUERY_STRING"]);
        }
    }
}
