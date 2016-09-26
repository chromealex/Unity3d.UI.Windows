using UnityEngine;
using System.Collections;

namespace UnityEngine.UI.Windows {

	public interface IWindowObject {

		T GetWindow<T>() where T : WindowBase;
		WindowBase GetWindow();
		void HideCurrentWindow();

	};

	public interface IWindow : IWindowObject {
	};

	public class WindowObject : MonoBehaviour, IWindowObject {

		[HideInInspector]
		private WindowBase window;
		
		//[HideInInspector]
		[ReadOnly]
		public int windowId;

		[Header("Navigation")]
		public NavigationGroup navigationGroup;

		internal virtual void Setup(WindowBase window) {
			
			this.window = window;

			var flowWindow = UnityEngine.UI.Windows.Plugins.Flow.FlowSystem.GetWindow(this.window, runtime: true);
			this.windowId = (flowWindow != null ? flowWindow.id : -1);

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
		public void OnValidate() {

			if (Application.isPlaying == true) return;

			this.OnValidateEditor();

		}

		/// <summary>
		/// Raises the validate editor event.
		/// You can override this method but call it's base.
		/// </summary>
		public virtual void OnValidateEditor() {

			this.navigationGroup.OnValidate();

		}
		#endif

	}

}