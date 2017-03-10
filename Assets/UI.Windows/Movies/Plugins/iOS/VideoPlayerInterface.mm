#include "VideoPlayerToTexture.h"

#if UNITY_VERSION < 450
#include "iPhone_View.h"
#endif

#import <UIKit/UIKit.h>

#include <stdlib.h>
#include <string.h>
#include <stdint.h>

static NSURL* _buildFileUrl(const char* fileName)
{
    NSMutableString *s = [[NSMutableString alloc] initWithString:@"file://"];
    [s appendString:[NSString stringWithUTF8String:fileName]];
    
    return [NSURL URLWithString: s];
}

static NSMutableArray* players = nil;

extern "C" bool VideoPlayer_Initialize(unsigned capacity)
{
    if (players)
    {
        return false;
    }
    
    players = [[NSMutableArray alloc] initWithCapacity: capacity];
    
    return true;
}

extern "C" void VideoPlayer_Finalize()
{
    if (players)
    {
        [players removeAllObjects];
        players = nil;
    }
    
}

extern "C" unsigned VideoPlayer_AddPlayer(const char* fileName)
{
    NSURL* url = _buildFileUrl(fileName);
    VideoPlayerToTexture* player = [VideoPlayerToTexture createPlayerForUrl:url delegate:nil];
    
    NSUInteger index = [players indexOfObject:[NSNull null]];
    if (NSNotFound == index)
    {
        [players addObject:player];
        index = unsigned([players count] - 1);
    }
    else
    {
        players[index] = player;
    }
    
    return (unsigned)index;
}

extern "C" void VideoPlayer_RemovePlayer(unsigned index)
{
    if (index >= [players count])
    {
        return;
    }
    
    [players replaceObjectAtIndex:index withObject:[NSNull null]];
}

extern "C" bool VideoPlayer_IsPlayerFailed(unsigned index)
{
    if (index >= [players count])
    {
        return false;
    }
    
    VideoPlayerToTexture* player = (VideoPlayerToTexture*)players[index];
    return player.failed;
}

extern "C" bool VideoPlayer_IsPlayerReady(unsigned index)
{
    if (index >= [players count])
    {
        return false;
    }
    
    VideoPlayerToTexture* player = (VideoPlayerToTexture*)players[index];
    return player.ready;
}

extern "C" unsigned VideoPlayer_GetTextureId(unsigned index)
{
    if (index >= [players count])
    {
        return false;
    }
    
    VideoPlayerToTexture* player = (VideoPlayerToTexture*)players[index];
    return player.textureId;
}

extern "C" void VideoPlayer_GetVideoSize(unsigned index, unsigned* width, unsigned* height)
{
    if (index >= [players count])
    {
        *width = 0;
        *height = 0;
        return;
    }
    
    VideoPlayerToTexture* player = (VideoPlayerToTexture*)players[index];
    CGSize size = [player videoSize];
    *width = (unsigned)size.width;
    *height = (unsigned)size.height;
}

extern "C" void VideoPlayer_Update(unsigned index, bool* playFinished)
{
    if (index >= [players count])
    {
        return;
    }
    
    VideoPlayerToTexture* player = (VideoPlayerToTexture*)players[index];
    [player update];
    *playFinished = player.playFinished;
}

extern "C" void VideoPlayer_Play(unsigned index, bool loop)
{
    if (index >= [players count])
    {
        return;
    }
    
    VideoPlayerToTexture* player = (VideoPlayerToTexture*)players[index];
    [player play:loop];
}
