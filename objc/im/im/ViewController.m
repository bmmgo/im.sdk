//
//  ViewController.m
//  im
//
//  Created by zhangbin on 2017/12/15.
//  Copyright © 2017年 bmmgo. All rights reserved.
//

#import "ViewController.h"
#import "ImClient.h"

@interface ViewController ()

@end

@implementation ViewController
{
    ImClient *imClient;
}

- (void)viewDidLoad {
    [super viewDidLoad];
    // Do any additional setup after loading the view, typically from a nib.
    imClient = [ImClient new];
    imClient.delegate = self;
    imClient.ip = @"www.bmmgo.com";
    imClient.port = 16666;
    imClient.appkey = @"www.bmmgo.com";
    imClient.userId = @"b0086244b5664fbd9501924e6ef98a71";
    imClient.token = @"3855d38582d0611a43fd74ecb0e53573";
    
    [imClient start];
}

-(void)loginResult:(bool)result message:(NSString *)message{
    if(result){
        // login success,then bind to channel and send a channel message
        [imClient bindToChannel:@"1"];
        [imClient bindToGroup:@"1"];
    }
}

-(void)receivedUserMessage:(ReceivedUserMessage *)message{}

-(void)receivedChannelMessage:(ReceivedChannelMessage *)message{}

-(void)receivedGroupMessage:(ReceivedGroupMessage *)message{}

- (void)didReceiveMemoryWarning {
    [super didReceiveMemoryWarning];
    // Dispose of any resources that can be recreated.
}

-(void)connected{
    
}

@end
