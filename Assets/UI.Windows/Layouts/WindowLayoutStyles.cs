using UnityEngine;
using System.Collections;

namespace UnityEngine.UI.Windows.Styles {

	public class WindowLayoutStyles {
		
		public const int MAX_DEPTH = 6;
		
		public class Styles {
			
			public GUISkin skin;
			public GUIStyle[] boxes;
			public GUIStyle[] boxesSelected;
			public GUIStyle boxSelected;
			public GUIStyle layoutElementSelected;
			public GUIStyle layoutElementHighlighted;
			
			public Styles() {

				#if UNITY_EDITOR
				this.skin = Resources.Load<GUISkin>("UI.Windows/Core/Styles/Boxes/" + (UnityEditor.EditorGUIUtility.isProSkin == true ? "SkinDark" : "SkinLight"));
				this.boxes = new GUIStyle[WindowLayoutStyles.MAX_DEPTH] {
					
					this.skin.FindStyle("flow node 0"),
					this.skin.FindStyle("flow node 1"),
					this.skin.FindStyle("flow node 2"),
					this.skin.FindStyle("flow node 3"),
					this.skin.FindStyle("flow node 4"),
					this.skin.FindStyle("flow node 5")
					
				};
				this.boxesSelected = new GUIStyle[WindowLayoutStyles.MAX_DEPTH] {
					
					this.skin.FindStyle("flow node 0"), // on
					this.skin.FindStyle("flow node 1"),
					this.skin.FindStyle("flow node 2"),
					this.skin.FindStyle("flow node 3"),
					this.skin.FindStyle("flow node 4"),
					this.skin.FindStyle("flow node 5")
					
				};
				
				this.boxSelected = this.skin.FindStyle("flow node 6");
				this.layoutElementSelected = this.skin.FindStyle("LayoutElementSelected");
				this.layoutElementHighlighted = this.skin.FindStyle("LayoutElementHighlighted");
				#endif

			}

			public GUIStyle GetInstanceByName(string name) {

				//name = name.Replace(" on", string.Empty);

				//return new GUIStyle(this.skin.FindStyle(name));

				return new GUIStyle(name);

			}

		}

		private static Styles _styles;
		public static Styles styles {

			get {

				if (WindowLayoutStyles._styles == null) {

					WindowLayoutStyles._styles = new Styles();

				}

				return WindowLayoutStyles._styles;

			}

		}

	}

}