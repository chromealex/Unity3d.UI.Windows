#include "VideoPlayerToTexture.h"

#import <AVFoundation/AVFoundation.h>

#include <OpenGLES/ES3/glext.h>

#include "Unity/GLESHelper.h"

@implementation VideoPlayerToTexture
{
    id<VideoPlayerToTextureDelegate>    _delegate;

    BOOL                                _assetReady;
    BOOL                                _itemReady;

    AVPlayerItem*                       _playerItem;
    AVPlayer*                           _player;

    AVAssetReader*                      _reader;
    AVAssetReaderTrackOutput*           _videoOut;

    GLuint                              _textureId;

    CMSampleBufferRef                   _sampleBuffer;

    CMTime                              _duration;
    CMTime                              _curTime;
    CMTime                              _curFrameTimestamp;
    CMTime                              _lastFrameTimestamp;
    CGSize                              _videoSize;

    BOOL                                _loop;
    BOOL                                _audioRouteWasChanged;
}

@synthesize failed;
@synthesize ready;
@synthesize player = _player;
@synthesize textureId = _textureId;
@synthesize playFinished;


+ (BOOL)canPlayUrl:(NSURL*)url
{
    return [url isFileURL];
}

+ (VideoPlayerToTexture*)createPlayerForUrl:(NSURL*)url delegate:(id<VideoPlayerToTextureDelegate>)delegate
{
    return [[VideoPlayerToTexture alloc] initWithAssetUrl:url delegate:delegate];
}


- (id)initWithAssetUrl:(NSURL*)assetUrl delegate:(id<VideoPlayerToTextureDelegate>)delegate
{
    if ((self = [super init]))
    {
        _delegate = delegate;
        AVURLAsset* asset = [AVURLAsset URLAssetWithURL:assetUrl options:nil];
        if (!asset)
        {
            return nil;
        }

        _duration = _curTime = kCMTimeZero;
        _curFrameTimestamp = _lastFrameTimestamp = kCMTimeZero;

        NSArray* requestedKeys = @[@"tracks", @"playable"];
        [asset loadValuesAsynchronouslyForKeys:requestedKeys completionHandler:^{
            dispatch_async(dispatch_get_main_queue(), ^{
                [self initPlayer:asset requestedKeys:requestedKeys];
            });
        }];
    }

    return self;
}

- (void)onPlayerInited
{
    if (![self initReader])
    {
        failed = YES;
        return;
    }
    
    if (![self initTexture])
    {
        failed = YES;
        return;
    }
    
    ready = YES;
    [_delegate onPlayerReady];
}

- (void)dealloc
{
    [self destroySampleBuffer];
    [self destroyTexture];
    [self destroyReader];
    [self destroyPlayer];
}

