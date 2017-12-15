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

- (void)viewDidLoad {
    [super viewDidLoad];
    // Do any additional setup after loading the view, typically from a nib.
    ImClient *imClient = [ImClient new];
    imClient.ip = @"im.redolphin.cn";
    imClient.port = 16666;
    [imClient start];
}


- (void)didReceiveMemoryWarning {
    [super didReceiveMemoryWarning];
    // Dispose of any resources that can be recreated.
}


@end
