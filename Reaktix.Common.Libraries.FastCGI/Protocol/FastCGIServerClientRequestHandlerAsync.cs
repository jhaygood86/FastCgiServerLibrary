using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Reaktix.Common.Libraries.FastCGI.Protocol
{
    class FastCGIServerClientRequestHandlerAsync
    {
        protected Stream ClientStream;
		protected ushort RequestId;
		public MemoryStream ParamsStream = new MemoryStream();
		public FastCGIRequestAsync FastcgiRequestAsync;
		public FastCGIResponseAsync FastcgiResponseAsync;
		internal FastCGIServerClientHandlerAsync FastcgiServerClientHandlerAsync;

        public FastCGIServerClientRequestHandlerAsync(FastCGIServerClientHandlerAsync FastcgiServerClientHandlerAsync, Stream ClientStream, ushort RequestId)
		{
			this.FastcgiServerClientHandlerAsync = FastcgiServerClientHandlerAsync;
			this.ClientStream = ClientStream;
			this.RequestId = RequestId;
			this.FastcgiRequestAsync = new FastCGIRequestAsync()
			{
				StdInStream = new FastCGIInputStream(),
			};
			this.FastcgiResponseAsync = new FastCGIResponseAsync()
			{
				StdOutStream = new FastCGIOutputStream(),
                StdErrStream = new FastCGIOutputStream(),
				Headers = new FastCGIHeaders(),
			};

			this.FastcgiResponseAsync.StdOutWriter = new StreamWriter(this.FastcgiResponseAsync.StdOutStream);
			this.FastcgiResponseAsync.StdOutWriter.AutoFlush = true;

			this.FastcgiResponseAsync.StdErrWriter = new StreamWriter(this.FastcgiResponseAsync.StdErrStream);
			this.FastcgiResponseAsync.StdErrWriter.AutoFlush = true;
		}

		protected static int ReadVariable(Stream Stream)
		{
			int Value = 0;
			byte Data;
			do
			{
				Data = (byte)Stream.ReadByte();
				Value <<= 7;
				Value |= Data & 0x7F;
			} while ((Data & 0x80) != 0);
			return Value;
		}

        static private byte[] ReadBytes(Stream Stream, int ToRead)
        {
            if (ToRead == 0) return new byte[0];
            var Buffer = new byte[ToRead];
            int Readed = 0;
            while (Readed < ToRead)
            {
                int ReadedNow = Stream.Read(Buffer, Readed, ToRead - Readed);
                if (ReadedNow <= 0) throw (new Exception("Unable to read " + ToRead + " bytes, readed " + Readed + "."));
                Readed += ReadedNow;
            }
            return Buffer;
        }

        static private String ReadString(Stream stream, int bytesToRead, Encoding encoding = null)
        {
            if (encoding == null) encoding = Encoding.UTF8;

            if (bytesToRead == 0) return string.Empty;
            var buffer = new byte[bytesToRead];
            int bytesRead = 0;
            while (bytesRead < bytesToRead)
            {
                int readNow = stream.Read(buffer, bytesRead, bytesToRead - bytesRead);
                if (readNow <= 0) throw (new Exception("Unable to read " + bytesToRead + " bytes, readed " + bytesRead + "."));
                bytesRead += readNow;
            }

            return encoding.GetString(buffer);
        }


		public async Task HandlePacket(TcpClient Client, FastCGIPacket Packet)
		{
			if (FastcgiServerClientHandlerAsync.FastcgiServerAsync.Debug)
			{
				await Console.Out.WriteLineAsync(String.Format("HandlePacket"));
			}
			var Content = Packet.Content.Array;
			var ContentLength = Content.Length;

			switch (Packet.Type)
			{
				case FastCGI.PacketType.FCGI_BEGIN_REQUEST:
                    var Role = (FastCGI.Role)(Content[0] | (Content[1] << 8));
                    var Flags = (FastCGI.Flags)(Content[2]);
					break;
                case FastCGI.PacketType.FCGI_PARAMS:
					if (Content.Length == 0)
					{
						ParamsStream.Position = 0;
						FastcgiRequestAsync.Params = new Dictionary<string, string>();
						while (ParamsStream.Position < ParamsStream.Length)
						{
							int KeyLength = ReadVariable(ParamsStream);
							int ValueLength = ReadVariable(ParamsStream);

							var Key = ReadString(ParamsStream, KeyLength, Encoding.UTF8);
							var Value = ReadString(ParamsStream, ValueLength, Encoding.UTF8);

							FastcgiRequestAsync.Params[Key] = Value;
						}
					}
					else
					{
						ParamsStream.Write(Content, 0, ContentLength);
					}
					break;
				case FastCGI.PacketType.FCGI_STDIN:
					if (Content.Length == 0)
					{
						FastcgiRequestAsync.StdInStream.Position = 0;
						Exception Exception = null;
						var Stopwatch = new Stopwatch();

						Stopwatch.Start();
						try
						{
							await FastcgiServerClientHandlerAsync.FastcgiServerAsync.HandleRequestAsync(this.FastcgiRequestAsync, this.FastcgiResponseAsync);
						}
						catch (Exception _Exception)
						{
							Exception = _Exception;
						}
						Stopwatch.Stop();

						if (Exception != null)
						{
							var StreamWriter = new StreamWriter(FastcgiResponseAsync.StdErrStream);
							StreamWriter.WriteLine(String.Format("{0}", Exception));
							StreamWriter.Flush();
						}
						var HeaderPlusOutputStream = new MemoryStream();

						var HeaderStream = new MemoryStream();
						var HeaderStreamWriter = new StreamWriter(HeaderStream);
						HeaderStreamWriter.AutoFlush = true;

						FastcgiResponseAsync.Headers.Add("Content-Type", "text/html");
						FastcgiResponseAsync.Headers.Add("X-Time", Stopwatch.Elapsed.ToString());

						foreach (var Header in FastcgiResponseAsync.Headers.Headers)
						{
							HeaderStreamWriter.Write("{0}: {1}\r\n", Header.Key, Header.Value);
						}
						HeaderStreamWriter.Write("\r\n");

						HeaderStream.Position = 0;
						await HeaderStream.CopyToAsync(HeaderPlusOutputStream);
						FastcgiResponseAsync.StdOutStream.Position = 0;
						await FastcgiResponseAsync.StdOutStream.CopyToAsync(HeaderPlusOutputStream);
						HeaderPlusOutputStream.Position = 0;

                        await FastCGIPacket.WriteMemoryStreamToAsync(RequestId: RequestId, PacketType: FastCGI.PacketType.FCGI_STDOUT, From: HeaderPlusOutputStream, ClientStream: ClientStream);
                        await FastCGIPacket.WriteMemoryStreamToAsync(RequestId: RequestId, PacketType: FastCGI.PacketType.FCGI_STDERR, From: FastcgiResponseAsync.StdErrStream, ClientStream: ClientStream);

                        await new FastCGIPacket() { Type = FastCGI.PacketType.FCGI_STDOUT, RequestId = RequestId, Content = new ArraySegment<byte>() }.WriteToAsync(ClientStream);
                        await new FastCGIPacket() { Type = FastCGI.PacketType.FCGI_STDERR, RequestId = RequestId, Content = new ArraySegment<byte>() }.WriteToAsync(ClientStream);
                        await new FastCGIPacket() { Type = FastCGI.PacketType.FCGI_END_REQUEST, RequestId = RequestId, Content = new ArraySegment<byte>(new byte[] { 0, 0, 0, 0, (byte)FastCGI.ProtocolStatus.FCGI_REQUEST_COMPLETE }) }.WriteToAsync(ClientStream);
						ClientStream.Close();
					}
					else
					{
						await FastcgiRequestAsync.StdInStream.WriteAsync(Content, 0, ContentLength);
					}
					break;
				default:
					Console.Error.WriteLine("Unhandled packet type: '" + Packet.Type + "'");
					Client.Close();
					break;
			}
		}
    }
}
