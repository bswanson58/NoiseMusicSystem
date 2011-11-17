//
//  FavoriteListCell.h
//  Noisie
//
//  Created by William Swanson on 11/17/11.
//  Copyright (c) 2011 Secret Squirrel Products. All rights reserved.
//

#import <UIKit/UIKit.h>
#import "RoFavorite.h"

@interface FavoriteListCell : UITableViewCell

@property (retain, nonatomic)   RoFavorite  *Favorite;

@property (retain, nonatomic) IBOutlet UILabel *uiFavoriteName;

- (IBAction)cmdPlay:(id)sender;

@end
