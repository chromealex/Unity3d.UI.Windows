using UnityEngine;
using System.Collections;
using System;
using UnityEngine.SocialPlatforms.Impl;

namespace UnityEngine.UI.Windows.Plugins.Social.Modules.VK {

	public class VKUsers<TSettings> : Users<TSettings> where TSettings : SubModuleSettings {

		public VKUsers(
			SocialModule socialModule,
			ModuleSettings moduleSettings,
			TSettings settings) : base(socialModule, moduleSettings, settings) {
			
		}

		public override string PrepareIds(string[] userIDs) {
			
			return string.Join(",", userIDs);
			
		}

	}

	public class VKModule : SocialModule {

		public override void LoadUsers(string[] userIDs, Action<ISocialUser[]> callback) {

			(this.users as IUsers).LoadInfo(userIDs, callback);

		}

		public override void LoadFriends(Action<bool> callback) {

			(this.friends as IFriends).LoadInfo(callback);

		}

		public override void Authenticate(Action<bool> callback) {
			
			(this.auth as IAuth).Authenticate(callback);

		}
		
		public override void Authenticate(string token, Action<bool> callback) {
			
			(this.auth as IAuth).Authenticate(token, callback);

		}

		public override void OnLoadUser() {

			(this.profile as IProfile).user = new VKLocalUser();

		}
		
		public override void OnLoadModules() {

			base.OnLoadModules();

			this.RegisterModule<IUsers>(this.users = new VKUsers<UsersSettings>(this, this.settings, this.settings.standardUsersSettings));

		}

	}

}