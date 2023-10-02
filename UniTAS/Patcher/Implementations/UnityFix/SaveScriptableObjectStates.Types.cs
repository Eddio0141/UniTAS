using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using UniTAS.Patcher.Models.Utils;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.Logging;
using UniTAS.Patcher.Utils;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UniTAS.Patcher.Implementations.UnityFix;

public partial class SaveScriptableObjectStates
{
    private readonly struct StoredState
    {
        public readonly ScriptableObject ScriptableObject;

        private readonly FieldData[] _savedFields;

        private readonly ILogger _logger;

        public StoredState(ScriptableObject scriptableObject, ILogger logger, ITryFreeMalloc freeMalloc)
        {
            ScriptableObject = scriptableObject;
            _logger = logger;

            // save
            // TODO limit fields that are serializable to save space
            var fields = AccessTools.GetDeclaredFields(scriptableObject.GetType())
                .Where(x => !x.IsStatic && !x.IsLiteral);
            _savedFields = fields.Select(x => new FieldData(x, scriptableObject, logger, freeMalloc)).ToArray();
        }

        public void Load()
        {
            if (ScriptableObject == null)
            {
                _logger.LogError("ScriptableObject is null, this should not happen");
                return;
            }

            foreach (var savedField in _savedFields)
            {
                savedField.Load();
            }
        }
    }

    private readonly struct FieldData
    {
        private readonly ScriptableObject _instance;
        private readonly Either<Object, object> _value;
        private readonly FieldInfo _saveField;

        private readonly ITryFreeMalloc _freeMalloc;

        public FieldData(FieldInfo fieldInfo, ScriptableObject instance, ILogger logger, ITryFreeMalloc freeMalloc)
        {
            _saveField = fieldInfo;
            _instance = instance;

            var value = fieldInfo.GetValue(instance);

            if (fieldInfo.FieldType.IsPointer)
            {
                _freeMalloc = freeMalloc;
            }

            if (value is Object unityObject)
            {
                _value = unityObject;
            }
            else
            {
                try
                {
                    _value = DeepCopy.MakeDeepCopy(value);
                }
                catch (Exception e)
                {
                    logger.LogError($"Failed to deep copy field value, type is {fieldInfo.FieldType.FullName}, {e}");
                }
            }
        }

        public void Load()
        {
            // try to free pointer if it's a pointer
            if (_freeMalloc != null)
            {
                var prevValue = _saveField.GetValue(_instance);
                unsafe
                {
                    var prevValueRaw = (IntPtr)Pointer.Unbox(prevValue);
                    _freeMalloc.TryFree(prevValueRaw);
                }
            }

            _saveField.SetValue(_instance, _value.IsLeft ? _value.Left : _value.Right);
        }
    }
}