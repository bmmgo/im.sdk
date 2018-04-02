//
//  ReceiveLooper.m
//  im
//
//  Created by zhangbin on 2017/12/15.
//  Copyright © 2017年 bmmgo. All rights reserved.
//

#import "ReceiveLooper.h"
#import "StreamState.h"

@implementation ReceiveLooper
{
    NSInputStream *inputStream;
}

@synthesize delegate;

-(ReceiveLooper *)initWithOutputStream:(NSInputStream *)stream
{
    inputStream=stream;
    self.ReceiveState = Wait;
    return self;
}

- (void)stream:(NSStream *)aStream handleEvent:(NSStreamEvent)eventCode
{
    switch (eventCode)
    {
        case NSStreamEventEndEncountered:
        {
            NSLog(@"imsdk: input socket closed");
            [inputStream close];
            [inputStream removeFromRunLoop:[NSRunLoop mainRunLoop] forMode:NSDefaultRunLoopMode];
            break;
        }
        case NSStreamEventErrorOccurred:
            NSLog(@"imsdk: input socket error");
            self.ReceiveState = Error;
            [[self delegate] error:nil];
            break;
        case NSStreamEventHasBytesAvailable:
            [self receivedData];
            break;
        case NSStreamEventOpenCompleted:
            NSLog(@"imsdk: input connect success");
            self.ReceiveState = Success;
            [[self delegate] ready];
            break;
        case NSStreamEventHasSpaceAvailable:
            NSLog(@"imsdk: ready for receive");
            break;
        case NSStreamEventNone:
        default:
            NSLog(@"imsdk: input socket event none");
            break;
    }
}

-(void)receivedData
{
    NSLog(@"imsdk: reading data from socket");
    uint8_t buf[1024];
    NSInteger len = [inputStream read:buf maxLength:sizeof(buf)];
    if(len == -1){
        self.ReceiveState = Error;
        return;
    }
    NSData *data = [NSData dataWithBytes:buf length:len];
    [[self delegate] receive:data];
}

@end
