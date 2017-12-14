//
//  main.m
//  imsdk
//
//  Created by zhangbin on 2017/12/14.
//  Copyright © 2017年 bmmgo. All rights reserved.
//

#import <Foundation/Foundation.h>
#import "src/ImClient.m"

int main(int argc, const char * argv[])
{
    NSAutoreleasePool *pool = [[NSAutoreleasePool alloc] init];
    NSLog(@"Hello imsdk !");
    ImClient *imClient = [ImClient new];
    imClient.ip = @"www.bmmgo.com";
    imClient.port = 16666;
    [imClient start];
    [NSThread sleepForTimeInterval:99999999];
    [pool drain];
    return 0;
}
