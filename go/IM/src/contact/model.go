// model
package contact

type SocketResult struct {
	ResultCode int
	Message    string
	ResultData string
}

type IMToken struct {
	IMUserID string
	Token    string
}

type SessionInfo struct {
	ConnectionId     string
	ClientAddress    string
	ClientPort       int
	ConnectorAddress string
	ConnectorPort    int
}
