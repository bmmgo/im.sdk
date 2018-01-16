package com.bmmgo.im.sdk.socket;

import com.bmmgo.im.sdk.listener.ReceiveListener;
import com.bmmgo.im.sdk.listener.SocketListener;
import com.google.protobuf.ByteString;
import com.google.protobuf.GeneratedMessage;
import com.google.protobuf.InvalidProtocolBufferException;

import java.io.IOException;
import java.net.Socket;
import java.util.HashMap;
import java.util.Map;
import java.util.concurrent.ConcurrentHashMap;
import java.util.concurrent.atomic.AtomicInteger;

import IM.Protocol.ImProto;

/**
 * Created by Administrator on 2016/7/13.
 */
public class SocketClient implements ReceiveListener {
    private boolean mClosed = false;
    private Socket mSocket;
    private ContextWriter mWriter;
    private ContextReader mReader;
    private SocketListener mSocketListener;
    private AtomicInteger mSeq = new AtomicInteger();
    private ConcurrentHashMap<Integer, ImProto.SocketPackage> mSendPackages = new ConcurrentHashMap<>();

    private String IP;
    private int Port;

    public SocketClient() {
        mWriter = new ContextWriter();
        mReader = new ContextReader(false, this);
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
            mReader.reset();
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

    private boolean send(final byte[] bytes) {
        boolean flag = true;
        new Thread(new Runnable() {
            @Override
            public void run() {
                sendInternal(bytes);
            }
        }).start();
        return flag;
    }

    private void sendInternal(byte[] bytes) {
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
                .setSeq(mSeq.getAndIncrement())
                .setCategory(category)
                .setContent(message == null ? ByteString.EMPTY : message.toByteString())
                .build();
        mSendPackages.put(sp.getSeq(), sp);
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
        send(ImProto.PackageCategory.BindToChannel, channel);
    }

    public void leaveChannel(String channelId) {
        ImProto.Channel channel = ImProto.Channel
                .newBuilder()
                .setChannelID(channelId)
                .build();
        send(ImProto.PackageCategory.UnbindToChannel, channel);
    }

    public boolean sendToChannel(String channel, String content, int type, Map<String, ByteString> tags) {
        ImProto.SendChannelMessage sendChannelMessage = ImProto.SendChannelMessage
                .newBuilder()
                .setChannelID(channel)
                .setContent(content)
                .setType(type)
                .putAllUserTags(tags == null ? new HashMap<String, ByteString>() : tags)
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

    @Override
    public void onReceived(ImProto.SocketPackage socketPackage) throws InvalidProtocolBufferException {
        switch (socketPackage.getCategory()) {
            case Ping:
                break;
            case ReceivedUserMsg:
                break;
            case ReceivedChannelMsg:
                onReceivedChannelMessage(socketPackage);
                break;
            case ReceivedGroupMsg:
                onReceivedGroupMessage(socketPackage);
                break;
            case Result:
                onReceivedSocketResult(socketPackage);
                break;
            case UNRECOGNIZED:
                break;
        }
    }

    private void onReceivedChannelMessage(ImProto.SocketPackage socketPackage) throws InvalidProtocolBufferException {
        if (mSocketListener == null) return;
        ImProto.ReceivedChannelMessage receivedChannelMessage = ImProto.ReceivedChannelMessage
                .parseFrom(socketPackage.getContent());
        mSocketListener.onReceivedChannelMessage(receivedChannelMessage);
    }

    private void onReceivedGroupMessage(ImProto.SocketPackage socketPackage) throws InvalidProtocolBufferException {
        if (mSocketListener == null) return;
        ImProto.ReceivedGroupMessage receivedGroupMessage = ImProto.ReceivedGroupMessage
                .parseFrom(socketPackage.getContent());
        mSocketListener.onReceivedGroupMessage(receivedGroupMessage);
    }

    public void onReceivedSocketResult(ImProto.SocketPackage socketPackage) throws InvalidProtocolBufferException {
        if (mSocketListener == null) return;
        ImProto.SocketResult socketResult = ImProto.SocketResult
                .parseFrom(socketPackage.getContent());
        switch (socketResult.getCategory()) {
            case Login:
                if (socketResult.getCode() == ImProto.ResultCode.Success) {
                    mSocketListener.onLoginSuccess();
                } else {
                    mSocketListener.onLoginFailed(socketResult.getMessage());
                }
            case Logout:
                break;
            case SendToUser:
                break;
            case SendToChannel:
                break;
            case BindToChannel: {
                if (!mSendPackages.containsKey(socketPackage.getSeq())) return;
                ImProto.SocketPackage p = mSendPackages.get(socketPackage.getSeq());
                mSocketListener.onJoinChannelSuccess(ImProto.Channel.parseFrom(p.getContent()).getChannelID());
                break;
            }
            case UnbindToChannel:
                if (!mSendPackages.containsKey(socketPackage.getSeq())) return;
                ImProto.SocketPackage p = mSendPackages.get(socketPackage.getSeq());
                mSocketListener.onLeaveChannelSuccess(ImProto.Channel.parseFrom(p.getContent()).getChannelID());
                break;
        }
    }
}
