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

@property (retain, nonatomic) RoSearchResultItem    *SearchItem;

@property (retain, nonatomic) IBOutlet UILabel *uiItemTitle;

- (IBAction)cmdPlay:(id)sender;

@end
