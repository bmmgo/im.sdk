using System;
using System.Linq;
using System.Net.Sockets;
using System.Threading;

namespace im.sdk
{
    public class SimpleSocket : IDisposable
    {
        private Socket _socket;
        private Thread _readThread;
        private FixLengthConvert _convert;

        public void Connect(string ip, int port)
        {
            _convert = new FixLengthConvert();
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(ip, port);
            _socket = socket;
            _readThread = new Thread(() =>
            {
                var buffer = new byte[1460];
                while (true)
                {
                    var len = 0;
                    try
                    {
                        len = _socket.Receive(buffer, SocketFlags.None);
                    }
                    catch (Exception)
                    {
                        //ignore
                    }
                    if (len == 0)
                    {
                        OnDisconnected?.Invoke(this);
                        break;
                    }
                    try
                    {
                        foreach (var bytese in _convert.DecodeFrame(buffer.Take(len).ToArray()))
                        {
                            OnReceived?.Invoke(bytese);
                        }
                    }
                    catch (Exception ex)
                    {
                        _socket.Close();
                        OnDisconnected?.Invoke(this);
                    }
                }
            });
            _readThread.Start();
        }

        public bool Send(byte[] bytes)
        {
            bytes = _convert.ToFrame(bytes);
            var len = _socket.Send(bytes);
            return len == bytes.Length;
        }

        public event Action<byte[]> OnReceived;
        public event Action<SimpleSocket> OnDisconnected;
        public void Dispose()
        {
            _readThread?.Abort();
            _readThread?.Join();
            _socket.Dispose();
        }
    }
}
