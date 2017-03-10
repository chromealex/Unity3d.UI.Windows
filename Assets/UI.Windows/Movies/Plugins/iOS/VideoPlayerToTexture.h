#pragma once

#import <CoreMedia/CMTime.h>

@class AVPlayer;

@protocol VideoPlayerToTextureDelegate<NSObject>
- (void)onPlayerReady;
- (void)onPlayerDidFinishPlayingVideo;
- (void)onPlayerError:(NSError*)error;
@end

@interface VideoPlayerToTexture : NSObject

@property (nonatomic, readonly) BOOL failed;
@property (nonatomic, readonly) BOOL ready;
@property (nonatomic, readonly) AVPlayer* player;
@property (nonatomic, readonly) unsigned int textureId;
@property (nonatomic, readonly) BOOL playFinished;

+ (BOOL)canPlayUrl:(NSURL*)url;
+ (VideoPlayerToTexture*)createPlayerForUrl:(NSURL*)url delegate:(id<VideoPlayerToTextureDelegate>)delegate;

- (void)dealloc;

- (void)update;

- (void)play:(BOOL)loop;

- (void)setAudioVolume:(float)volume;

- (CGSize)videoSize;
- (CMTime)duration;
- (float)durationSeconds;


@end
