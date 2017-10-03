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
        private Thread _heartThread;
        private bool _autoReconnect;
        private SimpleSocket _socket;

        public string Ip { get; set; } = "www.bmmgo.com";
        public int Port { get; set; } = 16666;

        /// <summary>
        /// 初始化
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
                _socket.Connect(Ip, Port);
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

        public void Stop()
        {
            _heartThread.Abort();
            _heartThread.Join();
            _socket.Dispose();
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
            try
            {
                var package = SocketPackage.Parser.ParseFrom(obj);
                switch (package.Category)
                {
                    case PackageCategory.ReceivedChannelMsg:
                        ReceivedChannelMessage(package);
                        break;
                    case PackageCategory.Result:
                        ProcessSocketResult(package);
                        break;
                }
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, ex);
            }
        }

        private void ProcessSocketResult(SocketPackage package)
        {
            var result = SocketResult.Parser.ParseFrom(package.Content);
            switch (result.Category)
            {
                case PackageCategory.Login:
                    if (result.Code == ResultCode.Success)
                    {
                        StartHeart();
                        OnLogin?.Invoke(this, true, null);
                    }
                    else
                    {
                        OnLogin?.Invoke(this, false, result.Message);
                    }
                    break;
                    //case PackageCategory.Ping:
                    //    Console.WriteLine("心跳成功");
                    //    break;
            }
        }

        private void StartHeart()
        {
            _heartThread = new Thread(obj =>
            {
                while (true)
                {
                    Send(PackageCategory.Ping);
                    Thread.Sleep(60 * 1000);
                }
            });
            _heartThread.Start();
        }

        private void ReceivedChannelMessage(SocketPackage package)
        {
            var message = IM.Protocol.ReceivedChannelMessage.Parser.ParseFrom(package.Content);
            OnReceivedChannelMessage?.Invoke(this, message);
        }

        private void Send(PackageCategory category, IMessage msg = null)
        {
            //var seq = Interlocked.Increment(ref _seq);
            //var completionSource = new TaskCompletionSource<SocketResult>();
            //_taskCompletionSources.TryAdd(seq, completionSource);
            _socket.Send(new SocketPackage { Seq = 0, Category = category, Content = msg == null ? ByteString.Empty : msg.ToByteString() }.ToByteArray());
            //completionSource.Task.ContinueWith(t =>
            //{
            //    //var res = t.Result;
            //    //Console.WriteLine("received {0},{1},{2} {3}", res.Category, res.Code, res.Message, DateTime.Now);
            //});
        }

        public void Login(string appkey, string userId, string secrect, bool isAdmin = false)
        {
            var loginToken = new LoginToken();
            loginToken.Appkey = appkey;
            loginToken.UserID = userId;
            loginToken.Token = Md5(loginToken.Appkey + loginToken.UserID + secrect).ToLower();
            loginToken.IsAdmin = isAdmin;
            Send(PackageCategory.Login, loginToken);
        }

        private string Md5(string from)
        {
            return System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile(from, "MD5");
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

        public void SendToChannel(SendChannelMessage message)
        {
            Send(PackageCategory.SendToChannel, message);
        }

        public void SendToUser(SendUserMessage message)
        {
            Send(PackageCategory.SendToUser, message);
        }

        public event Action<ImClient, ReceivedChannelMessage> OnReceivedChannelMessage;
        public event Action<ImClient> OnConnected;
        public event Action<ImClient> OnDisconnected;
        public event Action<ImClient> OnConnectedFailed;
        public event Action<ImClient, Exception> OnError;
        public event Action<ImClient, bool, string> OnLogin;
    }
}
