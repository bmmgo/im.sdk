// model
package bnet

import (
	"bnet/tools"
)

type BufferContext struct {
	Protocol byte
	Context  string
}

func (c BufferContext) GetBytes(compress bool) []byte {
	cBuf := tools.ToBytes(c.Context, compress)
	return tools.FormatBuffer(c.Protocol, cBuf)
}
