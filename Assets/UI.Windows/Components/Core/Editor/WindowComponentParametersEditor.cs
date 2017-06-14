using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows.Components;
using System.Reflection;
using System.Linq;
using UnityEngine.UI.Windows.Types;

namespace UnityEditor.UI.Windows.Components {

	public class ParametersEditor : UnityEditor.Editor, IParametersEditor {
		
		public virtual void OnParametersGUI(Rect rect) {}
		public virtual float GetHeight() { return 0f; }
		
	}

	[CanEditMultipleObjects()]
	[CustomEditor(typeof(WindowComponentParametersBase), editorForChildClasses: true)]
	public class WindowComponentParametersEditor : ParametersEditor {

		private WindowComponentParametersBase parameters;
		private FieldInfo[] fields;
		private SerializedProperty[] properties;
		private long[] values;
		private float referenceHeight;

		public void OnEnable() {

			this.VerifyParameters();

		}

		private void VerifyParameters() {

			this.parameters = this.target as WindowComponentParametersBase;
			this.fields = this.parameters.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public);

			this.values = new long[this.fields.Length];
			this.properties = new SerializedProperty[this.fields.Length];
			for (int i = 0; i < this.properties.Length; ++i) {

				this.properties[i] = this.serializedObject.FindProperty(this.fields[i].Name);
				var attrs = this.fields[i].GetCustomAttributes(false);
				if (attrs != null && attrs.Length > 0) {

					var attr = attrs.FirstOrDefault((a) => a is ParamFlagAttribute) as ParamFlagAttribute;
					if (attr != null) this.values[i] = attr.flag;

				}

			}

			this.referenceHeight = 16f;
			/*var referenceField = this.fields.FirstOrDefault((f) => f.GetCustomAttributes(false).Count() == 0);
			var referenceIndex = System.Array.IndexOf(this.fields, referenceField);
			if (referenceIndex >= 0) {
				
				this.referenceHeight = EditorGUI.GetPropertyHeight(this.properties[referenceIndex]);
				
			}*/

		}

		public override void OnInspectorGUI() {

			var referenceName = "Null";
			var found = false;

			if (this.serializedObject.isEditingMultipleObjects == true) {

				referenceName = "[Multiply Values]";
				found = true;

			} else {

				found = this.GetReferenceName(ref referenceName);

			}

			EditorGUILayout.HelpBox(string.Format("Do not destroy this object. Referenced by: {0}.", referenceName) + (found == false ? " You can remove this item." : string.Empty), MessageType.Warning);

			if (found == false) {

				Component.DestroyImmediate(this.target, allowDestroyingAssets: true);

			}

			/*if (this.targets.Length != 1) {

				EditorGUILayout.HelpBox("Can't edit multiply objects.", MessageType.Warning);
				return;

			}

			var oldState = GUI.enabled;
			GUI.enabled = false;
			this.DrawDefaultInspector();
			GUI.enabled = oldState;*/

		}

		public bool GetReferenceName(ref string referenceName) {

			var found = false;
			var parameters = this.target as WindowComponentParametersBase;

			var linkerComponent = parameters.GetComponent<LinkerComponent>();
			if (linkerComponent != null) {

				if (linkerComponent.prefabParameters == parameters) {

					referenceName = string.Format("[{0}] LinkerComponent", System.Array.IndexOf(linkerComponent.transform.GetComponents<Component>(), linkerComponent));
					found = true;
					return found;

				}

			}

			var layoutWindow = parameters.GetComponent<LayoutWindowType>();
			if (layoutWindow != null) {

				for (int i = 0; i < layoutWindow.layouts.layouts.Length; ++i) {

					var layout = layoutWindow.layouts.layouts[i];
					for (int j = 0; j < layout.components.Length; ++j) {

						var component = layout.components[j];
						if (component.componentParameters == parameters) {

							referenceName = string.Format("[{0}] {1}", layoutWindow.layouts.types[i], component.description);
							found = true;
							return found;

						}

					}

				}

			}

			return found;

		}

		public override void OnParametersGUI(Rect rect) {

			this.VerifyParameters();

			if (this == null || this.serializedObject == null) return;

			const float toggleWidth = 30f;
			const float space = 2f;

			var oldState = GUI.enabled;
			GUI.enabled = false;

			this.serializedObject.Update();

			var height = 0f;
			for (int i = 0; i < this.properties.Length; ++i) {
				
				GUI.enabled = true;
				
				var property = this.properties[i];
				var value = this.values[i];
				var state = this.parameters.IsChanged(value);

				height = EditorGUI.GetPropertyHeight(property);

				var offset = height - this.referenceHeight;

				GUI.enabled = state;

				if (state == true) {

					var bRect = new Rect(rect);
					bRect.height = height;

					var style = new GUIStyle(EditorStyles.label);
					style.normal.background = Texture2D.whiteTexture;
					var oldColor = GUI.backgroundColor;
					GUI.backgroundColor = new Color(1f, 1f, 1f, 0.2f);
					GUI.Box(bRect, string.Empty, style);
					GUI.backgroundColor = oldColor;

				}

				var cRect = new Rect(rect);
				cRect.x += toggleWidth;
				cRect.width -= toggleWidth;
				cRect.height = height;
				if (property.propertyType == SerializedPropertyType.String) {

					var offsetX = 16f;
					GUI.Label(new Rect(cRect.x + offsetX, cRect.y, EditorGUIUtility.labelWidth, cRect.height), property.displayName);
					property.stringValue = GUI.TextArea(new Rect(cRect.x + EditorGUIUtility.labelWidth, cRect.y, cRect.width - EditorGUIUtility.labelWidth, cRect.height), property.stringValue);

				} else {
					
					EditorGUI.PropertyField(cRect, property, includeChildren: true);

				}

				GUI.enabled = true;

				var tRect = new Rect(rect);
				tRect.x += 5f;
				tRect.y += offset;
				tRect.width = toggleWidth;
				tRect.height = this.referenceHeight;
				var val = EditorGUI.Toggle(tRect, state);
				if (val != state) this.parameters.SetChanged(value, val);

				rect.y += height + space;

			}
			
			this.serializedObject.ApplyModifiedProperties();

			GUI.enabled = oldState;

		}

		public override float GetHeight() {
			
			const float space = 2f;

			var height = 0f;
			for (int i = 0; i < this.properties.Length; ++i) {

				var property = this.properties[i];
				height += EditorGUI.GetPropertyHeight(property) + space;

			}

			return height;

		}

	}

}