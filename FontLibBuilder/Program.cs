using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using System.IO;
using System.Text.RegularExpressions;

namespace FontLibBuilder
{
    class Program
    {
        static void Main(string[] args)
        {
            var options = new Options();
            bool success = Parser.Default.ParseArguments(args, options);

            if (options.Verbose)
            {
                Log.Level = LogType.Debug;
                Log.ShowTime = true;
            }
            else
            {
                Log.Level = LogType.Success;
                Log.ShowTime = false;
            }

            if (options.InputFiles == null)
                options.InputFiles = new string[0];
            if (options.InputDirectories == null)
                options.InputDirectories = new string[0];
            if (options.FileExtensions == null)
                options.FileExtensions = new string[0];

            options.FileExtensions = options.FileExtensions.Select(ext => ext.ToLowerInvariant()).ToArray();
            var files = options.InputDirectories
                .Select(dirString => new DirectoryInfo(dirString))
                .SelectMany(dir => dir.EnumerateFiles("*", SearchOption.AllDirectories)
                    .Where(file => options.FileExtensions.Contains(file.Extension.ToLowerInvariant())))
                .Concat(options.InputFiles.Select(name => new FileInfo(name)));

            // var chars = new List<char>();
            var chars = new HashSet<char>();

            foreach (var file in files)
            {
                Log.I($"正在处理文件 {file.Name}");
                string content;
                using (StreamReader reader = new StreamReader(file.OpenRead(), Encoding.GetEncoding(options.Encoding), true))
                {
                    content = reader.ReadToEnd();
                    chars.UnionWith(Utils.SearchForStrings(content).Select(str => Utils.EscapeString(str)).SelectMany(str => str.ToCharArray()));
                }
            }



        }

       
    }
}
