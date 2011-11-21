//
//  ServerResultDelegate.m
//  Noisie
//
//  Created by William Swanson on 11/21/11.
//  Copyright (c) 2011 Secret Squirrel Products. All rights reserved.
//

#import "ServerResultDelegate.h"

@implementation ServerResultDelegate

@synthesize mBlockPtr;

- (id) initWithServerResultBlock:(OnServerResultBlock) blockPtr {
    self = [super init];
    if( self ) {
        self.mBlockPtr = blockPtr;
    }
    
    return( self );
}

- (void) dealloc {
    [mBlockPtr release];
    
    [super dealloc];
}

- (void)objectLoader:(RKObjectLoader *)objectLoader didLoadObjects:(NSArray *)objects {
    BaseServerResult *result = [objects objectAtIndex:0];
    
    mBlockPtr( result );
}

@end
