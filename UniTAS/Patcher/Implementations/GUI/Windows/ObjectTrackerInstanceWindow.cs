using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.Events.UnityEvents;
using UniTAS.Patcher.Interfaces.GUI;
using UniTAS.Patcher.Models;
using UniTAS.Patcher.Models.GUI;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.GUI;
using UniTAS.Patcher.Services.UnitySafeWrappers.Wrappers;
using UniTAS.Patcher.Utils;
using UnityEngine;

namespace UniTAS.Patcher.Implementations.GUI.Windows;

[Register]
[ExcludeRegisterIfTesting]
public class ObjectTrackerInstanceWindow : Window
{
    private Object _instance;
    private readonly UnityObjectIdentifier _unityObjectIdentifier;

    private readonly string _trackSettingsConfigKey;
    private TrackSettings _trackSettings;
    public TrackSettings TrackingSettings => _trackSettings;

    private readonly IConfig _config;
    private readonly IToolBar _toolBar;
    private readonly ISceneWrapper _sceneWrapper;
    private readonly IWindowFactory _windowFactory;

    public ObjectTrackerInstanceWindow(WindowDependencies windowDependencies,
        UnityObjectIdentifier identifier,
        IOnSceneLoadEvent onSceneLoadEvent, ISceneWrapper sceneWrapper, IWindowFactory windowFactory) : base(
        windowDependencies,
        new WindowConfig(defaultWindowRect: GUIUtils.WindowRect(200, 200), showByDefault: true,
            removeConfigOnClose: true),
        $"ObjectTracker-{identifier}")
    {
        _config = windowDependencies.Config;
        _toolBar = windowDependencies.ToolBar;
        _unityObjectIdentifier = identifier;
        _sceneWrapper = sceneWrapper;
        _windowFactory = windowFactory;
        onSceneLoadEvent.OnSceneLoadEvent += UpdateInstance;
        windowDependencies.UpdateEvents.OnLateUpdateActual += OnLateUpdateActual;
        windowDependencies.UpdateEvents.OnFixedUpdateActual += OnFixedUpdateActual;
        Init();

        _trackSettingsConfigKey = $"ObjectTracker-Instance-trackSettings-{identifier}";
        if (!_config.TryGetBackendEntry(_trackSettingsConfigKey, out _trackSettings))
        {
            UpdateInstance();
            _trackSettings = new TrackSettings
            {
                ShowEulerRotation = true, ShowPos = true, ShowPosX = true, ShowPosY = true, ShowPosZ = true,
                ShowRot = true, ShowRotW = true, ShowRotX = true, ShowRotY = true, ShowRotZ = true,
                Name = _instance?.name ?? "", ShowName = true, ShowVel = true, ShowVelY = true, ShowHSpd = true,
                ShowEstVel = true, ShowEstHSpd = true, ShowEstVelY = true, ObjectSearch = new()
            };
            _config.WriteBackendEntry(_trackSettingsConfigKey, _trackSettings);
        }
        else
        {
            UpdateInstance();
        }

        _prevToolbarShow = _toolBar.Show;

        OnShowChange += (_, show) =>
        {
            if (show) return;
            // dispose config stuff
            _config.RemoveBackendEntry(_trackSettingsConfigKey);
        };
    }

    private new void Init()
    {
        NoWindowDuringToolBarHide = true;
        Resizable = false;
    }

    public Transform Transform { get; private set; }
    private Rigidbody _rigidbody;

    private void UpdateInstance()
    {
        var updateComponents = false;
        if (_instance == null)
        {
            _instance = _unityObjectIdentifier.FindObject(_trackSettings.ObjectSearch, _sceneWrapper);
            updateComponents = true;
        }

        if (_instance == null) return;
        var config = Config;
        config.WindowName = $"Tracking '{_instance.name}'";

        Transform = _instance switch
        {
            Transform t => t.transform,
            GameObject go => go.transform,
            Component comp => comp.transform,
            _ => null
        };

        if (updateComponents || _rigidbody == null)
        {
            _rigidbody = Transform?.GetComponent<Rigidbody>();
        }

        if (Transform != null)
        {
            _prevPos = Transform.position;
        }
    }

