using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Markdig;

namespace MarkdownCheck
{
    class Program
    {
        string mdFiles = @"C:\Users\rishabh\Documents\Development\Blog\Posts\src\";
        string HTMLFolder = @"C:\Users\rishabh\Documents\Development\Blog\Posts\html\";

        static void Main(string[] args)
        {
            new Program().Start();
        }

        void Start()
        {
            FileInfo[] files = new DirectoryInfo(mdFiles).GetFiles("*.md");
            Task.WaitAll(files.Select(file => WriteHTML(file.FullName, RemoveFileExtension(file.Name, file.Extension))).ToArray());

            FileSystemWatcher watcher = new FileSystemWatcher();
            watcher.Path = mdFiles;
            watcher.NotifyFilter = NotifyFilters.LastWrite;
            watcher.Filter = "*.md";
            watcher.Changed += new FileSystemEventHandler(OnChange);

            watcher.EnableRaisingEvents = true;
            Console.WriteLine("Built Files. Starting watcher. Press 'q' to stop.");
            while (Console.Read() != 'q') ;
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
        }

        async Task WriteHTML(string fileLoc, string fileNameWitchoutExt)
        {
            string markdownData = await File.ReadAllTextAsync(fileLoc);
            string compiledHTML = Markdown.ToHtml(markdownData);
            await File.WriteAllTextAsync($"{HTMLFolder}{fileNameWitchoutExt}.html", compiledHTML);
        }

        string RemoveFileExtension(string fileName, string extension)
        {
            return fileName.Split(extension)[0];
        }
    }
}
