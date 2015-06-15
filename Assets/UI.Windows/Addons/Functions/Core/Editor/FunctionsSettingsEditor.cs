using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEngine.UI.Windows.Plugins.Flow;
using System.Collections.Generic;

namespace UnityEditor.UI.Windows.Plugins.Functions {

	public class FunctionsSettingsEditor : Editor {

		private List<FlowWindow> functions = new List<FlowWindow>();
		/*
		public void OnEnable() {

			var dataPath = AssetDatabase.GetAssetPath(this.target);
			var directory = System.IO.Path.GetDirectoryName(dataPath);

		}*/

		public override void OnInspectorGUI() {

			GUILayout.Label("Functions", EditorStyles.boldLabel);

			var data = FlowSystem.GetData();
			if (data == null) return;

			this.functions.Clear();
			foreach (var window in data.windows) {

				if (window.IsFunction() == true &&
				    window.IsContainer() == true) {

					this.functions.Add(window);

				}

			}

			foreach (var function in this.functions) {

				GUILayout.Button(function.title);

			}

		}

	}

}