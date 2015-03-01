using System.Linq;
using UnityEditor;
using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows;

[CustomEditor(typeof(WindowLayoutElement))]
public class WindowLayoutElementEditor : Editor {

	private WindowLayoutElement target {

		get { return base.target as WindowLayoutElement; }
	}

	public void OnSceneGUI() {

		var ownerLayout = target.transform.root.GetComponentInChildren<WindowLayout>();
		
		foreach ( var each in ownerLayout.elements) {

			each.DrawHandle( isActive: each == target );	
		}
	}
}
