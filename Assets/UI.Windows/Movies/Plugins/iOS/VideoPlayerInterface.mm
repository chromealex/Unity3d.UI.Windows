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
@public BOOL         loop;
}
- (void)loadVideo:(NSURL*)url;

- (void)onPlayerReady;
- (void)onPlayerDidFinishPlayingVideo;
@end

@implementation VideoPlayerScripting
- (void)loadVideo:(NSURL*)url
{
    if(!player)
    {
        player = [[VideoPlayer alloc] init];
        player.delegate = self;
    }
    [player loadVideo:url];
    loop = NO;
}

- (void)onPlayerReady
{
    playerReady = YES;
    
    [player playToTexture];
    [player setAudioVolume:1.0f];
    [player pause];
}

- (void)onPlayerDidFinishPlayingVideo
{
    if (loop == YES)
    {
        [player rewind];
        [player playToTexture];
    }
    else
        playerReady = NO;
}
@end

static NSURL* _GetUrl(const char* filename)
{
    NSMutableString *s = [[NSMutableString alloc] initWithString:@"file://"];
    [s appendString:[NSString stringWithUTF8String:filename]];
    
    return [NSURL URLWithString: s];
}

static NSMutableArray* players = nil;

extern "C" unsigned VideoPlayer_AddPlayer()
{
    NSInteger index = [players indexOfObject:[NSNull null]];
    
    if (NSNotFound == index)
    {
        [players addObject: [[VideoPlayerScripting alloc] init]];
        index = unsigned([players count] - 1);
    }
    else
        players[index] = [[VideoPlayerScripting alloc] init];
    
    return index;
}

extern "C" void VideoPlayer_RemovePlayer(unsigned idx)
{
    if (idx >= [players count])
        return;
    
    [((VideoPlayerScripting *)[players objectAtIndex:idx])->player unloadPlayer];
    ((VideoPlayerScripting *)[players objectAtIndex:idx])->player = nil;
    
    
    [players replaceObjectAtIndex:idx withObject:[NSNull null]];
}


extern "C" bool VideoPlayer_Initialize(unsigned capacity)
{
    if (players != nil)
        return false;
    
    players = [[NSMutableArray alloc] initWithCapacity: capacity];
    
    return true;
}

extern "C" void VideoPlayer_Finalize()
{
    if (players == nil)
        return;
    
    [players removeAllObjects];
    
    players = nil;
}

extern "C" bool VideoPlayer_CanOutputToTexture(const char* filename)
{
    return [VideoPlayer CanPlayToTexture:_GetUrl(filename)];
}

extern "C" bool VideoPlayer_PlayerIsPlaying(unsigned idx)
{
    if (idx >= [players count])
        return false;
    
    return [((VideoPlayerScripting *)[players objectAtIndex:idx])->player isPlaying];
}

extern "C" bool VideoPlayer_PlayerReady(unsigned idx)
{
    if (idx >= [players count])
        return false;
    
    return [((VideoPlayerScripting *)[players objectAtIndex:idx])->player readyToPlay];
}

extern "C" void VideoPlayer_SetLoop(unsigned idx, bool loop)
{
    if (idx >= [players count])
        return;
    
    ((VideoPlayerScripting *)[players objectAtIndex:idx])->loop = loop;
}

extern "C" void VideoPlayer_Play(unsigned idx)
{
    if (idx >= [players count])
        return;
    
    [((VideoPlayerScripting *)[players objectAtIndex:idx])->player resume];
}

extern "C" void VideoPlayer_Stop(unsigned idx)
{
    if (idx >= [players count])
        return;
    
    VideoPlayer* p = ((VideoPlayerScripting *)[players objectAtIndex:idx])->player;
    
    [p rewind];
    [p pause];
}

extern "C" void VideoPlayer_Pause(unsigned idx)
{
    if (idx >= [players count])
        return;
    
    VideoPlayer* p = ((VideoPlayerScripting *)[players objectAtIndex:idx])->player;
    
    if ([p isPlaying] == YES)
        [p pause];
    else
        [p resume];
}

extern "C" void VideoPlayer_Rewind(unsigned idx, bool pause)
{
    if (idx >= [players count])
        return;
    
    VideoPlayer* p = ((VideoPlayerScripting *)[players objectAtIndex:idx])->player;
    
    [p rewind];
    
    if (pause)
        [p pause];
    else
        [p resume];
}

extern "C" float VideoPlayer_DurationSeconds(unsigned idx)
{
    if (idx >= [players count])
        return 0.0;
    
    return [((VideoPlayerScripting *)[players objectAtIndex:idx])->player durationSeconds];
}

extern "C" void VideoPlayer_VideoExtents(unsigned idx, int* w, int* h)
{
    if (idx >= [players count])
        return;
    
    CGSize sz = [((VideoPlayerScripting *)[players objectAtIndex:idx])->player videoSize];
    *w = (int)sz.width;
    *h = (int)sz.height;
}

extern "C" int VideoPlayer_CurFrameTexture(unsigned idx)
{
    if (idx >= [players count])
        return 0;
    
    return [((VideoPlayerScripting *)[players objectAtIndex:idx])->player curFrameTexture];
}

extern "C" bool VideoPlayer_LoadVideo(unsigned idx, const char* filename)
{
    if (idx >= [players count])
        return false;
    
    [((VideoPlayerScripting *)[players objectAtIndex:idx]) loadVideo:_GetUrl(filename)];
    
    return true;
}
