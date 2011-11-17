//
//  AlbumListCell.m
//  Noisie
//
//  Created by William Swanson on 11/17/11.
//  Copyright (c) 2011 Secret Squirrel Products. All rights reserved.
//

#import "AlbumListCell.h"
#import "Events.h"

@implementation AlbumListCell

@synthesize uiAlbumName;
@synthesize uiTrackCount;
@synthesize Album;

- (void)dealloc {
    self.Album = nil;
    self.uiAlbumName = nil;
    self.uiTrackCount = nil;

    [super dealloc];
}

- (id)initWithStyle:(UITableViewCellStyle)style reuseIdentifier:(NSString *)reuseIdentifier {
    self = [super initWithStyle:style reuseIdentifier:reuseIdentifier];
    if (self) {
        // Initialization code
    }
    return self;
}

- (void)setSelected:(BOOL)selected animated:(BOOL)animated {
    [super setSelected:selected animated:animated];

    // Configure the view for the selected state
}

- (IBAction)cmdPlay:(id)sender {
    [[NSNotificationCenter defaultCenter] postNotificationName:EventQueueAlbumRequest object:self.Album];
}
@end
