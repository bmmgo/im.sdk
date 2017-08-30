package com.bmmgo.im.sdk.socket;

import com.bmmgo.im.sdk.listener.SocketListener;
import com.google.protobuf.ByteString;
import com.google.protobuf.GeneratedMessage;

import java.io.IOException;
import java.net.Socket;
import java.util.UUID;

import IM.Protocol.ImProto;

/**
 * Created by Administrator on 2016/7/13.
 */
public class SocketClient {
    private boolean mClosed = false;
    private Socket mSocket;
    private ContextWriter mWriter;
    private ContextReader mReader;
    private SocketListener mSocketListener;

    private String IP;
    private int Port;

    public SocketClient() {
        mWriter = new ContextWriter();
        mReader = new ContextReader(false);
    }

    public void Connect(String ip, int port) {
        IP = ip;
        Port = port;
        mClosed = false;
        new Thread(connector).start();
    }

    public void close() {
        mClosed = true;
        try {
            if (mSocket != null) {
                mSocket.shutdownInput();
                mSocket.close();
            }
        } catch (IOException e) {
            e.printStackTrace();
        }
    }

    Runnable connector = new Runnable() {
        @Override
        public void run() {
            ConnectInternal();
        }
    };

    private void ConnectInternal() {
        if (mClosed) return;
        try {
            mSocket = new Socket(IP, Port);
            StartHeart();
            StartRead();
            if (mSocketListener != null)
                mSocketListener.onConnected(this);
            return;
        } catch (IOException e) {
            raiseOnError(e.getMessage());
        }
        if (mSocketListener != null)
            mSocketListener.onConnectFailed(this);
        try {
            Thread.sleep(3 * 1000);
        } catch (InterruptedException e) {
            e.printStackTrace();
        }
        new Thread(connector).start();
    }

    private void ReadLoop() {
        try {
            while (!mClosed) {
                if (!mReader.ReadPacket(mSocket.getInputStream())) {
                    break;
                }
            }
        } catch (IOException e) {
            raiseOnError(e.getMessage());
        }
        if (mSocketListener != null)
            mSocketListener.onDisconnected(this);
        new Thread(connector).start();
    }

    private boolean send(byte[] bytes) {
        boolean flag = true;
        try {
            mWriter.Write(mSocket.getOutputStream(), bytes);
        } catch (Exception e) {
            flag = false;
            e.printStackTrace();
        }
        if (!flag) {
            if (mSocketListener != null)
                mSocketListener.onDisconnected(this);
            new Thread(connector).start();
        }
        return flag;
    }

    private void StartRead() {
        new Thread(new Runnable() {
            @Override
            public void run() {
                ReadLoop();
            }
        }).start();
    }

    private void StartHeart() {
        new Thread(new Runnable() {
            @Override
            public void run() {
                while (!mClosed) {
                    sendHeart();
                    try {
                        Thread.sleep(60000);
                    } catch (InterruptedException e) {
                        e.printStackTrace();
                    }
                }
            }
        }).start();
    }

    public void setSocketListener(SocketListener listener) {
        mSocketListener = listener;
        mReader.setSocketListener(listener);
    }

    private void raiseOnError(String error) {
        if (mSocketListener != null) {
            mSocketListener.onError(error);
        }
    }

    public void sendHeart() {
        send(ImProto.PackageCategory.Ping, null);
    }

    private boolean send(ImProto.PackageCategory category, GeneratedMessage message) {
        ImProto.SocketPackage sp = ImProto.SocketPackage
                .newBuilder()
                .setCategory(category)
                .setContent(message == null ? ByteString.EMPTY : message.toByteString())
                .build();
        return send(sp.toByteArray());
    }

    public void login(String appkey, String userId, String token) {
        ImProto.LoginToken loginToken = ImProto.LoginToken
                .newBuilder()
                .setAppkey(appkey)
                .setUserID(userId)
                .setToken(token)
                .build();
        send(ImProto.PackageCategory.Login, loginToken);
    }

    public void joinChannel(String channelId) {
        ImProto.Channel channel = ImProto.Channel
                .newBuilder()
                .setChannelID(channelId)
                .build();
        send(ImProto.PackageCategory.JoinChannel, channel);
    }

    public void leaveChannel(String channelId) {
        ImProto.Channel channel = ImProto.Channel
                .newBuilder()
                .setChannelID(channelId)
                .build();
        send(ImProto.PackageCategory.LeaveChannel, channel);
    }

    public boolean sendToChannel(String channel, String content, int type) {
        ImProto.SendChannelMessage sendChannelMessage = ImProto.SendChannelMessage
                .newBuilder()
                .setChannelID(channel)
                .setContent(content)
                .setType(type)
                .build();
        return send(ImProto.PackageCategory.SendToChannel, sendChannelMessage);
    }

    public boolean sendToUser(String user, String content, int type) {
        ImProto.SendUserMessage sendUserMessage = ImProto.SendUserMessage
                .newBuilder()
                .setReceiver(user)
                .setContent(content)
                .setType(type)
                .build();
        return send(ImProto.PackageCategory.SendToUser, sendUserMessage);
    }
}
