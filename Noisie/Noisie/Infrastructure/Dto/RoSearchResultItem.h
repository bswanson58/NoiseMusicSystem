//
//  RoSearchResultItem.h
//  Noisie
//
//  Created by William Swanson on 11/18/11.
//  Copyright (c) 2011 Secret Squirrel Products. All rights reserved.
//

#import <Foundation/Foundation.h>

@interface RoSearchResultItem : NSObject

@property (retain, nonatomic)   NSNumber    *TrackId;
@property (copy, nonatomic)     NSString    *TrackName;
@property (retain, nonatomic)   NSNumber    *AlbumId;
@property (copy, nonatomic)     NSString    *AlbumName;
@property (copy, nonatomic)     NSString    *ArtistName;
@property (copy, nonatomic)     NSString    *CanPlay;

- (NSString *) formattedTitle;

@end
