//
//  SendLooper.h
//  im
//
//  Created by zhangbin on 2017/12/15.
//  Copyright © 2017年 bmmgo. All rights reserved.
//

#import <Foundation/Foundation.h>

@protocol SendLoopDelegate<NSObject>
@optional
-(void) ready;
-(void) error:(NSError *)error;
@end

@interface SendLooper : NSObject<NSStreamDelegate>
{
@public
    id<SendLoopDelegate> delegate;
    bool isReady;
}
@property(nonatomic,assign) id<SendLoopDelegate> delegate;
@property(nonatomic) bool isReady;
-(SendLooper *)initWithOutputStream:(NSOutputStream *)stream;
-(bool)send:(NSData *)data;
@end
