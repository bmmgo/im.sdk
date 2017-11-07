using System;
using System.Linq;
using System.Net.Sockets;
using System.Threading;

namespace im.sdk
{
    /// <inheritdoc />
    /// <summary>
    /// socket通讯实现
    /// </summary>
    public class SimpleSocket : IDisposable
    {
        private Socket _socket;
        private Thread _readThread;
        private FixLengthConvert _convert;
        private bool _connected = false;
        /// <summary>
        /// 连接
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        public void Connect(string ip, int port)
        {
            _convert = new FixLengthConvert();
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(ip, port);
            _connected = true;
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
                    if (len != 0)
                    {
                        try
                        {
                            foreach (var bytese in _convert.DecodeFrame(buffer.Take(len).ToArray()))
                            {
                                OnReceived?.Invoke(bytese);
                            }
                            continue;
                        }
                        catch (Exception)
                        {
                            //ignore
                        }
                    }
                    _connected = false;
                    _socket.Dispose();
                    OnDisconnected?.Invoke(this);
                    return;
                }
            });
            _readThread.Start();
        }
        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public bool Send(byte[] bytes)
        {
            if (!_connected) return false;
            bytes = _convert.ToFrame(bytes);
            try
            {
                var len = _socket.Send(bytes);
                return len == bytes.Length;
            }
            catch (Exception)
            {
                //ignore
            }
            return false;
        }
        /// <summary>
        /// 收到数据事件
        /// </summary>
        public event Action<byte[]> OnReceived;
        /// <summary>
        /// 连接断开事件
        /// </summary>
        public event Action<SimpleSocket> OnDisconnected;
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void Dispose()
        {
            _readThread?.Abort();
            _readThread?.Join();
            _socket.Dispose();
        }
    }
}
