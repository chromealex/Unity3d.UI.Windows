using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows.Plugins.Flow;

namespace UnityEditor.UI.Windows.Plugins.Flow.Editors {
	
	public class FlowSceneViewWindow : EditorWindowSceneView {
		
		public FlowSystemEditorWindow rootWindow;
		private Rect popupRect;
		private Vector2 popupSize;
		private bool hided;

		private bool isLocked;

		public void ShowView() {
			
			this.isLocked = false;//!FlowSystem.GetData().editorWindowAsPopup;
			if (this.isLocked == true) {
				
				this.ShowPopup();
				
			} else {
				
				this.ShowUtility();
				
			}

			this.hided = false;
			
			this.minSize = new Vector2(this.minSize.x, 1f);

			this.UpdateSize(1f);

		}
		
		public void HideView() {
			
			this.hided = true;
			
		}

		public void UpdateSize(float progress) {

			var popupOffset = FlowSceneItem.POPUP_OFFSET;
			var inspectorWidth = FlowSceneView.GetInspector() ? FlowSceneView.GetInspector().GetWidth() : 0f;
			var hierarchyWidth = FlowSceneView.GetHierarchy() ? FlowSceneView.GetHierarchy().GetWidth() : 0f;
			this.popupSize = new Vector2(this.rootWindow.position.width - popupOffset * 2f - inspectorWidth - hierarchyWidth, (this.rootWindow.position.height - popupOffset * 2f) * progress);
			this.popupRect = new Rect(this.rootWindow.position.x + popupOffset + inspectorWidth, this.rootWindow.position.y + popupOffset, this.popupSize.x, this.popupSize.y);

			this.position = this.popupRect;

		}

		new public void Update() {
			
			base.Update();

			if (this.rootWindow == null) return;

			this.rootWindow.Update();

			if (this.isLocked == true) {

				if (this.hided == true) {
					
					this.minSize = Vector2.zero;
					this.popupRect = new Rect(0f, 0f, 1f, 1f);
					
				} else {

					this.UpdateSize(this.progress);

				}
				
				this.position = this.popupRect;

			}

			if (FlowSceneView.recompileChecker == null) this.Close();
			
		}

		public override void OnClose() {
			
			FlowSceneView.Reset(this.rootWindow.OnItemProgress, onDestroy: true);
			
		}
		
	}

}