package com.bmmgo.im.sdk.utils;

import java.io.ByteArrayInputStream;
import java.io.ByteArrayOutputStream;
import java.io.IOException;
import java.util.zip.GZIPInputStream;
import java.util.zip.GZIPOutputStream;

/**
 * Created by Administrator on 2016/7/14.
 */
public class GZipUtil {
    public static byte[] Compress(byte[] source) throws IOException {
        ByteArrayOutputStream os = new ByteArrayOutputStream();
        GZIPOutputStream stream = new GZIPOutputStream(os);
        stream.write(source);
        byte[] buff = os.toByteArray();
        os.close();
        stream.close();
        return buff;
    }

    public static byte[] Compress(byte[] source, int offset, int count) throws IOException {
        ByteArrayOutputStream os = new ByteArrayOutputStream();
        GZIPOutputStream stream = new GZIPOutputStream(os);
        stream.write(source, offset, count);
        byte[] buff = os.toByteArray();
        os.close();
        stream.close();
        return buff;
    }

    public static byte[] Decompress(byte[] source) throws IOException {
        ByteArrayInputStream is = new ByteArrayInputStream(source);
        GZIPInputStream stream = new GZIPInputStream(is);
        ByteArrayOutputStream os = new ByteArrayOutputStream();
        byte[] buf = new byte[1024];
        int len;
        while ((len = stream.read(buf)) > 0) {
            os.write(buf, 0, len);
        }
        buf = os.toByteArray();
        is.close();
        stream.close();
        os.close();
        return buf;
    }

    public static byte[] Decompress(byte[] source, int offset, int length) throws IOException {
        if (length == 0) return new byte[0];
        ByteArrayInputStream is = new ByteArrayInputStream(source, offset, length);
        GZIPInputStream stream = null;
        try {
            stream = new GZIPInputStream(is);
        } catch (IOException e) {
            e.printStackTrace();
        }
        ByteArrayOutputStream os = new ByteArrayOutputStream();
        byte[] buf = new byte[1024];
        int len;
        while ((len = stream.read(buf)) > 0) {
            os.write(buf, 0, len);
        }
        buf = os.toByteArray();
        try {
            is.close();
            stream.close();
            os.close();
        } catch (IOException e) {
            e.printStackTrace();
        }
        return buf;
    }
}
