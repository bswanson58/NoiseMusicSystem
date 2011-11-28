//
//  SearchListCell.m
//  Noisie
//
//  Created by William Swanson on 11/18/11.
//  Copyright (c) 2011 Secret Squirrel Products. All rights reserved.
//

#import "SearchListCell.h"
#import "Events.h"

@interface SearchListCell ()

@property (retain, nonatomic)   RoSearchResultItem  *mSearchItem;

@end

@implementation SearchListCell

@synthesize uiItemTitle;
@synthesize uiPlayButton;
@synthesize mSearchItem;

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

- (void)dealloc {
    self.mSearchItem = nil;
    self.uiItemTitle = nil;
    self.uiPlayButton = nil;

    [super dealloc];
}

- (void) setSearchItem:(RoSearchResultItem *)searchItem {
    self.mSearchItem = searchItem;
    
    NSNumber    *dbNull = [NSNumber numberWithInt:0];
    
    if((( self.mSearchItem.TrackId != nil ) &&
        (![self.mSearchItem.TrackId isEqualToNumber:dbNull])) ||
       (( self.mSearchItem.AlbumId != nil ) &&
        (![self.mSearchItem.AlbumId isEqualToNumber:dbNull]))) {
        [self.uiPlayButton setHidden:NO];
    }
    else {
        [self.uiPlayButton setHidden:YES];
    }
    
    [self.uiItemTitle setText:[self.mSearchItem formattedTitle]];
}

- (IBAction)cmdPlay:(id)sender {
    if( self.mSearchItem != nil ) {
        NSNumber    *dbNull = [NSNumber numberWithInt:0];
        
        if(( self.mSearchItem.TrackId != nil ) &&
           (![self.mSearchItem.TrackId isEqualToNumber:dbNull])) {
            [[NSNotificationCenter defaultCenter] postNotificationName:EventQueueTrackRequest object:self.mSearchItem.TrackId];
        }
        else if(( self.mSearchItem.AlbumId != nil ) &&
                (![self.mSearchItem.AlbumId isEqualToNumber:dbNull])) {
            [[NSNotificationCenter defaultCenter] postNotificationName:EventQueueAlbumRequest object:self.mSearchItem.AlbumId];
        }
    }
}
@end
