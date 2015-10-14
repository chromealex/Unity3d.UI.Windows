using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows.Plugins.Flow;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UI.Windows.Extensions;
#endif

public class FlowDataPopupAttribute : ConditionAttribute {
	
	public FlowDataPopupAttribute() : base() {}
	
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(FlowDataPopupAttribute))]
public class FlowDataPopupAttributeDrawer : PropertyDrawer {
	
	public static bool IsEnabled(PropertyDrawer drawer, SerializedProperty property) {
		
		return PropertyExtensions.IsEnabled<ReadOnlyAttribute>(drawer, property);
		
	}
	
	public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
		
		return EditorGUI.GetPropertyHeight(property, label, true) + 2f;
		
	}
	
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {

		var files = ME.EditorUtilities.GetAssetsOfType<FlowData>();
		if (files != null && files.Length > 0) {
			
			var selected = string.Empty;
			long mod = 0;
			var index = System.Array.IndexOf(files, property.objectReferenceValue);

			if (index == -1) {

				for (int i = 0; i < files.Length; ++i) {

					if (files[i].lastModifiedUnix > mod) {

						mod = files[i].lastModifiedUnix;
						index = i;

						property.objectReferenceValue = files[index];
						property.serializedObject.ApplyModifiedPropertiesWithoutUndo();

					}

				}

			}

			var keys = new int[files.Length];
			var options = new GUIContent[files.Length];
			for (int i = 0; i < options.Length; ++i) {

				keys[i] = i;
				options[i] = new GUIContent(files[i].name);

				if (index == i) {

					selected = options[i].text;

				}

			}

			Popup.DrawInt(position, selected, label, (result) => {

				if (result >= 0) {
					
					property.objectReferenceValue = files[result];
					property.serializedObject.ApplyModifiedPropertiesWithoutUndo();
					
				}

			}, options, keys);

		} else {

			EditorGUI.PropertyField(position, property, label, true);

		}

	}
	
}
#endif