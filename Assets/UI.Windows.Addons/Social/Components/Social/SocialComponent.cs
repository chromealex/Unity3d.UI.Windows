using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows.Plugins.Social.Core;
using UnityEngine.UI.Windows.Plugins.Social.Modules.Impl.VK;
using UnityEngine.UI.Windows.Plugins.Social.Modules.Impl.FB;

namespace UnityEngine.UI.Windows.Components.Social {

	public class SocialComponent : WindowComponent {
		
		public string vkToken;
		public string fbToken;

		public void Start() {

			var social = SocialSystem.instance;

			var vk = social.GetModule<VKModule>();
			if (vk != null) vk.Authenticate(this.vkToken, (result) => {

				Debug.Log(vk.profile.user);

				vk.profile.user.LoadFriends((fResult) => {

					Debug.Log("Friends: " + fResult);

				});

			});
			
			var fb = social.GetModule<FBModule>();
			if (fb != null) fb.Authenticate(this.fbToken, (result) => {
				
				Debug.Log(fb.profile.user);
				
				fb.profile.user.LoadFriends((fResult) => {
					
					Debug.Log("Friends: " + fResult);
					
				});
				
			});
		}

	}

}