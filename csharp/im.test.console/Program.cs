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
                    var para = s.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                    if (para.Length == 0)
                    {
                        Console.WriteLine("输入错误");
                        continue;
                    }
                    try
                    {
                        handCmd(imClient, para);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
            }
        }

        private static void handCmd(ImClient imClient, string[] para)
        {
            switch (para[0])
            {
                case "login":
                    login(imClient, para[1]);
                    break;
                case "pm": //发送私信
                    {
                        var res = imClient.SendToUser(new SendUserMessage
                        {
                            Content = "这是一条私信",
                            Receiver = para[1],
                            Type = 0
                        });
                        Console.WriteLine("SendToUser:{0}", res.Result.Code);
                    }
                    break;
                case "gm"://发送群消息
                    {
                        var res = imClient.SendToGroup(new SendGroupMessage
                        {
                            Content = "这是一条群消息",
                            GroupID = "1"
                        });
                        Console.WriteLine("SendToGroup:{0}", res.Result.Code);
                    }
                    break;
                case "cm": //发送频道消息
                    {
                        var res = imClient.SendToChannel("1", "这是一条频道消息", 0);
                        Console.WriteLine("SendToChannel:{0}", res.Result.Code);
                    }
                    break;
                case "bc":
                    {
                        var res = imClient.BindToChannel("1");
                        Console.WriteLine("BindToChannel:{0}", res.Result.Code);
                    }
                    break;
                case "lc":
                    {
                        var res = imClient.UnbindToChannel("1");
                        Console.WriteLine("UnbindToChannel:{0}", res.Result.Code);
                    }
                    break;
                //case "bg":
                //    {
                //        var res = imClient.BindToGroup("1");
                //        Console.WriteLine("BindToGroup:{0}", res.Result.Code);
                //    }
                //    break;
                //case "lg":
                //    {
                //        var res = imClient.UnbindToGroup("1");
                //        Console.WriteLine("UnbindToGroup:{0}", res.Result.Code);
                //    }
                //break;
                case "sl":
                    {
                        var res = imClient.SubUserLogin();
                        Console.WriteLine("SubUserLogin:{0}", res.Result.Code);
                    }
                    break;
                case "ul":
                    {
                        var res = imClient.UnsubUserLogin();
                        Console.WriteLine("UnsubUserLogin:{0}", res.Result.Code);
                    }
                    break;
                case "push":
                    {
                        var res = imClient.PushUserMessage("1", new SendUserMessage
                        {
                            Receiver = "2",
                            Content = "这是推送的一条私信",
                            MsgID = Guid.NewGuid().ToString("N")
                        });
                        Console.WriteLine("PushUserMessage:{0}", res.Result.Code);
                        res = imClient.PushGroupMessage("1", "2", new SendGroupMessage
                        {
                            GroupID = "1",
                            Content = "这是推送的一条群消息",
                            MsgID = Guid.NewGuid().ToString("N")
                        });
                        Console.WriteLine("PushGroupMessage:{0}", res.Result.Code);
                        res = imClient.PushChannelMessage("1", new SendChannelMessage
                        {
                            ChannelID = "1",
                            Content = "这是推送的一条频道消息",
                            MsgID = Guid.NewGuid().ToString("N")
                        });
                        Console.WriteLine("PushChannelMessage:{0}", res.Result.Code);
                    }
                    break;
            }
            //imClient.SendToChannel("1", s, 0);
            //imClient.SendToGroup(new SendGroupMessage
            //{
            //    Content = "group",
            //    GroupID = "1"
            //});

            //AdminMessage message = new AdminMessage();
            //message.Receiver = "41f7d5a8942ba6956a9e6e9d70aaf7a8";
            //message.Category = PackageCategory.ReceivedChannelMsg;
            //var channelMessage = new ReceivedChannelMessage();
            //channelMessage.Content = "test";
            //message.MessageContent = channelMessage.ToByteString();
            //imClient.AdminSend(message);
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
        }

        private static void login(ImClient imClient, string userId)
        {
            var isAdmin = true;
            var secret = "41f7d5a8942ba6956a9e6e9d70aaf7a8";
            var token = Md5($"www.bmmgo.com{userId}{secret}{(isAdmin ? "1" : "")}").ToLower();
            imClient.Login("www.bmmgo.com", userId, token, isAdmin);
        }

        private static string Md5(string from)
        {
            return System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile(from, "MD5");
        }

        private static void ImClient_OnLogin(ImClient arg1, bool arg2, string arg3)
        {
            if (arg2)
            {
                Console.WriteLine("login success");
                //arg1.BindToChannel("1");
                //arg1.BindToGroup("1");
                //arg1.SubUserLogin();
            }
            else
            {
                Console.WriteLine("login failed:{0}", arg3);
            }
        }

    }
}
