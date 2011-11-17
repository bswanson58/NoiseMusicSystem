//
//  FavoriteListResult.h
//  Noisie
//
//  Created by William Swanson on 11/17/11.
//  Copyright (c) 2011 Secret Squirrel Products. All rights reserved.
//

#import "BaseServerResult.h"

@interface FavoriteListResult : BaseServerResult

@property (retain, nonatomic) NSArray   *Favorites;

@end
