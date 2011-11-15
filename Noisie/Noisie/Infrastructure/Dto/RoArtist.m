//
//  RoArtist.m
//  Noisie
//
//  Created by William Swanson on 11/13/11.
//  Copyright (c) 2011 Secret Squirrel Products. All rights reserved.
//

#import "RoArtist.h"

@implementation RoArtist

@synthesize Name;
@synthesize Website;
@synthesize AlbumCount;
@synthesize Rating;
@synthesize Genre;
@synthesize IsFavorite;

- (void) dealloc {
    self.Name = nil;
    self.Genre = nil;
    self.Website = nil;
    self.IsFavorite = nil;
    
    [super dealloc];
}

@end
