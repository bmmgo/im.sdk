// connection
package bnet

//	"bufio"
import (
	"bnet/tools"
	"net"
	"strconv"
	"strings"
)

type Connection struct {
	ConnID  string
	Address string
	Ip      string
	Port    int
}

type ConnListener interface {
	OnReceived(con *Connection, data *BufferContext)
	OnError(con *Connection, err error)
}

var connection net.Conn
var lis ConnListener

var recCompress, sendCompress = false, false

//  read
var readCache []byte = make([]byte, 4096*2, 4096*2)
var readBuffer []byte = make([]byte, 4096, 4096)
var cachedCount int16 = 0

//  write

func (c *Connection) Wrap(conn net.Conn, listener ConnListener) {
	c.Address = conn.RemoteAddr().String()
	c.ConnID = tools.GetGuid()
	add := strings.Split(c.Address, ":")
	if len(add) == 2 {
		c.Ip = add[0]
		c.Port, _ = strconv.Atoi(add[1])
	}
	connection = conn
	lis = listener
	go c.receiveAsync(conn)
}

func (c *Connection) SetCompress(receive bool, send bool) {
	recCompress = receive
	sendCompress = send
}

func (c *Connection) receiveAsync(conn net.Conn) {
	for {
		len, err := conn.Read(readBuffer)
		if err != nil {
			lis.OnError(c, err)
			c.dispose()
			return
		}
		c.readPacket(readBuffer, 0, len)
	}
}

func (c *Connection) readPacket(buffer []byte, start int, length int) {
	copy(readCache[cachedCount:], buffer[start:(start+length)])
	c.resolveCache(cachedCount + int16(length))
	c.receiveAsync(connection)
}

func (c *Connection) resolveCache(length int16) {
	var start int16 = 0
	for {
		//  小于最小包长度
		if (start + 3) > length {
			break
		}
		len := tools.ToShot(readCache, start)
		//  不够一个包的长度
		if len > length-start {
			break
		}
		protocol := readCache[start+2]
		context := new(BufferContext)
		context.Protocol = protocol

		context.Context = tools.ToString(readCache, start+3, start+len, recCompress)
		go c.process(context)
		start += len
	}
	//   剩余的移到缓存起始位置
	cachedCount = length - start
	copy(readCache[0:], readCache[start:length])
}

func (c *Connection) process(context *BufferContext) {
	lis.OnReceived(c, context)
}

func (c *Connection) SendContext(context *BufferContext) {
	buffer := context.GetBytes(sendCompress)
	_, err := connection.Write(buffer)
	if err != nil {
		lis.OnError(c, err)
		c.dispose()
		return
	}
}

func (c *Connection) SendBuffer(protocol byte, buffer []byte) {
	buf := buffer
	if sendCompress {
		buf = tools.CompressBytes(buffer)
	}
	buf = tools.FormatBuffer(protocol, buf)
	_, err := connection.Write(buf)
	if err != nil {
		lis.OnError(c, err)
		c.dispose()
		return
	}
}

func (c *Connection) dispose() {
	connection.Close()
}
