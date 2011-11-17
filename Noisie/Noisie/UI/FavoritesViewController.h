//
//  FavoritesViewController.h
//  Noisie
//
//  Created by William Swanson on 11/17/11.
//  Copyright (c) 2011 Secret Squirrel Products. All rights reserved.
//

#import <UIKit/UIKit.h>
#import "FavoriteListCell.h"

@interface FavoritesViewController : UIViewController <UITableViewDelegate, UITableViewDataSource>

@property (retain, nonatomic) IBOutlet UITableView      *uiFavoritesList;
@property (retain, nonatomic) IBOutlet FavoriteListCell *uiFavoriteCell;

- (id) initForTabController;

@end
