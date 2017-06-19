using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine.UI.Windows.Plugins.Social.Core;
using UnityEngine.UI.Windows.Plugins.Social.Queries;

namespace UnityEngine.UI.Windows.Plugins.Social.Modules.Impl.VK {

	public class VKSettings : ModuleSettings {

		public override string GetPlatformName() {

			return "VK (vk.com)";

		}
		
		public override string GetPlatformClassName() {

			return "VK";
			
		}

		[Header("VK Settings")]
		public string profileMainPhoto;

		public enum Display : byte {
			Page,
			Popup,
			Touch,
			Wap
		}
		
		public Display display;

		public override HTTPParams Prepare(string token, HTTPParams parameters, Dictionary<string, string> values = null) {

			parameters = base.Prepare(token, parameters, values);

			foreach (var item in parameters.items) {
				
				if (item.key.ToLower() == "display") {
					
					item.value = this.display.ToString().ToLower();
					
				}
				
				if (item.key.ToLower() == "access_token") {
					
					item.value = token;
					
				}

			}
			
			return parameters;
			
		}

		[HideInInspector]
		public List<string> declaredPermissionsGroup1 = new List<string>() {
			"friends", 
			"status",
			"email",
		};
		
		[HideInInspector]
		public List<string> declaredPermissionsGroup2 = new List<string>() {
			"photos",
			"audio",
			"video",
			"docs",
			"notes",
			"pages",
			"wall",
			"groups",
			"messages"

		};
		
		[HideInInspector]
		public List<string> declaredPermissionsGroup3 = new List<string>() {
			"notify",
			"notifications",
			"stats",
			"ads"
		};
		
		[HideInInspector]
		public List<string> declaredPermissionsGroup4 = new List<string>() {
			"offline",
			"nohttps"
		};
		
		[HideInInspector]
		public List<string> permissions = new List<string>();
		
		public override string GetPermissions() {
			
			return string.Join(",", this.permissions.ToArray());
			
		}

		#if UNITY_EDITOR
		public override void OnInspectorGUI() {
			
			base.OnInspectorGUI();
			
			UnityEditor.EditorGUILayout.LabelField("Permissions", UnityEditor.EditorStyles.boldLabel);
			this.DrawPermissionGroup(this.declaredPermissionsGroup1);
			this.DrawPermissionGroup(this.declaredPermissionsGroup2);
			this.DrawPermissionGroup(this.declaredPermissionsGroup3);
			this.DrawPermissionGroup(this.declaredPermissionsGroup4);
			
			UnityEditor.EditorGUILayout.HelpBox("Be sure that you have added `https://oauth.vk.com/blank.html` url into your auth as `REDIRECT_URI`.", UnityEditor.MessageType.Info);
			
		}
		
		private void DrawPermissionGroup(List<string> permissions) {
			
			var columns = 2;
			var column = 2;
			var rows = 0;
			var opened = false;
			
			UnityEditor.EditorGUILayout.BeginVertical(UnityEditor.EditorStyles.helpBox);
			{
				
				foreach (var perm in permissions) {
					
					if (column == columns) {
						
						column = 0;
						++rows;
						
						if (rows > 1) {
							
							UnityEditor.EditorGUILayout.EndHorizontal();
							opened = false;
							
						}
						
						UnityEditor.EditorGUILayout.BeginHorizontal();
						opened = true;
						
					}
					
					{
						
						var oldValue = this.permissions.Contains(perm);
						var flag = UnityEditor.EditorGUILayout.ToggleLeft(perm, oldValue);
						if (flag != oldValue) {
							
							if (flag == true) {
								
								this.permissions.Add(perm);
								
							} else {
								
								this.permissions.Remove(perm);
								
							}
							
						}
						
					}
					
					++column;
					
				}
				
				if (opened == true) UnityEditor.EditorGUILayout.EndHorizontal();
				
			}
			UnityEditor.EditorGUILayout.EndVertical();
			
		}

		[UnityEditor.MenuItem("Assets/Create/UI Windows/Social/VK Settings")]
		public static void CreateInstance() {
			
			ME.EditorUtilities.CreateAsset<VKSettings>();
			
		}
		#endif

	}

}