    private GUIStyle _labelNoWordWrap;
    private ObjectSearchConfigWindow _searchConfigWindow;

    // configuration of this
    protected override void OnGUI()
    {
        if (_prevToolbarShow != _toolBar.Show)
        {
            _prevToolbarShow = true;
            _resizeWindow = true;
            UpdateInstance();
        }

        GUILayout.BeginVertical();

        var newBool = GUILayout.Toggle(_trackSettings.ShowName, "Show tracker name");
        SaveTrackSettings(newBool, ref _trackSettings.ShowName);
        UnityEngine.GUI.enabled = _trackSettings.ShowName;

        GUILayout.BeginHorizontal();
        _labelNoWordWrap ??= new GUIStyle(UnityEngine.GUI.skin.label) { wordWrap = false };
        GUILayout.Label("Name: ", _labelNoWordWrap);
        GUILayout.Space(3);
        var newStr = GUILayout.TextField(_trackSettings.Name, 30, GUILayout.ExpandWidth(false), GUILayout.MinWidth(15));
        if (newStr != _trackSettings.Name)
            _resizeWindow = true;
        SaveTrackSettings(newStr, ref _trackSettings.Name);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        UnityEngine.GUI.enabled = true;
        if (GUILayout.Button("Object search settings") && _searchConfigWindow == null)
        {
            _searchConfigWindow = _windowFactory.Create(_trackSettings.ObjectSearch);
            _searchConfigWindow.OnSearchSettingsChanged += (_, updatedSettings) =>
            {
                if (updatedSettings != null)
                {
                    _trackSettings.ObjectSearch = updatedSettings.Value;
                    _config.WriteBackendEntry(_trackSettingsConfigKey, _trackSettings);
                }

                _searchConfigWindow = null;
            };
        }

        var hasTransform = Transform != null;
        UnityEngine.GUI.enabled = hasTransform;
        newBool = GUILayout.Toggle(_trackSettings.ShowPos, "Position");
        UnityEngine.GUI.enabled = _trackSettings.ShowPos && hasTransform;
        SaveTrackSettings(newBool, ref _trackSettings.ShowPos);
        if (_trackSettings is { ShowPos: true, ShowPosX: false, ShowPosY: false, ShowPosZ: false })
        {
            SaveTrackSettings(true, ref _trackSettings.ShowPosX);
            SaveTrackSettings(true, ref _trackSettings.ShowPosY);
            SaveTrackSettings(true, ref _trackSettings.ShowPosZ);
        }

        GUILayout.FlexibleSpace();

        GUILayout.BeginHorizontal();
        newBool = GUILayout.Toggle(_trackSettings.ShowPosX, "x");
        SaveTrackSettings(newBool, ref _trackSettings.ShowPosX);
        newBool = GUILayout.Toggle(_trackSettings.ShowPosY, "y");
        SaveTrackSettings(newBool, ref _trackSettings.ShowPosY);
        newBool = GUILayout.Toggle(_trackSettings.ShowPosZ, "z");
        SaveTrackSettings(newBool, ref _trackSettings.ShowPosZ);

        if (_trackSettings is { ShowPosX: false, ShowPosY: false, ShowPosZ: false })
        {
            SaveTrackSettings(false, ref _trackSettings.ShowPos);
        }

        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        UnityEngine.GUI.enabled = hasTransform;
        newBool = GUILayout.Toggle(_trackSettings.ShowRot, "Rotation");
        UnityEngine.GUI.enabled = _trackSettings.ShowRot && hasTransform;
        SaveTrackSettings(newBool, ref _trackSettings.ShowRot);
        GUILayout.FlexibleSpace();
        if (_trackSettings is { ShowRot: true, ShowRotX: false, ShowRotY: false, ShowRotZ: false }
            and ({ ShowEulerRotation: true } or { ShowEulerRotation: false, ShowRotW: false }))
        {
            SaveTrackSettings(true, ref _trackSettings.ShowRotX);
            SaveTrackSettings(true, ref _trackSettings.ShowRotY);
            SaveTrackSettings(true, ref _trackSettings.ShowRotZ);
            if (_trackSettings.ShowRotW)
                SaveTrackSettings(true, ref _trackSettings.ShowRotW);
        }

        GUILayout.BeginHorizontal();
        newBool = GUILayout.Toggle(_trackSettings.ShowRotX, "x");
        SaveTrackSettings(newBool, ref _trackSettings.ShowRotX);
        newBool = GUILayout.Toggle(_trackSettings.ShowRotY, "y");
        SaveTrackSettings(newBool, ref _trackSettings.ShowRotY);
        newBool = GUILayout.Toggle(_trackSettings.ShowRotZ, "z");
        SaveTrackSettings(newBool, ref _trackSettings.ShowRotZ);

        if (_trackSettings is { ShowRotX: false, ShowRotY: false, ShowRotZ: false, ShowEulerRotation: true })
        {
            SaveTrackSettings(false, ref _trackSettings.ShowRot);
        }

        GUILayout.Space(10);

        newBool = GUILayout.Toggle(_trackSettings.ShowEulerRotation, "Euler");
        SaveTrackSettings(newBool, ref _trackSettings.ShowEulerRotation);
        UnityEngine.GUI.enabled = hasTransform && _trackSettings is { ShowRot: true, ShowEulerRotation: false };
        newBool = GUILayout.Toggle(_trackSettings.ShowRotW, "w");
        SaveTrackSettings(newBool, ref _trackSettings.ShowRotW);

        if (_trackSettings is { ShowRotX: false, ShowRotY: false, ShowRotZ: false }
            and ({ ShowEulerRotation: true, ShowRotW: false } or { ShowEulerRotation: false }))
        {
            SaveTrackSettings(false, ref _trackSettings.ShowEulerRotation);
        }

        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        UnityEngine.GUI.enabled = hasTransform;
        newBool = GUILayout.Toggle(_trackSettings.ShowEstVel, "Est velocity");
        UnityEngine.GUI.enabled = _trackSettings.ShowEstVel && hasTransform;
        SaveTrackSettings(newBool, ref _trackSettings.ShowEstVel);
        if (_trackSettings is
            { ShowEstVel: true, ShowEstVelX: false, ShowEstVelY: false, ShowEstVelZ: false, ShowEstHSpd: false })
        {
            SaveTrackSettings(false, ref _trackSettings.ShowEstVelX);
            SaveTrackSettings(true, ref _trackSettings.ShowEstVelY);
            SaveTrackSettings(false, ref _trackSettings.ShowEstVelZ);
            SaveTrackSettings(true, ref _trackSettings.ShowEstHSpd);
        }

        GUILayout.FlexibleSpace();

        GUILayout.BeginHorizontal();
        newBool = GUILayout.Toggle(_trackSettings.ShowEstVelX, "x");
        SaveTrackSettings(newBool, ref _trackSettings.ShowEstVelX);
        newBool = GUILayout.Toggle(_trackSettings.ShowEstVelY, "y");
        SaveTrackSettings(newBool, ref _trackSettings.ShowEstVelY);
        newBool = GUILayout.Toggle(_trackSettings.ShowEstVelZ, "z");
        SaveTrackSettings(newBool, ref _trackSettings.ShowEstVelZ);
        newBool = GUILayout.Toggle(_trackSettings.ShowEstHSpd, "h spd");
        SaveTrackSettings(newBool, ref _trackSettings.ShowEstHSpd);

        if (_trackSettings is { ShowEstVelX: false, ShowEstVelY: false, ShowEstVelZ: false, ShowEstHSpd: false })
        {
            SaveTrackSettings(false, ref _trackSettings.ShowEstVel);
        }

        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        var hasRigidbody = _rigidbody != null;
        UnityEngine.GUI.enabled = hasRigidbody;
        newBool = GUILayout.Toggle(_trackSettings.ShowVel, "Velocity");
        UnityEngine.GUI.enabled = _trackSettings.ShowVel && hasRigidbody;
        SaveTrackSettings(newBool, ref _trackSettings.ShowVel);
        if (_trackSettings is { ShowVel: true, ShowVelX: false, ShowVelY: false, ShowVelZ: false, ShowHSpd: false })
        {
            SaveTrackSettings(false, ref _trackSettings.ShowVelX);
            SaveTrackSettings(true, ref _trackSettings.ShowVelY);
            SaveTrackSettings(false, ref _trackSettings.ShowVelZ);
            SaveTrackSettings(true, ref _trackSettings.ShowHSpd);
        }

        GUILayout.FlexibleSpace();

        GUILayout.BeginHorizontal();
        newBool = GUILayout.Toggle(_trackSettings.ShowVelX, "x");
        SaveTrackSettings(newBool, ref _trackSettings.ShowVelX);
        newBool = GUILayout.Toggle(_trackSettings.ShowVelY, "y");
        SaveTrackSettings(newBool, ref _trackSettings.ShowVelY);
        newBool = GUILayout.Toggle(_trackSettings.ShowVelZ, "z");
        SaveTrackSettings(newBool, ref _trackSettings.ShowVelZ);
        newBool = GUILayout.Toggle(_trackSettings.ShowHSpd, "h spd");
        SaveTrackSettings(newBool, ref _trackSettings.ShowHSpd);

        if (_trackSettings is { ShowVelX: false, ShowVelY: false, ShowVelZ: false, ShowHSpd: false })
        {
            SaveTrackSettings(false, ref _trackSettings.ShowVel);
        }

        UnityEngine.GUI.enabled = true;

        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.EndVertical();

        if (_resizeWindow && FitWindowSize())
            _resizeWindow = false;
    }

