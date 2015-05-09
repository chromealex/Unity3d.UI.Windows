using UnityEngine;
using System.Collections;
using System;
using UnityEngine.SocialPlatforms.Impl;

namespace UnityEngine.UI.Windows.Plugins.Social.Modules.VK {

	public class VKModule : SocialModule {

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

	}

}