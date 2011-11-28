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
    NSString    *retValue = @"";
    
    if([self.TrackName length] > 0 ) {
        retValue = [NSString stringWithFormat:@"Track: %@ (%@/%@)", self.TrackName, self.ArtistName, self.AlbumName];
    }
    else if([self.AlbumName length] > 0 ) {
        retValue = [NSString stringWithFormat:@"Album: %@ (%@)", self.AlbumName, self.ArtistName];
    }
    else if([self.ArtistName length] > 0 ) {
        retValue = [NSString stringWithFormat:@"Artist: %@", self.ArtistName];
    }
    else {
        retValue = [NSString stringWithFormat:@"%@", @"(Nothing was found.)"];
    }
    
    return( retValue );
}
@end
