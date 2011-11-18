//
//  RoArtistInfo.h
//  Noisie
//
//  Created by William Swanson on 11/18/11.
//  Copyright (c) 2011 Secret Squirrel Products. All rights reserved.
//

#import <Foundation/Foundation.h>

@interface RoArtistInfo : NSObject

@property (copy, nonatomic)     NSNumber    *ArtistId;
@property (copy, nonatomic)     NSString    *Website;
@property (copy, nonatomic)     NSString    *Biography;
@property (copy, nonatomic)     NSString    *ArtistImage;
@property (retain, nonatomic)   NSArray     *BandMembers;
@property (retain, nonatomic)   NSArray     *TopAlbums;
@property (retain, nonatomic)   NSArray     *SimilarArtists;

@end
