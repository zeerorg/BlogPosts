using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using McMaster.Extensions.CommandLineUtils;

namespace MarkdownCheck
{
    class Program
    {
        static void Main(string[] args) => CommandLineApplication.Execute<CommandLine>(args);
    }
}
