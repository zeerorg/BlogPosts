using System;
using System.IO;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;

namespace MarkdownCheck
{
  class StaticFileServer
  {
    private string FilePath;

    public StaticFileServer(string filePath)
    {
      FilePath = filePath;
    }
    public void Start()
    {
      WebHost
          .CreateDefaultBuilder()
          .UseUrls("http://localhost:8081/")
          .ConfigureServices(services => {
            services.AddCors();
          })
          .Configure(app => 
          {
            app.UseCors(builder => builder.WithOrigins("*"));
            app.UseStaticFiles(new StaticFileOptions
            {
              FileProvider = new PhysicalFileProvider(
                new DirectoryInfo(FilePath).FullName
              )
            });
          })
          .Build()
          .Run();
    }
  }
}