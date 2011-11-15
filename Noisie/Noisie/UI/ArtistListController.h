//
//  ArtistListController.h
//  Noisie
//
//  Created by William Swanson on 11/14/11.
//  Copyright (c) 2011 Secret Squirrel Products. All rights reserved.
//

#import <UIKit/UIKit.h>
#import "ArtistListCell.h"

@interface ArtistListController : UIViewController <UITableViewDelegate, UITableViewDataSource>

@property (retain, nonatomic) IBOutlet UITableView      *uiArtistList;
@property (retain, nonatomic) IBOutlet ArtistListCell   *artistListCell;

@end
