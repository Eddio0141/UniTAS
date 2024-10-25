using System;
using System.Collections.Generic;
using System.Linq;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.GUI;
using UniTAS.Patcher.Models.Customization;
using UniTAS.Patcher.Models.DependencyInjection;
using UniTAS.Patcher.Models.GUI;
using UniTAS.Patcher.Services.Customization;
using UniTAS.Patcher.Utils;
using UnityEngine;

namespace UniTAS.Patcher.Implementations.GUI.Windows;

[Singleton(RegisterPriority.KeyBindsWindow)]
[ForceInstantiate]
[ExcludeRegisterIfTesting]
public class KeyBindsWindow : Window
{
    // category name and binds
    private readonly (string, Bind[])[] _binds;

    // if bind is valid, indexed by category and bind index
    private readonly bool[][] _bindValid;

    private readonly string[][] _bindRaw;

    public KeyBindsWindow(WindowDependencies windowDependencies, IBinds binds) : base(windowDependencies,
        new WindowConfig(defaultWindowRect: GUIUtils.WindowRect(500, 300), windowName: "Key binds"), "Key binds")
    {
        var processingBinds = new List<Bind>(binds.AllBinds.Where(x => !x.NoGenConfig));

        _binds = Enum.GetValues(typeof(BindCategory)).Cast<BindCategory>()
            .Select(x =>
            {
                var matching = processingBinds.Where(y => y.Category == x).ToArray();
                foreach (var match in matching)
                {
                    processingBinds.Remove(match);
                }

                return (x.ToString(), matching);
            }).ToArray();

        _bindValid = new bool[_binds.Length][];
        _bindRaw = new string[_binds.Length][];

        for (var i = 0; i < _binds.Length; i++)
        {
            var categoryBinds = _binds[i].Item2;
            _bindValid[i] = new bool[categoryBinds.Length];
            _bindRaw[i] = new string[categoryBinds.Length];
            for (var j = 0; j < categoryBinds.Length; j++)
            {
                _bindValid[i][j] = true;
                _bindRaw[i][j] = categoryBinds[j].Key.ToString();
            }
        }
    }

    private const int BindRows = 3;
    private GUIStyle _redTextField;
    private GUIStyle _categoryText;

    protected override void OnGUI()
    {
        _redTextField ??= new GUIStyle(UnityEngine.GUI.skin.textField)
        {
            normal = { textColor = Color.red },
            hover = { textColor = Color.red },
            active = { textColor = Color.red },
            onNormal = { textColor = Color.red },
            onHover = { textColor = Color.red },
            onActive = { textColor = Color.red },
        };

        GUILayout.BeginVertical();

        for (var iCategories = 0; iCategories < _binds.Length; iCategories++)
        {
            var (category, binds) = _binds[iCategories];

            _categoryText ??= new GUIStyle(UnityEngine.GUI.skin.label)
            {
                fontSize = 23
            };

            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            GUILayout.Label(category, _categoryText);
            GUILayout.EndHorizontal();

            for (var iBinds = 0; iBinds < binds.Length; iBinds++)
            {
                var bind = binds[iBinds];
                var shift = iBinds % BindRows == 0;
                if (shift)
                {
                    if (iBinds > 0)
                        GUILayout.EndHorizontal();
                    if (iBinds < binds.Length - 1 || binds.Length == 1)
                    {
                        GUILayout.BeginHorizontal();
                    }
                }
                else
                {
                    GUILayout.FlexibleSpace();
                }

                GUILayout.Label(bind.Name);
                GUILayout.Space(5);

                var valid = _bindValid[iCategories][iBinds];
                var raw = _bindRaw[iCategories][iBinds];

                var newRaw = GUILayout.TextField(raw, valid ? UnityEngine.GUI.skin.textField : _redTextField,
                    GUILayout.ExpandWidth(false), GUILayout.MinWidth(20)).Trim();
                if (UnityEngine.GUI.changed && raw != newRaw)
                {
                    _bindRaw[iCategories][iBinds] = newRaw;
                    var parsed = InputSystemUtils.KeyCodeParse(newRaw);
                    if (parsed.HasValue)
                    {
                        // find conflict
                        if (_binds.Any(x => x.Item2.Any(b => b.Key == parsed.Value)))
                        {
                            _bindValid[iCategories][iBinds] = false;
                        }
                        else
                        {
                            bind.Key = parsed.Value;
                            _bindValid[iCategories][iBinds] = true;
                        }
                    }
                    else
                    {
                        _bindValid[iCategories][iBinds] = false;
                    }
                }
            }

            GUILayout.FlexibleSpace();
            if (binds.Length % BindRows == 0 || binds.Length == 1)
                GUILayout.EndHorizontal();

            GUILayout.Space(15);
        }

        GUILayout.EndVertical();
    }
}