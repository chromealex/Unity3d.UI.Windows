using UnityEngine;
using System.Collections;
using System.Net;
using System.Collections.Generic;

public static class HttpMethods {

	public const string CONNECT = "CONNECT";
	public const string GET = "GET";
	public const string HEAD = "HEAD";
	public const string MKCOL = "MKCOL";
	public const string POST = "POST";
	public const string PUT = "PUT";
	public const string DELETE = "DELETE";

}

public static class ContentType {

	public const string PLAIN_TEXT = "plain/text";
	public const string APPLICATION_JSON = "application/json";

}

public class HttpResult {

	public string response = string.Empty;
	public string errDescr = string.Empty;
	/// <summary>
	/// The status code.
	///	System.Net.HttpStatusCode or System.Net.WebExceptionStatus on exception
	/// </summary>
	public int statusCode = -1;

	public bool IsSuccess() {
		return string.IsNullOrEmpty(this.errDescr);
	}

}

public interface IHttpRequest {

	void MakeRequest(string url, string method, System.Action<HttpResult> onResult, System.Func<HttpWebRequest, bool> modifyer);
	void Get(string url, System.Action<HttpResult> onResult);
	void Post(string url, Dictionary<string, string> data, System.Action<HttpResult> onResult);

}
