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

		internal virtual void Setup(WindowBase window) {
			
			this.window = window;

			var flowWindow = UnityEngine.UI.Windows.Plugins.Flow.FlowSystem.GetWindow(this.window);
			this.windowId = (flowWindow != null ? flowWindow.id : -1);

		}

		public WindowBase GetWindow() {

			return this.window;

		}

		public T GetWindow<T>() where T : WindowBase {

			return this.window as T;

		}

		public void HideCurrentWindow() {
			
			this.GetWindow().Hide();
			
		}

	}

}