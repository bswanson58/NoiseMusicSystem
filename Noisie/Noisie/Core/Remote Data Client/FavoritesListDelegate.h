//
//  FavoritesListDelegate.h
//  Noisie
//
//  Created by William Swanson on 11/17/11.
//  Copyright (c) 2011 Secret Squirrel Products. All rights reserved.
//

#import "BaseResultDelegate.h"
#import "FavoriteListResult.h"

typedef void(^OnFavoriteListBlock)(FavoriteListResult *);

@interface FavoriteListDelegate : BaseResultDelegate {
    OnFavoriteListBlock   mBlockPtr;
}

@property (nonatomic, copy)   OnFavoriteListBlock mBlockPtr;

- (id) initWithFavoriteListBlock:(OnFavoriteListBlock) blockPtr;

@end
