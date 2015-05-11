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

		[Flags]
		public enum Permissions : int {
			Notify = 1,
			//(+1)	User allowed to send notifications to him/her.
			Friends = 2, 
			//(+2)	Access to friends.
			Photos = 4, 
			//(+4)	Access to photos.
			Audio = 8, 
			//(+8)	Access to audios.
			Video = 16,
			//(+16)	Access to videos.
			Docs = 131072,
			//(+131072)	Access to documents.
			Notes = 2048, 
			//(+2048)	Access to user notes.
			Pages = 128,
			//(+128)	Access to wiki pages.
			//+256	Addition of link to the application in the left menu.
			Status = 1024, 
			//(+1024)	Access to user status.
			//Offers = 32,
			//(+32)	Access to offers (obsolete methods).
			//Questions = 64, 
			//(+64)	Access to questions (obsolete methods).
			Wall = 8192,
			//(+8192)	Access to standard and advanced methods for the wall. Note that this access permission is unavailable for sites (it is ignored at attempt of authorization).
			Groups = 262144, 
			//(+262144)	Access to user groups.
			Messages = 4096, 
			//(+4096)	(for Standalone applications) Access to advanced methods for messaging.
			Email = 4194304,
			//(+4194304)	User e-mail access. Available only for sites.
			Notifications = 524288,
			//(+524288)	Access to notifications about answers to the user.
			Stats = 1048576,
			//(+1048576)	Access to statistics of user groups and applications where he/she is an administrator.
			Ads = 32768,
			//(+32768)	Access to advanced methods for Ads API.
			//Offline, //	Access to API at any time from a third party server.
			//Nohttps, //	Possibility to make API requests without HTTPS. Note that this functionality is under testing and can be changed.
		}

		[BitMask(typeof(Permissions))]
		public Permissions permissions;

		public override string GetPermissions() {
			
			var scopes = this.permissions.ToString().Split(',');
			for (int i = 0; i < scopes.Length; ++i) scopes[i] = scopes[i].Trim().ToLower();

			return string.Join(",", scopes);

		}

		#if UNITY_EDITOR
		[UnityEditor.MenuItem("Assets/Create/UI Windows/Social/VK Settings")]
		public static void CreateInstance() {
			
			ME.EditorUtilities.CreateAsset<VKSettings>();
			
		}
		#endif

	}

}