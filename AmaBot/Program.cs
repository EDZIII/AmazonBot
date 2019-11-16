using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using HtmlAgilityPack;
using Newtonsoft;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AmaBot
{
	class Program
	{
		static void Main(string[] args)
		{
			string computerLink = "https://www.amazon.de/gp/browse.html?node=427957031&ref_=nav_em_T1_0_4_17_1__desk";
			string pathProductsComputers = @"C:\Users\edocr\Desktop\GIT Projects\AmaBot\AmaBot\Products\computers.json";
			string response = UtilWebRequest.Get(computerLink);
			Stopwatch sw = new Stopwatch();
			sw.Start();
			string deserializedJsonString = string.Empty;
			using (StreamReader sr = new StreamReader(pathProductsComputers))
			{
				deserializedJsonString = sr.ReadToEnd();

			}

			JObject productsJObjects = JsonConvert.DeserializeObject<JObject>(deserializedJsonString);
			string productsJSON = productsJObjects.GetValue("Products").ToString();
			List<Products> prodList = JsonConvert.DeserializeObject<List<Products>>(productsJSON);

			HtmlDocument htmlDoc = new HtmlDocument();
			htmlDoc.LoadHtml(response);

			string linkToPgTwo = htmlDoc.DocumentNode.SelectSingleNode("//a[@class='pagnNext']").GetAttributeValue("href", "");
			response = UtilWebRequest.Get("https://www.amazon.de" + linkToPgTwo);
			int counter = 0;
			int maxPageRange = 200;
			while (counter < maxPageRange)
			{
				htmlDoc.LoadHtml(response);
				HtmlNodeCollection productsContainers = htmlDoc.DocumentNode.SelectNodes("//div[@class='sg-col-4-of-12 sg-col-8-of-16 sg-col-16-of-24 sg-col-12-of-20 sg-col-24-of-32 sg-col sg-col-28-of-36 sg-col-20-of-28']");

				string productTitle = "";
				string productPreis = "";
				string productHref = "";
				foreach (var n in productsContainers)
				{
					IEnumerable<HtmlNode> prodDetails = n.Descendants();
					foreach (var m in prodDetails)
					{
						if (m.GetAttributeValue("class", "") == "a-size-medium a-color-base a-text-normal")
						{
							productTitle = m.InnerText;
							productHref = m.ParentNode.GetAttributeValue("href", "");
						}
						if (m.GetAttributeValue("class", "") == "a-price-whole")
						{
							if (m.InnerText == "")
							{
								productPreis = "~";
							}
							else
							{
								productPreis = m.InnerText;
							}
						}
					}
					Products prod = new Products
					{
						Date = DateTime.Now.ToString("dd.MM.yyyy"),
						Title = productTitle,
						Price = productPreis,
						Href = "https://www.amazon.de" + productHref
					};
					prodList.Add(prod);

				}
				string nextPage = LinkToNextPage(response);

				counter++;
				Console.WriteLine("Seite " + counter + " von " + maxPageRange);
				response = UtilWebRequest.Get("https://www.amazon.de" + nextPage);
				if (response == "")
				{
					Console.WriteLine("End of the pages");
					break;
				}

			}
			Console.WriteLine("End of AmaBot");
			sw.Stop();
			Console.WriteLine("Process completion time "+sw.ElapsedMilliseconds);
			File.WriteAllText(pathProductsComputers, "{\n\"Products\" : " + JsonConvert.SerializeObject(prodList) + "\n}");
			Console.ReadLine();
		}

		/// <summary>
		/// looks for the link to the next page
		/// </summary>
		/// <param name="response"></param>
		/// <returns></returns>
		private static string LinkToNextPage(string response)
		{

			HtmlDocument htmlDoc = new HtmlDocument();
			htmlDoc.LoadHtml(response);
			HtmlNodeCollection nodeColl = htmlDoc.DocumentNode.SelectNodes("//a");
			string href = "";

			foreach (var x in nodeColl)
			{
				href = x.GetAttributeValue("href", "");
				string hrefClass = x.GetAttributeValue("class", "");

				if (x.InnerText.Contains("Weiter") && x.ParentNode.GetAttributeValue("class", "") == "a-last")
				{
					break;
				}
			}
			return href;
		}
	}
}
