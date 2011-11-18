//
//  PlayQueueListDelegate.m
//  Noisie
//
//  Created by William Swanson on 11/18/11.
//  Copyright (c) 2011 Secret Squirrel Products. All rights reserved.
//

#import "PlayQueueListDelegate.h"

@implementation PlayQueueListDelegate

@synthesize mBlockPtr;

- (id) initWithPlayQueueListBlock:(OnPlayQueueListBlock) blockPtr {
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
    PlayQueueListResult *result = [objects objectAtIndex:0];
    
    mBlockPtr( result );
}

@end
