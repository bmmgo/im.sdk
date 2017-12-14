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
-(void)stop;
-(void)loginWithAppkey:(NSString *)appkey userId:(NSString *)userId secrect:(NSString *)secrect;
@end

@implementation ImClient

@synthesize ip;
@synthesize port;

- (void)start
{
    simpleSocket = [[SimpleSocket alloc] init];
    simpleSocket.delegate = self;
    [simpleSocket connect:ip on:port];
}

-(void)Receive:(NSData *)data
{
    NSLog(@"Receive");
}

-(void)Disconnected
{
    NSLog(@"Disconnected");
}

-(void)Connected
{
    NSLog(@"connect success");
}

-(void)loginWithAppkey:(NSString *)appkey userId:(NSString *)userId secrect:(NSString *)secrect{

}

@end
