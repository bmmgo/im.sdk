using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using im.sdk;

namespace im.test.console
{
    class Program
    {
        static void Main(string[] args)
        {
            var imClient = new ImClient();
            imClient.OnConnected += ImClient_OnConnected;
            imClient.OnConnectedFailed += ImClient_OnConnectedFailed;
            imClient.OnDisconnected += ImClient_OnDisconnected;
            imClient.OnError += ImClient_OnError;
            imClient.OnReceivedChannelMessage += ImClient_OnReceivedChannelMessage;
            imClient.Start();
        }

        private static void ImClient_OnReceivedChannelMessage(ImClient im, IM.Protocol.ReceivedChannelMessage msg)
        {
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
            obj.Login(Guid.NewGuid().ToString("N"));
            obj.JoinChannel("1");
        }
    }
}
