using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows.Plugins.Flow;
using ME;
using System.Linq;
using System.Reflection;
using System.IO;
using System;
using System.Text.RegularExpressions;
using UnityEngine.UI.Windows;
using System.Collections.Generic;

namespace UnityEditor.UI.Windows.Plugins.Flow {

	public class FlowUpdater {

		public static void Run(FlowData data) {

			UnityEditor.EditorUtility.DisplayProgressBar("Upgrading", string.Format("Migrating from {0} to {1}", data.version, VersionInfo.BUNDLE_VERSION), 0f);
			var type = data.GetType();
			
			while (data.version < VersionInfo.BUNDLE_VERSION) {
				
				var nextVersion = data.version + 1;

				try {

					// Try to find upgrade method
					var methodName = "UpgradeTo" + nextVersion.ToSmallWithoutTypeString();
					var methodInfo = type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public);
					if (methodInfo != null) {
						
						var result = (bool)methodInfo.Invoke(data, null);
						if (result == true) {

							// Need to recompile
							var prevData = FlowSystem.GetData();
							FlowSystem.SetData(data);
							UnityEngine.UI.Windows.Plugins.FlowCompiler.CompilerSystem.currentNamespace = data.namespaceName;
							var path = UnityEditor.AssetDatabase.GetAssetPath(data);
							UnityEngine.UI.Windows.Plugins.FlowCompiler.CompilerSystem.Generate(path, recompile: true);
							FlowSystem.SetData(prevData);

						}

						Debug.Log("[UPGRADE] Invoked: `" + methodName + "`, version " + nextVersion);

					} else {
						
						Debug.Log("[UPGRADE] Method `" + methodName + "` was not found: version " + nextVersion + " skipped");
						
					}

					UnityEditor.EditorUtility.DisplayProgressBar("Upgrading", string.Format("Migrating from {0} to {1}", data.version, nextVersion), 0.5f);

				} catch (UnityException) {
				} finally {

					UnityEditor.EditorUtility.ClearProgressBar();

				}

				data.version = nextVersion;
				UnityEditor.EditorUtility.SetDirty(data);
				
			}
			
			UnityEditor.EditorUtility.DisplayProgressBar("Upgrading", string.Format("Migrating from {0} to {1}", data.version, VersionInfo.BUNDLE_VERSION), 1f);
			UnityEditor.EditorUtility.ClearProgressBar();

		}

	}

}