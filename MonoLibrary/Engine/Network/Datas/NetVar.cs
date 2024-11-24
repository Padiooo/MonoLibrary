using LiteNetLib.Utils;

using MonoLibrary.Engine.Network.Serializers;

using System;

namespace MonoLibrary.Engine.Network.Datas;

public delegate void ValueChanged<T>(T old, T @new);

public class NetVar<T> : INetVariable
    where T : struct, IEquatable<T>
{
    private bool isDirty;

    public int DirtyIndex { get; set; }

    public event Action<int> OnDirty;

    public event ValueChanged<T> OnValueChanged;

    private T _value = default;
    public T Value
    {
        get => _value;
        set
        {
            if (!value.Equals(_value))
            {
                var temp = _value;
                _value = value;
                OnValueChanged?.Invoke(temp, value);

                if (!isDirty)
                    OnDirty?.Invoke(DirtyIndex);
                isDirty = true;
            }
        }
    }

    public NetVar(T value = default)
    {
        Value = value;
    }

    public void Serialize(NetDataWriter writer, bool full)
    {
        writer.Write<T>(_value);
    }

    public void Deserialize(NetDataReader reader)
    {
        var temp = _value;
        _value = reader.Read<T>();

        if (!temp.Equals(_value))
            OnValueChanged?.Invoke(temp, _value);
    }

    public void ClearDirty()
    {
        isDirty = false;
    }

    public void Dispose()
    {

    }
}
