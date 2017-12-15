//
//  SimpleSocket.h
//  imsdk
//
//  Created by zhangbin on 2017/12/14.
//  Copyright © 2017年 bmmgo. All rights reserved.
//

#import <Foundation/Foundation.h>

@protocol SimpleSocketDelegate <NSObject>
@required
-(void)Receive:(NSData *)data;
@optional
-(void)Disconnected;
-(void)Connected;
@end

@interface SimpleSocket : NSObject<NSStreamDelegate>
{
    NSInputStream *inputStream;
    NSOutputStream *outputStream;
@public
    id<SimpleSocketDelegate> delegate;
}
@property(nonatomic,retain) id<SimpleSocketDelegate> delegate;
-(void)connect:(NSString *) ip on:(int) port;
-(bool)send:(NSData *)data;
@end
