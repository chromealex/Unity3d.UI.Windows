using UnityEngine;
using System.Collections;
using UnityEngine.Events;

namespace UnityEngine.UI.Windows {

	public static class WindowBaseExtensions {

		public static T ShowNext<T>(this WindowBase window, System.Action<T> onParametersPassCall = null) where T : WindowBase {

			var newPrefs = new Preferences(WindowSystem.GetSource<T>().preferences);
			newPrefs.showInSequence = true;

			var parameters = new WindowSystemShowParameters<T>() {
				onParametersPassCall = onParametersPassCall,
				overridePreferences = true,
				prefereces = newPrefs,
			};

			var instance = WindowSystem.ShowWithParameters<T>(parameters);

			UnityAction action = null;
			action = () => {

				instance.Show();

				window.events.onEveryInstance.Unregister(WindowEventType.OnHideEnd, action);

			};
			window.events.onEveryInstance.Register(WindowEventType.OnHideEnd, action);

			return instance;

		}

	}

}