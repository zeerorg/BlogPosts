using System;
using System.IO;
using CommandLine;

namespace MarkdownCheck
{
    class Program
    {
        static int Main(string[] args)
        {
            return CommandLine.Parser.Default.ParseArguments<Commands.DevelopmentOptions, Commands.UploadOptions, Commands.CompileOptions>(args)
                .MapResult(
                    (Commands.DevelopmentOptions opts) => RunDevelopment(opts),
                    (Commands.UploadOptions opts) => RunUpload(opts),
                    (Commands.CompileOptions opts) => RunCompile(opts.SourceDir, opts.OutputDir, opts.JsonFile),
                    errs => 1
                );
        }

        static int RunDevelopment(Commands.DevelopmentOptions opts)
        {
            RunCompile(opts.SourceDir, opts.OutputDir, opts.JsonFile);
            new StaticFileServer(opts.FileServerPath).Start();

            return 0;
        }

        static int RunUpload(Commands.UploadOptions opts)
        {
            RunCompile(opts.SourceDir, opts.OutputDir, opts.JsonFile);
            new AzureBlobUpload(opts.OutputDir, opts.JsonFile, Path.Combine(opts.OutputDir, "rss.xml"), opts.PhotosDir, opts.SeriesJsonFile).Upload();

            return 0;
        }

        static int RunCompile(string SourceDir, string OutputDir, string JsonFile)
        {
            if (!Commands.IsDirectory(SourceDir))
            {
                Console.Error.WriteLine("Wrong source directory");
                Console.WriteLine($"SourceDir: {SourceDir}");
                return 1;
            }

            if (!new DirectoryInfo(OutputDir).Exists)
            {
                new DirectoryInfo(OutputDir).Create();
            }

            Console.WriteLine($"source directory is: {new DirectoryInfo(SourceDir).FullName}");
            Console.WriteLine($"output directory is: {new DirectoryInfo(OutputDir).FullName}");

            Action postChange = () => CreateRSS.DefaultCreate(OutputDir, JsonFile, Path.Combine(OutputDir, "rss.xml"));
            new Compiler(SourceDir, OutputDir, postChange).Start();
            return 0;
        }
    }
}
