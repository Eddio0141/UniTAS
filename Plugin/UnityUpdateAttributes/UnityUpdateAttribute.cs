using System;

namespace UniTASPlugin.UnityUpdateAttributes;

[AttributeUsage(AttributeTargets.Method)]
public abstract class UnityUpdateAttribute : Attribute
{


    public UnityUpdateAttribute(int priority) { }
}