    private const int SpacingFromCategory = 3;
    private bool _resizeWindow = true;
    private bool _prevToolbarShow;

    private GUIStyle _valueDisplayLabel;
    private GUIStyle _categoryLabel;

    // just data
    protected override void OnGUIWhileToolbarHide()
    {
        if (_prevToolbarShow != _toolBar.Show)
        {
            _prevToolbarShow = false;
            _resizeWindow = true;
            UpdateInstance();
        }

        GUILayout.BeginVertical();

        _categoryLabel ??= new GUIStyle(UnityEngine.GUI.skin.label)
        {
            fontStyle = FontStyle.Bold
        };

        if (_trackSettings.ShowName)
        {
            GUILayoutUtils.ShadowedLabel(_trackSettings.Name, _categoryLabel);
            GUILayout.Space(SpacingFromCategory);
        }

        _valueDisplayLabel ??= new GUIStyle(UnityEngine.GUI.skin.label)
        {
            padding = new(),
            margin =
            {
                bottom = 0
            }
        };

        if (Transform != null)
        {
            if (_trackSettings.ShowPos)
            {
                GUILayoutUtils.ShadowedLabel("Position", _categoryLabel);
                GUILayout.Space(SpacingFromCategory);

                var pos = Transform.position;

                if (_trackSettings.ShowPosX)
                    GUILayoutUtils.ShadowedLabel($"x: {pos.x}", _valueDisplayLabel);

                if (_trackSettings.ShowPosY)
                    GUILayoutUtils.ShadowedLabel($"y: {pos.y}", _valueDisplayLabel);

                if (_trackSettings.ShowPosZ)
                    GUILayoutUtils.ShadowedLabel($"z: {pos.z}", _valueDisplayLabel);

                GUILayout.Space(10);
            }

            if (_trackSettings.ShowRot)
            {
                GUILayoutUtils.ShadowedLabel("Rotation", _categoryLabel);
                GUILayout.Space(SpacingFromCategory);

                var rot = Transform.rotation;
                float x, y, z, w;
                if (_trackSettings.ShowEulerRotation)
                {
                    var euler = rot.eulerAngles;
                    x = euler.x;
                    y = euler.y;
                    z = euler.z;
                    w = 0f;
                }
                else
                {
                    x = rot.x;
                    y = rot.y;
                    z = rot.z;
                    w = rot.w;
                }

                if (_trackSettings.ShowRotX)
                    GUILayoutUtils.ShadowedLabel($"x: {x}", _valueDisplayLabel);

                if (_trackSettings.ShowRotY)
                    GUILayoutUtils.ShadowedLabel($"y: {y}", _valueDisplayLabel);

                if (_trackSettings.ShowRotZ)
                    GUILayoutUtils.ShadowedLabel($"z: {z}", _valueDisplayLabel);

                if (_trackSettings is { ShowEulerRotation: false, ShowRotW: true })
                    GUILayoutUtils.ShadowedLabel($"w: {w}", _valueDisplayLabel);

                GUILayout.Space(10);
            }

            if (_trackSettings.ShowEstVel)
            {
                GUILayoutUtils.ShadowedLabel("Est velocity", _categoryLabel);
                GUILayout.Space(SpacingFromCategory);

                if (_trackSettings.ShowEstVelX)
                    GUILayoutUtils.ShadowedLabel($"x: {_estVel.x}", _valueDisplayLabel);

                if (_trackSettings.ShowEstVelY)
                    GUILayoutUtils.ShadowedLabel($"y: {_estVel.y}", _valueDisplayLabel);

                if (_trackSettings.ShowEstVelZ)
                    GUILayoutUtils.ShadowedLabel($"z: {_estVel.z}", _valueDisplayLabel);

                if (_trackSettings.ShowEstHSpd)
                    GUILayoutUtils.ShadowedLabel($"h spd: {new Vector3(_estVel.x, 0, _estVel.z).magnitude}",
                        _valueDisplayLabel);
            }
        }

        if (_rigidbody != null && _trackSettings.ShowVel)
        {
            GUILayoutUtils.ShadowedLabel("Velocity", _categoryLabel);
            GUILayout.Space(SpacingFromCategory);

            var vel = _rigidbody.velocity;

            if (_trackSettings.ShowVelX)
                GUILayoutUtils.ShadowedLabel($"x: {vel.x}", _valueDisplayLabel);

            if (_trackSettings.ShowVelY)
                GUILayoutUtils.ShadowedLabel($"y: {vel.y}", _valueDisplayLabel);

            if (_trackSettings.ShowVelZ)
                GUILayoutUtils.ShadowedLabel($"z: {vel.z}", _valueDisplayLabel);

            if (_trackSettings.ShowHSpd)
                GUILayoutUtils.ShadowedLabel($"h spd: {new Vector3(vel.x, 0, vel.z).magnitude}", _valueDisplayLabel);
        }

        GUILayout.FlexibleSpace();
        GUILayout.EndVertical();

        if (_resizeWindow && FitWindowSize())
            _resizeWindow = false;
    }

