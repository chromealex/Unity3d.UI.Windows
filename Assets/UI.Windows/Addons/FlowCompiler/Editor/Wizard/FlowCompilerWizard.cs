using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows.Plugins.Flow;
using UnityEngine.UI.Windows.Plugins.FlowCompiler;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.UI.Windows.Plugins.Flow;

namespace UnityEditor.UI.Windows.Plugins.FlowCompiler {

	public class FlowCompilerWizard : EditorWindowExt {

		public static object compilationSync = null;

		public static FlowCompilerWizard ShowEditor(System.Action onClose) {

			var rootWindow = EditorWindow.focusedWindow;
			
			var rootX = rootWindow.position.x;
			var rootY = rootWindow.position.y;
			var rootWidth = rootWindow.position.width;
			var rootHeight = rootWindow.position.height;

			var width = 600f;
			var height = 389f;

			FlowCompilerWizard editor = null;

			FlowCompilerWizard.FocusWindowIfItsOpen<FlowCompilerWizard>();
			editor = EditorWindow.focusedWindow as FlowCompilerWizard;

			if (editor == null) {

				editor = FlowCompilerWizard.CreateInstance<FlowCompilerWizard>();
				var title = "UI.Windows: Flow Compiler Wizard";
				#if !UNITY_4
				editor.titleContent = new GUIContent(title);
				#else
				editor.title = title;
				#endif
				editor.ShowUtility();

			}

			editor.position = new Rect(rootX + rootWidth * 0.5f - width * 0.5f, rootY + rootHeight * 0.5f - height * 0.5f, width, height);

			editor.compileNamespace = FlowSystem.GetData().namespaceName;
			editor.forceRecompile = FlowSystem.GetData().forceRecompile;
			editor.partIndex = 0;

			editor.image = Resources.Load("UI.Windows/FlowCompiler/WizardImage") as Texture;

			editor.maxSize = new Vector2(width, height);
			editor.minSize = editor.maxSize;

			FlowCompilerWizard.compilationSync = new object();

			editor.defaultSkin = FlowSystemEditorWindow.defaultSkin;

			return editor;

		}
		
		private GUISkin defaultSkin;

		private bool readyToNext = false;
		private int partIndex = 0;
		private int processPartIndex = 1;
		private int parts = 4;
		private string[] partTitles = new string[] { "Build Settings", "Tags", "Processing Files...", "Finishing" };
		private string[] partSteps = new string[] { "Build Settings", "Selecting Tags", "Processing Files", "Finishing" };

		private bool waitForCompileEnding;

		private Texture image;

		public override void Update() {
			
			if (this.partIndex <= this.processPartIndex && FlowCompilerWizard.compilationSync == null) {
				
				this.Close();
				return;
				
			}

			if (this.waitForCompileEnding == true && EditorApplication.isCompiling == false) {

				++this.partIndex;
				this.waitForCompileEnding = false;
				this.Repaint();

			}

		}

