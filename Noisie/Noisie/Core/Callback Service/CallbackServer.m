//
//  CallbackServer.m
//  Noisie
//
//  Created by William Swanson on 11/20/11.
//  Copyright (c) 2011 Secret Squirrel Products. All rights reserved.
//

#import "CallbackServer.h"
#import "CallbackHttpServer.h"
#import "CallbackHttpConnection.h"
#import "UriHandlers.h"

@interface CallbackServer ()

@property (retain, nonatomic)   CallbackHttpServer  *mServer;
@property (retain, nonatomic)   UriHandlers         *mUriHandlers;

@end

@implementation CallbackServer

@synthesize mServer;
@synthesize mUriHandlers;

- (void) dealloc {
    self.mServer = nil;
    self.mUriHandlers = nil;
    
    [super dealloc];
}

- (void) initializeServer {
    self.mServer = [[[CallbackHttpServer alloc] init] autorelease];
    self.mUriHandlers = [[[UriHandlers alloc] init] autorelease];

    [self.mServer setName:@"Noisie"];
    [self.mServer setPort:6502];
    [self.mServer setType:@"_Noisie._tcp."];
    [self.mServer setConnectionClass:[CallbackHttpConnection class]];
    
    [self.mServer.UriDispatcher registerScheme:@"/eventInQueue" toController:self.mUriHandlers andSelector:@"onQueueEvent:"];
    [self.mServer.UriDispatcher registerScheme:@"/eventInTransport" toController:self.mUriHandlers andSelector:@"onTransportEvent:"];

	NSError *error;
	if(![self.mServer start:&error]) {
		NSLog( @"Error starting HTTP Server: %@", error );
	}
}

@end
