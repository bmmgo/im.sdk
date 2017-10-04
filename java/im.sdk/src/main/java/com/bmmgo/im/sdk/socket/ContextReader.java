package com.bmmgo.im.sdk.socket;

import com.bmmgo.im.sdk.contact.BufPacket;
import com.bmmgo.im.sdk.listener.ReceiveListener;
import com.bmmgo.im.sdk.listener.SocketListener;
import com.bmmgo.im.sdk.utils.GZipUtil;
import com.google.protobuf.InvalidProtocolBufferException;

import java.io.IOException;
import java.io.InputStream;
import java.nio.ByteBuffer;

import IM.Protocol.ImProto;

/**
 * Created by Administrator on 2016/7/13.
 */
public class ContextReader {
    private byte[] ReadBuffer;
    private int offset = 0;
    private SocketListener mListener;
    private boolean mDecompress = false;
    private ReceiveListener mReceiveListener;

    public ContextReader(boolean decompress, ReceiveListener listener) {
        mDecompress = decompress;
        mReceiveListener = listener;
        ReadBuffer = new byte[Short.MAX_VALUE];
    }

    public void setSocketListener(SocketListener listener) {
        mListener = listener;
    }

    public boolean ReadPacket(InputStream stream) throws IOException {
        int readLen = stream.read(ReadBuffer, offset, Short.MAX_VALUE - offset);
        if (readLen < 0)
            return false;
        int length = readLen + offset;
        int start = 0;
        while (true) {
            if (length == start) {  //  没有剩余数据了
                offset = 0;
                break;
            }
            if (length - start < 2) {  //   最小包长度
                offset = length - start;
                System.arraycopy(ReadBuffer, start, ReadBuffer, 0, offset);
                break;
            }
            short len = ByteBuffer.wrap(ReadBuffer, start, 2).getShort();
            if (len > length - start) {  //  当前包还没有接收完
                offset = length - start;
                System.arraycopy(ReadBuffer, start, ReadBuffer, 0, offset);
                break;
            }
            BufPacket packet = new BufPacket();
            if (mDecompress) {
                packet.ContentBuf = GZipUtil.Decompress(ReadBuffer, start + 2, len - 2);
            } else {
                packet.ContentBuf = new byte[len - 2];
                System.arraycopy(ReadBuffer, start + 2, packet.ContentBuf, 0, packet.ContentBuf.length);
            }
            OnReceivedPacketInternal(packet);
            start += len;
        }
        return true;
    }

    private void OnReceivedPacketInternal(BufPacket packet) throws InvalidProtocolBufferException {
        if (mReceiveListener == null) return;
        ImProto.SocketPackage sp = ImProto.SocketPackage.parseFrom(packet.ContentBuf);
        mReceiveListener.onReceived(sp);
    }
}
