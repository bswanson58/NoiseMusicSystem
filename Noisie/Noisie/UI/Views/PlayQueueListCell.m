//
//  PlayQueueListCell.m
//  Noisie
//
//  Created by William Swanson on 11/18/11.
//  Copyright (c) 2011 Secret Squirrel Products. All rights reserved.
//

#import "PlayQueueListCell.h"

@interface PlayQueueListCell ()

@property (nonatomic, retain)   NSDateFormatter *mDateFormatter;
@property (nonatomic, retain)   UIFont          *mStandardFont;
@property (nonatomic, retain)   UIFont          *mItalicFont;

@end

@implementation PlayQueueListCell

@synthesize mDateFormatter;
@synthesize mStandardFont;
@synthesize mItalicFont;
@synthesize uiIsPlaying;
@synthesize uiTrackName;
@synthesize uiTrackDuration;

- (void)dealloc {
    self.mDateFormatter = nil;
    self.mStandardFont = nil;
    self.mItalicFont = nil;
    
    [uiTrackName release];
    [uiTrackDuration release];
    [uiIsPlaying release];
    [super dealloc];
}

- (void) setTrack:(RoPlayQueueTrack *)track {
    [self.uiTrackName setText:[track formattedName]];
    
    if([track.HasPlayed isEqualToString:@"true"]) {
        [self.uiTrackName setTextColor:[UIColor grayColor]];
    }
    if([track.IsPlaying isEqualToString:@"true"]) {
        [self.uiIsPlaying setHidden:NO];
    }
    else {
        [self.uiIsPlaying setHidden:YES];
    }
    if([track.IsFaulted isEqualToString:@"true"]) {
        [self.uiTrackName setTextColor:[UIColor redColor]];
    }
    
    if( self.mStandardFont == nil ) {
        self.mStandardFont = self.uiTrackName.font;
        self.mItalicFont = [UIFont italicSystemFontOfSize:self.mStandardFont.pointSize];
    }
    if([track.IsStrategySourced isEqualToString:@"true"]) {
        [self.uiTrackName setFont:self.mItalicFont];
    }
    else {
        [self.uiTrackName setFont:self.mStandardFont];
    }
    
    if( self.mDateFormatter == nil ) {
        self.mDateFormatter = [[[NSDateFormatter alloc] init] autorelease];
        [self.mDateFormatter setDateFormat:@"m:ss"];
    }
    
    NSDate *date = [NSDate dateWithTimeIntervalSince1970:track.DurationMilliseconds / 1000];
    [self.uiTrackDuration setText:[self.mDateFormatter stringFromDate:date]];
}

- (void)setSelected:(BOOL)selected animated:(BOOL)animated {
    [super setSelected:selected animated:animated];

    // Configure the view for the selected state
}

@end
