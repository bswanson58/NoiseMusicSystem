//
//  AlbumInfoDelegate.h
//  Noisie
//
//  Created by William Swanson on 11/18/11.
//  Copyright (c) 2011 Secret Squirrel Products. All rights reserved.
//

#import "BaseResultDelegate.h"
#import "AlbumInfoResult.h"

typedef void(^OnAlbumInfoBlock)(AlbumInfoResult *);

@interface AlbumInfoDelegate : BaseResultDelegate {
    OnAlbumInfoBlock   mBlockPtr;
}

@property (nonatomic, copy)   OnAlbumInfoBlock mBlockPtr;

- (id) initWithAlbumInfoBlock:(OnAlbumInfoBlock) blockPtr;

@end
