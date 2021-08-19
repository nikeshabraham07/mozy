using System;

namespace processor
{
    using System.IO;

    using CommandLine;

    using leacher.Services;

    class Program
    {
        public class Options
        {
            [Option('i', "input", Required = true, HelpText = "input file name")]
            public string InputFileName { get; set; }

            [Option('o', "output", Required = false, HelpText = "Set output folder.")]
            public string OutputFolder { get; set; }
        }

        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(RunAndReturnExitCodeAsync); // options is an instance of Options type

        }

        private static void RunAndReturnExitCodeAsync(Options options)
        {
            if (!File.Exists(options.InputFileName))
            {
                throw new ArgumentException($"File {options.InputFileName} does not exist");
            }

            string outputFolder = string.IsNullOrEmpty(options.OutputFolder)
                                      ? Directory.GetCurrentDirectory()
                                      : options.OutputFolder;

            if (!Directory.Exists(outputFolder))
            {
                Directory.CreateDirectory(outputFolder);
            }

            ChunkProcessor processor = new ChunkProcessor();
            processor.ChunkFile(options.InputFileName, outputFolder);
        }
    }
}
