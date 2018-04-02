//
//  SendLooper.m
//  im
//
//  Created by zhangbin on 2017/12/15.
//  Copyright © 2017年 bmmgo. All rights reserved.
//

#import "SendLooper.h"
#import "StreamState.h"

@implementation SendLooper
{
    NSOutputStream *outputStream;
}

@synthesize delegate;

-(SendLooper *)initWithOutputStream:(NSOutputStream *)stream
{
    outputStream=stream;
    self.SendState = Wait;
    return self;
}

- (void)stream:(NSStream *)aStream handleEvent:(NSStreamEvent)eventCode
{
    switch (eventCode)
    {
        case NSStreamEventEndEncountered:
        {
            NSLog(@"imsdk: output socket closed");
            [outputStream close];
            [outputStream removeFromRunLoop:[NSRunLoop mainRunLoop] forMode:NSDefaultRunLoopMode];
            break;
        }
        case NSStreamEventErrorOccurred:
            NSLog(@"imsdk: output socket error");
            self.SendState = Error;
            [[self delegate] error:nil];
            break;
        case NSStreamEventHasBytesAvailable:
            break;
        case NSStreamEventOpenCompleted:
            NSLog(@"imsdk: output connect success");
            break;
        case NSStreamEventHasSpaceAvailable:
            NSLog(@"imsdk: ready for send or send complete");
            if(self.SendState != Success)
            {
                self.SendState = Success;
                [[self delegate] ready];
            }
            break;
        case NSStreamEventNone:
        default:
            NSLog(@"imsdk: output socket event none");
            break;
    }
}

-(bool)send:(NSData *)data{
    if(self.SendState != Success)
        return NO;
    int len = [outputStream write:data.bytes maxLength:data.length];
    return data.length == len;
}

@end
