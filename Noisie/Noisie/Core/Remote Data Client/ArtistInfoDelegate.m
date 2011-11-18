//
//  ArtistInfoDelegate.m
//  Noisie
//
//  Created by William Swanson on 11/18/11.
//  Copyright (c) 2011 Secret Squirrel Products. All rights reserved.
//

#import "ArtistInfoDelegate.h"

@implementation ArtistInfoDelegate

@synthesize mBlockPtr;

- (id) initWithArtistInfoBlock:(OnArtistInfoBlock) blockPtr {
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
    ArtistInfoResult *result = [objects objectAtIndex:0];
    
    mBlockPtr( result );
}

@end
