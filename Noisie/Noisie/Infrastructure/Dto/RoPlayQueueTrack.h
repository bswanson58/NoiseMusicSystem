//
//  RoPlayQueueTrack.h
//  Noisie
//
//  Created by William Swanson on 11/18/11.
//  Copyright (c) 2011 Secret Squirrel Products. All rights reserved.
//

#import <Foundation/Foundation.h>

@interface RoPlayQueueTrack : NSObject

@property (retain, nonatomic)   NSNumber *TrackId;
@property (copy, nonatomic)     NSString *TrackName;
@property (copy, nonatomic)     NSString *AlbumName;
@property (copy, nonatomic)     NSString *ArtistName;
@property (copy, nonatomic)     NSString *IsPlaying;
@property (copy, nonatomic)     NSString *HasPlayed;
@property (copy, nonatomic)     NSString *IsFaulted;

- (NSString *) formattedName;

@end