		public void OnGUI() {

			GUILayout.BeginHorizontal();
			{

				GUILayout.BeginVertical(GUILayout.Width(this.image.width), GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
				{

					var boxStyle = new GUIStyle(GUI.skin.box);
					boxStyle.margin = new RectOffset(0, 0, 0, 0);
					boxStyle.padding = new RectOffset(0, 0, 0, 0);
					GUILayout.Box(this.image, boxStyle, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

				}
				GUILayout.EndVertical();

				GUILayout.BeginVertical(GUILayout.ExpandWidth(true), GUILayout.Width(15f), GUILayout.MinWidth(15f));
				{
					GUILayout.FlexibleSpace();
				}
				GUILayout.EndVertical();

				GUILayout.BeginVertical(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
				{
					
					GUILayout.BeginHorizontal(GUILayout.ExpandHeight(true), GUILayout.Height(15f));
					{
						GUILayout.FlexibleSpace();
					}
					GUILayout.EndHorizontal();
					
					GUILayout.BeginVertical(GUILayout.ExpandHeight(true));
					{
						this.DrawSteps();
						GUILayout.FlexibleSpace();
					}
					GUILayout.EndVertical();

					GUILayout.BeginVertical();
					{

						switch (partIndex) {
							
						case 0:
							this.DrawPart1();
							break;
							
						case 1:
							this.DrawPart2();
							break;
							
						case 2:
							this.DrawPart3();
							break;
							
						case 3:
							this.DrawPart4();
							break;

						}

					}
					GUILayout.EndVertical();
					
					GUILayout.BeginHorizontal(GUILayout.ExpandHeight(true), GUILayout.Height(15f));
					{
						GUILayout.FlexibleSpace();
					}
					GUILayout.EndHorizontal();

				}
				GUILayout.EndVertical();
				
				GUILayout.BeginVertical(GUILayout.ExpandWidth(true), GUILayout.Width(15f), GUILayout.MinWidth(15f));
				{
					GUILayout.FlexibleSpace();
				}
				GUILayout.EndVertical();

			}
			GUILayout.EndHorizontal();

		}

		public void DrawSteps() {

			this.DrawHeader("Steps:");

			var selected = new GUIStyle(EditorStyles.boldLabel);
			var normal = new GUIStyle(EditorStyles.label);

			for (int i = 0; i < this.partSteps.Length; ++i) {

				GUILayout.Label((i + 1).ToString() + ". " + this.partSteps[i], this.partIndex == i ? selected : normal);

			}

		}

		public void DrawPart4() {
			
			this.DrawHeader(this.partTitles[this.partIndex]);
			
			GUILayout.FlexibleSpace();

			GUILayout.Label("All files were successfully compiled!");

			this.DrawBottom();

			this.readyToNext = true;

		}

		public void DrawPart3() {
			
			this.DrawHeader(this.partTitles[this.partIndex]);

			GUILayout.FlexibleSpace();

			if (EditorApplication.isCompiling == true) {

				this.waitForCompileEnding = true;
				EditorGUILayout.HelpBox("Wait until the project is compiled...", MessageType.None);

			}

			GUI.enabled = false;
			this.DrawBottom();
			GUI.enabled = true;

			this.readyToNext = false;

		}

		private Vector2 tagsScroll;
		public void DrawPart2() {
			
			this.DrawHeader(this.partTitles[this.partIndex]);

			GUILayout.FlexibleSpace();

			this.tagsScroll = GUILayout.BeginScrollView(this.tagsScroll);
			
			var tagStyles = new GUIStyle[7] {
				
				new GUIStyle("sv_label_1"),
				new GUIStyle("sv_label_2"),
				new GUIStyle("sv_label_3"),
				new GUIStyle("sv_label_4"),
				new GUIStyle("sv_label_5"),
				new GUIStyle("sv_label_6"),
				new GUIStyle("sv_label_7")
				
			};

			var allTags = FlowSystem.GetData().tags;
			foreach (var tag in allTags) {
				
				var tagStyle = tagStyles[tag.color];
				tagStyle.alignment = TextAnchor.MiddleLeft;
				tagStyle.stretchWidth = false;
				tagStyle.stretchHeight = false;
				tagStyle.margin = new RectOffset(0, 0, 2, 2);

				var backStyle = new GUIStyle(EditorStyles.toolbar);
				backStyle.stretchHeight = false;
				backStyle.fixedHeight = 0f;
				backStyle.padding = new RectOffset(4, 4, 4, 4);

				GUILayout.BeginHorizontal(backStyle);
				{

					var oldState = !this.tagsIgnored.Contains(tag.id);
					var state = GUILayout.Toggle(oldState, string.Empty);

					if (GUILayout.Button(tag.title, tagStyle) == true) {

						state = !oldState;

					}

					if (oldState != state) {
						
						if (state == false) {
							
							this.tagsIgnored.Add(tag.id);
							
						} else {
							
							this.tagsIgnored.Remove(tag.id);
							
						}
						
					}

					GUILayout.FlexibleSpace();

				}
				GUILayout.EndHorizontal();
				
			}

			GUILayout.EndScrollView();
			
			this.readyToNext = true;
			
			if (allTags.Count == 0) {

				var style = new GUIStyle(GUI.skin.label);
				style.wordWrap = true;
				GUILayout.Label("Current project has no tags. You can add tags (`Android`, `iOS`, etc.) to improve your flow.", style);
				
			} else {
				
				if (this.tagsIgnored.Count == allTags.Count) {
					
					this.readyToNext = false;
					
				}
				
				EditorGUILayout.HelpBox("All tags included by default.", MessageType.Info);

				this.DrawSaveToDefaultToggle();

			}

			this.DrawBottom();

		}

		private string compileNamespace;
		private bool saveDefaultSettings = false;
		private bool forceRecompile = false;
		private List<int> tagsIgnored = new List<int>();
		public void DrawPart1() {
			
			this.DrawHeader(this.partTitles[this.partIndex]);

			GUILayout.FlexibleSpace();
			
			#if UNITY_WEBPLAYER
			EditorGUILayout.HelpBox("Due to the Unity3D WebPlayer limitations File System using is not allowed. Use other platform.", MessageType.Error);
			#else
			GUILayout.Label("Namespace:");
			this.compileNamespace = GUILayout.TextField(this.compileNamespace);

			CustomGUI.Splitter();
			
			this.forceRecompile = GUILayout.Toggle(this.forceRecompile, "Force to recompile all");
			EditorGUILayout.HelpBox("By default all not compiled windows will be compiled. If you want to recompile all ScreenBase* windows - turn on this flag. Your Screen* code will not be changed in any case.", MessageType.Info);

			this.DrawSaveToDefaultToggle();
			#endif

			this.DrawBottom();

			#if UNITY_WEBPLAYER
			this.readyToNext = false;
			#else
			this.readyToNext = true;
			#endif

		}

		private void DrawSaveToDefaultToggle() {

			CustomGUI.Splitter();

			this.saveDefaultSettings = GUILayout.Toggle(this.saveDefaultSettings, "Save as default compiler module settings");

		}

		private void DrawHeader(string text) {

			var style = new GUIStyle(EditorStyles.whiteLargeLabel);
			style.fontSize = 14;

			GUILayout.Label(text, style);

		}

		private void DrawBottom() {
			
			GUILayout.FlexibleSpace();

			CustomGUI.Splitter();

			GUILayout.BeginHorizontal();
			{
				var firstPart = (this.partIndex == 0);
				var lastPart = (this.partIndex == this.parts - 1);
				
				var oldEnabled = GUI.enabled;

				GUI.enabled = !lastPart;
				if (GUILayout.Button("Cancel", this.defaultSkin.button, GUILayout.Width(100f), GUILayout.Height(30f)) == true) {
					
					this.Close();
					
				}
				
				GUILayout.FlexibleSpace();

				GUI.enabled = oldEnabled && !firstPart && !lastPart;
				if (GUILayout.Button("Back", this.defaultSkin.button, GUILayout.Width(100f), GUILayout.Height(30f)) == true) {
					
					--this.partIndex;
					
				}
				GUI.enabled = oldEnabled;

				GUI.enabled = GUI.enabled && this.readyToNext;
				if (this.partIndex == this.processPartIndex) {

					if (GUILayout.Button("GO!", this.defaultSkin.button, GUILayout.Width(100f), GUILayout.Height(30f)) == true) {
						
						++this.partIndex;
						this.Repaint();

						if (this.saveDefaultSettings == true) {
							
							FlowSystem.GetData().namespaceName = this.compileNamespace;
							FlowSystem.GetData().forceRecompile = this.forceRecompile;
							FlowSystem.SetDirty();
							
						}
						
						FlowCompilerSystem.currentNamespace = this.compileNamespace;
						
						if (this.tagsIgnored.Count == 0) {
							
							// Build all
							
							FlowCompilerSystem.Generate(AssetDatabase.GetAssetPath(FlowSystem.GetData()), this.forceRecompile);
							
						} else {

							var tags = new List<int>();
							foreach (var tag in FlowSystem.GetData().tags) {

								if (this.tagsIgnored.Contains(tag.id) == false) tags.Add(tag.id);

							}

							FlowCompilerSystem.GenerateByTags(AssetDatabase.GetAssetPath(FlowSystem.GetData()), tags.ToArray(), this.forceRecompile);
							
						}

						this.Repaint();

					}

				} else {

					if (lastPart == true) {
						
						if (GUILayout.Button("Finish", this.defaultSkin.button, GUILayout.Width(100f), GUILayout.Height(30f)) == true) {

							this.Close();

						}

					} else {

						if (GUILayout.Button("Next", this.defaultSkin.button, GUILayout.Width(100f), GUILayout.Height(30f)) == true) {
							
							++this.partIndex;
							
						}

					}

				}
				GUI.enabled = oldEnabled;

			}
			GUILayout.EndHorizontal();

		}

	}

}