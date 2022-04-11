using System.Text.Json;

namespace NOAReact.ElectricitySensorDecoder.Library;

public class XDataMessage
{
    public double FrameTime { get; }

    public DateTime Timestamp { get; private set; }

    public byte[] RawData { get; }

    public bool IsValid { get; private set; } = false;

    public string? Error { get; private set; }

    public Dictionary<string, object> Variables { get; } = new Dictionary<string, object>();

    public XDataMessage(DateTime startTime, double frametime, byte[] rawData)
    {
        FrameTime = frametime;
        Timestamp = startTime.AddMilliseconds(frametime);
        RawData = rawData;

        ParseData();
    }

    public XDataMessage(DateTime startTime, RawXDataPacket packet)
    {
        FrameTime = packet.FrameTime;
        Timestamp = startTime.AddMilliseconds(packet.FrameTime);
        RawData = packet.Bytes;

        ParseData();
    }

    private void ParseData()
    {
        // Check package ID, check if it makes sense
        var sensorId = RawData[0];
        if (sensorId != 0x01)
        {
            Error = "Sensor ID is not 0x01";
            return;
        }

        // Grab sensor ID, try to detect sensor type
        var sensorTypeId = RawData[1];
        if (sensorTypeId != 0xAB)
        {
            IsValid = false;
            Error = $"Unknown sensor ID 0x{sensorTypeId.ToString("X")}";
            return;
        }

        // Parse number from RawData into Variables
        byte[] counts = { RawData[2], RawData[3] };
        Variables["space_counts"] = BitConverter.ToInt16(counts);

        Variables["space_state"] = RawData[4];

        Variables["mill_valid"] = RawData[5];

        byte[] millCounts = { RawData[6], RawData[7] };
        Variables["mill_counts"] = BitConverter.ToInt16(millCounts);

        byte[] millRoll = { RawData[8], RawData[9] };
        Variables["mill_roll"] = BitConverter.ToInt16(millRoll);

        byte[] millPitch = { RawData[10], RawData[11] };
        Variables["mill_pitch"] = BitConverter.ToInt16(millPitch);

        byte[] millYaw = { RawData[12], RawData[13] };
        Variables["mill_yaw"] = BitConverter.ToInt16(millYaw);

        IsValid = true;
    }

    public override string ToString()
    {
        var valid = IsValid ? "Valid" : $"Invalid - {Error}";
        var data = JsonSerializer.Serialize(Variables);
        return $"XDATA ({valid}): Timestamp: {Timestamp}, Data: {data}";
    }
}
