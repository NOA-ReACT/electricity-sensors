using System.CommandLine;
using NOAReact.ElectricitySensorDecoder.Library;

class BatchTool
{
    static int Main(string[] args)
    {
        var inputFileArgument = new Argument<string>("file", "File to convert (xdata.gsf)");
        var outputFileArgument = new Argument<string>("output", "Where to write CSV output");
        var printOption = new Option<bool>("--print", "Whether to print all data to the terminal");
        var rootCommand = new RootCommand { inputFileArgument, outputFileArgument, printOption };

        rootCommand.SetHandler((string inputPath, string outputPath, bool printData) => {
            BatchTool.ConvertGSFFile(inputPath, outputPath, printData);
        }, inputFileArgument, outputFileArgument, printOption);

        return rootCommand.Invoke(args);
    }

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
        } else if (File.Exists(outputPath)) {
            throw new Exception("File already exists!");
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

}