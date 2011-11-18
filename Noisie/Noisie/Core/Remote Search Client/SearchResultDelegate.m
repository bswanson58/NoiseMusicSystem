//
//  SearchResultDelegate.m
//  Noisie
//
//  Created by William Swanson on 11/18/11.
//  Copyright (c) 2011 Secret Squirrel Products. All rights reserved.
//

#import "SearchResultDelegate.h"

@implementation SearchResultDelegate

@synthesize mBlockPtr;

- (id) initWithSearchResultBlock:(OnSearchResultBlock) blockPtr {
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
    SearchResult *result = [objects objectAtIndex:0];
    
    mBlockPtr( result );
}

@end
