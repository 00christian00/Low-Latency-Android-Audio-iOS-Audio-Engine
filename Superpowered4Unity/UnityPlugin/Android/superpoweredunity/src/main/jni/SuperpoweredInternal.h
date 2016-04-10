#ifndef Header_SuperpoweredAndroid
#define Header_SuperpoweredAndroid


#define APPNAME "SuperpoweredInternal"

#ifdef ANDROID
#include <jni.h>
#endif

#include <math.h>
#include <pthread.h>

//#include "SuperpoweredAndroidNDK.h"
#include "../../../../../../../Superpowered/SuperpoweredAdvancedAudioPlayer.h"
#include "../../../../../../../Superpowered/SuperpoweredSimple.h"
#include "../../../../../../../Superpowered/SuperpoweredFilter.h"
#include "../../../../../../../Superpowered/SuperpoweredRoll.h"
#include "../../../../../../../Superpowered/SuperpoweredFlanger.h"

#include "../../../../../../../Superpowered/SuperpoweredMixer.h"

#ifdef ANDROID
#include "../../../../../../../Superpowered/SuperpoweredAndroidAudioIO.h"
#else
#include "../../../../../../../Superpowered/SuperpoweredIOSAudioIO.h"
#endif


#define NUM_BUFFERS 2
#define HEADROOM_DECIBEL 3.0f
#define MAX_PLAYERS 512
typedef struct{
    int length;
    int samplerate;
    int channels;
} audiobuffer_callback_params;


typedef void (*SIMPLE_CALLBACK)();
typedef void (*DATA_CALLBACK)(void *buffer, void* _params); //MonoPinvokecallback only accept 2  parameters
typedef void (*AUDIOBUFFER_CALLBACK)(void *buffer, audiobuffer_callback_params* _params);



static const float headroom = powf(10.0f, -HEADROOM_DECIBEL * 0.025);
char* MakeStringCopy (const char* string);


class SuperpoweredPlayer : SuperpoweredAdvancedAudioPlayer{
bool isPlaying;

};

void LogMsg(const char* format,	...	);

class SuperpoweredInternal {
public:

    SuperpoweredInternal(const char *path, unsigned int samplerate,unsigned int buffersize);
	~SuperpoweredInternal();
    int RegisterSound(long offset, long length );
    int RegisterSound(const char *path);
    void Play(int ClipID);
	bool process(short int *AndroidOutBuffer,float** IOSOutBuffers, unsigned int numberOfSamples, int samplerate,int channels);
	
	void onCrossfader(int value);
	void onFxSelect(int value);
	void onFxOff();
	void onFxValue(int value);

private:
    char* apkpath;
    int ActivePlayers;
    pthread_mutex_t mutex;
#ifdef ANDROID
    SuperpoweredAndroidAudioIO *audioSystemAndroid;
#else 
    SuperpoweredIOSAudioIO *audioSystemIOS;
#endif
    
    SuperpoweredAdvancedAudioPlayer* playerA, *playerB;



    SuperpoweredRoll *roll;
    SuperpoweredFilter *filter;
    SuperpoweredFlanger *flanger;
    float *stereoBuffer;
    float *stereoBufferTemp;
    SuperpoweredStereoMixer* StereoMixer;
    float inputLevels[8];
    float outputLevels[2];
    float* MixerInput[4];
    float* MixerOutput[2];
    unsigned char activeFx;
    float crossValue, volA, volB;
};

#ifdef ANDROID

#endif
#endif
