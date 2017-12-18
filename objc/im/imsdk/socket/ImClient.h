//
//  ImClient.h
//  imsdk
//
//  Created by zhangbin on 2017/12/14.
//  Copyright © 2017年 bmmgo. All rights reserved.
//

#import <Foundation/Foundation.h>
#import "SimpleSocket.h"
#import "ImProto.pbobjc.h"
#import "FixLengthFrameConverter.h"

@protocol ImClientDelegate <NSObject>
@optional
-(void)loginResult:(bool) result message:(NSString *)message;
-(void)receivedChannelMessage:(ReceivedChannelMessage *)message;
-(void)receivedUserMessage:(ReceivedUserMessage *)message;
-(void)connected;
@end

@interface ImClient : NSObject<SimpleSocketDelegate>
{
@public
    NSString *ip;
    int port;
    id<ImClientDelegate> delegate;
}
@property(copy) NSString *ip;
@property int port;
@property(nonatomic,retain) id<ImClientDelegate> delegate;
-(void)start;
-(void)stop;
-(void)loginWithAppkey:(NSString *)appkey userId:(NSString *)userId secrect:(NSString *)secrect;
-(void)bindToChannel:(NSString *)channel;
-(void)unbindToChannel:(NSString *)channel;
-(void)sendToChannel:(SendChannelMessage *)message;
-(void)sendToUser:(SendUserMessage *)message;

@end
