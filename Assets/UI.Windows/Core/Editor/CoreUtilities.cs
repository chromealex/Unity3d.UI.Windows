using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System;
using System.Linq;

namespace UnityEditor.UI.Windows {

	public interface IWindowAddon {

		void Show(System.Action onClose);

	}

	public class CoreUtilities {

		private static Dictionary<string, bool> cacheAvailable = new Dictionary<string, bool>();
		private static Dictionary<string, IWindowAddon> cache = new Dictionary<string, IWindowAddon>();

		private static bool addonsLoaded = false;
		public static void LoadAddons() {

			if (CoreUtilities.addonsLoaded == true) return;

			cache.Clear();
			cacheAvailable.Clear();

			var list = Resources.LoadAll("UI.Windows/AddonInfo");
			foreach (var item in list) {

				var addonName = item.ToString().Trim();
				if (string.IsNullOrEmpty(addonName) == false) {

					CoreUtilities.IsAddonAvailable(addonName);

				}

			}
			
			CoreUtilities.addonsLoaded = true;

		}

		public static List<T> GetAddons<T>(System.Action<string, T> forEach = null) where T : IWindowAddon {

			var list = cache.Where((a) => (a.Value is T));
			var output = new List<T>();

			foreach (var addon in list) {

				if (forEach != null) forEach(addon.Key, (T)addon.Value);
				output.Add((T)addon.Value);

			}

			return output;

		}

		public static IWindowAddon GetAddon(string addonName) {

			if (cacheAvailable.ContainsKey(addonName) == false || cacheAvailable[addonName] == false) {

				return null;

			}

			if (cache.ContainsKey(addonName) == true) {

				return cache[addonName];

			}

			var type = CoreUtilities.GetTypeFromAllAssemblies(addonName, string.Empty);
			if (type != null) {

				var addon = System.Activator.CreateInstance(type) as IWindowAddon;
				cache.Add(addonName, addon);

				return addon;

			}

			return null;

		}

		public static bool IsAddonAvailable(string addonName) {

			if (cacheAvailable.ContainsKey(addonName) == true) return cacheAvailable[addonName];

			var type = CoreUtilities.GetTypeFromAllAssemblies(addonName, string.Empty);
			var isActive = type != null;

			cacheAvailable.Add(addonName, isActive);
			CoreUtilities.GetAddon(addonName);

			return isActive;

		}

		public static Type GetTypeFromAllAssemblies(string typeName, string namespaceName) {

			Assembly[] assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
			foreach (Assembly assembly in assemblies) {

				Type[] types = assembly.GetTypes();
				foreach (Type type in types) {

					if (string.IsNullOrEmpty(namespaceName) == false && type.Namespace != namespaceName) continue;

					if (type.Name.Equals(typeName, StringComparison.InvariantCultureIgnoreCase) || type.Name.Contains('+' + typeName)) {//+ check for inline classes

						return type;

					}

				}

			}

			return null;

		}

	}

}