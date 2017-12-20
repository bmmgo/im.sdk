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
}

@synthesize ip;
@synthesize port;
@synthesize appkey;
@synthesize userId;
@synthesize token;
@synthesize delegate;

- (void)start
{
    simpleSocket = [[SimpleSocket alloc] init];
    frameConverter = [FixLengthFrameConverter new];
    simpleSocket.delegate = self;
    [simpleSocket connect:ip on:port];
}

-(void)disconnected
{
    NSLog(@"Disconnected");
    [frameConverter reset];
}

-(void)connected
{
    NSLog(@"connect success");
    [[self delegate] connected];
    [self loginWithAppkey:appkey userId:userId secrect:token];
}

-(void)loginWithAppkey:(NSString *)appkey userId:(NSString *)userId secrect:(NSString *)secrect
{
    LoginToken *token = [LoginToken new];
    token.appkey = appkey;
    token.userId = userId;
    token.token = secrect;
    [self send:PackageCategory_Login with:token.data];
}

-(void)bindToChannel:(NSString *)channel{
    Channel *ch=[Channel new];
    ch.channelId = channel;
    [self send:PackageCategory_BindToChannel with:ch.data];
    NSLog(@"bind to channel:%@",channel);
}

-(void)unbindToChannel:(NSString *)channel{
    Channel *ch=[Channel new];
    ch.channelId = channel;
    [self send:PackageCategory_UnbindToChannel with:ch.data];
    NSLog(@"unbind to channel:%@",channel);
}

-(void)sendToChannel:(SendChannelMessage *)message{
    [self send:PackageCategory_SendToChannel with:message.data];
    NSLog(@"send to channel:%@",message.content);
}

-(void)sendToUser:(SendUserMessage *)message{
    [self send:PackageCategory_SendToUser with:message.data];
    NSLog(@"bind to user:%@",message.content);
}

-(void)stop{
    
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
            ReceivedChannelMessage *msg = [ReceivedChannelMessage parseFromData:package.content error:nil];
            if([(NSObject *)delegate respondsToSelector:@selector(receivedChannelMessage:)] == YES ){
                [[self delegate] receivedChannelMessage:msg];
            }
            NSLog(@"received channel msg:%@",msg.content);
            break;
        }
        case PackageCategory_ReceivedUserMsg:
            [[self delegate] receivedUserMessage:[ReceivedUserMessage parseFromData:package.content error:nil]];
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
                [[self delegate] loginResult:YES message:nil];
            }else{
                [[self delegate] loginResult:NO message:result.message];
            }
            break;
        case PackageCategory_BindToChannel:
            NSLog(@"bind to channel success");
            // just for test
            SendChannelMessage *msg=[SendChannelMessage new];
            msg.channelId = @"1";
            msg.content= @"test";
            [self sendToChannel:msg];
            ///////
            break;
            //        case PackageCategory_UnbindToChannel:
            //            NSLog(@"unbind to channel success");
            //            break;
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

