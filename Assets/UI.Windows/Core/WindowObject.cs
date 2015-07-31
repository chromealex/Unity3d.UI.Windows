using UnityEngine;
using System.Collections;

namespace UnityEngine.UI.Windows {

	public interface IWindowObject {

		T GetWindow<T>() where T : WindowBase;
		WindowBase GetWindow();
		void HideCurrentWindow();

	};

	public class WindowObject : MonoBehaviour, IWindowObject {

		[HideInInspector]
		private WindowBase window;

		internal virtual void Setup(WindowBase window) {
			
			this.window = window;

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