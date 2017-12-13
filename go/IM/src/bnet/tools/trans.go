// trans
package tools

import (
	"bytes"
	"compress/gzip"
	"encoding/binary"
	"io/ioutil"
)

func ToShot(buffer []byte, start int16) int16 {
	b_buf := bytes.NewBuffer(buffer[start:(start + 2)])
	var x int16
	binary.Read(b_buf, binary.BigEndian, &x)
	return x
}

func ToString(buffer []byte, start int16, end int16, compress bool) string {
	if start == end {
		return ""
	}
	if compress {
		b := bytes.NewBuffer(buffer[start:end])
		r, _ := gzip.NewReader(b)
		defer r.Close()
		undatas, _ := ioutil.ReadAll(r)
		return string(undatas)
	}
	return string(buffer[start:end])
}

func CompressBytes(buf []byte) []byte {
	b := bytes.NewBuffer([]byte{})
	r := gzip.NewWriter(b)
	defer r.Close()
	r.Write(buf)
	r.Flush()
	return b.Bytes()
}

func ToBytes(s string, compress bool) []byte {
	sBuf := []byte(s)
	if compress {
		return CompressBytes(sBuf)
	}
	return sBuf
}

func FormatBuffer(protocol byte, cBuf []byte) []byte {
	l := len(cBuf) + 3
	lBuf := bytes.NewBuffer([]byte{})
	binary.Write(lBuf, binary.BigEndian, int16(l))
	buff := make([]byte, l)
	copy(buff[0:], lBuf.Bytes())
	buff[2] = protocol
	copy(buff[3:], cBuf)
	return buff
}
