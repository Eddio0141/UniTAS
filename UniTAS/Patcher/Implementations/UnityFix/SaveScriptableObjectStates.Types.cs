using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using UniTAS.Patcher.Extensions;
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
        private readonly FieldInfo[] _resetFields;

        private readonly ILogger _logger;
        private readonly ITryFreeMalloc _freeMalloc;

        public StoredState(ScriptableObject scriptableObject, ILogger logger, ITryFreeMalloc freeMalloc)
        {
            ScriptableObject = scriptableObject;
            _logger = logger;
            _freeMalloc = freeMalloc;

            // save
            var fields = scriptableObject.GetType().GetFieldsRecursive(AccessTools.all)
                .Where(x => !x.IsStatic && !x.IsLiteral && x.DeclaringType != typeof(Object));

            var resetFields = new List<FieldInfo>();
            var savedFields = new List<FieldData>();

            foreach (var field in fields)
            {
                if (field.IsFieldUnitySerializable())
                {
                    savedFields.Add(new(field, scriptableObject, logger, freeMalloc));
                    continue;
                }

                StaticLogger.Trace("Field is not unity serializable, resetting on load");
                resetFields.Add(field);
            }

            _savedFields = savedFields.ToArray();
            _resetFields = resetFields.ToArray();
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

            foreach (var resetField in _resetFields)
            {
                _freeMalloc?.TryFree(ScriptableObject, resetField);
                resetField.SetValue(ScriptableObject, null);
            }
        }
    }

    private readonly struct FieldData
    {
        private readonly ScriptableObject _instance;
        private readonly object _value;
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

            try
            {
                _value = DeepCopy.MakeDeepCopy(value);
            }
            catch (Exception e)
            {
                logger.LogError($"Failed to deep copy field value, type is {fieldInfo.FieldType.FullName}, {e}");
            }
        }

        public void Load()
        {
            // try to free pointer if it's a pointer
            _freeMalloc?.TryFree(_instance, _saveField);

            // additional one to make it not use the stored value
            var value = DeepCopy.MakeDeepCopy(_value);
            _saveField.SetValue(_instance, value);
        }
    }
}