using System;
using System.Collections;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(TextMeshProUGUI), typeof(RectTransform))]
public class InputDisplay : MonoBehaviour
{
    private TextMeshProUGUI _text;
    private RectTransform _textRectTransform;

    private RectTransform _parentRectTransform;

    public GameObject clone;

    private void Awake()
    {
        Debug.Log("Awake");

        _text = GetComponent<TextMeshProUGUI>();
        if (_text == null)
        {
            Debug.LogError("InputDisplay: TextMeshPro component not found");
            return;
        }

        _parentRectTransform = transform.parent.GetComponent<RectTransform>();
        if (_parentRectTransform == null)
        {
            Debug.LogError("InputDisplay: RectTransform component not found");
            return;
        }

        _textRectTransform = GetComponent<RectTransform>();
        if (_textRectTransform == null)
        {
            Debug.LogError("InputDisplay: RectTransform component not found");
            return;
        }

        var rect = _parentRectTransform.rect;
        _textRectTransform.sizeDelta = new(rect.width, rect.height);

        //StartCoroutine(CheckRes());

        SceneManager.activeSceneChanged += (before, after) =>
            Debug.Log($"activeSceneChanged, before: {before.name}, after: {after.name}, frame: {Time.frameCount}");
        SceneManager.sceneLoaded += (scene, mode) =>
            Debug.Log($"sceneLoaded, {scene.name}, {mode}, frame: {Time.frameCount}");
        SceneManager.sceneUnloaded += scene => Debug.Log($"sceneUnloaded, {scene.name}, frame: {Time.frameCount}");

        StartCoroutine(EndOfFrame());
    }

    private void OnEnable()
    {
        Debug.Log("OnEnable");
    }

    private void Start()
    {
        Debug.Log("Start");
    }

    private IEnumerator CheckRes()
    {
        for (int i = 0; i < 3; i++)
        {
            Debug.Log($"resolution: {Screen.width}x{Screen.height}");
            foreach (var res in Screen.resolutions)
            {
                Debug.Log($"found resolution: {res}");
            }

            yield return new WaitForSeconds(0.5f);
        }
    }

    private bool _smallRes;
    private AsyncOperation emptyOp;
    private AsyncOperation unloadOp;
    private Scene? _fooScene;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            _smallRes = !_smallRes;

