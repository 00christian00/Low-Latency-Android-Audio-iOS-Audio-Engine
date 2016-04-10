//
//  ViewController.m
//  SuperpoweredIOS
// Just a test app to play with the library without passing through Unity every time
//
//  Created by christian brandoni on 06/04/16.
//  Copyright © 2016 Chris. All rights reserved.
//

#include "../../../../../Superpowered4Unity/UnityPlugin/Android/superpoweredunity/src/main/jni/SuperpoweredInternal.h"
#import "ViewController.h"

@interface ViewController ()

@end

@implementation ViewController
SuperpoweredInternal* mSupInternal;
- (void)viewDidLoad {
    [super viewDidLoad];
    mSupInternal= new SuperpoweredInternal("",24000,12);
    mSupInternal->RegisterSound([[[NSBundle mainBundle] pathForResource:@"Samples/Piano 1" ofType:@"wav"] fileSystemRepresentation]);

    // Do any additional setup after loading the view, typically from a nib.
}

- (void)didReceiveMemoryWarning {
    [super didReceiveMemoryWarning];
    // Dispose of any resources that can be recreated.
}

- (IBAction)PlaybtnClicked:(id)sender {
    mSupInternal->Play(0);
}
@end