- (void)initPlayer:(AVAsset*)asset requestedKeys:(NSArray*)requestedKeys
{
    // check succesful loading
    for (NSString* key in requestedKeys)
    {
        NSError* error = nil;
        AVKeyValueStatus keyStatus = [asset statusOfValueForKey:key error:&error];
        if (keyStatus == AVKeyValueStatusFailed)
        {
            [self reportError:error category:"initPlayer"];
            failed = YES;
            return;
        }
    }

    /*if (!asset.playable)
    {
        [self reportErrorWithString:"Item cannot be played" category:"initPlayer"];
        failed = YES;
        return;
    }*/

    _playerItem = [AVPlayerItem playerItemWithAsset:asset];
    [_playerItem    addObserver:self forKeyPath:@"status"
                    options:NSKeyValueObservingOptionInitial | NSKeyValueObservingOptionNew
                    context:nil
    ];
    [[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(playerItemDidReachEnd:)
                                          name:AVPlayerItemDidPlayToEndTimeNotification object:_playerItem
    ];

    _player = [AVPlayer playerWithPlayerItem:_playerItem];
    [_player    addObserver:self forKeyPath:@"currentItem"
                options:NSKeyValueObservingOptionInitial | NSKeyValueObservingOptionNew
                context:nil
    ];
    [_player setAllowsExternalPlayback:NO];

    // we want to subscribe to route change notifications, for that we need audio session active
    // and in case FMOD wasnt used up to this point it is still not active
    [[AVAudioSession sharedInstance] setActive:YES error:nil];
    [[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(audioRouteChanged:)
                                          name:AVAudioSessionRouteChangeNotification object:nil
    ];
}

- (void)observeValueForKeyPath:(NSString*)path ofObject:(id)object change:(NSDictionary*)change context:(void*)context
{
    BOOL reportPlayerInited = NO;

    if ([path isEqualToString:@"status"])
    {
        AVPlayerStatus status = (AVPlayerStatus)[[change objectForKey:NSKeyValueChangeNewKey] integerValue];
        switch (status)
        {
            case AVPlayerStatusUnknown:
            break;

            case AVPlayerStatusReadyToPlay:
            {
                NSArray* video = [_playerItem.asset tracksWithMediaType:AVMediaTypeVideo];
                if ([video count])
                {
                    _videoSize = [(AVAssetTrack*)[video objectAtIndex:0] naturalSize];
                }

                _duration = [_playerItem duration];
                _assetReady = YES;
                reportPlayerInited = _itemReady;
            }
            break;

            case AVPlayerStatusFailed:
            {
                AVPlayerItem *playerItem = (AVPlayerItem*)object;
                [self reportError:playerItem.error category:"prepareAsset"];
            }
            break;
        }
    }
    else if ([path isEqualToString:@"currentItem"])
    {
        if ([change objectForKey:NSKeyValueChangeNewKey] != (id)[NSNull null])
        {
            _itemReady = YES;
            reportPlayerInited = _assetReady;
        }
    }
    else
    {
        [super observeValueForKeyPath:path ofObject:object change:change context:context];
    }

    if (reportPlayerInited)
    {
        [self onPlayerInited];
    }
}

- (void)destroyPlayer
{
    if (_player)
    {
        [[NSNotificationCenter defaultCenter] removeObserver:self name:AVAudioSessionRouteChangeNotification object:nil];
        [_player.currentItem removeObserver:self forKeyPath:@"status"];
        [_player removeObserver:self forKeyPath:@"currentItem"];
        [_player pause];
        _player = nil;
    }

    if (_playerItem)
    {
        [[NSNotificationCenter defaultCenter] removeObserver:self name:AVPlayerItemDidPlayToEndTimeNotification object:_playerItem];
        _playerItem = nil;
    }
}

- (BOOL)initReader
{
    AVURLAsset* asset = (AVURLAsset*)_playerItem.asset;
    if (![asset.URL isFileURL])
    {
        [self reportErrorWithString:"non-file url. no video to texture." category:"initReader"];
        return NO;
    }

    NSError* error = nil;
    _reader = [AVAssetReader assetReaderWithAsset:_playerItem.asset error:&error];
    if (error)
    {
        [self reportError:error category:"initReader"];
        return NO;
    }

    _reader.timeRange = CMTimeRangeMake(kCMTimeZero, _duration);


    AVAssetTrack* videoTrack = [[_playerItem.asset tracksWithMediaType:AVMediaTypeVideo] objectAtIndex:0];

    NSDictionary* options = @{ (NSString*)kCVPixelBufferPixelFormatTypeKey : @(kCVPixelFormatType_32BGRA) };
    _videoOut = [[AVAssetReaderTrackOutput alloc] initWithTrack:videoTrack outputSettings:options];
    _videoOut.alwaysCopiesSampleData = NO;

    if (![_reader canAddOutput:_videoOut])
    {
        [self reportErrorWithString:"canAddOutput returned false" category:"initReader"];
        return NO;
    }
    [_reader addOutput:_videoOut];

    if (![_reader startReading])
    {
        [self reportError:[_reader error] category:"initReader"];
        return NO;
    }

    return YES;
}

- (void)destroyReader
{
    if (_reader)
    {
        [_reader cancelReading];
    }

    _reader = nil;
    _videoOut = nil;
}

- (BOOL)initTexture
{
    GLsizei width = (GLsizei)_videoSize.width;
    GLsizei height = (GLsizei)_videoSize.height;

    _textureId = 0;

    GLint oldTexBinding = 0;
    GLES_CHK(glGetIntegerv(GL_TEXTURE_BINDING_2D, &oldTexBinding));
    GLES_CHK(glGenTextures(1, &_textureId));
    GLES_CHK(glBindTexture(GL_TEXTURE_2D, _textureId));
    GLES_CHK(glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_LINEAR));
    GLES_CHK(glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_LINEAR));
    GLES_CHK(glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, GL_CLAMP_TO_EDGE));
    GLES_CHK(glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, GL_CLAMP_TO_EDGE));
    GLES_CHK(glTexImage2D(GL_TEXTURE_2D, 0, GL_RGBA, width, height, 0, GL_BGRA_EXT, GL_UNSIGNED_BYTE, 0));
    GLES_CHK(glBindTexture(GL_TEXTURE_2D, oldTexBinding));

    return (_textureId > 0);
}

- (void)destroyTexture
{
    if (_textureId > 0)
    {
        glDeleteTextures(1, &_textureId);
        _textureId = 0;
    }
}

- (void)destroySampleBuffer
{
    if (_sampleBuffer)
    {
        CFRelease(_sampleBuffer);
        _sampleBuffer = nil;
    }
}

