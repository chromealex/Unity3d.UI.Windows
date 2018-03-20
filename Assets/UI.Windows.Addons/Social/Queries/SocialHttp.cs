//#define USE_WWW

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI.Windows.Plugins.Social.Core;
using System.Net;
using System.IO;
using System;

namespace UnityEngine.UI.Windows.Plugins.Social.Queries {

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

					if (UnityEngine.UI.Windows.Constants.LOGS_ENABLED == true) UnityEngine.Debug.LogError(this.www.error);

					// error
					onCompleted(null, false);
					
				} else {
					
					// success
					onCompleted(this.www.texture, true);
					
				}

			}));

		}

		public SocialHttp(ModuleSettings settings, string url, HTTPParams parameters, HTTPType httpType, System.Action<string, bool> onCompleted) {

			#if USE_WWW
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

			SocialSystem.WaitFor(this.Wait(() => {

				if (string.IsNullOrEmpty(this.www.error) == false) {
					
					if (UnityEngine.UI.Windows.Constants.LOGS_ENABLED == true) UnityEngine.Debug.LogError(this.www.error);

					// error
					onCompleted(this.www.error, false);

				} else {

					// success
					onCompleted(this.www.text, true);

				}

			}));
			#else
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

				ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(delegate { return true; });

				var uri = new Uri(url.Trim() + "/");	// I don't know about last `/` but it works only with it ;(

				WebRequest request = WebRequest.Create (uri);
				// If required by the server, set the credentials.
				request.Credentials = CredentialCache.DefaultCredentials;
				// Get the response.
				HttpWebResponse response = (HttpWebResponse)request.GetResponse ();

				// Get the stream containing content returned by the server.
				Stream dataStream = response.GetResponseStream ();
				// Open the stream using a StreamReader for easy access.
				StreamReader reader = new StreamReader (dataStream);
				// Read the content.
				string responseFromServer = reader.ReadToEnd ();

				// Cleanup the streams and the response.
				reader.Close ();
				dataStream.Close ();
				response.Close ();
				
				onCompleted(responseFromServer, response.StatusCode == HttpStatusCode.OK);

			}
			#endif
			
			if (UnityEngine.UI.Windows.Constants.LOGS_ENABLED == true) UnityEngine.Debug.Log(url);

		}

		private System.Collections.IEnumerator Wait(System.Action callback) {

			yield return this.www;

			callback();

		}

	}

}