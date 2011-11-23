//
//  TrackListCell.m
//  Noisie
//
//  Created by William Swanson on 11/17/11.
//  Copyright (c) 2011 Secret Squirrel Products. All rights reserved.
//

#import "TrackListCell.h"
#import "Events.h"

@interface TrackListCell ()

@property (retain, nonatomic)   RoTrack     *mTrack;
@property (retain, nonatomic)   NSDateFormatter *mDateFormatter;

@end

@implementation TrackListCell

@synthesize mTrack;
@synthesize mDateFormatter;
@synthesize uiTrackNumber;
@synthesize uiTrackName;
@synthesize uiTrackDuration;
@synthesize uiIsFavorite;

- (void)dealloc {
    self.mTrack = nil;
    self.mDateFormatter = nil;
    self.uiTrackNumber = nil;
    self.uiTrackName = nil;
    self.uiTrackDuration = nil;
    self.uiIsFavorite = nil;
    
    [super dealloc];
}

- (void) setTrack:(RoTrack *)track {
    self.mTrack = track;
    
    if( self.mDateFormatter == nil ) {
        self.mDateFormatter = [[[NSDateFormatter alloc] init] autorelease];
        [self.mDateFormatter setDateFormat:@"m:ss"];
    }

    NSDate *date = [NSDate dateWithTimeIntervalSince1970:self.mTrack.DurationMilliseconds / 1000];
    [self.uiTrackDuration setText:[self.mDateFormatter stringFromDate:date]];
    
    [self.uiTrackNumber setText:[NSString stringWithFormat:@"%d", track.TrackNumber]];
    [self.uiTrackName setText:self.mTrack.Name];
    
    if([track.IsFavorite isEqualToString:@"true"]) {
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
    [[NSNotificationCenter defaultCenter] postNotificationName:EventQueueTrackRequest object:self.mTrack];
}
@end
