using UnityEngine;
using UnityEngine.UI.Windows.Plugins.Flow;
using UnityEditor.UI.Windows.Plugins.Flow;
using System.IO;
using UnityEngine.UI.Windows.Plugins.Resources;
using UnityEngine.UI.Windows;

namespace UnityEditor.UI.Windows.Plugins.Resources {
	
	public class Resources : FlowAddon {
		
		private static WindowSystemResourcesSettings settings;
		private Editor editor;

		public override string GetName() {

			return "Resources";

		}

		public override void OnFlowSettingsGUI() {

			if (Resources.settings == null) Resources.settings = Resources.GetSettingsFile();
			
			var settings = Resources.settings;
			if (settings == null) {
				
				EditorGUILayout.HelpBox(string.Format(FlowAddon.MODULE_HAS_ERRORS, "Settings file not found (WindowSystemResourcesSettings)."), MessageType.Error);
				
			} else {
				
				GUILayout.Label(FlowAddon.MODULE_INSTALLED, EditorStyles.centeredGreyMiniLabel);
				
				if (this.editor == null) {
					
					this.editor = Editor.CreateEditor(settings);
					
				}
				
				if (this.editor != null) {
					
					this.editor.OnInspectorGUI();
					
				}

			}
			
		}
		
		public override GenericMenu GetSettingsMenu(GenericMenu menu) {
			
			if (menu == null) menu = new GenericMenu();
			menu.AddItem(new GUIContent("Reinstall"), false, () => { this.Reinstall(); });
			
			return menu;
			
		}
		
		public static WindowSystemResourcesSettings GetSettingsFile() {
			
			var data = FlowSystem.GetData();
			if (data == null) return null;
			
			var modulesPath = data.GetModulesPath();
			
			var settings = ME.EditorUtilities.GetAssetsOfType<WindowSystemResourcesSettings>(modulesPath, useCache: true);
			if (settings != null && settings.Length > 0) {
				
				return settings[0];
				
			}
			
			return null;
			
		}
		
		public override bool InstallationNeeded() {
			
			return Resources.GetSettingsFile() == null;
			
		}
		
		public override void Install() {
			
			this.Install_INTERNAL();
			
		}
		
		public override void Reinstall() {
			
			this.Install_INTERNAL();
			
		}
		
		private bool Install_INTERNAL() {

			var moduleName = "WindowSystemResources";
			var settingsName = "WindowSystemResourcesSettings";
			return this.InstallModule<WindowSystemResourcesSettings>(moduleName, settingsName);

		}
		/*
		[MenuItem("Window/UI.Windows: Tools/Asset Bundles/Build All")]
		public static void BuildAssetBundlesAll()
		{
			foreach (var kvp in WindowSystemResources.ALL_TARGETS)
			{
				Resources.BuildAssetBundle(kvp.Key);
			}
		}

		[MenuItem("Window/UI.Windows: Tools/Asset Bundles/Build Current Target")]
		public static void BuildCurrentAssetBundle()
		{
			Resources.BuildAssetBundle(EditorUserBuildSettings.activeBuildTarget);
		}

		[MenuItem("Window/UI.Windows: Tools/Asset Bundles/Build Mobiles")]
		public static void BuildAssetBundlesMobiles()
		{
			Resources.BuildAssetBundle(BuildTarget.Android);
			Resources.BuildAssetBundle(BuildTarget.iOS);
		}

		[MenuItem("Window/UI.Windows: Tools/Asset Bundles/Build Consoles")]
		public static void BuildAssetBundlesConsoles()
		{
			Resources.BuildAssetBundle(BuildTarget.PS4);
			Resources.BuildAssetBundle(BuildTarget.XboxOne);
			Resources.BuildAssetBundle(BuildTarget.tvOS);
		}

		[MenuItem("Window/UI.Windows: Tools/Asset Bundles/Build Standalone")]
		public static void BuildAssetBundlesStanalone()
		{
			Resources.BuildAssetBundle(BuildTarget.StandaloneWindows);
			Resources.BuildAssetBundle(BuildTarget.StandaloneOSXUniversal);
			Resources.BuildAssetBundle(BuildTarget.StandaloneLinuxUniversal);
		}

		private static void BuildAssetBundle(BuildTarget target)
		{

			var fullPath = Resources.CheckDirectory(target);
			var assetPaths = AssetDatabase.GetAllAssetPaths();
			var assetBundlesMap = new ME.SimpleDictionary<string, System.Collections.Generic.List<string>>();

			try
			{

				for (int i = 0, size = assetPaths.Length; i < size; ++i)
				{
					UnityEditor.EditorUtility.DisplayProgressBar("Building map", assetPaths[i], i / (float)assetPaths.Length);

					var importer = AssetImporter.GetAtPath(assetPaths[i]);
					if (importer != null && string.IsNullOrEmpty(importer.assetBundleName) == false)
					{
						if (assetBundlesMap.Contains(importer.assetBundleName))
						{
							assetBundlesMap[importer.assetBundleName].Add(assetPaths[i]);
						}
						else
						{
							var toAdd = new System.Collections.Generic.List<string>() { assetPaths[i] };
							assetBundlesMap.Add(importer.assetBundleName, toAdd);
						}
					}
				}

			}
			catch (System.Exception ex)
			{
				Debug.LogError(ex.Message);
			}
			finally
			{

				UnityEditor.EditorUtility.ClearProgressBar();

			}
			WindowSystemAssetBundlesMap map = null;
			var objects = AssetDatabase.FindAssets("t:ScriptableObject");
			for (int i = 0, size = objects.Length; i < size; ++i)
			{
				var path = AssetDatabase.GUIDToAssetPath(objects[i]);
				if (path.Contains("WindowSystemAssetBundlesMap"))
				{
					map = AssetDatabase.LoadAssetAtPath(path, typeof(WindowSystemAssetBundlesMap)) as WindowSystemAssetBundlesMap;
				}
			}

			if (map == null)
			{
				Debug.LogError("Missing WindowSystemAssetBundlesMap!!!");
				return;
			}

			map.Reset();

			var abb = new AssetBundleBuild[assetBundlesMap.Count];
			int k = 0;
			foreach (var kvp in assetBundlesMap)
			{
				abb[k].assetBundleName = kvp.Key;
				abb[k].assetNames = kvp.Value.ToArray();
				map.AddItem(kvp.Key, abb[k].assetNames);
				++k;
			}

			BuildPipeline.BuildAssetBundles(fullPath, abb, BuildAssetBundleOptions.ForceRebuildAssetBundle, target);
		}

		private static string CheckDirectory(BuildTarget target) {
			//"~/Bundles/0.6.6b/iOS/{0}"
			var outputPath = WindowSystemResources.GetAssetBundlePath(target);
			if (!Directory.Exists(outputPath))
			{
				Directory.CreateDirectory(outputPath);
			}
			return outputPath;
		}*/

	}

}