//
//  ArtistListDelegate.h
//  Noisie
//
//  Created by William Swanson on 11/15/11.
//  Copyright (c) 2011 Secret Squirrel Products. All rights reserved.
//

#import "BaseResultDelegate.h"
#import "ArtistListResult.h"

typedef void(^OnArtistListBlock)(ArtistListResult *);

@interface ArtistListDelegate : BaseResultDelegate {
    OnArtistListBlock   mBlockPtr;
}

@property (nonatomic, copy)   OnArtistListBlock mBlockPtr;

- (id) initWithArtistListBlock:(OnArtistListBlock) blockPtr;

@end
