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
			List<KeyValuePair<string, string>> ProductsLink = new List<KeyValuePair<string, string>>();
			ProductsLink.Add(new KeyValuePair<string, string>("laptops", "https://www.amazon.de/gp/browse.html?node=427957031&ref_=nav_em_T1_0_4_17_1__desk"));
			ProductsLink.Add(new KeyValuePair<string, string>("pcKomponente", "https://www.amazon.de/gp/browse.html?node=427956031&ref_=nav_em_T1_0_4_17_4__compc"));

			foreach (var x in ProductsLink)
			{

				string jsonPath = string.Empty;
				switch (x.Key)
				{
					case "laptops":
						Console.WriteLine("Gathering laptops from amazon...");
						jsonPath = @"C:\Users\edocr\Desktop\Git\AmaBot\AmaBot\AmaBot\laptops.json";
						break;
					case "pcKomponente":
						Console.WriteLine("Gathering PC components from amazon...");
						jsonPath = @"C:\Users\edocr\Desktop\Git\AmaBot\AmaBot\AmaBot\pckomponente.json";
						break;
				}

				//Seite1 wird geladen			
				string response = UtilWebRequest.Get(x.Value);

				Stopwatch sw = new Stopwatch();
				sw.Start();

				//deserialisierung json datei damit objekte hinzugefügt werden können
				string deserializedJsonString = string.Empty;
				using (StreamReader sr = new StreamReader(jsonPath))
				{
					deserializedJsonString = sr.ReadToEnd();
				}

				List<Laptop> laptopList = null;
				List<PcKomponente> komponenteList = null;
				JObject productsJObjects = JsonConvert.DeserializeObject<JObject>(deserializedJsonString);
				switch (x.Key)
				{
					case "laptops":
						string laptopJSON = productsJObjects.GetValue("Laptop").ToString();
						laptopList = JsonConvert.DeserializeObject<List<Laptop>>(laptopJSON);
						break;
					case "pcKomponente":
						string komponenteJSON = productsJObjects.GetValue("PcKomponente").ToString();
						komponenteList = JsonConvert.DeserializeObject<List<PcKomponente>>(komponenteJSON);
						break;
				}

				HtmlDocument htmlDoc = new HtmlDocument();
				htmlDoc.LoadHtml(response);

				string linkToPgTwo = htmlDoc.DocumentNode.SelectSingleNode("//a[@class='pagnNext']").GetAttributeValue("href", "");
				response = UtilWebRequest.Get("https://www.amazon.de" + linkToPgTwo);
				int counter = 0;
				int maxPageRange = 50;
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
						switch (x.Key)
						{
							case "laptops":
								Laptop laptop = new Laptop
								{
									Date = DateTime.Now.ToString("dd.MM.yyyy"),
									Title = productTitle,
									Price = productPreis,
									Href = "https://www.amazon.de" + productHref
								};
								laptopList.Add(laptop);
								break;
							case "pcKomponente":
								PcKomponente pcKomp = new PcKomponente
								{
									Date = DateTime.Now.ToString("dd.MM.yyyy"),
									Title = productTitle,
									Price = productPreis,
									Href = "https://www.amazon.de" + productHref
								};
								komponenteList.Add(pcKomp);
								break;
						}

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
				//jsons werden dann in einem jsonFile serialisiert
				switch (x.Key)
				{
					case "laptops":
						File.WriteAllText(jsonPath, "{\n\"Laptop\" : " + JsonConvert.SerializeObject(laptopList) + "\n}");
						break;
					case "pcKomponente":
						File.WriteAllText(jsonPath, "{\n\"PcKomponente\" : " + JsonConvert.SerializeObject(komponenteList) + "\n}");
						break;
				}

				Console.WriteLine("Process completion time " + ElapsedTimeInMinutes(sw));
			}

			Console.WriteLine("End of AmaBot");
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

		/// <summary>
		/// returns the elapsed time in min sec format
		/// </summary>
		/// <param name="sw"></param>
		/// <returns></returns>
		private static string ElapsedTimeInMinutes(Stopwatch sw)
		{
			sw.Stop();
			int minutes = (int)sw.Elapsed.TotalMinutes;
			double fsec = 60 * (sw.Elapsed.TotalMinutes - minutes);
			int sec = (int)fsec;
			int ms = 1000 * ((int)fsec - sec);
			string tsOut = String.Format("{0}:{1:D2}.{2}", minutes, sec, ms);
			return tsOut;
		}
	}
}
