using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using IM.Protocol;

namespace im.sdk
{
    public class ImClient
    {
        private bool _autoReconnect;
        private SimpleSocket _socket;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="autoReconnect">自动重连</param>
        public ImClient(bool autoReconnect = false)
        {
            _autoReconnect = autoReconnect;
        }

        public void Start()
        {
            _socket?.Dispose();
            _socket = new SimpleSocket();
            _socket.OnReceived += _socket_OnReceived;
            _socket.OnDisconnected += _socket_OnDisconnected;
            try
            {
                _socket.Connect("www.bmmgo.com", 16666);
                OnConnected?.Invoke(this);
                return;
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, ex);
                OnConnectedFailed?.Invoke(this);
            }
            ReconnectIfNeed();
        }

        private void _socket_OnDisconnected(SimpleSocket obj)
        {
            OnDisconnected?.Invoke(this);
            ReconnectIfNeed();
        }

        private void ReconnectIfNeed()
        {
            if (_autoReconnect)
            {
                Task.Factory.StartNew(() =>
                {
                    Thread.Sleep(1000);
                    Start();
                });
            }
        }

        private void _socket_OnReceived(byte[] obj)
        {
            var package = SocketPackage.Parser.ParseFrom(obj);
            switch (package.Category)
            {
                case PackageCategory.ReceivedChannelMsg:
                    ReceivedChannelMessage(package);
                    break;
            }
        }

        private void ReceivedChannelMessage(SocketPackage package)
        {
            var message = IM.Protocol.ReceivedChannelMessage.Parser.ParseFrom(package.Content);
            OnReceivedChannelMessage?.Invoke(this, message);
        }

        private void Send(PackageCategory category, IMessage msg)
        {
            //var seq = Interlocked.Increment(ref _seq);
            //var completionSource = new TaskCompletionSource<SocketResult>();
            //_taskCompletionSources.TryAdd(seq, completionSource);
            _socket.Send(new SocketPackage { Seq = 0, Category = category, Content = msg.ToByteString() }.ToByteArray());
            //completionSource.Task.ContinueWith(t =>
            //{
            //    //var res = t.Result;
            //    //Console.WriteLine("received {0},{1},{2} {3}", res.Category, res.Code, res.Message, DateTime.Now);
            //});
        }

        public void Login(string userId)
        {
            var loginToken = new LoginToken();
            loginToken.UserID = Guid.NewGuid().ToString("N");
            Send(PackageCategory.Login, loginToken);
        }

        public void JoinChannel(string channel)
        {
            var ch = new Channel();
            ch.ChannelID = channel;
            Send(PackageCategory.JoinChannel, ch);
        }

        public void LeaveChannel(string channel)
        {
            var ch = new Channel();
            ch.ChannelID = channel;
            Send(PackageCategory.LeaveChannel, ch);
        }

        public void SendToChannel(string channel, string content, int type)
        {
            var sendChannelMsg = new SendChannelMessage();
            sendChannelMsg.Content = content;
            sendChannelMsg.ChannelID = channel;
            sendChannelMsg.Type = type;
            Send(PackageCategory.SendToChannel, sendChannelMsg);
        }

        public void SendToUser(string user, string content, int type)
        {
        }

        public event Action<ImClient, ReceivedChannelMessage> OnReceivedChannelMessage;
        public event Action<ImClient> OnConnected;
        public event Action<ImClient> OnDisconnected;
        public event Action<ImClient> OnConnectedFailed;
        public event Action<ImClient, Exception> OnError;
    }
}
