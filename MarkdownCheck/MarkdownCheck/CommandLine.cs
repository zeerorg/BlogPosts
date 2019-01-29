using System;
using System.IO;
using McMaster.Extensions.CommandLineUtils;

namespace MarkdownCheck
{
  class CommandLine
  {
    [Option(Description = "Watch files")]
    public bool Watch { get; set; }

    [Option(LongName="source", ShortName="s", Description="Specify source files directory")]
    public string SourceDir { get; set; }

    [Option(LongName="output", ShortName="o", Description="Specify output files directory")]
    public string OutputDir { get; set; }

    [Option(LongName="json", ShortName="j", Description="Specify json file")]
    public string JsonFile { get; set; }

    public void OnExecute()
    {
      if (!(CommandLine.IsDirectory(SourceDir) && CommandLine.IsDirectory(OutputDir)))
      {
        Console.Error.WriteLine("Wrong source directory or output directory.");
        Console.WriteLine($"SourceDir: {SourceDir}\nOutputDir: {OutputDir}");
        return;
      }
      else
      {
        Console.WriteLine($"source directory is: {new DirectoryInfo(SourceDir).FullName}");
        Console.WriteLine($"output directory is: {new DirectoryInfo(OutputDir).FullName}");
      }

      Action postChange = () => {};
      if (CommandLine.IsFile(JsonFile))
      {
        Console.WriteLine($"json file is: {new FileInfo(JsonFile).FullName}");
        postChange = () => CreateRSS.DefaultCreate(OutputDir, JsonFile, Path.Combine(OutputDir, "rss.xml"));
      }

      new Compiler(SourceDir, OutputDir, postChange).Start();
      if (Watch)
      {
        while (Console.Read() != 'q') ;
      }
    }

    public static bool IsDirectory(string path)
    {
      if (path == null)
      {
        return false;
      }

      FileAttributes attr = File.GetAttributes(path);
      return attr.HasFlag(FileAttributes.Directory);
    }

    public static bool IsFile(string path)
    {
      if (path == null)
      {
        return false;
      }

      FileAttributes attr = File.GetAttributes(path);
      return !attr.HasFlag(FileAttributes.Directory);
    }
  }
}