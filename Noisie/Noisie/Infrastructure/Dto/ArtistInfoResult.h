//
//  ArtistInfoResult.h
//  Noisie
//
//  Created by William Swanson on 11/18/11.
//  Copyright (c) 2011 Secret Squirrel Products. All rights reserved.
//

#import "BaseServerResult.h"
#import "RoArtistInfo.h"

@interface ArtistInfoResult : BaseServerResult

@property (retain, nonatomic) RoArtistInfo  *ArtistInfo;

@end
