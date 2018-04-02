//
//  SimpleSocket.m
//  imsdk
//
//  Created by zhangbin on 2017/12/14.
//  Copyright © 2017年 bmmgo. All rights reserved.
//

#import "SimpleSocket.h"
#import "SendLooper.h"
#import "ReceiveLooper.h"
#import "StreamState.h"

@implementation SimpleSocket
{
    SendLooper *sender;
    ReceiveLooper *receiver;
    bool closed;
}

@synthesize delegate;

-(instancetype)init{
    self = [super init];
    if(self){
        closed = NO;
    }
    return self;
}

- (void)connect:(NSString *) ip on:(int) port
{
    NSLog(@"%@", [NSString stringWithFormat:@"imsdk: start connect to ip:%@ port:%d\n", ip, port]);
    
    NSInputStream *inputStream;
    NSOutputStream *outputStream;
    
    [NSStream getStreamsToHostWithName:ip port:port inputStream:&inputStream outputStream:&outputStream];
    
    sender = [[SendLooper alloc] initWithOutputStream:outputStream];
    receiver = [[ReceiveLooper alloc] initWithOutputStream:inputStream];
    
    sender.delegate = self;
    receiver.delegate = self;
    
    inputStream.delegate = receiver;
    outputStream.delegate = sender;
    
    [inputStream scheduleInRunLoop:[NSRunLoop mainRunLoop] forMode:NSDefaultRunLoopMode];
    [outputStream scheduleInRunLoop:[NSRunLoop mainRunLoop] forMode:NSDefaultRunLoopMode];
    
    [inputStream open];
    [outputStream open];
}

-(void)close{
//    closed = YES;
}

-(bool)send:(NSData *)data{
    [sender send:data];
    return YES;
}

-(void)error:(NSError *)error{
    if (sender.SendState == Error && receiver.ReceiveState == Error)
        [[self delegate] disconnected];
}

-(void)receive:(NSData *)data{
    [[self delegate] receive:data];
}

-(void)ready{
    if (sender.SendState == Success && receiver.ReceiveState == Success)
        [[self delegate] connected];
}

@end
