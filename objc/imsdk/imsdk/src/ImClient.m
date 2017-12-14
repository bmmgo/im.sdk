#import <Foundation/Foundation.h>
#import "SimpleSocket.m"

@interface ImClient : NSObject
{
@public
    NSString *ip;
@public
    int port;
    SimpleSocket *simpleSocket;
}
@property(copy) NSString *ip;
@property int port;
-(void)start;
@end

@implementation ImClient

@synthesize ip;
@synthesize port;

- (void)start
{
    simpleSocket = [[SimpleSocket alloc] init];
    [simpleSocket connect:ip on:port];
}

@end
