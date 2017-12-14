#import <Foundation/Foundation.h>

@interface SimpleSocket : NSObject
{
    NSInputStream *inputStream;
    NSOutputStream *outputStream;
}
-(void)connect:(NSString *) ip on:(int) port;
@end

@implementation SimpleSocket

- (void)connect:(NSString *) ip on:(int) port
{
    NSLog([NSString stringWithFormat:@"start connect to ip:%@ port:%d\n", ip, port]);

    CFReadStreamRef read_s;
    CFWriteStreamRef write_s;
    CFStreamCreatePairWithSocketToHost(NULL, (__bridge CFStringRef)ip, port, &read_s, &write_s);

    inputStream = (__bridge NSInputStream *)(read_s);
    outputStream = (__bridge NSOutputStream *)(write_s);
    inputStream.delegate = outputStream.delegate = self;

    [inputStream scheduleInRunLoop:[NSRunLoop mainRunLoop] forMode:NSDefaultRunLoopMode];
    [outputStream scheduleInRunLoop:[NSRunLoop mainRunLoop] forMode:NSDefaultRunLoopMode];

    [inputStream open];
    [outputStream open];
}

- (void)stream:(NSStream *)aStream handleEvent:(NSStreamEvent)eventCode
{
    switch (eventCode)
    {
    case NSStreamEventEndEncountered:
    {
        // connect closed
        NSLog(@"关闭输入输出流");
        [inputStream close];
        [outputStream close];
        [inputStream removeFromRunLoop:[NSRunLoop mainRunLoop] forMode:NSDefaultRunLoopMode];
        [outputStream removeFromRunLoop:[NSRunLoop mainRunLoop] forMode:NSDefaultRunLoopMode];
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
    NSInteger len = [inputStream read:buf maxLength:sizeof(buf)];

    NSData *data = [NSData dataWithBytes:buf length:len];
    NSString *msg = [[NSString alloc] initWithData:data encoding:NSUTF8StringEncoding];
}

@end
