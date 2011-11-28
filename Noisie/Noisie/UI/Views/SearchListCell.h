//
//  SearchListCell.h
//  Noisie
//
//  Created by William Swanson on 11/18/11.
//  Copyright (c) 2011 Secret Squirrel Products. All rights reserved.
//

#import <UIKit/UIKit.h>
#import "RoSearchResultItem.h"

@interface SearchListCell : UITableViewCell

@property (retain, nonatomic) IBOutlet UILabel  *uiItemTitle;
@property (retain, nonatomic) IBOutlet UIButton *uiPlayButton;

- (IBAction)cmdPlay:(id)sender;

- (void) setSearchItem:(RoSearchResultItem *) searchItem;

@end
