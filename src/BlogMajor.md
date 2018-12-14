# Blog's Architecture

This is my first blog on how I'm creating this blog. I've started writing this before the blog site is up detailing what restrictions I'm going to place, the features I'll be adding and how I'll go about them.

---

## Block diagrams ftw

A simple block diagram for the overall architecture.
![Architecture Diagram](https://zeerorgprocessedblog.blob.core.windows.net/photos/blog-architecture.png)

## Requirements (or maybe restrictions)

Every project needs some requirements so I should underline those for this blog. I want to work under these so I don't go on doing something I actually regret later on.

1. **Easy to write**. I don't want to go though the "write-build-deploy" cycle over and over again everytime I write a post. The process should be as easy as typing in my editor, seeing the preview and uploading the file. That's it, no preprocessing, no building, minimal stuff.  
2. **No bullsh*t features**. The feature set should be minimal. So things like comments and reacting or sharing of a post are not going to be included. Any new feature added will have an explanation on why its there.
3. **Easier on the bill**. This is not going to earn me something so I want this to be as easy on the wallet as possible. This might also push me to find newer solutions (hopefully).

---

## Going about it

This is why and what I decided.

1. **Storing in Azure blobs**. This is going to be the place for storing original blog markdown files. This keeps a backup and also creates a destination for original source files.  
2. **Stored files are automatically processed when uploaded**. I set this up and the files are processed without my interaction. These processed files are used by the backend to answer requests.  
3. **Frontend and backend live separately**. This is more of a personal choice than a practical one. I'll be writing the blog in react and I need a backend to query my blogs.  
4. **Functions as backend**. Functions are scalable though I don't think I'll need them for this feature. They are also really cheap to run compared to an always on server.  
5. **Hosting frontend on Netlify**. I have heard good things about netlify and wanted to try it out. Its free tier is great and I can add my custom domain for free.  
