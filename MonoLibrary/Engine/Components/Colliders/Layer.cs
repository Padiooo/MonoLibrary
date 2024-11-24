using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MonoLibrary.Engine.Components.Colliders;

public class Layer
{
    public static readonly Layer None = new("None", [], []);
    public static readonly Layer Full = new("Full", Enumerable.Range(0, sizeof(int) * 8), Enumerable.Range(0, sizeof(int) * 8));

    public string Name { get; }

    /// <summary>
    /// Layers are the bits that the object belongs to.
    /// </summary>
    public int Layers { get; set; }

    /// <summary>
    /// Masks are the bits that the object collide with.
    /// </summary>
    public int Masks { get; set; }

    public Layer(string name = "Default")
    {
        Name = name;
        Layers = 1;
        Masks = 1;
    }

    public Layer(string name, IEnumerable<int> layers, IEnumerable<int> masks)
    {
        Name = name;
        foreach (var layer in layers)
            Layers = Layers.Set(layer);
        foreach (var mask in masks)
            Masks = Masks.Set(mask);
    }

    public bool IsInterested(Layer other)
    {
        return (Masks & other.Layers) != 0
            || (other.Masks & Layers) != 0;
    }

    public override string ToString()
    {
        return new StringBuilder(200)
            .AppendFormat("'{0}' ", Name)
            .AppendFormat("Colliders: {0}\n", Format(Layers))
            .AppendFormat("Masks:     {0}", Format(Masks))
            .ToString();
    }

    private static string Format(long value)
    {
        var stb = new StringBuilder();
        var str = Convert.ToString(value, 2).PadLeft(64, '0');

        for (int i = 0; i < 8; i++)
        {
            stb.Append(str[(i * 8)..((i + 1) * 8)]);
            stb.Append(' ');
        }

        return stb.ToString();
    }

    public Layer Copy()
    {
        return new Layer()
        {
            Masks = Masks,
            Layers = Layers,
        };
    }

    public static string GetMatrix(IEnumerable<Layer> layers)
    {
        var names = layers.Select(l => l.Name).ToArray();
        var maxLength = names.Max(name => name.Length) + 1;
        var total = maxLength + names.Length;
        var builders = Enumerable.Range(0, total).Select(i => new StringBuilder(new string(' ', total))).ToList();

        for (int i = 0; i < maxLength; i++)
        {
            var builder = builders[i];
            for (int j = 0; j < total - maxLength; j++)
            {
                int index = -(maxLength - (names[j].Length + i));
                if (index >= 0)
                    builder[maxLength + j] = names[j][index];
            }
        }
        for (int i = 0; i < total - maxLength; i++)
        {
            StringBuilder builder = builders[maxLength + i];
            builder.Insert(0, names[i]);
            var layer = layers.Skip(i).FirstOrDefault();
            int j = 0;
            foreach (var other in layers)
            {
                if (layer.IsInterested(other))
                    builder.Insert(maxLength + j, 'O');
                j++;
            }
        }

        var stb = new StringBuilder(builders.Count * total);
        stb.AppendJoin(Environment.NewLine, builders);

        return stb.ToString();
    }
}

public static class BitHelper
{
    public static int Set(this int value, int index) => value | 1 << index;
    public static int UnSet(this int value, int index) => value ^ 1 << index;
}
