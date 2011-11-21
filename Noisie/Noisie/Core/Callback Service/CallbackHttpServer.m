//
//  CallbackHttpServer.m
//  Noisie
//
//  Created by William Swanson on 11/21/11.
//  Copyright (c) 2011 Secret Squirrel Products. All rights reserved.
//

#import "CallbackHttpServer.h"

@implementation CallbackHttpServer

@synthesize UriDispatcher;

- (id) init {
    self = [super init];
    
    if( self ) {
        self.UriDispatcher = [[[UriDispatcher alloc] init] autorelease];
    }
    
    return( self );
}

- (void) dealloc {
    self.UriDispatcher = nil;
    
    [super dealloc];
}

@end
