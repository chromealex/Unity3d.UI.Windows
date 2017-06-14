using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows.Plugins.Flow;
using UnityEditor.UI.Windows.Plugins.Flow;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using ME;
using UnityEngine.UI.Windows.Plugins.FlowCompiler;
using FD = UnityEngine.UI.Windows.Plugins.Flow.Data;
using UnityEditor.UI.Windows.Extensions;
using UnityEngine.UI.Windows.Plugins.ABTesting;
using UnityEngine.UI.Windows.Extensions;

namespace UnityEditor.UI.Windows.Plugins.ABTesting {

	public abstract class TabContent {

		public const float MARGIN = 20f;

		public GUIContent title = new GUIContent();
		public ABTestingItem selectedItem;

		private Vector2 scrollPosition;

		public TabContent(string title) {

			this.title = new GUIContent(title);

		}

		public void OnGUI() {

			var oldSkin = GUI.skin;
			GUI.skin = FlowSystemEditorWindow.defaultSkin;
			this.scrollPosition = GUILayout.BeginScrollView(this.scrollPosition);
			GUI.skin = oldSkin;
			{
				
				GUILayout.Space(MARGIN);

				GUILayout.BeginHorizontal();
				{

					GUILayout.Space(MARGIN);

					GUILayout.BeginVertical();
					{

						this.OnContentGUI();

					}
					GUILayout.EndVertical();

					GUILayout.Space(MARGIN);

				}
				GUILayout.EndHorizontal();

				GUILayout.Space(MARGIN);

			}
			GUILayout.EndScrollView();

		}

		protected abstract void OnContentGUI();

	}
	
	public class GeneralTab : TabContent {
		
		public GeneralTab() : base("General") {}
		
		protected override void OnContentGUI() {

			if (this.selectedItem != null) this.selectedItem.OnGeneralGUI();

		}
		
	}
	
	public class UserTab : TabContent {
		
		public UserTab() : base("User Info") {}
		
		protected override void OnContentGUI() {
			
			var userInfo = UnityEngine.UI.Windows.UserInfo.User.instance;
			if (this.selectedItem != null) this.selectedItem.OnUserInfoGUI(userInfo);
			
		}
		
	}
	
	public class PlayerPrefsTab : TabContent {
		
		public PlayerPrefsTab() : base("Player Prefs") {}
		
		protected override void OnContentGUI() {

			if (this.selectedItem != null) this.selectedItem.OnPlayerPrefsGUI();
			
		}
		
	}

	public class TabsContent {

		public List<TabContent> tabs = new List<TabContent>();
		public ABTestingItem selectedItem;
		private int activeTabIndex = 0;

		public void Validate() {
			
			if (this.tabs == null || this.tabs.Count == 0) {
				
				this.tabs.Add(new GeneralTab());
				this.tabs.Add(new UserTab());
				this.tabs.Add(new PlayerPrefsTab());
				
				this.activeTabIndex = 0;
				
			}

			for (int i = 0; i < this.tabs.Count; ++i) {

				this.tabs[i].selectedItem = this.selectedItem;

			}

		}

		public void OnGUI() {

			this.Validate();

			GUILayout.BeginHorizontal();
			{

				for (int i = 0; i < this.tabs.Count; ++i) {

					var style = ABTesting.styles.tabButtonMid;
					if (i == 0) style = ABTesting.styles.tabButtonLeft;
					if (i == this.tabs.Count - 1) style = ABTesting.styles.tabButtonRight;

					EditorGUI.BeginDisabledGroup(this.activeTabIndex == i);
					if (GUILayout.Button(this.tabs[i].title, style) == true) {

						this.activeTabIndex = i;
						GUI.FocusControl(string.Empty);

					}
					EditorGUI.EndDisabledGroup();

				}

			}
			GUILayout.EndHorizontal();

			this.tabs[this.activeTabIndex].OnGUI();

		}

	}

}