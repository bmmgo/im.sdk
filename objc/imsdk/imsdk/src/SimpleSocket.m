#import <Foundation/Foundation.h>

@interface SimpleSocket : NSObject
-(void)connect:(NSString *) ip on:(int) port;
@end

@implementation SimpleSocket

- (void)connect:(NSString *) ip on:(int) port
{
    NSLog([NSString stringWithFormat:@"start connect to ip:%@ port:%d\n", ip, port]);

    CFReadStreamRef read_s;
    CFWriteStreamRef write_s;
    CFStreamCreatePairWithSocketToHost(NULL, (__bridge CFStringRef)ip, port, &read_s, &write_s);

    NSInputStream *_input_s;
    NSOutputStream *_output_s;

    _input_s = (__bridge NSInputStream *)(read_s);
    _output_s = (__bridge NSOutputStream *)(write_s);
    _input_s.delegate = _output_s.delegate = self;

    [_input_s scheduleInRunLoop:[NSRunLoop mainRunLoop] forMode:NSDefaultRunLoopMode];
    [_output_s scheduleInRunLoop:[NSRunLoop mainRunLoop] forMode:NSDefaultRunLoopMode];

    [_input_s open];
    [_output_s open];
}

- (void)stream:(NSStream *)aStream handleEvent:(NSStreamEvent)eventCode
{
    switch (eventCode)
    {
    case NSStreamEventEndEncountered:
    {
        // connect closed
        NSLog(@"关闭输入输出流");
        [_input_s close];
        [_output_s close];
        [_input_s removeFromRunLoop:[NSRunLoop mainRunLoop] forMode:NSDefaultRunLoopMode];
        [_output_s removeFromRunLoop:[NSRunLoop mainRunLoop] forMode:NSDefaultRunLoopMode];
        break;
    }
    case NSStreamEventErrorOccurred:
        break;
    case NSStreamEventHasBytesAvailable:
        [self readData];
        break;
    case NSStreamEventNone:
        break;
    case NSStreamEventOpenCompleted:
        NSLog(@"connect success");
        break;
    case NSStreamEventHasSpaceAvailable:
        // ready for send
        break;
    default:
        break;
    }
}

-(void)readData
{
    uint8_t buf[1024];
    NSInteger len = [_input_s read:buf maxLength:sizeof(buf)];

    NSData *data = [NSData dataWithBytes:buf length:len];
    NSString *msg = [[NSString alloc] initWithData:data encoding:NSUTF8StringEncoding];
}

@end
