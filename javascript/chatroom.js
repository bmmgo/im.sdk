function startim() {
    $("#send").bind("click", function () {
        alert("未连接");
    });
    var im = new imsdk();
    im.init(function () {
        im_init_completed(im);
    });
}
function im_init_completed(im) {
    if (!window.imsdk) {
        alert("加载失败");
        return;
    }
    im.onmessage = function (msg) {
        var socketPackage = im.models.SocketPackage.decode(new Uint8Array(msg.data));
        switch (socketPackage.Category) {
            case 99:
                {
                    var socketResult = im.models.SocketResult.decode(socketPackage.Content);
                    switch (socketResult.Category) {
                        case 1:
                            {
                                if (socketResult.Code === 0) {
                                    im.joinChannel("1");
                                }
                                break;
                            }
                        default:
                            {
                                break;
                            }
                    }
                    break;
                }
            case 6:
                {
                    var receivedChannelMsg = im.models.ReceivedChannelMessage.decode(socketPackage.Content);
                    $("#chatcontainer").append("<p>" + receivedChannelMsg.Content + "</p>");
                    $("#chatcontainer").scrollTop($("#chatcontainer")[0].scrollHeight);
                    break;
                }
            default:
                {
                    $("#chatcontainer").html("message:" + msg);
                    break;
                }
        }
    }
    im.onerror = function (error) {
        $("#chatcontainer").html("error:" + error);
    }
    im.onconnected = function () {
        $("#send").keypress(function (e) {
            var eCode = e.keyCode ? e.keyCode : e.which ? e.which : e.charCode;
            if (eCode === 13) {
                alert('您按了回车键')
            }
        })
        $("#send").unbind("click").bind("click", function () {
            $("#warn_msg").html("");
            var content = $("#ipt_content").val();
            if (content === "")
                $("#warn_msg").html("不能发送空内容！");
            else {
                im.sendToChannel("1", content);
            }
        });
        im.login($("#userid").val());
    }
    im.setServerUrl("ws://www.bmmgo.com:16667");
    im.start();
}