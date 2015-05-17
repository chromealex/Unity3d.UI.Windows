using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEngine.UI.Windows.Plugins.Heatmap.Core;

namespace UnityEditor.UI.Windows.Plugins.Heatmap {

	[CustomEditor(typeof(HeatmapSettings))]
	public class HeatmapSettingsEditor : Editor {

		public override void OnInspectorGUI() {
			
			var target = this.target as HeatmapSettings;
			
			GUILayout.Label("Settings", EditorStyles.boldLabel);
			EditorGUILayout.BeginVertical(EditorStyles.helpBox);
			{
				target.show = EditorGUILayout.ToggleLeft("Show", target.show, EditorStyles.boldLabel);
				EditorGUILayout.LabelField("Key:");
				target.key = EditorGUILayout.TextArea(target.key);
			}
			EditorGUILayout.EndVertical();

			if (GUI.changed == true) {

				target.SetChanged();

			}

		}

	}

}