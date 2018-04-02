//
//  SimpleSocket.h
//  imsdk
//
//  Created by zhangbin on 2017/12/14.
//  Copyright © 2017年 bmmgo. All rights reserved.
//

#import <Foundation/Foundation.h>
#import "SendLooper.h"
#import "ReceiveLooper.h"

@protocol SimpleSocketDelegate <NSObject>
@required
-(void)receive:(NSData *)data;
@optional
-(void)disconnected;
-(void)connected;
@end

@interface SimpleSocket : NSObject<NSStreamDelegate,SendLoopDelegate,ReceiveLoopDelegate>
{
@public
    id<SimpleSocketDelegate> delegate;
}
@property(nonatomic,retain) id<SimpleSocketDelegate> delegate;
-(void)connect:(NSString *) ip on:(int) port;
-(bool)send:(NSData *)data;
-(void)close;
@end
