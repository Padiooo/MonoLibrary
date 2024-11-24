using System;

namespace MonoLibrary.Dependency;

[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
public sealed class SettingsAttribute(string name = null) : Attribute
{
    public string Name { get; } = name;
}
