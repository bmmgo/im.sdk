//
//  ReceiveLooper.h
//  im
//
//  Created by zhangbin on 2017/12/15.
//  Copyright © 2017年 bmmgo. All rights reserved.
//

#import <Foundation/Foundation.h>

@protocol ReceiveLoopDelegate<NSObject>
@optional
-(void) ready;
-(void) error:(NSError *)error;
-(void) receive:(NSData *)data;
@end

@interface ReceiveLooper : NSObject<NSStreamDelegate>
{
@public
    id<ReceiveLoopDelegate> delegate;
    bool isReady;
}
@property(nonatomic,assign) id<ReceiveLoopDelegate> delegate;
@property(nonatomic) bool isReady;

-(ReceiveLooper *)initWithOutputStream:(NSInputStream *)stream;

@end
