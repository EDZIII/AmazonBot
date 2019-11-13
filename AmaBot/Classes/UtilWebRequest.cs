using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using HtmlAgilityPack;

namespace AmaBot
{
	class UtilWebRequest
	{
		//The cookies will be here.
		private static CookieContainer _cookies = new CookieContainer();

		//In case you need to clear the cookies
		public void ClearCookies()
		{
			_cookies = new CookieContainer();
		}

		public static string Get(string url)
		{

			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
			request.Method = "GET";
			request.Timeout = 6000; //60 second timeout
			request.UserAgent = "Mozilla/5.0 (compatible; MSIE 9.0; Windows Phone OS 7.5; Trident/5.0; IEMobile/9.0)";

			//Set more parameters here...
			//...

			//This is the important part.
			request.CookieContainer = _cookies;

			HttpWebResponse response = (HttpWebResponse)request.GetResponse();
			var stream = response.GetResponseStream();

			//When you get the response from the website, the cookies will be stored
			//automatically in "_cookies".

			using (var reader = new StreamReader(stream))
			{
				string html = reader.ReadToEnd();

				return html;
			}
		}
		protected static void AppendParameter(StringBuilder sb, string name, string value)
		{
			string encodedValue = HttpUtility.UrlEncode(value);
			sb.AppendFormat("{0}={1}&", name, encodedValue);
		}

		public static string Post(Dictionary<string, string> dict, string requestUrl)
		{
			StringBuilder sb = new StringBuilder();
			foreach(KeyValuePair<string,string> kvp in dict)
			{
			AppendParameter(sb,kvp.Key, kvp.Value);
			}

			byte[] byteArray = Encoding.UTF8.GetBytes(sb.ToString());

			string url = requestUrl;

			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
			request.Method = "POST";
			request.ContentType = "application/x-www-form-urlencoded";
			//request.Credentials = CredentialCache.DefaultNetworkCredentials; // ??

			using (Stream requestStream = request.GetRequestStream())
			{
				requestStream.Write(byteArray, 0, byteArray.Length);
			}

			string response = request.GetResponse().ToString();

			return response;
		}

	}
}
