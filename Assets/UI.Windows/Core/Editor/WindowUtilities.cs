using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System;
using UnityEngine.UI.Windows.Plugins.Flow;
using System.Linq;

namespace UnityEditor.UI.Windows {

	public interface IWindowAddon {

		void Show(System.Action onClose);

	}

	public class WindowUtilities {

		private static Dictionary<string, bool> cacheAvailable = new Dictionary<string, bool>();
		private static Dictionary<string, IWindowAddon> cache = new Dictionary<string, IWindowAddon>();

		private static bool addonsLoaded = false;
		public static void LoadAddons() {

			if (WindowUtilities.addonsLoaded == true) return;

			cache.Clear();
			cacheAvailable.Clear();

			var list = Resources.LoadAll("UI.Windows/AddonInfo");
			foreach (var item in list) {

				var addonName = item.ToString().Trim();
				if (string.IsNullOrEmpty(addonName) == false) {

					WindowUtilities.IsAddonAvailable(addonName);

				}

			}
			
			WindowUtilities.addonsLoaded = true;

		}

		public static List<T> GetAddons<T>() where T : IWindowAddon {

			var list = cache.Where((a) => (a.Value is T));
			var output = new List<T>();

			foreach (var addon in list) {

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

			var type = WindowUtilities.GetTypeFromAllAssemblies(addonName);
			if (type != null) {

				var addon = System.Activator.CreateInstance(type) as IWindowAddon;
				cache.Add(addonName, addon);

				return addon;

			}

			return null;

		}

		public static bool IsAddonAvailable(string addonName) {

			if (cacheAvailable.ContainsKey(addonName) == true) return cacheAvailable[addonName];

			var type = WindowUtilities.GetTypeFromAllAssemblies(addonName);
			var isActive = type != null;

			cacheAvailable.Add(addonName, isActive);
			WindowUtilities.GetAddon(addonName);

			return isActive;

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