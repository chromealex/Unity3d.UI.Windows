using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using UnityEngine.UI.Windows.Plugins.ABTesting.Net.Api;

namespace UnityEngine.UI.Windows.Extensions.Net {
	public class HttpClient {
		public bool log;
		public string logPrefix = "Http ";
		public bool keepAlive = false;
		public Dictionary<string, string> extraHeaders = new Dictionary<string, string>();

		public System.Collections.Generic.IEnumerator<byte> JsonPost(string host, string func, string jsonString,
			System.Action<string> callback,
			System.Action<string> callbackFail) {

			if (keepAlive) {
				return JsonPostWWW(host, func, jsonString, callback, callbackFail);
			} else {
				return JsonPostWebRequest(host, func, jsonString, callback, callbackFail);
			}
		}

		public System.Collections.Generic.IEnumerator<byte> JsonPostWWW(string host, string func, string jsonString,
			System.Action<string> callback,
			System.Action<string> callbackFail) {

			var url = string.Format("http://{0}{1}", host, func);
			if (log == true) Debug.Log(logPrefix + url);

			var bytes = System.Text.Encoding.UTF8.GetBytes(jsonString);

			var postHeader = new Dictionary<string, string>();
			postHeader.Add("Content-Type", "application/json; charset=utf-8");
			postHeader.Add("Accept", "application/json, text/javascript, text/json, */*");
			postHeader.Add("Content-Length", bytes.Length.ToString());
			foreach (var h in extraHeaders)
				postHeader.Add(h.Key, h.Value);

			var www = new WWW(url,
				bytes,
				postHeader);

			while (www.isDone == false) {
				yield return 0;
			}

			// check for errors
			if (www.error != null) {
				//Debug.LogError("WWW Error: " + www.error);
				callbackFail(www.error);

			} else {
				//Debug.Log(System.Text.Encoding.UTF8.GetString(www.bytes));
				callback(www.text);
			}

			www.Dispose();
			www = null;
		}

		public System.Collections.Generic.IEnumerator<byte> JsonPostWebRequest(string host, string func, string jsonString,
			System.Action<string> callback,
			System.Action<string> callbackFail) {

			var url = string.Format("http://{0}{1}", host, func);
			if (log == true) Debug.Log(logPrefix + url);

			var bytes = System.Text.Encoding.UTF8.GetBytes(jsonString);

			var wr = (HttpWebRequest) WebRequest.Create(url);
			wr.ContentType = "application/json";
			wr.Method = "POST";
			wr.KeepAlive = true;
			foreach (var h in extraHeaders)
				wr.Headers.Add(h.Key, h.Value);
			wr.ContentLength = bytes.Length;

			var streamOut = wr.GetRequestStream();
			streamOut.Write(bytes, 0, bytes.Length);

			while (!wr.HaveResponse) yield return 0;

			try {
				string text;
				using (HttpWebResponse response = wr.GetResponse() as HttpWebResponse)
					using (StreamReader reader = new StreamReader(response.GetResponseStream(), System.Text.Encoding.UTF8)) {
						text = reader.ReadToEnd();
						reader.Close();
						response.Close();
					}

				callback(text);

				// check for errors
			} catch (Exception e) {
				var text = e.Message;
				Debug.LogError("WWW Error: " + text);
				callbackFail(text);
			}
		}
	}
}
