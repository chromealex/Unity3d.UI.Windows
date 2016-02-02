using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEditor.UI.Windows.Plugins.Flow;
using ME;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System;
using UnityEngine.UI.Windows.Plugins.GameData;
using UnityEngine.UI.Windows.Plugins.Flow;
using UnityEngine.UI.Windows.Plugins.Services;
using UnityEditor.UI.Windows.Plugins.Services;

namespace UnityEditor.UI.Windows.Plugins.GameData {

	[CustomEditor(typeof(GameDataSettings))]
	public class GameDataSettingsEditor : ServiceSettingsEditor {

		public override string GetServiceName() {

			return "GameData";

		}

		public override string GetNamespace() {

			return "UnityEngine.UI.Windows.Plugins.GameData.Services";

		}
		
		public override void OnInspectorGUI() {

			this.DrawServices<GameDataServiceItem>();
			
			CustomGUI.Splitter();
			
			GUILayout.Label("Versions", EditorStyles.boldLabel);

			EditorGUI.BeginDisabledGroup(true);

			var currentVersions = GameDataSystem.GetVersionsList();
			var currentVersion = GameDataSystem.GetCurrentVersion();
			foreach (var ver in currentVersions) {

				GUILayout.Toggle(currentVersion == ver, ver.ToString());

			}
			
			EditorGUI.EndDisabledGroup();

		}

	}

}