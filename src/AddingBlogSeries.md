# Displaying a Series of Posts

<!-- Here is the preview of the new feature:
![Series in a post](http://localhost:8080/Photos/series-feature-img.png) -->
This is another feature that I didn't have in mind at the start. I was inspired by dev's series feature and I even created my first ever series: ["my blog"](https://dev.to/zeerorg/my-blogs-architecture-43p7). The TECH/CASUAL/MAJOR sections on the [front page](https://blog.zeerorg.site/) are gone, they were taking up valuable space and I'm pretty sure instead of sorting out the posts they added to the confusion.

One of the big reasons I wanted to add this is because of what I'm working on as the next series of posts. I don't want to spoil the fun so I won't tell what I'm exactly working on but the posts would need to be tied together coherently to make better sense.

## Dev's series are limited

I took the concept of series from dev.to but I found the implementation fits greatly in dev but not so on my blog. So, I came up with a more comprehensive design which when implemented fitted perfectly on my blog.

## Changes to Content Management System

I've talked briefly about my content management system in the previous post in the series. I've wanted to keep things on this blog as simple as possible and push most of the complexity towards the frontend which is the major processing layer in the architecture. Keeping things simple, my content management system has:

- `main.json` : Contains metadata for all the posts.

```javascript
[
  {
    "id": 4,
    "slug": "blog-development-live-preview",
    "published": true,
    "filename": "LivePreviewBlogChanges",
    "type": "tech",
    "tldr": "Adding live preview of changes when I'm writing the blog.",
    "title": "Live Preview Blog Edits",
    "timestamp": 1544608657,
    "dev_to": "https://dev.to/zeerorg/live-preview-blog-edits-2oh1",
    "series": "this-blog-series"
  },
  {
    "id": 6,
    "slug": "frontend-caching",
    "published": true,
    "filename": "FrontEndCaching",
    "type": "tech",
    "tldr": "A simple trick to improve site performance with frontend caching",
    "title": "Caching data in frontend",
    "timestamp": 1545049130,
    "dev_to": "https://dev.to/zeerorg/caching-data-in-frontend-3973"
  }
]
```

- `html/` : Contains the markdown content compiled to html.

- `series.json` : Contains details about the series and list of posts in that series.

```javascript
{
  "this-blog-series": {
    "posts": [
      "this-blog-architecture",
      "blog-development-live-preview",
      "rss-feed-azure-functions",
      "adding-series-to-blog"
    ]
  }
}
```

I introduced a new `series.json` so that the frontend stays compliant with the existing `main.json` and I don't have to calculate the posts in the series on frontend which would be more complex and error prone. I found a middle ground between backend and frontend complexity.

## Optimization sacrifices

I'll admit my frontend caching design is not the most optimized approach. The 2 main costly operations are:

1. **Sending a network request everytime I fetch from cache**. The render prop component [`GetContent`](https://github.com/zeerorg/BlogSite/blob/master/src/Components/GetContent.jsx) which is responsible for caching and loading data from backend, initially acquires data from cache and also sends a request to the backend to fetch the latest data. This "update on new" caching provides better experience to viewers, as they get the latest updates instantly without closing the browser session but also don't have to wait for the network request to complete to get the first render.

2. **Parsing the text to JSON, everytime I need to access metadata**. Everytime there's a need to fetch `main.json`, `GetContent` makes the call, hence only the raw text version is returned which is parsed using `JSON.Parse` utility. The only way to prevent this is to introduce Redux or maintain a global variable for the data. This would also mean treating json data and html data differently and the responsibility will fall on the shoulders of `GetContent` and any component which fetches the data. This increase in complexity will certainly won't help much when compared to single render time, since the parse is called only once per render.

## Conclusion

Even with some sacrifices, the project has scaled with the addition of a new feature without giving a headache, I'd consider this an achievement. I'm also really happy with how this feature turned out to be and I'm looking forward for your feedback.