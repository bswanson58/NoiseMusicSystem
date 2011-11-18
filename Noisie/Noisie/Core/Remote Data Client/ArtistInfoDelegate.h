//
//  ArtistInfoDelegate.h
//  Noisie
//
//  Created by William Swanson on 11/18/11.
//  Copyright (c) 2011 Secret Squirrel Products. All rights reserved.
//

#import "BaseResultDelegate.h"
#import "ArtistInfoResult.h"

typedef void(^OnArtistInfoBlock)(ArtistInfoResult *);

@interface ArtistInfoDelegate : BaseResultDelegate {
    OnArtistInfoBlock   mBlockPtr;
}

@property (nonatomic, copy)   OnArtistInfoBlock mBlockPtr;

- (id) initWithArtistInfoBlock:(OnArtistInfoBlock) blockPtr;

@end
