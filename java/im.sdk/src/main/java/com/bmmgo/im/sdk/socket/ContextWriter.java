package com.bmmgo.im.sdk.socket;

import java.io.IOException;
import java.io.OutputStream;
import java.nio.ByteBuffer;

/**
 * Created by Administrator on 2016/7/15.
 */
public class ContextWriter {
    public void Write(OutputStream stream, byte[] bytes) throws IOException {
        byte[] content = bytes == null ? new byte[0] : bytes;
        ByteBuffer bb = ByteBuffer.allocate(content.length + 2);
        bb.putShort((short) (content.length + 2));
        bb.put(content);
        stream.write(bb.array());
    }
}
