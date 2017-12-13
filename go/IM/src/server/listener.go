// listener
package server

import (
	"bnet"
	"contact"
	"encoding/json"
	"fmt"
	"net"
)

type listener struct{}

var Ip string
var Port int

func StartAccept(ip string, port int) {
	Ip = ip
	Port = port
	go acceptInternal()
}

func acceptInternal() {
	ln, err := net.Listen("tcp", fmt.Sprintf("%s:%d", Ip, Port))
	if err != nil {
		fmt.Println(err.Error())
		return
	}
	for {
		conn, err := ln.Accept()
		if err != nil {
			fmt.Println(err.Error())
		}
		go handleConnection(conn)
	}
}

func handleConnection(conn net.Conn) {
	var connection bnet.Connection
	connection.SetCompress(false, true)
	connection.Wrap(conn, &listener{})
	// 发送session info
	var si contact.SessionInfo
	si.ConnectionId = connection.ConnID
	si.ConnectorAddress = Ip
	si.ConnectorPort = Port
	si.ClientAddress = connection.Ip
	si.ClientPort = connection.Port
	buf, _ := json.Marshal(si)
	fmt.Println(string(buf))
	fb := contact.SocketResult{ResultCode: 3, ResultData: string(buf)}
	fbBuf, _ := json.Marshal(fb)
	fmt.Println(string(fbBuf))
	connection.SendBuffer(contact.SERVERFEEDBACK, fbBuf)
}

func (l *listener) OnReceived(connection *bnet.Connection, data *bnet.BufferContext) {
	process(connection, data)
}
func (l *listener) OnError(connection *bnet.Connection, err error) {
	fmt.Println(connection, err.Error())
}

func process(connection *bnet.Connection, context *bnet.BufferContext) {
	switch context.Protocol {
	case contact.Heart:
		fmt.Println("心跳")
		break
	case contact.Login:

		break
	case contact.MessageFromServer:
		fmt.Println("消息")

		break
	}
	fmt.Println(context)
}
