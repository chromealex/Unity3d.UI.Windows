
using CodeEditor.Text.UI.Unity.Editor.Implementation;

namespace UnityEditor {

	public class EditorWindowSceneView : SceneView {
		
		private bool lastVisibleState = false;
		private bool currentVisibleState = false;
		
		public virtual void OnActive() {}
		public virtual void OnInactive() {}
		public virtual void OnClose() {}
		
		public override void OnEnable() {
			
			base.OnEnable();
			this.lastVisibleState = true;
			this.UpdateState();
			
		}
		
		public override void OnDisable() {
			
			base.OnDisable();
			this.SetState(false);
			this.OnClose();
			
		}
		
		new public void OnDestroy() {
			
			base.OnDestroy();
			this.SetState(false);
			//this.OnClose();
			
		}

		#if UNITY_5
		public void Update() {

			this.currentVisibleState = this.IsVisible();
			this.UpdateState();
			
		}
		#else
		new public void Update() {
			
			base.Update();
			this.currentVisibleState = this.IsVisible();
			this.UpdateState();
			
		}
		#endif
		
		private void SetState(bool state) {
			
			this.currentVisibleState = state;
			this.UpdateState();
			
		}
		
		private void UpdateState() {
			
			if (this.lastVisibleState != this.currentVisibleState) {
				
				if (this.currentVisibleState == true) {
					
					this.OnActive();
					
				} else {
					
					this.OnInactive();
					
				}
				
				this.lastVisibleState = this.currentVisibleState;
				
			}
			
		}
		
		public bool IsVisible() {
			
			if (this == null) return false;
			
			return MissingEditorAPI.IsVisible(this);
			
		}

		protected float progress;
		public void DrawProgress(float progress) {

			this.progress = progress;
			this.Repaint();

		}
		
	}
	
	public class EditorWindowExt : EditorWindow {
		
		private bool lastVisibleState = false;
		private bool currentVisibleState = false;
		
		public virtual void OnActive() {}
		public virtual void OnInactive() {}
		public virtual void OnClose() {}
		
		public void OnEnable() {
			
			this.lastVisibleState = true;
			this.UpdateState();
			
		}
		
		public void OnDisable() {
			
			this.SetState(false);
			this.OnClose();
			
		}
		
		public void OnDestroy() {
			
			this.SetState(false);
			//this.OnClose();
			
		}
		
		public virtual void Update() {
			
			this.currentVisibleState = this.IsVisible();
			this.UpdateState();
			
		}
		
		private void SetState(bool state) {
			
			this.currentVisibleState = state;
			this.UpdateState();
			
		}
		
		private void UpdateState() {
			
			if (this.lastVisibleState != this.currentVisibleState) {
				
				if (this.currentVisibleState == true) {
					
					this.OnActive();
					
				} else {
					
					this.OnInactive();
					
				}
				
				this.lastVisibleState = this.currentVisibleState;
				
			}
			
		}
		
		public bool IsVisible() {
			
			if (this == null) return false;
			
			return MissingEditorAPI.IsVisible(this);
			
		}
		
		protected float progress;
		public void DrawProgress(float progress) {
			
			this.progress = progress;
			this.Repaint();
			
		}

	}
	
}
