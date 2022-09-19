using System;
using System.Collections.Generic;
using System.Linq;
using UniTASPlugin.VersionSafeWrapper;
using UnityEngine;

namespace UniTASPlugin.TAS.Input.Movie;

public class Movie
{
    public readonly string Name;
    public readonly List<Framebulk> Framebulks;
    public readonly DateTime Time;

    /* V1 FORMAT

    version 1
    seed seedvalue
    frames
    mouse x|mouse y|left right middle|UpArrow W A S D|"axis X" 1 "sprint" -0.1|frametime|framecount
    |||W A S D||0.001|500
    |||||frametime|framecount
    // comment
    
    */
    public Movie(string filename, string text, out string error, out List<string> warnings)
    {
        error = "";
        warnings = new();

        Name = filename;
        Framebulks = new();

        string[] lines = text.Split('\n');

        var inVersion = true;
        var inProperties = true;
        var foundSeed = false;

        var comment = "//";
        var versionText = "version 1";
        var framesSection = "frames";
        var seedText = "seed ";
        var fieldSeparator = '|';
        var listSeparator = ' ';
        var axisNameSurround = '"';
        const string leftClick = "left";
        const string rightClick = "right";
        const string middleClick = "middle";

        foreach (var line in lines)
        {
            var lineTrim = line.Trim();

            if (lineTrim.StartsWith(comment))
                continue;

            if (lineTrim == "")
                continue;

            if (inVersion)
            {
                if (lineTrim != versionText)
                {
                    error = "First line not defining version";
                    break;
                }
                inVersion = false;
                continue;
            }

            if (inProperties)
            {
                if (lineTrim.StartsWith(seedText))
                {
                    if (foundSeed)
                    {
                        error = "Seed property defined twice";
                        break;
                    }

                    // TODO way to parse DateTime
                    if (!long.TryParse(lineTrim.Substring(seedText.Length), out var seed))
                    {
                        error = "Seed value not a value";
                        break;
                    }

                    Time = new DateTime(seed);
                    foundSeed = true;

                    continue;
                }

                if (lineTrim == framesSection)
                {
                    inProperties = false;
                    continue;
                }
            }

            var fields = lineTrim.Split(fieldSeparator);

            var framebulk = new Framebulk();
            var mouseXField = true;
            var mouseYField = true;
            var mouseClickField = true;
            var keysField = true;
            var axisField = true;
            var frametimeField = true;

            foreach (var field in fields)
            {
                if (mouseXField)
                {
                    if (field == "")
                    {
                        mouseXField = false;
                        continue;
                    }

                    if (!float.TryParse(field, out var x))
                    {
                        error = "Mouse X value not a valid decimal";
                        break;
                    }

                    framebulk.Mouse.X = x;
                    mouseXField = false;
                    continue;
                }

                if (mouseYField)
                {
                    if (field == "")
                    {
                        mouseYField = false;
                        continue;
                    }

                    if (!float.TryParse(field, out var y))
                    {
                        error = "Mouse Y value not a valid decimal";
                        break;
                    }

                    framebulk.Mouse.Y = y;
                    mouseYField = false;
                    continue;
                }

                if (mouseClickField)
                {
                    if (field == "")
                    {
                        mouseClickField = false;
                        continue;
                    }

                    var clickedButtons = field.Split(listSeparator);

                    foreach (var clickField in clickedButtons)
                    {
                        if (clickField == "")
                            continue;

                        switch (clickField)
                        {
                            case leftClick:
                                if (framebulk.Mouse.Left)
                                {
                                    error = "Mouse left click defined twice";
                                    break;
                                }
                                framebulk.Mouse.Left = true;
                                break;
                            case rightClick:
                                if (framebulk.Mouse.Right)
                                {
                                    error = "Mouse right click defined twice";
                                    break;
                                }
                                framebulk.Mouse.Right = true;
                                break;
                            case middleClick:
                                if (framebulk.Mouse.Middle)
                                {
                                    error = "Mouse middle click defined twice";
                                    break;
                                }
                                framebulk.Mouse.Middle = true;
                                break;
                            default:
                                error = "Mouse click value not valid";
                                break;
                        }

                        if (error != "")
                            break;
                    }

                    if (error != "")
                        break;

                    mouseClickField = false;
                    continue;
                }

                if (keysField)
                {
                    if (field == "")
                    {
                        framebulk.Keys = new();
                        keysField = false;
                        continue;
                    }

                    var keys = field.Split(listSeparator);

                    foreach (var key in keys)
                    {
                        if (key == "")
                            continue;

                        if (!Enum.IsDefined(typeof(KeyCode), key))
                        {
                            error = "Key value not a valid key";
                            break;
                        }

                        var k = Enum.Parse(typeof(KeyCode), key);
                        framebulk.Keys.Pressed.Add((KeyCode)k);
                    }

                    if (error != "")
                        break;

                    keysField = false;
                    continue;
                }

                if (axisField)
                {
                    if (field == "")
                    {
                        framebulk.Axises = new();
                        axisField = false;
                        continue;
                    }

                    var fieldChars = field.ToCharArray();

                    var gettingAxisName = true;
                    var firstSurroundChar = true;
                    var betweenNameAndValue = true;
                    var betweenValueAndName = false;
                    var builder = "";
                    var axisName = "";

                    for (int i = 0; i < fieldChars.Length; i++)
                    {
                        var ch = fieldChars[i];

                        if (betweenValueAndName)
                        {
                            if (ch == listSeparator)
                                continue;

                            betweenValueAndName = false;
                        }

                        if (gettingAxisName)
                        {
                            if (ch == axisNameSurround)
                            {
                                if (firstSurroundChar)
                                {
                                    firstSurroundChar = false;
                                    continue;
                                }

                                axisName = builder;
                                builder = "";
                                gettingAxisName = false;
                                continue;
                            }

                            builder += ch;
                            continue;
                        }

                        if (betweenNameAndValue)
                        {
                            if (ch == listSeparator)
                                continue;

                            betweenNameAndValue = false;
                        }

                        var finalIteration = i == fieldChars.Length - 1;

                        if (ch == listSeparator || finalIteration)
                        {
                            if (finalIteration)
                                builder += ch;

                            if (!float.TryParse(builder, out var axisValue))
                            {
                                error = "Axis value not a valid decimal";
                                break;
                            }

                            if (axisValue > 1 || axisValue < -1)
                            {
                                error = "Axis value needs to be between -1 and 1";
                                break;
                            }

                            framebulk.Axises.AxisMove.Add(axisName, axisValue);

                            if (!finalIteration)
                            {
                                gettingAxisName = true;
                                firstSurroundChar = true;
                                betweenNameAndValue = true;
                                betweenValueAndName = true;
                                builder = "";
                                axisName = "";
                            }

                            continue;
                        }

                        builder += ch;
                    }

                    if (error != "")
                        break;

                    if (gettingAxisName)
                    {
                        error = "Axis missing value";
                        break;
                    }

                    axisField = false;
                    continue;
                }

                if (frametimeField)
                {
                    if (field == "")
                    {
                        error = "Frametime is missing";
                        break;
                    }

                    if (!float.TryParse(field, out var frametime))
                    {
                        error = "Frametime not a decimal";
                        break;
                    }

                    if (frametime < 0)
                    {
                        error = "Frametime is not positive";
                        break;
                    }
                    if (frametime == 0)
                    {
                        error = "Frametime needs to be greater than 0";
                        break;
                    }

                    if (!TimeWrap.HasCaptureDeltaTime())
                    {
                        var framerate = 1 / frametime;

                        if (Helper.ValueHasDecimalPoints(framerate))
                        {
                            warnings.Add("Frametime has decimal points, this version of unity can't do framerate with decimal points, frametime will be rounded for rounded framerate");
                            frametime = 1 / (ulong)framerate;
                        }
                    }

                    framebulk.Frametime = frametime;
                    frametimeField = false;
                    continue;
                }

                if (!int.TryParse(field, out var frameCount))
                {
                    error = "Framecount not an integer";
                    break;
                }

                if (frameCount < 1)
                {
                    error = "Framecount needs to be greater than 0";
                    break;
                }

                framebulk.FrameCount = frameCount;
            }

            if (error != "")
                break;

            Framebulks.Add(framebulk);
        }
    }

    public Movie(string name, List<Framebulk> framebulks) : this(name, framebulks, new DateTime(0)) { }

    public Movie(string name, List<Framebulk> framebulks, DateTime time)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Framebulks = framebulks ?? throw new ArgumentNullException(nameof(framebulks));
        Time = time;
    }

    public float TotalSeconds()
    {
        return Framebulks.Sum(f => f.FrameCount * f.Frametime);
    }

    public float TotalFrames()
    {
        return Framebulks.Sum(f => f.FrameCount);
    }

    public override string ToString()
    {
        return $"Name: {Name}, {Framebulks.Count} framebulks, {TotalFrames()} total frames, {TotalSeconds()} seconds of runtime";
    }
}
