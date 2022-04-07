using System.Xml.Linq;
using System.Collections.Generic;
using System.Linq;

namespace NOAReact.ElectricitySensorDecoder.Library;

/// <summary>
/// Represents a Raw (ie. unparsed) XDATA packet.
/// Contains the timestamp and all data bytes.
/// </summary>
public class RawXDataPacket
{
    /// <summary>
    /// Timestamp of packet retrieval by the ground station. Referenced to start of sounding.
    /// </summary>
    public double FrameTime { get; private set; }

    /// <summary>
    /// Array of data bytes from the XDATA packet
    /// </summary>
    public byte[] Bytes { get; private set; }

    public RawXDataPacket(double frametime, byte[] bytes)
    {
        FrameTime = frametime;
        Bytes = bytes;
    }
}

/// <summary>
/// 
/// </summary>
public class GSFFileError : Exception
{
    private string Path { get; }

    public GSFFileError(string path, string message) : base($"Could not parse GSF File! ({message})")
    {
        Path = path;
    }

}

/// <summary>
/// Parser for GSF (XML) files
/// </summary>
public class GSFParser
{
    /// <summary>
    /// Path of the GSF file we are processing
    /// </summary>
    private readonly string path;

    private readonly XElement xml = default!;

    public GSFParser(string path)
    {
        this.path = path;
        xml = XElement.Load(path);
    }

    /// <summary>
    /// Get all XDATA packets from the GSF file
    /// </summary>
    /// <returns></returns>
    public IEnumerable<RawXDataPacket> GetXData()
    {
        /*        var xdata = new List<RawXDataPacket>();
                var nodes = xml.Descendants("XdataOutputString");
                foreach (var node in nodes)
                {
                    var timestamp = Convert.ToDouble(
                        node.Descendants("Time").First().Value
                    );
                    var dataNode = node.Descendants("XdataValue").First();
                    var bytes = dataNode.Attributes().Select(x => Convert.ToByte(x.Value)).ToArray();

                    xdata.Add(new RawXDataPacket(timestamp, bytes));
                }*/

        var xdata = new List<RawXDataPacket>();
        var frames = xml.Element("SENSORDATA")?.Descendants("Frame");
        if (frames == null)
        {
            throw new GSFFileError(path, "SENSORDATA or Frame nodes not found");
        }

        foreach (var frame in frames)
        {
            var xdataNode = frame.Descendants("XDATA").FirstOrDefault();
            if (xdataNode == null)
            {
                // This frame has no XDATA
                continue;
            }

            var bytes = xdataNode.Attributes().Select(x => Convert.ToByte(x.Value)).ToArray();
            var frametime = Convert.ToDouble(frame.Descendants("FrameTime").First().Value);

            xdata.Add(new RawXDataPacket(frametime, bytes));
        }

        // If this GSF file links to other files, open them recursively and read data
        var nextFileNode = xml.Element("NextFile")?.Element("FileName");
        if (nextFileNode != null)
        {
            // Determine full path of next file
            var nextFileName = nextFileNode.Value;
            var parent = Directory.GetParent(path);
            var nextFilePath = $"{parent!.FullName}\\{nextFileName}";

            var nextFileParser = new GSFParser(nextFilePath);
            xdata.AddRange(nextFileParser.GetXData());
        }

        return xdata;
    }

    public DateTime GetStartTime()
    {
        var node = xml.Element("Header")?.Element("RecordStartDate");
        if (node == null)
        {
            throw new GSFFileError(this.path, "Could not locate Header/RecordStartDate node. Is this the first file (ie. not gsf1, gsf2, ...)?");
        }

        return DateTime.Parse(node.Value);
    }
}
