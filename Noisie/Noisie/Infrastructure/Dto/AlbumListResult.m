//
//  AlbumListResult.m
//  Noisie
//
//  Created by William Swanson on 11/14/11.
//  Copyright (c) 2011 Secret Squirrel Products. All rights reserved.
//

#import "AlbumListResult.h"

@implementation AlbumListResult

@synthesize ArtistId;
@synthesize Albums;

- (void) dealloc {
    self.ArtistId = nil;
    self.Albums = nil;
    
    [super dealloc];
}

@end
