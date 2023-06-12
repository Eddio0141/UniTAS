using System;
using System.Collections.Generic;
using UniTAS.Patcher.Models.Serialization;

namespace UniTAS.Patcher.Services.Serialization;

public interface ISerializer
{
    /// <returns>Serialized data and reference data</returns>
    IEnumerable<SerializedData> SerializeStaticFields(Type targetClass);
}