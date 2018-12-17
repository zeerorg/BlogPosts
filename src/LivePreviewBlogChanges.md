# Live Preview Blog Edits

_This is 2nd post in continuation to my major project on this blog._

The original requirements didn't consider live preview as an option. Seems like I was wrong, the live preview would help me see the current changes inside the window. This also gives me the flexibility to see my changes and gives me more confidence in what I'm writing. A lot of these changes are not compatible with what I see in my editor markdown preview because I'm using a custom CSS for posts.

Adding live preview involves changes in the frontend and a test backend. One option I considered was:

**Uploading directly to Azure storage.** This would be great since I won't have to change much in the backend and could leverage what I've built till now. But life ain't that easy, uploading to the backend takes some time and the azure blob trigger can take up to 10 minutes to run 😲!! This solution was obviously not viable, I cannot escape this ordeal easily.

## Test Backend

I reluctantly start with building a backend. The good part is that laying out a simple blog architecture makes this process much easier. If I could find a way to provide the current draft as an input to the frontend I could preview the changes in my local machine. This proved to be a relatively easy problem to tackle.

For the backend, I run an HTTP based file server to emulate my blob storage use. I use nodejs-based [http-server](https://www.npmjs.com/package/http-server "Http server npm page").

For the frontend, I check if the `process.env.NODE_ENV` environment is set to `"development"` and point to the local server.

## Building markdown on change

My azure function backend uses [Markdig](https://github.com/lunet-io/markdig "Markdig github page") to compile markdown to HTML. I have a test project from few days before, that used Markdig to compile markdown, I picked it up to repurpose it for building the markdown files on change. I implemented a simple file watcher and compiled the files that changed to markdown. I'll link down the code.

## What I learned

C# [FileSystemWatcher](https://docs.microsoft.com/en-us/dotnet/api/system.io.filesystemwatcher?view=netcore-2.1 "C# file system watcher api") was pretty easy to implement. I also learned about the `process.env.NODE_ENV` variable in React.

## Code links

* [Blog Posts](https://github.com/zeerorg/BlogPosts/tree/master/src "Github page for blog posts")
* [Markdown file watcher and compiler](https://github.com/zeerorg/BlogPosts/tree/master/MarkdownCheck "Github link to markdown compiler")
* [React Frontend](https://github.com/zeerorg/BlogSite "Frontend written in react")
