using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NOAReact.ElectricitySensorDecoder.Library;

internal class CSVFile
{
    public string Path { get; }

    private readonly StreamWriter file;

    public CSVFile(string path)
    {
        Path = path;
        file = new StreamWriter(Path);
    }

    public void WriteHeader()
    {
        file.WriteLine("timestamp,frametime,is_valid,sensor_type,data");
    }

    public void WriteXDataMessage(XDataMessage xdata)
    {
        var lineSB = new StringBuilder();

        lineSB.Append(xdata.Timestamp.ToString("yyyy-MM-dd HH:mm:ss"));
        lineSB.Append(',');

        lineSB.Append(xdata.FrameTime);
        lineSB.Append(',');

        lineSB.Append(xdata.IsValid);
        lineSB.Append(',');

        lineSB.Append(xdata.SensorType);
        lineSB.Append(',');

        foreach (KeyValuePair<string, object> kv in xdata.Variables)
        {
            lineSB.Append(kv.Key);
            lineSB.Append('=');
            lineSB.Append(kv.Value);
            lineSB.Append(' ');
        }

        file.WriteLine(lineSB.ToString());
    }

    public void Dispose()
    {
        file?.Dispose();
    }
}

