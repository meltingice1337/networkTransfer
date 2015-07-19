using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nTransfer_Server
{
    class Program
    {
        private static eSock.Server _server = new eSock.Server();
        static Dictionary<int, FileStream> FileHandles = new Dictionary<int, FileStream>();
        private static int Handle = 1;
        static void Main(string[] args)
        {
            _server.OnClientConnect += _server_OnClientConnect;
            _server.OnDataRetrieved += _server_OnDataRetrieved;
            _server.OnClientDisconnect += _server_OnClientDisconnect;
            _server.Start(1337);
            Console.WriteLine("Started on 1337");
            while (true)
            {
                Console.ReadLine();
            }
        }

        static void _server_OnClientDisconnect(eSock.Server sender, eSock.Server.eSockClient client, System.Net.Sockets.SocketError ER)
        {
            Console.WriteLine("Client disconnected");
        }

        static void _server_OnDataRetrieved(eSock.Server sender, eSock.Server.eSockClient client, object[] data)
        {
            PacketHeader header = (PacketHeader) (byte)data[0];
            if (header == PacketHeader.StartFileTransfer)
            {
                int _fHandle = Handle;
                lock (_server)
                {
                    while (FileHandles.ContainsKey(_fHandle))
                    {
                        _fHandle = ++Handle;
                    }
                }
                FileHandles.Add(_fHandle, new FileStream((string)data[1], FileMode.Create));
                client.Send((byte)PacketHeader.FileTransferResponce, _fHandle);
            }

            if (header == PacketHeader.FileTransferBlock)
            {
                int _fhandle = (int) data[1];
                byte[] block = (byte[]) data[2];
                bool finalBlock = (bool) data[3];
                if (!FileHandles.ContainsKey(_fhandle))
                    return;
                FileHandles[_fhandle].Write(block, 0, block.Length);
                if (finalBlock)
                {
                    FileHandles[_fhandle].Close();
                    FileHandles[_fhandle].Dispose();
                    FileHandles.Remove(_fhandle);
                    lock (_server)
                    {
                        if (Handle > _fhandle)
                            Handle = _fhandle;
                    }
                }
                    
            }
        }

        static void _server_OnClientConnect(eSock.Server sender, eSock.Server.eSockClient client)
        {
            
                 Console.WriteLine("Client connected");
        }
    }
}
