//
//  ServerVersionDelegate.m
//  Noisie
//
//  Created by William Swanson on 11/21/11.
//  Copyright (c) 2011 Secret Squirrel Products. All rights reserved.
//

#import "ServerVersionDelegate.h"

@implementation ServerVersionDelegate

@synthesize mBlockPtr;

- (id) initWithServerVersionBlock:(OnServerVersionBlock) blockPtr {
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
    ServerVersion *result = [objects objectAtIndex:0];
    
    mBlockPtr( result );
}

@end
