using UnityEngine;
using System.Collections;
using ME;
using System.Collections.Generic;

namespace UnityEngine.UI.Windows.Plugins.Analytics.Services {

	public class UIWService : AnalyticsService {
		
		public override string GetPlatformName() {
			
			return "UIWAnalytics";
			
		}

		public override IEnumerator Auth(string key) {

			yield return false;

		}

		public override IEnumerator OnEvent(int screenId, string group1, string group2, string group3, int weight) {

			yield return false;

		}

		public override IEnumerator OnScreenTransition(int index, int screenId, int toScreenId) {
			
			yield return false;

		}
		
		public override IEnumerator OnTransaction(int screenId, string productId, decimal price, string currency, string receipt, string signature) {
			
			yield return false;
			
		}
		
		public override IEnumerator OnScreenPoint(int screenId, int screenWidth, int screenHeight, byte tag, float x, float y) {
			
			yield return false;
			
		}

		#if UNITY_EDITOR
		public override void OnInspectorGUI(UnityEngine.UI.Windows.Plugins.Heatmap.Core.HeatmapSettings settings, AnalyticsItem item, System.Action onReset, GUISkin skin) {

			UnityEditor.EditorGUILayout.LabelField("Key:");
			item.authKey = UnityEditor.EditorGUILayout.TextArea(item.authKey, skin.textArea);

			CustomGUI.Splitter();

			GUI.changed = item.userFilter.OnGUI(skin) || GUI.changed;

			CustomGUI.Splitter();

			if (GUI.changed == true) {
				
				item.isChanged = true;

			}

			UnityEditor.EditorGUI.BeginDisabledGroup(!item.isChanged);
			if (GUILayout.Button("Apply", skin.button) == true) {

				item.isChanged = false;
				GUI.changed = true;

				if (onReset != null) onReset.Invoke();

			}
			UnityEditor.EditorGUI.EndDisabledGroup();

			item.show = UnityEditor.EditorGUILayout.ToggleLeft("Show", item.show);

			if (GUI.changed == true) {

				UnityEditor.EditorUtility.SetDirty(settings);

			}

		}
		#endif

	}

}