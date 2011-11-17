//
//  FavoriteListCell.m
//  Noisie
//
//  Created by William Swanson on 11/17/11.
//  Copyright (c) 2011 Secret Squirrel Products. All rights reserved.
//

#import "FavoriteListCell.h"
#import "Events.h"

@implementation FavoriteListCell

@synthesize uiFavoriteName;
@synthesize Favorite;

- (id)initWithStyle:(UITableViewCellStyle)style reuseIdentifier:(NSString *)reuseIdentifier {
    self = [super initWithStyle:style reuseIdentifier:reuseIdentifier];
    if (self) {
        // Initialization code
    }
    return self;
}

- (void)dealloc {
    self.Favorite = nil;
    self.uiFavoriteName = nil;
    
    [super dealloc];
}

- (void)setSelected:(BOOL)selected animated:(BOOL)animated {
    [super setSelected:selected animated:animated];

    // Configure the view for the selected state
}

- (IBAction)cmdPlay:(id)sender {
    if( self.Favorite.TrackId != 0 ) {
        [[NSNotificationCenter defaultCenter] postNotificationName:EventQueueTrackRequest object:self.Favorite.TrackId];
    }
    else if( self.Favorite.AlbumId != 0 ) {
        [[NSNotificationCenter defaultCenter] postNotificationName:EventQueueAlbumRequest object:self.Favorite.AlbumId];
    }
}

@end
