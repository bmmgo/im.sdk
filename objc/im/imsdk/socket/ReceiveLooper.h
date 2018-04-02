//
//  ReceiveLooper.h
//  im
//
//  Created by zhangbin on 2017/12/15.
//  Copyright © 2017年 bmmgo. All rights reserved.
//

#import <Foundation/Foundation.h>
#import "StreamState.h"

@protocol ReceiveLoopDelegate<NSObject>
@optional
-(void) ready;
-(void) error:(NSError *)error;
-(void) receive:(NSData *)data;
@end

@interface ReceiveLooper : NSObject<NSStreamDelegate>
{
@public
    __unsafe_unretained id<ReceiveLoopDelegate> delegate;
}
@property(nonatomic,assign) id<ReceiveLoopDelegate> delegate;
@property(nonatomic) enum StreamState ReceiveState;

-(ReceiveLooper *)initWithOutputStream:(NSInputStream *)stream;

@end
