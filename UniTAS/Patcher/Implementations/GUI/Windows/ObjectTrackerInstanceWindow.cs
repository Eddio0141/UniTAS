using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.Events.UnityEvents;
using UniTAS.Patcher.Interfaces.GUI;
using UniTAS.Patcher.Models;
using UniTAS.Patcher.Models.GUI;
using UniTAS.Patcher.Models.UnitySafeWrappers.SceneManagement;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.GUI;
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

    private readonly IConfig _config;
    private readonly IToolBar _toolBar;

    public ObjectTrackerInstanceWindow(WindowDependencies windowDependencies,
        UnityObjectIdentifier identifier,
        IOnSceneLoadEvent onSceneLoadEvent) : base(windowDependencies,
        new WindowConfig(defaultWindowRect: GUIUtils.WindowRect(200, 200), showByDefault: true),
        $"ObjectTracker-{identifier}")
    {
        _config = windowDependencies.Config;
        _toolBar = windowDependencies.ToolBar;
        _unityObjectIdentifier = identifier;
        onSceneLoadEvent.OnSceneLoadEvent += OnSceneLoad;
        UpdateInstance();
        Init();

        _trackSettingsConfigKey = $"ObjectTracker-Instance-trackSettings-{identifier}";
        if (!_config.TryGetBackendEntry(_trackSettingsConfigKey, out _trackSettings))
        {
            _trackSettings = new TrackSettings
            {
                ShowEulerRotation = true, ShowPos = true, ShowPosX = true, ShowPosY = true, ShowPosZ = true,
                ShowRot = true, ShowRotW = true, ShowRotX = true, ShowRotY = true, ShowRotZ = true,
                Name = _instance?.name ?? "", ShowName = true
            };
        }

        _prevToolbarShow = _toolBar.Show;
    }

    private new void Init()
    {
        NoWindowDuringToolBarHide = true;
        Resizable = false;
    }

    private void OnSceneLoad(string sceneName, int sceneBuildIndex, LoadSceneMode loadSceneMode,
        LocalPhysicsMode localPhysicsMode)
    {
        // instance might have updated
        UpdateInstance();
    }

    private Transform _transform;
    private bool _hasTransform;
    private Rigidbody _rigidbody;
    private bool _hasRigidbody;

    private void UpdateInstance()
    {
        var updateComponents = false;
        if (_instance == null)
        {
            _instance = _unityObjectIdentifier.FindObject();
            updateComponents = true;
        }

        if (_instance == null) return;
        var config = Config;
        config.WindowName = $"Tracking '{_instance.name}'";

        _transform = _instance switch
        {
            Transform t => t.transform,
            GameObject go => go.transform,
            Component comp => comp.transform,
            _ => null
        };
        _hasTransform = _transform != null;

        if (updateComponents || !_hasRigidbody)
        {
            _rigidbody = _transform?.GetComponent<Rigidbody>();
            _hasRigidbody = _rigidbody != null;
        }
    }

    private GUIStyle _labelNoWordWrap;

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
        var newStr = GUILayout.TextField(_trackSettings.Name, 30, GUILayout.ExpandWidth(false));
        if (newStr != _trackSettings.Name)
            _resizeWindow = true;
        SaveTrackSettings(newStr, ref _trackSettings.Name);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        UnityEngine.GUI.enabled = _hasTransform;
        newBool = GUILayout.Toggle(_trackSettings.ShowPos, "Position");
        UnityEngine.GUI.enabled = _trackSettings.ShowPos && _hasTransform;
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

        UnityEngine.GUI.enabled = _hasTransform;
        newBool = GUILayout.Toggle(_trackSettings.ShowRot, "Rotation");
        UnityEngine.GUI.enabled = _trackSettings.ShowRot && _hasTransform;
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
        UnityEngine.GUI.enabled = _hasTransform && _trackSettings is { ShowRot: true, ShowEulerRotation: false };
        newBool = GUILayout.Toggle(_trackSettings.ShowRotW, "w");
        SaveTrackSettings(newBool, ref _trackSettings.ShowRotW);

        if (_trackSettings is { ShowRotX: false, ShowRotY: false, ShowRotZ: false }
            and ({ ShowEulerRotation: true, ShowRotW: false } or { ShowEulerRotation: false }))
        {
            SaveTrackSettings(false, ref _trackSettings.ShowEulerRotation);
        }

        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        UnityEngine.GUI.enabled = _hasRigidbody;
        newBool = GUILayout.Toggle(_trackSettings.ShowVel, "Velocity");
        UnityEngine.GUI.enabled = _trackSettings.ShowVel && _hasRigidbody;
        SaveTrackSettings(newBool, ref _trackSettings.ShowVel);
        if (_trackSettings is { ShowVel: true, ShowVelX: false, ShowVelY: false, ShowVelZ: false })
        {
            SaveTrackSettings(true, ref _trackSettings.ShowVelX);
            SaveTrackSettings(true, ref _trackSettings.ShowVelY);
            SaveTrackSettings(true, ref _trackSettings.ShowVelZ);
        }

        GUILayout.FlexibleSpace();

        GUILayout.BeginHorizontal();
        newBool = GUILayout.Toggle(_trackSettings.ShowVelX, "x");
        SaveTrackSettings(newBool, ref _trackSettings.ShowVelX);
        newBool = GUILayout.Toggle(_trackSettings.ShowVelY, "y");
        SaveTrackSettings(newBool, ref _trackSettings.ShowVelY);
        newBool = GUILayout.Toggle(_trackSettings.ShowVelZ, "z");
        SaveTrackSettings(newBool, ref _trackSettings.ShowVelZ);

        if (_trackSettings is { ShowVelZ: false, ShowVelY: false, ShowVelZ: false })
        {
            SaveTrackSettings(false, ref _trackSettings.ShowVel);
        }

        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.EndVertical();

        FixWindowSize();

        UnityEngine.GUI.enabled = true;
    }

    private void FixWindowSize()
    {
        if (!_resizeWindow || Event.current.type != EventType.Repaint)
            return;
        _resizeWindow = false;

        var rect = GUILayoutUtility.GetLastRect();
        var windowRect = WindowRect;
        windowRect.width = rect.width;
        windowRect.height = rect.height;
        WindowRect = windowRect;
    }

    private const int SpacingFromCategory = 3;
    private bool _resizeWindow = true;
    private bool _prevToolbarShow;

    private GUIStyle _nameLabel;

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

        if (_trackSettings.ShowName)
        {
            _nameLabel ??= new GUIStyle(UnityEngine.GUI.skin.label) { alignment = TextAnchor.MiddleCenter };

            GUILayout.Label(_trackSettings.Name, _nameLabel);
            GUILayout.Space(SpacingFromCategory);
        }

        if (_hasTransform)
        {
            if (_trackSettings.ShowPos)
            {
                GUILayout.Label("Position");
                GUILayout.Space(SpacingFromCategory);

                var pos = _transform.position;

                if (_trackSettings.ShowPosX)
                    GUILayout.Label($"x: {pos.x}");

                if (_trackSettings.ShowPosY)
                    GUILayout.Label($"y: {pos.y}");

                if (_trackSettings.ShowPosZ)
                    GUILayout.Label($"z: {pos.z}");

                GUILayout.Space(10);
            }

            if (_trackSettings.ShowRot)
            {
                GUILayout.Label("Rotation");
                GUILayout.Space(SpacingFromCategory);

                var rot = _transform.rotation;
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
                    GUILayout.Label($"x: {x}");

                if (_trackSettings.ShowRotY)
                    GUILayout.Label($"y: {y}");

                if (_trackSettings.ShowRotZ)
                    GUILayout.Label($"z: {z}");

                if (_trackSettings is { ShowEulerRotation: false, ShowRotW: true })
                    GUILayout.Label($"w: {w}");

                GUILayout.Space(10);
            }
        }

        if (_hasRigidbody && _trackSettings.ShowVel)
        {
            GUILayout.Label("Velocity");
            GUILayout.Space(SpacingFromCategory);

            var vel = _rigidbody.velocity;

            if (_trackSettings.ShowVelX)
                GUILayout.Label($"x: {vel.x}");

            if (_trackSettings.ShowVelY)
                GUILayout.Label($"y: {vel.y}");

            if (_trackSettings.ShowVelZ)
                GUILayout.Label($"z: {vel.z}");
        }

        GUILayout.EndVertical();

        FixWindowSize();
    }

    // update field entry and save to settings if different
    private void SaveTrackSettings<T>(T newValue, ref T settingsField)
    {
        if (newValue.Equals(settingsField)) return;
        settingsField = newValue;
        _config.WriteBackendEntry(_trackSettingsConfigKey, _trackSettings);
    }

    private struct TrackSettings
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
    }
}