            if (_smallRes)
            {
                Screen.SetResolution(1700, 800, false);
            }
            else
            {
                Screen.SetResolution(1920, 1080, false);
            }
        }

        if (Input.GetKeyDown(KeyCode.K))
        {
            emptyOp = SceneManager.LoadSceneAsync("Empty", LoadSceneMode.Additive)!;
            Debug.Log(
                $"sceneCount: {SceneManager.sceneCount}, loadedSceneCount: {SceneManager.loadedSceneCount}, {emptyOp.progress}, {Time.frameCount}");
            // emptyOp.allowSceneActivation = false;
            // SceneManager.LoadScene("Empty", LoadSceneMode.Additive);
            // var op = SceneManager.LoadSceneAsync("Empty", LoadSceneMode.Additive);
            // op.allowSceneActivation = false;
            // op.completed += _ => { Debug.Log("a"); };
            Debug.Log($"progress: {emptyOp.progress}");
            emptyOp.completed += _ =>
            {
                Debug.Log(
                    $"loaded Empty async at frame {Time.frameCount}, {Time.captureFramerate}, {Time.renderedFrameCount}, scene count: {SceneManager.sceneCount}, {SceneManager.loadedSceneCount}");
            };
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            Debug.Log("Empty load non async");
            SceneManager.LoadScene("Empty", LoadSceneMode.Additive);
            Debug.Log($"allow scene after scene count: {SceneManager.sceneCount}, {SceneManager.loadedSceneCount}");
        }

        if (Input.GetKeyDown(KeyCode.N))
        {
            Debug.Log(
                $"load sync scene at frame: {Time.frameCount}, {Time.captureFramerate}, {Time.renderedFrameCount}, scene count: {SceneManager.sceneCount}, {SceneManager.loadedSceneCount}");
            SceneManager.LoadScene("Empty");
        }

        if (Input.GetKeyDown(KeyCode.M))
        {
            // Debug.Log($"Empty status {_emptyOp.allowSceneActivation}, {_emptyOp.isDone}, {_emptyOp.progress}");
            Debug.Log($"sceneCount: {SceneManager.sceneCount}, loadedSceneCount: {SceneManager.loadedSceneCount}");
        }

        if (Input.GetKeyDown(KeyCode.B))
        {
            unloadOp = SceneManager.UnloadSceneAsync("Empty");
            if (unloadOp != null)
                unloadOp.completed += op => { Debug.Log($"Empty unload async, {op.progress}, {op.isDone}"); };
        }

        if (Input.GetKeyDown(KeyCode.V))
        {
            Debug.Log(
                $"allow scene at frame: {Time.frameCount}, {Time.captureFramerate}, {Time.renderedFrameCount}, scene count: {SceneManager.sceneCount}, {SceneManager.loadedSceneCount}");
            emptyOp.allowSceneActivation = true;
        }

        if (Input.GetKeyDown(KeyCode.U))
        {
            Debug.Log("spawn");
            var pos = new ReadOnlySpan<Vector3>(new Vector3[] { new(1, 2, 3), new(4, 5, 6) });
            // // var rot = new ReadOnlySpan<Quaternion>(new[] { Quaternion.Euler(7, 8, 9), Quaternion.Euler(10, 11, 12) });
            var rot = new ReadOnlySpan<Quaternion>(new[]
                { Quaternion.Euler(7, 8, 9), Quaternion.Euler(10, 11, 12), Quaternion.Euler(13, 14, 15) });
            foreach (var r in rot)
            {
                Debug.Log($"thingy: {r.x}, {r.y}, {r.z}");
            }

            InstantiateAsync(clone, 4, pos, rot);
            // Thingy(new ReadOnlySpan<Foo>(new Foo[] { new() { x = 1.2f } }));
        }

        if (Input.GetKeyDown(KeyCode.H))
        {
            Debug.Log("create scene foo");
            if (_fooScene.HasValue)
            {
                SceneManager.LoadScene(0);
            }
            else
            {
                _fooScene = SceneManager.CreateScene("foo");
            }
        }

        if (_text == null) return;

        var builder = new StringBuilder();

        var mouse = Mouse.current;
        if (mouse == null)
        {
            builder.AppendLine("Mouse not found");
        }
        else
        {
            builder.AppendLine($"Mouse: {mouse.position.ReadValue()}" +
                               $", scroll: {mouse.scroll.ReadValue()}" +
                               (mouse.leftButton.isPressed ? ", left click" : "") +
                               (mouse.middleButton.isPressed ? ", middle click" : "") +
                               (mouse.rightButton.isPressed ? ", right click" : ""));
        }

        var keyboard = Keyboard.current;
        builder.AppendLine(keyboard == null
            ? "Keyboard not found"
            : $"Keyboard: {string.Join(", ", keyboard.allKeys.Where(k => k.isPressed).Select(k => k.keyCode.ToString()))}");

        _text.text = builder.ToString();
    }

    private IEnumerator EndOfFrame()
    {
        while (true)
        {
            yield return new WaitForEndOfFrame();
            _ = 1;
        }
    }

    private struct Foo
    {
        public float x;
    }

    private struct Bar
    {
        public float y;
    }

    private static void Thingy(ReadOnlySpan<Foo> a)
    {
        unsafe
        {
            fixed (void* aPtr = a)
            {
                Call(new ReadOnlySpan<Bar>(aPtr, a.Length));
            }
        }
    }

    private static void Call(ReadOnlySpan<Bar> b)
    {
        Debug.Log($"b is {b.Length}, {b[0].y}");
    }
}