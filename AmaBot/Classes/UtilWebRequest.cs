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
		private static CookieContainer sessionCookies = new CookieContainer();
		public static string Get(string uri)
		{
			HttpWebRequest webReq = (HttpWebRequest)HttpWebRequest.Create(uri);

			webReq.CookieContainer = sessionCookies;
			webReq.Method = "GET";
			webReq.AllowAutoRedirect = true;
			webReq.UseDefaultCredentials = true;
			webReq.PreAuthenticate = true;
			

			string responseFromServer;

			using (WebResponse response = webReq.GetResponse())
			{
				using (Stream stream = response.GetResponseStream())
				{
					StreamReader reader = new StreamReader(stream);
					responseFromServer = reader.ReadToEnd();
				}
			}

			sessionCookies = webReq.CookieContainer;


			return responseFromServer;
		}

		public static bool GetFileDownload(string uri, string pfad)
		{
			HttpWebRequest webReq = (HttpWebRequest)HttpWebRequest.Create(uri);

			webReq.CookieContainer = sessionCookies;
			webReq.Method = "GET";
			webReq.AllowAutoRedirect = true;
			webReq.UseDefaultCredentials = true;
			webReq.PreAuthenticate = true;

			// if the URI doesn't exist, an exception will be thrown here...
			using (HttpWebResponse httpResponse = (HttpWebResponse)webReq.GetResponse())
			{
				using (Stream responseStream = httpResponse.GetResponseStream())
				{
					using (FileStream localFileStream =
						new FileStream(pfad, FileMode.Create))
					{
						var buffer = new byte[4096];
						long totalBytesRead = 0;
						int bytesRead;

						while ((bytesRead = responseStream.Read(buffer, 0, buffer.Length)) > 0)
						{
							totalBytesRead += bytesRead;
							localFileStream.Write(buffer, 0, bytesRead);
						}
					}
				}
			}

			sessionCookies = webReq.CookieContainer;

			return true;
		}



	}
}
