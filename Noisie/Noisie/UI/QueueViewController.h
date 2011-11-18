//
//  QueueViewController.h
//  Noisie
//
//  Created by William Swanson on 11/17/11.
//  Copyright (c) 2011 Secret Squirrel Products. All rights reserved.
//

#import <UIKit/UIKit.h>
#import "PlayQueueListCell.h"

@interface QueueViewController : UIViewController

@property (retain, nonatomic) IBOutlet UITableView          *uiQueueList;
@property (retain, nonatomic) IBOutlet  PlayQueueListCell   *uiListCell;

- (id) initForTabController;

@end
