//
//  RoPlayQueueTrack.m
//  Noisie
//
//  Created by William Swanson on 11/18/11.
//  Copyright (c) 2011 Secret Squirrel Products. All rights reserved.
//

#import "RoPlayQueueTrack.h"

@implementation RoPlayQueueTrack

@synthesize TrackId;
@synthesize TrackName;
@synthesize AlbumName;
@synthesize ArtistName;
@synthesize IsPlaying;
@synthesize HasPlayed;
@synthesize IsFaulted;

- (void) dealloc {
    self.TrackId = nil;
    self.TrackName = nil;
    self.AlbumName = nil;
    self.ArtistName = nil;
    self.IsPlaying = nil;
    self.HasPlayed = nil;
    self.IsFaulted = nil;
    
    [super dealloc];
}

- (NSString *) formattedName {
    return([NSString stringWithFormat:@"%@ (%@/%@)", self.TrackName, self.ArtistName, self.AlbumName]);
}

@end
