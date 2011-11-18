//
//  RoArtistInfo.m
//  Noisie
//
//  Created by William Swanson on 11/18/11.
//  Copyright (c) 2011 Secret Squirrel Products. All rights reserved.
//

#import "RoArtistInfo.h"

@implementation RoArtistInfo

@synthesize ArtistId;
@synthesize Website;
@synthesize Biography;
@synthesize ArtistImage;
@synthesize BandMembers;
@synthesize TopAlbums;
@synthesize SimilarArtists;

- (void) dealloc {
    self.ArtistId = nil;
    self.Biography = nil;
    self.Website = nil;
    self.ArtistImage = nil;
    self.BandMembers = nil;
    self.TopAlbums = nil;
    self.SimilarArtists = nil;
    
    [super dealloc];
}
@end
