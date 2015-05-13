using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine.UI.Windows.Plugins.Social.Core;
using UnityEngine.UI.Windows.Plugins.Social.Queries;

namespace UnityEngine.UI.Windows.Plugins.Social.Modules.Impl.FB {

	public class FBSettings : ModuleSettings {

		public override string GetPlatformName() {

			return "Facebook (facebook.com)";

		}
		
		public override string GetPlatformClassName() {
			
			return "FB";
			
		}

		[Header("FB Settings")]
		public string profileMainPhoto;

		public enum AuthType : byte {
			None,
			rerequest,
		}
		
		public AuthType authType;

		public enum Display : byte {
			Page,
			Popup,
			Touch,
		}
		
		public Display display;

		public enum ResponseType : byte {
			code, 			// Response data is included as URL parameters and contains code parameter (an encrypted string unique to each login request). This is the default behaviour if this parameter is not specified. It's most useful when your server will be handling the token.
			token, 			// Response data is included as a URL fragment and contains an access token. Desktop apps must use this setting for response_type. This is most useful when the client will be handling the token.
			code_token, 	// Response data is included as a URL fragment and contains both an access token and the code parameter.
			granted_scopes	// Returns a comma-separated list of all Permissions granted to the app by the user at the time of login. Can be combined with other response_type values. When combined with token, response data is included as a URL fragment, otherwise included as a URL parameter
		}
		
		public ResponseType responseType;

		public override HTTPParams Prepare(string token, HTTPParams parameters, Dictionary<string, string> values = null) {

			parameters = base.Prepare(token, parameters, values);

			foreach (var item in parameters.items) {
				
				if (item.key.ToLower() == "display") {
					
					item.value = this.display.ToString().ToLower();
					
				}
				
				if (item.key.ToLower() == "access_token") {
					
					item.value = token;
					
				}
				
				if (item.key.ToLower() == "auth_type") {

					if (this.authType != AuthType.None) {

						item.value = this.authType.ToString().ToLower();

					}

				}

				if (item.key.ToLower() == "response_type") {

					var responseType = this.responseType.ToString().ToLower();

					if (this.responseType == ResponseType.code_token) {

						responseType = "code%20token";

					}

					item.value = responseType;
					
				}

			}
			
			return parameters;
			
		}
		
		[HideInInspector]
		public List<string> declaredPermissionsGroup1 = new List<string>() {
			"public_profile",
			"user_friends", 
			"email"
		};
		
		[HideInInspector]
		public List<string> declaredPermissionsGroup2 = new List<string>() {
			"user_about_me", 
			"user_actions:books",
			"user_actions:fitness",
			"user_actions:music",
			"user_actions:news",
			"user_actions:video"
		};
		
		[HideInInspector]
		public List<string> declaredPermissionsGroup3 = new List<string>() {
			"user_birthday",
			"user_education_history", 
			"user_events", 
			"user_games_activity",
			"user_groups",
			"user_hometown",
			"user_likes",
			"user_location",
			"user_managed_groups",
			"user_photos",
			"user_posts",
			"user_relationships",
			"user_relationship_details",
			"user_religion_politics",
			"user_status",
			"user_tagged_places",
			"user_videos",
			"user_website",
			"user_work_history"
		};
		
		[HideInInspector]
		public List<string> declaredPermissionsGroup4 = new List<string>() {
			"read_custom_friendlists",
			"read_insights",
			"read_mailbox",
			"read_page_mailboxes",
			"read_stream",
			"manage_notifications",
			"manage_pages",
			"publish_pages",
			
			"publish_actions",
			
			"rsvp_event",
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

			UnityEditor.EditorGUILayout.HelpBox("Be sure that you have added `https://www.facebook.com/connect/login_success.html` url into your app settings in advanced tab. And your app enabled as Desktop Applicaiton.", UnityEditor.MessageType.Info);

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

		[UnityEditor.MenuItem("Assets/Create/UI Windows/Social/FB Settings")]
		public static void CreateInstance() {
			
			ME.EditorUtilities.CreateAsset<FBSettings>();
			
		}
		#endif

	}

}