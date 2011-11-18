//
//  SearchListCell.m
//  Noisie
//
//  Created by William Swanson on 11/18/11.
//  Copyright (c) 2011 Secret Squirrel Products. All rights reserved.
//

#import "SearchListCell.h"
#import "Events.h"

@implementation SearchListCell

@synthesize uiItemTitle;
@synthesize SearchItem;

- (id)initWithStyle:(UITableViewCellStyle)style reuseIdentifier:(NSString *)reuseIdentifier
{
    self = [super initWithStyle:style reuseIdentifier:reuseIdentifier];
    if (self) {
        // Initialization code
    }
    return self;
}

- (void)setSelected:(BOOL)selected animated:(BOOL)animated
{
    [super setSelected:selected animated:animated];

    // Configure the view for the selected state
}

- (void)dealloc {
    self.SearchItem = nil;
    self.uiItemTitle = nil;
    
    [super dealloc];
}

- (IBAction)cmdPlay:(id)sender {
    if( self.SearchItem != nil ) {
        if( self.SearchItem.TrackId != 0 ) {
            [[NSNotificationCenter defaultCenter] postNotificationName:EventQueueTrackRequest object:self.SearchItem.TrackId];
        }
        else if( self.SearchItem.AlbumId != 0 ) {
            [[NSNotificationCenter defaultCenter] postNotificationName:EventQueueAlbumRequest object:self.SearchItem.AlbumId];
        }
    }
}
@end
