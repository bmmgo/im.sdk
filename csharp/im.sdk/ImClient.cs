using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using IM.Protocol;

namespace im.sdk
{
    /// <summary>
    /// im 客户端
    /// </summary>
    public class ImClient
    {
        private Thread _heartThread;
        private readonly bool _autoReconnect;
        private SimpleSocket _socket;
        private readonly ConcurrentDictionary<int, TaskCompletionSource<SocketResult>> _requests = new ConcurrentDictionary<int, TaskCompletionSource<SocketResult>>();
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
            foreach (var requestKey in _requests.Keys)
            {
                if (_requests.TryRemove(requestKey, out TaskCompletionSource<SocketResult> request))
                {
                    request.TrySetCanceled();
                    SeqGenerator.Instance.FreeSeq(requestKey);
                }
            }
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
                    case PackageCategory.PubUserLogin:
                        ReceivedUserLogin(package);
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
            if (_requests.TryRemove(package.Seq, out TaskCompletionSource<SocketResult> request))
            {
                request.TrySetResult(result);
                SeqGenerator.Instance.FreeSeq(package.Seq);
            }

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

        private void ReceivedUserLogin(SocketPackage package)
        {
            var token = LoginToken.Parser.ParseFrom(package.Content);
            OnReceivedUserLogin?.Invoke(this, token);
        }

        private Task<SocketResult> Send(PackageCategory category, IMessage msg = null)
        {
            var seq = SeqGenerator.Instance.GetSeq();
            var completionSource = new TaskCompletionSource<SocketResult>();
            _requests.TryAdd(seq, completionSource);
            _socket.Send(new SocketPackage { Seq = seq, Category = category, Content = msg == null ? ByteString.Empty : msg.ToByteString() }.ToByteArray());
            return completionSource.Task;
        }
        /// <summary>
        /// 登陆
        /// </summary>
        /// <param name="appkey"></param>
        /// <param name="userId"></param>
        /// <param name="token"></param>
        /// <param name="isAdmin"></param>
        public Task<SocketResult> Login(string appkey, string userId, string token, bool isAdmin = false)
        {
            var loginToken = new LoginToken();
            loginToken.Appkey = appkey;
            loginToken.UserID = userId;
            loginToken.Token = token;
            loginToken.IsAdmin = isAdmin;
            return Send(PackageCategory.Login, loginToken);
        }

        /// <summary>
        /// 频道订阅
        /// </summary>
        /// <param name="channel"></param>
        public Task<SocketResult> BindToChannel(string channel)
        {
            var ch = new Channel();
            ch.ChannelID = channel;
            return Send(PackageCategory.BindToChannel, ch);
        }
        /// <summary>
        /// 取消频道订阅
        /// </summary>
        /// <param name="channel"></param>
        public Task<SocketResult> UnbindToChannel(string channel)
        {
            var ch = new Channel();
            ch.ChannelID = channel;
            return Send(PackageCategory.UnbindToChannel, ch);
        }
        /// <summary>
        /// 发送频道消息
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="content"></param>
        /// <param name="type"></param>
        public Task<SocketResult> SendToChannel(string channel, string content, int type)
        {
            var sendChannelMsg = new SendChannelMessage();
            sendChannelMsg.Content = content;
            sendChannelMsg.ChannelID = channel;
            sendChannelMsg.Type = type;
            return Send(PackageCategory.SendToChannel, sendChannelMsg);
        }
        /// <summary>
        /// 发送频道消息
        /// </summary>
        /// <param name="message"></param>
        public Task<SocketResult> SendToChannel(SendChannelMessage message)
        {
            return Send(PackageCategory.SendToChannel, message);
        }
        /// <summary>
        /// 发送私信
        /// </summary>
        /// <param name="message"></param>
        public Task<SocketResult> SendToUser(SendUserMessage message)
        {
            return Send(PackageCategory.SendToUser, message);
        }
        /// <summary>
        /// send message to group
        /// </summary>
        /// <param name="message"></param>
        public Task<SocketResult> SendToGroup(SendGroupMessage message)
        {
            return Send(PackageCategory.SendToGroup, message);
        }
        /// <summary>
        /// 推送用户消息
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="message">消息</param>
        /// <returns></returns>
        public Task<SocketResult> PushUserMessage(string sender, SendUserMessage message)
        {
            var pushMessage = new PushMessage();
            pushMessage.Receiver = message.Receiver;
            pushMessage.Category = PackageCategory.SendToUser;
            pushMessage.Sender = sender;
            pushMessage.MessageContent = message.ToByteString();
            return Send(PackageCategory.PushMsg, pushMessage);
        }
        /// <summary>
        /// 推送群消息
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="receiver">消息接收者</param>
        /// <param name="message">消息</param>
        /// <returns></returns>
        public Task<SocketResult> PushGroupMessage(string sender, string receiver, SendGroupMessage message)
        {
            var pushMessage = new PushMessage();
            pushMessage.Receiver = receiver;
            pushMessage.Category = PackageCategory.SendToGroup;
            pushMessage.Sender = sender;
            pushMessage.MessageContent = message.ToByteString();
            return Send(PackageCategory.PushMsg, pushMessage);
        }
        /// <summary>
        /// 推送频道消息
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="message">消息</param>
        /// <returns></returns>
        public Task<SocketResult> PushChannelMessage(string sender, SendChannelMessage message)
        {
            var pushMessage = new PushMessage();
            pushMessage.Receiver = message.ChannelID;
            pushMessage.Category = PackageCategory.SendToChannel;
            pushMessage.Sender = sender;
            pushMessage.MessageContent = message.ToByteString();
            return Send(PackageCategory.PushMsg, pushMessage);
        }

        ///// <summary>
        ///// bind group to receive group message
        ///// </summary>
        ///// <param name="groupId"></param>
        //public Task<SocketResult> BindToGroup(string groupId)
        //{
        //    var userGroup = new UserGroup();
        //    userGroup.GroupIDs.Add(groupId);
        //    return Send(PackageCategory.BindToGroup, userGroup);
        //}
        ///// <summary>
        ///// unbind group
        ///// </summary>
        ///// <param name="groupId"></param>
        //public Task<SocketResult> UnbindToGroup(string groupId)
        //{
        //    var userGroup = new UserGroup();
        //    userGroup.GroupIDs.Add(groupId);
        //    return Send(PackageCategory.UnbindToGroup, userGroup);
        //}
        ///// <summary>
        ///// admin send message
        ///// </summary>
        ///// <param name="message"></param>
        //public Task<SocketResult> AdminSend(AdminMessage message)
        //{
        //    return Send(PackageCategory.AdminSend, message);
        //}
        /// <summary>
        /// 订阅用户登陆
        /// </summary>
        public Task<SocketResult> SubUserLogin()
        {
            return Send(PackageCategory.SubUserLogin);
        }
        /// <summary>
        /// 取消订阅用户登陆
        /// </summary>
        public Task<SocketResult> UnsubUserLogin()
        {
            return Send(PackageCategory.UnsubUserLogin);
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
        /// <summary>
        /// 用户登陆事件
        /// </summary>
        public event Action<ImClient, LoginToken> OnReceivedUserLogin;
    }
}
