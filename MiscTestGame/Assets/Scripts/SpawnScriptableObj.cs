using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/Test", order = 1)]
public class SpawnScriptableObj : ScriptableObject
{
    public int testInt;
    public List<Vector3> testList;
    public TestType testType;
}

[Serializable]
public class TestType
{
    public TestType someInstance;
    public int someInt;
    public float someFloat;
    public string someStr;
    public DateTime someTime;
}
