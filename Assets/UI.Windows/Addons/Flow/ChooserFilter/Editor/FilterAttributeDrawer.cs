using UnityEditor;
using UnityEditor.UI.Windows.Plugins.Flow;
using UnityEditor.UI.Windows;
using UnityEngine;

[CustomPropertyDrawer(typeof(ComponentChooserAttribute))]
public class FilterDrawer : PropertyDrawer {
	
	public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
		
		return EditorGUI.GetPropertyHeight(property, label, true);
		
	}
	
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
		
		GUI.BeginGroup(position);
		{
			
			var width = 30f;
			var offset = 5f;

			property.serializedObject.Update();

			EditorGUI.PropertyField(new Rect(0f, 0f, position.width - width - offset, position.height), property, label, true);
			
			if (GUI.Button(new Rect(position.width - width, 0f, width, position.height), "...") == true) {

				WindowComponentLibraryChooser.Show((element) => {

					property.objectReferenceValue = element.mainComponent;
					property.serializedObject.ApplyModifiedProperties();

				});

			}
			
		}
		GUI.EndGroup();
		
	}
	
}