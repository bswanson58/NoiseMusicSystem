//
//  TrackListResult.m
//  Noisie
//
//  Created by William Swanson on 11/15/11.
//  Copyright (c) 2011 Secret Squirrel Products. All rights reserved.
//

#import "TrackListResult.h"

@implementation TrackListResult

@synthesize ArtistId;
@synthesize AlbumId;
@synthesize Tracks;

- (void) dealloc {
    self.ArtistId = nil;
    self.AlbumId = nil;
    self.Tracks = nil;
    
    [super dealloc];
}

@end
