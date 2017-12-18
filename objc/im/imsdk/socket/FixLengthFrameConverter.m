
//  FixLengthFrameConverter.m
//  im
//
//  Created by zhangbin on 2017/12/18.
//  Copyright © 2017年 bmmgo. All rights reserved.
//

#import "FixLengthFrameConverter.h"

@implementation FixLengthFrameConverter
{
    NSMutableData *cache;
    int left;
}

- (instancetype)init
{
    self = [super init];
    if (self) {
        cache = [[NSMutableData alloc] initWithCapacity:1024];
    }
    return self;
}

-(NSData *)encode:(NSData *)data{
    short len = NSSwapHostShortToBig(data.length + 2);
    //  2字节长度+body
    NSMutableData *mdata=[[NSMutableData alloc] initWithCapacity:data.length+2];
    [mdata appendBytes:&len length:2];
    [mdata appendBytes:data.bytes length:data.length];
    return mdata;
}

-(void)append:(NSData *)data{
    [cache appendData:data];
}

-(NSData *)decode{
    if(cache.length < 2)
        return nil;
    short len;
    [cache getBytes:&len length:sizeof(len)];
    len = NSSwapBigShortToHost(len);
    if(cache.length < len)
        return nil;
    NSData *body = [cache subdataWithRange:NSMakeRange(2, len - 2)];
    
    //  remove decode buffer in cache
    if(cache.length == len){
        [cache resetBytesInRange:NSMakeRange(0, len)];
        [cache setLength:0];
//        cache = [[NSMutableData alloc] initWithCapacity:1024];
    }else{
        NSData *left = [cache subdataWithRange:NSMakeRange(len, cache.length - len)];
        [cache resetBytesInRange:NSMakeRange(0, cache.length)];
        [cache setLength:0];
//        cache = [[NSMutableData alloc] initWithCapacity:1024];
        [cache appendData:left];
    }
    
//    [cache resetBytesInRange:NSMakeRange(0, len)];
    return body;
}

-(void)reset{
    left = 0;
}

@end
