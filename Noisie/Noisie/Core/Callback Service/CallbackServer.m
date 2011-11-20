//
//  CallbackServer.m
//  Noisie
//
//  Created by William Swanson on 11/20/11.
//  Copyright (c) 2011 Secret Squirrel Products. All rights reserved.
//

#import "CallbackServer.h"
#import "HTTPServer.h"

@interface CallbackServer ()

@property (retain, nonatomic)   HTTPServer  *mServer;

@end

@implementation CallbackServer

@synthesize mServer;

- (void) dealloc {
    self.mServer = nil;
    
    [super dealloc];
}

- (void) initializeServer {
    self.mServer = [[[HTTPServer alloc] init] autorelease];

    [self.mServer setName:@"Noisie"];
    [self.mServer setPort:6502];
    [self.mServer setType:@"_http._tcp."];

	NSError *error;
	if(![self.mServer start:&error]) {
		NSLog( @"Error starting HTTP Server: %@", error );
	}
}

@end
