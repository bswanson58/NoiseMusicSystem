//
//  ArtistListCell.m
//  Noisie
//
//  Created by William Swanson on 11/15/11.
//  Copyright (c) 2011 Secret Squirrel Products. All rights reserved.
//

#import "ArtistListCell.h"

@implementation ArtistListCell

@synthesize uiArtistName;
@synthesize uiAlbumCount;
@synthesize uiArtistGenre;
@synthesize uiIsFavorite;

- (void)dealloc {
    [uiArtistName release];
    [uiAlbumCount release];
    [uiArtistGenre release];
    
    [uiIsFavorite release];
    [super dealloc];
}

- (void) setArtist:(RoArtist *)artist {
    [self.uiArtistName setText:artist.Name];
    [self.uiAlbumCount setText:[NSString stringWithFormat:@"%d", artist.AlbumCount]];
    [self.uiArtistGenre setText:artist.Genre];

    if([artist.IsFavorite isEqualToString:@"true"]) {
        [self.uiIsFavorite setHidden:NO];
    }
    else {
        [self.uiIsFavorite setHidden:YES];
    }
}

- (void)setSelected:(BOOL)selected animated:(BOOL)animated {
    [super setSelected:selected animated:animated];

    // Configure the view for the selected state
}
@end
