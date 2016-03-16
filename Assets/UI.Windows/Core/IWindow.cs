using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace UnityEngine.UI.Windows {

	public struct AppearanceParameters {
		
		public bool replaceCallback;
		public System.Action callback;
		
		public bool replaceImmediately;
		public bool immediately;
		
		public bool replaceResetAnimation;
		public bool resetAnimation;

		public bool replaceIncludeChilds;
		public bool includeChilds;

		public bool replaceChildsBehaviourMode; 
		public ChildsBehaviourMode childsBehaviourMode;

		public AppearanceParameters(AppearanceParameters other) {

			this.replaceCallback = other.replaceCallback;
			this.callback = other.callback;

			this.replaceImmediately = other.replaceImmediately;
			this.immediately = other.immediately;

			this.replaceResetAnimation = other.replaceResetAnimation;
			this.resetAnimation = other.resetAnimation;

			this.replaceIncludeChilds = other.replaceIncludeChilds;
			this.includeChilds = other.includeChilds;

			this.replaceChildsBehaviourMode = other.replaceChildsBehaviourMode;
			this.childsBehaviourMode = other.childsBehaviourMode;

		}

		public static AppearanceParameters Default() {

			return new AppearanceParameters() {

				replaceCallback = false,
				callback = null,
				
				replaceImmediately = false,
				immediately = false,
				
				replaceResetAnimation = false,
				resetAnimation = true,
				
				replaceIncludeChilds = false,
				includeChilds = true,
				
				replaceChildsBehaviourMode = false,
				childsBehaviourMode = ChildsBehaviourMode.Simultaneously,
				
			};

		}

		public System.Action GetCallback(System.Action defaultValue) {
			
			if (this.replaceCallback == true) return this.callback;
			
			return defaultValue;
			
		}

		public bool GetImmediately(bool defaultValue) {
			
			if (this.replaceImmediately == true) return this.immediately;
			
			return defaultValue;
			
		}
		
		public bool GetResetAnimation(bool defaultValue) {
			
			if (this.replaceResetAnimation == true) return this.resetAnimation;
			
			return defaultValue;
			
		}
		
		public bool GetIncludeChilds(bool defaultValue) {
			
			if (this.replaceIncludeChilds == true) return this.includeChilds;
			
			return defaultValue;
			
		}

		public ChildsBehaviourMode GetChildsBehaviourMode(ChildsBehaviourMode defaultValue) {
			
			if (this.replaceChildsBehaviourMode == true) return this.childsBehaviourMode;
			
			return defaultValue;
			
		}
		
		public AppearanceParameters ReplaceCallback(System.Action callback) {

			return new AppearanceParameters(this) { replaceCallback = true, callback = callback };
			
		}

		public AppearanceParameters ReplaceImmediately(bool immediately) {

			return new AppearanceParameters(this) { replaceImmediately = true, immediately = immediately };

		}
		
		public AppearanceParameters ReplaceResetAnimation(bool resetAnimation) {
			
			return new AppearanceParameters(this) { replaceResetAnimation = true, resetAnimation = resetAnimation };

		}
		
		public AppearanceParameters ReplaceIncludeChilds(bool includeChilds) {
			
			return new AppearanceParameters(this) { replaceIncludeChilds = true, includeChilds = includeChilds };

		}
		
		public AppearanceParameters ReplaceChildsBehaviourMode(ChildsBehaviourMode childsBehaviourMode) {
			
			return new AppearanceParameters(this) { replaceChildsBehaviourMode = true, childsBehaviourMode = childsBehaviourMode };

		}

		public void Call() {

			if (this.callback != null) {

				this.callback.Invoke();

			}

		}

	};
	
	public enum ChildsBehaviourMode : byte {
		
		Simultaneously = 0,		// Call Show/Hide methods on all childs
		Consequentially = 1,	// Call Show/Hide after all childs complete hide
		
	};

	public class CompilerIgnore : System.Attribute {
	};
	
	/// <summary>
	/// Window or component object state.
	/// </summary>
	public enum WindowObjectState : byte {
		
		NotInitialized = 0,
		Deinitializing,
		Initializing,
		Initialized,
		
		Showing,
		Shown,
		
		Hiding,
		Hidden,
		
	};
	
	public enum ActiveState : byte {
		
		None = 0,
		Active,
		Inactive,
		
	};
	
	public enum DragState : byte {
		
		None = 0,
		Begin,
		Move,
		End,
		
	};
	
	public interface IManualEvent {}

	public interface IPreviewEditor {
		
		bool HasPreviewGUI();
		void OnEnable();
		void OnDisable();
		void OnPreviewGUI(Rect rect, GUIStyle style);
		void OnPreviewGUI(Color color, Rect rect, GUIStyle style);
		void OnPreviewGUI(Color color, Rect rect, GUIStyle style, bool drawInfo, bool selectable);
		void OnPreviewGUI(Color color, Rect rect, GUIStyle style, bool drawInfo, bool selectable, bool hovered);
		void OnPreviewGUI(Color color, Rect rect, GUIStyle style, bool drawInfo, bool selectable, WindowLayoutElement selectedElement);
		void OnPreviewGUI(Color color, Rect rect, GUIStyle style, WindowLayoutElement selected, System.Action<WindowLayoutElement> onSelection, List<WindowLayoutElement> highlighted);
		
	}

	public enum WindowEventType : byte {

		OnInit,
		OnDeinit,
		OnShowBegin,
		OnShowEnd,
		OnHideBegin,
		OnHideEnd,
		OnWindowOpen,

	};
	/*
	public interface IWindowEvents {
		
		void OnInit();
		void OnDeinit();
		void OnShowBegin();
		void OnShowEnd();
		void OnHideBegin();
		void OnHideEnd();
		
	}*/
	
	public interface IWindowEventsAsync {
		
		void OnInit();
		void OnDeinit();
		void OnShowBegin(System.Action callback, bool resetAnimation = true);
		void OnShowEnd();
		void OnHideBegin(System.Action callback, bool immediately = false);
		void OnHideEnd();
		
	}
	
	public interface IWindowEventsController {
		
		void DoInit();
		void DoDeinit();
		void DoShowBegin(AppearanceParameters appearanceParameters);
		void DoShowEnd();
		void DoHideBegin(AppearanceParameters appearanceParameters);
		void DoHideEnd();
		
	}

}