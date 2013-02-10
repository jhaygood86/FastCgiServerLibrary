using Reaktix.Common.Libraries.FastCGI.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reaktix.Common.Libraries.FastCGI.Handler
{
    class FastCGIServerHandler : FastCGIServerAsync
    {
        private Dictionary<string, Func<FastCGIRequest, FastCGIResponse>> handlers = new Dictionary<string, Func<FastCGIRequest, FastCGIResponse>>();

        public void AddHandlerDelegate(string path, Func<FastCGIRequest, FastCGIResponse> handlerDelegate)
        {
            handlers[path] = handlerDelegate;
        }

        public override Task HandleRequestAsync(FastCGIRequestAsync request, FastCGIResponseAsync response)
        {
            Action<FastCGIRequestAsync,FastCGIResponseAsync> action = this.HandleRequest;

            return Task.Factory.FromAsync(action.BeginInvoke, action.EndInvoke, request, response, null);
        }

        private void HandleRequest(FastCGIRequestAsync request, FastCGIResponseAsync response)
        {
            if (!handlers.ContainsKey(request.Params["SCRIPT_FILENAME"]))
            {
                GenerateNotFoundResponse(response);
            }
            else
            {
                GenerateResponse(request, response);
            }
        }

        private void GenerateNotFoundResponse(FastCGIResponseAsync response)
        {
            response.Headers.Add("Status", "404 Not Found");
        }

        private void GenerateResponse(FastCGIRequestAsync request,FastCGIResponseAsync response)
        {
            var handler = handlers[request.Params["SCRIPT_FILENAME"]];

            FastCGIRequest requestArg = new FastCGIRequest();

            var responseResult = handler(requestArg);

            response.StdOutWriter.WriteLine("Hi!");
        }
    }
}
