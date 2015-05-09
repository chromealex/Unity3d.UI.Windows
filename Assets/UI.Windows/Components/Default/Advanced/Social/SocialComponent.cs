using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows.Plugins.Social;
using UnityEngine.UI.Windows.Plugins.Social.Modules.VK;

namespace UnityEngine.UI.Windows.Components.Social {

	public class SocialComponent : WindowComponent {

		public string token;

		public void Start() {

			var social = SocialSystem.instance;

			var vk = social.GetModule<VKModule>();

			Debug.Log("AUTH");
			vk.Authenticate(this.token, (result) => {

				Debug.Log(vk.profile.user);

				vk.profile.user.LoadFriends((fResult) => {

					Debug.Log("Friends: " + fResult);

				});

			});

		}

	}

}