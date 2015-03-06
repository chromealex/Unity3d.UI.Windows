using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class DevicePreviewCamera : MonoBehaviour {

	private RenderTexture targetTexture;

	public Material material;

	public void Initialize(RenderTexture texture) {
		
		//this.camera.depthTextureMode = DepthTextureMode.Depth;
		this.targetTexture = texture;

		this.Render();

	}

	public void CleanUp() {

		this.camera.targetTexture = null;
		GameObject.DestroyImmediate(this.gameObject);

	}

	/*public Texture2D tmp;
	public bool needScreen;
	public void Update() {

		if (this.needScreen == true) {

			this.tmp = this.TakeScreenshot();
			this.needScreen = false;

		}

	}*/

	public void Render() {

		this.camera.Render();

	}

	public void OnRenderImage(RenderTexture src, RenderTexture dest) {

		this.targetTexture = src;
		RenderTexture.active = src;
		Graphics.Blit(src, dest);//, this.material);

	}

	public void OnPreRender() {

	}

	public void OnPostRender() {

	}

	public Texture2D TakeScreenshot() {

		var width = targetTexture.width;
		var height = targetTexture.height;

		Texture2D screenshot = new Texture2D(width, height, TextureFormat.RGB24, false);

		this.camera.targetTexture = targetTexture;
		this.camera.Render();

		RenderTexture.active = targetTexture;

		screenshot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
		screenshot.Apply(false);

		this.camera.targetTexture = null;

		RenderTexture.active = null;

		return screenshot;

	}

}
