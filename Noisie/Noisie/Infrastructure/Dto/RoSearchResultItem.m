//
//  RoSearchResultItem.m
//  Noisie
//
//  Created by William Swanson on 11/18/11.
//  Copyright (c) 2011 Secret Squirrel Products. All rights reserved.
//

#import "RoSearchResultItem.h"

@implementation RoSearchResultItem

@synthesize TrackId;
@synthesize TrackName;
@synthesize AlbumId;
@synthesize AlbumName;
@synthesize ArtistId;
@synthesize ArtistName;
@synthesize CanPlay;

- (void) dealloc {
    self.TrackId = nil;
    self.TrackName = nil;
    self.AlbumId = nil;
    self.AlbumName = nil;
    self.ArtistId = nil;
    self.ArtistName = nil;
    self.CanPlay = nil;
    
    [super dealloc];
}

- (NSString *) formattedTitle {
    return([NSString stringWithFormat:@"%@ (%@/%@)", self.TrackName, self.ArtistName, self.AlbumName]);
}
@end
