using System;
using System.Collections.Generic;
using System.Net;

namespace im.sdk
{
    public class FixLengthConvert
    {
        private byte[] _cache = new byte[1024];
        /// <summary>
        /// cache中剩余字节长度
        /// </summary>
        private int _left;
        /// <summary>
        /// 转换数据
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public virtual byte[] ToFrame(byte[] buffer)
        {
            var contextBuffer = new byte[0];
            if (buffer != null)
            {
                contextBuffer = buffer;
            }

            //获取总大小
            var totalLength = (short)(sizeof(short) + contextBuffer.Length);
            var buf = new byte[totalLength];
            //写入总大小
            totalLength = IPAddress.HostToNetworkOrder(totalLength);
            BitConverter.GetBytes(totalLength).CopyTo(buf, 0);
            //写入消息内容
            if (contextBuffer.Length > 0)
                contextBuffer.CopyTo(buf, 2);
            return buf;
        }
        /// <summary>
        /// 解析数据
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public virtual IEnumerable<byte[]> DecodeFrame(byte[] buffer)
        {
            if (_cache.Length - _left < buffer.Length)
            {
                var cache = new byte[(int)Math.Ceiling((buffer.Length + _left) / 1024f) * 1024];
                Buffer.BlockCopy(_cache, 0, cache, 0, _left);
                _cache = cache;
            }
            Buffer.BlockCopy(buffer, 0, _cache, _left, buffer.Length);
            _left += buffer.Length;
            return DecodeFrameInternal();
        }

        private IEnumerable<byte[]> DecodeFrameInternal()
        {
            var offset = 0;
            while (true)
            {
                var t = _left - offset;
                if (t < 2)
                {
                    break;
                }
                //  前两个字节是包的总长度
                var allLen = BitConverter.ToInt16(_cache, offset);
                allLen = IPAddress.NetworkToHostOrder(allLen);
                if (allLen <= 0)
                    throw new Exception("解包错误");
                //  不够一个包的长度
                if (t < allLen)
                    break;
                var payloadData = new byte[allLen - 2];
                //  大于3个字节的，解析内容
                if (allLen > 2)
                {
                    Buffer.BlockCopy(_cache, offset + 2, payloadData, 0, allLen - 2);
                }
                offset += allLen;
                yield return payloadData;
            }
            _left -= offset;
            Buffer.BlockCopy(_cache, offset, _cache, 0, _left);
        }
    }
}
