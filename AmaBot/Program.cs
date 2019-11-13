using System;
using System.Collections.Generic;
using HtmlAgilityPack;

namespace AmaBot
{
	class Program
	{
		static void Main(string[] args)
		{
			//UtilWebRequest request = new UtilWebRequest();

			string get = UtilWebRequest.Get("https://www.amazon.de");

			HtmlDocument htmlDoc = new HtmlDocument();
			htmlDoc.LoadHtml(get);
			HtmlNodeCollection nodeColl = htmlDoc.DocumentNode.SelectNodes("//div");

			Dictionary<string, string> dict = new Dictionary<string, string>();
			dict.Add("__mk_de_DE", "ÅMÅŽÕÑ");
			dict.Add("url", "srs=7194943031&search-alias=specialty-aps");
			dict.Add("field-keywords", "a");
			string response = UtilWebRequest.Post(dict, "https://www.amazon.de");
		}
	}
}
