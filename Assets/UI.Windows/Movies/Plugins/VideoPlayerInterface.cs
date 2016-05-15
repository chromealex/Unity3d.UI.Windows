using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine.UI.Windows;

public class VideoPlayerInterface {

	#if UNITY_IPHONE && !UNITY_EDITOR
    [DllImport("__Internal")]
	private static extern bool VideoPlayer_CanOutputToTexture(string filename);
    [DllImport("__Internal")]
	private static extern bool VideoPlayer_PlayerReady();
    [DllImport("__Internal")]
	private static extern float VideoPlayer_DurationSeconds();
    [DllImport("__Internal")]
	private static extern void VideoPlayer_VideoExtents(ref int w, ref int h);
    [DllImport("__Internal")]
	private static extern int VideoPlayer_CurFrameTexture();
	[DllImport("__Internal")]
	private static extern bool VideoPlayer_Load(string filename);
	[DllImport("__Internal")]
	private static extern bool VideoPlayer_SetLoop(bool loop);
	[DllImport("__Internal")]
	private static extern bool VideoPlayer_IsPlaying();
	[DllImport("__Internal")]
	private static extern bool VideoPlayer_Play();
	[DllImport("__Internal")]
	private static extern void VideoPlayer_Pause();
	[DllImport("__Internal")]
	private static extern void VideoPlayer_Stop();
	#else
	private static bool VideoPlayer_CanOutputToTexture(string filename) {
		return false;
	}

	private static bool VideoPlayer_PlayerReady() {
		return false;
	}

	private static float VideoPlayer_DurationSeconds() {
		return 0.0f;
	}

	private static void VideoPlayer_VideoExtents(ref int w, ref int h) {
	}

	private static int VideoPlayer_CurFrameTexture() {
		return 0;
	}
	
	private static bool VideoPlayer_Load(string filepath) {

		return false;

	}

	private static void VideoPlayer_SetLoop(bool loop) {
	}

	private static bool VideoPlayer_IsPlaying() { return false; }

	private static void VideoPlayer_Pause() {
	}
	
	private static void VideoPlayer_Stop() {
	}
	
	private static bool VideoPlayer_Play() {
		return false;
	}
	#endif

	public bool videoReady { get { return VideoPlayer_PlayerReady(); } }

	public float videoDuration { get { return VideoPlayer_DurationSeconds(); } }

	public string filepath;
	private System.Action waitingForLoad;
	private MonoBehaviour system;
	private Coroutine routine;

	public VideoPlayerInterface(MonoBehaviour system) {

		this.system = system;

		this.routine = this.system.StartCoroutine(this.Update());

	}

	~VideoPlayerInterface() {

		this.system.StopCoroutine(this.routine);

	}

	public IEnumerator Update() {

		if (this.videoReady == true && this.videoTexture != null) {
			
			if (this.waitingForLoad != null) this.waitingForLoad.Invoke();
			this.waitingForLoad = null;
			
		}

		yield return false;

		this.routine = this.system.StartCoroutine(this.Update());
		
	}

	public int videoWidth {

		get {

			int w = 0, h = 0;
			VideoPlayer_VideoExtents(ref w, ref h);
			return w;

		}

	}

	public int videoHeight {

		get {

			int w = 0, h = 0;
			VideoPlayer_VideoExtents(ref w, ref h);
			return h;

		}

	}
	
	private Texture2D _videoTexture = null;
	public Texture2D videoTexture {

		get {

			int nativeTex = videoReady ? VideoPlayer_CurFrameTexture() : 0;
			if (nativeTex != 0) {

				if (this._videoTexture == null) {

					var w = 0;
					var h = 0;
					this.GetSize(out w, out h);
					this._videoTexture = Texture2D.CreateExternalTexture(w, h, TextureFormat.BGRA32, false, false, (System.IntPtr)nativeTex);
					this._videoTexture.filterMode = FilterMode.Bilinear;
					this._videoTexture.wrapMode = TextureWrapMode.Repeat;

				}

				this._videoTexture.UpdateExternalTexture((System.IntPtr)nativeTex);

			} else {

				this._videoTexture = null;

			}

			return this._videoTexture;

		}

	}

	public void Load(ResourceAsyncOperation asyncOperation, string filepath) {
		
		if (this.videoReady == true) {

			asyncOperation.SetValues(isDone: true, progress: 1f, asset: this.videoTexture);
			//callback.Invoke();
			return;

		}
		
		Debug.Log("LOAD VideoPlayerInterface: " + this.filepath);
		this.filepath = filepath;
		this.waitingForLoad += () => {

			asyncOperation.SetValues(isDone: true, progress: 1f, asset: this.videoTexture);
			//if (callback != null) callback.Invoke();

		};

		if (VideoPlayer_Load(this.filepath) == true) {

			asyncOperation.SetValues(isDone: true, progress: 1f, asset: null);
			Debug.Log("Failed to load: " + this.filepath);

		}

	}

	public void GetSize(out int w, out int h) {
		
		w = 0;
		h = 0;
		VideoPlayer_VideoExtents(ref w, ref h);

	}
	
	public static bool CanOutputToTexture(string filename) {
		
		return VideoPlayer_CanOutputToTexture(filename);
		
	}
	
	public bool Play(string filename) {

		this.filepath = filename;
		
		Debug.Log("PLAY VideoPlayerInterface: " + this.filepath);
		if (VideoPlayerInterface.CanOutputToTexture(filename) == true) {
			
			Debug.Log("PlayVideo");
			return VideoPlayer_Play();

		}

		return false;
		
	}
	
	public bool Play(string filename, bool loop) {
		
		this.filepath = filename;
		
		Debug.Log("PLAY LOOP VideoPlayerInterface: " + this.filepath);
		if (VideoPlayerInterface.CanOutputToTexture(filename) == true) {
			
			Debug.Log("PlayVideo with loop");
			VideoPlayer_SetLoop(loop);
			return VideoPlayer_Play();
			
		}
		
		return false;
		
	}
	
	public void Pause() {
		
		VideoPlayer_Pause();
		
	}
	
	public void Stop() {
		
		VideoPlayer_Stop();
		
	}

	public bool IsPlaying() {

		return VideoPlayer_IsPlaying();

	}

};
