using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace UnityEngine.UI.Windows.Plugins.Social {

	public enum HTTPType {

		Get,
		Post,

	};

	[System.Serializable]
	public class HTTPParams {

		[System.Serializable]
		public class Item {

			public enum Type : byte {

				Constant,
				GetPermissions,

			}

			public string key;
			public Type type;
			public string value;

			public string GetValue(ModuleSettings settings) {

				var result = string.Empty;

				switch (this.type) {

				case Type.Constant:
					result = this.value;
					break;

				case Type.GetPermissions:
					result = settings.GetPermissions();
					break;

				}

				return result;

			}

		}

		public Item[] items;

	}

	public class SocialHttp {

		private WWW www;
		
		public SocialHttp(string url, System.Action<Texture, bool> onCompleted) {

			this.www = new WWW(url);

			SocialSystem.WaitFor(this.Wait(() => {

				if (string.IsNullOrEmpty(this.www.error) == false) {

					Debug.LogError(this.www.error);

					// error
					onCompleted(null, false);
					
				} else {
					
					// success
					onCompleted(this.www.texture, true);
					
				}

			}));

		}

		public SocialHttp(ModuleSettings settings, string url, HTTPParams parameters, HTTPType httpType, System.Action<string, bool> onCompleted) {

			if (httpType == HTTPType.Post) {

				var form = new WWWForm();
				foreach (var param in parameters.items) {

					form.AddField(param.key, param.GetValue(settings));

				}

				this.www = new WWW(url, form);

			} else if (httpType == HTTPType.Get) {

				foreach (var param in parameters.items) {

					url = url.Replace("{" + param.key + "}", param.GetValue(settings));

				}
				
				this.www = new WWW(url);

			}

			Debug.Log(url);

			SocialSystem.WaitFor(this.Wait(() => {

				if (string.IsNullOrEmpty(this.www.error) == false) {
					
					Debug.LogError(this.www.error);

					// error
					onCompleted(this.www.error, false);

				} else {

					// success
					onCompleted(this.www.text, true);

				}

			}));

		}

		private IEnumerator Wait(System.Action callback) {

			yield return this.www;

			callback();

		}

	}

}