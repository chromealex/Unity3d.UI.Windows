using UnityEngine;
using System.Collections;
using System.Reflection;
using System;

namespace UnityEditor.UI.Windows {

	public interface IWindowAddon {

		void Show(System.Action onClose);

	}

	public class WindowUtilities {

		public static IWindowAddon GetAddon(string addonName) {

			var type = WindowUtilities.GetTypeFromAllAssemblies(addonName);
			if (type != null) {

				return System.Activator.CreateInstance(type) as IWindowAddon;

			}

			return null;

		}

		public static bool IsAddonAvailable(string addonName) {

			var type = WindowUtilities.GetTypeFromAllAssemblies(addonName);
			return type != null;

		}

		public static Type GetTypeFromAllAssemblies(string typeName) {

			Assembly[] assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
			foreach (Assembly assembly in assemblies) {

				Type[] types = assembly.GetTypes();
				foreach (Type type in types) {

					if (type.Name.Equals(typeName, StringComparison.InvariantCultureIgnoreCase) || type.Name.Contains('+' + typeName)) {//+ check for inline classes

						return type;

					}

				}

			}

			return null;

		}

	}

}