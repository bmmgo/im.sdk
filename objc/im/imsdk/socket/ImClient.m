//
//  ImClient.m
//  imsdk
//
//  Created by zhangbin on 2017/12/14.
//  Copyright © 2017年 bmmgo. All rights reserved.
//

#import "ImClient.h"

@implementation ImClient
{
    SimpleSocket *simpleSocket;
    FixLengthFrameConverter *frameConverter;
    bool stoped;
    NSOperationQueue *reconnectQueue;
    bool logined;
    int connecteFailedTimes;
}

@synthesize ip;
@synthesize port;
@synthesize appkey;
@synthesize userId;
@synthesize token;
@synthesize delegate;

-(instancetype)init{
    self = [super init];
    if(self){
        connecteFailedTimes = 0;
        reconnectQueue = [NSOperationQueue new];
        reconnectQueue.maxConcurrentOperationCount = 1;
        simpleSocket = [[SimpleSocket alloc] init];
        frameConverter = [FixLengthFrameConverter new];
        simpleSocket.delegate = self;
    }
    return self;
}

- (void)start
{
    [reconnectQueue addOperationWithBlock:^{
        stoped = NO;
    }];
    [self postConnect];
}

-(void)postConnect{
    [reconnectQueue addOperationWithBlock:^{
        if(stoped)return;
        if(connecteFailedTimes >=10){
            //重连超过10次，退出
            connecteFailedTimes = 0;
            stoped = YES;
            return;
        }
        if(connecteFailedTimes != 0){
            //不是第一次连接，需要停顿一下
            [NSThread sleepForTimeInterval:5];
        }
        connecteFailedTimes ++;
        NSLog(@"imsdk: reconnectQueue.operationCount %lu",(unsigned long)reconnectQueue.operationCount);
        [simpleSocket connect:ip on:port];
    }];
}

-(void)stop{
    NSLog(@"imsdk: stop");
    [reconnectQueue addOperationWithBlock:^{
        stoped = YES;
    }];
    [simpleSocket close];
}

-(void)disconnected
{
    NSLog(@"imsdk: Disconnected");
    [frameConverter reset];
    [self postConnect];
}

-(void)connected
{
    NSLog(@"imsdk: connect success");
    connecteFailedTimes = 0;
    [[self delegate] connected];
    if(logined)
        [self login];
}

-(void)login
{
    LoginToken *token = [LoginToken new];
    token.appkey = self.appkey;
    token.userId = self.userId;
    token.token = self.token;
    token.versionCode = self.versionCode;
    token.versionName = self.versionName;
    [self send:PackageCategory_Login with:token.data];
}

-(void)logout{
    self.appkey = nil;
    self.userId = nil;
    self.token = nil;
    logined = false;
    [self send:PackageCategory_Logout with:nil];
}

-(void)bindToChannel:(NSString *)channel{
    Channel *ch=[Channel new];
    ch.channelId = channel;
    [self send:PackageCategory_BindToChannel with:ch.data];
    NSLog(@"imsdk: bind to channel:%@",channel);
}

-(void)unbindToChannel:(NSString *)channel{
    Channel *ch=[Channel new];
    ch.channelId = channel;
    [self send:PackageCategory_UnbindToChannel with:ch.data];
    NSLog(@"imsdk: unbind to channel:%@",channel);
}

-(void)sendToChannel:(SendChannelMessage *)message{
    [self send:PackageCategory_SendToChannel with:message.data];
    NSLog(@"imsdk: send to channel:%@",message.content);
}

-(void)sendToUser:(SendUserMessage *)message{
    [self send:PackageCategory_SendToUser with:message.data];
    NSLog(@"imsdk: bind to user:%@",message.content);
}

-(void)bindToGroup:(NSString *)groupId{
    UserGroup *userGroup = [UserGroup new];
    [[userGroup groupIdsArray] addObject:groupId];
    [self send:PackageCategory_BindToGroup with:userGroup.data];
}

-(void)unbindToGroup:(NSString *)groupId{
    UserGroup *userGroup = [UserGroup new];
    [[userGroup groupIdsArray] addObject:groupId];
    [self send:PackageCategory_UnbindToGroup with:userGroup.data];
}

-(void)sendToGroup:(SendGroupMessage *)message{
    [self send:PackageCategory_SendToGroup with:message.data];
}

-(void)receive:(NSData *)data
{
    [frameConverter append:data];
    while (true) {
        NSData *packageData = [frameConverter decode];
        if(packageData == nil){
            break;
        }
        SocketPackage *package = [SocketPackage parseFromData:packageData error:nil];
        [self process:package];
    }
}

-(void)process:(SocketPackage *)package{
    switch (package.category) {
        case PackageCategory_ReceivedChannelMsg:
        {
            if([delegate respondsToSelector:@selector(receivedChannelMessage:)] == YES ){
                ReceivedChannelMessage *msg = [ReceivedChannelMessage parseFromData:package.content error:nil];
                NSLog(@"imsdk: received channel msg:%@",msg.content);
                [[self delegate] receivedChannelMessage:msg];
            }
            break;
        }
        case PackageCategory_ReceivedUserMsg:
            if([delegate respondsToSelector:@selector(receivedUserMessage:)]==YES){
                ReceivedUserMessage *msg = [ReceivedUserMessage parseFromData:package.content error:nil];
                NSLog(@"rimsdk: eceived user msg:%@",msg);
                [[self delegate] receivedUserMessage:msg];
            }
            break;
        case PackageCategory_ReceivedGroupMsg:
            if([delegate respondsToSelector:@selector(receivedGroupMessage:)]==YES){
                ReceivedGroupMessage *msg= [ReceivedGroupMessage parseFromData:package.content error:nil];
                NSLog(@"imsdk: received group msg:%@",msg.content);
                [[self delegate] receivedGroupMessage:[ReceivedGroupMessage parseFromData:package.content error:nil]];
            }
            break;
        case PackageCategory_Result:
            [self processResult:package];
            break;
    }
}

-(void)processResult:(SocketPackage *)package{
    SocketResult *result = [SocketResult parseFromData:package.content error:nil];
    switch (result.category) {
        case PackageCategory_Login:
            if(result.code == ResultCode_Success){
                [self startHeart];
                logined = YES;
                NSLog(@"imsdk: login success");
                [[self delegate] loginResult:YES message:nil];
            }else{
                logined = NO;
                NSLog(@"imsdk: login failed");
                [[self delegate] loginResult:NO message:result.message];
            }
            break;
        case PackageCategory_BindToChannel:
            NSLog(@"imsdk: bind to channel success");
            break;
    }
}

//启动心跳
-(void)startHeart{
    
}

-(void)send:(PackageCategory)category with:(NSData *)data
{
    SocketPackage *package = [SocketPackage new];
    package.category = category;
    package.seq = 0;
    package.content = data;
    [simpleSocket send:[frameConverter encode:package.data]];
}
@end

