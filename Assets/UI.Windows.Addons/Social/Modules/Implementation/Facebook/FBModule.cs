using UnityEngine;
using System.Collections;
using System;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI.Windows.Plugins.Social.Modules.Core;
using UnityEngine.UI.Windows.Plugins.Social.Core;

namespace UnityEngine.UI.Windows.Plugins.Social.Modules.Impl.FB {

	public class FBUsers<TSettings> : Users<TSettings> where TSettings : SubModuleSettings {

		public FBUsers(
			SocialModule socialModule,
			ModuleSettings moduleSettings,
			TSettings settings) : base(socialModule, moduleSettings, settings) {
			
		}

		public override string PrepareIds(string[] userIDs) {
			
			return string.Join(",", userIDs);
			
		}

	}

	public class FBModule : SocialModule {

		public override void OnLoadUser() {

			(this.profile as IProfile).user = new FBLocalUser();

		}
		
		public override void OnLoadModules() {

			base.OnLoadModules();

			this.RegisterModule<IUsers>(this.users = new FBUsers<UsersSettings>(this, this.settings, this.settings.standardUsersSettings));

		}

	}

}