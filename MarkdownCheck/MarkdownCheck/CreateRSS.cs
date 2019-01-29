using System;
using System.Threading.Tasks;
using System.ServiceModel.Syndication;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Xml;
using System.Linq;

namespace MarkdownCheck
{
  public class CreateRSS
    {
        private string JSONData;
        private Func<string, Task<string>> GetContent;
        
        public CreateRSS(string jsonData, Func<string, Task<string>> getContent)
        {
            this.JSONData = jsonData;
            this.GetContent = getContent;
        }

        public static void DefaultCreate(string htmlDir, string jsonFile, string outputFile)
        {
          Console.WriteLine("Creating RSS");
          string jsonData = File.ReadAllText(jsonFile);
          Func<string, Task<string>> GetContent = async (fileName) => {
            return await Task.Run<string>(() => File.ReadAllText(Path.Combine(htmlDir, fileName+".html")));
          };
          string rssFeed = new CreateRSS(jsonData, GetContent).GetXml().GetAwaiter().GetResult();
          File.WriteAllText(outputFile, rssFeed);
        }

        public async Task<string> GetXml()
        {
            var feed = CreateFeed("Rishabh's Blog", "I'm Rishabh, I blog about software development and some casual stuff.", new Uri("https://blog.zeerorg.site/"));

            SyndicationPerson sp = new SyndicationPerson("r.g.gupta@outlok.com", "Rishabh Gupta", "https://zeerorg.site/");
            feed.Authors.Add(sp);

            feed.Items = await Task.WhenAll<SyndicationItem>(JArray.Parse(JSONData).Reverse().Cast<JObject>().Where(json => (bool)json["published"]).Select(GetSyndicationItem(GetContent)));
            return FeedToXml(feed);
        }

        public string FeedToXml(SyndicationFeed feed)
        {
            StringWriter sw = new StringWriter();
            XmlWriter rssWriter = XmlWriter.Create(sw);
            Rss20FeedFormatter rssFormatter = new Rss20FeedFormatter(feed, false);
            rssFormatter.WriteTo(rssWriter);
            rssWriter.Close();
            return sw.ToString();
        }

        public SyndicationFeed CreateFeed(string title, string description, Uri url)
        {
            SyndicationFeed feed = new SyndicationFeed(title, description, url);
            feed.Language = "en-us";
            feed.LastUpdatedTime = DateTime.Now;
            return feed;
        }

        public Func<JObject, Task<SyndicationItem>> GetSyndicationItem(Func<string, Task<string>> GetContent)
        {
            return async (json) =>
            {
                SyndicationItem item = new SyndicationItem((string)json["title"], (string)json["tldr"], new Uri($"https://blog.zeerorg.site/post/{json["slug"]}"));
                item.LastUpdatedTime = DateTimeOffset.FromUnixTimeSeconds((long)json["timestamp"]);

                XmlElement content = new XmlDocument().CreateElement("content", "encoded", "http://purl.org/rss/1.0/modules/content/");
                content.InnerText = await GetContent((string)json["filename"]);
                item.ElementExtensions.Add(content);
                return item;
            };
        }

    }
}