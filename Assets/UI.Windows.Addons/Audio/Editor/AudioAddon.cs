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
using UnityEngine.UI.Windows;

namespace UnityEditor.UI.Windows.Plugins.Audio {

	public class Audio : FlowAddon {

		private ReorderableList audioMusicList;
		private ReorderableList audioFXList;

		public override void Reset() {
			
			this.audioMusicList = null;
			this.audioFXList = null;

		}

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

		public override void OnFlowWindowGUI(FD.FlowWindow window) {

			var data = FlowSystem.GetData();
			if (data == null) return;

			if (data.modeLayer == ModeLayer.Audio) {

				if (window.IsContainer() == true ||
				    window.IsSmall() == true ||
				    window.IsShowDefault() == true)
					return;

				var screen = window.GetScreen().Load<WindowBase>();
				if (screen != null) {

					GUILayout.BeginHorizontal();
					{
						var playType = (int)screen.audio.playType;
						playType = GUILayoutExt.Popup(playType, new string[2] { "Keep Current", "Replace" }, FlowSystemEditorWindow.defaultSkin.label, GUILayout.Width(EditorGUIUtility.labelWidth));
						screen.audio.playType = (UnityEngine.UI.Windows.Audio.Window.PlayType)playType;

						var rect = GUILayoutUtility.GetLastRect();

						/*var newId = */AudioPopupEditor.Draw(new Rect(rect.x + rect.width, rect.y, window.rect.width - EditorGUIUtility.labelWidth - 10f, rect.height), screen.audio.id, (result) => {

							screen.audio.id = result;
							window.audioEditor = null;

						}, screen.audio.clipType, screen.audio.flowData.audio, null);
						/*if (newId != screen.audio.id) {

							screen.audio.id = newId;
							window.audioEditor = null;

						}*/

					}
					GUILayout.EndHorizontal();
					
					var state = data.audio.GetState(screen.audio.clipType, screen.audio.id);
					if (state != null && state.clip != null) {
						
						GUILayout.BeginVertical();
						{

							GUILayout.Box(string.Empty, FlowSystemEditorWindow.styles.layoutBoxStyle, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
							var rect = GUILayoutUtility.GetLastRect();

							if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition) == true) {

								window.audioEditor = null;

							}

							if (window.audioEditor == null) {

								EditorPrefs.SetBool("AutoPlayAudio", false);
								window.audioEditor = Editor.CreateEditor(state.clip);
								//System.Type.GetType("AudioUtil").InvokeMember("StopClip", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public, null, null, new object[] { state.clip });

							}

							if (Event.current.type != EventType.MouseDrag && Event.current.type != EventType.DragPerform) {

								window.audioEditor.OnPreviewGUI(rect, EditorStyles.helpBox);
								GUILayout.BeginHorizontal();
								window.audioEditor.OnPreviewSettings();
								GUILayout.EndHorizontal();

							}

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