using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows.Plugins.Flow;
using UnityEditor.UI.Windows.Plugins.Flow;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using ME;
using UnityEngine.UI.Windows.Plugins.FlowCompiler;
using FD = UnityEngine.UI.Windows.Plugins.Flow.Data;
using UnityEditor.UI.Windows.Plugins.Flow.Audio;
using UnityEditorInternal;
using UnityEngine.UI.Windows.Audio;
using UnityEditor.UI.Windows.Audio;

namespace UnityEditor.UI.Windows.Plugins.Audio {

	public class Audio : FlowAddon {

		private ReorderableList audioMusicList;
		private ReorderableList audioFXList;

		public override void OnFlowSettingsGUI() {
			
			GUILayout.Label(FlowAddon.MODULE_INSTALLED, EditorStyles.centeredGreyMiniLabel);

			AudioSettingsEditor.DrawList(ref this.audioMusicList, "Music", ClipType.Music);
			AudioSettingsEditor.DrawList(ref this.audioFXList, "FX", ClipType.SFX);

		}

		public override GenericMenu GetSettingsMenu(GenericMenu menu) {

			if (menu == null) menu = new GenericMenu();
			menu.AddItem(new GUIContent("Setup"), false, () => {
				
				if (EditorUtility.DisplayDialog("Warning!",
				                                "You want to setup all audio in your screens. It may causes some problems with id's. Do you want to continue?",
				                                "Yes, go ahead",
				                                "No") == true) {
					
					var data = FlowSystem.GetData();
					if (data == null) return;

					data.audio.Setup();
					
				}
				
			});

			return menu;

		}

		private GUIStyle layoutBoxStyle;
		public override void OnFlowWindowGUI(FD.FlowWindow window) {

			var data = FlowSystem.GetData();
			if (data == null) return;

			if (data.modeLayer == ModeLayer.Audio) {

				if (window.IsContainer() == true ||
				    window.IsSmall() == true ||
				    window.IsShowDefault() == true)
					return;

				var screen = window.GetScreen();
				if (screen != null) {

					GUILayout.BeginHorizontal();
					{
						var clipType = (int)screen.audio.clipType;
						clipType = GUILayoutExt.Popup(clipType, new string[2] { "Keep Current", "Restart If Equals" }, FlowSystemEditorWindow.defaultSkin.label, GUILayout.Width(EditorGUIUtility.labelWidth));
						screen.audio.clipType = (ClipType)clipType;
						
						var newId = AudioPopupEditor.DrawLayout(screen.audio.id, screen.audio.clipType, screen.audio.flowData.audio, null);
						if (newId != screen.audio.id) {

							screen.audio.id = newId;
							window.audioEditor = null;

						}

					}
					GUILayout.EndHorizontal();
					
					var state = data.audio.GetState(screen.audio.clipType, screen.audio.id);
					if (state != null && state.clip != null) {
						
						GUILayout.BeginVertical();
						{
							if (this.layoutBoxStyle == null) {
								
								this.layoutBoxStyle = FlowSystemEditorWindow.defaultSkin.FindStyle("LayoutBox");
								
							}
							
							GUILayout.Box(string.Empty, this.layoutBoxStyle, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
							var rect = GUILayoutUtility.GetLastRect();

							if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition) == true) {

								window.audioEditor = null;

							}

							if (window.audioEditor == null) window.audioEditor = Editor.CreateEditor(state.clip);
							window.audioEditor.OnPreviewGUI(rect, EditorStyles.helpBox);
							GUILayout.BeginHorizontal();
							window.audioEditor.OnPreviewSettings();
							GUILayout.EndHorizontal();

						}
						GUILayout.EndVertical();
						
					}

				}

			}

		}

		public override bool InstallationNeeded() {

			return false;

		}

	}

}