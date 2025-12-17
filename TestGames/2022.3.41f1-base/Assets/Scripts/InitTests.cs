using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class InitTests : MonoBehaviour, ISerializationCallbackReceiver
{
    private int _deserializeCallCount;

    public void OnBeforeSerialize()
    {
    }

    public void OnAfterDeserialize()
    {
        _deserializeCallCount++;
        // I really wanna test this but it causes building to fail lmao
        // throw new Exception("foo");
    }

    static InitTests()
    {
        Assert.False("runtime init method: SubsystemRegistration", _subsystemRegistrationCalled);
        Assert.False("runtime init method: AfterAssembliesLoaded", _afterAssembliesLoaded);
        Assert.False("runtime init method: BeforeSplashScreen", _beforeSplashScreenCalled);
        Assert.False("runtime init method: BeforeSceneLoad", _beforeSceneLoadCalled);
        Assert.False("runtime init method, AfterSceneLoad", _afterSceneLoadCalled);
    }

    [SerializeField] private ObjSpawn obj;

    private void Awake()
    {
#if UNITY_EDITOR
        Time.captureDeltaTime = 0.01f;
#endif

        Assert.False("InputSystem.onBeforeUpdate", _updatedBeforeUpdate);
        InputSystem.onBeforeUpdate += () => { _updatedBeforeUpdate = true; };

        Assert.True("runtime init method: SubsystemRegistration", _subsystemRegistrationCalled);
        Assert.True("runtime init method: AfterAssembliesLoaded", _afterAssembliesLoaded);
        Assert.True("runtime init method: BeforeSplashScreen", _beforeSplashScreenCalled);
        Assert.True("runtime init method: BeforeSceneLoad", _beforeSceneLoadCalled);
        Assert.False("runtime init method, AfterSceneLoad", _afterSceneLoadCalled);
        Assert.Equal("init.frame_count", 0, Time.frameCount);
        Assert.Equal("init.rendered_frame_count", 0, Time.renderedFrameCount);
        Assert.Equal("init.time", 0f, Time.time);
        Assert.Equal("init.time", 0f, Time.timeSinceLevelLoad);
        Assert.Equal("init.time", 0f, Time.fixedTime);
        Assert.Equal("init.time", 0f, Time.unscaledTime);
        Assert.Equal("init.time", 0f, Time.realtimeSinceStartup);

        Assert.Equal("OnAfterDeserialize", 1, _deserializeCallCount);

        Assert.True("ScriptableObject.exists", obj != null);
        Assert.Equal("ScriptableObject.value", "foo", obj.str);
        Assert.Equal("ScriptableObject.value", 123, obj.value);
        Assert.Equal("ScriptableObject.value", 4, obj.obj.value);
        Assert.Equal("ScriptableObject.value", 4, obj.values.Count);
        Assert.Equal("ScriptableObject.value", 5, obj.values[0].value);
        Assert.Equal("ScriptableObject.value", 6, obj.values[1].value);
        Assert.Equal("ScriptableObject.value", 7, obj.values[2].value);
        Assert.Equal("ScriptableObject.value", 8, obj.values[3].value);

        obj.str = "bar";
        obj.value += 5;
        obj.obj.value += 5;
        obj.values.RemoveRange(0, 2);
        obj.values[0].value += 5;
        obj.values[1].value += 5;

        StartCoroutine(AwakeCoroutine());
    }

    private IEnumerator AwakeCoroutine()
    {
        Assert.Equal("init.frame_count", 0, Time.frameCount);
        Assert.Equal("init.rendered_frame_count", 0, Time.renderedFrameCount);
        yield return null;
        Assert.Equal("init.frame_count", 1, Time.frameCount);
        Assert.Equal("init.rendered_frame_count", 1, Time.renderedFrameCount);

#if !UNITY_EDITOR
        DestroyImmediate(obj, true);
        Assert.True("ScriptableObject.exists", obj == null);
#endif
    }

    private IEnumerator Start()
    {
        Assert.Equal("init.time", 0.01f, Time.time, 0.0001f);
        Assert.Equal("init.time", 0.01f, Time.timeSinceLevelLoad, 0.0001f);
        Assert.Equal("init.time", 0f, Time.fixedTime, 0.0001f);
        Assert.Equal("init.time", 0.01f, Time.unscaledTime, 0.0001f);
        Assert.Equal("init.time", 0.01f, Time.realtimeSinceStartup, 0.0001f);

        Assert.True("runtime init method, AfterSceneLoad", _afterSceneLoadCalled);
        Assert.Equal("init.frame_count", 1, Time.frameCount);
        Assert.Equal("init.rendered_frame_count", 1, Time.renderedFrameCount);

        yield return new WaitForEndOfFrame();
        Assert.Equal("init.frame_count", 1, Time.frameCount);
        Assert.Equal("init.rendered_frame_count", 1, Time.renderedFrameCount);
        // Assert.True("after OnGUI call", _calledOnGUI);
        yield return null;
        Assert.True("InputSystem.onBeforeUpdate", _updatedBeforeUpdate);
        Assert.Equal("init.frame_count", 2, Time.frameCount);
        Assert.Equal("init.rendered_frame_count", 2, Time.renderedFrameCount);
        yield return null;

        Assert.Null("scene.unload.current_only_scene", SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene()));
    }

#pragma warning disable CS0414 // Field is assigned but its value is never used
    private static bool _updated;
    private static bool _updatedBeforeUpdate;
    private static bool _fixedUpdate;
#pragma warning restore CS0414 // Field is assigned but its value is never used

    private void Update()
    {
        _updated = true;
    }

    private void FixedUpdate()
    {
        _fixedUpdate = true;
    }

    private static bool _beforeSplashScreenCalled;
    private static bool _subsystemRegistrationCalled;
    private static bool _beforeSceneLoadCalled;
    private static bool _afterAssembliesLoaded;
    private static bool _afterSceneLoadCalled;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
    private static void BeforeSplashScreen()
    {
        _beforeSplashScreenCalled = true;
        Debug.Log("BeforeSplashScreen");
        throw new Exception("foo");
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void SubsystemRegistration()
    {
        _subsystemRegistrationCalled = true;
        Debug.Log("SubsystemRegistration");
        throw new Exception("foo");
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void BeforeSceneLoad()
    {
        _beforeSceneLoadCalled = true;
        Debug.Log("BeforeSceneLoad");
        throw new Exception("foo");
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
    private static void AfterAssembliesLoaded()
    {
        _afterAssembliesLoaded = true;
        Debug.Log("AfterAssembliesLoaded");
        throw new Exception("foo");
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AfterSceneLoad()
    {
        _afterSceneLoadCalled = true;
        Debug.Log("AfterSceneLoad");
        throw new Exception("foo");
    }
}