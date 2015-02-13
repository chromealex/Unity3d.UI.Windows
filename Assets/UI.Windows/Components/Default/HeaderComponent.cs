using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows;
using UnityEngine.UI;

[ExecuteInEditMode()]
public class HeaderComponent : WindowComponent {

	[HideInInspector][SerializeField]
	public Text text;

	public void SetValue(string text) {

		if (this.text != null) this.text.text = text;

	}

#if UNITY_EDITOR
	public void Update() {

		if (Application.isPlaying == true) return;

		this.text = this.GetComponent<Text>();

	}
#endif

}
