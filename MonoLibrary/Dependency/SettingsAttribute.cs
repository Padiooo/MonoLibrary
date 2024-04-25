using System;

namespace MonoLibrary.Dependency
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public sealed class SettingsAttribute : Attribute
    {
        public string Name { get; }

        public SettingsAttribute(string name = null)
        {
            Name = name;
        }
    }
}
