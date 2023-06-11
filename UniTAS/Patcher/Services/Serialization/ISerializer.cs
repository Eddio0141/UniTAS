using System.Collections.Generic;
using UniTAS.Patcher.Models.Serialization;
using UniTAS.Patcher.Utils;

namespace UniTAS.Patcher.Services.Serialization;

public interface ISerializer
{
    TupleValue<IEnumerable<SerializedData>, IEnumerable<SerializedReferenceType>> SerializeStaticFields(
        string targetClass);
}