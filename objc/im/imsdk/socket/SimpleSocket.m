//
//  SimpleSocket.m
//  imsdk
//
//  Created by zhangbin on 2017/12/14.
//  Copyright © 2017年 bmmgo. All rights reserved.
//

#import "SimpleSocket.h"

@implementation SimpleSocket

@synthesize delegate;

- (void)connect:(NSString *) ip on:(int) port
{
    NSLog(@"%@", [NSString stringWithFormat:@"start connect to ip:%@ port:%d\n", ip, port]);
    
    [NSStream getStreamsToHostWithName:ip port:port inputStream:&inputStream outputStream:&outputStream];
    
    inputStream.delegate = outputStream.delegate = self;
    
    [inputStream scheduleInRunLoop:[NSRunLoop mainRunLoop] forMode:NSDefaultRunLoopMode];
    [outputStream scheduleInRunLoop:[NSRunLoop mainRunLoop] forMode:NSDefaultRunLoopMode];
    
    [inputStream open];
    [outputStream open];
}

- (void)stream:(NSStream *)aStream handleEvent:(NSStreamEvent)eventCode
{
    switch (eventCode)
    {
        case NSStreamEventEndEncountered:
        {
            NSLog(@"socket closed");
            [inputStream close];
            [outputStream close];
            [inputStream removeFromRunLoop:[NSRunLoop mainRunLoop] forMode:NSDefaultRunLoopMode];
            [outputStream removeFromRunLoop:[NSRunLoop mainRunLoop] forMode:NSDefaultRunLoopMode];
            break;
        }
        case NSStreamEventErrorOccurred:
            NSLog(@"socket error");
            break;
        case NSStreamEventHasBytesAvailable:
            [self receivedData];
            break;
        case NSStreamEventOpenCompleted:
            //NSLog(@"connect success");
            [self.delegate Connected];
            break;
        case NSStreamEventHasSpaceAvailable:
            NSLog(@"ready for send");
            break;
        case NSStreamEventNone:
        default:
            NSLog(@"socket event none");
            break;
    }
}

-(void)receivedData
{
    NSLog(@"socket read data");
    uint8_t buf[1024];
    NSInteger len = [inputStream read:buf maxLength:sizeof(buf)];
    
    NSData *data = [NSData dataWithBytes:buf length:len];
    [self.delegate Receive:data];
    //NSString *msg = [[NSString alloc] initWithData:data encoding:NSUTF8StringEncoding];
}

-(bool)send:(NSData *)data{
    [outputStream write:data.bytes maxLength:data.length];
    return YES;
}
@end