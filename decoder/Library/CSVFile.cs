using System.Text;

namespace NOAReact.ElectricitySensorDecoder.Library;

/// <summary>
/// Used to write XDataMessages to a CSV file
/// </summary>
public class CSVFile
{
    public string Path { get; }

    private readonly StreamWriter file;

    private readonly string[] dataColumns = { "space_counts", "space_state", "mill_valid", "mill_counts", "mill_roll", "mill_pitch", "mill_yaw" };

    public CSVFile(string path)
    {
        Path = path;
        file = new StreamWriter(Path);
    }

    /// <summary>
    /// Write the CSV header (column names)
    /// </summary>
    public void WriteHeader()
    {
        file.WriteLine($"timestamp,frametime,is_valid,{string.Join(',', dataColumns)}");
    }

    /// <summary>
    /// Write a XDataMessage to the CSV file as a new row
    /// </summary>
    /// <param name="xdata">The message to write</param>
    public void WriteXDataMessage(XDataMessage xdata)
    {
        var lineSB = new StringBuilder();

        lineSB.Append(xdata.Timestamp.ToString("yyyy-MM-dd HH:mm:ss"));
        lineSB.Append(',');

        lineSB.Append(xdata.FrameTime);
        lineSB.Append(',');

        lineSB.Append(xdata.IsValid);

        foreach (string key in dataColumns)
        {
            lineSB.Append(',');
            if (xdata.Variables.ContainsKey(key))
            {
                lineSB.Append(xdata.Variables[key]);
            }

        }

        file.WriteLine(lineSB.ToString());
    }

    /// <summary>
    /// Closes the output file
    /// </summary>
    public void Dispose()
    {
        file?.Dispose();
    }
}

