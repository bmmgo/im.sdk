(function () {
    var immodels = {};

    window.imsdk = function () {
        var _url;
        var socket;
        var ths = this;
        this.models = immodels;
        this.onmessage = function (ev) { };
        this.onerror = function (ev) { };
        this.onconnected = function () { };
        this.onconnectfailed = function () { };
        this.ondisconnected = function () { };
        function onwsclose(ev) {
            setTimeout(function () {
                ths.start();
            }, 2000);
        };
        function onwserror(ev) { ths.onerror(JSON.stringify(ev)); };
        function onwsmessage(ev) { ths.onmessage(ev); };
        function onwsopen(ev) { ths.onconnected(); };

        function sendPackage(seq, category, content) {
            var socketPackage = immodels.SocketPackage.create({ Seq: seq, Category: category, Content: content });
            socket.send(immodels.SocketPackage.encode(socketPackage).finish());
        }
        this.init = function (callback) {
            protobuf.load("ImProto.proto", function (err, root) {
                immodels.PackageCategory = root.lookupEnum("IM.Protocol.PackageCategory");
                immodels.SocketPackage = root.lookupType("IM.Protocol.SocketPackage");
                immodels.SocketResult = root.lookupType("IM.Protocol.SocketResult");
                immodels.LoginToken = root.lookupType("IM.Protocol.LoginToken");
                immodels.SendUserMessage = root.lookupType("IM.Protocol.SendUserMessage");
                immodels.ReceivedUserMessage = root.lookupType("IM.Protocol.ReceivedUserMessage");
                immodels.SendChannelMessage = root.lookupType("IM.Protocol.SendChannelMessage");
                immodels.ReceivedChannelMessage = root.lookupType("IM.Protocol.ReceivedChannelMessage");
                immodels.Channel = root.lookupType("IM.Protocol.Channel");
                callback();
            });
        }
        this.setServerUrl = function (url) {
            _url = url;
        }
        this.login = function (uid) {
            var loginToken = immodels.LoginToken.create({ UserID: uid });
            sendPackage(0, 1, immodels.LoginToken.encode(loginToken).finish());
        }
        this.joinChannel = function (channelId) {
            var channel = immodels.Channel.create({ ChannelID: "1" });
            sendPackage(0, 7, immodels.Channel.encode(channel).finish());
        }
        this.sendToChannel = function (channelId, content) {
            var sendChannelMsg = immodels.SendChannelMessage.create();
            sendChannelMsg.ChannelID = channelId;
            sendChannelMsg.Content = content;
            sendPackage(0, 4, immodels.SendChannelMessage.encode(sendChannelMsg).finish());
        }
        this.start = function () {
            var ws = new WebSocket(_url, "im");
            ws.binaryType = "arraybuffer";
            ws.onclose = onwsclose;
            ws.onerror = onwserror;
            ws.onmessage = onwsmessage;
            ws.onopen = onwsopen;
            socket = ws;
        }
    }
})()