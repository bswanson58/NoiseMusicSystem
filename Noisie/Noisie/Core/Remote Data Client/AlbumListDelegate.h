//
//  AlbumListDelegate.h
//  Noisie
//
//  Created by William Swanson on 11/15/11.
//  Copyright (c) 2011 Secret Squirrel Products. All rights reserved.
//

#import "BaseResultDelegate.h"
#import "AlbumListResult.h"

typedef void(^OnAlbumListBlock)(AlbumListResult *);

@interface AlbumListDelegate : BaseResultDelegate {
    OnAlbumListBlock   mBlockPtr;
}

@property (nonatomic, copy)   OnAlbumListBlock mBlockPtr;

- (id) initWithAlbumListBlock:(OnAlbumListBlock) blockPtr;

@end
