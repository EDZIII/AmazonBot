using System;
using System.Collections.Generic;
using HtmlAgilityPack;

namespace AmaBot
{
	class Program
	{
		static void Main(string[] args)
		{
			string computerLink = "https://www.amazon.de/gp/browse.html?node=427957031&ref_=nav_em_T1_0_4_17_1__desk";

			string response = UtilWebRequest.Get(computerLink);

			#region first page computers
			HtmlDocument htmlDoc = new HtmlDocument();
			htmlDoc.LoadHtml(response);

			HtmlNodeCollection itemList = htmlDoc.DocumentNode.SelectNodes("//div[@class='s-item-container']");
			foreach (var n in itemList)
			{
				IEnumerable<HtmlNode> v = n.Descendants();
				foreach (var s in v)
				{
					string productTitle = s.GetAttributeValue("title", "");
					string productPreis = s.GetAttributeValue("class", "");
					string productHref = s.GetAttributeValue("href", "");

					string title = "";
					string preis = "";
					string link = "";
					if (productTitle != "")
					{
						title = productTitle;
						link = productHref;

					}
					if (productPreis == "a-size-base a-color-price a-text-bold")
					{
						preis = s.InnerText;

					}
					if (title != "")
					{
						Console.WriteLine(title);
						if (preis != "") { Console.WriteLine(preis); } else { Console.WriteLine("<no price>"); }
						Console.WriteLine(link);
					}
				}
			}
			#endregion
			//Alle produkte der ersten Seite sind aufgenommen worden
			//Jetzt get next page

			string linkToPgTwo = htmlDoc.DocumentNode.SelectSingleNode("//a[@class='pagnNext']").GetAttributeValue("href", "");
			response = UtilWebRequest.Get("https://www.amazon.de" + linkToPgTwo);
			int counter = 0;

			while (counter < 100)
			{
				htmlDoc.LoadHtml(response);
				HtmlNodeCollection productsContainers = htmlDoc.DocumentNode.SelectNodes("//div[@class='sg-col-4-of-12 sg-col-8-of-16 sg-col-16-of-24 sg-col-12-of-20 sg-col-24-of-32 sg-col sg-col-28-of-36 sg-col-20-of-28']");

				string productTitle="";
				string productPreis="";
				string productHref="";
				foreach (var n in productsContainers)
				{
					IEnumerable<HtmlNode> prodDetails = n.Descendants();
					foreach (var m in prodDetails)
					{
						if(m.GetAttributeValue("class","")== "a-size-medium a-color-base a-text-normal")
						{
							productTitle = m.InnerText;
							productHref = m.ParentNode.GetAttributeValue("href", "");
						}
						if (m.GetAttributeValue("class", "") == "a-price-whole")
						{
							productPreis = m.InnerText;
						}
					}
				
					Console.WriteLine(productTitle);
					Console.WriteLine(productPreis);
					Console.WriteLine(productHref);

				}
				string nextPage = LinkToNextPage(response);
			
				counter++;
				response = UtilWebRequest.Get("https://www.amazon.de" + nextPage);

			}

			Console.WriteLine("Seite: " + counter);
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
