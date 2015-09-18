using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Text;
using UnityEngine.UI.Windows.Plugins.Social.ThirdParty;
using System;
using System.IO;

public class HttpRequestAsync : HttpRequest {

	private UTF8Encoding encoding = new UTF8Encoding();

	public override void MakeRequest(string url, string method, System.Action<HttpResult> onResult, System.Func<HttpWebRequest, bool> modifyer = null) {

		HttpWebRequest r = WebRequest.Create(url) as HttpWebRequest;
		
		r.Method = method;

		var isAsyncModifyer = false;
		if (modifyer != null) {
			isAsyncModifyer = modifyer(r);
		}

		if (isAsyncModifyer == false) {
			this.BeginRequest(r, onResult);
		}

	}

	public override void Post(string url, Dictionary<string, string> data, System.Action<HttpResult> onResult) {

		this.MakeRequest(url, HttpMethods.POST, onResult, (r) => {
			
			JSONObject js = new JSONObject(data);
			var requestPayload = js.ToString();

			r.ContentLength = encoding.GetByteCount(requestPayload);
			r.Credentials = CredentialCache.DefaultCredentials;
			r.Accept = "application/json";
			r.ContentType = "application/json";
			
			r.BeginGetRequestStream(new AsyncCallback((result) => { 
				var requestStream = r.EndGetRequestStream(result);
				requestStream.Write(encoding.GetBytes(requestPayload), 0, encoding.GetByteCount(requestPayload));
				this.BeginRequest(r, onResult);
			}), null);

			return true;

		});

	}

	private void BeginRequest(HttpWebRequest request, System.Action<HttpResult> onResult) {

		int total = 0;
		byte[] buffer = new byte[1000];

		request.BeginGetResponse(new AsyncCallback((result) => {
			var data = request.EndGetResponse(result);
			HttpResult r = new HttpResult();
			try {

				var stream = new StreamReader(data.GetResponseStream());
				r.response = stream.ReadToEnd();
				stream.Close();

				//TODO:!!
				/*stream.BeginRead(buffer, 0, 1000, new AsyncCallback((read) => {
					var r = stream.EndRead(read);
					if (r == 0) {
						//TODO: !!
					} else {
						//stream.BeginRead
					}
				}));*/

			} catch (Exception ex) {
				r.errDescr = ex.Message;
			} finally {
				data.Close();
			}

			if (onResult != null) {
				onResult(r);
			}

		}), null);
	}

}
