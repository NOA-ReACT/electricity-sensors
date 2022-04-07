using System.CommandLine;
using NOAReact.ElectricitySensorDecoder.Library;

class BatchTool
{
    static int Main(string[] args)
    {
        var rootCommand = new RootCommand();

        // Convert verb (GSF -> CSV)
        var inputFileArgument = new Argument<string>("file", "File to convert (xdata.gsf)");
        var outputFileArgument = new Argument<string>("output", "Where to write CSV output");
        var printOption = new Option<bool>("--print", "Whether to print all data to the terminal");
        var convertVerb = new Command("convert") { inputFileArgument, outputFileArgument, printOption };
        
        convertVerb.SetHandler((string inputPath, string outputPath, bool printData) => {
            ConvertGSFFile(inputPath, outputPath, printData);
        }, inputFileArgument, outputFileArgument, printOption);

        rootCommand.Add(convertVerb);

        // Watch verb (continously convert GSF)
        var watchVerb = new Command("watch") { inputFileArgument, outputFileArgument, printOption };
        watchVerb.SetHandler((string inputPath, string outputPath, bool printData) => {
            WatchGSFFile(inputPath, outputPath, printData);
        }, inputFileArgument, outputFileArgument, printOption);

        rootCommand.Add(watchVerb);
        
        return rootCommand.Invoke(args);
    }

    /// <summary>
    /// Converts a GSF file set to CSV
    /// </summary>
    /// <param name="inputPath">Path to the first GSF file (.gsf) of the set</param>
    /// <param name="outputPath">Where to store the output CSV file</param>
    /// <param name="printData">If true, the data will also be printed to the terminal</param>
    static void ConvertGSFFile(string inputPath, string outputPath, bool printData)
    {
        // Open input file
        var parser = new GSFParser(inputPath);
        var startTime = parser.GetStartTime();

        // Determine output path
        if (Directory.Exists(outputPath))
        {
            var formattedTime = startTime.ToString("yyyy-MM-dd_HH-mm-ss");
            outputPath = Path.Combine(outputPath, $"{formattedTime}.csv");
        }

        // Open output file and start writing packets
        var outputFile = new CSVFile(outputPath);
        outputFile.WriteHeader();

        var packets = parser.GetXData();
        foreach(var packet in packets)
        {
            var xdata = new XDataMessage(startTime, packet);
            if (printData)
            {
                Console.WriteLine(xdata.ToString());
            }
            outputFile.WriteXDataMessage(xdata);
        }

        outputFile.Dispose();
    }

    /// <summary>
    /// Watches a GSF file and continiously converts it to CSV
    /// </summary>
    /// <param name="inputPath">Path to the first GSF file (.gsf) of the set</param>
    /// <param name="outputPath">Where to store the output CSV file</param>
    /// <param name="printData">If true, the data will also be printed to the terminal</param>
    static void WatchGSFFile(string inputPath, string outputPath, bool printData)
    {
        var inputParent = Directory.GetParent(inputPath);
        if (inputParent == null || !inputParent.Exists)
        {
            throw new Exception("Input file's parent directory not found");
        }
        var filter = Path.GetFileNameWithoutExtension(inputPath) + "*";

        var watcher = new FileSystemWatcher(inputParent.FullName);
        watcher.Filter = filter;
        watcher.Changed += (sender, e) => ConvertGSFFile(inputPath, outputPath, printData);
        watcher.Created += (sender, e) => ConvertGSFFile(inputPath, outputPath, printData);
        watcher.EnableRaisingEvents = true;

        Console.WriteLine($"Watching {filter} for new data. Press any key to stop.");
        Console.ReadLine();
    }
}