using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace UnityEngine.UI.Windows {

	public enum Orientation : byte {

		Horizontal,
		Vertical,

	};

	public struct InitializeParameters {

		public float depth;
		public float zDepth;
		public int raycastPriority;
		public int orderInLayer;

	}

	public struct AppearanceParameters {
		
		public bool replaceCallback;
		public System.Action callback;
		
		public bool replaceDelay;
		public float delay;

		public bool replaceImmediately;
		public bool immediately;
		
		public bool replaceResetAnimation;
		public bool resetAnimation;

		public bool replaceIncludeChilds;
		public bool includeChilds;

		public bool replaceChildsBehaviourMode; 
		public ChildsBehaviourMode childsBehaviourMode;

		public bool replaceForced;
		public bool forced;

		public bool replaceManual;
		public bool manual;

		public AppearanceParameters(AppearanceParameters other) {

			this.replaceCallback = other.replaceCallback;
			this.callback = other.callback;
			
			this.replaceDelay = other.replaceDelay;
			this.delay = other.delay;
			
			this.replaceImmediately = other.replaceImmediately;
			this.immediately = other.immediately;

			this.replaceResetAnimation = other.replaceResetAnimation;
			this.resetAnimation = other.resetAnimation;

			this.replaceIncludeChilds = other.replaceIncludeChilds;
			this.includeChilds = other.includeChilds;

			this.replaceChildsBehaviourMode = other.replaceChildsBehaviourMode;
			this.childsBehaviourMode = other.childsBehaviourMode;
			
			this.replaceForced = other.replaceForced;
			this.forced = other.forced;

			this.replaceManual = other.replaceManual;
			this.manual = other.manual;

		}

		public static AppearanceParameters Default() {

			return new AppearanceParameters() {

				replaceCallback = false,
				callback = null,
				
				replaceDelay = false,
				delay = 0f,

				replaceImmediately = false,
				immediately = false,
				
				replaceResetAnimation = false,
				resetAnimation = true,
				
				replaceIncludeChilds = false,
				includeChilds = true,
				
				replaceChildsBehaviourMode = false,
				childsBehaviourMode = ChildsBehaviourMode.Simultaneously,

				replaceForced = false,
				forced = false,

				replaceManual = false,
				manual = false,

			};

		}

		public System.Action GetCallback(System.Action defaultValue) {
			
			if (this.replaceCallback == true) return this.callback;
			
			return defaultValue;
			
		}
		
		public bool GetForced(bool defaultValue) {
			
			if (this.replaceForced == true) return this.forced;
			
			return defaultValue;
			
		}
		
		public float GetDelay(float defaultValue) {
			
			if (this.replaceDelay == true) return this.delay;
			
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

		public bool GetManual(bool defaultValue) {

			if (this.replaceManual == true) return this.manual;

			return defaultValue;

		}

		public ChildsBehaviourMode GetChildsBehaviourMode(ChildsBehaviourMode defaultValue) {
			
			if (this.replaceChildsBehaviourMode == true) return this.childsBehaviourMode;
			
			return defaultValue;
			
		}
		
		public AppearanceParameters ReplaceCallback(System.Action callback) {

			return new AppearanceParameters(this) { replaceCallback = true, callback = callback };
			
		}
		
		public AppearanceParameters ReplaceDelay(float delay) {
			
			return new AppearanceParameters(this) { replaceDelay = true, delay = delay };
			
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

		public AppearanceParameters ReplaceForced(bool forced) {

			return new AppearanceParameters(this) { replaceForced = true, forced = forced };

		}

		public AppearanceParameters ReplaceManual(bool manual) {

			return new AppearanceParameters(this) { replaceManual = true, manual = manual };

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

	public interface IDraggableHandler {

		bool IsDraggable();

	}

	public interface IResourceValidationObject {

		Object GetValidationObject();

	}

	public interface IResourceReference {
	}

	public interface ILoadableReference {

		IResourceReference GetReference();

	}

	public interface ILoadableResource : IResourceReference {
		
		ResourceBase GetResource();
		
	}

	public interface ILoadableObject {

		void OnLoad(System.Action callback);
		void OnUnload();

		void Validate();

	}

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
		OnHideEndLate,
		OnWindowOpen,
		OnWindowClose,
		OnWindowActive,
		OnWindowInactive,
		OnWindowLayoutComplete,

	};

	public interface IWindowEventsAsync {

		void OnWindowUnload();
		void OnWindowActive();
		void OnWindowInactive();
		void OnWindowOpen();
		void OnWindowClose();

		void OnInit();
		void OnDeinit(System.Action callback);
		void OnShowBegin();
		void OnShowBegin(AppearanceParameters appearanceParameters);
		void OnShowEnd();
		void OnShowEnd(AppearanceParameters appearanceParameters);
		void OnHideBegin();
		void OnHideBegin(AppearanceParameters appearanceParameters);
		void OnHideEnd();
		void OnHideEnd(AppearanceParameters appearanceParameters);
		
	}
	
	public interface IWindowEventsController {

		void DoWindowUnload();
		void DoWindowActive();
		void DoWindowInactive();
		void DoWindowOpen();
		void DoWindowClose();
		void DoWindowLayoutComplete();

		void DoInit();
		void DoDeinit(System.Action callback);
		void DoShowBegin(AppearanceParameters appearanceParameters);
		void DoShowEnd(AppearanceParameters appearanceParameters);
		void DoHideBegin(AppearanceParameters appearanceParameters);
		void DoHideEnd(AppearanceParameters appearanceParameters);
		
	}

}