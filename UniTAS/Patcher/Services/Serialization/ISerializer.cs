using System;
using System.Collections.Generic;
using UniTAS.Patcher.Models.Serialization;

namespace UniTAS.Patcher.Services.Serialization;

public interface ISerializer
{
    IEnumerable<SerializedData> SerializeStaticFields(Type targetClass, List<(object, SerializedData)> references);
}