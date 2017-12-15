//
//  ImClient.m
//  imsdk
//
//  Created by zhangbin on 2017/12/14.
//  Copyright © 2017年 bmmgo. All rights reserved.
//

#import "ImClient.h"
#import "ImProto.pbobjc.h"

@implementation ImClient
{
    SimpleSocket *simpleSocket;
}

@synthesize ip;
@synthesize port;

- (void)start
{
    simpleSocket = [[SimpleSocket alloc] init];
    simpleSocket.delegate = self;
    [simpleSocket connect:ip on:port];
}

-(void)Receive:(NSData *)data
{
    NSLog(@"Receive");
}

-(void)Disconnected
{
    NSLog(@"Disconnected");
}

-(void)Connected
{
    NSLog(@"connect success");
    [self loginWithAppkey:@"1" userId:@"1" secrect:@"1"];
}

-(void)loginWithAppkey:(NSString *)appkey userId:(NSString *)userId secrect:(NSString *)secrect
{
    LoginToken *token = [LoginToken new];
    token.appkey = appkey;
    token.userId = userId;
    token.token = secrect;
    [self send:PackageCategory_Login with:token.data];
}

-(void)stop{
    
}

-(void)send:(PackageCategory)category with:(NSData *)data
{
    SocketPackage *package = [SocketPackage new];
    package.category = category;
    package.seq = 0;
    package.content = data;
    NSData *body = package.data;
    short len = body.length;
    //  2字节长度+body
    NSMutableData *mdata=[[NSMutableData alloc] initWithCapacity:body.length+2];
    [mdata appendBytes:&len length:2];
    [mdata appendBytes:body length:body.length];
    
    [simpleSocket send:mdata.bytes];
}
@end
