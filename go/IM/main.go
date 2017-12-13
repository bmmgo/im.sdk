// IM project main.go
package main

import (
	"client"
	"fmt"
	//	"time"
	"bufio"
	"os"
	"server"
)

func main() {
	switchMode()
	processCmd()
	//	test()
}

func switchMode() {
	fmt.Println("1.启动服务端\t2.启动客户端")
	reader := bufio.NewReader(os.Stdin)
	data, _, _ := reader.ReadLine()
	ipt := string(data)
	switch ipt {
	case "1":
		server.StartAccept("172.16.67.202", 20001)
		return
	case "2":
		client.Connect("172.16.67.202", 20000)
		return
	}
	switchMode()
}

func processCmd() {
	reader := bufio.NewReader(os.Stdin)
	data, _, _ := reader.ReadLine()
	ipt := string(data)
	switch ipt {
	case "stop":
		return
	}
	processCmd()
}

func test() {
	s1 := make([]byte, 20, 40)
	fmt.Println(len(s1))
	s2 := make([]byte, 10, 40)
	copy(s1[20:], s2)
	fmt.Println(len(s1))
	fmt.Println(s1)
}
