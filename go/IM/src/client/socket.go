// socket
package client

import (
	//	"bufio"
	"bnet"
	"contact"
	"fmt"
	"net"
	//	"strconv"
	"encoding/json"
	"sync"
	"time"
)

var connection bnet.Connection

//  read
var readCache []byte = make([]byte, 4096*2, 4096*2)
var readBuffer []byte = make([]byte, 4096, 4096)
var cachedCount int16 = 0

var syncObj sync.Mutex
var count int32

type listener struct {
}

func (l *listener) OnReceived(connection *bnet.Connection, data *bnet.BufferContext) {
	process(data)
}
func (l *listener) OnError(connection *bnet.Connection, err error) {
	fmt.Println(err.Error())
}

func Connect(ip string, port int) {
	conn, err := net.Dial("tcp", fmt.Sprintf("%s:%d", ip, port))
	if err != nil {
		panic(err)
	}
	fmt.Println("socket连接成功")
	connection.SetCompress(true, false)
	connection.Wrap(conn, &listener{})
}

func process(context *bnet.BufferContext) {
	switch context.Protocol {
	case contact.Heart:
		fmt.Println("心跳")
		break
	case contact.Login:
		onLoginFeedback(context)
		break
	case contact.MessageFromServer:
		fmt.Println("消息")
		syncObj.Lock()
		count++
		syncObj.Unlock()
		break
	case contact.SERVERFEEDBACK:
		fmt.Println("服务器反馈")
		onServerFeedback(context)
		break
	}
}

func onLoginFeedback(context *bnet.BufferContext) {
	var res contact.SocketResult
	err := json.Unmarshal([]byte(context.Context), &res)
	if err != nil {
		fmt.Println(err.Error())
	}
	if res.ResultCode == 0 {
		fmt.Println("登陆成功")
		startHeart()
	} else {
		fmt.Println(res.Message)
	}
}

func onServerFeedback(context *bnet.BufferContext) {
	var res contact.SocketResult
	err := json.Unmarshal([]byte(context.Context), &res)
	if err != nil {
		fmt.Println(err.Error())
	}
	if res.ResultCode == 3 {
		//  收到session info
		token := new(contact.IMToken)
		token.IMUserID = "faad3c1f795f4a5890b51b48b6413dc5"
		token.Token = "/9RRvuOxfwN7QhvBgUNM4tPgin6pPJcxRu3FyaJgqEqp/YYEuwozid3lpXQ2MsXNjcgEULP4lvmz0n98qZ/xSy+oY5EvcQQuGLG8iLRkWI2Gx3xOxBvbRv9YSiNGXyNm"
		buf, _ := json.Marshal(token)
		connection.SendBuffer(contact.Login, buf)
	}
}

func startHeart() {
	for {
		time.Sleep(60)
		connection.SendBuffer(contact.Heart, []byte{})
	}
}
