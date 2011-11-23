//
//  ArtistListCell.h
//  Noisie
//
//  Created by William Swanson on 11/15/11.
//  Copyright (c) 2011 Secret Squirrel Products. All rights reserved.
//

#import <UIKit/UIKit.h>
#import "RoArtist.h"

@interface ArtistListCell : UITableViewCell

@property (retain, nonatomic) IBOutlet UILabel      *uiArtistName;
@property (retain, nonatomic) IBOutlet UILabel      *uiAlbumCount;
@property (retain, nonatomic) IBOutlet UILabel      *uiArtistGenre;
@property (retain, nonatomic) IBOutlet UIImageView  *uiIsFavorite;

- (void) setArtist:(RoArtist *) artist;

@end
