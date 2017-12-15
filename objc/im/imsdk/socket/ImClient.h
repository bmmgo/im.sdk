//
//  ImClient.h
//  imsdk
//
//  Created by zhangbin on 2017/12/14.
//  Copyright © 2017年 bmmgo. All rights reserved.
//

#import <Foundation/Foundation.h>
#import "SimpleSocket.h"

@interface ImClient : NSObject<SimpleSocketDelegate>
{
@public
    NSString *ip;
    int port;
}
@property(copy) NSString *ip;
@property int port;
-(void)start;
-(void)stop;
-(void)loginWithAppkey:(NSString *)appkey userId:(NSString *)userId secrect:(NSString *)secrect;
@end
