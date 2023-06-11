using System;

namespace UniTAS.Patcher.Interfaces.Serialization;

public abstract class DataTypeSerializer
{
    protected abstract Type SerializeType { get; }

    public bool Serialize(object data, out object serializedData)
    {
        if (data.GetType() != SerializeType)
        {
            serializedData = null;
            return false;
        }

        serializedData = Serialize(data);
        return true;
    }

    protected abstract object Serialize(object data);
}

public abstract class DataTypeSerializer<T> : DataTypeSerializer
{
    protected override Type SerializeType => typeof(T);
}