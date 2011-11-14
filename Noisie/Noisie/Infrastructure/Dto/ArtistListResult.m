//
//  ArtistListResult.m
//  Noisie
//
//  Created by William Swanson on 11/13/11.
//  Copyright (c) 2011 Secret Squirrel Products. All rights reserved.
//

#import "ArtistListResult.h"

@implementation ArtistListResult

@synthesize Artists;

- (void) dealloc {
    self.Artists = nil;
    
    [super dealloc];
}

@end
