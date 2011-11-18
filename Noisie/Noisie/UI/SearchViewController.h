//
//  SearchViewController.h
//  Noisie
//
//  Created by William Swanson on 11/17/11.
//  Copyright (c) 2011 Secret Squirrel Products. All rights reserved.
//

#import <UIKit/UIKit.h>
#import "SearchListCell.h"

@interface SearchViewController : UIViewController

@property (retain, nonatomic) IBOutlet UITextField      *uiSearchText;
@property (retain, nonatomic) IBOutlet UITableView      *uiResultsList;
@property (retain, nonatomic) IBOutlet SearchListCell   *uiSearchCell;

- (id) initForTabController;

- (IBAction)cmdSearch:(id)sender;

@end
