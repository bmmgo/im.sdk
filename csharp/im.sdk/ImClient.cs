using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Google.Protobuf.Collections;
using IM.Protocol;

namespace im.sdk
{
    /// <summary>
    /// im 客户端
    /// </summary>
    public class ImClient
    {
        private Thread _heartThread;
        private bool _autoReconnect;
        private SimpleSocket _socket;
        /// <summary>
        /// 服务器ip地址
        /// </summary>
        public string Ip { get; set; } = "www.bmmgo.com";
        /// <summary>
        /// 服务器端口
        /// </summary>
        public int Port { get; set; } = 16666;

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="autoReconnect">自动重连</param>
        public ImClient(bool autoReconnect = false)
        {
            _autoReconnect = autoReconnect;
        }
        /// <summary>
        /// 启动客户端
        /// </summary>
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
        /// <summary>
        /// 停止客户端
        /// </summary>
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
                    case PackageCategory.ReceivedUserMsg:
                        ReceivedUserMessage(package);
                        break;
                    case PackageCategory.ReceivedGroupMsg:
                        ReceivedGroupMessage(package);
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
                    try
                    {
                        Send(PackageCategory.Ping);
                        Thread.Sleep(60 * 1000);
                    }
                    catch (Exception)
                    {
                        //ignor
                        break;
                    }
                }
            });
            _heartThread.Start();
        }

        private void ReceivedChannelMessage(SocketPackage package)
        {
            var message = IM.Protocol.ReceivedChannelMessage.Parser.ParseFrom(package.Content);
            OnReceivedChannelMessage?.Invoke(this, message);
        }

        private void ReceivedUserMessage(SocketPackage package)
        {
            var message = IM.Protocol.ReceivedUserMessage.Parser.ParseFrom(package.Content);
            OnReceivedUserMessage?.Invoke(this, message);
        }

        private void ReceivedGroupMessage(SocketPackage package)
        {
            var message = IM.Protocol.ReceivedGroupMessage.Parser.ParseFrom(package.Content);
            OnReceivedGroupMessage?.Invoke(this, message);
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
        /// <summary>
        /// 登陆
        /// </summary>
        /// <param name="appkey"></param>
        /// <param name="userId"></param>
        /// <param name="secrect"></param>
        /// <param name="isAdmin"></param>
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
        /// <summary>
        /// 频道订阅
        /// </summary>
        /// <param name="channel"></param>
        public void BindToChannel(string channel)
        {
            var ch = new Channel();
            ch.ChannelID = channel;
            Send(PackageCategory.BindToChannel, ch);
        }
        /// <summary>
        /// 取消频道订阅
        /// </summary>
        /// <param name="channel"></param>
        public void UnbindToChannel(string channel)
        {
            var ch = new Channel();
            ch.ChannelID = channel;
            Send(PackageCategory.UnbindToChannel, ch);
        }
        /// <summary>
        /// 发送频道消息
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="content"></param>
        /// <param name="type"></param>
        public void SendToChannel(string channel, string content, int type)
        {
            var sendChannelMsg = new SendChannelMessage();
            sendChannelMsg.Content = content;
            sendChannelMsg.ChannelID = channel;
            sendChannelMsg.Type = type;
            Send(PackageCategory.SendToChannel, sendChannelMsg);
        }
        /// <summary>
        /// 发送频道消息
        /// </summary>
        /// <param name="message"></param>
        public void SendToChannel(SendChannelMessage message)
        {
            Send(PackageCategory.SendToChannel, message);
        }
        /// <summary>
        /// 发送私信
        /// </summary>
        /// <param name="message"></param>
        public void SendToUser(SendUserMessage message)
        {
            Send(PackageCategory.SendToUser, message);
        }
        /// <summary>
        /// send message to group
        /// </summary>
        /// <param name="message"></param>
        public void SendToGroup(SendGroupMessage message)
        {
            Send(PackageCategory.SendToGroup, message);
        }

        /// <summary>
        /// bind group to receive group message
        /// </summary>
        /// <param name="groupId"></param>
        public void BindToGroup(string groupId)
        {
            var userGroup = new UserGroup();
            userGroup.GroupIDs.Add(groupId);
            Send(PackageCategory.BindToGroup, userGroup);
        }
        /// <summary>
        /// unbind group
        /// </summary>
        /// <param name="groupId"></param>
        public void UnbindToGroup(string groupId)
        {
            var userGroup = new UserGroup();
            userGroup.GroupIDs.Add(groupId);
            Send(PackageCategory.UnbindToGroup, userGroup);
        }

        /// <summary>
        /// 收到频道消息事件
        /// </summary>
        public event Action<ImClient, ReceivedChannelMessage> OnReceivedChannelMessage;
        /// <summary>
        /// receive user message event
        /// </summary>
        public event Action<ImClient, ReceivedUserMessage> OnReceivedUserMessage;
        /// <summary>
        /// receive group message event
        /// </summary>
        public event Action<ImClient, ReceivedGroupMessage> OnReceivedGroupMessage;
        /// <summary>
        /// 连接成功事件
        /// </summary>
        public event Action<ImClient> OnConnected;
        /// <summary>
        /// 连接断开事件
        /// </summary>
        public event Action<ImClient> OnDisconnected;
        /// <summary>
        /// 连接失败事件
        /// </summary>
        public event Action<ImClient> OnConnectedFailed;
        /// <summary>
        /// 异常事件
        /// </summary>
        public event Action<ImClient, Exception> OnError;
        /// <summary>
        /// 登陆成功事件
        /// </summary>
        public event Action<ImClient, bool, string> OnLogin;
    }
}
