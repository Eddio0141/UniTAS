using System;
using System.Collections.Generic;
using UniTAS.Patcher.Models.Serialization;
using UniTAS.Patcher.Utils;

namespace UniTAS.Patcher.Services.Serialization;

public interface ISerializer
{
    IEnumerable<SerializedData> SerializeStaticFields(Type targetClass,
        List<TupleValue<object, SerializedData>> references);
}