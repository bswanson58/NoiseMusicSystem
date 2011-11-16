//
//  TrackListResult.h
//  Noisie
//
//  Created by William Swanson on 11/15/11.
//  Copyright (c) 2011 Secret Squirrel Products. All rights reserved.
//

#import "BaseServerResult.h"

@interface TrackListResult : BaseServerResult

@property (nonatomic, retain)   NSNumber    *ArtistId;
@property (nonatomic, retain)   NSNumber    *AlbumId;
@property (nonatomic, retain)   NSArray     *Tracks;

@end
