namespace UniTAS.Patcher.Models.Serialization;

public class SerializedReferenceType
{
    public uint ReferenceId { get; }
    public SerializedData Data { get; }

    public SerializedReferenceType(uint referenceId, SerializedData data)
    {
        ReferenceId = referenceId;
        Data = data;
    }
}