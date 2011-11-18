//
//  SearchResultDelegate.h
//  Noisie
//
//  Created by William Swanson on 11/18/11.
//  Copyright (c) 2011 Secret Squirrel Products. All rights reserved.
//

#import "BaseResultDelegate.h"
#import "SearchResult.h"

typedef void(^OnSearchResultBlock)(SearchResult *);

@interface SearchResultDelegate : BaseResultDelegate {
    OnSearchResultBlock   mBlockPtr;
}

@property (nonatomic, copy)   OnSearchResultBlock mBlockPtr;

- (id) initWithSearchResultBlock:(OnSearchResultBlock) blockPtr;

@end
