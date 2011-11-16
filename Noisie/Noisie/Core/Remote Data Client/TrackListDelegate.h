//
//  TrackListDelegate.h
//  Noisie
//
//  Created by William Swanson on 11/15/11.
//  Copyright (c) 2011 Secret Squirrel Products. All rights reserved.
//

#import "BaseResultDelegate.h"
#import "TrackListResult.h"

typedef void(^OnTrackListBlock)(TrackListResult *);

@interface TrackListDelegate : BaseResultDelegate {
    OnTrackListBlock   mBlockPtr;
}

@property (nonatomic, copy)   OnTrackListBlock mBlockPtr;

- (id) initWithTrackListBlock:(OnTrackListBlock) blockPtr;

@end
