//
//  TrackListCell.h
//  Noisie
//
//  Created by William Swanson on 11/17/11.
//  Copyright (c) 2011 Secret Squirrel Products. All rights reserved.
//

#import <UIKit/UIKit.h>
#import "RoTrack.h"

@interface TrackListCell : UITableViewCell

@property (retain, nonatomic) IBOutlet UILabel *uiTrackName;
@property (retain, nonatomic) IBOutlet UILabel *uiTrackDuration;

- (IBAction)cmdPlay:(id)sender;

- (void) setTrack:(RoTrack *) track;

@end
