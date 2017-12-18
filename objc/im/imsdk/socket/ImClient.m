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

-(void)receive:(NSData *)data
{
    NSLog(@"Receive");
}

-(void)disconnected
{
    NSLog(@"Disconnected");
}

-(void)connected
{
    NSLog(@"connect success");
    [self loginWithAppkey:@"www.bmmgo.com" userId:@"b0086244b5664fbd9501924e6ef98a71" secrect:@"3855d38582d0611a43fd74ecb0e53573"];
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
    short len = NSSwapHostShortToBig(body.length + 2);
    //  2字节长度+body
    NSMutableData *mdata=[[NSMutableData alloc] initWithCapacity:body.length+2];
    [mdata appendBytes:&len length:2];
    [mdata appendBytes:body length:body.length];
    
    [simpleSocket send:mdata];
}
@end
