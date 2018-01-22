using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using im.sdk;
using IM.Protocol;
using IM.Test;
using Google.Protobuf;

namespace im.test.console
{
    class Program
    {
        private static Thread _heartThread;
        static void Main(string[] args)
        {
            var imClient = new ImClient(true);
            imClient.Ip = "127.0.0.1";
            imClient.Port = 16666;
            imClient.OnConnected += ImClient_OnConnected;
            imClient.OnConnectedFailed += ImClient_OnConnectedFailed;
            imClient.OnDisconnected += ImClient_OnDisconnected;
            imClient.OnError += ImClient_OnError;
            imClient.OnReceivedChannelMessage += ImClient_OnReceivedChannelMessage;
            imClient.OnReceivedUserMessage += ImClient_OnReceivedUserMessage;
            imClient.OnReceivedGroupMessage += ImClient_OnReceivedGroupMessage;
            imClient.OnReceivedUserLogin += ImClient_OnReceivedUserLogin;
            imClient.OnLogin += ImClient_OnLogin;
            imClient.Start();
            while (true)
            {
                var s = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(s))
                {
                    imClient.SendToChannel("1", s, 0);
                    imClient.SendToGroup(new SendGroupMessage
                    {
                        Content = "group",
                        GroupID = "1"
                    });

                    AdminMessage message = new AdminMessage();
                    message.Receiver = "41f7d5a8942ba6956a9e6e9d70aaf7a8";
                    message.Category = PackageCategory.ReceivedChannelMsg;
                    var channelMessage = new ReceivedChannelMessage();
                    channelMessage.Content = "test";
                    message.MessageContent = channelMessage.ToByteString();
                    imClient.AdminSend(message);
                }
            }
        }

        private static void ImClient_OnReceivedUserLogin(ImClient arg1, LoginToken arg2)
        {
            Console.WriteLine("received user login:{0}", arg2.UserID);
        }

        private static void ImClient_OnReceivedGroupMessage(ImClient arg1, ReceivedGroupMessage msg)
        {
            Console.WriteLine("received group message:{0}", msg.Content);
        }

        private static void ImClient_OnReceivedUserMessage(ImClient arg1, ReceivedUserMessage msg)
        {
            Console.WriteLine("received user message:{0}", msg.Content);
        }

        private static void ImClient_OnReceivedChannelMessage(ImClient im, IM.Protocol.ReceivedChannelMessage msg)
        {
            if (msg.Type != -1)
            {
                im.SendToChannel("1", ReplayTxt.GetTxt(), -1);
            }
            Console.WriteLine("received channel message:{0}", msg.Content);
        }

        private static void ImClient_OnError(ImClient obj, Exception exception)
        {
            Console.WriteLine("error:{0}", exception.Message);
        }

        private static void ImClient_OnDisconnected(ImClient obj)
        {
            Console.WriteLine("disconnected");
        }

        private static void ImClient_OnConnectedFailed(ImClient obj)
        {
            Console.WriteLine("connect failed");
        }

        private static void ImClient_OnConnected(ImClient obj)
        {
            Console.WriteLine("connected");
            obj.Login("www.bmmgo.com", Guid.NewGuid().ToString("N"), "41f7d5a8942ba6956a9e6e9d70aaf7a8", true);
        }

        private static void ImClient_OnLogin(ImClient arg1, bool arg2, string arg3)
        {
            if (arg2)
            {
                Console.WriteLine("login success");
                arg1.BindToChannel("1");
                arg1.BindToGroup("1");
                arg1.SubUserLogin();
            }
            else
            {
                Console.WriteLine("login failed:{0}", arg3);
            }
        }

    }
}
