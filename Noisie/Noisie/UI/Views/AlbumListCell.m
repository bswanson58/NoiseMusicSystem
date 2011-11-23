//
//  AlbumListCell.m
//  Noisie
//
//  Created by William Swanson on 11/17/11.
//  Copyright (c) 2011 Secret Squirrel Products. All rights reserved.
//

#import "AlbumListCell.h"
#import "Events.h"

@interface AlbumListCell ()

@property (retain, nonatomic) RoAlbum  *mAlbum;

@end

@implementation AlbumListCell

@synthesize uiAlbumName;
@synthesize uiTrackCount;
@synthesize uiPublishedYear;
@synthesize uiPublishedLabel;
@synthesize uiIsFavorite;
@synthesize mAlbum;

- (void)dealloc {
    self.mAlbum = nil;
    self.uiAlbumName = nil;
    self.uiTrackCount = nil;
    self.uiPublishedYear = nil;
    self.uiPublishedLabel = nil;
    self.uiIsFavorite = nil;

    [super dealloc];
}

- (void) setAlbum:(RoAlbum *)album {
    self.mAlbum = album;
    
    [self.uiAlbumName setText:self.mAlbum.Name];
    [self.uiTrackCount setText:[NSString stringWithFormat:@"%d", self.mAlbum.TrackCount]];
    
    if( self.mAlbum.PublishedYear > 0 ) {
        if( self.mAlbum.PublishedYear == 1 ) {
            [self.uiPublishedYear setText:@"Various"];
        }
        else {
            [self.uiPublishedYear setText:[NSString stringWithFormat:@"%d", self.mAlbum.PublishedYear]];
        }
        
        [self.uiPublishedYear setHidden:NO];
        [self.uiPublishedLabel setHidden:NO];
    }
    else {
        [self.uiPublishedYear setHidden:YES];
        [self.uiPublishedLabel setHidden:YES];
        
    }
    
    if([album.IsFavorite isEqualToString:@"true"]) {
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

- (IBAction)cmdPlay:(id)sender {
    [[NSNotificationCenter defaultCenter] postNotificationName:EventQueueAlbumRequest object:self.mAlbum];
}
@end
