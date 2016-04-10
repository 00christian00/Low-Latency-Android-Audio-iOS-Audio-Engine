using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Runtime.InteropServices;
using AOT;
using System.Diagnostics;

/// <summary>
/// Superpowered manager.
/// </summary>

public class SuperpoweredManager : MonoBehaviour {
#if UNITY_EDITOR
    const int MAX_CLIPS = 512;
     AudioClip[] Clips = new AudioClip[MAX_CLIPS];
    AudioSource[] Sources = new AudioSource[MAX_CLIPS];
#endif


    static SuperpoweredManager _instance;

    public static SuperpoweredManager Instance
    {

        get
        {
            if (_instance == null) {
                GameObject GO = new GameObject();
                DontDestroyOnLoad(GO);
                GO.name = "SuperpoweredManager";
                _instance = GO.AddComponent<SuperpoweredManager>();
                
            }

            return _instance;
        }
        
    }


    private AndroidJavaObject activityContext;      // android.app.Context; subclass of android.content.Activity
    private AndroidJavaObject SuperpoweredAndroidHelper;  // com.superpowered.SuperpoweredAndroid

	//need to pass as a pointer to class, as I couldn't get it to work with structs
	[StructLayout (LayoutKind.Sequential)]
	public class AudioCallbackParams {
		public int length;
		public int samplerate;
		public int channels;
	}

    private delegate void SimpleCallback();
    private SimpleCallback _recurring_callback_holder;

	public delegate void DataCallback(IntPtr buffer, AudioCallbackParams _params);
    private DataCallback AudioCallback;


#if UNITY_IOS
	[DllImport ("__Internal")]
	private static extern void CreateInstanceiOS(int samplerate, int buffersize_in_ms);

	[DllImport ("__Internal")]
	private static extern int RegisterSoundiOS(string path);
#endif

#if UNITY_IOS
	[DllImport ("__Internal")]
#elif UNITY_ANDROID
    [DllImport("SuperpoweredAndroidNDK")]
#endif
    public static extern void PlaySound(int ClipID);


#if UNITY_IOS
	[DllImport ("__Internal")]
#elif UNITY_ANDROID
    [DllImport("SuperpoweredAndroidNDK")]
#endif
    public static extern void SetAudioCallback(int ClipID, DataCallback callback);

    static float[] buffer;
    public void Awake() {
        
        //Clips = new List<AudioClip>(MAX_CLIPS);
        //Sources = new List<AudioSource>(MAX_CLIPS);
        //AudioCallback = new DataCallback(FillBuffer);
        // SetAudioCallback(1, AudioCallback);

    }
    [Conditional("DEBUG")]
    void DebugMsg(object msg) {
        print(msg.ToString());
    }

    public  void Init() {
        if (Application.isEditor)
            return;



        buffer = new float[1024];
        /*for (int i = 0; i < buffer.Length; i++) {
			buffer[i] = UnityEngine.Random.Range(-1f, 1f);
		}*/


        if (Application.platform == RuntimePlatform.Android) {

            AndroidInit();
        } else if (Application.platform == RuntimePlatform.IPhonePlayer) {
            iOSInit();
        }
    }

    /// <summary>
    /// Superpowered Android plugin is divided in 2 libraries, one is in Java and one is in C++.
    /// The Java one initialize the c++ singleton but later I use directly the c++ shared library.
    /// The reason of the java library is because certain things like access to the resources and assets of the apk are available only in java. 
    /// On the iOS plugin instead everything is handled on the c++(actually a mix between c++ and obj c) side.
    /// </summary>
    void AndroidInit() {
#if UNITY_ANDROID
        print("Initializing Superpowered Android");

        using (AndroidJavaClass activityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer")) {
            activityContext = activityClass.GetStatic<AndroidJavaObject>("currentActivity");
        }

        using (AndroidJavaClass pluginClass = new AndroidJavaClass("com.superpowered.SuperpoweredAndroid")) {
            //Debug.Log("pluginClass is " + pluginClass);
            // SuperpoweredAndroid = pluginClass.CallStatic<AndroidJavaObject>("createInstance", activityContext);
            SuperpoweredAndroidHelper = pluginClass.CallStatic<AndroidJavaObject>("createInstance", activityContext, 44100, 512);

        }
#endif

    }