- (void)renderSampleBufferToTexture
{
    CVImageBufferRef imageBuffer = CMSampleBufferGetImageBuffer(_sampleBuffer);
    CFRetain(imageBuffer);

    size_t width = CVPixelBufferGetWidth(imageBuffer);
    size_t height = CVPixelBufferGetHeight(imageBuffer);
    _videoSize = CGSizeMake(width, height);

    CVPixelBufferLockBaseAddress(imageBuffer, 0);
    void* pixels = CVPixelBufferGetBaseAddress(imageBuffer);

    GLint oldTexBinding = 0;
    GLES_CHK(glGetIntegerv(GL_TEXTURE_BINDING_2D, &oldTexBinding));
    GLES_CHK(glBindTexture(GL_TEXTURE_2D, _textureId));
    GLES_CHK(glTexImage2D(GL_TEXTURE_2D, 0, GL_RGBA, static_cast<GLsizei>(width), static_cast<GLsizei>(height), 0, GL_BGRA_EXT, GL_UNSIGNED_BYTE, pixels));
    GLES_CHK(glBindTexture(GL_TEXTURE_2D, oldTexBinding));

    CVPixelBufferUnlockBaseAddress(imageBuffer, 0);

    CFRelease(imageBuffer);
}

- (void)update
{
    if (!ready)
    {
        return;
    }

    CMTime time = [_player currentTime];

    // if we have changed audio route and due to current category apple decided to pause playback - resume automatically
    if (_audioRouteWasChanged && _player.rate == 0.0f)
    {
        _audioRouteWasChanged = NO;
        _player.rate = 1.0f;
    }

    if (CMTimeCompare(time, _curTime) == 0 || _reader.status != AVAssetReaderStatusReading)
    {
        return;
    }

    _curTime = time;
    while (_reader.status == AVAssetReaderStatusReading && CMTimeCompare(_curFrameTimestamp, _curTime) <= 0)
    {
        if (_sampleBuffer)
        {
            CFRelease(_sampleBuffer);
        }

        // TODO: properly handle ending
        _sampleBuffer = [_videoOut copyNextSampleBuffer];
        if (_sampleBuffer == 0)
        {
            if (_loop)
            {
                [_player seekToTime:kCMTimeZero];
                _curTime = kCMTimeZero;
                _curFrameTimestamp = kCMTimeZero;
                _lastFrameTimestamp = kCMTimeZero;
                [self destroyReader];
                [self initReader];
                [_player play];
                _sampleBuffer = [_videoOut copyNextSampleBuffer];
            }
            else
            {
                [_player pause];
                _curFrameTimestamp = _curTime;
                playFinished = YES;
                return;
            }
        }

        _curFrameTimestamp = CMSampleBufferGetPresentationTimeStamp(_sampleBuffer);
    }

    if (CMTimeCompare(_lastFrameTimestamp, _curFrameTimestamp) < 0)
    {
        _lastFrameTimestamp = _curFrameTimestamp;
        [self renderSampleBufferToTexture];
    }
}

- (void)play:(BOOL)loop
{
    if (!ready)
    {
        return;
    }

    _loop = loop;
    playFinished = NO;
    [_player play];
}

- (void)setAudioVolume:(float)volume
{
    if (!ready)
    {
        return;
    }

    NSArray* audio = [_playerItem.asset tracksWithMediaType:AVMediaTypeAudio];
    NSMutableArray* params = [NSMutableArray array];
    for (AVAssetTrack* track in audio)
    {
        AVMutableAudioMixInputParameters* inputParams = [AVMutableAudioMixInputParameters audioMixInputParameters];
        [inputParams setVolume:volume atTime:kCMTimeZero];
        [inputParams setTrackID:[track trackID]];
        [params addObject:inputParams];
    }

    AVMutableAudioMix* audioMix = [AVMutableAudioMix audioMix];
    [audioMix setInputParameters:params];

    [_playerItem setAudioMix:audioMix];
}


- (CGSize)videoSize
{
    return _videoSize;
}

- (CMTime)duration
{
    return _duration;
}

- (float)durationSeconds
{
    return CMTIME_IS_VALID(_duration) ? (float)CMTimeGetSeconds(_duration) : 0.0f;
}


- (void)reportError:(NSError*)error category:(const char*)category
{
    ::printf("[%s]Error: %s\n", category, [[error localizedDescription] UTF8String]);
    ::printf("%s\n", [[error localizedFailureReason] UTF8String]);
    [_delegate onPlayerError:error];
}

- (void)reportErrorWithString:(const char*)error category:(const char*)category
{
    ::printf("[%s]Error: %s\n", category, error);
    [_delegate onPlayerError:nil];
}


- (void)playerItemDidReachEnd:(NSNotification*)notification
{
    playFinished = YES;
    [_delegate onPlayerDidFinishPlayingVideo];
}

- (void)audioRouteChanged:(NSNotification*)notification
{
    _audioRouteWasChanged = YES;
}

@end
