//
//  RoAlbum.m
//  Noisie
//
//  Created by William Swanson on 11/14/11.
//  Copyright (c) 2011 Secret Squirrel Products. All rights reserved.
//

#import "RoAlbum.h"

@implementation RoAlbum

@synthesize Name;
@synthesize ArtistId;
@synthesize Genre;
@synthesize PublishedYear;
@synthesize Rating;
@synthesize TrackCount;
@synthesize IsFavorite;

- (void) dealloc {
    self.Name = nil;
    self.ArtistId = nil;
    self.Genre = nil;
    self.IsFavorite = nil;
    
    [super dealloc];
}

@end
