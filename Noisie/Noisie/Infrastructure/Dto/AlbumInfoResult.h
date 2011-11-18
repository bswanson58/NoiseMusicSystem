//
//  AlbumInfoResult.h
//  Noisie
//
//  Created by William Swanson on 11/18/11.
//  Copyright (c) 2011 Secret Squirrel Products. All rights reserved.
//

#import "BaseServerResult.h"
#import "RoAlbumInfo.h"

@interface AlbumInfoResult : BaseServerResult

@property (retain, nonatomic)   RoAlbumInfo *AlbumInfo;

@end
