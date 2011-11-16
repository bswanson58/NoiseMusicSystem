//
//  TrackListDelegate.m
//  Noisie
//
//  Created by William Swanson on 11/15/11.
//  Copyright (c) 2011 Secret Squirrel Products. All rights reserved.
//

#import "TrackListDelegate.h"

@implementation TrackListDelegate

@synthesize mBlockPtr;

- (id) initWithTrackListBlock:(OnTrackListBlock) blockPtr {
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
    TrackListResult *result = [objects objectAtIndex:0];
    
    mBlockPtr( result );
}

@end
