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
    imClient.ip = @"im.redolphin.cn";
    imClient.port = 16666;
    [imClient start];
}

-(void)loginResult:(bool)result message:(NSString *)message{
    if(result){
        // login success,then bind to channel and send a channel message
        [imClient bindToChannel:@"1"];
    }
}

- (void)didReceiveMemoryWarning {
    [super didReceiveMemoryWarning];
    // Dispose of any resources that can be recreated.
}


@end
