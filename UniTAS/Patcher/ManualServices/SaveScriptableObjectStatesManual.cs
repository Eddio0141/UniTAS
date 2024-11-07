using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using UniTAS.Patcher.Utils;
using UnityEngine;

namespace UniTAS.Patcher.ManualServices;

public static class SaveScriptableObjectStatesManual
{
    private static readonly Dictionary<ScriptableObject, StoredState> StoredStates = [];

    public static void LoadAll()
    {
        StaticLogger.LogDebug("Loading all ScriptableObject states");

        foreach (var storedState in StoredStates)
        {
            storedState.Value.Load();
        }
    }

    public static void Save(ScriptableObject obj)
    {
        if (obj == null) return;
        if (StoredStates.ContainsKey(obj)) return;

        StaticLogger.LogDebug($"Saving object {obj.name}");
        StoredStates.Add(obj, new(obj));
    }

    private readonly struct StoredState
    {
        private readonly ScriptableObject _scriptableObject;

        private readonly FieldData[] _savedFields;

        public StoredState(ScriptableObject scriptableObject)
        {
            _scriptableObject = scriptableObject;

            // save
            var fields = scriptableObject.GetType().GetFields(AccessTools.all).Where(x => !x.IsStatic && !x.IsLiteral);

            _savedFields = fields.Select(field => new FieldData(field, scriptableObject)).ToArray();
        }

        public void Load()
        {
            if (_scriptableObject == null)
            {
                StaticLogger.LogError("ScriptableObject is null, this should not happen");
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
        private readonly object _value;
        private readonly FieldInfo _saveField;
        private readonly bool _pointerField;

        public FieldData(FieldInfo fieldInfo, ScriptableObject instance)
        {
            _saveField = fieldInfo;
            _instance = instance;

            var value = fieldInfo.GetValue(instance);

            _pointerField = fieldInfo.FieldType.IsPointer;

            try
            {
                _value = DeepCopy.MakeDeepCopy(value, processor: ProcessScriptableObjectCopy);
            }
            catch (Exception e)
            {
                StaticLogger.LogError($"Failed to deep copy field value, type is {fieldInfo.FieldType.FullName}, {e}");
            }
        }

        public void Load()
        {
            if (_pointerField)
            {
                // try to free pointer if it's a pointer
                TryFreeMallocManual.TryFree(_instance, _saveField);
            }

            // additional one to make it not use the stored value
            var value = DeepCopy.MakeDeepCopy(_value, processor: ProcessScriptableObjectCopy);
            _saveField.SetValue(_instance, value);
        }

        private static bool ProcessScriptableObjectCopy(string path, object source, out object copiedObj)
        {
            if (source is not ScriptableObject)
            {
                copiedObj = null;
                return false;
            }

            copiedObj = source;
            return true;
        }
    }
}