# Adding RSS with .NET Core

_This is 3rd post in continuation to my major project on this blog._

RSS is an old technology and has been fading out of the general ecosystem over the years. With Google shuting down its Google Reader service and Firefox [dropping support](https://dev.to/sudiukil/firefox-64-drops-rss-support-thoughts-4hbo "Firefox 64 drops RSS support") for RSS in their last 2018 release. I admit I've never actually gotten into RSS, setting up a reading list for different blogs. I am not a frequent blog reader and most of my news source is through apps which meant I never actually had any need for RSS.

So when I thought of adding RSS to my website it was actually a decision I took after thinking long and hard. I actually went ahead and tried out [Feedly](https://feedly.com/ "feedly") to get an idea of how modern RSS readers work. I can certainly see why a lot of people do like it, which forms the second reason why I thought of going ahead with it. The last reason is purely because I wanted to learn more about RSS and XML in general. This would be my first time implementing XML based schema for data.

## .NET Core SyndicationFeed

The main library that implements helper classes for RSS feed in .NET is [System.ServiceModel.Syndication](https://docs.microsoft.com/en-us/dotnet/api/system.servicemodel.syndication?view=netcore-2.2 "System.ServiceModel.Syndication"). The top level class is `SyndicationFeed` which can be serialized to not only the latest RSS 2.0 standard but also to Atom 1.0 feed so you can choose whichever you want. You'll need to add the library module through [NuGet](https://www.nuget.org/packages/System.ServiceModel.Syndication/ "NuGet Syndication Feed").

First we create a [syndication feed](https://docs.microsoft.com/en-us/dotnet/api/system.servicemodel.syndication.syndicationfeed?view=netcore-2.2 "Syndication Feed class reference") object and add preliminary information to it. Like the title, description and url of the blog. Then we add authors, here we can specify more than one authors.

```csharp
// Top level feed setup
SyndicationFeed feed = new SyndicationFeed(title, description, url);
feed.Language = "en-us";
feed.LastUpdatedTime = DateTime.Now;

// Adding Authors
SyndicationPerson sp = new SyndicationPerson("r.g.gupta@outlok.com", "Rishabh Gupta", "https://zeerorg.site/");
feed.Authors.Add(sp);
```

Every `<item>` element in RSS is represented as an object of [SyndicationItem](https://docs.microsoft.com/en-us/dotnet/api/system.servicemodel.syndication.syndicationitem?view=netcore-2.2 "SyndicationItem reference") class. The items can be added to the feed object.

```csharp
SyndicationItem item = new SyndicationItem(title, description, url);
item.LastUpdatedTime = DateTimeOffset.FromUnixTimeSeconds(timestamp);

feed.items.Add(item);
```

One important but optional element inside the `<item>` element is `<content:encoded>` element, this is important only for those feeds which also need to provide content of the post along with the updates. But setting this is not directly supported inside the api, so we need to create a custom element and assign it to the item.

```csharp
// The third argument specifies
XMLDocument doc = new XmlDocument()
XMLElement content = doc.CreateElement("content", "encoded", "http://purl.org/rss/1.0/modules/content/");
content.InnerText = htmlContent;
item.ElementExtensions.Add(content);
```

After setting up the feed it can be serialized to RSS 2.0 standard.

```csharp
StringWriter sw = new StringWriter();
XmlWriter rssWriter = XmlWriter.Create(sw);

// The second false specifies the format for feed.
// As it can be converted to an older RSS 2.0 format or the newer a10 format.
Rss20FeedFormatter rssFormatter = new Rss20FeedFormatter(feed, false);
rssFormatter.WriteTo(rssWriter);
rssWriter.Close();
string FeedString = sw.ToString();
```

## Getting RSS Feed for my blog

For my blog all the metadata is stored in a `main.json` file which is fetched from the blob storage. It's a very rudimentary content management system. The json contains a list of posts as json objects and each item contains data corresponding to that post. One of the data that is stored is `filename` which specifies the filename that contains the HTML to be fetched from blob container. The RSS feed generator extracts data from JSON object of every post for the `<item>` elements and sets the html content fetched from the blob container using `filename` attribute.

First, I used to trigger this function on every request, but after some tests I realized creating an RSS Feed on the fly was a costly operation, so I am triggering the creation of rss on every change to `main.json` and saving the results in a blob which is served directly.

Checkout my rss feed [here](https://blog.zeerorg.site/rss.xml "Rishabh's blog RSS Feed").