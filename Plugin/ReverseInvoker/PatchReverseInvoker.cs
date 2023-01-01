using System;
using System.IO;
using System.Reflection;
using HarmonyLib;

namespace UniTASPlugin.ReverseInvoker;

// ReSharper disable once ClassNeverInstantiated.Global
public class PatchReverseInvoker
{
    private readonly char[] _invalidPathChars;
    private readonly char _altDirectorySeparatorChar;
    private readonly char _directorySeparatorChar;
    private readonly char _pathSeparator;
    private readonly string _directorySeparatorStr;
    private readonly char _volumeSeparatorChar;
    private readonly char[] _pathSeparatorChars;
    private readonly bool _dirEqualsVolume;

    private char[] _currentInvalidPathChars;
    private char _currentAltDirectorySeparatorChar;
    private char _currentDirectorySeparatorChar;
    private char _currentPathSeparator;
    private string _currentDirectorySeparatorStr;
    private char _currentVolumeSeparatorChar;
    private char[] _currentPathSeparatorChars;
    private bool _currentDirEqualsVolume;

    private readonly FieldInfo _invalidPathCharsField;
    private readonly FieldInfo _altDirectorySeparatorCharField;
    private readonly FieldInfo _directorySeparatorCharField;
    private readonly FieldInfo _pathSeparatorField;
    private readonly FieldInfo _directorySeparatorStrField;
    private readonly FieldInfo _volumeSeparatorCharField;
    private readonly FieldInfo _pathSeparatorCharsField;
    private readonly FieldInfo _dirEqualsVolumeField;

    public PatchReverseInvoker()
    {
#pragma warning disable CS0618
        _invalidPathCharsField = AccessTools.Field(typeof(Path), nameof(Path.InvalidPathChars));
#pragma warning restore CS0618
        _altDirectorySeparatorCharField = AccessTools.Field(typeof(Path), nameof(Path.AltDirectorySeparatorChar));
        _directorySeparatorCharField = AccessTools.Field(typeof(Path), nameof(Path.DirectorySeparatorChar));
        _pathSeparatorField = AccessTools.Field(typeof(Path), nameof(Path.PathSeparator));
        _directorySeparatorStrField = AccessTools.Field(typeof(Path), "DirectorySeparatorStr");
        _volumeSeparatorCharField = AccessTools.Field(typeof(Path), nameof(Path.VolumeSeparatorChar));
        _pathSeparatorCharsField = AccessTools.Field(typeof(Path), "PathSeparatorChars");
        _dirEqualsVolumeField = AccessTools.Field(typeof(Path), "dirEqualsVolume");

#pragma warning disable CS0618
        _invalidPathChars = Path.InvalidPathChars;
#pragma warning restore CS0618
        _altDirectorySeparatorChar = Path.AltDirectorySeparatorChar;
        _directorySeparatorChar = Path.DirectorySeparatorChar;
        _pathSeparator = Path.PathSeparator;
        _directorySeparatorStr = (string)_directorySeparatorStrField.GetValue(null);
        _volumeSeparatorChar = Path.VolumeSeparatorChar;
        _pathSeparatorChars = (char[])_pathSeparatorCharsField.GetValue(null);
        _dirEqualsVolume = (bool)_dirEqualsVolumeField.GetValue(null);
    }

    private bool _invoking;

    public bool Invoking
    {
        get => _invoking;
        set
        {
            if (value)
            {
#pragma warning disable CS0618
                _currentInvalidPathChars = Path.InvalidPathChars;
#pragma warning restore CS0618
                _currentAltDirectorySeparatorChar = Path.AltDirectorySeparatorChar;
                _currentDirectorySeparatorChar = Path.DirectorySeparatorChar;
                _currentPathSeparator = Path.PathSeparator;
                _currentDirectorySeparatorStr = (string)_directorySeparatorStrField.GetValue(null);
                _currentVolumeSeparatorChar = Path.VolumeSeparatorChar;
                _currentPathSeparatorChars = (char[])_pathSeparatorCharsField.GetValue(null);
                _currentDirEqualsVolume = (bool)AccessTools.Field(typeof(Path), "dirEqualsVolume").GetValue(null);

                _invalidPathCharsField.SetValue(null, _invalidPathChars);
                _altDirectorySeparatorCharField.SetValue(null, _altDirectorySeparatorChar);
                _directorySeparatorCharField.SetValue(null, _directorySeparatorChar);
                _pathSeparatorField.SetValue(null, _pathSeparator);
                _directorySeparatorStrField.SetValue(null, _directorySeparatorStr);
                _volumeSeparatorCharField.SetValue(null, _volumeSeparatorChar);
                _pathSeparatorCharsField.SetValue(null, _pathSeparatorChars);
                _dirEqualsVolumeField.SetValue(null, _dirEqualsVolume);
            }
            else
            {
                _invalidPathCharsField.SetValue(null, _currentInvalidPathChars);
                _altDirectorySeparatorCharField.SetValue(null, _currentAltDirectorySeparatorChar);
                _directorySeparatorCharField.SetValue(null, _currentDirectorySeparatorChar);
                _pathSeparatorField.SetValue(null, _currentPathSeparator);
                _directorySeparatorStrField.SetValue(null, _currentDirectorySeparatorStr);
                _volumeSeparatorCharField.SetValue(null, _currentVolumeSeparatorChar);
                _pathSeparatorCharsField.SetValue(null, _currentPathSeparatorChars);
                _dirEqualsVolumeField.SetValue(null, _currentDirEqualsVolume);
            }

            _invoking = value;
        }
    }

    public TRet Invoke<TRet>(Func<TRet> method)
    {
        Invoking = true;
        var ret = method.Invoke();
        Invoking = false;
        return ret;
    }

    public TRet Invoke<TRet, T>(Func<T, TRet> method, T arg1)
    {
        Invoking = true;
        var ret = method.Invoke(arg1);
        Invoking = false;
        return ret;
    }

    public TRet Invoke<TRet, T1, T2>(Func<T1, T2, TRet> method, T1 arg1, T2 arg2)
    {
        Invoking = true;
        var ret = method.Invoke(arg1, arg2);
        Invoking = false;
        return ret;
    }

    public T GetProperty<T>(Func<T> property)
    {
        Invoking = true;
        var ret = property.Invoke();
        Invoking = false;
        return ret;
    }
}