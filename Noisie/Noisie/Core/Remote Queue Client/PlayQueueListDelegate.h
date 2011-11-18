//
//  PlayQueueListDelegate.h
//  Noisie
//
//  Created by William Swanson on 11/18/11.
//  Copyright (c) 2011 Secret Squirrel Products. All rights reserved.
//

#import "BaseResultDelegate.h"
#import "PlayQueueListResult.h"

typedef void(^OnPlayQueueListBlock)(PlayQueueListResult *);

@interface PlayQueueListDelegate : BaseResultDelegate {
    OnPlayQueueListBlock   mBlockPtr;
}

@property (nonatomic, copy)   OnPlayQueueListBlock mBlockPtr;

- (id) initWithPlayQueueListBlock:(OnPlayQueueListBlock) blockPtr;

@end
