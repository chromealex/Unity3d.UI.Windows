using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI.Windows.Plugins.Flow;

namespace UnityEditor.UI.Windows.Plugins.Flow.Editors {
	
	public class FlowInspectorWindow : EditorWindowExt {
		
		public FlowSystemEditorWindow rootWindow;
		private Rect popupRect;
		private Vector2 popupSize;
		private bool hided;
		
		public const float MIN_WIDTH = 275f;
		public const float MAX_WIDTH = 350f;
		
		public float GetWidth() {
			
			return this.popupSize.x;
			
		}
		
		new public void Focus() {
			
			base.Focus();
			
		}
		
		private GameObject lastSelected;
		private GameObject currentSelected;

		private bool isLocked = false;
		private bool pinned = true;

		public void OnSelected(GameObject go) {
			
			this.editors.Clear();
			this.editorsFoldOut.Clear();
			
			if (go != null) {
				
				this.editors.Add(Editor.CreateEditor(go.transform));
				this.editorsFoldOut.Add(true);
				
				var comps = go.GetComponents<Behaviour>();
				foreach (var comp in comps) {
					
					this.editors.Add(Editor.CreateEditor(comp));
					this.editorsFoldOut.Add(true);
					
				}
				
			}
			
			this.Repaint();
			
		}
		
		public void ShowView() {
			
			this.isLocked = false;//!FlowSystem.GetData().editorWindowAsPopup;
			if (this.isLocked == true) {
				
				this.ShowPopup();
				
			} else {
				
				this.ShowUtility();
				
			}

			this.hided = false;
			
			this.minSize = new Vector2(this.minSize.x, 1f);
			this.autoRepaintOnSceneChange = true;

			this.UpdateSize(1f);

		}
		
		public void HideView() {
			
			this.hided = true;
			
		}

		public void UpdateSize(float progress) {

			var popupOffset = FlowSceneItem.POPUP_OFFSET;
			var margin = FlowSceneItem.POPUP_MARGIN;
			this.popupSize = new Vector2((this.rootWindow.position.width - popupOffset * 2f) * 0.3f, (this.rootWindow.position.height - popupOffset * 2f) * progress);
			this.popupSize.x = Mathf.Clamp(this.popupSize.x, MIN_WIDTH + margin, MAX_WIDTH);
			this.popupRect = new Rect(this.rootWindow.position.x + popupOffset, this.rootWindow.position.y + popupOffset, this.popupSize.x - margin, this.popupSize.y);
			
			this.position = this.popupRect;

		}

		new public void Update() {
			
			base.Update();
			
			if (this.rootWindow == null) return;

			this.currentSelected = Selection.activeGameObject;
			if (this.currentSelected != this.lastSelected) {
				
				this.OnSelected(this.currentSelected);
				this.lastSelected = this.currentSelected;
				
			}

			if (this.isLocked == true) {

				if (this.hided == true) {
					
					this.minSize = Vector2.zero;
					this.popupRect = new Rect(0f, 0f, 1f, 1f);

				} else {

					this.UpdateSize(this.progress);

				}
				
				this.position = this.popupRect;

			} else {
				
				if (this.pinned == true) {
					
					this.UpdateSize(this.progress);
					
				}
				
				var pinnedDistance = 10f;
				
				// snap to SceneView
				if (Vector2.Distance(new Vector2(this.rootWindow.position.x, this.rootWindow.position.y), new Vector2(this.position.x + this.position.width, this.position.y)) < pinnedDistance) {
					
					this.pinned = true;
					
				} else {
					
					this.pinned = false;
					
				}
				
			}

			if (FlowSceneView.recompileChecker == null) {
				
				this.Close();
				
			}
			
		}
		
		private Vector2 scrollPosition;
		private List<Editor> editors = new List<Editor>();
		private List<bool> editorsFoldOut = new List<bool>();
		public void OnGUI() {

			this.Update();

			var color = GUI.color;
			color.a = this.progress;
			GUI.color = color;

			if (this.editors != null && this.editors.Count > 0) {
				
				this.scrollPosition = EditorGUILayout.BeginScrollView(this.scrollPosition, false, false);
				
				for (int i = 0; i < this.editors.Count; ++i) {
					
					var editor = this.editors[i];
					this.editorsFoldOut[i] = EditorGUILayout.InspectorTitlebar(this.editorsFoldOut[i], editor.target);
					if (this.editorsFoldOut[i] == true && editor != null && editor.target != null) editor.OnInspectorGUI();
					
				}
				
				EditorGUILayout.EndScrollView();
				
			} else {

				var style = new GUIStyle(EditorStyles.whiteLargeLabel);
				style.alignment = TextAnchor.MiddleCenter;
				style.normal.textColor = Color.gray;

				var rect = this.position;
				rect.x = 0f;
				rect.y = 0f;
				GUI.Label(rect, "No Object Selected", style);

			}
			
		}
		
		public override void OnClose() {
			
			FlowSceneView.Reset(this.rootWindow.OnItemProgress, onDestroy: true);
			
		}
		
	}

}