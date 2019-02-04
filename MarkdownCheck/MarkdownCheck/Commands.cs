using System;
using System.IO;
using CommandLine;

namespace MarkdownCheck
{
  class Commands
  {
    [Verb("develop", HelpText = "Start for development")]
    public class DevelopmentOptions
    {
      [Option('p', "server-path", Required = true, HelpText = "File server location")]
      public string FileServerPath { get; set; }

      [Option('s', "source", Required = true, HelpText = "Source directory")]
      public string SourceDir { get; set; }

      [Option('o', "output", Required = true, HelpText = "Output directory")]
      public string OutputDir { get; set; }

      [Option('j', "json", Required = true, HelpText="Specify json file")]
      public string JsonFile { get; set; }

      [Option('c', "command", Required = true, HelpText="Commad to run with server")]
      public string Command { get; set; }
    }

    [Verb("upload", HelpText = "Upload compiled blog")]
    public class UploadOptions
    {
      [Option('s', "source", Required = true, HelpText = "Source directory")]
      public string SourceDir { get; set; }

      [Option('o', "output", Required = true, HelpText = "Output directory")]
      public string OutputDir { get; set; }

      [Option('j', "json", Required = true, HelpText="Specify json file")]
      public string JsonFile { get; set; }

      [Option('n', "seriesjson", Required = true, HelpText="Specify series json file")]
      public string SeriesJsonFile { get; set; }

      [Option('p', "photos", Required = true, HelpText="Specify photos directory")]
      public string PhotosDir { get; set; }
    }

    [Verb("compile", HelpText = "Compile blog")]
    public class CompileOptions
    {
      [Option('s', "source", Required = true, HelpText = "Source directory")]
      public string SourceDir { get; set; }

      [Option('o', "output", Required = true, HelpText = "Output directory")]
      public string OutputDir { get; set; }

      [Option('j', "json", Required = true, HelpText="Specify json file")]
      public string JsonFile { get; set; }
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