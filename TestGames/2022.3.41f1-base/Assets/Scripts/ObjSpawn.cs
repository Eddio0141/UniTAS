using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Scriptable", menuName = "Assets/Spawn Scriptable Object", order = 1)]
public class ObjSpawn : ScriptableObject
{
    public string str;
    public int value;
    public Obj obj;
    public List<Obj> values;
}

[Serializable]
public class Obj
{
    public int value;
}
