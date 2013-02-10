using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Reaktix.Common.Libraries.FastCGI.Protocol
{
    class FastCGIServerClientHandlerAsync
    {
		public FastCGIServerAsync FastcgiServerAsync { get; set; }
		protected TcpClient Client;
		protected Dictionary<ushort, FastCGIServerClientRequestHandlerAsync> Handlers;

        public FastCGIServerClientHandlerAsync(FastCGIServerAsync FastcgiServerAsync, TcpClient Client)
		{
			this.FastcgiServerAsync = FastcgiServerAsync;
			this.Client = Client;
            this.Handlers = new Dictionary<ushort, FastCGIServerClientRequestHandlerAsync>();
		}

		public async Task Handle()
		{
			if (FastcgiServerAsync.Debug) await Console.Out.WriteLineAsync(String.Format("Handling Client"));
			var ClientStream = Client.GetStream();

			try
			{
				while (Client.Connected)
				{
					FastCGIPacket Packet;
					try
					{
						Packet = await new FastCGIPacket().ReadFromAsync(ClientStream);
					}
					catch (IOException)
					{
						Console.Error.WriteLine("Error Reading");
						break;
					}
                    FastCGIServerClientRequestHandlerAsync Handler;
					if (!this.Handlers.TryGetValue(Packet.RequestId, out Handler))
					{
                        Handler = this.Handlers[Packet.RequestId] = new FastCGIServerClientRequestHandlerAsync(this, ClientStream, Packet.RequestId);
					}
					await Handler.HandlePacket(Client, Packet);
				}
			}
			catch (IOException IOException)
			{
				if (FastcgiServerAsync.Debug)
				{
					Console.Error.Write(IOException.ToString());
				}
			}
		}
    }
}
