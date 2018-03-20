using UnityEngine;

namespace UnityEngine.UI.Windows {

	public class WindowLayoutPreferences : ScriptableObject {

		[System.Serializable]
		public class Item {

			public TargetPreferences preferences = new TargetPreferences();
			[SerializeField]
			private bool fixedScale;
			[SerializeField]
			private Vector2 fixedScaleResolution = new Vector2(1024f, 768f);
			[SerializeField]
			[Range(0f, 1f)]
			private float matchWidthOrHeight = 0f;

			public bool IsFixedScale() {
				
				return this.fixedScale;

			}

			public Vector2 GetFixedScaleResolution() {

				return this.fixedScaleResolution;

			}

			public float GetMatchWidthOrHeight() {

				return this.matchWidthOrHeight;

			}

		}

		public Item[] items;

		[Header("Default")]
		[SerializeField]
		private bool fixedScale;
		[SerializeField]
		private Vector2 fixedScaleResolution = new Vector2(1024f, 768f);
		[SerializeField]
		[Range(0f, 1f)]
		private float matchWidthOrHeight = 0f;

		public bool IsFixedScale() {

			if (this.items != null) {

				for (int i = 0; i < this.items.Length; ++i) {

					if (this.items[i].preferences.GetRunOnAnyTarget() == true || this.items[i].preferences.IsValid() == true) {

						return this.items[i].IsFixedScale();

					}

				}

			}

			return this.fixedScale;

		}

		public Vector2 GetFixedScaleResolution() {

			if (this.items != null) {
				
				for (int i = 0; i < this.items.Length; ++i) {

					if (this.items[i].preferences.GetRunOnAnyTarget() == true || this.items[i].preferences.IsValid() == true) {

						return this.items[i].GetFixedScaleResolution();

					}

				}

			}

			return this.fixedScaleResolution;

		}

		public float GetMatchWidthOrHeight() {

			if (this.items != null) {
				
				for (int i = 0; i < this.items.Length; ++i) {

					if (this.items[i].preferences.GetRunOnAnyTarget() == true || this.items[i].preferences.IsValid() == true) {

						return this.items[i].GetMatchWidthOrHeight();

					}

				}

			}

			return this.matchWidthOrHeight;

		}

		#if UNITY_EDITOR
		[UnityEditor.MenuItem("Assets/Create/UI Windows/Layout Preferences")]
		public static void CreateInstance() {
			
			ME.EditorUtilities.CreateAsset<WindowLayoutPreferences>();
			
		}
		#endif


	}

}