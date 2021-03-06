//
//  AppDelegate.m
//  Noisie
//
//  Created by William Swanson on 11/13/11.
//  Copyright (c) 2011 Secret Squirrel Products. All rights reserved.
//

#import "AppDelegate.h"
#import "StartupViewController.h"
#import "MainViewController.h"
#import "ArtistListController.h"
#import "ArtistViewController.h"
#import "RemoteMgr.h"
#import "Events.h"

@interface AppDelegate ()

@property (nonatomic, retain)   RemoteMgr               *mManager;
@property (nonatomic, retain)   StartupViewController   *mStartupViewController;
@property (nonatomic, retain)   MainViewController      *mMainViewController;

- (void) startup;
- (void) onServerConnected:(NSNotification *) notification;

@end

@implementation AppDelegate

@synthesize window = _window;
@synthesize mManager;
@synthesize mStartupViewController;
@synthesize mMainViewController;

- (void)dealloc {
    [_window release];
    self.mManager = nil;
    self.mStartupViewController = nil;
    self.mMainViewController = nil;
    
    [super dealloc];
}

- (BOOL)application:(UIApplication *)application didFinishLaunchingWithOptions:(NSDictionary *)launchOptions {
    self.window = [[[UIWindow alloc] initWithFrame:[[UIScreen mainScreen] bounds]] autorelease];
    // Override point for customization after application launch.
    self.window.backgroundColor = [UIColor whiteColor];
    [self.window makeKeyAndVisible];
    
    self.mStartupViewController = [[[StartupViewController alloc] initWithNibName:nil bundle:nil] autorelease];
    self.mMainViewController = [[[MainViewController alloc] initWithNibName:nil bundle:nil] autorelease];
    
    [[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(onServerConnected:) name:EventServerConnected object:nil];
    
    self.mManager = [[[RemoteMgr alloc] init] autorelease];
    
    return YES;
}

- (void) startup {
    [self.window setRootViewController:self.mStartupViewController];
    [self.mManager startDiscovery];
}

- (void) onServerConnected:(NSNotification *)notification {
    [self.window setRootViewController:self.mMainViewController];
}

- (void)applicationWillResignActive:(UIApplication *)application {
    /*
     Sent when the application is about to move from active to inactive state. This can occur for certain types of temporary interruptions (such as an incoming phone call or SMS message) or when the user quits the application and it begins the transition to the background state.
     Use this method to pause ongoing tasks, disable timers, and throttle down OpenGL ES frame rates. Games should use this method to pause the game.
     */
}

- (void)applicationDidEnterBackground:(UIApplication *)application {
    /*
     Use this method to release shared resources, save user data, invalidate timers, and store enough application state information to restore your application to its current state in case it is terminated later. 
     If your application supports background execution, this method is called instead of applicationWillTerminate: when the user quits.
     */
}

- (void)applicationWillEnterForeground:(UIApplication *)application {
    /*
     Called as part of the transition from the background to the inactive state; here you can undo many of the changes made on entering the background.
     */
}

- (void)applicationDidBecomeActive:(UIApplication *)application {
    /*
     Restart any tasks that were paused (or not yet started) while the application was inactive. If the application was previously in the background, optionally refresh the user interface.
     */
    [self startup];
}

- (void)applicationWillTerminate:(UIApplication *)application {
    /*
     Called when the application is about to terminate.
     Save data if appropriate.
     See also applicationDidEnterBackground:.
     */
}

@end