    void iOSInit() {
#if UNITY_IOS
		print ("Initializing Superpowered iOS");
		CreateInstanceiOS (44100,12);//here buffer size is in ms. don't know why...
#endif

    }
    /// <summary>
    /// Registers a sound file with the Superpowered audio SDK.
    /// </summary>
    /// <returns>The sound ID to use on subsequent play() calls.</returns>
    /// <param name="soundFile">File name in StreamingAssets directory.</param>
    public int RegisterSoundFile(string soundFile) {
        int soundId = -1;
#if UNITY_EDITOR
        string path = "file://" + Application.streamingAssetsPath + "/" + soundFile;
        return RegisterSoundFileDesktop(path);


#elif UNITY_IOS
		//RegisterSoundiOS(Application.dataPath +  "/Raw/" + soundFile);
		RegisterSoundiOS(Application.streamingAssetsPath  + "/" + soundFile);

#elif UNITY_ANDROID
        if (SuperpoweredAndroidHelper == null) { return -1; }

        // TODO: if sound file is already registered, don't register it again.
        // Note that this potentially makes UnregisterSound() more complicated
        // as we would need to refcount.

        soundId = SuperpoweredAndroidHelper.Call<int>("register", soundFile);


            // TODO: the sound isn't necessarily ready to play at this point -- the
            // load mechanism is asynchronous.  We would need to tie into the
            // SoundPool OnLoadCompleteListener to figure out when sounds were
            // actually ready to go.

#endif
        
//NEEED TO HANDLE ASYNC LOADING ON PLUGIN SIDE
        DebugMsg("Registered " + soundFile + " as ID " + soundId);
       
        return soundId;
    }
#if UNITY_EDITOR
    public int RegisterSoundFileDesktop(string path) {
        int SoundId = -1;
        for (int i = 0; i < Clips.Length; i++) {
            if (Clips[i] == null) {
                SoundId = i;
                break;
            }

        }

        if (SoundId != -1) {
            print(string.Format("loading {0}", path));
            WWW www = new WWW(path);
            Clips[SoundId]= www.audioClip;

            StartCoroutine(loadFile(path, SoundId));
            //Sources[SoundId].
            // www.audioClip;
        }
        return SoundId;
    }

    IEnumerator loadFile(string path,int SoundId) {
        WWW www = new WWW( path);

        AudioClip clip = www.GetAudioClip(false, false, AudioType.WAV);

       if (clip == null) print("WHY?!");
        while (clip.loadState != AudioDataLoadState.Loaded) {
            if (clip.loadState == AudioDataLoadState.Failed) break;
            // print(clip.loadState);
            yield return www;
        }
        Sources[SoundId] = this.gameObject.AddComponent<AudioSource>();
        Sources[SoundId].playOnAwake = false;
        Sources[SoundId].clip = clip;



    }

    void PlaySoundDesktop(int SoundID) {
        if (SoundID < 0 || SoundID >= Clips.Length) return;
        if (Clips[SoundID].loadState == AudioDataLoadState.Loaded) Sources[SoundID].Play();
    }
#endif

    //IntPtr is actually a void *. it can be anything
    /*[MonoPInvokeCallback(typeof(DataCallback))]
	public static void FillBuffer(IntPtr in_buffer, IntPtr length,IntPtr samplerate,IntPtr channels) {

        //int length = in_length.ToInt32();
        //buffer[0] = 5f;
        //print("hello from fillbuffer " + length);
        //Marshal.Copy(buffer, 0, in_buffer, length * 2);

    }*/

    //just a wrapper for PlaySound for coherence, but you could also call directly the static method PlaySound and save a method call
    public void PlaySoundAt(int SoundID) {
        DebugMsg("Playing sound with handle: " + SoundID);
#if UNITY_EDITOR
        PlaySoundDesktop(SoundID);
        return;
#endif
        PlaySound(SoundID);
    }

    

    // Use this for initialization
    void Start() {
        

    }
    /*
    // Update is called once per frame
    void Update() {
        if (Input.GetMouseButtonDown(0)) PlaySound(0);
    }*/
}
