using System.Text.Json;

namespace NOAReact.ElectricitySensorDecoder.Library;

public enum ElectritySensorType : byte
{
    FIELD_MILL = 0xAA,
    SPACE_CHARGE = 0xAB,
    UNKNOWN = 0x00
}


public class XDataMessage
{
    public double FrameTime { get; }

    public DateTime Timestamp { get; private set; }

    public byte[] RawData { get; }

    public bool IsValid { get; private set; } = false;

    public string? Error { get; private set; }

    public ElectritySensorType SensorType { get; private set; }

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
        if (!Enum.IsDefined(typeof(ElectritySensorType), sensorTypeId))
        {
            SensorType = ElectritySensorType.UNKNOWN;
            Error = $"Unknown sensor ID 0x{sensorTypeId.ToString("X")}";
            return;
        }
        SensorType = (ElectritySensorType)sensorTypeId;

        // Parse numbers from RawData into Variables
        if (SensorType == ElectritySensorType.FIELD_MILL)
        {
            byte[] counts = { RawData[2], RawData[3] };
            Variables["counts"] = BitConverter.ToInt16(counts);

            byte[] roll = { RawData[4], RawData[5], RawData[6], RawData[7] };
            Variables["roll"] = BitConverter.ToSingle(roll);

            byte[] pitch = { RawData[8], RawData[9], RawData[10], RawData[11] };
            Variables["pitch"] = BitConverter.ToSingle(pitch);

            byte[] yaw = { RawData[12], RawData[13], RawData[14], RawData[15] };
            Variables["yaw"] = BitConverter.ToSingle(yaw);
        } else if (SensorType == ElectritySensorType.SPACE_CHARGE)
        {
            byte[] counts = { RawData[2], RawData[3] };
            Variables["counts"] = BitConverter.ToInt16(counts);

            //byte[] state = {  };
            Variables["state"] = RawData[4];
        }

        IsValid = true;
    }

    public override string ToString()
    {
        var valid = IsValid ? "Valid" : $"Invalid - {Error}";
        var data = JsonSerializer.Serialize(Variables);
        return $"XDATA ({valid}): Timestamp: {Timestamp}, SensorType: {SensorType}, Data: {data}";
    }
}
