using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Markdig;

namespace MarkdownCheck
{
  class Compiler
  {
    public string SourceDir;
    public string OutputDir;

    public Action PostChangeHook;

    public Compiler(string source, string output, Action postChangeHook = null)
    {
      this.SourceDir = new DirectoryInfo(source).FullName;
      this.OutputDir = new DirectoryInfo(output).FullName;
      if (postChangeHook == null)
      {
        this.PostChangeHook = () => {};
      }
      else
      {
        this.PostChangeHook = postChangeHook;
      }
    }

    public void Start()
    {
      FileInfo[] files = new DirectoryInfo(SourceDir).GetFiles("*.md");
      Task.WaitAll(files.Select(file => WriteHTML(file.FullName, RemoveFileExtension(file.Name, file.Extension))).ToArray());
      this.PostChangeHook();

      FileSystemWatcher watcher = new FileSystemWatcher();
      watcher.Path = SourceDir;
      watcher.NotifyFilter = NotifyFilters.LastWrite;
      watcher.Filter = "*.md";
      watcher.Changed += new FileSystemEventHandler(OnChange);

      watcher.EnableRaisingEvents = true;
    }

    void OnChange(object obj, FileSystemEventArgs e)
    {
      FileInfo file = new FileInfo(e.FullPath);
      Console.WriteLine($"File ${file.Name} was updated.");
      bool retry = true;
      while (retry)
      {
        try
        {
          Task.WaitAll(WriteHTML(file.FullName, RemoveFileExtension(file.Name, file.Extension)));
          retry = false;
        }
        catch (AggregateException)
        {
          Console.WriteLine($"Exception ocurred while processing {file.Name}.");
          Thread.Sleep(100);
          retry = true;
        }
      }
      this.PostChangeHook();
    }

    async Task WriteHTML(string fileLoc, string fileNameWithoutExt)
    {
      string markdownData = await File.ReadAllTextAsync(fileLoc);
      string compiledHTML = Markdown.ToHtml(markdownData);
      await File.WriteAllTextAsync($"{Path.Combine(OutputDir, fileNameWithoutExt)}.html", compiledHTML);
    }

    string RemoveFileExtension(string fileName, string extension)
    {
      return fileName.Split(extension)[0];
    }
  }
}