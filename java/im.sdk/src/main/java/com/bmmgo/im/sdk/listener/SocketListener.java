package com.bmmgo.im.sdk.listener;

import com.bmmgo.im.sdk.socket.SocketClient;

import IM.Protocol.ImProto;

/**
 * Created by bing on 2017-08-29.
 */

public interface SocketListener {
    void onConnected(SocketClient socket);

    void onDisconnected(SocketClient socket);

    void onConnectFailed(SocketClient socket);

    void onError(String error);

    void onLoginSuccess();

    void onLoginFailed(String error);

    void onReceivedUserMessage(ImProto.ReceivedUserMessage receivedUserMessage);

    void onReceivedChannelMessage(ImProto.ReceivedChannelMessage receivedChannelMessage);

    void onJoinChannelSuccess(String channelId);

    void onLeaveChannelSuccess(String channelId);
}
