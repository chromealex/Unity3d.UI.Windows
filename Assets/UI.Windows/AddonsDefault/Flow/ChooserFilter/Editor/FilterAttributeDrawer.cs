using UnityEditor;
using UnityEditor.UI.Windows.Plugins.Flow;
using UnityEditor.UI.Windows;
using UnityEngine;
using ME;
using UnityEditor.UI.Windows.Extensions;

namespace UnityEngine.UI.Windows.Extensions {

	[CustomPropertyDrawer(typeof(ComponentChooserAttribute))]
	public class FilterDrawer : PropertyDrawer {
		
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
			
			return EditorGUI.GetPropertyHeight(property, label, true);
			
		}
		
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {

			var offset = new Vector2(position.x, position.y);

			//GUI.BeginGroup(position);
			{
				
				var width = 30f;
				var margin = 5f;

				//property.serializedObject.Update();

				EditorGUI.PropertyField(new Rect(offset.x, offset.y, position.width - width - margin, position.height), property, label, true);
				GUILayoutExt.DrawComponentChooser(new Rect(position.width - width + offset.x, offset.y, width, position.height), (property.serializedObject.targetObject as WindowComponent).gameObject, property.objectReferenceValue as WindowComponent, (component) => {

					property.objectReferenceValue = component;
					property.serializedObject.ApplyModifiedProperties();

				});

				//property.serializedObject.ApplyModifiedProperties();

			}
			//GUI.EndGroup();
			
		}
		
	}

}