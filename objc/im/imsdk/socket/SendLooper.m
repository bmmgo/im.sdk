//
//  SendLooper.m
//  im
//
//  Created by zhangbin on 2017/12/15.
//  Copyright © 2017年 bmmgo. All rights reserved.
//

#import "SendLooper.h"

@implementation SendLooper
{
    NSOutputStream *outputStream;
}

@synthesize delegate;
@synthesize isReady;

-(SendLooper *)initWithOutputStream:(NSOutputStream *)stream
{
    outputStream=stream;
    return self;
}

- (void)stream:(NSStream *)aStream handleEvent:(NSStreamEvent)eventCode
{
    switch (eventCode)
    {
        case NSStreamEventEndEncountered:
        {
            NSLog(@"output socket closed");
            [outputStream close];
            [outputStream removeFromRunLoop:[NSRunLoop mainRunLoop] forMode:NSDefaultRunLoopMode];
            break;
        }
        case NSStreamEventErrorOccurred:
            NSLog(@"output socket error");
            isReady = NO;
            [[self delegate] error:nil];
            break;
        case NSStreamEventHasBytesAvailable:
            //            [self receivedData];
            break;
        case NSStreamEventOpenCompleted:
            NSLog(@"output connect success");
            //            [self.delegate Connected];
            //            isReady = YES;
            //            [[self delegate] ready];
            break;
        case NSStreamEventHasSpaceAvailable:
            NSLog(@"ready for send or send complete");
            if(!isReady)
            {
                isReady = YES;
                [[self delegate] ready];
            }
            break;
        case NSStreamEventNone:
        default:
            NSLog(@"output socket event none");
            break;
    }
}

-(bool)send:(NSData *)data{
    if(!isReady)
        return NO;
    int len = [outputStream write:data.bytes maxLength:data.length];
//    NSString *base64 = [data base64EncodedStringWithOptions:0];
//    NSLog(@"%@", base64);
//    NSLog(@"send %d",len);
    return data.length == len;
}

@end
