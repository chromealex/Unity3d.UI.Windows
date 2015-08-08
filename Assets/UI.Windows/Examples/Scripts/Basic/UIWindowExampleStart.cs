using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows;

[ExecuteInEditMode()]
public class UIWindowExampleStart : MonoBehaviour {

	public string exampleTitle;
	public string version;
	public GUIStyle largeLabel;
	public GUIStyle toolbarButton;

	public void Start() {
		
		if (Application.isPlaying == false) return;

		WindowSystem.ShowDefault();

	}

#if UNITY_EDITOR
	public void LateUpdate() {
		
		this.version = UnityEditor.UI.Windows.VersionInfo.BUNDLE_VERSION.ToString();

	}
#endif

	public void OnGUI() {

		if (Application.isPlaying == false) return;

		var headerStyle = this.largeLabel;
		var buttonStyle = this.toolbarButton;

		GUILayout.BeginHorizontal();

		if (GUILayout.Button("Hide All", buttonStyle, GUILayout.Width(200f)) == true) {
			
			WindowSystem.HideAll();

		}
		
		if (GUILayout.Button("Hide All and Free Memory", buttonStyle, GUILayout.Width(200f)) == true) {
			
			WindowSystem.HideAllAndClean();
			
		}
		
		if (GUILayout.Button("Show Default", buttonStyle, GUILayout.Width(200f)) == true) {
			
			WindowSystem.ShowDefault();
			
		}
		
		if (GUILayout.Button("Version " + this.version + " (Check for Update)", buttonStyle, GUILayout.Width(200f)) == true) {
			
			Application.OpenURL("https://github.com/chromealex/Unity3d.UI.Windows");
			
		}

		GUILayout.Label(string.Empty, buttonStyle);

		GUILayout.EndHorizontal();
		
		GUILayout.Label(this.exampleTitle, headerStyle);

		var content = new GUIContent("UI.Windows Extension. Version " + this.version + ". You can e-mail me chrome.alex@gmail.com or use repo to get updates: https://github.com/chromealex/Unity3d.UI.Windows");
		var height = GUILayoutUtility.GetRect(content, GUI.skin.label).height;
		GUI.Label(new Rect(0f, Screen.height - height, Screen.width, height), content);

	}

}
