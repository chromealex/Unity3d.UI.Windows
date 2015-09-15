using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Text;
using UnityEngine.UI.Windows.Plugins.Social.ThirdParty;
using System.IO;

public class HttpRequest : IHttpRequest {

	public virtual void MakeRequest(string url, string method, System.Action<HttpResult> onResult, System.Func<HttpWebRequest, bool> modifyer = null) {

		var res = new HttpResult();

		HttpWebRequest r = WebRequest.Create(url) as HttpWebRequest;

		r.Method = method;

		if (modifyer != null) {
			modifyer(r);
		}

		try {

			HttpWebResponse response = r.GetResponse() as HttpWebResponse;
			using (Stream rspStm = response.GetResponseStream()) {

				using (StreamReader reader = new StreamReader(rspStm)) {

					res.response = reader.ReadToEnd();

				}

				res.statusCode = (int)response.StatusCode;

			}
		} catch (System.Net.WebException ex){

			// get error details sent from the server
			StreamReader reader = new StreamReader(ex.Response.GetResponseStream());
			res.response = reader.ReadToEnd();
			res.errDescr = ex.Message;
			res.statusCode = (int)ex.Status;
			
		}

		if (onResult != null) onResult(res);

	}

	public virtual void Get(string url, System.Action<HttpResult> onResult) {
		this.MakeRequest(url, HttpMethods.GET, onResult);
	}

	public virtual void Post(string url, Dictionary<string, string> data, System.Action<HttpResult> onResult) {
	
		this.MakeRequest(url, HttpMethods.POST, onResult, (r) => {

			JSONObject js = new JSONObject(data);
			var requestPayload = js.ToString();

			UTF8Encoding encoding = new UTF8Encoding();
			r.ContentLength = encoding.GetByteCount(requestPayload);
			r.Credentials = CredentialCache.DefaultCredentials;
			r.Accept = "application/json";
			r.ContentType = "application/json";
			
			//Write the payload to the request body.
			using ( Stream requestStream = r.GetRequestStream())
			{
				requestStream.Write(encoding.GetBytes(requestPayload), 0,
				                    encoding.GetByteCount(requestPayload));
			}

			return false;

		});

	}

}
