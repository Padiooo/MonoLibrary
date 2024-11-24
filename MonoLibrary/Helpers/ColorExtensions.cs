using Microsoft.Xna.Framework;

using System;
using System.Linq;
using System.Reflection;

namespace MonoLibrary.Helpers;

public static class ColorExtensions
{
    /// <summary>
    /// Get a <see cref="Color"/> by its name.
    /// </summary>
    /// <param name="name">One of the named <see cref="Color"/>.</param>
    /// <returns></returns>
    public static Color ToColor(this string name)
    {
        var property = typeof(Color)
            .GetProperties(BindingFlags.Static | BindingFlags.Public)
            .FirstOrDefault(pi => pi.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

        if (property is null)
            return Color.Transparent;
        else
            return (Color)property.GetValue(null);
    }

    public static Color FromHex(this string hex)
    {
        if (hex[0] != '#' || !(hex.Length != 7 || hex.Length != 9))
            throw new ArgumentException("Hex string must start with # and contain 7 characters. Ex: #FF0000, #AABBCCFF", nameof(hex));

        byte[] bytes = Convert.FromHexString(hex[1..]);
        if (bytes.Length == 3)
            return new Color(bytes[0], bytes[1], bytes[2], byte.MaxValue);
        else
            return new Color(bytes[0], bytes[1], bytes[2], bytes[3]);
    }

    public static string ToName(this Color color)
    {
        var name = typeof(Color)
            .GetProperties(BindingFlags.Static | BindingFlags.Public)
            .Where(pi => ((Color)pi.GetValue(null)) == color)
            .Select(pi => pi.Name)
            .FirstOrDefault();

        return name ?? throw new ArgumentException("Given color does not correspond to a named color.", nameof(color));
    }

    public static string ToHex(this Color color)
    {
        return $"#{Convert.ToHexString(new byte[] { color.R, color.G, color.B, color.A })}";
    }
}
