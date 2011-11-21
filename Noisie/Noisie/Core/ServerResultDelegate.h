//
//  ServerResultDelegate.h
//  Noisie
//
//  Created by William Swanson on 11/21/11.
//  Copyright (c) 2011 Secret Squirrel Products. All rights reserved.
//

#import "BaseResultDelegate.h"
#import "BaseServerResult.h"

typedef void(^OnServerResultBlock)(BaseServerResult *);

@interface ServerResultDelegate : BaseResultDelegate {
    OnServerResultBlock   mBlockPtr;
}

@property (nonatomic, copy)   OnServerResultBlock mBlockPtr;

- (id) initWithServerResultBlock:(OnServerResultBlock) blockPtr;

@end
