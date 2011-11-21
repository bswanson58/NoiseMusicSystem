//
//  ServerVersionDelegate.h
//  Noisie
//
//  Created by William Swanson on 11/21/11.
//  Copyright (c) 2011 Secret Squirrel Products. All rights reserved.
//

#import "BaseResultDelegate.h"
#import "ServerVersion.h"

typedef void(^OnServerVersionBlock)(ServerVersion *);

@interface ServerVersionDelegate : BaseResultDelegate {
    OnServerVersionBlock   mBlockPtr;
}

@property (nonatomic, copy)   OnServerVersionBlock mBlockPtr;

- (id) initWithServerVersionBlock:(OnServerVersionBlock) blockPtr;

@end
