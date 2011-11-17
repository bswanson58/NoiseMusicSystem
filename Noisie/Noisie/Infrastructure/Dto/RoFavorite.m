//
//  RoFavorite.m
//  Noisie
//
//  Created by William Swanson on 11/17/11.
//  Copyright (c) 2011 Secret Squirrel Products. All rights reserved.
//

#import "RoFavorite.h"

@implementation RoFavorite

@synthesize ArtistId;
@synthesize AlbumId;
@synthesize TrackId;
@synthesize Artist;
@synthesize Album;
@synthesize Track;

- (void) dealloc {
    self.ArtistId = nil;
    self.Artist = nil;
    self.AlbumId = nil;
    self.Album = nil;
    self.TrackId = nil;
    self.Track = nil;
    
    [super dealloc];
}

- (NSString *) formattedName {
    NSString    *retValue;
    
    if([self.Track length] > 0 ) {
        retValue = [NSString stringWithFormat:@"%@ (%@/%@)", self.Track, self.Artist, self.Album];
    }
    else if([self.Album length] > 0 ) {
        retValue = [NSString stringWithFormat:@"%@ (%@)", self.Album, self.Artist];
    }
    else if([self.Track length] > 0 ) {
        retValue = [NSString stringWithFormat:@"%@", self.Track];
    }
    
    return( retValue );
}

@end
