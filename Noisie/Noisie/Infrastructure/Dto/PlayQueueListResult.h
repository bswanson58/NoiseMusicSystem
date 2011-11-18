//
//  PlayQueueListResult.h
//  Noisie
//
//  Created by William Swanson on 11/18/11.
//  Copyright (c) 2011 Secret Squirrel Products. All rights reserved.
//

#import "BaseServerResult.h"

@interface PlayQueueListResult : BaseServerResult

@property (retain, nonatomic) NSArray *Tracks;

@end
