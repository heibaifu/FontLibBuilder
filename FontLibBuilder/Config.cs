﻿using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FontLibBuilder
{
    class Options
    {
        [OptionList('i', "input")]
        public IEnumerable<string> InputFiles { get; set; }

        [OptionList('d', "directories")]
        public IEnumerable<string> InputDirectories { get; set; }

        [OptionList('e', "extensions")]
        public IEnumerable<string> FileExtensions { get; set; }

        [Option('r', "recursive")]
        public bool Recursive { get; set; }

        [Option('t', "template", Required = true)]
        public string Template { get; set; }

        [Option('o', "output", Required = true)]
        public string Output { get; set; }

        [Option('s', "fontsize", Required = true)]
        public int FontSize { get; set; }

        [Option('c', "encoding", Required = false, DefaultValue = "UTF8")]
        public string Encoding { get; set; }

        [Option('f', "Font", Required = false)]
        public string Font { get; set; }

        [Option('v', "verbose", Required = false)]
        public bool Verbose { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            return FontLibBuilder.Properties.Resources.Usage;
        }
    }
}