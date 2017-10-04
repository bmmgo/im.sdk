package com.bmmgo.im.sdk.listener;

import com.google.protobuf.InvalidProtocolBufferException;

import IM.Protocol.ImProto;

/**
 * Created by bing on 2017-10-04.
 */

public interface ReceiveListener {
    void onReceived(ImProto.SocketPackage socketPackage) throws InvalidProtocolBufferException;
}
