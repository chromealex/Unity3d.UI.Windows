using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows;

public class UIWindowExampleStart : MonoBehaviour {

	public void Start() {

		WindowSystem.ShowDefault();

	}

	public void OnGUI() {
		
		if (GUILayout.Button("Hide All", GUILayout.Width(200f)) == true) {
			
			WindowSystem.HideAll();

		}
		
		if (GUILayout.Button("Hide All and Free Memory", GUILayout.Width(200f)) == true) {
			
			WindowSystem.HideAllAndClean();
			
		}

		if (GUILayout.Button("Show Default", GUILayout.Width(200f)) == true) {

			WindowSystem.ShowDefault();

		}

		var content = new GUIContent("UI.Windows Extension. Version 0.1a. You can e-mail me: chrome.alex@gmail.com or use repo to get updates: https://github.com/chromealex/Unity3d.UI.Windows");
		var height = GUILayoutUtility.GetRect(content, GUI.skin.label).height;
		GUI.Label(new Rect(0f, Screen.height - height, Screen.width, height), content);

	}

}
