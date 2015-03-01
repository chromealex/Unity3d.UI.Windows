using UnityEngine;
using System.Collections;

namespace UnityEditor.UI.Windows.Plugins.Flow.Editors {
	
	public class FlowGameViewWindow : EditorWindowExt {
		
		public FlowSystemEditorWindow rootWindow;
		private Rect popupRect;
		private Vector2 popupSize;
		private bool hided;

		private EditorWindow gameView;

		public void ShowView() {
			
			this.ShowPopup();
			this.hided = false;
			
			this.minSize = new Vector2(this.minSize.x, 1f);
			this.progress = 1f;

			this.gameView = this.GetGameView();
			this.gameView.ShowPopup();
			this.gameView.Focus();
			
		}

		private EditorWindow GetGameView() {

			var type = System.Type.GetType("UnityEditor.GameView,UnityEditor");
			return EditorWindow.CreateInstance(type) as EditorWindow;

		}
		
		public void HideView() {
			
			this.hided = true;
			
		}

		public void OnFocus() {

			if (this.gameView != null) this.gameView.Focus();

		}

		public void OnLostFocus() {

			if (this.gameView != null) this.gameView.Close();
			this.Close();

		}
		
		new public void Update() {
			
			base.Update();

			if (this.rootWindow == null) return;

			this.rootWindow.Update();
			
			if (this.hided == true) {
				
				this.minSize = Vector2.zero;
				this.popupRect = new Rect(0f, 0f, 1f, 1f);
				
			} else {
				
				var progress = this.progress;
				
				var popupOffset = FlowSceneItem.POPUP_OFFSET + 50f;
				this.popupSize = new Vector2(this.rootWindow.position.width - popupOffset * 2f, (this.rootWindow.position.height - popupOffset * 2f) * progress);
				this.popupRect = new Rect(this.rootWindow.position.x + popupOffset, this.rootWindow.position.y + popupOffset, this.popupSize.x, this.popupSize.y);
				
			}
			
			this.position = this.popupRect;
			if (this.gameView != null) this.gameView.position = this.popupRect;
			
			if (FlowSceneView.recompileChecker == null) this.Close();
			
		}
		
		public override void OnClose() {

			this.OnLostFocus();

		}
		
	}
	
}