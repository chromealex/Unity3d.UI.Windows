using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI.Windows;
using UnityEngine.UI.Windows.Plugins.Flow;
using System.Linq;
using UnityEditorInternal;
using ME;
using System.Reflection;
using UnityEngine.UI.Windows.Types;

namespace UnityEditor.UI.Windows.Plugins.Flow {

	public class FlowSystemEditor {
		
		public static GUIStyle[] GetTagStyles() {
			
			return ME.Utilities.Cache<GUIStyle[]>("FlowEditor.Styles.Tags", () => {
				
				return new GUIStyle[7] {
					
					new GUIStyle("sv_label_1"),
					new GUIStyle("sv_label_2"),
					new GUIStyle("sv_label_3"),
					new GUIStyle("sv_label_4"),
					new GUIStyle("sv_label_5"),
					new GUIStyle("sv_label_6"),
					new GUIStyle("sv_label_7")
						
				};
				
			});
			
		}
		
		public static GUIStyle[] GetTagStylesEdited() {
			
			return ME.Utilities.Cache<GUIStyle[]>("FlowEditor.Styles.TagsEdit", () => {
				
				return new GUIStyle[7] {
					
					new GUIStyle("sv_label_1"),
					new GUIStyle("sv_label_2"),
					new GUIStyle("sv_label_3"),
					new GUIStyle("sv_label_4"),
					new GUIStyle("sv_label_5"),
					new GUIStyle("sv_label_6"),
					new GUIStyle("sv_label_7")
						
				};
				
			});
			
		}

		public static Rect GetCenterRect(EditorWindow editorWindow, float width, float height) {
			
			var size = editorWindow.position;
			
			return new Rect(size.width * 0.5f - width * 0.5f, size.height * 0.5f - height * 0.5f, width, height);
			
		}
		
		public static Rect GetCenterRect(Rect rect, float width, float height) {
			
			var size = rect;
			
			return new Rect(size.width * 0.5f - width * 0.5f, size.height * 0.5f - height * 0.5f, width, height);
			
		}

		public static Rect Scale(Rect realRect, Rect minMaxRect, Rect toPosition, Vector2 offset) {

			var width = minMaxRect.width;
			var height = minMaxRect.height;

			return new Rect(realRect.x / width * toPosition.width + offset.x, realRect.y / height * toPosition.height + offset.y, realRect.width / width * toPosition.width, realRect.height / height * toPosition.height);

		}

	}

}
