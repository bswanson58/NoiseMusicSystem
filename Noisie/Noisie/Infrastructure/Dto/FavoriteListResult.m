//
//  FavoriteListResult.m
//  Noisie
//
//  Created by William Swanson on 11/17/11.
//  Copyright (c) 2011 Secret Squirrel Products. All rights reserved.
//

#import "FavoriteListResult.h"

@implementation FavoriteListResult

@synthesize Favorites;

- (void) dealloc {
    self.Favorites = nil;
    
    [super dealloc];
}

@end
