using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEngine.UI.Windows.Audio;
using UnityEngine.UI.Windows;
using System.Collections.Generic;
using System;
using UnityEditor.UI.Windows.Extensions;
using UnityEditorInternal;
using UnityEditor.UI.Windows.Plugins.Flow;
using UnityEngine.UI.Windows.Plugins.Flow;
using ME;

namespace UnityEditor.UI.Windows.Plugins.Flow.Audio {

	public static class AudioSettingsEditor {

		public static void DrawList(ref ReorderableList reorderableList, string label, ClipType clipType) {
			
			if (reorderableList == null) {

				var objectField = ME.Utilities.CacheStyle("UI.Windows.ObjectFieldSmall", "ObjectFieldSmall", (name) => FlowSystemEditorWindow.defaultSkin.FindStyle(name));
				var textField = ME.Utilities.CacheStyle("UI.Windows.TextFieldSmall", "TextFieldSmall", (name) => FlowSystemEditorWindow.defaultSkin.FindStyle(name));

				var audioSources = FlowSystem.GetAudioItems(clipType);
				if (audioSources != null) {
					
					var sources = FlowSystem.GetAudioItems(clipType);
					
					reorderableList = new ReorderableList(audioSources, typeof(UnityEngine.UI.Windows.Audio.Data.State), true, true, true, true);
					
					reorderableList.onAddCallback += (list) => {
						
						FlowSystem.AddAudioItem(clipType, new UnityEngine.UI.Windows.Audio.Data.State());
						FlowSystem.SetDirty();
						
					};
					reorderableList.onRemoveCallback += (list) => {
						
						var index = list.index;
						if (index < 0 || index >= sources.Count) return;
						
						FlowSystem.RemoveAudioItem(clipType, (list.list[index] as UnityEngine.UI.Windows.Audio.Data.State).key);
						FlowSystem.SetDirty();
						
					};
					reorderableList.drawHeaderCallback += (rect) => {
						
						const float widthNumber = 20f;
						const float widthCategory = 70f;

						rect.x += 14f;

						var labelRect = new Rect(rect);
						labelRect.width = widthNumber;
						
						GUI.Label(labelRect, "#");

						var categoryRect = new Rect(rect);
						categoryRect.x += widthNumber;
						categoryRect.width = widthCategory;
						
						GUI.Label(categoryRect, "Category");
						
						rect.x += widthNumber + widthCategory;
						rect.width -= widthNumber + widthCategory;
						
						GUI.Label(rect, label);

					};
					reorderableList.drawElementCallback += (rect, index, active, focused) => {
						
						const float widthNumber = 20f;
						const float widthCategory = 70f;

						if (index < 0 || index >= sources.Count) return;
						
						var item = sources[index];
						
						var labelRect = new Rect(rect);
						labelRect.width = widthNumber;
						
						GUI.Label(labelRect, item.key.ToString(), EditorStyles.miniLabel);

						var categoryRect = new Rect(rect);
						categoryRect.x += widthNumber;
						categoryRect.width = widthCategory;
						
						item.category = EditorGUI.TextField(categoryRect, item.category, textField);

						rect.x += widthNumber + widthCategory;
						rect.width -= widthNumber + widthCategory;
						
						var clip = GUILayoutExt.ObjectField<AudioClip>(rect, item.clip, false, objectField);
						if (clip != item.clip) {
							
							item.clip = clip;
							FlowSystem.SetDirty();
							
						}
						
					};
					
				}
				
			}
			
			if (reorderableList != null) reorderableList.DoLayoutList();

		}

	}

}