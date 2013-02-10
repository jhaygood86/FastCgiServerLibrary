using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reaktix.Common.Libraries.FastCGI
{
    public class FastCGIHeaders
    {
        public List<KeyValuePair<String, String>> Headers = new List<KeyValuePair<string, string>>();

        public void Replace(String Key, String Value)
        {
            for (int n = 0; n < Headers.Count; n++)
            {
                if (Headers[n].Key == Key)
                {
                    Headers[n] = new KeyValuePair<String, String>(Key, Value);
                    return;
                }
            }
            Add(Key, Value);
        }

        public void Add(String Key, String Value)
        {
            Headers.Add(new KeyValuePair<String, String>(Key, Value));
        }
    }
}