    private Vector3 _prevPos;
    private Vector3 _estVel;

    private void OnLateUpdateActual()
    {
        // rigid body would only update every fixed update, so unrelated to here
        if (Transform == null || _rigidbody != null) return;
        var pos = Transform.position;
        if (_trackSettings.ShowEstVel)
            _estVel = pos - _prevPos;
        _prevPos = pos;
    }

    private void OnFixedUpdateActual()
    {
        if (Transform == null || _rigidbody == null) return;
        var pos = Transform.position;
        if (_trackSettings.ShowEstVel)
        {
            _estVel = pos - _prevPos;
            _estVel /= Time.fixedDeltaTime;
        }

        _prevPos = pos;
    }

    // update field entry and save to settings if different
    private void SaveTrackSettings<T>(T newValue, ref T settingsField)
    {
        if (newValue.Equals(settingsField)) return;
        settingsField = newValue;
        _config.WriteBackendEntry(_trackSettingsConfigKey, _trackSettings);
    }

    public struct TrackSettings
    {
        public bool ShowName;
        public string Name;

        public bool ShowPos;
        public bool ShowPosX;
        public bool ShowPosY;
        public bool ShowPosZ;

        public bool ShowRot;
        public bool ShowRotX;
        public bool ShowRotY;
        public bool ShowRotZ;
        public bool ShowRotW; // for quaternion
        public bool ShowEulerRotation;

        public bool ShowVel;
        public bool ShowVelX;
        public bool ShowVelY;
        public bool ShowVelZ;
        public bool ShowHSpd;

        public bool ShowEstVel;
        public bool ShowEstVelX;
        public bool ShowEstVelY;
        public bool ShowEstVelZ;
        public bool ShowEstHSpd;

        public UnityObjectIdentifier.SearchSettings ObjectSearch;
    }
}