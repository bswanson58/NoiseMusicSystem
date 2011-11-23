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
@property (retain, nonatomic) IBOutlet PlayQueueListCell    *uiListCell;
@property (retain, nonatomic) IBOutlet UIButton             *uiPlayButton;
@property (retain, nonatomic) IBOutlet UIButton             *uiPauseButton;
@property (retain, nonatomic) IBOutlet UIButton             *uiStopButton;
@property (retain, nonatomic) IBOutlet UIButton             *uiPlayPreviousButton;
@property (retain, nonatomic) IBOutlet UIButton             *uiPlayNextButton;
@property (retain, nonatomic) IBOutlet UIButton             *uiRepeatButton;

- (IBAction)cmdPlay:(id)sender;
- (IBAction)cmdPause:(id)sender;
- (IBAction)cmdStop:(id)sender;
- (IBAction)cmdPlayPrevious:(id)sender;
- (IBAction)cmdPlayNext:(id)sender;
- (IBAction)cmdRepeat:(id)sender;

- (id) initForTabController;

@end
