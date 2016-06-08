using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FontLibBuilder
{
    class Options
    {
        [Option('i', "input")]
        public string[] InputFiles { get; set; }

        [Option('d', "directories")]
        public string[] InputDirectories { get; set; }

        [Option('e', "extensions")]
        public string[] FileExtensions { get; set; }

        [Option('r', "recursive")]
        public bool Recursive { get; set; }

        [Option('t', "template", Required = true)]
        public string Template { get; set; }

        [Option('o', "output", Required = true)]
        public string Output { get; set; }

        [Option('s', "fontsize", Required = true)]
        public int FontSize { get; set; }

        [Option('e', "encoding", Required = false)]
        public bool Encoding { get; set; }

        [Option('f', "Font", Required = false)]
        public string Font { get; set; }

        [Option('v', "verbose", Required = false)]
        public bool Verbose { get; set; }

    }
}