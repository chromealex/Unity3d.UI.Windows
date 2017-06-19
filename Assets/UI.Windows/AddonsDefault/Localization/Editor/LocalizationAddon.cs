using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows.Plugins.Flow;
using UnityEditor.UI.Windows.Plugins.Flow;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using ME;
using FD = UnityEngine.UI.Windows.Plugins.Flow.Data;
using UnityEditor.UI.Windows.Extensions;
using UnityEngine.UI.Windows.Plugins.Localization;
using UnityEngine.UI.Windows.Extensions;

namespace UnityEditor.UI.Windows.Plugins.Localization {
	
	public class Localization : FlowAddon {
		
		public class Styles {
			
			public GUISkin skin;
			public GUIStyle backLock;
			public GUIStyle content;
			public GUIStyle contentScreen;
			public GUIStyle closeButton;
			public GUIStyle listButton;
			public GUIStyle listButtonSelected;
			public GUIStyle listTag;
			public GUIStyle objectField;
			public GUIStyle layoutBack;
			public GUIStyle dropShadow;
			
			public GUIStyle tabButtonLeft;
			public GUIStyle tabButtonMid;
			public GUIStyle tabButtonRight;
			
			public Styles() {
				
				this.skin = UnityEngine.Resources.Load("UI.Windows/Flow/Styles/Skin" + (EditorGUIUtility.isProSkin == true ? "Dark" : "Light")) as GUISkin;
				if (this.skin != null) {
					
					this.backLock = this.skin.FindStyle("LayoutBackLock");
					this.content = this.skin.FindStyle("LayoutContent");
					this.contentScreen = this.skin.FindStyle("LayoutContentScreen");
					this.closeButton = new GUIStyle("TL SelectionBarCloseButton");
					this.listButton = this.skin.FindStyle("ListButton");
					this.listButtonSelected = this.skin.FindStyle("ListButtonSelected");
					
					this.listTag = new GUIStyle(this.skin.FindStyle("ListButton"));
					this.listTag.alignment = TextAnchor.MiddleRight;
					this.objectField = this.skin.FindStyle("ObjectField");
					this.layoutBack = this.skin.FindStyle("LayoutBack");
					this.dropShadow = this.skin.FindStyle("DropShadowOuter");
					
					this.tabButtonLeft = new GUIStyle("ButtonLeft");
					this.tabButtonLeft.margin = new RectOffset();
					this.tabButtonMid = new GUIStyle("ButtonMid");
					this.tabButtonMid.margin = new RectOffset();
					this.tabButtonRight = new GUIStyle("ButtonRight");
					this.tabButtonRight.margin = new RectOffset();

				}
				
			}
			
		}

		private static LocalizationSettings settings;
		private Editor editor;

		public override string GetName() {

			return "Localization";

		}

		public override void OnFlowSettingsGUI() {

			if (Localization.settings == null) Localization.settings = Localization.GetSettingsFile();
			
			var settings = Localization.settings;
			if (settings == null) {
				
				EditorGUILayout.HelpBox(string.Format(FlowAddon.MODULE_HAS_ERRORS, "Settings file not found (LocalizationSettings)."), MessageType.Error);
				
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
		
		public static LocalizationSettings GetSettingsFile() {
			
			var data = FlowSystem.GetData();
			if (data == null) return null;
			
			var modulesPath = data.GetModulesPath();
			
			var settings = ME.EditorUtilities.GetAssetsOfType<LocalizationSettings>(modulesPath, useCache: true);
			if (settings != null && settings.Length > 0) {
				
				return settings[0];
				
			}
			
			return null;
			
		}
		
		public override bool InstallationNeeded() {
			
			return Localization.GetSettingsFile() == null;
			
		}
		
		public override void Install() {
			
			this.Install_INTERNAL();
			
		}
		
		public override void Reinstall() {
			
			this.Install_INTERNAL();
			
		}
		
		private bool Install_INTERNAL() {

			var moduleName = "Localization";
			var settingsName = "LocalizationSettings";
			return this.InstallModule<LocalizationSettings>(moduleName, settingsName);

		}

		[MenuItem("Window/UI.Windows: Tools/Update Localizations", isValidateFunction: true)]
		public static bool UpdateLocalizationsValidator() {

			var initializer = GameObject.FindObjectOfType<UnityEngine.UI.Windows.WindowSystemFlow>();
			if (initializer != null) {

				var data = initializer.GetBaseFlow();
				if (data != null) {

					var modulesPath = data.GetModulesPath();
					var settings = ME.EditorUtilities.GetAssetsOfType<LocalizationSettings>(modulesPath, useCache: true);

					if (settings.Length > 0) {

						return true;

					}

				}

			}

			return false;

		}

		[MenuItem("Window/UI.Windows: Tools/Update Localizations")]
		public static void UpdateLocalizations() {

			var initializer = GameObject.FindObjectOfType<UnityEngine.UI.Windows.WindowSystemFlow>();
			if (initializer != null) {

				var data = initializer.GetBaseFlow();

				var modulesPath = data.GetModulesPath();
				var settings = ME.EditorUtilities.GetAssetsOfType<LocalizationSettings>(modulesPath, useCache: true);

				if (settings.Length > 0) {

					var settingsFile = settings.First();
					var services = GameObject.FindObjectsOfType<LocalizationService>();
					for (int i = 0; i < services.Length; ++i) {
						
						var service = services[i];
						service.EditorLoad(settingsFile, settingsFile.items[i]);

					}

				}

			}

		}

	}

}