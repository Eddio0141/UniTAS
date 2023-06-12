using System;
using System.Collections.Generic;
using UniTAS.Patcher.Models.Serialization;
using UniTAS.Patcher.Utils;

namespace UniTAS.Patcher.Services.Serialization;

public interface ISerializer
{
    /// <returns>Serialized data and reference data</returns>
    TupleValue<IEnumerable<SerializedData>, IEnumerable<SerializedData>> SerializeStaticFields(Type targetClass);
}