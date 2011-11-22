//
//  PlayQueueListCell.h
//  Noisie
//
//  Created by William Swanson on 11/18/11.
//  Copyright (c) 2011 Secret Squirrel Products. All rights reserved.
//

#import <UIKit/UIKit.h>
#import "RoPlayQueueTrack.h"

@interface PlayQueueListCell : UITableViewCell

@property (retain, nonatomic) IBOutlet UILabel *uiTrackName;
@property (retain, nonatomic) IBOutlet UILabel *uiTrackDuration;

- (void) setTrack:(RoPlayQueueTrack *) track;

@end
