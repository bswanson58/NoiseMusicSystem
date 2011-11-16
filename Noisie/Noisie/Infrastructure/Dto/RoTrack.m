//
//  RoTrack.m
//  Noisie
//
//  Created by William Swanson on 11/15/11.
//  Copyright (c) 2011 Secret Squirrel Products. All rights reserved.
//

#import "RoTrack.h"

@implementation RoTrack

@synthesize Name;
@synthesize ArtistId;
@synthesize AlbumId;
@synthesize DurationMilliseconds;
@synthesize Rating;
@synthesize TrackNumber;
@synthesize VolumeName;
@synthesize IsFavorite;

- (void) dealloc {
    self.Name = nil;
    self.ArtistId = nil;
    self.AlbumId = nil;
    self.VolumeName = nil;
    self.IsFavorite = nil;
    
    [super dealloc];
}

@end
