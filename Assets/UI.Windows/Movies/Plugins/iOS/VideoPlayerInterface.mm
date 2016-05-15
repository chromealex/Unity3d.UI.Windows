#include "Unity/VideoPlayer.h"

#if UNITY_VERSION < 450
    #include "iPhone_View.h"
#endif

#import <UIKit/UIKit.h>

#include <stdlib.h>
#include <string.h>
#include <stdint.h>

@interface VideoPlayerScripting : NSObject <VideoPlayerDelegate>
{
    @public VideoPlayer* player;
    @public BOOL         playerReady;
    @public BOOL         playerLoop;
}
- (bool)load:(NSURL*)url;
- (bool)play;
- (void)loop:(BOOL)value;

- (void)onPlayerReady;
- (void)onPlayerDidFinishPlayingVideo;
@end

@implementation VideoPlayerScripting
- (bool)load:(NSURL*)url {
    
    if(!player) {
        
        player = [[VideoPlayer alloc] init];
        player.delegate = self;
        
    }
    
    return [player loadVideo:url];
    
}

- (bool)play {
    
    if (playerReady == true) {
        
        [player playToTexture];
        [player setAudioVolume:1.0f];
        return true;
        
    }
    
    return false;
    
}

- (void)onPlayerReady {
    
    playerReady = true;
    
}

- (void)loop:(BOOL)value {
    
    playerLoop = value;
    
}

- (void)onPlayerDidFinishPlayingVideo {
    
    if (playerLoop == true) {
        
        [player rewind];
        
    } else {
        
        playerReady = NO;
        
    }
    
}
@end

static VideoPlayerScripting* _GetPlayer()
{
    static VideoPlayerScripting* _Player = nil;
    if(!_Player)
        _Player = [[VideoPlayerScripting alloc] init];

    return _Player;
}
static NSURL* _GetUrl(const char* filename)
{
    NSURL* url = nil;
    if(::strstr(filename, "://"))
        url = [NSURL URLWithString: [NSString stringWithUTF8String:filename]];
    else
        url = [NSURL fileURLWithPath: [[[NSBundle mainBundle] bundlePath] stringByAppendingPathComponent: [NSString stringWithUTF8String:filename]]];

    return url;
}


extern "C" bool VideoPlayer_CanOutputToTexture(const char* filename)
{
    return [VideoPlayer CanPlayToTexture:_GetUrl(filename)];
}

extern "C" bool VideoPlayer_PlayerReady()
{
    return [_GetPlayer()->player readyToPlay];
}

extern "C" float VideoPlayer_DurationSeconds()
{
    return [_GetPlayer()->player durationSeconds];
}

extern "C" void VideoPlayer_VideoExtents(int* w, int* h)
{
    CGSize sz = [_GetPlayer()->player videoSize];
    *w = (int)sz.width;
    *h = (int)sz.height;
}

extern "C" int VideoPlayer_CurFrameTexture()
{
    return (int)[_GetPlayer()->player curFrameTexture];
}

extern "C" bool VideoPlayer_Load(const char* filename)
{
    return [_GetPlayer() load:_GetUrl(filename)];
}

extern "C" bool VideoPlayer_Play()
{
    return [_GetPlayer() play];
}

extern "C" void VideoPlayer_SetLoop(bool loop)
{
    [_GetPlayer() loop:loop];
}

extern "C" bool VideoPlayer_IsPlaying()
{
    return [_GetPlayer()->player isPlaying];
}

extern "C" void VideoPlayer_Pause()
{
    [_GetPlayer()->player pause];
}

extern "C" void VideoPlayer_Stop()
{
    [_GetPlayer()->player pause];
    [_GetPlayer()->player rewind];
}
