//
//  FavoritesListDelegate.m
//  Noisie
//
//  Created by William Swanson on 11/17/11.
//  Copyright (c) 2011 Secret Squirrel Products. All rights reserved.
//

#import "FavoritesListDelegate.h"

@implementation FavoriteListDelegate

@synthesize mBlockPtr;

- (id) initWithFavoriteListBlock:(OnFavoriteListBlock) blockPtr {
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
    FavoriteListResult *result = [objects objectAtIndex:0];
    
    mBlockPtr( result );
}

@end
