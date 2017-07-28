using UnityEngine;
using System.Collections;
using UnityEngine.Extensions;
using UnityEngine.UI.Windows.Extensions;

namespace UnityEngine.UI.Windows {

	public interface IWindowObject {

		void Setup(WindowBase window);
		T GetWindow<T>() where T : WindowBase;
		WindowBase GetWindow();
		void HideCurrentWindow();

		int windowId { get; set; }

	};

	public interface IWindow : IWindowObject {
		
	};

	public abstract class WindowObject : MonoBehaviour, IWindowObject {

		[HideInInspector]
		private WindowBase window;
		
		//[HideInInspector]
		[ReadOnly][SerializeField]
		private int _windowId;
		public int windowId { get { return this._windowId; } set { this._windowId = value; } }

		//[Header("Navigation")]
		public NavigationGroup navigationGroup = new NavigationGroup();

		public virtual bool IsDestroyable() {
			
			return true;
			
		}

		public virtual void OnDeinit(System.Action callback) {
			
			this.window = null;
			if (this.navigationGroup != null) this.navigationGroup.Clear();
			
			if (this != null && this.IsDestroyable() == true) {
				
				//WindowObject.Destroy(this);
				//WindowObject.Destroy(this.gameObject);
				//Debug.LogWarning("RECYCLE: " + this.name);
				this.Recycle();

			}

			WindowSystemResources.UnregisterObject(this);
			WindowSystem.RemoveDebugWeakReference(this);

			callback.Invoke();

		}
		
		public virtual void Setup(WindowBase window) {

			if (this.window != window) {

				this.window = window;

				var flowWindow = UnityEngine.UI.Windows.Plugins.Flow.FlowSystem.GetWindow(this.window, runtime: true);
				this.windowId = (flowWindow != null ? flowWindow.id : -1);
				
				if ((this is WindowModule) == false || (this as WindowModule).IsInstantiate() == true) {

					WindowSystemResources.RegisterObject(this);
					WindowSystem.AddDebugWeakReference(this);

				}

			}

		}

		public WindowBase GetWindow() {

			#if UNITY_EDITOR
			if (this.window == null && Application.isPlaying == false) {

				this.window = this.GetComponentInParent<WindowBase>();

			}
			#endif

			return this.window;

		}

		public T GetWindow<T>() where T : WindowBase {

			return this.GetWindow() as T;

		}

		public void HideCurrentWindow() {
			
			this.GetWindow().Hide();
			
		}

		#if UNITY_EDITOR
		/// <summary>
		/// Raises the validate event. Editor Only.
		/// </summary>
		public virtual void OnValidate() {

			if (Application.isPlaying == true) return;
			if (UnityEditor.EditorApplication.isUpdating == true) return;

			this.OnValidateEditor();

		}

		/// <summary>
		/// Raises the validate editor event.
		/// You can override this method but call it's base.
		/// </summary>
		public virtual void OnValidateEditor() {

			if (this.navigationGroup == null) this.navigationGroup = new NavigationGroup();
			this.navigationGroup.OnValidate();

		}

		[ContextMenu("Create Canvas Linker")]
		public void CreateCanvasLinker_EDITOR() {

			this.CleanCanvasLinker_EDITOR();

			var linker = this.gameObject.AddComponent<CanvasLinker>();
			var canvas = this.gameObject.AddComponent<Canvas>();
			var raycaster = this.gameObject.AddComponent<GraphicRaycaster>();

			linker.canvas = canvas;
			linker.raycaster = raycaster;
			linker.windowObject = this;
			linker.orderDelta = 1;

		}

		[ContextMenu("Clean Canvas Linker")]
		public void CleanCanvasLinker_EDITOR() {

			Component.DestroyImmediate(this.GetComponent<CanvasLinker>(), true);
			Component.DestroyImmediate(this.GetComponent<GraphicRaycaster>(), true);
			Component.DestroyImmediate(this.GetComponent<Canvas>(), true);

		}
		#endif

	}

}