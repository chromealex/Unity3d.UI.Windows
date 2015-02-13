using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows;
using UnityEngine.UI;

[ExecuteInEditMode()]
public class DecoratorComponent : WindowComponent {
	
	[HideInInspector][SerializeField]
	public Image image;
	
	[HideInInspector][SerializeField]
	public RawImage rawImage;
	
	public void SetImage(Sprite sprite) {
		
		if (this.image != null) this.image.sprite = sprite;
		
	}
	
	public void SetImage(Texture2D texture) {
		
		if (this.rawImage != null) this.rawImage.texture = texture;
		
	}

	#if UNITY_EDITOR
	public void Update() {
		
		if (Application.isPlaying == true) return;
		
		this.image = this.GetComponent<Image>();
		this.rawImage = this.GetComponent<RawImage>();
		
	}
	#endif

}
