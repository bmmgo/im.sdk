//
//  ReceiveLooper.m
//  im
//
//  Created by zhangbin on 2017/12/15.
//  Copyright © 2017年 bmmgo. All rights reserved.
//

#import "ReceiveLooper.h"

@implementation ReceiveLooper
{
    NSInputStream *inputStream;
}

@synthesize delegate;
@synthesize isReady;

-(ReceiveLooper *)initWithOutputStream:(NSInputStream *)stream
{
    inputStream=stream;
    return self;
}

- (void)stream:(NSStream *)aStream handleEvent:(NSStreamEvent)eventCode
{
    switch (eventCode)
    {
        case NSStreamEventEndEncountered:
        {
            NSLog(@"input socket closed");
            [inputStream close];
//            [outputStream close];
            [inputStream removeFromRunLoop:[NSRunLoop mainRunLoop] forMode:NSDefaultRunLoopMode];
//            [outputStream removeFromRunLoop:[NSRunLoop mainRunLoop] forMode:NSDefaultRunLoopMode];
            break;
        }
        case NSStreamEventErrorOccurred:
            NSLog(@"input socket error");
            isReady = NO;
            [[self delegate] error:nil];
            break;
        case NSStreamEventHasBytesAvailable:
            [self receivedData];
            break;
        case NSStreamEventOpenCompleted:
            NSLog(@"input connect success");
//            [self.delegate Connected];
            isReady = YES;
            [[self delegate] ready];
            break;
        case NSStreamEventHasSpaceAvailable:
            NSLog(@"ready for receive");
            break;
        case NSStreamEventNone:
        default:
            NSLog(@"input socket event none");
            break;
    }
}

-(void)receivedData
{
    NSLog(@"reading data from socket");
    uint8_t buf[1024];
    NSInteger len = [inputStream read:buf maxLength:sizeof(buf)];
    if(len == -1){
        isReady = NO;
        return;
    }
    NSData *data = [NSData dataWithBytes:buf length:len];
    [[self delegate] receive:data];
//    [self.delegate Receive:data];
    //NSString *msg = [[NSString alloc] initWithData:data encoding:NSUTF8StringEncoding];
}

@end
