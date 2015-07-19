using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace nTransfer_Client
{
    class Program
    {
        private static eSock.Client _client = new eSock.Client();
        static bool Connected = false;
        private static FileStream _stream;
        private static int FileHandle = 0;
        static void Main(string[] args)
        {
            _client.OnDataRetrieved += _client_OnDataRetrieved;
            _client.OnDisconnect += _client_OnDisconnect;
            if (!_client.Connect("127.0.0.1", 1337))
            {
                Console.WriteLine("Failed to connect");
                Console.ReadLine();
                return;
            }
            Connected = true;
            
            while (Connected)
            {
                try
                {
                    Console.Write("File to send: ");
                    string path = Console.ReadLine();
                    _stream = new FileStream(path, FileMode.Open);
                    _client.Send((byte)PacketHeader.StartFileTransfer, Path.GetFileName(path));
                    Console.WriteLine("Waiting for responce..");
                    while(FileHandle == 0)
                    {
                        Thread.Sleep(100);
                    }
                    Console.WriteLine("Handle: {0}", FileHandle);
                    byte[] buffer = new byte[1000];
                    int bytesRead = 0;
                    while ((bytesRead = _stream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        byte[] block = new byte[bytesRead];
                        Buffer.BlockCopy(buffer, 0, block, 0, block.Length);
                        _client.Send((byte)PacketHeader.FileTransferBlock, FileHandle, block, _stream.Position == _stream.Length);
                        Console.WriteLine("Sent block ({0} bytes) || LastBlock: {1}", bytesRead, _stream.Position == _stream.Length);
                    }
                    FileHandle = 0;
                    _stream.Dispose();
                    _stream = null;
                    Console.WriteLine("File Complete");
                }
                catch
                {
                    Console.WriteLine("Error");
                }
            }
            Console.WriteLine("Not connected");
            Console.ReadLine();
        }

        static void _client_OnDisconnect(eSock.Client sender, System.Net.Sockets.SocketError ER)
        {
            Connected = false;
        }

        static void _client_OnDataRetrieved(eSock.Client sender, object[] data)
        {
            byte header = (byte) data[0];
            if (header == (byte)PacketHeader.FileTransferResponce)
            {
                FileHandle = (int)data[1];
            }
        }
    }
}
