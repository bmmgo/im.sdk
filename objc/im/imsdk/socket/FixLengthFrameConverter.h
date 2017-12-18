//
//  FixLengthFrameConverter.h
//  im
//
//  Created by zhangbin on 2017/12/18.
//  Copyright © 2017年 bmmgo. All rights reserved.
//

#import <Foundation/Foundation.h>

@interface FixLengthFrameConverter : NSObject

-(NSData *)encode:(NSData *)data;
-(void)append:(NSData *)data;
-(NSData *)decode;
-(void)reset;

@end
